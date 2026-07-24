using UnityEditor;
using UnityEngine;

namespace ProgrammaticStylized3D.Weather.Editor
{
    [CustomEditor(typeof(WeatherLightRayAnchor))]
    public sealed class WeatherLightRayAnchorEditor : UnityEditor.Editor
    {
        private bool showBinding;
        private bool showLifecycle;
        private bool showShape;
        private bool showStrands;
        private bool showAtmosphere;
        private bool showSurface;
        private bool showEvolution;
        private bool showActions;
        private bool showLiveStatus;

        public override void OnInspectorGUI()
        {
            var anchor = (WeatherLightRayAnchor)target;
            serializedObject.UpdateIfRequiredOrScript();
            WeatherInspectorGui.DrawScriptReference(serializedObject);

            WeatherInspectorGui.Info(
                "Authored visual proof using the same immutable per-ray descriptor " +
                "reserved for future procedural and gameplay-created LightRays. " +
                "The current renderer still accepts one active authored anchor.");

            if (!string.IsNullOrEmpty(anchor.LastError))
            {
                WeatherInspectorGui.Error(anchor.LastError);
            }

            DrawBinding();
            DrawLifecycle();
            DrawShape();
            DrawStrands();
            DrawAtmosphere();
            DrawSurface();
            DrawEvolution();

            bool changed = serializedObject.ApplyModifiedProperties();
            if (changed)
            {
                anchor.RefreshRegistration();
                EditorUtility.SetDirty(anchor);
                SceneView.RepaintAll();
            }

            DrawActions(anchor);
            DrawLiveStatus(anchor);
        }

        private void DrawBinding()
        {
            if (!WeatherInspectorGui.Foldout(
                    ref showBinding,
                    "Binding & Policy",
                    "Selects the controller, celestial source, cloud policy, and source gate."))
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                WeatherInspectorGui.Property(
                    serializedObject,
                    "controllerOverride",
                    "Controller Override",
                    "Optional explicit Weather LightRay Controller. When unassigned, the published controller is used.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "previewInEditMode",
                    "Update in Edit Mode",
                    "Keeps the authored descriptor and visual proof current outside Play Mode.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "sourceKind",
                    "Light Source",
                    "Selects Sun or the future Moon source. Moon remains unavailable until Time of Day publishes an approved Moon light.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "cloudPolicy",
                    "Cloud Policy",
                    "Respect Clouds fades through the validated cloud-transmission query. Ignore Clouds renders a divine ray without changing the cloud cookie.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "sourceGatePolicy",
                    "Source Gate",
                    "Require Active Source respects the celestial intensity and horizon gate. Ignore Source Gate keeps the ray eligible even when that gate is closed.");
                WeatherInspectorGui.ReadOnlyRow(
                    "Movement",
                    "Static (only implemented mode)");
            }
        }

