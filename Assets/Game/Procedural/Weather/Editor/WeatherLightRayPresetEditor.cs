#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ProgrammaticStylized3D.Weather.Editor
{
    [CustomEditor(typeof(WeatherLightRayPreset))]
    public sealed class WeatherLightRayPresetEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            WeatherInspectorGui.DrawScriptReference(serializedObject);
            EditorGUILayout.HelpBox(
                "This preset defines the complete visual presentation for any " +
                "LightRay that resolves to it. Rays may inherit the Controller " +
                "Default Preset or use a per-ray Preset Override. Source, cloud, " +
                "lifecycle, quest, and Weather eligibility are not preset-owned.",
                MessageType.Info);

            DrawSection("Identity",
                ("displayName", "Display Name", "Human-readable preset name used by LightRay telemetry and authoring surfaces."));

            DrawSection("Shared Atmospheric Presentation",
                ("colourMultiplier", "Colour Multiplier", "Atmospheric beam colour multiplier for rays using this preset."),
                ("warmthContribution", "Warmth Contribution", "Blends Sun-bound source presentation toward the LightRay warm presentation colour. Source ownership remains per ray."),
                ("atmosphericIntensity", "Atmospheric Intensity", "Base atmospheric beam strength before the ray's local intensity multiplier and lifecycle gates."),
                ("softeningStrength", "Softening Strength", "Gap-preserving screen-space atmospheric softening strength."),
                ("cameraIntersectionFade", "Camera Intersection Fade", "Controls fading where the ray volume approaches the camera."),
                ("screenSpaceSurfaceIntensity", "Screen-Space Surface Intensity", "Optional screen-space surface complement. Non-zero values require an isolated final-composite presentation group."));

            DrawSection("Beam Composition",
                ("beamSpacingMetres", "Beam Spacing", "Default centre-to-centre beam spacing. Individual rays may explicitly override spacing."),
                ("beamWidthRatioRange", "Beam Width Ratio Range", "Seeded beam-width weight range inside the resolved area."),
                ("beamIntensityVariation", "Beam Intensity Variation", "Seeded per-beam intensity hierarchy variation."),
                ("beamEdgeSoftness", "Beam Edge Softness", "Base beam-edge feathering."),
                ("beamSoftnessVariation", "Beam Softness Variation", "Seeded asymmetry and softness variation between beams."),
                ("upperFade", "Upper Fade", "Normalized upper fade shaping."),
                ("groundFade", "Ground Fade", "Normalized ground-contact fade shaping."),
                ("contactPlaneOpacity", "Contact Plane Opacity", "Contact-plane contribution encoded with the per-ray zone record."));

            DrawSection("Surface Response",
                ("surfaceSpotLightIntensity", "Surface Spot Light Intensity", "Physical Spot proxy intensity scale for this preset."),
                ("footprintEdgeSoftness", "Footprint Edge Softness", "Surface Spot and screen-space footprint transition softness."),
                ("accentLineIntensity", "Vegetation Accent Intensity", "Strength of the LightRay-specific vegetation accent-line response."),
                ("vegetationAccentCoverage", "Vegetation Accent Coverage", "Fraction of eligible vegetation cards/blades participating in this ray's accent response."),
                ("vegetationAccentSoftness", "Vegetation Accent Softness", "Softness of this ray's indexed vegetation accent response."));

            DrawSection("Evolution",
                ("evolutionPreset", "Evolution Preset", "Seeded beam-layout evolution mode for rays using this preset."),
                ("evolutionStrength", "Evolution Strength", "Custom evolution strength when the selected mode uses the authored value."),
                ("evolutionSpeed", "Evolution Speed", "Custom evolution speed when the selected mode uses the authored value."));

            DrawSection("Default Spawn Geometry",
                ("defaultHeightMetres", "Default Height", "Default procedural ray height when the request does not override height."),
                ("defaultMaximumVisualLeanDegrees", "Default Maximum Visual Lean", "Default procedural presentation lean clamp when the request does not override it."),
                ("defaultAreaDiameterMetres", "Default Area Diameter", "Default area diameter available to request owners that choose to use preset spawn geometry."));

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSection(
            string title,
            params (string propertyName, string label, string tooltip)[] fields)
        {
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                for (int index = 0; index < fields.Length; index++)
                {
                    var field = fields[index];
                    SerializedProperty property = serializedObject.FindProperty(
                        field.propertyName);
                    if (property == null)
                    {
                        EditorGUILayout.HelpBox(
                            $"Missing serialized preset property: {field.propertyName}",
                            MessageType.Error);
                        continue;
                    }

                    EditorGUILayout.PropertyField(
                        property,
                        new GUIContent(field.label, field.tooltip),
                        true);
                }
            }
        }
    }
}
#endif
