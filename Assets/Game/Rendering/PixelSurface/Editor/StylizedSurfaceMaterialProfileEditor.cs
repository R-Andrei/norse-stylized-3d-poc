using System;
using UnityEditor;
using UnityEngine;

namespace ProgrammaticStylized3D.Rendering.PixelSurface.Editor
{
    [CustomEditor(typeof(StylizedSurfaceMaterialProfile))]
    public sealed class StylizedSurfaceMaterialProfileEditor : UnityEditor.Editor
    {
        private enum PreviewOrientation
        {
            Horizontal,
            Vertical
        }

        private Texture2D previewTexture;
        private int previewSignature = int.MinValue;
        private PreviewOrientation previewOrientation;

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            EditorGUILayout.HelpBox(
                "The Inspector Preview pane is optional material-diagnostic assistance only. Production-camera scene rendering is the authoritative visual acceptance test.",
                MessageType.Info);

            DrawSection("Identity", "displayName");
            DrawSection("Payload", "payloadMode");
            SerializedProperty payloadMode =
                serializedObject.FindProperty("payloadMode");
            bool authoredColorMode =
                payloadMode != null &&
                payloadMode.enumValueIndex ==
                    (int)StylizedSurfaceMaterialPayloadMode.AuthoredColor;
            if (authoredColorMode)
            {
                DrawSection(
                    "Authored Color",
                    "authoredColorStrength",
                    "authoredColorTint",
                    "authoredColorTintStrength",
                    "authoredColorLightingStrength",
                    "authoredRoughnessStrength");
            }

            DrawSection(
                "Palette",
                "baseColor",
                "darkColor",
                "lightColor",
                "cavityColor");
            DrawSection(
                "Broad Response",
                "macroContrast",
                "legacyPixelCellInfluence");
            if (!authoredColorMode)
            {
                DrawSection(
                    "Palette Detail Variation",
                    "detailValueStrength");
            }
            DrawSection(
                "Structural Detail",
                "detailEnabled",
                "detailLibrary");

            SerializedProperty detailEnabled =
                serializedObject.FindProperty("detailEnabled");
            bool detailIsEnabled =
                detailEnabled != null && detailEnabled.boolValue;
            using (new EditorGUI.DisabledScope(!detailIsEnabled))
            {
                DrawDetailEntryPopup();
                DrawProperties(
                    "detailWorldScale",
                    "detailNormalStrength",
                    "detailCavityStrength",
                    "detailCavityBias");
                if (!authoredColorMode)
                {
                    DrawProperties("detailFormHighlightStrength");
                }
            }

            if (detailIsEnabled)
            {
                DrawDetailStatus();
            }

            DrawSection(
                "Dry Finish",
                "drySmoothness",
                "drySpecularStrength");
            if (!authoredColorMode)
            {
                DrawSection(
                    "Palette Detail Finish Variation",
                    "finishVariationStrength");
            }

            if (serializedObject.ApplyModifiedProperties())
            {
                StylizedSurfaceMaterialProfile profile =
                    (StylizedSurfaceMaterialProfile)target;
                EditorUtility.SetDirty(profile);
                profile.NotifyEditorChanged();
                previewSignature = int.MinValue;
                SceneView.RepaintAll();
            }
        }

        public override bool HasPreviewGUI()
        {
            return ResolveSourceTexture() != null;
        }

        public override void OnPreviewSettings()
        {
            PreviewOrientation selected =
                (PreviewOrientation)GUILayout.Toolbar(
                    (int)previewOrientation,
                    new[] { "Horizontal", "Vertical" },
                    EditorStyles.toolbarButton,
                    GUILayout.Width(150f));
            if (selected != previewOrientation)
            {
                previewOrientation = selected;
                previewSignature = int.MinValue;
            }
        }