        private void DrawLifecycle()
        {
            if (!WeatherInspectorGui.Foldout(
                    ref showLifecycle,
                    "Lifecycle",
                    "Controls timed, permanent, or externally controlled visibility."))
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                SerializedProperty policy = WeatherInspectorGui.Property(
                    serializedObject,
                    "lifetimePolicy",
                    "Lifetime Policy",
                    "Timed runs one fade-in, hold, and fade-out sequence. Permanent remains registered. Externally Controlled follows the External Visibility request.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "fadeInDurationSeconds",
                    "Fade-In Duration (s)",
                    "Time used by timed activation and by source, cloud, or external visibility recovery.");

                WeatherLightRayLifetimePolicy lifetimePolicy = policy != null
                    ? (WeatherLightRayLifetimePolicy)policy.enumValueIndex
                    : WeatherLightRayLifetimePolicy.Permanent;
                if (lifetimePolicy == WeatherLightRayLifetimePolicy.Timed)
                {
                    WeatherInspectorGui.Property(
                        serializedObject,
                        "holdDurationSeconds",
                        "Hold Duration (s)",
                        "Time spent at full timed-lifecycle weight before fade-out begins.");
                }
                else if (lifetimePolicy ==
                    WeatherLightRayLifetimePolicy.ExternallyControlled)
                {
                    WeatherInspectorGui.Property(
                        serializedObject,
                        "externallyControlledVisible",
                        "External Visibility",
                        "Authoring proof input for an externally controlled ray. Runtime owners may call SetExternallyControlledVisible.");
                }

                WeatherInspectorGui.Property(
                    serializedObject,
                    "fadeOutDurationSeconds",
                    "Fade-Out Duration (s)",
                    "Time used by timed expiry and by source, cloud, or external visibility loss.");
            }
        }

        private void DrawShape()
        {
            if (!WeatherInspectorGui.Foldout(
                    ref showShape,
                    "Shape",
                    "Defines the containing gameplay zone and the faint visual envelope."))
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                WeatherInspectorGui.Property(
                    serializedObject,
                    "radiusMetres",
                    "Ground Radius (m)",
                    "Base radius of the authoritative LightRay zone and initial ground footprint.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "topRadiusScale",
                    "Top Radius Scale",
                    "Top radius divided by ground radius. This changes bundle taper without filling the volume.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "heightMetres",
                    "Height (m)",
                    "Distance from the anchor toward the source along the bounded presentation direction.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "visualEnvelopeRadiusScale",
                    "Visual Envelope Radius Scale",
                    "Scales only the almost-invisible atmospheric envelope. It does not change the authoritative footprint radius.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "visualEnvelopeEdgeSoftness",
                    "Visual Envelope Edge Softness",
                    "Feathers only the almost-invisible containing haze boundary. It does not soften individual shaft gaps or the ground footprint.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "maximumVisualLeanDegrees",
                    "Maximum Visual Lean (°)",
                    "Maximum deviation from straight down. The source profile may impose a stricter limit.");
                WeatherInspectorGui.Help(
                    "The anchor Transform is the ground/base centre. No ground raycast or automatic placement correction is performed.");
            }
        }

        private void DrawStrands()
        {
            if (!WeatherInspectorGui.Foldout(
                    ref showStrands,
                    "Internal Ray Structure",
                    "Builds several separated sunlight shafts inside the containing zone."))
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                WeatherInspectorGui.Property(
                    serializedObject,
                    "strandCount",
                    "Strand Count",
                    "Number of distinguishable shafts. V1.1A/B supports one to eight strands in the one-ray shader.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "strandWidthRange",
                    "Strand Width Range",
                    "Minimum and maximum strand radius as fractions of the local LightRay radius.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "strandSpread",
                    "Strand Spread / Separation",
                    "How far strand centres spread from the bundle centre. Higher values increase visible gaps.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "strandPositionVariation",
                    "Position Variation",
                    "Perturbs the evenly distributed strand positions without replacing them with unrestricted random overlap.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "strandIntensityVariation",
                    "Intensity Variation",
                    "Stable per-strand brightness difference before subtle time evolution.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "strandLengthVariation",
                    "Length Variation",
                    "Shortens secondary shafts by different amounts while keeping one primary shaft continuous.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "strandTaper",
                    "Strand Taper",
                    "Changes strand width along the source axis independently of the broad bundle taper.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "strandEdgeSoftness",
                    "Strand Edge Softness",
                    "Feathers each shaft edge while preserving gaps between separate shafts.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "strandClusterBias",
                    "Cluster Bias",
                    "Moves strand centres toward the bundle centre at higher values or distributes them farther outward at lower values.");
            }
        }

        private void DrawAtmosphere()
        {
            if (!WeatherInspectorGui.Foldout(
                    ref showAtmosphere,
                    "Atmospheric Appearance",
                    "Controls warm shaft visibility, faint envelope haze, and directional softness."))
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                WeatherInspectorGui.Property(
                    serializedObject,
                    "colourMultiplier",
                    "HDR Sunray Colour",
                    "Multiplies the source colour after Sun Warmth Contribution is applied. Leave white for the shared warm baseline; use this only for authored tinting or HDR amplification.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "warmthContribution",
                    "Sun Warmth Contribution",
                    "Blends the resolved Sun colour toward a restrained warm-golden sunlight reference before applying the HDR colour multiplier. Zero preserves the source colour; one uses the full warm target.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "shaftIntensity",
                    "Strand Intensity",
                    "Brightness of the structured shafts. This no longer fills the containing volume.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "envelopeHazeIntensity",
                    "Envelope Haze Intensity",
                    "Very weak atmospheric continuity around the strand bundle. Keep substantially below Strand Intensity.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "scatterLength",
                    "Directional Scatter Length",
                    "Length of source-aligned screen-space softening in quarter-resolution texels.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "scatterSoftness",
                    "Directional Scatter Softness",
                    "Blends from the direct strand mask toward its source-aligned filtered result without lateral blur.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "heightFade",
                    "Atmospheric End Fade",
                    "Fraction of the volume used to soften top and ground-end atmospheric caps. Surface lighting remains separate.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "cameraIntersectionFade",
                    "Camera Intersection Fade",
                    "Suppresses broad screen wash when the camera enters the LightRay envelope.");
            }
        }

        private void DrawSurface()
        {
            if (!WeatherInspectorGui.Foldout(
                    ref showSurface,
                    "Surface Illumination",
                    "Controls the separate ground and visible-object light contribution."))
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                WeatherInspectorGui.Property(
                    serializedObject,
                    "groundLightIntensity",
                    "Ground Light Intensity",
                    "Stylized brightness applied to upward-facing depth-visible Ground, River, and other approximately horizontal surfaces inside the zone.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "surfaceLightIntensity",
                    "Visible Object Light Intensity",
                    "Stylized brightness applied to steeper depth-visible objects such as rocks, trunks, walls, Vegetation, and characters inside the zone.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "cloudCompensationIntensity",
                    "Cloud Compensation Intensity",
                    "For Ignore Clouds rays, restores brightness where the main Sun cookie shades visible surfaces. It does not change the cookie.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "edgeSoftness",
                    "Footprint Edge Softness",
                    "Width of the ground and visible-surface transition at the authoritative zone boundary.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "footprintIrregularity",
                    "Footprint Irregularity",
                    "Adds low-amplitude deterministic edge variation so the lit region is not a perfect circular decal.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "coreEmphasis",
                    "Core / Ground Contact Emphasis",
                    "Adds restrained extra brightness beneath strand cores and near ground contact.");
            }
        }

        private void DrawEvolution()
        {
            if (!WeatherInspectorGui.Foldout(
                    ref showEvolution,
                    "Subtle Evolution",
                    "Adds slow independent changes inside a stationary ray bundle."))
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                WeatherInspectorGui.Property(
                    serializedObject,
                    "fluctuationStrength",
                    "Intensity Fluctuation Strength",
                    "Maximum independent per-strand brightness change. Keep low to avoid visible pulsing.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "fluctuationSpeed",
                    "Intensity Fluctuation Speed",
                    "Base rate of slow strand brightness evolution.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "widthBreathingStrength",
                    "Width Breathing Strength",
                    "Small per-strand width change over time.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "lateralDriftStrength",
                    "Lateral Drift Strength",
                    "Tiny internal strand-centre movement as a fraction of local radius. The bundle footprint remains stationary.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "patternEvolutionSpeed",
                    "Pattern Evolution Speed",
                    "Rate of slow strand drift and width phase progression.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "perStrandPhaseVariation",
                    "Per-Strand Phase Variation",
                    "Separates strand evolution phases so the bundle does not pulse in unison.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "variationSeed",
                    "Variation Seed",
                    "Stable seed for strand layout and evolution. It does not change cloud projection or anchor position.");
            }
        }

        private void DrawActions(WeatherLightRayAnchor anchor)
        {
            if (!WeatherInspectorGui.Foldout(
                    ref showActions,
                    "Actions",
                    "Lifecycle actions for the authored proof ray."))
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                if (anchor.LifetimePolicy ==
                    WeatherLightRayLifetimePolicy.Timed &&
                    GUILayout.Button("Restart Timed Lifecycle"))
                {
                    anchor.RestartTimedLifecycle();
                    SceneView.RepaintAll();
                }

                if (anchor.LifetimePolicy !=
                    WeatherLightRayLifetimePolicy.Timed)
                {
                    WeatherInspectorGui.Help(
                        "Restart Timed Lifecycle is available when Lifetime Policy is Timed.");
                }
            }
        }

        private void DrawLiveStatus(WeatherLightRayAnchor anchor)
        {
            if (!WeatherInspectorGui.Foldout(
                    ref showLiveStatus,
                    "Live Status",
                    "Read-only registration, lifecycle, and immutable descriptor state."))
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                WeatherInspectorGui.ReadOnlyObject(
                    "Registered Controller",
                    anchor.RegisteredController);
                WeatherInspectorGui.ReadOnlyRow(
                    "Handle",
                    anchor.Handle.ToString());

                if (anchor.RegisteredController != null &&
                    anchor.RegisteredController.TryGetSnapshot(
                        anchor.Handle,
                        out WeatherLightRaySnapshot snapshot))
                {
                    WeatherInspectorGui.ReadOnlyRow(
                        "Source / Lifetime",
                        $"{snapshot.SourceKind} / {snapshot.LifetimePolicy}");
                    WeatherInspectorGui.ReadOnlyRow(
                        "Lifecycle",
                        snapshot.LifecycleState.ToString());
                    WeatherInspectorGui.ReadOnlyRow(
                        "Current Intensity",
                        snapshot.CurrentIntensity);
                    WeatherInspectorGui.ReadOnlyRow(
                        "Cloud Transmission",
                        snapshot.CurrentCloudTransmission);
                    WeatherInspectorGui.ReadOnlyRow(
                        "Strands",
                        snapshot.Descriptor.StrandCount);
                    WeatherInspectorGui.ReadOnlyRow(
                        "Presentation Direction",
                        snapshot.RayDirectionWorld.ToString("F3"));
                }
                else
                {
                    WeatherInspectorGui.ReadOnlyRow(
                        "Snapshot",
                        "Not registered");
                }
            }
        }
    }
}
