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
                "Base, Dark, Light, and Cavity are the only colour authorities. Imported material-set colour is converted to a grayscale texture-form map, so it can preserve stone structure without imposing source hue.",
                MessageType.Info);
            EditorGUILayout.HelpBox(
                "The Inspector Preview is diagnostic assistance only. Production-camera scene rendering remains the authoritative visual acceptance test.",
                MessageType.Info);

            DrawSection("Identity", "displayName");
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
                bool usesTextureForm = SelectedEntryUsesTextureForm();
                DrawProperties(
                    "detailWorldScale",
                    "detailNormalStrength",
                    "detailCavityStrength",
                    "detailCavityBias");
                if (usesTextureForm)
                {
                    DrawProperties(
                        "authoredColorStrength",
                        "authoredColorLightingStrength");
                }
                else
                {
                    DrawProperties(
                        "detailValueStrength",
                        "detailFormHighlightStrength");
                }
            }

            if (detailIsEnabled)
            {
                DrawDetailStatus();
            }

            EditorGUILayout.LabelField("Dry Finish", EditorStyles.boldLabel);
            DrawProperties("drySmoothness", "drySpecularStrength");
            if (detailIsEnabled)
            {
                if (SelectedEntryUsesTextureForm())
                {
                    DrawProperties("authoredRoughnessStrength");
                }
                else
                {
                    DrawProperties("finishVariationStrength");
                }
            }
            EditorGUILayout.Space(4f);

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
            return ResolvePreviewSource() != null;
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

        private bool SelectedEntryUsesTextureForm()
        {
            SerializedProperty enabledProperty =
                serializedObject.FindProperty("detailEnabled");
            if (enabledProperty == null || !enabledProperty.boolValue)
            {
                return false;
            }

            StylizedSurfaceDetailLibrary.Entry entry =
                ResolveSelectedEntryFromSerializedProperties();
            return entry != null && entry.UsesAuthoredMaterialSet;
        }

        private StylizedSurfaceDetailLibrary.Entry
            ResolveSelectedEntryFromSerializedProperties()
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
            string entryId = idProperty != null
                ? idProperty.stringValue
                : string.Empty;
            return ResolveEntry(library, entryId);
        }

        private static StylizedSurfaceDetailLibrary.Entry ResolveEntry(
            StylizedSurfaceDetailLibrary library,
            string entryId)
        {
            if (library == null || string.IsNullOrWhiteSpace(entryId))
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
                        entryId,
                        StringComparison.Ordinal))
                {
                    return entry;
                }
            }

            return null;
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
            StylizedSurfaceDetailLibrary.Entry entry =
                ResolveEntry(library, entryId);
            if (entry == null)
            {
                EditorGUILayout.HelpBox(
                    string.IsNullOrWhiteSpace(entryId)
                        ? "Select a surface-detail entry."
                        : $"The detail entry '{entryId}' is missing from the assigned library.",
                    MessageType.Warning);
                return;
            }

            EditorGUILayout.HelpBox(
                entry.UsesAuthoredMaterialSet
                    ? "This entry supplies normalized grayscale texture form, packed normal/cavity data, and roughness variation. Palette colours remain the sole visible colour controls."
                    : "This entry supplies prepacked normal, cavity, value/form, and finish variation.",
                MessageType.None);

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

        private StylizedSurfaceDetailLibrary.Entry ResolveSelectedEntry()
        {
            StylizedSurfaceMaterialProfile profile =
                target as StylizedSurfaceMaterialProfile;
            return profile != null
                ? ResolveEntry(profile.DetailLibrary, profile.DetailEntryId)
                : null;
        }

        private Texture2D ResolvePreviewSource()
        {
            StylizedSurfaceDetailLibrary.Entry entry = ResolveSelectedEntry();
            if (entry == null)
            {
                return null;
            }

            return entry.UsesAuthoredMaterialSet
                ? entry.AuthoredBaseColor
                : entry.SourceTexture;
        }

        private void EnsurePreviewTexture()
        {
            StylizedSurfaceMaterialProfile profile =
                target as StylizedSurfaceMaterialProfile;
            StylizedSurfaceDetailLibrary.Entry entry = ResolveSelectedEntry();
            Texture2D source = ResolvePreviewSource();
            if (profile == null ||
                entry == null ||
                source == null ||
                !source.isReadable)
            {
                return;
            }

            int signature = ComputePreviewSignature(profile, entry, source);
            if (previewTexture != null && signature == previewSignature)
            {
                return;
            }

            const int size = 192;
            Color[] packedPixels;
            Color[] formPixels = null;
            if (entry.UsesAuthoredMaterialSet)
            {
                if (entry.AuthoredNormal == null ||
                    entry.AuthoredHeight == null ||
                    entry.AuthoredAmbientOcclusion == null ||
                    entry.AuthoredRoughness == null ||
                    !entry.AuthoredNormal.isReadable ||
                    !entry.AuthoredHeight.isReadable ||
                    !entry.AuthoredAmbientOcclusion.isReadable ||
                    !entry.AuthoredRoughness.isReadable)
                {
                    return;
                }

                packedPixels =
                    StylizedSurfaceDetailLibraryBuilder
                        .BuildPackedMaterialPixels(entry, size);
                StylizedSurfaceDetailLibraryBuilder.AuthoredColorBuildResult
                    formBuild = StylizedSurfaceDetailLibraryBuilder
                        .BuildAuthoredColorMipChain(
                            entry.AuthoredBaseColor,
                            size);
                formPixels = formBuild.MipPixels[0];
            }
            else
            {
                packedPixels = ResamplePixels(source, size);
            }

            EnsurePreviewTextureAllocated(size);
            Color[] pixels = new Color[size * size];
            Vector3 lightDirection =
                new Vector3(-0.42f, 0.78f, 0.46f).normalized;
            Vector3 viewDirection = previewOrientation ==
                PreviewOrientation.Horizontal
                    ? new Vector3(0f, 1f, 0.3f).normalized
                    : new Vector3(0f, 0.15f, 1f).normalized;
            Vector3 halfDirection =
                (lightDirection + viewDirection).normalized;

            for (int index = 0; index < pixels.Length; index++)
            {
                Color packed = packedPixels[index];
                float formSigned = entry.UsesAuthoredMaterialSet
                    ? (StylizedSurfaceDetailLibraryBuilder.DecodeFormValue(
                        formPixels[index]) * 2f - 1f) *
                      profile.TextureFormStrength
                    : (packed.a * 2f - 1f) *
                      profile.DetailValueStrength;
                Color palette = ResolvePalette(
                    profile,
                    packed,
                    formSigned);
                Vector3 normal = ResolvePreviewNormal(profile, packed);
                float diffuse = Mathf.Lerp(
                    0.38f,
                    1.08f,
                    Mathf.Clamp01(Vector3.Dot(normal, lightDirection)));
                float diffuseResponse = entry.UsesAuthoredMaterialSet
                    ? Mathf.Lerp(
                        1f,
                        diffuse,
                        profile.SceneLightingResponse)
                    : diffuse;
                float smoothness = ResolvePreviewSmoothness(
                    profile,
                    packed,
                    entry.UsesAuthoredMaterialSet);
                float specular = Mathf.Pow(
                    Mathf.Clamp01(Vector3.Dot(normal, halfDirection)),
                    Mathf.Lerp(8f, 96f, smoothness)) *
                    profile.DrySpecularStrength;
                Color result = palette * diffuseResponse;
                result.r = Mathf.Clamp01(result.r + specular);
                result.g = Mathf.Clamp01(result.g + specular);
                result.b = Mathf.Clamp01(result.b + specular);
                result.a = 1f;
                pixels[index] = result;
            }

            previewTexture.SetPixels(pixels);
            previewTexture.Apply(false, false);
            previewSignature = signature;
        }

        private void EnsurePreviewTextureAllocated(int size)
        {
            if (previewTexture != null &&
                previewTexture.width == size &&
                previewTexture.height == size)
            {
                return;
            }

            if (previewTexture != null)
            {
                DestroyImmediate(previewTexture);
            }

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

        private Vector3 ResolvePreviewNormal(
            StylizedSurfaceMaterialProfile profile,
            Color packed)
        {
            Vector2 slope = new Vector2(
                packed.r * 2f - 1f,
                packed.g * 2f - 1f) *
                profile.DetailNormalStrength;
            float normalZ = Mathf.Sqrt(
                Mathf.Clamp01(1f - slope.sqrMagnitude));
            Vector3 normal = previewOrientation ==
                PreviewOrientation.Horizontal
                    ? new Vector3(slope.x, normalZ, slope.y)
                    : new Vector3(slope.x, slope.y, normalZ);
            return normal.sqrMagnitude > 0.000001f
                ? normal.normalized
                : Vector3.up;
        }

        private static Color ResolvePalette(
            StylizedSurfaceMaterialProfile profile,
            Color packed,
            float formSigned)
        {
            float positive = Mathf.Clamp01(formSigned);
            positive = Mathf.Clamp01(
                positive *
                (1f + profile.DetailFormHighlightStrength));
            Color palette = formSigned < 0f
                ? Color.Lerp(
                    profile.BaseColor,
                    profile.DarkColor,
                    Mathf.Clamp01(-formSigned))
                : Color.Lerp(
                    profile.BaseColor,
                    profile.LightColor,
                    positive);

            float cavityRaw = Mathf.Clamp01(
                (packed.b - profile.DetailCavityBias) /
                Mathf.Max(0.001f, 1f - profile.DetailCavityBias));
            float cavityShoulder = Mathf.SmoothStep(
                0f,
                0.82f,
                cavityRaw) * profile.DetailCavityStrength;
            float cavityCore = Mathf.SmoothStep(
                0.66f,
                0.98f,
                cavityRaw) * profile.DetailCavityStrength;
            palette = Color.Lerp(
                palette,
                profile.DarkColor,
                Mathf.Clamp01(cavityShoulder * 0.42f));
            palette = Color.Lerp(
                palette,
                profile.CavityColor,
                Mathf.Clamp01(cavityCore));
            palette.a = 1f;
            return palette;
        }

        private static float ResolvePreviewSmoothness(
            StylizedSurfaceMaterialProfile profile,
            Color packed,
            bool usesTextureForm)
        {
            float finishSigned = usesTextureForm
                ? 0f
                : (packed.a * 2f - 1f) *
                  profile.FinishVariationStrength;
            float roughnessVariation = usesTextureForm
                ? (0.5f - packed.a) *
                  0.5f *
                  profile.RoughnessVariationStrength
                : 0f;
            float cavityRaw = Mathf.Clamp01(
                (packed.b - profile.DetailCavityBias) /
                Mathf.Max(0.001f, 1f - profile.DetailCavityBias));
            float cavity = Mathf.SmoothStep(0f, 0.82f, cavityRaw) *
                profile.DetailCavityStrength;
            return Mathf.Clamp01(
                profile.DrySmoothness +
                finishSigned +
                roughnessVariation -
                cavity * 0.08f);
        }

        private static Color[] ResamplePixels(Texture2D source, int size)
        {
            Color[] pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                float v = (y + 0.5f) / size;
                for (int x = 0; x < size; x++)
                {
                    float u = (x + 0.5f) / size;
                    pixels[y * size + x] =
                        source.GetPixelBilinear(u, v);
                }
            }

            return pixels;
        }

        private int ComputePreviewSignature(
            StylizedSurfaceMaterialProfile profile,
            StylizedSurfaceDetailLibrary.Entry entry,
            Texture2D source)
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + source.GetEntityId().GetHashCode();
                hash = hash * 31 + entry.SourceMode.GetHashCode();
                hash = hash * 31 + profile.BaseColor.GetHashCode();
                hash = hash * 31 + profile.DarkColor.GetHashCode();
                hash = hash * 31 + profile.LightColor.GetHashCode();
                hash = hash * 31 + profile.CavityColor.GetHashCode();
                hash = hash * 31 + profile.TextureFormStrength.GetHashCode();
                hash = hash * 31 +
                       profile.SceneLightingResponse.GetHashCode();
                hash = hash * 31 +
                       profile.RoughnessVariationStrength.GetHashCode();
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
                hash = hash * 31 +
                       profile.FinishVariationStrength.GetHashCode();
                hash = hash * 31 + profile.DrySmoothness.GetHashCode();
                hash = hash * 31 +
                       profile.DrySpecularStrength.GetHashCode();
                hash = hash * 31 + (int)previewOrientation;
                return hash;
            }
        }
    }
}
