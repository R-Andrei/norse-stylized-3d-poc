using ProgrammaticStylized3D.Geometry;
using ProgrammaticStylized3D.Rivers;
using UnityEditor;
using UnityEngine;

namespace ProgrammaticStylized3D.Geometry.Masses.Editor
{
    [CustomEditor(typeof(GeneratedMass))]
    [CanEditMultipleObjects]
    public sealed class GeneratedMassEditor : UnityEditor.Editor
    {
        private SerializedProperty riverInteraction;
        private SerializedProperty participation;
        private SerializedProperty staticPressureMode;
        private SerializedProperty staticPressureStrength;
        private SerializedProperty staticPressureContactSharpness;
        private SerializedProperty staticPressureWaveResponse;
        private SerializedProperty obstructionWakeMode;
        private SerializedProperty obstructionWakeStrength;
        private SerializedProperty obstructionWakeReach;
        private SerializedProperty obstructionWakeSpread;

        private void OnEnable()
        {
            riverInteraction = serializedObject.FindProperty(
                "riverInteraction");
            participation = riverInteraction?.FindPropertyRelative(
                "participation");
            staticPressureMode = riverInteraction?.FindPropertyRelative(
                "staticPressureMode");
            staticPressureStrength = riverInteraction?.FindPropertyRelative(
                "staticPressureStrength");
            staticPressureContactSharpness =
                riverInteraction?.FindPropertyRelative(
                    "staticPressureContactSharpness");
            staticPressureWaveResponse =
                riverInteraction?.FindPropertyRelative(
                    "staticPressureWaveResponse");
            obstructionWakeMode = riverInteraction?.FindPropertyRelative(
                "obstructionWakeMode");
            obstructionWakeStrength = riverInteraction?.FindPropertyRelative(
                "obstructionWakeStrength");
            obstructionWakeReach = riverInteraction?.FindPropertyRelative(
                "obstructionWakeReach");
            obstructionWakeSpread = riverInteraction?.FindPropertyRelative(
                "obstructionWakeSpread");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawPropertiesExcluding(
                serializedObject,
                "m_Script",
                "riverInteraction");

            DrawRiverInteraction();

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField(
                "Variant Controls",
                EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("New Shape"))
            {
                ApplyToTargets(
                    "New Generated Mass Shape",
                    mass => mass.CreateNewShape());
            }

            if (GUILayout.Button("New Surface"))
            {
                ApplyToTargets(
                    "New Generated Mass Surface",
                    mass => mass.CreateNewSurface());
            }

            if (GUILayout.Button("New Variant"))
            {
                ApplyToTargets(
                    "New Generated Mass Variant",
                    mass => mass.CreateNewVariant());
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Regenerate"))
            {
                ApplyToTargets(
                    "Regenerate Generated Mass",
                    mass => mass.Regenerate());
            }

            if (GUILayout.Button("Reset to Archetype"))
            {
                ApplyToTargets(
                    "Reset Generated Mass Recipe",
                    mass => mass.ResetRecipeToArchetype());
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.HelpBox(
                "Shape Seed changes proportions, major cuts and silhouette. " +
                "Surface Seed changes surface triangulation, subtle facet relief " +
                "and vertex-colour variation.",
                MessageType.Info);
        }

        private void DrawRiverInteraction()
        {
            if (riverInteraction == null || participation == null)
            {
                return;
            }

            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField(
                "River Interaction",
                EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(participation);

            bool disabled =
                participation.enumValueIndex ==
                (int)GeneratedRiverInteractionParticipation.Disabled;

            if (disabled)
            {
                EditorGUILayout.HelpBox(
                    "This generated object is ignored by automatic static river-obstruction discovery.",
                    MessageType.Info);
                DrawRuntimeDiagnostics();
                return;
            }

            DrawStaticPressureControls();
            DrawObstructionWakeControls();

            EditorGUILayout.HelpBox(
                "Inherit uses the defaults of the river that detects this object. Custom replaces only the selected feature's values; it does not multiply unrelated interaction systems.",
                MessageType.None);

            DrawRuntimeDiagnostics();
        }

        private void DrawStaticPressureControls()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(
                "Static Pressure",
                EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(
                staticPressureMode,
                new GUIContent("Mode"));

            if (staticPressureMode == null)
            {
                return;
            }

            GeneratedRiverFeatureMode mode =
                (GeneratedRiverFeatureMode)staticPressureMode.enumValueIndex;

            if (mode == GeneratedRiverFeatureMode.Custom)
            {
                EditorGUILayout.PropertyField(
                    staticPressureStrength,
                    new GUIContent("Strength"));
                EditorGUILayout.PropertyField(
                    staticPressureContactSharpness,
                    new GUIContent("Contact Sharpness"));
                EditorGUILayout.PropertyField(
                    staticPressureWaveResponse,
                    new GUIContent("Wave Response"));
            }
            else if (mode == GeneratedRiverFeatureMode.Inherit)
            {
                EditorGUILayout.HelpBox(
                    "Uses the detected river's Static Pressure defaults.",
                    MessageType.None);
            }
        }

        private void DrawObstructionWakeControls()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(
                "Obstruction Wake",
                EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(
                obstructionWakeMode,
                new GUIContent("Mode"));

            if (obstructionWakeMode == null)
            {
                return;
            }

            GeneratedRiverFeatureMode mode =
                (GeneratedRiverFeatureMode)obstructionWakeMode.enumValueIndex;

            if (mode == GeneratedRiverFeatureMode.Custom)
            {
                EditorGUILayout.PropertyField(
                    obstructionWakeStrength,
                    new GUIContent("Strength"));
                EditorGUILayout.PropertyField(
                    obstructionWakeReach,
                    new GUIContent("Reach"));
                EditorGUILayout.PropertyField(
                    obstructionWakeSpread,
                    new GUIContent("Spread"));
            }
            else if (mode == GeneratedRiverFeatureMode.Inherit)
            {
                EditorGUILayout.HelpBox(
                    "Uses the detected river's Obstruction Wake defaults.",
                    MessageType.None);
            }
        }

        private void DrawRuntimeDiagnostics()
        {
            if (serializedObject.isEditingMultipleObjects ||
                !Application.isPlaying)
            {
                return;
            }

            GeneratedMass mass = (GeneratedMass)target;
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(
                "Runtime River Contact",
                EditorStyles.boldLabel);

            if (!StylizedRiverDisturbanceRuntime.TryGetGeneratedSourceDiagnostics(
                    mass,
                    out GeneratedRiverDisturbanceDiagnostics diagnostics))
            {
                EditorGUILayout.HelpBox(
                    "No active river contact is registered for this generated object.",
                    MessageType.None);
                return;
            }

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.ObjectField(
                    "Detected River",
                    diagnostics.River,
                    typeof(StylizedRiver),
                    true);
                EditorGUILayout.Toggle("Contact Active", diagnostics.Active);
                EditorGUILayout.FloatField(
                    "Across Width",
                    diagnostics.AcrossWidth);
                EditorGUILayout.FloatField(
                    "Along Length",
                    diagnostics.AlongLength);
                EditorGUILayout.FloatField(
                    "Local River Width",
                    diagnostics.LocalRiverWidth);
                EditorGUILayout.Slider(
                    "Blockage",
                    diagnostics.BlockageRatio,
                    0f,
                    1f);
                EditorGUILayout.FloatField(
                    "Effective Padding",
                    diagnostics.EffectivePadding);
                EditorGUILayout.FloatField(
                    "Representative Support",
                    diagnostics.RepresentativeSupportHeight);
                EditorGUILayout.FloatField(
                    "Wave Allowance",
                    diagnostics.WaveAllowance);
                EditorGUILayout.Toggle(
                    "Static Pressure Enabled",
                    diagnostics.StaticPressureEnabled);
                if (diagnostics.StaticPressureEnabled)
                {
                    EditorGUILayout.Slider(
                        "Pressure Strength",
                        diagnostics.PressureStrength,
                        0f,
                        1f);
                    EditorGUILayout.FloatField(
                        "Contact Sharpness",
                        diagnostics.ContactSharpness);
                    EditorGUILayout.FloatField(
                        "Wave Response",
                        diagnostics.WaveResponse);
                    EditorGUILayout.Vector2Field(
                        "Feasible Pressure Range",
                        new Vector2(
                            diagnostics.PressureMinimumHeight,
                            diagnostics.PressureMaximumHeight));
                    EditorGUILayout.FloatField(
                        "Resolved Pressure Height",
                        diagnostics.EffectiveAmplitude);
                    EditorGUILayout.Toggle(
                        "Support Clamp Reached",
                        diagnostics.HeightClampReached);
                }

                EditorGUILayout.Toggle(
                    "Obstruction Wake Enabled",
                    diagnostics.ObstructionWakeEnabled);
                if (diagnostics.ObstructionWakeEnabled)
                {
                    EditorGUILayout.FloatField(
                        "Resolved Wake Strength",
                        diagnostics.EffectiveWakeStrength);
                    EditorGUILayout.FloatField(
                        "Wake Reach",
                        diagnostics.ObstructionWakeReach);
                    EditorGUILayout.FloatField(
                        "Wake Spread",
                        diagnostics.ObstructionWakeSpread);
                }
            }

            EditorGUILayout.HelpBox(
                diagnostics.Status,
                diagnostics.Active
                    ? MessageType.Info
                    : MessageType.None);
        }

        private void ApplyToTargets(
            string undoName,
            ActionForMass action)
        {
            for (int i = 0; i < targets.Length; i++)
            {
                GeneratedMass mass = targets[i] as GeneratedMass;

                if (mass == null)
                {
                    continue;
                }

                Undo.RecordObject(mass, undoName);
                action(mass);
                EditorUtility.SetDirty(mass);
            }

            serializedObject.Update();
            Repaint();
        }

        private delegate void ActionForMass(GeneratedMass mass);
    }
}
