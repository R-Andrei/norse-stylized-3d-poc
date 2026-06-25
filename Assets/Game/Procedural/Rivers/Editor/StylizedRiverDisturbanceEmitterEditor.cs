using UnityEditor;
using UnityEngine;

namespace ProgrammaticStylized3D.Rivers.Editor
{
    [CustomEditor(typeof(StylizedRiverDisturbanceEmitter))]
    [CanEditMultipleObjects]
    public sealed class StylizedRiverDisturbanceEmitterEditor :
        UnityEditor.Editor
    {
        private SerializedProperty explicitRiver;
        private SerializedProperty autoDetectRiver;
        private SerializedProperty verticalContactTolerance;
        private SerializedProperty sourceMobility;
        private SerializedProperty useSeparateFootprintDimensions;
        private SerializedProperty linkedFootprintRadius;
        private SerializedProperty acrossFlowHalfWidth;
        private SerializedProperty alongFlowHalfLength;
        private SerializedProperty strength;
        private SerializedProperty geometryContribution;
        private SerializedProperty normalContribution;
        private SerializedProperty stationaryObstruction;
        private SerializedProperty emitEntryImpact;
        private SerializedProperty entryImpact;
        private SerializedProperty emitExitImpact;
        private SerializedProperty exitImpact;
        private SerializedProperty sourceUpdateInterval;

        private void OnEnable()
        {
            explicitRiver = serializedObject.FindProperty("explicitRiver");
            autoDetectRiver = serializedObject.FindProperty("autoDetectRiver");
            verticalContactTolerance = serializedObject.FindProperty(
                "verticalContactTolerance");
            sourceMobility = serializedObject.FindProperty("sourceMobility");
            useSeparateFootprintDimensions = serializedObject.FindProperty(
                "useSeparateFootprintDimensions");
            linkedFootprintRadius = serializedObject.FindProperty(
                "linkedFootprintRadius");
            acrossFlowHalfWidth = serializedObject.FindProperty(
                "acrossFlowHalfWidth");
            alongFlowHalfLength = serializedObject.FindProperty(
                "alongFlowHalfLength");
            strength = serializedObject.FindProperty("strength");
            geometryContribution = serializedObject.FindProperty(
                "geometryContribution");
            normalContribution = serializedObject.FindProperty(
                "normalContribution");
            stationaryObstruction = serializedObject.FindProperty(
                "stationaryObstruction");
            emitEntryImpact = serializedObject.FindProperty(
                "emitEntryImpact");
            entryImpact = serializedObject.FindProperty(
                "entryImpact");
            emitExitImpact = serializedObject.FindProperty(
                "emitExitImpact");
            exitImpact = serializedObject.FindProperty(
                "exitImpact");
            sourceUpdateInterval = serializedObject.FindProperty(
                "sourceUpdateInterval");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            bool legacyStaticEmitter =
                sourceMobility != null &&
                !sourceMobility.hasMultipleDifferentValues &&
                sourceMobility.enumValueIndex == 0;

            if (legacyStaticEmitter)
            {
                EditorGUILayout.HelpBox(
                    "This is a legacy Static River Disturbance Emitter. It is " +
                    "inert because stationary geometry is now handled through " +
                    "the generated-geometry registry. Remove this component.",
                    MessageType.Warning);
                serializedObject.ApplyModifiedProperties();
                return;
            }

            EditorGUILayout.LabelField("River Contact", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(
                explicitRiver,
                new GUIContent(
                    "Explicit River",
                    "Optional fixed river target. When assigned, this emitter submits only to that river and does not choose another river automatically."));
            EditorGUILayout.PropertyField(
                autoDetectRiver,
                new GUIContent(
                    "Auto Detect River",
                    "When Explicit River is empty, searches enabled rivers and uses the one whose water footprint contains this emitter within Vertical Contact Tolerance."));
            EditorGUILayout.PropertyField(
                verticalContactTolerance,
                new GUIContent(
                    "Vertical Contact Tolerance",
                    "Maximum vertical separation, in metres, between the emitter pivot and the detected river surface. Larger values accept objects farther above or below the water; this affects contact detection only."));

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Source Footprint", EditorStyles.boldLabel);
            DrawManualFootprint();

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField(
                "Continuous Wake",
                EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(
                strength,
                new GUIContent(
                    "Strength",
                    "Local continuous-Wake multiplier applied before the river's canonical Wake Strength. Zero disables this emitter's continuous Wake; it does not affect Entry or Exit Impact Ripples."));
            EditorGUILayout.PropertyField(
                geometryContribution,
                new GUIContent(
                    "Geometry Contribution",
                    "Scales this emitter's continuous Wake geometry contribution. Zero prevents Wake height from this source; Impact Ripple event profiles remain independent."));
            EditorGUILayout.PropertyField(
                normalContribution,
                new GUIContent(
                    "Normal Contribution",
                    "Scales this emitter's continuous Wake normal/intensity contribution used by lighting and refraction. Impact Ripple event profiles remain independent."));
            EditorGUILayout.PropertyField(
                stationaryObstruction,
                new GUIContent(
                    "Stationary Obstruction",
                    "When enabled, a slow or stopped emitter blends toward the runtime's stationary-obstruction Wake pattern instead of behaving as a fully moving trail. Disable it when Wake should depend only on movement."));

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField(
                "Impact Ripples",
                EditorStyles.boldLabel);
            DrawImpactEvent(
                emitEntryImpact,
                entryImpact,
                "Entry Impact",
                "Emits once after the runtime observes an outside-to-inside river transition. Starting or enabling the component while already inside water intentionally does not count as an entry.",
                "Independent Entry profile: starting radius, signed impulse, immediate elevation, shape, sharpness, and geometry/normal contributions. These values do not modify Continuous Wake.");
            DrawImpactEvent(
                emitExitImpact,
                exitImpact,
                "Exit / Suction Impact",
                "Emits once after the runtime observes an inside-to-outside river transition. Use a negative Signed Impulse for suction-like behavior.",
                "Independent Exit / Suction profile: starting radius, signed impulse, immediate elevation, shape, sharpness, and geometry/normal contributions. These values do not modify Continuous Wake.");

            using (new EditorGUI.DisabledScope(
                !Application.isPlaying ||
                targets.Length != 1))
            {
                if (GUILayout.Button(new GUIContent(
                        "Emit Entry Impact Now",
                        "In Play Mode, manually emits this emitter's configured Entry Impact at its current position. The emitter must currently resolve to a river; this does not simulate an outside-to-inside transition.")) &&
                    target is StylizedRiverDisturbanceEmitter emitter)
                {
                    serializedObject.ApplyModifiedProperties();
                    emitter.EmitImpactNow();
                    serializedObject.Update();
                }
            }

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField(
                "Runtime Sampling",
                EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(
                sourceUpdateInterval,
                new GUIContent(
                    "Source Update Interval",
                    "Seconds between CPU contact and movement samples. Lower values react sooner but cost more CPU work; swept Wake submission bridges the path between samples."));

            serializedObject.ApplyModifiedProperties();
        }

        private static void DrawImpactEvent(
            SerializedProperty enabledProperty,
            SerializedProperty settingsProperty,
            string label,
            string enabledTooltip,
            string settingsTooltip)
        {
            EditorGUILayout.PropertyField(
                enabledProperty,
                new GUIContent(label, enabledTooltip));

            bool disabled =
                enabledProperty != null &&
                !enabledProperty.hasMultipleDifferentValues &&
                !enabledProperty.boolValue;

            using (new EditorGUI.DisabledScope(disabled))
            using (new EditorGUI.IndentLevelScope())
            {
                if (settingsProperty != null)
                {
                    EditorGUILayout.PropertyField(
                        settingsProperty,
                        new GUIContent("Settings", settingsTooltip),
                        true);
                }
            }
        }

        private void DrawManualFootprint()
        {
            EditorGUILayout.PropertyField(
                useSeparateFootprintDimensions,
                new GUIContent(
                    "Separate Footprint Dimensions",
                    "Uses independent half-width and half-length values for the continuous dynamic Wake footprint. Disable this to use one linked half-size. Impact Ripple radius is configured separately in each event."));

            if (useSeparateFootprintDimensions.boolValue)
            {
                EditorGUILayout.PropertyField(
                    acrossFlowHalfWidth,
                    new GUIContent(
                        "Across Flow Half Width",
                        "Continuous Wake footprint half-width measured across the local river, in metres. This does not set Impact Ripple radius."));
                EditorGUILayout.PropertyField(
                    alongFlowHalfLength,
                    new GUIContent(
                        "Along Flow Half Length",
                        "Continuous Wake footprint half-length measured along the local river, in metres. This does not set Impact Ripple radius."));
            }
            else
            {
                EditorGUILayout.PropertyField(
                    linkedFootprintRadius,
                    new GUIContent(
                        "Linked Footprint Radius",
                        "Continuous Wake footprint half-size in metres, used across and along the river. This does not set Entry or Exit Impact radius."));
            }
        }
    }
}
