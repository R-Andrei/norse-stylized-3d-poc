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
                InspectorSection.FoamTransportVisibilityContract,
                "Transport & Visibility Contract",
                DrawFoamTransportVisibilityContract);
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
                "Layer D — Evaluated Shape (Diagnostic Only)",
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

            SerializedProperty gridMode = Find("foamGridMode");
            EditorGUILayout.PropertyField(
                gridMode,
                new GUIContent(
                    "Grid Mode",
                    "Fixed Metric is the accepted physical-grid baseline. Legacy Normalized Across remains available for direct A/B comparison and rollback."));

            bool fixedMetricSelected = gridMode.hasMultipleDifferentValues ||
                gridMode.enumValueIndex ==
                    (int)StylizedRiverFoamGridMode.FixedMetric;
            using (new EditorGUI.DisabledScope(!fixedMetricSelected))
            {
                EditorGUILayout.PropertyField(
                    Find("foamFixedMetricCellSize"),
                    new GUIContent(
                        "Fixed Cell Size",
                    "Quality Default resolves Low to 0.25 m, Medium to the selected 0.15 m baseline, and High to 0.10 m. The explicit 0.20 m option remains available as a historical intermediate comparison."));
            }

            EditorGUILayout.HelpBox(
                "Changing Grid Mode or the resolved Fixed Cell Size deliberately " +
                "invalidates the active Foam resources and topology-cache " +
                "descriptor. " +
                "Rebuild the assigned Foam topology cache in Edit Mode before " +
                "testing the new selection. Play Mode changes may interrupt " +
                "Foam until that rebuild is completed.",
                MessageType.None);

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

        private void DrawFoamTransportVisibilityContract()
        {
            SerializedProperty transport = Find("foamTransportScheme");
            SerializedProperty visibility = Find("foamFinalVisibilityMode");
            SerializedProperty footprint = Find("foamPresenceFootprintMode");

            EditorGUILayout.PropertyField(
                transport,
                new GUIContent(
                    "Material Transport Scheme",
                    "Controls numerical transport of geometric Coverage. It must not silently change decoded intrinsic Presence or Remaining Life."));
            EditorGUILayout.PropertyField(
                visibility,
                new GUIContent(
                    "Final Foam Visibility Mode",
                    "Controls how transported Coverage and Remaining Life form the Final Foam shape. It does not change persistent Layer C material."));
            EditorGUILayout.PropertyField(
                footprint,
                new GUIContent(
                    "Presence Footprint",
                    "Controls whether decoded intrinsic Presence scales the resolved Final Foam shape. Coverage-Only ignores Presence as amplitude; Presence-Amplitude carries exact Presence through the same shape coupling without changing the coupling weights."));

            EditorGUILayout.Space(5f);
            EditorGUILayout.LabelField(
                "Resolved Foam Contract",
                EditorStyles.miniBoldLabel);
            EditorGUILayout.HelpBox(
                ResolveFoamTransportContractText(transport),
                MessageType.None);
            EditorGUILayout.HelpBox(
                ResolveFoamVisibilityContractText(visibility),
                MessageType.None);
            EditorGUILayout.HelpBox(
                ResolveFoamPresenceContractText(footprint),
                MessageType.None);
            EditorGUILayout.HelpBox(
                ResolveFoamCombinedContractText(
                    transport,
                    visibility,
                    footprint),
                MessageType.Info);

            EditorGUILayout.Space(3f);
            EditorGUILayout.LabelField(
                "Persistent State Meaning",
                EditorStyles.miniBoldLabel);
            EditorGUILayout.HelpBox(
                "Coverage — geometric cell occupancy transported by Donor Cell, " +
                "TVD Superbee, or Bulk-Phase Residual TVD. " +
                "Source shape, subcell width, progressive reveal, " +
                "and valid-fluid clipping may change Coverage.\n\n" +
                "Presence — intrinsic authored material strength. New material " +
                "writes Initial Presence exactly; transport must not reinterpret " +
                "it as Coverage or source probability.\n\n" +
                "Remaining Life — intrinsic normalized lifecycle state. New " +
                "material writes Initial Life exactly; only explicit Layer C " +
                "aging changes it. Negative topology remains allowed to consume " +
                "it rapidly.\n\n" +
                "Material Pattern — stable material identity transported with " +
                "the same coherent state and used by deterioration/rendering.",
                MessageType.None);
        }

        private static string ResolveFoamTransportContractText(
            SerializedProperty transport)
        {
            if (transport.hasMultipleDifferentValues)
            {
                return "Transport — mixed selection. Selected Rivers do not share " +
                    "one Material Transport Scheme.";
            }

            StylizedRiverFoamTransportScheme scheme =
                (StylizedRiverFoamTransportScheme)transport.enumValueIndex;
            return scheme switch
            {
                StylizedRiverFoamTransportScheme.TvdSuperbee =>
                    "Transport — TVD Superbee reconstructs bounded geometric " +
                    "Coverage at interior faces to reduce numerical diffusion " +
                    "and retain sharper Foam footprints. One coherent donor " +
                    "material state is transported, so decoded Presence and " +
                    "Remaining Life are not independently limited or reduced.",
                StylizedRiverFoamTransportScheme.BulkPhaseResidualTvd =>
                    "Transport — Bulk-Phase Residual TVD advances the shared " +
                    "downstream speed as one global subcell phase. The existing " +
                    "single-pass TVD solver handles only local slowdown residuals, " +
                    "lateral motion, and obstacle routing. It allocates no extra " +
                    "full-field texture and adds no material dispatch.",
                _ =>
                    "Transport — Donor Cell transports the upstream coherent " +
                    "material state conservatively. Coverage becomes broader " +
                    "and more numerically diffuse, but decoded Presence and " +
                    "Remaining Life are not reduced merely because material moved."
            };
        }

        private static string ResolveFoamVisibilityContractText(
            SerializedProperty visibility)
        {
            if (visibility.hasMultipleDifferentValues)
            {
                return "Final Visibility — mixed selection. Selected Rivers do " +
                    "not share one Final Foam Visibility Mode.";
            }

            return visibility.enumValueIndex ==
                    (int)StylizedRiverFinalFoamVisibilityMode.LifecycleFaithful
                ? "Final Visibility — Lifecycle-Faithful uses meaningful Coverage " +
                    "to establish the Foam footprint. Explicit Layer C Remaining " +
                    "Life owns ordinary survival, so transport dilution cannot " +
                    "masquerade as early death. Negative topology may still " +
                    "accelerate Remaining Life loss."
                : "Final Visibility — Concentration + Lifetime lets both local " +
                    "Coverage concentration and Remaining Life reduce visibility. " +
                    "Diffuse transported Foam may disappear before its Remaining " +
                    "Life reaches zero by explicit design.";
        }

        private static string ResolveFoamPresenceContractText(
            SerializedProperty footprint)
        {
            if (footprint.hasMultipleDifferentValues)
            {
                return "Presence — mixed selection. Selected Rivers do not share " +
                    "one Presence Footprint mode.";
            }

            return footprint.enumValueIndex ==
                    (int)StylizedRiverFoamPresenceFootprintMode.PresenceAmplitude
                ? "Presence — Presence-Amplitude carries decoded intrinsic " +
                    "Presence through the same Presence-independent shape and " +
                    "surface-coupling weights. Uniform Presence 0.75 therefore " +
                    "produces exactly 75% of the equivalent Presence 1.00 resolved " +
                    "mask before other explicit global controls."
                : "Presence — Coverage-Only stores authored Presence exactly but " +
                    "does not use it as Final Foam amplitude. Coverage, Life, " +
                    "Pattern, Chipping, and Strands determine the visible result.";
        }

        private static string ResolveFoamCombinedContractText(
            SerializedProperty transport,
            SerializedProperty visibility,
            SerializedProperty footprint)
        {
            if (transport.hasMultipleDifferentValues ||
                visibility.hasMultipleDifferentValues ||
                footprint.hasMultipleDifferentValues)
            {
                return "Combined Result — mixed selection. Resolve the three " +
                    "selectors to one shared combination to see an exact summary.";
            }

            StylizedRiverFoamTransportScheme transportScheme =
                (StylizedRiverFoamTransportScheme)transport.enumValueIndex;
            bool tvd = transportScheme ==
                StylizedRiverFoamTransportScheme.TvdSuperbee;
            bool bulkPhase = transportScheme ==
                StylizedRiverFoamTransportScheme.BulkPhaseResidualTvd;
            bool lifecycle = visibility.enumValueIndex ==
                (int)StylizedRiverFinalFoamVisibilityMode.LifecycleFaithful;
            bool amplitude = footprint.enumValueIndex ==
                (int)StylizedRiverFoamPresenceFootprintMode.PresenceAmplitude;

            string transportSummary = bulkPhase
                ? "with the accepted one-dispatch Bulk-Phase transport"
                : tvd
                    ? "with sharper bounded Superbee reconstruction"
                    : "with the more diffuse first-order Donor state";

            return "Combined Result — Foam Coverage is transported " +
                transportSummary + ". " +
                (lifecycle
                    ? "Meaningful living Coverage remains visible until explicit " +
                        "lifecycle aging removes it. "
                    : "Low Coverage concentration and Remaining Life may both " +
                        "remove it from Final Foam. ") +
                (amplitude
                    ? "The surviving resolved shape retains exact proportional " +
                        "intrinsic Presence through all shape coupling."
                    : "Intrinsic Presence remains stored but does not scale the " +
                        "surviving resolved shape.");
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
                Find("foamShoreLateralMovementSuppression"),
                new GUIContent(
                    "Shore Lateral Movement Suppression",
                    "Suppresses lateral/cross-river canonical Foam velocity inside the existing Shore Support field. Zero preserves current lateral movement; one removes it completely at full Shore Support."));
            EditorGUILayout.PropertyField(
                Find("foamShoreDownstreamMovementSuppression"),
                new GUIContent(
                    "Shore Downstream Movement Suppression",
                    "Suppresses downstream canonical Foam velocity inside the existing Shore Support field. Zero preserves current downstream movement; one removes it completely at full Shore Support."));
            EditorGUILayout.PropertyField(
                Find("foamObstacleSlowdownStrength"),
                new GUIContent(
                    "Object Contact Slowdown Falloff",
                    "Controls how quickly the object-contact slowdown halo reaches full authority. Zero disables contact slowdown; any positive value reaches the exact Minimum Speed Factor at full contact influence."));
            EditorGUILayout.PropertyField(
                Find("foamObstacleMinimumDownstreamFactor"),
                new GUIContent(
                    "Object Contact Minimum Speed Factor",
                    "Exact factor applied to the complete routed Foam velocity vector at full contact influence. Zero permits local stagnation and prevents automatic object-source rearm while slowdown is enabled."));
            EditorGUILayout.PropertyField(
                Find("foamObjectContactFullSlowdownReachMetres"),
                new GUIContent(
                    "Object Contact Full Slowdown Reach (m)",
                    "Distance from the obstacle surface over which the contact slowdown remains at full influence."));
            EditorGUILayout.PropertyField(
                Find("foamObjectContactSlowdownOuterReachMetres"),
                new GUIContent(
                    "Object Contact Slowdown Outer Reach (m)",
                    "Outer distance from the obstacle surface where contact slowdown reaches zero. This value is clamped to at least Full Slowdown Reach."));
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
                Find("foamFullSupportedAgingAt"),
                new GUIContent(
                    "Full Supported Aging At",
                    "Raw combined positive-support value at which Supported Aging Rate is applied fully. Lower values make ordinary support preserve Foam more strongly; 0.92 reproduces the previous fixed curve."));
            EditorGUILayout.PropertyField(
                Find("foamNegativeAgingRate"),
                new GUIContent(
                    "Negative Aging Rate",
                    "Aging-rate multiplier at full Negative Aging Pressure. Values above one shorten life."));
        }

        private void DrawFoamLayerD()
        {
            EditorGUILayout.HelpBox(
                "Layer D evaluated-shape controls and previews are diagnostic-only. They do not affect normal Final Foam, persistent material, Remaining Life, or Layer E Chipping.",
                MessageType.Info);
            EditorGUILayout.LabelField(
                "Temporal Shape",
                EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(
                Find("foamVisualOccupancyBuildTime"),
                new GUIContent(
                    "Visual Occupancy Build Time",
                    "Time used by Layer D diagnostic temporal occupancy to build toward the current instantaneous shape target. Normal Final Foam is unchanged."));
            EditorGUILayout.PropertyField(
                Find("foamVisualOccupancyReleaseTime"),
                new GUIContent(
                    "Visual Occupancy Release Time",
                    "Time used by Layer D diagnostic temporal occupancy to release coverage after the instantaneous target recedes. Normal Final Foam is unchanged."));
        }

        private void DrawFoamProductionChipping(
            SerializedProperty presenceFootprint)
        {
            EditorGUILayout.LabelField(
                "Production Chipping",
                EditorStyles.miniBoldLabel);
            EditorGUILayout.HelpBox(
                "Production Chipping is Layer E render-only work. It uses the original full-rate analytical Candidate Field, multiplies it by the soft Eligibility band, and reconstructs the Foam body plus fringe from the chipped pre-hardened signal. Use Chip Candidate Field, Chip Eligibility Composite, Production Chip Mask, and Foam Chip And Strand Probe to inspect the exact relationship.",
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
                    "Approximate inward width of the soft Foam edge band in rendered pixels. Coverage-Only and Presence-Amplitude use derivative-normalized soft-visibility coordinates. Zero disables edge permission exactly. The slider covers 0–256 px; numeric entry accepts any non-negative value for deliberately extreme bands."),
                0f,
                256f);
            bool showPresenceAmplitudeEdgeStart =
                presenceFootprint.hasMultipleDifferentValues ||
                presenceFootprint.enumValueIndex ==
                    (int)StylizedRiverFoamPresenceFootprintMode
                        .PresenceAmplitude;
            if (showPresenceAmplitudeEdgeStart)
            {
                EditorGUILayout.PropertyField(
                    Find("foamChipSoftEdgeStart"),
                    new GUIContent(
                        "Presence-Amplitude Edge Start",
                        "Soft-visibility contour treated as the exterior start of the Presence-Amplitude Eligibility coordinate. Default 0.06 matches the historical Coverage-Only route. Raise it to move the band inward; lower it to include fainter fringe."));
            }

            bool showCoverageOnlyInteriorAccess =
                presenceFootprint.hasMultipleDifferentValues ||
                presenceFootprint.enumValueIndex ==
                    (int)StylizedRiverFoamPresenceFootprintMode.Current;
            if (showCoverageOnlyInteriorAccess)
            {
                EditorGUILayout.PropertyField(
                    Find("foamChipInteriorAccess"),
                    new GUIContent(
                        "Chip Interior Access",
                        "Coverage-Only Presence Footprint only. Fraction of activated candidate identities granted permission in the established body outside Chip Edge Width. Zero is edge-only; one lets every active candidate cut the full visible body. Presence-Amplitude always disables Interior Access."));
            }
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

        }

        private void DrawFoamLayerE()
        {
            SerializedProperty presenceFootprint = Find(
                "foamPresenceFootprintMode");
            DrawFoamProductionChipping(presenceFootprint);

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(
                "Structural Strands",
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
                "Strands run after soft-mask Chipping and own structural anisotropic lineification.",
                MessageType.None);

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(
                "Final Composition",
                EditorStyles.miniBoldLabel);
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

        private void DrawAutomaticShorePopulationPrediction()
        {
            StylizedRiver selectedRiver = target as StylizedRiver;
            if (selectedRiver == null || targets.Length != 1)
            {
                DrawReadOnlyRow(
                    new GUIContent("Predicted Active Heads"),
                    "Select one river");
                return;
            }

            float representedRiverLength = selectedRiver.Domain.IsValid
                ? Mathf.Max(0f, selectedRiver.Domain.LocalLength)
                : 0f;
            float representedBankLength = representedRiverLength * 2f;
            float meanHeadCount = Mathf.Clamp01(
                    selectedRiver.FoamShoreFoamActivity) *
                representedBankLength /
                Mathf.Max(
                    0.01f,
                    StylizedRiver.AutomaticShoreFullActivityHeadSpacingMetres);
            int minimumHeadCount = Mathf.FloorToInt(meanHeadCount);
            int maximumHeadCount = Mathf.CeilToInt(meanHeadCount);
            string predicted = minimumHeadCount == maximumHeadCount
                ? $"{minimumHeadCount} (mean {meanHeadCount:0.##})"
                : $"{minimumHeadCount}-{maximumHeadCount} " +
                  $"(mean {meanHeadCount:0.##})";
            DrawReadOnlyRow(
                new GUIContent(
                    "Predicted Active Heads",
                    "Long-term target range resolved from Activity and the represented shoreline length. Packet clearance or invalid geometry may temporarily keep the live count below this target."),
                predicted);

            int chunkCount = representedRiverLength > 0f
                ? Mathf.Max(1, Mathf.CeilToInt(representedRiverLength / 32f))
                : 0;
            DrawReadOnlyRow(
                new GUIContent("Represented Shoreline"),
                $"{representedBankLength:0.#} m across 2 banks | " +
                $"{chunkCount} Foam chunk(s)");

            StylizedRiverFoamRuntime runtime = Application.isPlaying
                ? selectedRiver.GetComponent<StylizedRiverFoamRuntime>()
                : null;
            if (runtime != null)
            {
                DrawReadOnlyRow(
                    new GUIContent("Runtime Shore Population"),
                    runtime.AutomaticShoreBirthStatus);
            }
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
            SerializedProperty maximum,
            string tooltip = null)
        {
            EditorGUILayout.LabelField(label, EditorStyles.miniBoldLabel);
            EditorGUI.indentLevel++;
            string resolvedTooltip = string.IsNullOrEmpty(tooltip)
                ? $"Authored {label.ToLowerInvariant()} range in metres."
                : tooltip;
            EditorGUILayout.PropertyField(
                minimum,
                new GUIContent("Min", resolvedTooltip));
            EditorGUILayout.PropertyField(
                maximum,
                new GUIContent("Max", resolvedTooltip));
            EditorGUI.indentLevel--;
        }

        private void DrawMinMaxCellControls(
            string label,
            SerializedProperty minimum,
            SerializedProperty maximum,
            string tooltip = null)
        {
            EditorGUILayout.LabelField(label, EditorStyles.miniBoldLabel);
            EditorGUI.indentLevel++;
            string resolvedTooltip = string.IsNullOrEmpty(tooltip)
                ? $"Authored {label.ToLowerInvariant()} range in Foam cells."
                : tooltip;
            EditorGUILayout.PropertyField(
                minimum,
                new GUIContent("Min Cells", resolvedTooltip));
            EditorGUILayout.PropertyField(
                maximum,
                new GUIContent("Max Cells", resolvedTooltip));
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
                "Automatic birth creates finite Layer C material packets. Shore Activity controls a river-length-scaled target active-head population across the complete shoreline, while Minimum Packet Gap enforces physical packet clearance. Object and Free-Water categories retain their own population controls.",
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
                    Find("foamShoreFoamActivity"),
                    new GUIContent(
                        "Activity",
                        "Controls the target active Shore-head population from the represented shoreline length. Zero requests no heads; one requests approximately one head per 17.5 metres across both banks. Minimum Packet Gap remains the final placement authority."));
                DrawAutomaticShorePopulationPrediction();
                EditorGUILayout.PropertyField(
                    Find("foamShoreMinimumPacketGapMetres"),
                    new GUIContent(
                        "Minimum Packet Gap (m)",
                        "Minimum downstream clearance reserved after a Shore packet completes. It rearms the same slot and extends shared cross-source packet separation."));
                EditorGUILayout.HelpBox(
                    "All shoreline scheduling buckets remain eligible. Activity resolves a river-length-scaled active-head target, Minimum Packet Gap enforces physical separation, and each selected pattern starts in the nearest Foam cell touching the current visible shore.",
                    MessageType.None);
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
                    DrawMinMaxCellControls(
                        "Segment Length",
                        Find("foamShoreRibbonLengthMinCells"),
                        Find("foamShoreRibbonLengthMaxCells"));
                    DrawReadOnlyRow(
                        new GUIContent(
                            "Birth Head",
                            "Shore Ribbon birth is structurally fixed to one longitudinal cell by one lateral cell. Width and head-size controls cannot expand it."),
                        "Fixed 1 × 1 cell");
                    DrawReadOnlyRow(
                        new GUIContent(
                            "Shore Placement",
                            "Every Ribbon head uses the nearest valid Foam cell touching the current visible shore."),
                        "Nearest shore cell");
                    EditorGUILayout.PropertyField(
                        Find("foamShoreRibbonRevealSpeedCellsPerSecond"),
                        new GUIContent("Reveal Speed (Cells/s)"));
                    DrawMinMaxUnitControls(
                        "Initial Presence",
                        Find("foamShoreRibbonInitialPresenceMin"),
                        Find("foamShoreRibbonInitialPresenceMax"),
                        "Intrinsic Presence written exactly to newly occupied material for this pattern. Shape, progressive reveal, subcell width, and valid-fluid clipping affect geometric Coverage only.");
                    DrawMinMaxUnitControls(
                        "Initial Life",
                        Find("foamShoreRibbonInitialLifeMin"),
                        Find("foamShoreRibbonInitialLifeMax"),
                        "Initial normalized Remaining Life written exactly to newly occupied material. One writes the full life budget; only explicit Layer C aging changes it afterward.");
                    EditorGUI.indentLevel--;
                }

                if (DrawInlineFoldout(
                        InspectorSection.FoamBirthInwardWashPattern,
                        "Inward Wash Pattern"))
                {
                    EditorGUI.indentLevel++;
                    DrawMinMaxCellControls(
                        "Along-Bank Length",
                        Find("foamInwardWashAlongLengthMinCells"),
                        Find("foamInwardWashAlongLengthMaxCells"));
                    DrawMinMaxCellControls(
                        "Stroke Width",
                        Find("foamInwardWashWidthMinCells"),
                        Find("foamInwardWashWidthMaxCells"));
                    DrawMinMaxCellControls(
                        "Inward Reach",
                        Find("foamInwardWashReachMinCells"),
                        Find("foamInwardWashReachMaxCells"));
                    EditorGUILayout.PropertyField(Find("foamInwardWashHeadLengthCells"), new GUIContent("Head Length Cells"));
                    EditorGUILayout.PropertyField(Find("foamInwardWashHeadWidthCells"), new GUIContent("Head Width Cells"));
                    DrawReadOnlyRow(
                        new GUIContent(
                            "Shore Placement",
                            "Every Inward Wash starts in the nearest valid Foam cell touching the current visible shore."),
                        "Nearest shore cell");
                    DrawMinMaxCellControls(
                        "Bend Amplitude",
                        Find("foamInwardWashBendAmplitudeMinCells"),
                        Find("foamInwardWashBendAmplitudeMaxCells"));
                    EditorGUILayout.PropertyField(Find("foamInwardWashRevealSpeedCellsPerSecond"), new GUIContent("Reveal Speed (Cells/s)"));
                    DrawMinMaxUnitControls(
                        "Initial Presence",
                        Find("foamInwardWashInitialPresenceMin"),
                        Find("foamInwardWashInitialPresenceMax"),
                        "Intrinsic Presence written exactly to newly occupied material for this pattern. Shape, progressive reveal, subcell width, and valid-fluid clipping affect geometric Coverage only.");
                    DrawMinMaxUnitControls(
                        "Initial Life",
                        Find("foamInwardWashInitialLifeMin"),
                        Find("foamInwardWashInitialLifeMax"),
                        "Initial normalized Remaining Life written exactly to newly occupied material. One writes the full life budget; only explicit Layer C aging changes it afterward.");
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
                        "Fleck Coverage",
                        "How much of the registered static object/contact population can participate in supplemental Contact Fleck events."));
                EditorGUILayout.PropertyField(
                    Find("foamObjectFoamActivity"),
                    new GUIContent(
                        "Fleck Activity",
                        "How promptly an eligible object attempts to start a finite Contact Fleck. Activity cannot bypass the shared per-object packet-clearance gate, and one Fleck cannot chain directly into another when contact cycles are enabled."));
                EditorGUILayout.PropertyField(
                    Find("foamObjectContactMinimumPacketGapMetres"),
                    new GUIContent(
                        "Object Contact Minimum Packet Gap (m)",
                        "Minimum downstream gap between released Object packets from the same anchor. It also extends shared cross-source packet separation. Contact-only reinforcement from the same anchor remains the intentional overlap exemption."));
                EditorGUILayout.PropertyField(
                    Find("foamObjectContactStrokeCount"),
                    new GUIContent(
                        "Object Contact Stroke Count",
                        "Finite initial Arc/Semi-Arc burst size. Stroke one progressively establishes a complete narrow ring around the obstacle and then emits the recipe's finite wake arm or arms once. Later Arc strokes reinforce the complete Arc contact profile; later Semi-Arc strokes reinforce only the selected Semi-Arc half-profile. Range 1–3; default 2."));
                EditorGUILayout.PropertyField(
                    Find("foamObjectContactReinforcementEnabled"),
                    new GUIContent(
                        "Contact Reinforcement Enabled",
                        "Enables independent finite contact maintenance after a full Arc/Semi-Arc packet. Arc maintenance emits one complete Arc contact-profile stroke; Semi-Arc maintenance emits one selected half-profile stroke. Neither emits wake arms, and each event ends after one stroke."));
                using (new EditorGUI.DisabledScope(
                    !Find("foamObjectContactReinforcementEnabled").boolValue))
                {
                    EditorGUILayout.PropertyField(
                        Find("foamObjectContactReinforcementIntervalSeconds"),
                        new GUIContent(
                            "Contact Reinforcement Interval (s)",
                            "Seconds between finite contact-only maintenance strokes while the next full Object packet is still waiting for released-wake clearance. Full packets keep priority and reinforcement never changes their eligibility."));
                }
                EditorGUILayout.PropertyField(
                    Find("foamObjectFoamFormationSpeedMetresPerSecond"),
                    new GUIContent(
                        "Base Reveal Speed",
                        "Base reveal speed used independently by each finite Arc/Semi-Arc stroke and by each Fleck. Stroke Count changes total burst duration, not metres-per-second reveal speed. Activity, packet clearance, and later Layer C transport remain independent."));

                EditorGUILayout.Space(4f);
                EditorGUILayout.LabelField(
                    "Contact Packets & Reinforcement",
                    EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(
                    Find("foamObjectContactCycleCoverage"),
                    new GUIContent(
                        "Anchor Coverage",
                        "Stable share of registered object anchors that can emit finite Arc/Semi-Arc reinforcement bursts. One includes every eligible object."));
                EditorGUILayout.HelpBox(
                    "Full Arc/Semi-Arc packets remain finite and distance-spaced. Their first stroke establishes one complete obstacle-contact ring and emits the recipe wake geometry once. Optional later initial strokes and independent maintenance use recipe contact geometry only: complete Arc or selected Semi-Arc half-profile, with no wake arms. Maintenance never changes full-packet eligibility and runs only while the next full packet remains in clearance. Full packets have priority over reinforcement; reinforcement has priority over Flecks. No persistent material-cadence emitter exists.",
                    MessageType.None);

                EditorGUILayout.PropertyField(
                    Find("foamObjectFoamPattern"),
                    new GUIContent(
                        "Debug Pattern Mode",
                        "Mixed uses Arc and Semi-Arc weights for per-object contact cycles and enables supplemental Flecks through their independent Coverage and Activity controls. Pure modes force one pattern for validation."));
                EditorGUILayout.HelpBox(
                    "The first Arc or Semi-Arc stroke derives a one-cell contact ring locally from the existing obstacle-exclusion field around the actual rock boundary, then emits only the recipe's finite straight downstream wake arm geometry. Later Arc strokes use the complete five-point contact profile; later Semi-Arc strokes use the deterministic selected half-profile. No later stroke emits a wake arm. Fleck geometry remains independent, and Static Pressure Front Reach cannot widen any object source.",
                    MessageType.None);

                EditorGUILayout.Space(4f);
                EditorGUILayout.LabelField("Pattern Mix", EditorStyles.boldLabel);
                SerializedProperty arcWeight = Find("foamObjectContactArcPatternWeight");
                SerializedProperty semiArcWeight = Find("foamObjectContactSemiArcPatternWeight");
                DrawNormalizedPatternWeight(
                    arcWeight,
                    semiArcWeight,
                    new GUIContent(
                        "Contact Arcs",
                        "Relative share of Mixed per-object contact cycles assigned to full Contact Arcs. Flecks are independent and are not part of this normalized cycle mix."));
                DrawNormalizedPatternWeight(
                    semiArcWeight,
                    arcWeight,
                    new GUIContent(
                        "Contact Semi-Arcs",
                        "Relative share of Mixed per-object contact cycles assigned to single-arm Contact Semi-Arcs. Flecks are independent and are not part of this normalized cycle mix."));
                EditorGUILayout.HelpBox(
                    "Contact Flecks are an independent packet population. Their Coverage and Activity control eligibility behind the full-packet clearance clock; Arc/Semi-Arc weights do not scale Fleck rate. Contact-only reinforcement uses its own interval and does not consume Fleck Activity.",
                    MessageType.None);

                EditorGUILayout.Space(4f);
                if (DrawInlineFoldout(
                        InspectorSection.FoamBirthObjectContactArcPattern,
                        "Object Contact Arc Pattern"))
                {
                    EditorGUI.indentLevel++;
                    DrawMinMaxCellControls("Contact Span", Find("foamObjectArcContactSpanMinCells"), Find("foamObjectArcContactSpanMaxCells"));
                    DrawMinMaxCellControls("Contact Width", Find("foamObjectArcContactWidthMinCells"), Find("foamObjectArcContactWidthMaxCells"));
                    DrawMinMaxCellControls("Wake Arm Length", Find("foamObjectArcWakeLengthMinCells"), Find("foamObjectArcWakeLengthMaxCells"));
                    DrawMinMaxCellControls("Wake Arm Width", Find("foamObjectArcWakeWidthMinCells"), Find("foamObjectArcWakeWidthMaxCells"));
                    EditorGUILayout.PropertyField(Find("foamObjectArcHeadLengthCells"), new GUIContent("Head Length Cells"));
                    EditorGUILayout.PropertyField(Find("foamObjectArcHeadWidthCells"), new GUIContent("Head Width Cells"));
                    EditorGUILayout.PropertyField(Find("foamObjectArcAlongFlowOffsetCells"), new GUIContent("Along-Flow Offset Cells"));
                    EditorGUILayout.PropertyField(Find("foamObjectArcAcrossRiverOffsetCells"), new GUIContent("Across-River Offset Cells"));
                    EditorGUILayout.PropertyField(Find("foamObjectArcRevealSpeedCellsPerSecond"), new GUIContent("Reveal Speed (Cells/s)"));
                    DrawMinMaxUnitControls(
                        "Initial Presence",
                        Find("foamObjectContactArcInitialPresenceMin"),
                        Find("foamObjectContactArcInitialPresenceMax"),
                        "Intrinsic Presence written exactly to newly occupied open-C Arc material. Immediate-contact geometry and valid-fluid clipping affect Coverage only. Arc ribbons use no breakup or patterned source-fill holes.");
                    DrawMinMaxUnitControls(
                        "Initial Life",
                        Find("foamObjectContactArcInitialLifeMin"),
                        Find("foamObjectContactArcInitialLifeMax"),
                        "Initial normalized Remaining Life written exactly to newly occupied material. One writes the full life budget; only explicit Layer C aging changes it afterward.");
                    EditorGUI.indentLevel--;
                }

                if (DrawInlineFoldout(
                        InspectorSection.FoamBirthObjectContactSemiArcPattern,
                        "Object Contact Semi-Arc Pattern"))
                {
                    EditorGUI.indentLevel++;
                    DrawMinMaxCellControls("Contact Span", Find("foamObjectSemiArcContactSpanMinCells"), Find("foamObjectSemiArcContactSpanMaxCells"));
                    DrawMinMaxCellControls("Contact Width", Find("foamObjectSemiArcContactWidthMinCells"), Find("foamObjectSemiArcContactWidthMaxCells"));
                    DrawMinMaxCellControls("Wake Arm Length", Find("foamObjectSemiArcWakeLengthMinCells"), Find("foamObjectSemiArcWakeLengthMaxCells"));
                    DrawMinMaxCellControls("Wake Arm Width", Find("foamObjectSemiArcWakeWidthMinCells"), Find("foamObjectSemiArcWakeWidthMaxCells"));
                    EditorGUILayout.PropertyField(Find("foamObjectSemiArcHeadLengthCells"), new GUIContent("Head Length Cells"));
                    EditorGUILayout.PropertyField(Find("foamObjectSemiArcHeadWidthCells"), new GUIContent("Head Width Cells"));
                    EditorGUILayout.PropertyField(Find("foamObjectSemiArcAlongFlowOffsetCells"), new GUIContent("Along-Flow Offset Cells"));
                    EditorGUILayout.PropertyField(Find("foamObjectSemiArcAcrossRiverOffsetCells"), new GUIContent("Across-River Offset Cells"));
                    EditorGUILayout.PropertyField(Find("foamObjectSemiArcRevealSpeedCellsPerSecond"), new GUIContent("Reveal Speed (Cells/s)"));
                    DrawMinMaxUnitControls(
                        "Initial Presence",
                        Find("foamObjectContactSemiArcInitialPresenceMin"),
                        Find("foamObjectContactSemiArcInitialPresenceMax"),
                        "Intrinsic Presence written exactly to newly occupied open-C Semi-Arc material. Immediate-contact geometry and valid-fluid clipping affect Coverage only. Semi-Arc ribbons use no breakup or patterned source-fill holes.");
                    DrawMinMaxUnitControls(
                        "Initial Life",
                        Find("foamObjectContactSemiArcInitialLifeMin"),
                        Find("foamObjectContactSemiArcInitialLifeMax"),
                        "Initial normalized Remaining Life written exactly to newly occupied material. One writes the full life budget; only explicit Layer C aging changes it afterward.");
                    EditorGUI.indentLevel--;
                }

                if (DrawInlineFoldout(
                        InspectorSection.FoamBirthObjectContactFleckPattern,
                        "Object Contact Fleck Pattern"))
                {
                    EditorGUI.indentLevel++;
                    DrawMinMaxCellControls("Fleck Length", Find("foamObjectFleckLengthMinCells"), Find("foamObjectFleckLengthMaxCells"));
                    DrawMinMaxCellControls("Fleck Width", Find("foamObjectFleckWidthMinCells"), Find("foamObjectFleckWidthMaxCells"));
                    EditorGUILayout.PropertyField(Find("foamObjectFleckHeadLengthCells"), new GUIContent("Head Length Cells"));
                    EditorGUILayout.PropertyField(Find("foamObjectFleckHeadWidthCells"), new GUIContent("Head Width Cells"));
                    DrawMinMaxCellControls("Contact Offset", Find("foamObjectFleckOffsetMinCells"), Find("foamObjectFleckOffsetMaxCells"));
                    EditorGUILayout.PropertyField(Find("foamObjectFleckRevealSpeedCellsPerSecond"), new GUIContent("Reveal Speed (Cells/s)"));
                    DrawMinMaxUnitControls(
                        "Initial Presence",
                        Find("foamObjectContactFleckInitialPresenceMin"),
                        Find("foamObjectContactFleckInitialPresenceMax"),
                        "Intrinsic Presence written exactly to newly occupied material for this pattern. Shape, progressive reveal, subcell width, and valid-fluid clipping affect geometric Coverage only.");
                    DrawMinMaxUnitControls(
                        "Initial Life",
                        Find("foamObjectContactFleckInitialLifeMin"),
                        Find("foamObjectContactFleckInitialLifeMax"),
                        "Initial normalized Remaining Life written exactly to newly occupied material. One writes the full life budget; only explicit Layer C aging changes it afterward.");
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
                        "How promptly an eligible Free Water slot starts a finite packet. Zero disables starts; one fires immediately after clearance."));
                EditorGUILayout.PropertyField(
                    Find("foamFreeWaterMinimumPacketGapMetres"),
                    new GUIContent(
                        "Minimum Packet Gap (m)",
                        "Minimum downstream clearance reserved after a Free Water packet completes. It rearms the same slot and extends shared cross-source packet separation."));
                EditorGUILayout.HelpBox(
                    "D8.2 cell geometry is staged only. Legacy metric Free-Water birth geometry remains active until D8.3 conversion.",
                    MessageType.Info);
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
                    DrawMinMaxCellControls("Length", Find("foamFreeWaterLaceLengthMinCells"), Find("foamFreeWaterLaceLengthMaxCells"));
                    DrawMinMaxCellControls("Width", Find("foamFreeWaterLaceWidthMinCells"), Find("foamFreeWaterLaceWidthMaxCells"));
                    EditorGUILayout.PropertyField(Find("foamFreeWaterLaceHeadLengthCells"), new GUIContent("Head Length Cells"));
                    EditorGUILayout.PropertyField(Find("foamFreeWaterLaceHeadWidthCells"), new GUIContent("Head Width Cells"));
                    DrawMinMaxCellControls("Bend Amplitude", Find("foamFreeWaterLaceBendMinCells"), Find("foamFreeWaterLaceBendMaxCells"));
                    EditorGUILayout.PropertyField(Find("foamFreeWaterLaceRevealSpeedCellsPerSecond"), new GUIContent("Reveal Speed (Cells/s)"));
                    DrawMinMaxUnitControls(
                        "Initial Presence",
                        Find("foamFreeWaterLaceInitialPresenceMin"),
                        Find("foamFreeWaterLaceInitialPresenceMax"),
                        "Intrinsic Presence written exactly to newly occupied material for this pattern. Shape, progressive reveal, subcell width, and valid-fluid clipping affect geometric Coverage only.");
                    DrawMinMaxUnitControls(
                        "Initial Life",
                        Find("foamFreeWaterLaceInitialLifeMin"),
                        Find("foamFreeWaterLaceInitialLifeMax"),
                        "Initial normalized Remaining Life written exactly to newly occupied material. One writes the full life budget; only explicit Layer C aging changes it afterward.");
                    EditorGUI.indentLevel--;
                }

                if (DrawInlineFoldout(
                        InspectorSection.FoamBirthFreeWaterCrossLacePattern,
                        "Free Water Cross-Lace Connector Pattern"))
                {
                    EditorGUI.indentLevel++;
                    DrawMinMaxCellControls("Length", Find("foamFreeWaterCrossLaceLengthMinCells"), Find("foamFreeWaterCrossLaceLengthMaxCells"));
                    DrawMinMaxCellControls("Width", Find("foamFreeWaterCrossLaceWidthMinCells"), Find("foamFreeWaterCrossLaceWidthMaxCells"));
                    EditorGUILayout.PropertyField(Find("foamFreeWaterCrossLaceHeadLengthCells"), new GUIContent("Head Length Cells"));
                    EditorGUILayout.PropertyField(Find("foamFreeWaterCrossLaceHeadWidthCells"), new GUIContent("Head Width Cells"));
                    DrawMinMaxCellControls("Flow-Bend Amplitude", Find("foamFreeWaterCrossLaceBendMinCells"), Find("foamFreeWaterCrossLaceBendMaxCells"));
                    EditorGUILayout.PropertyField(Find("foamFreeWaterCrossLaceRevealSpeedCellsPerSecond"), new GUIContent("Reveal Speed (Cells/s)"));
                    DrawMinMaxUnitControls(
                        "Initial Presence",
                        Find("foamFreeWaterCrossLaceInitialPresenceMin"),
                        Find("foamFreeWaterCrossLaceInitialPresenceMax"),
                        "Intrinsic Presence written exactly to newly occupied material for this pattern. Shape, progressive reveal, subcell width, and valid-fluid clipping affect geometric Coverage only.");
                    DrawMinMaxUnitControls(
                        "Initial Life",
                        Find("foamFreeWaterCrossLaceInitialLifeMin"),
                        Find("foamFreeWaterCrossLaceInitialLifeMax"),
                        "Initial normalized Remaining Life written exactly to newly occupied material. One writes the full life budget; only explicit Layer C aging changes it afterward.");
                    EditorGUI.indentLevel--;
                }

                if (DrawInlineFoldout(
                        InspectorSection.FoamBirthFreeWaterFragmentPattern,
                        "Free Water Broken Filament (staged Torn identity)"))
                {
                    EditorGUI.indentLevel++;
                    DrawMinMaxCellControls("Broken Filament Length", Find("foamFreeWaterBrokenFilamentLengthMinCells"), Find("foamFreeWaterBrokenFilamentLengthMaxCells"));
                    DrawMinMaxCellControls("Broken Filament Width", Find("foamFreeWaterBrokenFilamentWidthMinCells"), Find("foamFreeWaterBrokenFilamentWidthMaxCells"));
                    EditorGUILayout.PropertyField(Find("foamFreeWaterBrokenFilamentHeadLengthCells"), new GUIContent("Head Length Cells"));
                    EditorGUILayout.PropertyField(Find("foamFreeWaterBrokenFilamentHeadWidthCells"), new GUIContent("Head Width Cells"));
                    DrawMinMaxCellControls("Bend Amplitude", Find("foamFreeWaterBrokenFilamentBendMinCells"), Find("foamFreeWaterBrokenFilamentBendMaxCells"));
                    EditorGUILayout.PropertyField(Find("foamFreeWaterBrokenFilamentBreakCountMin"), new GUIContent("Break Count Min"));
                    EditorGUILayout.PropertyField(Find("foamFreeWaterBrokenFilamentBreakCountMax"), new GUIContent("Break Count Max"));
                    EditorGUILayout.PropertyField(Find("foamFreeWaterBrokenFilamentRevealSpeedCellsPerSecond"), new GUIContent("Reveal Speed (Cells/s)"));
                    DrawMinMaxUnitControls(
                        "Initial Presence",
                        Find("foamFreeWaterFragmentInitialPresenceMin"),
                        Find("foamFreeWaterFragmentInitialPresenceMax"),
                        "Intrinsic Presence written exactly to newly occupied material for this pattern. Shape, progressive reveal, subcell width, and valid-fluid clipping affect geometric Coverage only.");
                    DrawMinMaxUnitControls(
                        "Initial Life",
                        Find("foamFreeWaterFragmentInitialLifeMin"),
                        Find("foamFreeWaterFragmentInitialLifeMax"),
                        "Initial normalized Remaining Life written exactly to newly occupied material. One writes the full life budget; only explicit Layer C aging changes it afterward.");
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.HelpBox(
                    "Free Water Foam emits finite one-shot packets. Lace, Cross-Lace, and Torn Fragment slots must clear their configured downstream packet gap before rearming. Bright specular glints are intentionally not spawned as persistent material.",
                    MessageType.Info);
                EditorGUI.indentLevel--;
            }


        }
    }
}
