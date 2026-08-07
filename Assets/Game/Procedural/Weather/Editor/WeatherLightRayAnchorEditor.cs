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
        private bool showEvolution;
        private bool showActions;
        private bool showLiveStatus;

        public override void OnInspectorGUI()
        {
            var anchor = (WeatherLightRayAnchor)target;
            serializedObject.UpdateIfRequiredOrScript();
            WeatherInspectorGui.DrawScriptReference(serializedObject);

            WeatherInspectorGui.Info(
                "This Anchor owns placement, lifecycle, source policy, seed, and local intensity. Appearance resolves from Preset Override when assigned, otherwise from the Controller Default Preset.");

            if (!string.IsNullOrEmpty(anchor.LastError))
            {
                WeatherInspectorGui.Error(anchor.LastError);
            }

            DrawBinding();
            DrawLifecycle(anchor);
            DrawShape(anchor);
            DrawVariation();
            DrawActions(anchor);
            DrawLiveStatus(anchor);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawBinding()
        {
            if (!WeatherInspectorGui.Foldout(
                    ref showBinding,
                    "Binding and Policy",
                    "Chooses the controller, source, cloud policy, and source gate."))
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                WeatherInspectorGui.Property(
                    serializedObject,
                    "controllerOverride",
                    "Controller Override",
                    "Optional explicit Weather LightRay Controller. When empty, the published controller is used.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "presetOverride",
                    "Preset Override",
                    "Optional per-ray visual preset. None inherits the Controller Default Preset.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "previewInEditMode",
                    "Preview In Edit Mode",
                    "Keeps this authored proof registered outside Play Mode.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "sourceKind",
                    "Source Kind",
                    "Chooses how this individual ray resolves its directional/source state. This policy belongs to the ray, not its visual preset.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "cloudPolicy",
                    "Cloud Policy",
                    "Respect Clouds uses the validated CPU transmission query. Ignore Clouds permits authored divine rays through overcast.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "sourceGatePolicy",
                    "Source Gate",
                    "Require Active Source follows the resolved celestial gate. Ignore Source Gate is reserved for explicit authored overrides.");
            }
        }

        private void DrawLifecycle(WeatherLightRayAnchor anchor)
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
                WeatherInspectorGui.Property(
                    serializedObject,
                    "lifetimePolicy",
                    "Lifetime Policy",
                    "Timed uses Fade In, Hold, and Fade Out. Permanent remains active while registered. Externally Controlled follows its visibility flag.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "fadeInDurationSeconds",
                    "Fade In Duration",
                    "Seconds used to reach full authoritative intensity.");
                if (anchor.LifetimePolicy == WeatherLightRayLifetimePolicy.Timed)
                {
                    WeatherInspectorGui.Property(
                        serializedObject,
                        "holdDurationSeconds",
                        "Hold Duration",
                        "Seconds held at full intensity before fade-out.");
                }
                WeatherInspectorGui.Property(
                    serializedObject,
                    "fadeOutDurationSeconds",
                    "Fade Out Duration",
                    "Seconds used to leave the active state.");
                if (anchor.LifetimePolicy ==
                    WeatherLightRayLifetimePolicy.ExternallyControlled)
                {
                    WeatherInspectorGui.Property(
                        serializedObject,
                        "externallyControlledVisible",
                        "Externally Controlled Visible",
                        "Authoritative visibility input for this proof anchor.");
                }
            }
        }

        private void DrawShape(WeatherLightRayAnchor anchor)
        {
            if (!WeatherInspectorGui.Foldout(
                    ref showShape,
                    "Continuous Beam Cluster",
                    "Defines the variable-count parallel atmospheric structure inside one shared LightRay zone."))
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                WeatherInspectorGui.Property(
                    serializedObject,
                    "heightMetres",
                    "Beam Height",
                    "World-space distance from each ground contact to its upper fade.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "maximumVisualLeanDegrees",
                    "Maximum Visual Lean",
                    "Presentation clamp retained from the source-direction contract.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "areaDiameterMetres",
                    "Light Ray Area Diameter",
                    "The one authoritative area control. It derives footprint radius and beam count; the dense-overlap layout keeps the first and last visible beam edges on the exact world-X diameter endpoints while every adjacent pair overlaps.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "overridePresetBeamSpacing",
                    "Override Preset Beam Spacing",
                    "Enable only when this authored placement needs different granularity from its resolved preset.");
                SerializedProperty spacingOverride = serializedObject.FindProperty("overridePresetBeamSpacing");
                if (spacingOverride != null && spacingOverride.boolValue)
                {
                    WeatherInspectorGui.Property(
                        serializedObject,
                        "beamSpacingMetres",
                        "Local Beam Spacing",
                        "Target centre-to-centre spacing used only by this Anchor while the override is enabled.");
                }
                SerializedProperty areaProperty =
                    serializedObject.FindProperty("areaDiameterMetres");
                SerializedProperty spacingProperty =
                    serializedObject.FindProperty("beamSpacingMetres");
                WeatherLightRayPreset resolvedPreset = anchor.PresetOverride != null
                    ? anchor.PresetOverride
                    : anchor.RegisteredController != null
                        ? anchor.RegisteredController.DefaultPreset
                        : null;
                float resolvedSpacing = spacingOverride != null && spacingOverride.boolValue
                    ? spacingProperty != null ? spacingProperty.floatValue : anchor.BeamSpacingMetres
                    : resolvedPreset != null
                        ? resolvedPreset.BeamSpacingMetres
                        : anchor.BeamSpacingMetres;
                WeatherLightRayAreaLayout layout =
                    WeatherLightRayAreaLayout.Calculate(
                        areaProperty != null
                            ? areaProperty.floatValue
                            : anchor.AreaDiameterMetres,
                        resolvedSpacing);
                WeatherInspectorGui.ReadOnlyRow(
                    "Beam Spacing Source",
                    spacingOverride != null && spacingOverride.boolValue
                        ? "Local Override"
                        : resolvedPreset != null
                            ? resolvedPreset.DisplayName
                            : "Missing Preset");
                WeatherInspectorGui.ReadOnlyRow(
                    "Derived Footprint Radius",
                    layout.RadiusMetres,
                    "0.### m");
                WeatherInspectorGui.ReadOnlyRow(
                    "Derived Beam Count",
                    layout.BeamCount);
                WeatherInspectorGui.ReadOnlyRow(
                    "Resolved Centre Pitch",
                    layout.BeamPitchMetres,
                    "0.### m");
                WeatherInspectorGui.ReadOnlyRow(
                    "Average Atmospheric Beam Width",
                    layout.AverageAtmosphericBeamWidthMetres,
                    "0.### m");
                WeatherInspectorGui.ReadOnlyRow(
                    "Representative Adjacent Overlap",
                    layout.AverageAtmosphericOverlapMetres,
                    "0.### m");
                WeatherInspectorGui.ReadOnlyRow(
                    "Contact Layout Axis",
                    "World X");
            }
        }

        private void DrawVariation()
        {
            if (!WeatherInspectorGui.Foldout(
                    ref showEvolution,
                    "Instance Variation",
                    "Controls only this ray's deterministic identity and local strength."))
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                WeatherInspectorGui.Property(
                    serializedObject,
                    "variationSeed",
                    "Variation Seed",
                    "Deterministic seed for this zone's independent endpoint sequence. Shared evolution behaviour comes from this ray's resolved preset.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "localIntensityMultiplier",
                    "Local Intensity Multiplier",
                    "Scales this ray relative to its resolved preset without creating a second appearance authority.");
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
                    WeatherInspectorGui.ReadOnlyObject(
                        "Resolved Preset",
                        snapshot.ResolvedPreset);
                    WeatherInspectorGui.ReadOnlyRow(
                        "Preset Source",
                        snapshot.InheritsDefaultPreset
                            ? "Controller Default"
                            : "Per-Ray Override");
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
                        "Area Diameter / Radius",
                        $"{snapshot.Descriptor.AreaDiameterMetres:0.###} m / {snapshot.Descriptor.FootprintRadiusMetres:0.###} m");
                    WeatherInspectorGui.ReadOnlyRow(
                        "Resolved Beams / Centre Pitch",
                        $"{snapshot.Descriptor.BeamCount} / {snapshot.Descriptor.BeamPitchMetres:0.###} m");
                    WeatherInspectorGui.ReadOnlyRow(
                        "Average Atmospheric Beam Width",
                        WeatherLightRayAreaLayout.Calculate(
                            snapshot.Descriptor.AreaDiameterMetres,
                            snapshot.Descriptor.BeamSpacingMetres)
                            .AverageAtmosphericBeamWidthMetres,
                        "0.### m");
                    WeatherInspectorGui.ReadOnlyRow(
                        "Representative Adjacent Overlap",
                        WeatherLightRayAreaLayout.Calculate(
                            snapshot.Descriptor.AreaDiameterMetres,
                            snapshot.Descriptor.BeamSpacingMetres)
                            .AverageAtmosphericOverlapMetres,
                        "0.### m");
                    WeatherInspectorGui.ReadOnlyRow(
                        "Width Weight Range",
                        snapshot.Descriptor.BeamWidthRatioRange.ToString("F2"));
                    WeatherInspectorGui.ReadOnlyRow(
                        "Presentation Direction",
                        snapshot.RayDirectionWorld.ToString("F3"));
                    WeatherInspectorGui.ReadOnlyRow(
                        "Real Spot / Screen Complement / Softness",
                        $"{snapshot.Descriptor.SurfaceSpotLightIntensity:0.###} / " +
                        $"{snapshot.Descriptor.ScreenSpaceSurfaceIntensity:0.###} / " +
                        $"{snapshot.Descriptor.FootprintEdgeSoftness:0.###}");
                    WeatherInspectorGui.ReadOnlyRow(
                        "Vegetation Accent I / C / S",
                        $"{snapshot.Descriptor.VegetationAccentIntensity:0.###} / " +
                        $"{snapshot.Descriptor.VegetationAccentCoverage:0.###} / " +
                        $"{snapshot.Descriptor.VegetationAccentSoftness:0.###}");
                    Light surfaceSpot = anchor.RegisteredController
                        .GetSurfaceSpotLight(anchor.Handle);
                    WeatherInspectorGui.ReadOnlyObject(
                        "Runtime Surface Spot",
                        surfaceSpot);
                    if (anchor.RegisteredController.TryGetSurfaceSpotLightState(
                            anchor.Handle,
                            out float spotHeight,
                            out float innerRadius,
                            out float outerRadius,
                            out float appliedIntensity))
                    {
                        WeatherInspectorGui.ReadOnlyRow(
                            "Spot Height / Inner / Outer",
                            $"{spotHeight:0.###} m / {innerRadius:0.###} m / " +
                            $"{outerRadius:0.###} m");
                        WeatherInspectorGui.ReadOnlyRow(
                            "Applied Spot Intensity",
                            appliedIntensity);
                    }
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
