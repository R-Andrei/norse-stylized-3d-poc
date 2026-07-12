using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

namespace ProgrammaticStylized3D.Rivers.Editor
{
    internal sealed partial class StylizedRiverEditor
    {
        private void DrawRuntimeDisturbances()
        {
            EditorGUILayout.HelpBox(
                "Pressure, Wake, and Impact Ripples share one runtime " +
                "disturbance system but retain separate authoring groups.",
                MessageType.None);

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
            if (missingProperties.Length > 0)
            {
                EditorGUILayout.HelpBox(
                    "The StylizedRiver Inspector and runtime component do not match. Missing serialized properties: " +
                    missingProperties +
                    ". No missing property will be drawn.",
                    MessageType.Error);
            }

            DrawNestedSection(
                InspectorSection.DisturbanceMasterPreset,
                "Master & Preset",
                () =>
                {
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
                });

            bool controlsDisabled =
                enabledProperty != null &&
                !enabledProperty.hasMultipleDifferentValues &&
                !enabledProperty.boolValue;

            using (new EditorGUI.DisabledScope(controlsDisabled))
            {
                DrawNestedSection(
                    InspectorSection.DisturbancePressure,
                    "Pressure Response",
                    () =>
                    {
                        EditorGUI.BeginChangeCheck();
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
                        MarkDisturbancePresetCustomIfChanged(presetProperty);
                    });

                DrawNestedSection(
                    InspectorSection.DisturbanceWake,
                    "Wake Response",
                    () =>
                    {
                        EditorGUI.BeginChangeCheck();
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
                        MarkDisturbancePresetCustomIfChanged(presetProperty);
                    });

                DrawNestedSection(
                    InspectorSection.DisturbanceRipples,
                    "Impact Ripples",
                    () =>
                    {
                        EditorGUI.BeginChangeCheck();
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
                        MarkDisturbancePresetCustomIfChanged(presetProperty);
                    });
            }
        }

        private void MarkDisturbancePresetCustomIfChanged(
            SerializedProperty presetProperty)
        {
            if (EditorGUI.EndChangeCheck() && presetProperty != null)
            {
                presetProperty.enumValueIndex =
                    (int)StylizedRiverDisturbancePreset.Custom;
            }
        }

        private void ApplyImpactTestProperties()
        {
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }
    }
}