        public override void OnPreviewGUI(Rect rect, GUIStyle background)
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            EnsurePreviewTexture();
            if (previewTexture != null)
            {
                EditorGUI.DrawPreviewTexture(
                    rect,
                    previewTexture,
                    null,
                    ScaleMode.ScaleToFit);
            }
        }

        private void OnDisable()
        {
            if (previewTexture != null)
            {
                DestroyImmediate(previewTexture);
                previewTexture = null;
            }
        }

        private void DrawSection(string label, params string[] properties)
        {
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            DrawProperties(properties);
            EditorGUILayout.Space(4f);
        }

        private void DrawProperties(params string[] propertyNames)
        {
            for (int index = 0; index < propertyNames.Length; index++)
            {
                SerializedProperty property =
                    serializedObject.FindProperty(propertyNames[index]);
                if (property != null)
                {
                    EditorGUILayout.PropertyField(property);
                }
            }
        }


        private void DrawDetailStatus()
        {
            SerializedProperty libraryProperty =
                serializedObject.FindProperty("detailLibrary");
            SerializedProperty idProperty =
                serializedObject.FindProperty("detailEntryId");
            StylizedSurfaceDetailLibrary library =
                libraryProperty != null
                    ? libraryProperty.objectReferenceValue as
                        StylizedSurfaceDetailLibrary
                    : null;

            if (library == null)
            {
                EditorGUILayout.HelpBox(
                    "Detail is enabled but no detail library is assigned.",
                    MessageType.Warning);
                return;
            }

            string entryId = idProperty != null
                ? idProperty.stringValue
                : string.Empty;
            bool entryExists = false;
            for (int index = 0; index < library.Entries.Count; index++)
            {
                StylizedSurfaceDetailLibrary.Entry entry =
                    library.Entries[index];
                if (entry != null &&
                    string.Equals(
                        entry.StableId,
                        entryId,
                        StringComparison.Ordinal))
                {
                    entryExists = true;
                    break;
                }
            }

            if (!entryExists)
            {
                EditorGUILayout.HelpBox(
                    string.IsNullOrWhiteSpace(entryId)
                        ? "Select a packed detail entry."
                        : $"The detail entry '{entryId}' is missing from the assigned library.",
                    MessageType.Warning);
                return;
            }

            if (StylizedSurfaceDetailLibraryBuilder.NeedsRebuild(library))
            {
                EditorGUILayout.HelpBox(
                    "The assigned detail library is missing its generated array or is stale.",
                    MessageType.Info);
                if (GUILayout.Button("Rebuild Assigned Detail Library"))
                {
                    StylizedSurfaceDetailLibraryBuilder.Rebuild(library);
                }
            }
        }

        private void DrawDetailEntryPopup()
        {
            SerializedProperty libraryProperty =
                serializedObject.FindProperty("detailLibrary");
            SerializedProperty idProperty =
                serializedObject.FindProperty("detailEntryId");
            StylizedSurfaceDetailLibrary library =
                libraryProperty != null
                    ? libraryProperty.objectReferenceValue as
                        StylizedSurfaceDetailLibrary
                    : null;

            if (idProperty == null)
            {
                return;
            }

            if (library == null || library.Entries.Count == 0)
            {
                EditorGUILayout.PropertyField(
                    idProperty,
                    new GUIContent("Detail Entry ID"));
                return;
            }

            GUIContent[] options =
                new GUIContent[library.Entries.Count + 1];
            options[0] = new GUIContent("None");
            int currentIndex = 0;
            for (int index = 0; index < library.Entries.Count; index++)
            {
                StylizedSurfaceDetailLibrary.Entry entry =
                    library.Entries[index];
                options[index + 1] = new GUIContent(
                    entry != null ? entry.DisplayName : "Missing Entry",
                    entry != null ? entry.StableId : string.Empty);
                if (entry != null &&
                    string.Equals(
                        entry.StableId,
                        idProperty.stringValue,
                        StringComparison.Ordinal))
                {
                    currentIndex = index + 1;
                }
            }

            int selected = EditorGUILayout.Popup(
                new GUIContent("Detail Entry"),
                currentIndex,
                options);
            idProperty.stringValue = selected <= 0
                ? string.Empty
                : library.Entries[selected - 1].StableId;
        }

        private Texture2D ResolveSourceTexture()
        {
            StylizedSurfaceMaterialProfile profile =
                target as StylizedSurfaceMaterialProfile;
            StylizedSurfaceDetailLibrary library =
                profile != null ? profile.DetailLibrary : null;
            if (profile == null ||
                !profile.DetailEnabled ||
                library == null)
            {
                return null;
            }

            for (int index = 0; index < library.Entries.Count; index++)
            {
                StylizedSurfaceDetailLibrary.Entry entry =
                    library.Entries[index];
                if (entry != null &&
                    string.Equals(
                        entry.StableId,
                        profile.DetailEntryId,
                        StringComparison.Ordinal))
                {
                    return profile.UsesAuthoredColor
                        ? entry.AuthoredBaseColor
                        : entry.SourceTexture;
                }
            }

            return null;
        }

        private void EnsurePreviewTexture()
        {
            StylizedSurfaceMaterialProfile profile =
                target as StylizedSurfaceMaterialProfile;
            Texture2D source = ResolveSourceTexture();
            if (profile == null || source == null || !source.isReadable)
            {
                return;
            }

            int signature = ComputePreviewSignature(profile, source);
            if (previewTexture != null && signature == previewSignature)
            {
                return;
            }

            const int size = 192;
            if (previewTexture == null)
            {
                previewTexture = new Texture2D(
                    size,
                    size,
                    TextureFormat.RGBA32,
                    false,
                    false)
                {
                    name = "Stylized Surface Material Preview",
                    hideFlags = HideFlags.HideAndDontSave,
                    wrapMode = TextureWrapMode.Clamp,
                    filterMode = FilterMode.Bilinear
                };
            }

            Color[] pixels = new Color[size * size];
            Vector3 lightDirection =
                new Vector3(-0.42f, 0.78f, 0.46f).normalized;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = (x + 0.5f) / size;
                    float v = (y + 0.5f) / size;
                    Color packed = source.GetPixelBilinear(u, v);
                    if (profile.UsesAuthoredColor)
                    {
                        Color tinted = Color.Lerp(
                            packed,
                            packed * profile.AuthoredColorTint,
                            profile.AuthoredColorTintStrength);
                        float authoredLighting = Mathf.Lerp(
                            1f,
                            0.92f,
                            profile.AuthoredColorLightingStrength);
                        pixels[y * size + x] = Color.Lerp(
                            profile.BaseColor,
                            tinted * authoredLighting,
                            profile.AuthoredColorStrength);
                        pixels[y * size + x].a = 1f;
                        continue;
                    }

                    Vector2 slope = new Vector2(
                        packed.r * 2f - 1f,
                        packed.g * 2f - 1f) *
                        profile.DetailNormalStrength;
                    float normalZ = Mathf.Sqrt(
                        Mathf.Clamp01(1f - slope.sqrMagnitude));
                    Vector3 normal = previewOrientation ==
                        PreviewOrientation.Horizontal
                            ? new Vector3(
                                slope.x,
                                normalZ,
                                slope.y)
                            : new Vector3(
                                slope.x,
                                slope.y,
                                normalZ);
                    normal.Normalize();

                    float formSigned =
                        (packed.a * 2f - 1f) *
                        profile.DetailValueStrength;
                    Color palette = formSigned < 0f
                        ? Color.Lerp(
                            profile.BaseColor,
                            profile.DarkColor,
                            Mathf.Clamp01(-formSigned))
                        : Color.Lerp(
                            profile.BaseColor,
                            profile.LightColor,
                            Mathf.Clamp01(
                                formSigned *
                                (1f + profile.DetailFormHighlightStrength)));
                    float cavityRaw = Mathf.Clamp01(
                        (packed.b - profile.DetailCavityBias) /
                        Mathf.Max(0.001f, 1f - profile.DetailCavityBias));
                    float cavityShoulder = Mathf.SmoothStep(
                        0f,
                        1f,
                        Mathf.Clamp01(cavityRaw / 0.82f)) *
                        profile.DetailCavityStrength;
                    float cavityCore = Mathf.SmoothStep(
                        0f,
                        1f,
                        Mathf.Clamp01(
                            (cavityRaw - 0.66f) / 0.32f)) *
                        profile.DetailCavityStrength;
                    palette = Color.Lerp(
                        palette,
                        profile.DarkColor,
                        Mathf.Clamp01(cavityShoulder * 0.42f));
                    palette = Color.Lerp(
                        palette,
                        profile.CavityColor,
                        Mathf.Clamp01(cavityCore));
                    float lighting = Mathf.Lerp(
                        0.38f,
                        1.08f,
                        Mathf.Clamp01(Vector3.Dot(normal, lightDirection)));
                    pixels[y * size + x] = new Color(
                        palette.r * lighting,
                        palette.g * lighting,
                        palette.b * lighting,
                        1f);
                }
            }

            previewTexture.SetPixels(pixels);
            previewTexture.Apply(false, false);
            previewSignature = signature;
        }

        private int ComputePreviewSignature(
            StylizedSurfaceMaterialProfile profile,
            Texture2D source)
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + source.GetEntityId().GetHashCode();
                hash = hash * 31 + profile.BaseColor.GetHashCode();
                hash = hash * 31 + profile.DarkColor.GetHashCode();
                hash = hash * 31 + profile.LightColor.GetHashCode();
                hash = hash * 31 + profile.CavityColor.GetHashCode();
                hash = hash * 31 + profile.PayloadMode.GetHashCode();
                hash = hash * 31 + profile.AuthoredColorStrength.GetHashCode();
                hash = hash * 31 + profile.AuthoredColorTint.GetHashCode();
                hash = hash * 31 + profile.AuthoredColorTintStrength.GetHashCode();
                hash = hash * 31 + profile.AuthoredColorLightingStrength.GetHashCode();
                hash = hash * 31 +
                       profile.DetailValueStrength.GetHashCode();
                hash = hash * 31 +
                       profile.DetailNormalStrength.GetHashCode();
                hash = hash * 31 +
                       profile.DetailCavityStrength.GetHashCode();
                hash = hash * 31 +
                       profile.DetailCavityBias.GetHashCode();
                hash = hash * 31 +
                       profile.DetailFormHighlightStrength.GetHashCode();
                hash = hash * 31 + (int)previewOrientation;
                return hash;
            }
        }
    }
}
