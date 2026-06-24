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
        private SerializedProperty responseProfile;
        private SerializedProperty pressureStrength;
        private SerializedProperty strengthMultiplier;
        private SerializedProperty geometryAmplitude;
        private SerializedProperty surfaceDetail;
        private SerializedProperty responseStiffness;
        private SerializedProperty wakeLength;
        private SerializedProperty unsteadiness;
        private SerializedProperty footprintPadding;


        private void OnEnable()
        {
            riverInteraction = serializedObject.FindProperty(
                "riverInteraction");
            participation = riverInteraction?.FindPropertyRelative(
                "participation");
            responseProfile = riverInteraction?.FindPropertyRelative(
                "responseProfile");
            pressureStrength = riverInteraction?.FindPropertyRelative(
                "pressureStrength");
            strengthMultiplier = riverInteraction?.FindPropertyRelative(
                "strengthMultiplier");
            geometryAmplitude = riverInteraction?.FindPropertyRelative(
                "geometryAmplitude");
            surfaceDetail = riverInteraction?.FindPropertyRelative(
                "surfaceDetail");
            responseStiffness = riverInteraction?.FindPropertyRelative(
                "responseStiffness");
            wakeLength = riverInteraction?.FindPropertyRelative(
                "wakeLength");
            unsteadiness = riverInteraction?.FindPropertyRelative(
                "unsteadiness");
            footprintPadding = riverInteraction?.FindPropertyRelative(
                "footprintPadding");
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

            EditorGUILayout.PropertyField(responseProfile);
            EditorGUILayout.PropertyField(footprintPadding);

            bool custom =
                responseProfile.enumValueIndex ==
                (int)GeneratedRiverResponseProfile.Custom;

            if (custom)
            {
                EditorGUILayout.PropertyField(
                    pressureStrength,
                    new GUIContent("Pressure Strength"));
                EditorGUILayout.PropertyField(
                    strengthMultiplier,
                    new GUIContent("Wake / General Strength"));
                EditorGUILayout.PropertyField(
                    geometryAmplitude,
                    new GUIContent("Non-Pressure Geometry"));
                EditorGUILayout.PropertyField(surfaceDetail);
                EditorGUILayout.PropertyField(responseStiffness);
                EditorGUILayout.PropertyField(wakeLength);
                EditorGUILayout.PropertyField(unsteadiness);
            }
            else if (!serializedObject.isEditingMultipleObjects)
            {
                GeneratedMass mass = (GeneratedMass)target;
                ResolvedGeneratedRiverInteraction resolved =
                    mass.RiverInteractionSettings.Resolve();

                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.Slider(
                        "Resolved Pressure Strength",
                        resolved.PressureStrength,
                        0f,
                        1f);
                    EditorGUILayout.FloatField(
                        "Resolved Strength Multiplier",
                        resolved.StrengthMultiplier);
                    EditorGUILayout.FloatField(
                        "Resolved Geometry Amplitude",
                        resolved.GeometryAmplitude);
                    EditorGUILayout.FloatField(
                        "Resolved Surface Detail",
                        resolved.SurfaceDetail);
                    EditorGUILayout.FloatField(
                        "Resolved Response Stiffness",
                        resolved.ResponseStiffness);
                    EditorGUILayout.FloatField(
                        "Resolved Wake Length",
                        resolved.WakeLength);
                    EditorGUILayout.FloatField(
                        "Resolved Unsteadiness",
                        resolved.Unsteadiness);
                }
            }

            EditorGUILayout.HelpBox(
                "No disturbance-emitter component is required for static generated geometry. Pressure Strength selects a normalized position inside the computed support/flow range; the remaining controls retain wake and non-pressure interaction authorship.",
                MessageType.None);

            DrawRuntimeDiagnostics();
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
                EditorGUILayout.Slider(
                    "Pressure Strength",
                    diagnostics.PressureStrength,
                    0f,
                    1f);
                EditorGUILayout.Vector2Field(
                    "Feasible Pressure Range",
                    new Vector2(
                        diagnostics.PressureMinimumHeight,
                        diagnostics.PressureMaximumHeight));
                EditorGUILayout.FloatField(
                    "Effective Amplitude",
                    diagnostics.EffectiveAmplitude);
                EditorGUILayout.FloatField(
                    "Effective Wake Strength",
                    diagnostics.EffectiveWakeStrength);
                EditorGUILayout.FloatField(
                    "Maximum Allowed",
                    diagnostics.MaximumAllowedAmplitude);
                EditorGUILayout.Toggle(
                    "Height Clamp Reached",
                    diagnostics.HeightClampReached);
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
