using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using ProgrammaticStylized3D.Rendering.PixelSurface;

namespace ProgrammaticStylized3D.Geometry.Ground.Editor
{
    [CustomEditor(typeof(GeneratedGround))]
    [CanEditMultipleObjects]
    public sealed class GeneratedGroundEditor : UnityEditor.Editor
    {
        private const double SharedStyleSaveDelaySeconds = 0.35;
        private const string DefaultSurfaceLayerFolder =
            "Assets/Game/Demo/Profiles/Ground/Layers";
        private const string DefaultHydrologyModifierFolder =
            "Assets/Game/Demo/Profiles/Ground";

        private static readonly HashSet<GroundSurfaceStyleProfile>
            PendingSharedStyleSaves =
                new HashSet<GroundSurfaceStyleProfile>();

        private static readonly HashSet<GroundSurfaceLayerProfile>
            PendingSurfaceLayerSaves =
                new HashSet<GroundSurfaceLayerProfile>();

        private static readonly HashSet<GroundHydrologyModifierProfile>
            PendingHydrologyModifierSaves =
                new HashSet<GroundHydrologyModifierProfile>();

        private static List<GroundSurfaceLayerProfile>
            cachedSurfaceLayerProfiles;
        private static List<GroundHydrologyModifierProfile>
            cachedHydrologyModifierProfiles;

        private static readonly Dictionary<EntityId, bool>
            SharedSurfaceMaterialFoldouts =
                new Dictionary<EntityId, bool>();

        private static bool sharedStyleSaveUpdateRegistered;
        private static double sharedStyleSaveDeadline;

        static GeneratedGroundEditor()
        {
            AssemblyReloadEvents.beforeAssemblyReload +=
                FlushPendingSharedStyleSaves;
            EditorApplication.quitting += FlushPendingSharedStyleSaves;
            EditorApplication.projectChanged +=
                InvalidateSurfaceLayerProfileCache;
            StylizedSurfaceMaterialProfile.EditorProfileChanged +=
                RefreshLoadedGroundsUsingSurfaceMaterial;
        }

        private SerializedProperty recipe;
        private SerializedProperty surfaceStyleProfile;
        private SerializedProperty surfaceVariantId;
        private SerializedProperty overrideSurfaceProfile;
        private SerializedProperty surfaceProfile;
        private SerializedProperty overrideMaterialControls;
        private SerializedProperty groundMaterialControls;
        private SerializedProperty regenerateOnValidate;
        private SerializedProperty debugView;
        private SerializedProperty showPaintedAccentDistributionOverlay;
        private SerializedProperty showPaintedAccentWeightedProposals;
        private SerializedProperty showPaintedAccentLastAcceptedPositions;
        private SerializedProperty showPaintedAccentCompositionDebug;
        private SerializedProperty showPaintedAccentProjectedGlyphDebug;
        private SerializedProperty paintedAccentGlyphFamilyPreview;
        private SerializedProperty paintedAccentPlacementOverlayWeight;

        private int paintedAccentPlacementDebugSignature = int.MinValue;
        private bool paintedAccentPlacementDebugSnapshotBuildFailed;
        private GroundPaintedAccentPlacementDebugSnapshot
            paintedAccentPlacementDebugSnapshot =
                GroundPaintedAccentPlacementDebugSnapshot.Empty;
        private int paintedAccentProjectedGlyphDebugSignature = int.MinValue;
        private bool paintedAccentProjectedGlyphDebugSnapshotBuildFailed;
        private GroundPaintedAccentProjectedGlyphDebugSnapshot
            paintedAccentProjectedGlyphDebugSnapshot =
                GroundPaintedAccentProjectedGlyphDebugSnapshot.Empty;

        private SerializedProperty shapeSeed;
        private SerializedProperty patchSize;
        private SerializedProperty resolution;
        private SerializedProperty patchCoordinate;
        private SerializedProperty transitionDirection;
        private SerializedProperty transitionHeight;
        private SerializedProperty profile;
        private SerializedProperty broadForm;
        private SerializedProperty roughness;
        private SerializedProperty surfaceDetail;
        private SerializedProperty edgeBlend;
        private SerializedProperty surfaceVariation;
        private SerializedProperty useModifiers;

        private SerializedProperty baseColor;
        private SerializedProperty frostColor;
        private SerializedProperty dampTint;
        private SerializedProperty dampTintStrength;
        private SerializedProperty rockyDryTint;
        private SerializedProperty rockyDryTintStrength;
        private SerializedProperty vegetationTint;
        private SerializedProperty vegetationTintStrength;
        private SerializedProperty bankSurfaceLayer;
        private SerializedProperty riverbedSurfaceLayer;
        private SerializedProperty riverbedSurfaceSource;
        private SerializedProperty shoreHydrologyModifier;
        private SerializedProperty riverbedHydrologySource;
        private SerializedProperty riverbedHydrologyModifier;
        private SerializedProperty bankMaterialStrength;
        private SerializedProperty bankDetailScaleMultiplier;
        private SerializedProperty bankAuthoredColorStrengthMultiplier;
        private SerializedProperty bankAuthoredColorLightingMultiplier;
        private SerializedProperty bankDetailNormalStrengthMultiplier;
        private SerializedProperty bankDetailCavityStrengthMultiplier;
        private SerializedProperty bankDetailValueFormMultiplier;
        private SerializedProperty bankDetailFinishVariationMultiplier;
        private SerializedProperty bankLegacyPixelCellInfluenceMultiplier;
        private SerializedProperty riverbedMaterialStrength;
        private SerializedProperty riverbedDetailScaleMultiplier;
        private SerializedProperty riverbedAuthoredColorStrengthMultiplier;
        private SerializedProperty riverbedAuthoredColorLightingMultiplier;
        private SerializedProperty riverbedDetailNormalStrengthMultiplier;
        private SerializedProperty riverbedDetailCavityStrengthMultiplier;
        private SerializedProperty riverbedDetailValueFormMultiplier;
        private SerializedProperty riverbedDetailFinishVariationMultiplier;
        private SerializedProperty riverbedLegacyPixelCellInfluenceMultiplier;
        private SerializedProperty riverbedWetnessStrength;
        private SerializedProperty riverbedToBankWetnessBlendDistance;
        private SerializedProperty riverbedToBankWetnessBlendSoftness;
        private SerializedProperty riverbedWetSmoothnessResponse;
        private SerializedProperty riverbedWetSpecularResponse;
        private SerializedProperty bankMaterialReach;
        private SerializedProperty immediateBankExposure;
        private SerializedProperty waterlineMaterialStrength;
        private SerializedProperty bankTransitionSoftness;
        private SerializedProperty outerBankExtension;
        private SerializedProperty outerBankStrength;
        private SerializedProperty outerBankFade;
        private SerializedProperty vegetationRetreatStrength;
        private SerializedProperty snowMeltStrength;
        private SerializedProperty frostRetreatStrength;
        private SerializedProperty paintedAccentRetreatStrength;
        private SerializedProperty shoreWetnessStrength;
        private SerializedProperty shoreWetnessReach;
        private SerializedProperty shoreWetnessFade;
        private SerializedProperty broadBankSaturation;
        private SerializedProperty immediateBankSaturation;
        private SerializedProperty waterlineSaturation;
        private SerializedProperty shoreWetHighlightWidth;
        private SerializedProperty shoreWetHighlightFeather;
        private SerializedProperty shoreWetHighlightStrength;
        private SerializedProperty shoreWetHighlightTightness;
        private SerializedProperty shoreWetHighlightCameraBias;
        private SerializedProperty shoreWetHighlightVerticalFalloff;
        private SerializedProperty pixelCellSize;
        private SerializedProperty pixelToneCount;
        private SerializedProperty pixelClusterStrength;
        private SerializedProperty pixelVariation;
        private SerializedProperty broadVariation;
        private SerializedProperty vertexVariation;
        private SerializedProperty pixelEffectStrength;
        private SerializedProperty cellWarpStrength;
        private SerializedProperty groundMacroPatchScale;
        private SerializedProperty groundMacroPatchPatternSeed;
        private SerializedProperty groundMacroPatchTransitionSoftness;
        private SerializedProperty groundMacroPatchSeparation;
        private SerializedProperty reliefShadingStrength;
        private SerializedProperty relativeHeightContrast;
        private SerializedProperty profileContrastScale;
        private SerializedProperty profilePixelContrastScale;
        private SerializedProperty groundSnowResponseScale;
        private SerializedProperty groundDampResponseScale;
        private SerializedProperty groundVegetationResponseScale;
        private SerializedProperty groundRockyDryResponseScale;
        private SerializedProperty groundPatchBlendStrength;
        private SerializedProperty groundSnowTintStrength;
        private SerializedProperty groundSnowBrightness;
        private SerializedProperty groundDampDarkenStrength;
        private SerializedProperty wetness;
        private SerializedProperty wetDarkenStrength;
        private SerializedProperty wetPixelSoftening;
        private SerializedProperty wetSmoothnessBoost;
        private SerializedProperty frostStrength;
        private SerializedProperty frostContrast;
        private SerializedProperty monolithicFlatten;
        private SerializedProperty monolithicSmoothnessBoost;
        private SerializedProperty smoothness;
        private SerializedProperty specularStrength;

        private bool showGroundOverview = true;
        private bool showResolvedFeatureSummary;
        private bool showGroundGeometry = true;
        private bool showPatchDomain = true;
        private bool showBaseShape = true;
        private bool showMountainTransition;
        private bool showSurfaceAppearance = true;
        private bool showSurfaceResponseProfile = true;
        private bool showSurfaceFeatures = true;
        private bool showDirectionalStreaks;
        private bool showPooledWetness;
        private bool showTrampledWear;
        private bool showGroundInteraction;
        private bool showRegenerationAndCaching;
        private bool showDebugAndDiagnostics;
        private bool showGroundDebug;
        private bool showCurrentRegenerationTiming;
        private bool showRegenerationAccounting;
        private bool showPaintedAccentStrokes = true;
        private bool showPaintedAccentBasics = true;
        private bool showPaintedAccentDistribution;
        private bool showPaintedAccentHorizontalCompanions = true;
        private bool showPaintedAccentAdvancedCompanionLayoutMix;
        private bool showPaintedAccentFamilyMix;
        private bool showPaintedAccentGeometry;
        private bool showPaintedAccentProfile;
        private bool showPaintedAccentPreviewAndProduction = true;
        private bool showPaintedAccentPlacementDebug;
        private bool showPaintedAccentPlacementOverlays;
        private bool showPaintedAccentShapeOverlay;
        private bool showPaintedAccentDiagnostics;
        private bool showSurfaceDiagnostics;
        private bool showMaterialControls;
        private bool showMaterialRiverCoupledBank = true;
        private bool showMaterialRiverCoupledRiverbed = true;
        private bool showBankSurfaceLayerSettings;
        private bool showRiverbedSurfaceLayerSettings;
        private bool showShoreHydrologyModifierSettings;
        private bool showRiverbedHydrologyModifierSettings;
        private bool showMaterialPalette;
        private bool showMaterialMacroPatchComposition = true;
        private bool showMaterialElevationReadability = true;
        private bool showMaterialPixelVariation;
        private bool showMaterialSemanticResponse;
        private bool showMaterialWeatherFinish;
        private bool showStyleAssetDetails;


        private static void InvalidateSurfaceLayerProfileCache()
        {
            cachedSurfaceLayerProfiles = null;
            cachedHydrologyModifierProfiles = null;
        }

        private static void QueueSharedStyleSave(
            GroundSurfaceStyleProfile style)
        {
            if (style == null || !EditorUtility.IsPersistent(style))
            {
                return;
            }

            PendingSharedStyleSaves.Add(style);
            sharedStyleSaveDeadline =
                EditorApplication.timeSinceStartup +
                SharedStyleSaveDelaySeconds;

            RegisterPendingAssetSaveUpdate();
        }

        private static void QueueSurfaceLayerSave(
            GroundSurfaceLayerProfile layer)
        {
            if (layer == null || !EditorUtility.IsPersistent(layer))
            {
                return;
            }

            PendingSurfaceLayerSaves.Add(layer);
            sharedStyleSaveDeadline =
                EditorApplication.timeSinceStartup +
                SharedStyleSaveDelaySeconds;
            RegisterPendingAssetSaveUpdate();
        }

        private static void QueueHydrologyModifierSave(
            GroundHydrologyModifierProfile modifier)
        {
            if (modifier == null || !EditorUtility.IsPersistent(modifier))
            {
                return;
            }

            PendingHydrologyModifierSaves.Add(modifier);
            sharedStyleSaveDeadline =
                EditorApplication.timeSinceStartup +
                SharedStyleSaveDelaySeconds;
            RegisterPendingAssetSaveUpdate();
        }

        private static void RegisterPendingAssetSaveUpdate()
        {
            if (sharedStyleSaveUpdateRegistered)
            {
                return;
            }

            sharedStyleSaveUpdateRegistered = true;
            EditorApplication.update += TryFlushPendingSharedStyleSaves;
        }

        private static void TryFlushPendingSharedStyleSaves()
        {
            if (EditorApplication.timeSinceStartup <
                sharedStyleSaveDeadline)
            {
                return;
            }

            FlushPendingSharedStyleSaves();
        }

        private static void FlushPendingSharedStyleSaves()
        {
            if (sharedStyleSaveUpdateRegistered)
            {
                EditorApplication.update -=
                    TryFlushPendingSharedStyleSaves;
                sharedStyleSaveUpdateRegistered = false;
            }

            foreach (GroundSurfaceStyleProfile style in
                     PendingSharedStyleSaves)
            {
                if (style != null && EditorUtility.IsPersistent(style))
                {
                    AssetDatabase.SaveAssetIfDirty(style);
                }
            }

            PendingSharedStyleSaves.Clear();

            foreach (GroundSurfaceLayerProfile layer in
                     PendingSurfaceLayerSaves)
            {
                if (layer != null && EditorUtility.IsPersistent(layer))
                {
                    AssetDatabase.SaveAssetIfDirty(layer);
                }
            }

            PendingSurfaceLayerSaves.Clear();

            foreach (GroundHydrologyModifierProfile modifier in
                     PendingHydrologyModifierSaves)
            {
                if (modifier != null && EditorUtility.IsPersistent(modifier))
                {
                    AssetDatabase.SaveAssetIfDirty(modifier);
                }
            }

            PendingHydrologyModifierSaves.Clear();
        }

        private static void DrawMaterialStorageLine(
            string storage,
            string tooltip)
        {
            EditorGUILayout.LabelField(
                new GUIContent("Stored In", tooltip),
                new GUIContent(storage, tooltip));
        }

        private static bool DrawSectionFoldout(
            ref bool expanded,
            string label,
            float spacing = 8f)
        {
            if (spacing > 0f)
            {
                EditorGUILayout.Space(spacing);
            }

            expanded = EditorGUILayout.Foldout(
                expanded,
                label,
                true);
            return expanded;
        }

        private static bool DrawSubsectionFoldout(
            ref bool expanded,
            string label)
        {
            expanded = EditorGUILayout.Foldout(
                expanded,
                label,
                true);
            return expanded;
        }

        private void OnEnable()
        {
            recipe =
                serializedObject.FindProperty("recipe");

            surfaceStyleProfile =
                serializedObject.FindProperty("surfaceStyleProfile");

            surfaceVariantId =
                serializedObject.FindProperty("surfaceVariantId");

            overrideSurfaceProfile =
                serializedObject.FindProperty("overrideSurfaceProfile");

            surfaceProfile =
                serializedObject.FindProperty("surfaceProfile");

            overrideMaterialControls =
                serializedObject.FindProperty("overrideMaterialControls");

            groundMaterialControls =
                serializedObject.FindProperty("groundMaterialControls");

            regenerateOnValidate =
                serializedObject.FindProperty(
                    "regenerateOnValidate");

            debugView =
                serializedObject.FindProperty("debugView");

            showPaintedAccentDistributionOverlay =
                serializedObject.FindProperty(
                    "showPaintedAccentDistributionOverlay");

            showPaintedAccentWeightedProposals =
                serializedObject.FindProperty(
                    "showPaintedAccentWeightedProposals");

            showPaintedAccentLastAcceptedPositions =
                serializedObject.FindProperty(
                    "showPaintedAccentLastAcceptedPositions");

            showPaintedAccentCompositionDebug =
                serializedObject.FindProperty(
                    "showPaintedAccentCompositionDebug");

            showPaintedAccentProjectedGlyphDebug =
                serializedObject.FindProperty(
                    "showPaintedAccentProjectedGlyphDebug");

            paintedAccentGlyphFamilyPreview =
                serializedObject.FindProperty(
                    "paintedAccentGlyphFamilyPreview");

            paintedAccentPlacementOverlayWeight =
                serializedObject.FindProperty(
                    "paintedAccentPlacementOverlayWeight");

            shapeSeed =
                recipe.FindPropertyRelative("shapeSeed");

            patchSize =
                recipe.FindPropertyRelative("patchSize");

            resolution =
                recipe.FindPropertyRelative("resolution");

            patchCoordinate =
                recipe.FindPropertyRelative("patchCoordinate");

            transitionDirection =
                recipe.FindPropertyRelative(
                    "transitionDirection");

            transitionHeight =
                recipe.FindPropertyRelative(
                    "transitionHeight");

            profile =
                recipe.FindPropertyRelative("profile");

            broadForm =
                recipe.FindPropertyRelative("broadForm");

            roughness =
                recipe.FindPropertyRelative("roughness");

            surfaceDetail =
                recipe.FindPropertyRelative("surfaceDetail");

            edgeBlend =
                recipe.FindPropertyRelative("edgeBlend");

            surfaceVariation =
                recipe.FindPropertyRelative(
                    "surfaceVariation");

            useModifiers =
                recipe.FindPropertyRelative("useModifiers");

            baseColor =
                groundMaterialControls.FindPropertyRelative("baseColor");

            frostColor =
                groundMaterialControls.FindPropertyRelative("frostColor");

            dampTint =
                groundMaterialControls.FindPropertyRelative("dampTint");

            dampTintStrength =
                groundMaterialControls.FindPropertyRelative("dampTintStrength");

            rockyDryTint =
                groundMaterialControls.FindPropertyRelative("rockyDryTint");

            rockyDryTintStrength =
                groundMaterialControls.FindPropertyRelative("rockyDryTintStrength");

            vegetationTint =
                groundMaterialControls.FindPropertyRelative("vegetationTint");

            vegetationTintStrength =
                groundMaterialControls.FindPropertyRelative("vegetationTintStrength");

            bankSurfaceLayer =
                groundMaterialControls.FindPropertyRelative("bankSurfaceLayer");

            riverbedSurfaceLayer =
                groundMaterialControls.FindPropertyRelative("riverbedSurfaceLayer");

            riverbedSurfaceSource =
                groundMaterialControls.FindPropertyRelative(
                    "riverbedSurfaceSource");

            shoreHydrologyModifier =
                groundMaterialControls.FindPropertyRelative(
                    "shoreHydrologyModifier");

            riverbedHydrologySource =
                groundMaterialControls.FindPropertyRelative(
                    "riverbedHydrologySource");

            riverbedHydrologyModifier =
                groundMaterialControls.FindPropertyRelative(
                    "riverbedHydrologyModifier");

            bankMaterialStrength =
                groundMaterialControls.FindPropertyRelative("bankMaterialStrength");

            bankDetailScaleMultiplier =
                groundMaterialControls.FindPropertyRelative(
                    "bankDetailScaleMultiplier");

            bankAuthoredColorStrengthMultiplier =
                groundMaterialControls.FindPropertyRelative(
                    "bankAuthoredColorStrengthMultiplier");

            bankAuthoredColorLightingMultiplier =
                groundMaterialControls.FindPropertyRelative(
                    "bankAuthoredColorLightingMultiplier");

            bankDetailNormalStrengthMultiplier =
                groundMaterialControls.FindPropertyRelative(
                    "bankDetailNormalStrengthMultiplier");

            bankDetailCavityStrengthMultiplier =
                groundMaterialControls.FindPropertyRelative(
                    "bankDetailCavityStrengthMultiplier");

            bankDetailValueFormMultiplier =
                groundMaterialControls.FindPropertyRelative(
                    "bankDetailValueFormMultiplier");

            bankDetailFinishVariationMultiplier =
                groundMaterialControls.FindPropertyRelative(
                    "bankDetailFinishVariationMultiplier");

            bankLegacyPixelCellInfluenceMultiplier =
                groundMaterialControls.FindPropertyRelative(
                    "bankLegacyPixelCellInfluenceMultiplier");

            riverbedMaterialStrength =
                groundMaterialControls.FindPropertyRelative(
                    "riverbedMaterialStrength");

            riverbedDetailScaleMultiplier =
                groundMaterialControls.FindPropertyRelative(
                    "riverbedDetailScaleMultiplier");

            riverbedAuthoredColorStrengthMultiplier =
                groundMaterialControls.FindPropertyRelative(
                    "riverbedAuthoredColorStrengthMultiplier");

            riverbedAuthoredColorLightingMultiplier =
                groundMaterialControls.FindPropertyRelative(
                    "riverbedAuthoredColorLightingMultiplier");

            riverbedDetailNormalStrengthMultiplier =
                groundMaterialControls.FindPropertyRelative(
                    "riverbedDetailNormalStrengthMultiplier");

            riverbedDetailCavityStrengthMultiplier =
                groundMaterialControls.FindPropertyRelative(
                    "riverbedDetailCavityStrengthMultiplier");

            riverbedDetailValueFormMultiplier =
                groundMaterialControls.FindPropertyRelative(
                    "riverbedDetailValueFormMultiplier");

            riverbedDetailFinishVariationMultiplier =
                groundMaterialControls.FindPropertyRelative(
                    "riverbedDetailFinishVariationMultiplier");

            riverbedLegacyPixelCellInfluenceMultiplier =
                groundMaterialControls.FindPropertyRelative(
                    "riverbedLegacyPixelCellInfluenceMultiplier");

            riverbedWetnessStrength =
                groundMaterialControls.FindPropertyRelative(
                    "riverbedWetnessStrength");

            riverbedToBankWetnessBlendDistance =
                groundMaterialControls.FindPropertyRelative(
                    "riverbedToBankWetnessBlendDistance");

            riverbedToBankWetnessBlendSoftness =
                groundMaterialControls.FindPropertyRelative(
                    "riverbedToBankWetnessBlendSoftness");

            riverbedWetSmoothnessResponse =
                groundMaterialControls.FindPropertyRelative(
                    "riverbedWetSmoothnessResponse");

            riverbedWetSpecularResponse =
                groundMaterialControls.FindPropertyRelative(
                    "riverbedWetSpecularResponse");

            bankMaterialReach =
                groundMaterialControls.FindPropertyRelative("bankMaterialReach");

            immediateBankExposure =
                groundMaterialControls.FindPropertyRelative("immediateBankExposure");

            waterlineMaterialStrength =
                groundMaterialControls.FindPropertyRelative("waterlineMaterialStrength");

            bankTransitionSoftness =
                groundMaterialControls.FindPropertyRelative("bankTransitionSoftness");

            outerBankExtension =
                groundMaterialControls.FindPropertyRelative("outerBankExtension");

            outerBankStrength =
                groundMaterialControls.FindPropertyRelative("outerBankStrength");

            outerBankFade =
                groundMaterialControls.FindPropertyRelative("outerBankFade");

            vegetationRetreatStrength =
                groundMaterialControls.FindPropertyRelative(
                    "vegetationRetreatStrength");

            snowMeltStrength =
                groundMaterialControls.FindPropertyRelative("snowMeltStrength");

            frostRetreatStrength =
                groundMaterialControls.FindPropertyRelative(
                    "frostRetreatStrength");

            paintedAccentRetreatStrength =
                groundMaterialControls.FindPropertyRelative(
                    "paintedAccentRetreatStrength");

            shoreWetnessStrength =
                groundMaterialControls.FindPropertyRelative(
                    "shoreWetnessStrength");

            shoreWetnessReach =
                groundMaterialControls.FindPropertyRelative(
                    "shoreWetnessReach");

            shoreWetnessFade =
                groundMaterialControls.FindPropertyRelative(
                    "shoreWetnessFade");

            broadBankSaturation =
                groundMaterialControls.FindPropertyRelative(
                    "broadBankSaturation");

            immediateBankSaturation =
                groundMaterialControls.FindPropertyRelative(
                    "immediateBankSaturation");

            waterlineSaturation =
                groundMaterialControls.FindPropertyRelative(
                    "waterlineSaturation");

            shoreWetHighlightWidth =
                groundMaterialControls.FindPropertyRelative(
                    "shoreWetHighlightWidth");

            shoreWetHighlightFeather =
                groundMaterialControls.FindPropertyRelative(
                    "shoreWetHighlightFeather");

            shoreWetHighlightStrength =
                groundMaterialControls.FindPropertyRelative(
                    "shoreWetHighlightStrength");

            shoreWetHighlightTightness =
                groundMaterialControls.FindPropertyRelative(
                    "shoreWetHighlightTightness");

            shoreWetHighlightCameraBias =
                groundMaterialControls.FindPropertyRelative(
                    "shoreWetHighlightCameraBias");

            shoreWetHighlightVerticalFalloff =
                groundMaterialControls.FindPropertyRelative(
                    "shoreWetHighlightVerticalFalloff");

            pixelCellSize =
                groundMaterialControls.FindPropertyRelative("pixelCellSize");

            pixelToneCount =
                groundMaterialControls.FindPropertyRelative("pixelToneCount");

            pixelClusterStrength =
                groundMaterialControls.FindPropertyRelative("pixelClusterStrength");

            pixelVariation =
                groundMaterialControls.FindPropertyRelative("pixelVariation");

            broadVariation =
                groundMaterialControls.FindPropertyRelative("broadVariation");

            vertexVariation =
                groundMaterialControls.FindPropertyRelative("vertexVariation");

            pixelEffectStrength =
                groundMaterialControls.FindPropertyRelative("pixelEffectStrength");

            cellWarpStrength =
                groundMaterialControls.FindPropertyRelative("cellWarpStrength");

            groundMacroPatchScale =
                groundMaterialControls.FindPropertyRelative("groundMacroPatchScale");

            groundMacroPatchPatternSeed =
                groundMaterialControls.FindPropertyRelative(
                    "groundMacroPatchPatternSeed");

            groundMacroPatchTransitionSoftness =
                groundMaterialControls.FindPropertyRelative(
                    "groundMacroPatchTransitionSoftness");

            groundMacroPatchSeparation =
                groundMaterialControls.FindPropertyRelative(
                    "groundMacroPatchSeparation");

            reliefShadingStrength =
                groundMaterialControls.FindPropertyRelative(
                    "reliefShadingStrength");

            relativeHeightContrast =
                groundMaterialControls.FindPropertyRelative(
                    "relativeHeightContrast");

            profileContrastScale =
                groundMaterialControls.FindPropertyRelative("profileContrastScale");

            profilePixelContrastScale =
                groundMaterialControls.FindPropertyRelative("profilePixelContrastScale");

            groundSnowResponseScale =
                groundMaterialControls.FindPropertyRelative("groundSnowResponseScale");

            groundDampResponseScale =
                groundMaterialControls.FindPropertyRelative("groundDampResponseScale");

            groundVegetationResponseScale =
                groundMaterialControls.FindPropertyRelative("groundVegetationResponseScale");

            groundRockyDryResponseScale =
                groundMaterialControls.FindPropertyRelative("groundRockyDryResponseScale");

            groundPatchBlendStrength =
                groundMaterialControls.FindPropertyRelative("groundPatchBlendStrength");

            groundSnowTintStrength =
                groundMaterialControls.FindPropertyRelative("groundSnowTintStrength");

            groundSnowBrightness =
                groundMaterialControls.FindPropertyRelative("groundSnowBrightness");

            groundDampDarkenStrength =
                groundMaterialControls.FindPropertyRelative("groundDampDarkenStrength");

            wetness =
                groundMaterialControls.FindPropertyRelative("wetness");

            wetDarkenStrength =
                groundMaterialControls.FindPropertyRelative("wetDarkenStrength");

            wetPixelSoftening =
                groundMaterialControls.FindPropertyRelative("wetPixelSoftening");

            wetSmoothnessBoost =
                groundMaterialControls.FindPropertyRelative("wetSmoothnessBoost");

            frostStrength =
                groundMaterialControls.FindPropertyRelative("frostStrength");

            frostContrast =
                groundMaterialControls.FindPropertyRelative("frostContrast");

            monolithicFlatten =
                groundMaterialControls.FindPropertyRelative("monolithicFlatten");

            monolithicSmoothnessBoost =
                groundMaterialControls.FindPropertyRelative("monolithicSmoothnessBoost");

            smoothness =
                groundMaterialControls.FindPropertyRelative("smoothness");

            specularStrength =
                groundMaterialControls.FindPropertyRelative("specularStrength");

            Undo.undoRedoPerformed += HandleUndoRedo;
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= HandleUndoRedo;
        }

        private void HandleUndoRedo()
        {
            serializedObject.UpdateIfRequiredOrScript();

            GeneratedGround[] grounds =
                Object.FindObjectsByType<GeneratedGround>(
                    FindObjectsInactive.Include);

            for (int index = 0; index < grounds.Length; index++)
            {
                GeneratedGround ground = grounds[index];
                if (IsLoadedSceneGround(ground))
                {
                    ground.RefreshSurfaceStyleState();
                }
            }

            Repaint();
            SceneView.RepaintAll();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawGroundOverviewSection();
            DrawGroundGeometrySection();
            DrawSurfaceAppearanceSection();
            DrawSurfaceFeaturesSection();
            DrawPaintedAccentStrokeControls();
            DrawGroundInteractionSection();
            DrawRegenerationAndCachingSection();
            DrawDebugAndDiagnosticsSection();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawGroundOverviewSection()
        {
            if (!DrawSectionFoldout(
                    ref showGroundOverview,
                    "Ground Overview",
                    0f))
            {
                return;
            }

            EditorGUI.indentLevel++;

            DrawSurfaceFamilyPopup();

            GroundSurfaceStyleProfile style =
                surfaceStyleProfile.objectReferenceValue as
                    GroundSurfaceStyleProfile;

            DrawSurfaceVariantPopup(style);

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField(
                "Deterministic Identity",
                EditorStyles.miniBoldLabel);
            EditorGUILayout.IntSlider(
                shapeSeed,
                GroundRecipe.MinimumSeed,
                GroundRecipe.MaximumSeed,
                new GUIContent(
                    "Shape Seed",
                    "Deterministic terrain variation seed."));
            EditorGUILayout.PropertyField(
                patchCoordinate,
                new GUIContent(
                    "Patch Coordinate",
                    "Stable deterministic noise coordinate used by patch assembly."));

            DrawStyleWarnings(style);
            DrawSurfaceProfileOverride(style);
            DrawResolvedFeatureSummary();
            DrawStyleAssetDetails(style);

            EditorGUI.indentLevel--;
        }

        private void DrawSurfaceFamilyPopup()
        {
            GroundSurfaceStyleProfile[] styles =
                LoadAvailableStyleProfiles();

            if (styles.Length == 0)
            {
                EditorGUILayout.HelpBox(
                    "No GroundSurfaceStyleProfile assets were found. Create or assign a style profile before choosing a family.",
                    MessageType.Warning);

                DrawManualSurfaceStyleField();
                return;
            }

            GroundSurfaceStyleProfile current =
                surfaceStyleProfile.objectReferenceValue as
                    GroundSurfaceStyleProfile;

            int selectedIndex = 0;
            bool foundCurrent = false;

            for (int index = 0; index < styles.Length; index++)
            {
                if (styles[index] == current)
                {
                    selectedIndex = index;
                    foundCurrent = true;
                    break;
                }
            }

            if (current != null && !foundCurrent)
            {
                styles = AppendStyle(styles, current);
                selectedIndex = styles.Length - 1;
                foundCurrent = true;
            }

            GUIContent[] labels = new GUIContent[styles.Length];

            for (int index = 0; index < styles.Length; index++)
            {
                GroundSurfaceStyleProfile style = styles[index];
                labels[index] = new GUIContent(
                    style != null ? style.DisplayName : "Missing Style");
            }

            EditorGUI.showMixedValue =
                surfaceStyleProfile.hasMultipleDifferentValues;
            EditorGUI.BeginChangeCheck();

            int newSelectedIndex = EditorGUILayout.Popup(
                new GUIContent(
                    "Surface Family",
                    "Top-level visual ground family. This assigns a GroundSurfaceStyleProfile asset without manual dragging."),
                Mathf.Clamp(selectedIndex, 0, labels.Length - 1),
                labels);

            EditorGUI.showMixedValue = false;

            if (EditorGUI.EndChangeCheck())
            {
                GroundSurfaceStyleProfile selectedStyle =
                    styles[Mathf.Clamp(
                        newSelectedIndex,
                        0,
                        styles.Length - 1)];

                serializedObject.ApplyModifiedProperties();
                ApplyToTargets(
                    "Select Ground Surface Family",
                    ground => ground.SetSurfaceStyleProfile(selectedStyle));
            }

            if (current == null && !surfaceStyleProfile.hasMultipleDifferentValues)
            {
                EditorGUILayout.HelpBox(
                    "No style is currently assigned. GeneratedGround will use the first valid discovered family after validation.",
                    MessageType.Info);
            }
        }

        private void DrawManualSurfaceStyleField()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(
                surfaceStyleProfile,
                new GUIContent(
                    "Surface Style Profile",
                    "Manual style asset fallback. Normal authoring should use the Surface Family dropdown when profiles are discoverable."));

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                ApplyToTargets(
                    "Change Ground Surface Style",
                    ground => ground.RefreshSurfaceStyleState());
            }
        }

        private void DrawStyleAssetDetails(
            GroundSurfaceStyleProfile style)
        {
            showStyleAssetDetails = EditorGUILayout.Foldout(
                showStyleAssetDetails,
                "Advanced Style Asset",
                true);

            if (!showStyleAssetDetails)
            {
                return;
            }

            EditorGUI.indentLevel++;

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(
                surfaceStyleProfile,
                new GUIContent(
                    "Style Asset",
                    "Direct asset reference for custom or externally stored style profiles."));

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                ApplyToTargets(
                    "Change Ground Surface Style",
                    ground => ground.RefreshSurfaceStyleState());
            }

            if (style != null)
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.ObjectField(
                        new GUIContent(
                            "Resolved Style Asset",
                            "The asset currently driving family and variant options."),
                        style,
                        typeof(GroundSurfaceStyleProfile),
                        false);
                }
            }

            EditorGUI.indentLevel--;
        }

        private void DrawStyleWarnings(
            GroundSurfaceStyleProfile style)
        {
            if (style == null)
            {
                EditorGUILayout.HelpBox(
                    "Missing surface family. Assign or create a GroundSurfaceStyleProfile asset.",
                    MessageType.Warning);
                return;
            }

            if (style.DefaultSurfaceProfile == null)
            {
                EditorGUILayout.HelpBox(
                    "The selected surface family has no default GroundSurfaceProfile. Generation will fall back to the local override/profile if available.",
                    MessageType.Warning);
            }

            if (style.Variants == null || style.Variants.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "The selected surface family has no variants.",
                    MessageType.Warning);
                return;
            }

            bool selectedVariantFound = false;
            string currentId = surfaceVariantId.stringValue;

            for (int index = 0; index < style.Variants.Count; index++)
            {
                GroundSurfaceVariantRecipe variant = style.Variants[index];

                if (variant == null || !variant.HasValidId)
                {
                    continue;
                }

                if (variant.Id == currentId)
                {
                    selectedVariantFound = true;
                    break;
                }
            }

            if (!selectedVariantFound &&
                !surfaceVariantId.hasMultipleDifferentValues)
            {
                EditorGUILayout.HelpBox(
                    "The stored variant id is not present in the selected family. The first valid variant will be used after validation.",
                    MessageType.Warning);
            }

            string duplicateId = FindDuplicateVariantId(style);

            if (!string.IsNullOrWhiteSpace(duplicateId))
            {
                EditorGUILayout.HelpBox(
                    $"The selected family contains duplicate variant id '{duplicateId}'. Variant ids must be stable and unique.",
                    MessageType.Warning);
            }

            DrawSelectedVariantFeatureWarnings(style, currentId);
        }

        private static void DrawSelectedVariantFeatureWarnings(
            GroundSurfaceStyleProfile style,
            string variantId)
        {
            GroundSurfaceVariantRecipe selectedVariant = null;

            for (int index = 0; index < style.Variants.Count; index++)
            {
                GroundSurfaceVariantRecipe candidate = style.Variants[index];
                if (candidate != null && candidate.Id == variantId)
                {
                    selectedVariant = candidate;
                    break;
                }
            }

            if (selectedVariant == null || selectedVariant.Features == null)
            {
                return;
            }

            System.Collections.Generic.HashSet<GroundSurfaceFeatureKind>
                seenRuntimeKinds =
                    new System.Collections.Generic.HashSet<
                        GroundSurfaceFeatureKind>();

            for (int index = 0;
                 index < selectedVariant.Features.Count;
                 index++)
            {
                GroundSurfaceFeatureRecipe feature =
                    selectedVariant.Features[index];

                if (feature == null)
                {
                    EditorGUILayout.HelpBox(
                        $"Feature entry {index + 1} is missing.",
                        MessageType.Warning);
                    continue;
                }

                if (!feature.Enabled)
                {
                    continue;
                }

                if (feature.Kind == GroundSurfaceFeatureKind.None)
                {
                    EditorGUILayout.HelpBox(
                        $"Feature entry {index + 1} is enabled but has kind None.",
                        MessageType.Warning);
                    continue;
                }

                if (feature.CostClass !=
                    GroundSurfaceFeatureCostClass.ShaderOnly)
                {
                    EditorGUILayout.HelpBox(
                        $"Enabled feature '{feature.Kind}' uses a reserved non-shader cost class and currently has no rendered output.",
                        MessageType.Info);
                    continue;
                }

                if (!IsCurrentlyRenderableShaderFeature(feature.Kind))
                {
                    EditorGUILayout.HelpBox(
                        $"Enabled feature '{feature.Kind}' is reserved but not currently rendered by the Ground shader feature stack.",
                        MessageType.Info);
                    continue;
                }

                if (feature.Strength <= 0f)
                {
                    EditorGUILayout.HelpBox(
                        $"Enabled feature '{feature.Kind}' has zero Strength and is ignored by runtime resolution.",
                        MessageType.Info);
                    continue;
                }

                if (!seenRuntimeKinds.Add(feature.Kind))
                {
                    EditorGUILayout.HelpBox(
                        $"The selected variant has multiple runtime-applicable '{feature.Kind}' recipes. Runtime uses the first applicable entry and ignores later duplicates.",
                        MessageType.Warning);
                }
            }
        }

        private static bool IsCurrentlyRenderableShaderFeature(
            GroundSurfaceFeatureKind kind)
        {
            return kind == GroundSurfaceFeatureKind.DirectionalStreaks ||
                kind == GroundSurfaceFeatureKind.PooledWetness ||
                kind == GroundSurfaceFeatureKind.PaintedAccentLines ||
                kind == GroundSurfaceFeatureKind.TrampledWear;
        }

        private void DrawGroundDebugControls()
        {
            if (!DrawSubsectionFoldout(
                    ref showGroundDebug,
                    "Ground Material Debug"))
            {
                return;
            }

            EditorGUI.indentLevel++;

            EditorGUILayout.HelpBox(
                "Ground debug views are applied through this GeneratedGround object's MaterialPropertyBlock. They do not require editing shared material assets and do not regenerate terrain.",
                MessageType.None);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(
                debugView,
                new GUIContent(
                    "Debug View",
                    "Renderer-local generated-ground debug view. Use None for normal rendering."));

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                ApplyToTargets(
                    "Change Generated Ground Debug View",
                    ground => ground.RefreshSurfaceMaterialProperties());
            }

            using (new EditorGUI.DisabledScope(
                       !debugView.hasMultipleDifferentValues &&
                       debugView.enumValueIndex == 0))
            {
                if (GUILayout.Button("Clear Debug View"))
                {
                    serializedObject.ApplyModifiedProperties();
                    ApplyToTargets(
                        "Clear Generated Ground Debug View",
                        ground => ground.ClearDebugView());
                }
            }

            EditorGUI.indentLevel--;
        }

        private void DrawCurrentRegenerationTimingControls()
        {
            if (!DrawSubsectionFoldout(
                    ref showCurrentRegenerationTiming,
                    "Current Regeneration Timing"))
            {
                return;
            }

            EditorGUI.indentLevel++;
            if (targets.Length != 1 || target is not GeneratedGround ground)
            {
                EditorGUILayout.HelpBox(
                    "Select one GeneratedGround to inspect its latest regeneration pass.",
                    MessageType.Info);
                EditorGUI.indentLevel--;
                return;
            }

            EditorGUILayout.HelpBox(
                ground.LastRegenerationTimingDiagnostics,
                MessageType.None);
            if (GUILayout.Button("Copy Current Regeneration Timing"))
            {
                EditorGUIUtility.systemCopyBuffer =
                    BuildRegenerationTimingClipboardReport(ground);
            }

            EditorGUILayout.LabelField(
                "Only stages executed by the latest pass are shown. Historical Painted Accent stage telemetry is retained separately below.",
                EditorStyles.wordWrappedMiniLabel);
            EditorGUI.indentLevel--;
        }

        private void DrawRegenerationAccountingControls()
        {
            if (!DrawSubsectionFoldout(
                    ref showRegenerationAccounting,
                    "Editor Regeneration Accounting"))
            {
                return;
            }

            EditorGUI.indentLevel++;
            if (targets.Length != 1 || target is not GeneratedGround ground)
            {
                EditorGUILayout.HelpBox(
                    "Select one GeneratedGround to inspect or copy its latest accounting batch.",
                    MessageType.Info);
                EditorGUI.indentLevel--;
                return;
            }

            EditorGUILayout.HelpBox(
                ground.LastEditorRegenerationAccountingReport,
                MessageType.None);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Copy Latest Accounting Batch"))
            {
                EditorGUIUtility.systemCopyBuffer =
                    ground.LastEditorRegenerationAccountingReport;
            }
            if (GUILayout.Button("Clear Accounting"))
            {
                ground.ClearEditorRegenerationAccounting();
                Repaint();
            }
            if (GUILayout.Button("Log Next Batch Once"))
            {
                ground.LogNextEditorRegenerationBatchOnce();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField(
                "Editor-only observational request/pass accounting. It does not change regeneration behavior.",
                EditorStyles.wordWrappedMiniLabel);
            EditorGUI.indentLevel--;
        }

        private static string BuildRegenerationTimingClipboardReport(
            GeneratedGround ground)
        {
            return
                "GeneratedGround Current Regeneration Timing\n" +
                ground.LastRegenerationTimingDiagnostics;
        }

        private static string BuildSurfaceMaskDiagnosticsClipboardReport(
            GeneratedGround ground)
        {
            return
                "GeneratedGround Last Surface Mask Diagnostics\n" +
                ground.LastSurfaceMaskDiagnostics;
        }

        private static string BuildPaintedAccentSurfaceStrokeTimingClipboardReport(
            GeneratedGround ground)
        {
            return
                "GeneratedGround Last Completed Painted Accent SurfaceStrokes Timing\n" +
                ground.LastCompletedPaintedAccentSurfaceStrokeTimingDiagnostics;
        }

        private static string BuildPaintedAccentProjectedGlyphTimingClipboardReport(
            GeneratedGround ground)
        {
            return
                "GeneratedGround Last Completed Painted Accent ProjectedGlyphs Timing\n" +
                ground.LastCompletedPaintedAccentProjectedGlyphTimingDiagnostics;
        }

        private static string BuildPaintedAccentCoverageTimingClipboardReport(
            GeneratedGround ground)
        {
            return
                "GeneratedGround Last Completed Painted Accent Coverage Timing\n" +
                ground.LastCompletedPaintedAccentCoverageTimingDiagnostics;
        }

        private static string BuildPaintedAccentPlacementClipboardReport(
            GeneratedGround ground)
        {
            return
                "GeneratedGround Painted Accent Placement Report\n" +
                ground.GetLastPaintedAccentPlacementStatistics();
        }

        private static string BuildPaintedAccentCoverageClipboardReport(
            GeneratedGround ground)
        {
            return
                "GeneratedGround Painted Accent Coverage Report\n" +
                ground.GetLastPaintedAccentCoverageStatistics();
        }

        private static string BuildPaintedAccentProjectedGlyphClipboardReport(
            GeneratedGround ground)
        {
            return
                "GeneratedGround Painted Accent Projected Baseline Report\n" +
                ground.GetLastPaintedAccentProjectedGlyphStatistics();
        }

        private static string BuildPaintedAccentGenerationDiagnosticsClipboardReport(
            GeneratedGround ground)
        {
            return
                BuildPaintedAccentSurfaceStrokeTimingClipboardReport(ground) +
                "\n\n" +
                BuildPaintedAccentPlacementClipboardReport(ground) +
                "\n\n" +
                BuildPaintedAccentProjectedGlyphTimingClipboardReport(ground) +
                "\n\n" +
                BuildPaintedAccentProjectedGlyphClipboardReport(ground) +
                "\n\n" +
                BuildPaintedAccentCoverageTimingClipboardReport(ground) +
                "\n\n" +
                BuildPaintedAccentCoverageClipboardReport(ground);
        }

        private static string BuildAllGroundDiagnosticsClipboardReport(
            GeneratedGround ground)
        {
            return
                BuildRegenerationTimingClipboardReport(ground) +
                "\n\n" +
                BuildSurfaceMaskDiagnosticsClipboardReport(ground) +
                "\n\n" +
                BuildPaintedAccentGenerationDiagnosticsClipboardReport(ground) +
                "\n\nGeneratedGround Editor Regeneration Accounting\n" +
                ground.LastEditorRegenerationAccountingReport;
        }

        private void DrawPaintedAccentStrokeControls()
        {
            if (targets.Length != 1)
            {
                if (DrawSectionFoldout(
                        ref showPaintedAccentStrokes,
                        "Painted Accents"))
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.HelpBox(
                        "Shared Painted Accent editing is disabled for multi-object selection. Select one GeneratedGround so the exact shared variant recipe is unambiguous.",
                        MessageType.Info);
                    EditorGUI.indentLevel--;
                }

                return;
            }

            GeneratedGround selectedGround = target as GeneratedGround;
            GroundSurfaceStyleProfile style =
                surfaceStyleProfile.objectReferenceValue as
                    GroundSurfaceStyleProfile;

            if (style == null ||
                string.IsNullOrWhiteSpace(surfaceVariantId.stringValue))
            {
                return;
            }

            SerializedObject styleObject = new SerializedObject(style);
            styleObject.Update();

            SerializedProperty feature =
                FindSelectedPaintedAccentFeatureProperty(
                    styleObject,
                    style,
                    surfaceVariantId.stringValue,
                    out int paintedAccentEntryCount,
                    out int runtimeApplicableCount,
                    out int firstPaintedAccentIndex,
                    out int runtimeFeatureIndex,
                    out int authoringFeatureIndex);

            if (feature == null)
            {
                if (DrawSectionFoldout(
                        ref showPaintedAccentStrokes,
                        "Painted Accents"))
                {
                    EditorGUI.indentLevel++;
                    DrawPaintedAccentResolutionWarnings(
                        paintedAccentEntryCount,
                        runtimeApplicableCount,
                        firstPaintedAccentIndex,
                        runtimeFeatureIndex,
                        authoringFeatureIndex);
                    EditorGUI.indentLevel--;
                }

                return;
            }

            SerializedProperty enabled =
                feature.FindPropertyRelative("enabled");
            SerializedProperty costClass =
                feature.FindPropertyRelative("costClass");
            SerializedProperty strength =
                feature.FindPropertyRelative("strength");
            SerializedProperty maskInfluence =
                feature.FindPropertyRelative("maskInfluence");
            SerializedProperty seedOffset =
                feature.FindPropertyRelative("seedOffset");
            SerializedProperty strokeWidth =
                feature.FindPropertyRelative("paintedAccentStrokeWidth");
            SerializedProperty strokeDensity =
                feature.FindPropertyRelative("paintedAccentStrokeDensity");
            SerializedProperty distributionPatchScale =
                feature.FindPropertyRelative("paintedAccentDistributionPatchScale");
            SerializedProperty distributionPatchiness =
                feature.FindPropertyRelative("paintedAccentDistributionPatchiness");
            SerializedProperty horizontalCompanionStrength =
                feature.FindPropertyRelative("paintedAccentHorizontalCompanionStrength");
            SerializedProperty companionTripletShare =
                feature.FindPropertyRelative("paintedAccentCompanionTripletShare");
            SerializedProperty companionAccentBias =
                feature.FindPropertyRelative("paintedAccentCompanionAccentBias");
            SerializedProperty companionTightness =
                feature.FindPropertyRelative("paintedAccentCompanionTightness");
            SerializedProperty companionTripletVerticality =
                feature.FindPropertyRelative("paintedAccentCompanionTripletVerticality");
            SerializedProperty companionTripletVerticalityInitialized =
                feature.FindPropertyRelative("paintedAccentCompanionTripletVerticalityInitialized");
            SerializedProperty horizontalCompanionsInitialized =
                feature.FindPropertyRelative("paintedAccentHorizontalCompanionsInitialized");
            SerializedProperty companionQuotaControlsInitialized =
                feature.FindPropertyRelative("paintedAccentCompanionQuotaControlsInitialized");
            SerializedProperty pairSteppedWeight =
                feature.FindPropertyRelative("paintedAccentPairSteppedWeight");
            SerializedProperty pairShoulderWeight =
                feature.FindPropertyRelative("paintedAccentPairShoulderWeight");
            SerializedProperty pairOffsetWeight =
                feature.FindPropertyRelative("paintedAccentPairOffsetWeight");
            SerializedProperty pairShallowWeight =
                feature.FindPropertyRelative("paintedAccentPairShallowWeight");
            SerializedProperty tripletSteppedRunWeight =
                feature.FindPropertyRelative("paintedAccentTripletSteppedRunWeight");
            SerializedProperty tripletCrownRunWeight =
                feature.FindPropertyRelative("paintedAccentTripletCrownRunWeight");
            SerializedProperty tripletBrokenTerraceWeight =
                feature.FindPropertyRelative("paintedAccentTripletBrokenTerraceWeight");
            SerializedProperty tripletShallowRunWeight =
                feature.FindPropertyRelative("paintedAccentTripletShallowRunWeight");
            SerializedProperty companionLayoutWeightsInitialized =
                feature.FindPropertyRelative("paintedAccentCompanionLayoutWeightsInitialized");
            SerializedProperty completeMoundWeight =
                feature.FindPropertyRelative("paintedAccentCompleteMoundWeight");
            SerializedProperty asymmetricMoundWeight =
                feature.FindPropertyRelative("paintedAccentAsymmetricMoundWeight");
            SerializedProperty singleShoulderWeight =
                feature.FindPropertyRelative("paintedAccentSingleShoulderWeight");
            SerializedProperty shallowCrestWeight =
                feature.FindPropertyRelative("paintedAccentShallowCrestWeight");
            SerializedProperty familyWeightsInitialized =
                feature.FindPropertyRelative("paintedAccentGlyphFamilyWeightsInitialized");
            SerializedProperty strokeLengthMin =
                feature.FindPropertyRelative("paintedAccentStrokeLengthMin");
            SerializedProperty strokeLengthMax =
                feature.FindPropertyRelative("paintedAccentStrokeLengthMax");
            SerializedProperty strokeFacingDirectionDegrees =
                feature.FindPropertyRelative("paintedAccentStrokeFacingDirectionDegrees");
            SerializedProperty strokeAngleJitterDegrees =
                feature.FindPropertyRelative("paintedAccentStrokeAngleJitterDegrees");
            SerializedProperty strokePathWiggle =
                feature.FindPropertyRelative("paintedAccentStrokePathWiggle");
            SerializedProperty strokePathWiggleInitialized =
                feature.FindPropertyRelative("paintedAccentStrokePathWiggleInitialized");
            SerializedProperty foldHeight =
                feature.FindPropertyRelative("paintedAccentFoldHeight");
            SerializedProperty crestCrownHeight =
                feature.FindPropertyRelative("paintedAccentCrestCrownHeight");
            SerializedProperty foldIrregularity =
                feature.FindPropertyRelative("paintedAccentFoldIrregularity");
            SerializedProperty foldEndTaper =
                feature.FindPropertyRelative("paintedAccentFoldEndTaper");
            SerializedProperty inkColor =
                feature.FindPropertyRelative("paintedAccentInkColor");
            SerializedProperty inkOpacity =
                feature.FindPropertyRelative("paintedAccentInkOpacity");
            SerializedProperty inkOpacityInitialized =
                feature.FindPropertyRelative("paintedAccentInkOpacityInitialized");

            if (enabled == null ||
                costClass == null ||
                strength == null ||
                maskInfluence == null ||
                seedOffset == null ||
                strokeWidth == null ||
                strokeDensity == null ||
                distributionPatchScale == null ||
                distributionPatchiness == null ||
                horizontalCompanionStrength == null ||
                companionTripletShare == null ||
                companionAccentBias == null ||
                companionTightness == null ||
                companionTripletVerticality == null ||
                companionTripletVerticalityInitialized == null ||
                horizontalCompanionsInitialized == null ||
                companionQuotaControlsInitialized == null ||
                pairSteppedWeight == null ||
                pairShoulderWeight == null ||
                pairOffsetWeight == null ||
                pairShallowWeight == null ||
                tripletSteppedRunWeight == null ||
                tripletCrownRunWeight == null ||
                tripletBrokenTerraceWeight == null ||
                tripletShallowRunWeight == null ||
                companionLayoutWeightsInitialized == null ||
                completeMoundWeight == null ||
                asymmetricMoundWeight == null ||
                singleShoulderWeight == null ||
                shallowCrestWeight == null ||
                familyWeightsInitialized == null ||
                strokeLengthMin == null ||
                strokeLengthMax == null ||
                strokeFacingDirectionDegrees == null ||
                strokeAngleJitterDegrees == null ||
                strokePathWiggle == null ||
                strokePathWiggleInitialized == null ||
                foldHeight == null ||
                crestCrownHeight == null ||
                foldIrregularity == null ||
                foldEndTaper == null ||
                inkColor == null ||
                inkOpacity == null ||
                inkOpacityInitialized == null)
            {
                return;
            }

            bool styleChanged = false;
            bool paintedAccentAuthoringNeedsInitialization =
                !horizontalCompanionsInitialized.boolValue ||
                !companionTripletVerticalityInitialized.boolValue ||
                !companionQuotaControlsInitialized.boolValue ||
                !companionLayoutWeightsInitialized.boolValue ||
                !familyWeightsInitialized.boolValue ||
                !strokePathWiggleInitialized.boolValue;

            bool expanded = DrawSectionFoldout(
                ref showPaintedAccentStrokes,
                "Painted Accents");

            if (expanded)
            {
                EditorGUI.indentLevel++;

                GroundSurfaceVariantRecipe selectedVariant =
                    ResolveSelectedVariant(style, surfaceVariantId.stringValue);
                DrawSharedVariantAuthoringScope(style, selectedVariant, false);

                EditorGUILayout.HelpBox(
                    "Edits the first runtime-applicable Painted Accent recipe. If none is currently applicable, it edits the first matching recipe so Enabled, Execution Path, and Stroke Intensity can restore it. Stroke Intensity and shape controls rebuild procedural coverage; Ink Colour and Ink Opacity are material-only.",
                    MessageType.None);

                DrawPaintedAccentResolutionWarnings(
                    paintedAccentEntryCount,
                    runtimeApplicableCount,
                    firstPaintedAccentIndex,
                    runtimeFeatureIndex,
                    authoringFeatureIndex);

                if (paintedAccentAuthoringNeedsInitialization)
                {
                    EditorGUILayout.HelpBox(
                        "This recipe still relies on compatibility defaults. The Inspector no longer writes those defaults merely by being drawn. Initialize them explicitly before editing Painted Accent-specific values.",
                        MessageType.Warning);

                    if (GUILayout.Button(
                            "Initialize Painted Accent Authoring Values"))
                    {
                        Undo.RecordObject(
                            style,
                            "Initialize Painted Accent Authoring Values");

                        if (!horizontalCompanionsInitialized.boolValue)
                        {
                            horizontalCompanionStrength.floatValue = 1f;
                            companionTightness.floatValue = 1f;
                            horizontalCompanionsInitialized.boolValue = true;
                        }

                        if (!companionTripletVerticalityInitialized.boolValue)
                        {
                            companionTripletVerticality.floatValue = 1f;
                            companionTripletVerticalityInitialized.boolValue = true;
                        }

                        if (!companionQuotaControlsInitialized.boolValue)
                        {
                            companionTripletShare.floatValue = 0.40f;
                            companionAccentBias.floatValue = 0.99f;
                            companionQuotaControlsInitialized.boolValue = true;
                        }

                        if (!companionLayoutWeightsInitialized.boolValue)
                        {
                            pairSteppedWeight.floatValue = 0.55f;
                            pairShoulderWeight.floatValue = 0.45f;
                            pairOffsetWeight.floatValue = 0.50f;
                            pairShallowWeight.floatValue = 0.05f;
                            tripletSteppedRunWeight.floatValue = 0.40f;
                            tripletCrownRunWeight.floatValue = 0.30f;
                            tripletBrokenTerraceWeight.floatValue = 0.25f;
                            tripletShallowRunWeight.floatValue = 0.05f;
                            companionLayoutWeightsInitialized.boolValue = true;
                        }

                        if (!familyWeightsInitialized.boolValue)
                        {
                            completeMoundWeight.floatValue = 0.50f;
                            asymmetricMoundWeight.floatValue = 0.40f;
                            singleShoulderWeight.floatValue = 0.50f;
                            shallowCrestWeight.floatValue = 0.50f;
                            familyWeightsInitialized.boolValue = true;
                        }

                        if (!strokePathWiggleInitialized.boolValue)
                        {
                            strokePathWiggle.floatValue = 0.85f;
                            strokePathWiggleInitialized.boolValue = true;
                        }

                        styleObject.ApplyModifiedProperties();
                        EditorUtility.SetDirty(style);
                        paintedAccentPlacementDebugSignature = int.MinValue;
                        RefreshLoadedGroundsUsingStyleVariant(
                            style,
                            surfaceVariantId.stringValue,
                            true);
                    }

                    EditorGUI.indentLevel--;
                    return;
                }

                if (DrawSubsectionFoldout(
                        ref showPaintedAccentBasics,
                        "Enable and Visibility"))
                {
                    EditorGUI.indentLevel++;
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(
                        enabled,
                        new GUIContent(
                            "Enable Painted Accents",
                            "Disables Painted Accents while preserving the shared recipe values."));
                    EditorGUILayout.PropertyField(
                        costClass,
                        new GUIContent(
                            "Execution Path",
                            "Shader Only is the current Painted Accent path. Other values are reserved and do not render."));
                    EditorGUILayout.Slider(
                        strength,
                        0f,
                        1f,
                        new GUIContent(
                            "Stroke Intensity",
                            "Controls generated per-stroke strength and slight projected-profile amplitude. Zero makes the recipe runtime-inactive. This changes procedural coverage and is not Ink Opacity."));
                    EditorGUILayout.PropertyField(
                        inkColor,
                        new GUIContent(
                            "Ink Colour",
                            "Family/variant-authored line colour. Material-only update; does not rebuild placement, projected glyphs, or coverage."));
                    styleChanged |= EditorGUI.EndChangeCheck();
                    styleChanged |= DrawPaintedAccentInkOpacityControl(
                        inkOpacity,
                        inkOpacityInitialized);
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.Slider(
                        strokeWidth,
                        0.002f,
                        0.20f,
                        new GUIContent(
                            "Stroke Width (m)",
                            "Authored projected-contour width in metres. This affects placement validation and coverage, so changing it rebuilds Painted Accents."));
                    styleChanged |= EditorGUI.EndChangeCheck();

                    if (target is GeneratedGround generatedGround)
                    {
                        DrawPaintedAccentVisibilityStatus(generatedGround);
                    }

                    EditorGUI.indentLevel--;
                }

                if (DrawSubsectionFoldout(
                        ref showPaintedAccentDistribution,
                        "Distribution"))
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.HelpBox(
                        "Scale controls the size of sparse/dense structure. Contrast controls how strongly the field separates into populated and quiet areas. Cluster Region Bias only decides where the fixed companion quota is concentrated.",
                        MessageType.None);
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.Slider(
                        maskInfluence,
                        0f,
                        1f,
                        new GUIContent(
                            "Surface Suitability Influence",
                            "How strongly generated semantic Ground masks gate Painted Accent placement."));
                    EditorGUILayout.PropertyField(
                        seedOffset,
                        new GUIContent(
                            "Pattern Seed Offset",
                            "Stable Painted Accent seed offset mixed with the Ground seed."));
                    EditorGUILayout.Slider(
                        strokeDensity,
                        0f,
                        2000f,
                        new GUIContent(
                            "Stroke Density",
                            "Approximate requested stroke proposals per standard 40x40 ground patch. Physical validation may reduce the final count."));
                    EditorGUILayout.Slider(
                        distributionPatchScale,
                        2f,
                        24f,
                        new GUIContent(
                            "Distribution Scale",
                            "Lower values create smaller, more frequent variation. Higher values create broader local patches and larger coherent regions."));
                    EditorGUILayout.Slider(
                        distributionPatchiness,
                        0f,
                        1f,
                        new GUIContent(
                            "Distribution Contrast",
                            "Zero approaches an even field. One creates strong sparse-versus-dense separation while retaining a protected non-zero sparse-region floor."));
                    using (new EditorGUI.DisabledScope(
                               !horizontalCompanionStrength.hasMultipleDifferentValues &&
                               horizontalCompanionStrength.floatValue <= 0f))
                    {
                        EditorGUILayout.Slider(
                            companionAccentBias,
                            0f,
                            1f,
                            new GUIContent(
                                "Cluster Region Bias",
                                "Zero distributes clusters like the overall field. One concentrates the same fixed cluster quota into denser accent regions."));
                    }
                    styleChanged |= EditorGUI.EndChangeCheck();
                    EditorGUI.indentLevel--;
                }

                if (DrawSubsectionFoldout(
                        ref showPaintedAccentHorizontalCompanions,
                        "Companion Composition"))
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.HelpBox(
                        "Participation, pair/triplet split, and layout weights resolve to deterministic whole-mark quotas after ordinary projected validation. Shape controls cannot silently reduce those quotas.",
                        MessageType.None);
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.Slider(
                        horizontalCompanionStrength,
                        0f,
                        1f,
                        new GUIContent(
                            "Companion Participation",
                            "Authoritative target share of final valid projected marks assigned to complete pairs or triplets."));
                    using (new EditorGUI.DisabledScope(
                               !horizontalCompanionStrength.hasMultipleDifferentValues &&
                               horizontalCompanionStrength.floatValue <= 0f))
                    {
                        EditorGUILayout.Slider(
                            companionTripletShare,
                            0f,
                            1f,
                            new GUIContent(
                                "Triplet Share",
                                "Of clustered participants, the authoritative target share assigned to triplets. The remainder is assigned to pairs."));
                        EditorGUILayout.Slider(
                            companionTightness,
                            0f,
                            1f,
                            new GUIContent(
                                "Companion Tightness",
                                "Junction spacing only. One stops terminal endpoints at the visible edge of the contacted mark without overlap."));
                        EditorGUILayout.Slider(
                            companionTripletVerticality,
                            0f,
                            1f,
                            new GUIContent(
                                "Cluster Verticality",
                                "Translation-driven stepping for pairs and triplets. This does not change cluster counts or Angle Jitter."));
                    }
                    styleChanged |= EditorGUI.EndChangeCheck();
                    if (!serializedObject.isEditingMultipleObjects &&
                        target is GeneratedGround generatedGround)
                    {
                        EditorGUILayout.HelpBox(
                            generatedGround.GetLastPaintedAccentCompanionQuotaSummary(),
                            MessageType.None);
                    }
                    EditorGUI.indentLevel--;
                }

                if (DrawSubsectionFoldout(
                        ref showPaintedAccentAdvancedCompanionLayoutMix,
                        "Advanced Companion Layout Mix"))
                {
                    EditorGUI.indentLevel++;
                    using (new EditorGUI.DisabledScope(
                               !horizontalCompanionStrength.hasMultipleDifferentValues &&
                               horizontalCompanionStrength.floatValue <= 0f))
                    {
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.LabelField("Pair Layout Weights", EditorStyles.miniBoldLabel);
                        EditorGUILayout.Slider(pairSteppedWeight, 0f, 1f, new GUIContent("Stepped", "Exact normalized quota weight for stepped pairs."));
                        EditorGUILayout.Slider(pairShoulderWeight, 0f, 1f, new GUIContent("Shoulder", "Exact normalized quota weight for shoulder/interior-contact pairs."));
                        EditorGUILayout.Slider(pairOffsetWeight, 0f, 1f, new GUIContent("Offset", "Exact normalized quota weight for offset pairs."));
                        EditorGUILayout.Slider(pairShallowWeight, 0f, 1f, new GUIContent("Shallow Offset", "Exact normalized quota weight for quieter visibly separated pairs."));
                        EditorGUILayout.LabelField("Triplet Layout Weights", EditorStyles.miniBoldLabel);
                        EditorGUILayout.Slider(tripletSteppedRunWeight, 0f, 1f, new GUIContent("Stepped Run", "Exact normalized quota weight for rising/falling stepped runs."));
                        EditorGUILayout.Slider(tripletCrownRunWeight, 0f, 1f, new GUIContent("Crown Run", "Exact normalized quota weight for centre-raised/lowered triplets."));
                        EditorGUILayout.Slider(tripletBrokenTerraceWeight, 0f, 1f, new GUIContent("Broken Terrace", "Exact normalized quota weight for alternating terrace triplets."));
                        EditorGUILayout.Slider(tripletShallowRunWeight, 0f, 1f, new GUIContent("Shallow Run", "Exact normalized quota weight for quieter non-collinear triplets."));
                        styleChanged |= EditorGUI.EndChangeCheck();
                    }
                    EditorGUI.indentLevel--;
                }

                if (DrawSubsectionFoldout(
                        ref showPaintedAccentFamilyMix,
                        "Glyph Family Mix"))
                {
                    EditorGUI.indentLevel++;
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.Slider(
                        completeMoundWeight,
                        0f,
                        1f,
                        new GUIContent(
                            "Complete Mound Weight",
                            "Relative weight for the accepted two-sided mound family. Values are normalized against the other family weights."));
                    EditorGUILayout.Slider(
                        asymmetricMoundWeight,
                        0f,
                        1f,
                        new GUIContent(
                            "Asymmetric Mound Weight",
                            "Relative weight for strongly unequal two-sided mound silhouettes. Values are normalized internally."));
                    EditorGUILayout.Slider(
                        singleShoulderWeight,
                        0f,
                        1f,
                        new GUIContent(
                            "Single Shoulder Weight",
                            "Relative weight for open one-sided shoulder silhouettes. Values are normalized internally."));
                    EditorGUILayout.Slider(
                        shallowCrestWeight,
                        0f,
                        1f,
                        new GUIContent(
                            "Shallow Crest Weight",
                            "Relative weight for low predominantly lateral crest silhouettes. Values are normalized internally."));
                    styleChanged |= EditorGUI.EndChangeCheck();
                    EditorGUI.indentLevel--;
                }

                if (DrawSubsectionFoldout(
                        ref showPaintedAccentGeometry,
                        "Stroke Geometry"))
                {
                    EditorGUI.indentLevel++;
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.Slider(
                        strokeLengthMin,
                        0.20f,
                        4.0f,
                        new GUIContent(
                            "Stroke Length Min",
                            "Minimum accepted ground-surface descriptor length in metres."));
                    EditorGUILayout.Slider(
                        strokeLengthMax,
                        0.25f,
                        6.0f,
                        new GUIContent(
                            "Stroke Length Max",
                            "Maximum accepted ground-surface descriptor length in metres."));
                    EditorGUILayout.Slider(
                        strokeFacingDirectionDegrees,
                        0f,
                        360f,
                        new GUIContent(
                            "Facing Direction Degrees",
                            "Local X/Z orientation reference. Accepted descriptor strokes are perpendicular to this direction before signed Angle Jitter is applied."));
                    EditorGUILayout.Slider(
                        strokeAngleJitterDegrees,
                        0f,
                        30f,
                        new GUIContent(
                            "Angle Jitter Degrees",
                            "Maximum signed angle offset around the perpendicular stroke angle."));
                    EditorGUILayout.Slider(
                        strokePathWiggle,
                        0f,
                        1f,
                        new GUIContent(
                            "Stroke Path Wiggle",
                            "Smooth lateral curvature of the ground-surface stroke path. This does not alter Profile Irregularity or family height."));
                    styleChanged |= EditorGUI.EndChangeCheck();
                    EditorGUI.indentLevel--;
                }

                if (DrawSubsectionFoldout(
                        ref showPaintedAccentProfile,
                        "Projected Contour Profile"))
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.HelpBox(
                        "The mesh-free projected contour applies its solved scalar profile toward fixed world +Z, which is permanent gameplay screen-up.",
                        MessageType.None);
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.Slider(
                        foldHeight,
                        0f,
                        0.50f,
                        new GUIContent(
                            "Profile Height",
                            "Primary projected contour amplitude in metres, applied toward fixed world +Z."));
                    EditorGUILayout.Slider(
                        crestCrownHeight,
                        0f,
                        0.05f,
                        new GUIContent(
                            "Crest Crown Height",
                            "Additional projected crest/cap amplitude added directly to fixed world +Z displacement."));
                    EditorGUILayout.Slider(
                        foldIrregularity,
                        0f,
                        1f,
                        new GUIContent(
                            "Profile Irregularity",
                            "Seeded longitudinal variation in the projected contour silhouette."));
                    EditorGUILayout.Slider(
                        foldEndTaper,
                        0f,
                        1f,
                        new GUIContent(
                            "End Taper",
                            "Projected contour and visible-width endpoint envelope."));
                    styleChanged |= EditorGUI.EndChangeCheck();
                    EditorGUI.indentLevel--;
                }

                EditorGUI.indentLevel--;
            }

            if (strokeLengthMax.floatValue < strokeLengthMin.floatValue + 0.05f)
            {
                EditorGUILayout.HelpBox(
                    "Stroke Length Max is below the minimum valid separation. Runtime currently resolves it to Stroke Length Min + 0.05 m; edit the value explicitly to remove this compatibility correction.",
                    MessageType.Warning);
            }

            if (styleChanged)
            {
                styleObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(style);
                paintedAccentPlacementDebugSignature = int.MinValue;
                RefreshLoadedGroundsUsingStyleVariant(
                    style,
                    surfaceVariantId.stringValue,
                    true);
            }

            if (expanded)
            {
                EditorGUI.indentLevel++;
                DrawPaintedAccentPreviewAndProductionControls(
                    selectedGround);
                EditorGUI.indentLevel--;
            }
        }

        private void DrawPaintedAccentPreviewAndProductionControls(
            GeneratedGround ground)
        {
            if (ground == null ||
                !DrawSubsectionFoldout(
                    ref showPaintedAccentPreviewAndProduction,
                    "Preview and Production"))
            {
                return;
            }

            EditorGUI.indentLevel++;
            GroundPaintedAccentProductionBakeDiagnostics diagnostics =
                ground.GetPaintedAccentProductionBakeDiagnostics();
            bool duplicateIdentifier =
                GroundPaintedAccentProductionBaker
                    .HasDuplicateIdentifier(ground);
            bool ownershipMismatch =
                GroundPaintedAccentProductionBaker
                    .HasOwnershipMismatch(ground);
            GroundPaintedAccentProductionBakeStatus productionStatus =
                duplicateIdentifier || ownershipMismatch
                    ? GroundPaintedAccentProductionBakeStatus.Incompatible
                    : diagnostics.ProductionStatus;
            if (Application.isPlaying)
            {
                EditorGUILayout.LabelField(
                    "Edit Preview",
                    "Suspended during Play Mode");
                EditorGUILayout.LabelField(
                    "Renderer Source",
                    "Persistent production coverage (PA-B2)");
                EditorGUILayout.LabelField(
                    "Runtime Coverage",
                    FormatRuntimeCoverageStatus(
                        ground.PaintedAccentRuntimeCoverageStatus));
            }
            else
            {
                EditorGUILayout.LabelField(
                    "Live Preview",
                    FormatLivePreviewStatus(diagnostics.LivePreviewStatus));
                EditorGUILayout.LabelField(
                    "Renderer Source",
                    "Live procedural preview (Edit Mode)");
            }
            EditorGUILayout.LabelField(
                Application.isPlaying
                    ? "Production Artifact"
                    : "Production Bake",
                Application.isPlaying
                    ? FormatRuntimeProductionArtifactStatus(productionStatus)
                    : FormatProductionBakeStatus(productionStatus));

            string assetPath =
                diagnostics.ProductionTexture != null
                    ? AssetDatabase.GetAssetPath(
                        diagnostics.ProductionTexture)
                    : string.Empty;
            if (!string.IsNullOrWhiteSpace(assetPath))
            {
                EditorGUILayout.LabelField(
                    "Production Asset",
                    assetPath,
                    EditorStyles.wordWrappedMiniLabel);
                EditorGUILayout.LabelField(
                    "Production Resolution",
                    $"{diagnostics.ProductionTexture.width} × " +
                    $"{diagnostics.ProductionTexture.height} R8");
                EditorGUILayout.LabelField(
                    "Covered Texels",
                    $"{diagnostics.CoveredTexelCount:N0} " +
                    $"({diagnostics.CoveredTexelFraction * 100f:F3}%)");
            }

            if (duplicateIdentifier)
            {
                EditorGUILayout.HelpBox(
                    "This Ground shares a generated-output identifier with another loaded Ground, usually because the object was duplicated. The next bake will assign this Ground a new identifier and output asset instead of overwriting the shared texture.",
                    MessageType.Warning);
            }

            if (ownershipMismatch)
            {
                EditorGUILayout.HelpBox(
                    "The stored production texture belongs to a different scene or generated-output path. This commonly occurs after copying a scene. Rebake to create an output owned by the current scene without overwriting the original.",
                    MessageType.Error);
            }

            switch (productionStatus)
            {
                case GroundPaintedAccentProductionBakeStatus.Missing:
                    EditorGUILayout.HelpBox(
                        "No persistent Painted Accent production coverage exists yet.",
                        MessageType.Info);
                    break;

                case GroundPaintedAccentProductionBakeStatus.Stale:
                    EditorGUILayout.HelpBox(
                        "The persistent coverage does not match the current geometry, placement, eligibility, cluster, profile, or coverage inputs. Ink Colour and Ink Opacity do not make this bake stale.",
                        MessageType.Warning);
                    break;

                case GroundPaintedAccentProductionBakeStatus.Incompatible:
                    if (!duplicateIdentifier && !ownershipMismatch)
                    {
                        EditorGUILayout.HelpBox(
                            "The stored production output uses an incompatible bake contract, texture format, or mapping record. Rebake it.",
                            MessageType.Error);
                    }
                    break;
            }

            if (!Application.isPlaying &&
                diagnostics.LivePreviewStatus !=
                    GroundPaintedAccentLivePreviewStatus.Current)
            {
                EditorGUILayout.HelpBox(
                    "Bake will first regenerate the Ground and rebuild the live Painted Accent preview so the persistent output is sourced from current authoritative coverage.",
                    MessageType.Info);
            }

            if (Application.isPlaying &&
                ground.PaintedAccentRuntimeCoverageStatus !=
                    GroundPaintedAccentRuntimeCoverageStatus.Current &&
                ground.PaintedAccentRuntimeCoverageStatus !=
                    GroundPaintedAccentRuntimeCoverageStatus.NotRequired)
            {
                EditorGUILayout.HelpBox(
                    string.IsNullOrWhiteSpace(
                        ground.PaintedAccentRuntimeCoverageFailureReason)
                        ? "Runtime production coverage is unavailable. No procedural fallback is active."
                        : ground.PaintedAccentRuntimeCoverageFailureReason,
                    MessageType.Error);
            }

            using (new EditorGUI.DisabledScope(
                       Application.isPlaying ||
                       EditorUtility.IsPersistent(ground)))
            {
                if (GUILayout.Button("Validate Production Bake"))
                {
                    GroundPaintedAccentProductionValidator
                        .ShowGroundValidationDialog(ground);
                    serializedObject.Update();
                    Repaint();
                }

                if (GUILayout.Button("Bake Painted Accents"))
                {
                    bool baked =
                        GroundPaintedAccentProductionBaker.Bake(
                            ground,
                            out string message);
                    EditorUtility.DisplayDialog(
                        baked
                            ? "Painted Accent Bake Complete"
                            : "Painted Accent Bake Failed",
                        message,
                        "OK");
                    serializedObject.Update();
                    Repaint();
                }

                using (new EditorGUI.DisabledScope(
                           diagnostics.ProductionTexture == null &&
                           string.IsNullOrWhiteSpace(
                               diagnostics.BakeIdentifier) &&
                           string.IsNullOrWhiteSpace(
                               diagnostics.StoredCoverageSignature) &&
                           diagnostics.StoredFormatRevision == 0))
                {
                    if (GUILayout.Button("Release Production Bake"))
                    {
                        bool release = EditorUtility.DisplayDialog(
                            "Release Painted Accent Production Bake?",
                            "This clears the Ground's production texture reference, identifier, signature, mapping, and bake metadata. The generated asset is not deleted. Save the scene, then run Audit and Clean Painted Accent Assets to remove it once no project reference remains.",
                            "Release",
                            "Cancel");
                        if (release)
                        {
                            Undo.RecordObject(
                                ground,
                                "Release Painted Accent Production Bake");
                            ground
                                .EditorReleasePaintedAccentProductionBake();
                            EditorUtility.SetDirty(ground);
                            serializedObject.Update();
                            SceneView.RepaintAll();
                            Repaint();
                        }
                    }
                }
            }

            EditorGUILayout.HelpBox(
                "Validate Production Bake refreshes authoritative Edit Mode coverage and compares it without writing an asset. Release Production Bake clears this Ground's serialized ownership but deliberately leaves the generated texture for the project-wide cleanup audit. Player builds render only from the persistent R8 output, and PA-B3 blocks builds when required production coverage is invalid. No procedural runtime fallback is executed.",
                MessageType.None);
            EditorGUI.indentLevel--;
        }

        private static string FormatLivePreviewStatus(
            GroundPaintedAccentLivePreviewStatus status)
        {
            return status switch
            {
                GroundPaintedAccentLivePreviewStatus.Current => "Current",
                GroundPaintedAccentLivePreviewStatus.Stale => "Stale",
                _ => "Missing"
            };
        }

        private static string FormatProductionBakeStatus(
            GroundPaintedAccentProductionBakeStatus status)
        {
            return status switch
            {
                GroundPaintedAccentProductionBakeStatus.Current => "Current",
                GroundPaintedAccentProductionBakeStatus.Stale => "Stale",
                GroundPaintedAccentProductionBakeStatus.Incompatible =>
                    "Incompatible",
                _ => "Missing"
            };
        }

        private static string FormatRuntimeProductionArtifactStatus(
            GroundPaintedAccentProductionBakeStatus status)
        {
            return status switch
            {
                GroundPaintedAccentProductionBakeStatus.Current =>
                    "Available (structural validation)",
                GroundPaintedAccentProductionBakeStatus.Incompatible =>
                    "Incompatible",
                GroundPaintedAccentProductionBakeStatus.Stale => "Stale",
                _ => "Missing"
            };
        }

        private static string FormatRuntimeCoverageStatus(
            GroundPaintedAccentRuntimeCoverageStatus status)
        {
            return status switch
            {
                GroundPaintedAccentRuntimeCoverageStatus.Current => "Current",
                GroundPaintedAccentRuntimeCoverageStatus.NotRequired =>
                    "Not required (feature disabled)",
                GroundPaintedAccentRuntimeCoverageStatus.Missing => "Missing",
                GroundPaintedAccentRuntimeCoverageStatus.Incompatible =>
                    "Incompatible",
                _ => "Not evaluated"
            };
        }

        private static bool DrawPaintedAccentInkOpacityControl(
            SerializedProperty opacity,
            SerializedProperty initialized)
        {
            float resolvedOpacity =
                initialized.boolValue
                    ? Mathf.Clamp01(opacity.floatValue)
                    : 1f;

            EditorGUI.BeginChangeCheck();
            float authoredOpacity = EditorGUILayout.Slider(
                new GUIContent(
                    "Ink Opacity",
                    "Material-only albedo blend after coverage generation. Increasing it makes lines more visible without rebuilding placement, projected glyphs, or coverage."),
                resolvedOpacity,
                0f,
                1f);
            bool changed = EditorGUI.EndChangeCheck();

            if (changed)
            {
                opacity.floatValue = authoredOpacity;
                initialized.boolValue = true;
            }
            else if (!initialized.boolValue)
            {
                EditorGUILayout.HelpBox(
                    "This existing recipe uses the GI-A3 compatibility opacity of 1.00. Moving Ink Opacity records an explicit authored value; merely viewing this Inspector does not mutate the asset.",
                    MessageType.Info);
            }

            return changed;
        }

        private static void DrawPaintedAccentVisibilityStatus(
            GeneratedGround ground)
        {
            GroundPaintedAccentVisibilityDiagnostics diagnostics =
                ground.GetPaintedAccentVisibilityDiagnostics();

            EditorGUILayout.Space(3f);
            EditorGUILayout.LabelField(
                "Resolved Visibility",
                EditorStyles.miniBoldLabel);

            if (!diagnostics.HasRuntimeFeature)
            {
                EditorGUILayout.HelpBox(
                    "No runtime-applicable Painted Accent recipe currently resolves. Enable the recipe, select Shader Only, and keep Stroke Intensity above zero.",
                    MessageType.Warning);
                return;
            }

            EditorGUILayout.LabelField(
                "Coverage",
                diagnostics.CoverageGenerated
                    ? $"{diagnostics.TextureWidth} × {diagnostics.TextureHeight} R8"
                    : "Not generated");
            EditorGUILayout.LabelField(
                "Coverage Binding",
                diagnostics.MaterialBindingCurrent
                    ? "Current"
                    : "Stale or incomplete");
            EditorGUILayout.LabelField(
                "Coverage Mapping",
                diagnostics.CoverageMappingMatchesMeshBounds
                    ? "Matches generated mesh bounds"
                    : "Does not match generated mesh bounds");
            EditorGUILayout.LabelField(
                "World Texel Size",
                diagnostics.MaximumTexelWorldSize > 0f
                    ? $"{diagnostics.MaximumTexelWorldSize:F5} m"
                    : "Unavailable");
            EditorGUILayout.LabelField(
                "Authored Width",
                diagnostics.MaximumTexelWorldSize > 0f
                    ? $"{diagnostics.AuthoredStrokeWidth:F5} m ({diagnostics.AuthoredWidthInTexels:F2} texels)"
                    : $"{diagnostics.AuthoredStrokeWidth:F5} m");
            EditorGUILayout.LabelField(
                "Ink Opacity",
                $"{diagnostics.InkOpacity * 100f:F0}%");
            EditorGUILayout.LabelField(
                "Estimated Max Palette Contrast",
                $"{diagnostics.EstimatedMaximumVisibleChannelDifference:F3}");

            if (!diagnostics.CoverageGenerated)
            {
                EditorGUILayout.HelpBox(
                    "Projected coverage has not been generated. Regenerate Ground before judging visibility or binding.",
                    MessageType.Info);
            }
            else if (!diagnostics.CoverageEnabled)
            {
                EditorGUILayout.HelpBox(
                    "Coverage diagnostics exist, but the renderer-local coverage enable state is off.",
                    MessageType.Error);
            }

            if (diagnostics.CoverageGenerated &&
                !diagnostics.MaterialBindingCurrent)
            {
                EditorGUILayout.HelpBox(
                    "The renderer MaterialPropertyBlock does not match the current coverage texture, mapping, ink, or opacity. Regenerate or perform a material refresh before evaluating the visual result.",
                    MessageType.Error);
            }

            if (diagnostics.CoverageGenerated &&
                !diagnostics.CoverageMappingMatchesMeshBounds)
            {
                EditorGUILayout.HelpBox(
                    "Coverage origin or size does not match the generated mesh bounds. Use Raw Coverage Binding debug to distinguish mapping from colour/opacity problems.",
                    MessageType.Error);
            }

            if (diagnostics.AuthoredWidthInTexels > 0f &&
                diagnostics.AuthoredWidthInTexels < 1f)
            {
                EditorGUILayout.HelpBox(
                    "The authored line is narrower than one coverage texel. Partial raster coverage and bilinear filtering can soften it even when Ink Opacity is high.",
                    MessageType.Warning);
            }

            if (diagnostics.InkOpacity <= 0.15f ||
                diagnostics.EstimatedMaximumVisibleChannelDifference < 0.05f)
            {
                EditorGUILayout.HelpBox(
                    "The current ink contribution is likely difficult to see in normal lit rendering. Increase Ink Opacity or choose an Ink Colour with stronger contrast against the Ground palette.",
                    MessageType.Warning);
            }
        }

        private void DrawPaintedAccentPlacementDebugControls()
        {
            if (!DrawSubsectionFoldout(
                    ref showPaintedAccentPlacementDebug,
                    "Painted Accent Scene Debug"))
            {
                return;
            }

            EditorGUI.indentLevel++;
            EditorGUILayout.HelpBox(
                "Editor-only Scene view overlays. These controls do not change production coverage or generate report data.",
                MessageType.None);

            bool debugChanged = false;

            if (DrawSubsectionFoldout(
                    ref showPaintedAccentPlacementOverlays,
                    "Placement and Composition Overlays"))
            {
                EditorGUI.indentLevel++;
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(
                    paintedAccentPlacementOverlayWeight,
                    new GUIContent(
                        "Overlay Weight",
                        "Patch Preference displays only continuous patch weight. Effective Proposal Weight also includes semantic support."));
                EditorGUILayout.PropertyField(
                    showPaintedAccentDistributionOverlay,
                    new GUIContent(
                        "Show Distribution Overlay",
                        "Displays a filled-cell heatmap of the continuous patch-weight field."));
                EditorGUILayout.PropertyField(
                    showPaintedAccentWeightedProposals,
                    new GUIContent(
                        "Show Weighted Proposals",
                        "Displays weighted proposal centres before physical rejection."));
                EditorGUILayout.PropertyField(
                    showPaintedAccentLastAcceptedPositions,
                    new GUIContent(
                        "Show Last Accepted Positions",
                        "Displays accepted stroke centres from the most recent placement generation."));
                EditorGUILayout.PropertyField(
                    showPaintedAccentCompositionDebug,
                    new GUIContent(
                        "Show Composition Debug",
                        "Displays region modes, directions, thinning survival, mark roles, and selected glyph families."));
                debugChanged |= EditorGUI.EndChangeCheck();
                EditorGUI.indentLevel--;
            }

            if (DrawSubsectionFoldout(
                    ref showPaintedAccentShapeOverlay,
                    "Projected Shape Overlay"))
            {
                EditorGUI.indentLevel++;
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(
                    showPaintedAccentProjectedGlyphDebug,
                    new GUIContent(
                        "Show Accepted Projected Debug",
                        "Displays accepted projected glyphs at their true positions."));
                EditorGUILayout.PropertyField(
                    paintedAccentGlyphFamilyPreview,
                    new GUIContent(
                        "Family Preview",
                        "Filters only Scene debug drawing. It never changes generation or baked production coverage."));
                debugChanged |= EditorGUI.EndChangeCheck();
                EditorGUI.indentLevel--;
            }

            if (debugChanged)
            {
                serializedObject.ApplyModifiedProperties();
                paintedAccentPlacementDebugSignature = int.MinValue;
                paintedAccentPlacementDebugSnapshotBuildFailed = false;
                paintedAccentProjectedGlyphDebugSignature = int.MinValue;
                paintedAccentProjectedGlyphDebugSnapshotBuildFailed = false;
                SceneView.RepaintAll();
            }

            if (showPaintedAccentPlacementOverlays &&
                (showPaintedAccentDistributionOverlay.boolValue ||
                 showPaintedAccentWeightedProposals.boolValue) &&
                paintedAccentPlacementDebugSnapshotBuildFailed)
            {
                EditorGUILayout.HelpBox(
                    "The live Painted Accent placement snapshot could not be built. Confirm that the ground has a valid generated mesh and base-surface snapshot, then regenerate the ground.",
                    MessageType.Warning);
            }

            if (showPaintedAccentShapeOverlay &&
                showPaintedAccentProjectedGlyphDebug.boolValue &&
                paintedAccentProjectedGlyphDebugSnapshotBuildFailed)
            {
                EditorGUILayout.HelpBox(
                    "The projected glyph snapshot could not be built. Confirm that Painted Accent Lines are enabled and that the ground has valid generated descriptors, then regenerate the ground.",
                    MessageType.Warning);
            }

            GeneratedGround ground = target as GeneratedGround;
            if (ground == null)
            {
                EditorGUI.indentLevel--;
                return;
            }

            if (showPaintedAccentPlacementOverlays &&
                showPaintedAccentCompositionDebug.boolValue &&
                !ground.GetLastPaintedAccentCompositionDebugSnapshot().IsValid)
            {
                EditorGUILayout.HelpBox(
                    "The composition snapshot is unavailable. Regenerate Painted Accent placement before using the composition overlay.",
                    MessageType.Warning);
            }

            EditorGUI.indentLevel--;
        }

        private void DrawPaintedAccentDiagnosticsControls()
        {
            if (!DrawSubsectionFoldout(
                    ref showPaintedAccentDiagnostics,
                    "Painted Accent Reports"))
            {
                return;
            }

            EditorGUI.indentLevel++;
            if (targets.Length != 1 || target is not GeneratedGround ground)
            {
                EditorGUILayout.HelpBox(
                    "Select one GeneratedGround to inspect its retained Painted Accent reports.",
                    MessageType.Info);
                EditorGUI.indentLevel--;
                return;
            }

            EditorGUILayout.LabelField(
                "Last Completed SurfaceStrokes Timing",
                EditorStyles.miniBoldLabel);
            EditorGUILayout.HelpBox(
                ground.LastCompletedPaintedAccentSurfaceStrokeTimingDiagnostics,
                MessageType.None);
            if (GUILayout.Button("Copy SurfaceStrokes Timing"))
            {
                EditorGUIUtility.systemCopyBuffer =
                    BuildPaintedAccentSurfaceStrokeTimingClipboardReport(ground);
            }

            EditorGUILayout.LabelField(
                "Last Placement Result",
                EditorStyles.miniBoldLabel);
            EditorGUILayout.HelpBox(
                ground.GetLastPaintedAccentPlacementStatistics(),
                MessageType.None);
            if (GUILayout.Button("Copy Placement Report"))
            {
                EditorGUIUtility.systemCopyBuffer =
                    BuildPaintedAccentPlacementClipboardReport(ground);
            }

            EditorGUILayout.LabelField(
                "Last Completed ProjectedGlyphs Timing",
                EditorStyles.miniBoldLabel);
            EditorGUILayout.HelpBox(
                ground.LastCompletedPaintedAccentProjectedGlyphTimingDiagnostics,
                MessageType.None);
            if (GUILayout.Button("Copy ProjectedGlyphs Timing"))
            {
                EditorGUIUtility.systemCopyBuffer =
                    BuildPaintedAccentProjectedGlyphTimingClipboardReport(ground);
            }

            EditorGUILayout.LabelField(
                "Accepted Projected Baseline",
                EditorStyles.miniBoldLabel);
            EditorGUILayout.HelpBox(
                ground.GetLastPaintedAccentProjectedGlyphStatistics(),
                MessageType.None);
            if (GUILayout.Button("Copy Projected Baseline"))
            {
                EditorGUIUtility.systemCopyBuffer =
                    BuildPaintedAccentProjectedGlyphClipboardReport(ground);
            }

            EditorGUILayout.LabelField(
                "Last Completed Coverage Timing",
                EditorStyles.miniBoldLabel);
            EditorGUILayout.HelpBox(
                ground.LastCompletedPaintedAccentCoverageTimingDiagnostics,
                MessageType.None);
            if (GUILayout.Button("Copy Coverage Timing"))
            {
                EditorGUIUtility.systemCopyBuffer =
                    BuildPaintedAccentCoverageTimingClipboardReport(ground);
            }

            EditorGUILayout.LabelField(
                "Last Coverage Raster",
                EditorStyles.miniBoldLabel);
            EditorGUILayout.HelpBox(
                ground.GetLastPaintedAccentCoverageStatistics(),
                MessageType.None);
            if (GUILayout.Button("Copy Coverage Report"))
            {
                EditorGUIUtility.systemCopyBuffer =
                    BuildPaintedAccentCoverageClipboardReport(ground);
            }

            if (GUILayout.Button("Copy All Painted Accent Reports"))
            {
                EditorGUIUtility.systemCopyBuffer =
                    BuildPaintedAccentGenerationDiagnosticsClipboardReport(ground);
            }

            EditorGUILayout.LabelField(
                "These reports retain the last completed stage result. Merely opening or copying them does not regenerate Painted Accents.",
                EditorStyles.wordWrappedMiniLabel);
            EditorGUI.indentLevel--;
        }

        private static SerializedProperty FindSelectedPaintedAccentFeatureProperty(
            SerializedObject styleObject,
            GroundSurfaceStyleProfile style,
            string variantId,
            out int paintedAccentEntryCount,
            out int runtimeApplicableCount,
            out int firstPaintedAccentIndex,
            out int runtimeFeatureIndex,
            out int authoringFeatureIndex)
        {
            paintedAccentEntryCount = 0;
            runtimeApplicableCount = 0;
            firstPaintedAccentIndex = -1;
            runtimeFeatureIndex = -1;
            authoringFeatureIndex = -1;

            if (!TryFindSelectedVariantProperty(
                    styleObject,
                    style,
                    variantId,
                    out GroundSurfaceVariantRecipe selectedVariant,
                    out SerializedProperty serializedVariant))
            {
                return null;
            }

            return FindAuthoringFeatureProperty(
                serializedVariant,
                selectedVariant,
                GroundSurfaceFeatureKind.PaintedAccentLines,
                out paintedAccentEntryCount,
                out runtimeApplicableCount,
                out firstPaintedAccentIndex,
                out runtimeFeatureIndex,
                out authoringFeatureIndex);
        }

        private static SerializedProperty FindAuthoringFeatureProperty(
            SerializedProperty serializedVariant,
            GroundSurfaceVariantRecipe variant,
            GroundSurfaceFeatureKind kind,
            out int matchingEntryCount,
            out int runtimeApplicableCount,
            out int firstMatchingIndex,
            out int runtimeFeatureIndex,
            out int authoringFeatureIndex)
        {
            matchingEntryCount = 0;
            runtimeApplicableCount = 0;
            firstMatchingIndex = -1;
            runtimeFeatureIndex = -1;
            authoringFeatureIndex = -1;

            if (serializedVariant == null ||
                variant == null ||
                variant.Features == null)
            {
                return null;
            }

            variant.TryGetFirstShaderFeature(
                kind,
                out GroundSurfaceFeatureRecipe runtimeFeature);

            for (int featureIndex = 0;
                 featureIndex < variant.Features.Count;
                 featureIndex++)
            {
                GroundSurfaceFeatureRecipe candidate =
                    variant.Features[featureIndex];

                if (candidate == null || candidate.Kind != kind)
                {
                    continue;
                }

                if (firstMatchingIndex < 0)
                {
                    firstMatchingIndex = featureIndex;
                }

                matchingEntryCount++;

                if (candidate.CanApplyAsShaderOnly)
                {
                    runtimeApplicableCount++;
                }

                if (object.ReferenceEquals(candidate, runtimeFeature))
                {
                    runtimeFeatureIndex = featureIndex;
                }
            }

            authoringFeatureIndex =
                runtimeFeatureIndex >= 0
                    ? runtimeFeatureIndex
                    : firstMatchingIndex;

            SerializedProperty features =
                serializedVariant.FindPropertyRelative("features");

            if (features == null ||
                !features.isArray ||
                authoringFeatureIndex < 0 ||
                authoringFeatureIndex >= features.arraySize)
            {
                return null;
            }

            return features.GetArrayElementAtIndex(authoringFeatureIndex);
        }

        private static void DrawPaintedAccentResolutionWarnings(
            int paintedAccentEntryCount,
            int runtimeApplicableCount,
            int firstPaintedAccentIndex,
            int runtimeFeatureIndex,
            int authoringFeatureIndex)
        {
            DrawFeatureResolutionWarnings(
                "Painted Accents",
                paintedAccentEntryCount,
                runtimeApplicableCount,
                firstPaintedAccentIndex,
                runtimeFeatureIndex,
                authoringFeatureIndex);
        }

        private static void DrawFeatureResolutionWarnings(
            string label,
            int matchingEntryCount,
            int runtimeApplicableCount,
            int firstMatchingIndex,
            int runtimeFeatureIndex,
            int authoringFeatureIndex)
        {
            if (matchingEntryCount == 0)
            {
                EditorGUILayout.HelpBox(
                    $"The selected variant has no {label} recipe. Use Advanced Style Asset only when adding or restructuring recipe entries; normal tuning remains in this Inspector.",
                    MessageType.Info);
                return;
            }

            if (runtimeApplicableCount == 0)
            {
                EditorGUILayout.HelpBox(
                    $"No {label} entry is currently runtime-applicable. The controls below edit recipe entry {authoringFeatureIndex + 1}; enable it, choose Shader Only, and set its visible intensity above zero to render it.",
                    MessageType.Warning);
            }

            if (runtimeApplicableCount > 1)
            {
                EditorGUILayout.HelpBox(
                    $"The selected variant has {runtimeApplicableCount} runtime-applicable {label} recipes. Runtime and this Inspector use the first applicable entry; later duplicates are ignored.",
                    MessageType.Warning);
            }

            if (firstMatchingIndex >= 0 &&
                runtimeFeatureIndex > firstMatchingIndex)
            {
                EditorGUILayout.HelpBox(
                    $"One or more earlier {label} entries are disabled, non-shader, or zero-intensity. They are ignored; the controls below edit the first entry the renderer actually uses.",
                    MessageType.Info);
            }

            if (matchingEntryCount > 1)
            {
                EditorGUILayout.LabelField(
                    "Authoring Entry",
                    EditorStyles.miniBoldLabel);
                EditorGUILayout.LabelField(
                    $"Variant entry {authoringFeatureIndex + 1}; {matchingEntryCount} matching recipes",
                    EditorStyles.miniLabel);
            }
        }

        private bool TryGetSingleSelectedVariantContext(
            out GroundSurfaceStyleProfile style,
            out GroundSurfaceVariantRecipe variant,
            out SerializedObject styleObject,
            out SerializedProperty serializedVariant)
        {
            style = null;
            variant = null;
            styleObject = null;
            serializedVariant = null;

            if (targets.Length != 1 ||
                surfaceStyleProfile.hasMultipleDifferentValues ||
                surfaceVariantId.hasMultipleDifferentValues)
            {
                return false;
            }

            style = surfaceStyleProfile.objectReferenceValue as
                GroundSurfaceStyleProfile;

            if (style == null)
            {
                return false;
            }

            styleObject = new SerializedObject(style);
            styleObject.Update();

            return TryFindSelectedVariantProperty(
                styleObject,
                style,
                surfaceVariantId.stringValue,
                out variant,
                out serializedVariant);
        }

        private static bool TryFindSelectedVariantProperty(
            SerializedObject styleObject,
            GroundSurfaceStyleProfile style,
            string variantId,
            out GroundSurfaceVariantRecipe variant,
            out SerializedProperty serializedVariant)
        {
            variant = null;
            serializedVariant = null;

            if (styleObject == null ||
                style == null ||
                style.Variants == null)
            {
                return false;
            }

            int selectedVariantIndex = -1;

            for (int variantIndex = 0;
                 variantIndex < style.Variants.Count;
                 variantIndex++)
            {
                GroundSurfaceVariantRecipe candidate =
                    style.Variants[variantIndex];

                if (candidate != null && candidate.Id == variantId)
                {
                    variant = candidate;
                    selectedVariantIndex = variantIndex;
                    break;
                }
            }

            SerializedProperty variants =
                styleObject.FindProperty("variants");

            if (variant == null ||
                variants == null ||
                !variants.isArray ||
                selectedVariantIndex < 0 ||
                selectedVariantIndex >= variants.arraySize)
            {
                return false;
            }

            serializedVariant =
                variants.GetArrayElementAtIndex(selectedVariantIndex);
            return serializedVariant != null;
        }

        private static GroundSurfaceVariantRecipe ResolveSelectedVariant(
            GroundSurfaceStyleProfile style,
            string variantId)
        {
            if (style != null &&
                style.TryGetVariant(
                    variantId,
                    out GroundSurfaceVariantRecipe variant))
            {
                return variant;
            }

            return null;
        }

        private static void DrawSharedVariantAuthoringScope(
            GroundSurfaceStyleProfile style,
            GroundSurfaceVariantRecipe variant,
            bool materialControlsOnly)
        {
            string styleName =
                style != null ? style.DisplayName : "Missing Style";
            string variantName =
                variant != null ? variant.DisplayName : "Missing Variant";
            string consequence = materialControlsOnly
                ? "Changes affect every GeneratedGround using this style variant without a local material override."
                : "Changes affect every GeneratedGround using this style variant.";

            EditorGUILayout.HelpBox(
                $"Editing Shared Style — {styleName} / {variantName}. {consequence}",
                MessageType.Info);
        }

        private void DrawGroundGeometrySection()
        {
            if (!DrawSectionFoldout(
                    ref showGroundGeometry,
                    "Ground Geometry"))
            {
                return;
            }

            EditorGUI.indentLevel++;
            DrawPatchDomainControls();
            DrawBaseShapeControls();
            DrawMountainTransitionControls();
            EditorGUI.indentLevel--;
        }

        private void DrawPatchDomainControls()
        {
            if (!DrawSubsectionFoldout(
                    ref showPatchDomain,
                    "Patch Domain"))
            {
                return;
            }

            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(
                patchSize,
                new GUIContent("Patch Size"));

            EditorGUILayout.PropertyField(
                resolution,
                new GUIContent("Mesh Resolution"));

            GroundPatchSize selectedSize =
                (GroundPatchSize)patchSize.enumValueIndex;
            GroundResolution selectedResolution =
                (GroundResolution)resolution.enumValueIndex;
            float metres = GroundGenerator.ResolvePatchSize(selectedSize);
            int verticesPerSide =
                GroundGenerator.ResolveResolution(selectedResolution);
            int triangleCount =
                (verticesPerSide - 1) *
                (verticesPerSide - 1) *
                2;

            EditorGUILayout.HelpBox(
                $"{metres:0} × {metres:0} m, " +
                $"{verticesPerSide} × {verticesPerSide} vertices, " +
                $"{triangleCount:N0} triangles.",
                MessageType.None);
            EditorGUI.indentLevel--;
        }

        private void DrawBaseShapeControls()
        {
            if (!DrawSubsectionFoldout(
                    ref showBaseShape,
                    "Base Shape"))
            {
                return;
            }

            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(
                profile,
                new GUIContent("Profile"));

            EditorGUILayout.Slider(
                broadForm,
                0f,
                6f,
                new GUIContent(
                    "Broad Form",
                    "Height contribution in metres."));

            EditorGUILayout.Slider(
                roughness,
                0f,
                1f,
                new GUIContent(
                    "Roughness",
                    "Controls broad and detail noise frequency."));

            EditorGUILayout.Slider(
                surfaceDetail,
                0f,
                1f,
                new GUIContent(
                    "Surface Detail",
                    "Restrained small-scale height variation."));

            EditorGUILayout.PropertyField(
                edgeBlend,
                new GUIContent(
                    "Edge Blend",
                    "Fades generated variation near patch borders."));
            EditorGUI.indentLevel--;
        }

        private void DrawMountainTransitionControls()
        {
            if (!DrawSubsectionFoldout(
                    ref showMountainTransition,
                    "Mountain Transition"))
            {
                return;
            }

            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(
                transitionDirection,
                new GUIContent(
                    "Direction",
                    "The side toward which this patch rises."));

            using (new EditorGUI.DisabledScope(
                       transitionDirection.enumValueIndex ==
                       (int)GroundTransitionDirection.None))
            {
                EditorGUILayout.Slider(
                    transitionHeight,
                    -12f,
                    12f,
                    new GUIContent(
                        "Height Change",
                        "Metres from the low side to the high side."));
            }
            EditorGUI.indentLevel--;
        }

        private void DrawRegenerationAndCachingSection()
        {
            if (!DrawSectionFoldout(
                    ref showRegenerationAndCaching,
                    "Regeneration and Caching"))
            {
                return;
            }

            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(
                regenerateOnValidate,
                new GUIContent(
                    "Live Regeneration",
                    "Regenerate when authoring values change."));

            EditorGUILayout.Space(4f);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Randomize Shape Seed"))
                {
                    ApplyToTargets(
                        "Randomize Generated Ground Shape Seed",
                        ground => ground.CreateNewShape());
                }

                if (GUILayout.Button("Regenerate Ground"))
                {
                    ApplyToTargets(
                        "Regenerate Generated Ground",
                        ground => ground.Regenerate());
                }
            }

            if (GUILayout.Button(
                    new GUIContent(
                        "Refresh Modifier and River Links + Regenerate",
                        "Rediscovers child GroundModifier and StylizedRiver components, then performs a full Ground regeneration.")))
            {
                ApplyToTargets(
                    "Refresh Generated Ground Links",
                    ground =>
                    {
                        ground.RefreshModifiers();
                        ground.Regenerate();
                    });
            }

            EditorGUI.indentLevel--;
        }

        private void DrawSurfaceAppearanceSection()
        {
            if (!DrawSectionFoldout(
                    ref showSurfaceAppearance,
                    "Surface Appearance"))
            {
                return;
            }

            EditorGUI.indentLevel++;
            EditorGUILayout.Slider(
                surfaceVariation,
                0f,
                1f,
                new GUIContent(
                    "Material Variation",
                    "Overall strength of generated tonal variation written to vertex colour red. This is stored on the selected GeneratedGround."));

            DrawResolvedSurfaceProfileControls();
            DrawResolvedMaterialControls();
            EditorGUI.indentLevel--;
        }

        private void DrawResolvedSurfaceProfileControls()
        {
            if (!DrawSubsectionFoldout(
                    ref showSurfaceResponseProfile,
                    "Surface Response Profile"))
            {
                return;
            }

            EditorGUI.indentLevel++;

            if (targets.Length != 1)
            {
                EditorGUILayout.HelpBox(
                    "Resolved surface-profile asset editing is disabled for multi-object selection. Select one GeneratedGround so the Inspector can show the exact shared authoring owner.",
                    MessageType.Info);
                EditorGUI.indentLevel--;
                return;
            }

            GeneratedGround ground = target as GeneratedGround;
            GroundSurfaceProfile resolvedProfile =
                ground != null ? ground.SurfaceProfile : null;

            if (resolvedProfile == null)
            {
                EditorGUILayout.HelpBox(
                    "No GroundSurfaceProfile resolves for this Ground. Assign a family default or a profile override in Ground Overview.",
                    MessageType.Warning);
                EditorGUI.indentLevel--;
                return;
            }

            string ownership =
                ground.OverrideSurfaceProfile
                    ? "Referenced Override Profile"
                    : "Shared Family Profile";

            EditorGUILayout.HelpBox(
                $"Editing {ownership} — {resolvedProfile.DisplayName}. Changes affect every loaded GeneratedGround that resolves this profile asset.",
                MessageType.Info);

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.ObjectField(
                    new GUIContent(
                        "Profile Asset",
                        "The GroundSurfaceProfile asset that owns the values below."),
                    resolvedProfile,
                    typeof(GroundSurfaceProfile),
                    false);
            }

            SerializedObject profileObject =
                new SerializedObject(resolvedProfile);
            profileObject.Update();

            SerializedProperty patchScaleProperty =
                profileObject.FindProperty("patchScale");
            SerializedProperty patchContrastProperty =
                profileObject.FindProperty("patchContrast");
            SerializedProperty patchEdgeSoftnessProperty =
                profileObject.FindProperty("patchEdgeSoftness");
            SerializedProperty exposureBiasProperty =
                profileObject.FindProperty("exposureBias");
            SerializedProperty dampDepositBiasProperty =
                profileObject.FindProperty("dampDepositBias");
            SerializedProperty vegetationSuitabilityProperty =
                profileObject.FindProperty("vegetationSuitability");
            SerializedProperty rockyDrySuitabilityProperty =
                profileObject.FindProperty("rockyDrySuitability");
            SerializedProperty snowEligibilityProperty =
                profileObject.FindProperty("snowEligibility");
            SerializedProperty rainAbsorptionProperty =
                profileObject.FindProperty("rainAbsorption");

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.LabelField(
                "Generated Patch Structure",
                EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(
                patchScaleProperty,
                new GUIContent(
                    "Patch Scale (m)",
                    "Approximate metre scale of broad generated tonal patches. Increasing it creates larger regions. Shared profile edit; may rebuild Ground masks when Live Regeneration is enabled."));
            EditorGUILayout.PropertyField(
                patchContrastProperty,
                new GUIContent(
                    "Patch Contrast",
                    "Controls how strongly generated tonal islands separate from neutral. Shared profile edit; may rebuild Ground masks when Live Regeneration is enabled."));
            EditorGUILayout.PropertyField(
                patchEdgeSoftnessProperty,
                new GUIContent(
                    "Patch Edge Softness",
                    "Controls how softly broad generated patch values transition. Shared profile edit; may rebuild Ground masks when Live Regeneration is enabled."));

            EditorGUILayout.Space(3f);
            EditorGUILayout.LabelField(
                "Static Surface Tendencies",
                EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(
                exposureBiasProperty,
                new GUIContent(
                    "Exposure Bias",
                    "Baseline tendency for exposed and upward-facing places to receive light, frost, or snow response. Shared profile edit; may rebuild Ground masks."));
            EditorGUILayout.PropertyField(
                dampDepositBiasProperty,
                new GUIContent(
                    "Damp Deposit Bias",
                    "Baseline tendency for low, flat, and shore-adjacent places to collect dark damp or deposit response. Shared profile edit; may rebuild Ground masks."));
            EditorGUILayout.PropertyField(
                vegetationSuitabilityProperty,
                new GUIContent(
                    "Vegetation Suitability",
                    "Baseline vegetation-friendly surface response used by current semantic masks and future vegetation systems. Shared profile edit; may rebuild Ground masks."));
            EditorGUILayout.PropertyField(
                rockyDrySuitabilityProperty,
                new GUIContent(
                    "Rocky/Dry Suitability",
                    "Baseline tendency for dry or rocky secondary response. Shared profile edit; may rebuild Ground masks."));
            EditorGUILayout.PropertyField(
                snowEligibilityProperty,
                new GUIContent(
                    "Snow Eligibility",
                    "Controls how eligible this surface is for the current snow response and future accumulation systems. Shared profile edit; may rebuild Ground masks."));
            EditorGUILayout.PropertyField(
                rainAbsorptionProperty,
                new GUIContent(
                    "Rain Absorption",
                    "Controls the current rain and wetness tendency used by semantic response. Shared profile edit; may rebuild Ground masks."));

            if (EditorGUI.EndChangeCheck())
            {
                profileObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(resolvedProfile);
                RefreshLoadedGroundsUsingSurfaceProfile(resolvedProfile);
            }

            EditorGUI.indentLevel--;
        }

        private void DrawSurfaceFeaturesSection()
        {
            if (!DrawSectionFoldout(
                    ref showSurfaceFeatures,
                    "Surface Features"))
            {
                return;
            }

            EditorGUI.indentLevel++;

            if (!TryGetSingleSelectedVariantContext(
                    out GroundSurfaceStyleProfile style,
                    out GroundSurfaceVariantRecipe variant,
                    out SerializedObject styleObject,
                    out SerializedProperty serializedVariant))
            {
                EditorGUILayout.HelpBox(
                    targets.Length == 1
                        ? "No valid shared style variant resolves for this Ground."
                        : "Shared feature editing is disabled for multi-object selection. Select one GeneratedGround so the exact shared variant owner is unambiguous.",
                    MessageType.Info);
                EditorGUI.indentLevel--;
                return;
            }

            DrawSharedVariantAuthoringScope(style, variant, false);

            bool styleChanged = false;
            styleChanged |= DrawShaderFeatureControls(
                ref showDirectionalStreaks,
                "Directional Streaks",
                serializedVariant,
                variant,
                GroundSurfaceFeatureKind.DirectionalStreaks,
                true);
            styleChanged |= DrawShaderFeatureControls(
                ref showPooledWetness,
                "Pooled Wetness",
                serializedVariant,
                variant,
                GroundSurfaceFeatureKind.PooledWetness,
                false);
            styleChanged |= DrawShaderFeatureControls(
                ref showTrampledWear,
                "Trampled Wear",
                serializedVariant,
                variant,
                GroundSurfaceFeatureKind.TrampledWear,
                false);

            if (styleChanged)
            {
                styleObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(style);
                RefreshLoadedGroundsUsingStyleVariant(
                    style,
                    variant.Id,
                    true);
            }

            EditorGUI.indentLevel--;
        }

        private static bool DrawShaderFeatureControls(
            ref bool expanded,
            string label,
            SerializedProperty serializedVariant,
            GroundSurfaceVariantRecipe variant,
            GroundSurfaceFeatureKind kind,
            bool drawDirection)
        {
            if (!DrawSubsectionFoldout(ref expanded, label))
            {
                return false;
            }

            EditorGUI.indentLevel++;

            SerializedProperty feature = FindAuthoringFeatureProperty(
                serializedVariant,
                variant,
                kind,
                out int matchingEntryCount,
                out int runtimeApplicableCount,
                out int firstMatchingIndex,
                out int runtimeFeatureIndex,
                out int authoringFeatureIndex);

            DrawFeatureResolutionWarnings(
                label,
                matchingEntryCount,
                runtimeApplicableCount,
                firstMatchingIndex,
                runtimeFeatureIndex,
                authoringFeatureIndex);

            if (feature == null)
            {
                EditorGUI.indentLevel--;
                return false;
            }

            SerializedProperty enabled =
                feature.FindPropertyRelative("enabled");
            SerializedProperty costClass =
                feature.FindPropertyRelative("costClass");
            SerializedProperty strength =
                feature.FindPropertyRelative("strength");
            SerializedProperty scale =
                feature.FindPropertyRelative("scale");
            SerializedProperty contrast =
                feature.FindPropertyRelative("contrast");
            SerializedProperty maskInfluence =
                feature.FindPropertyRelative("maskInfluence");
            SerializedProperty direction =
                feature.FindPropertyRelative("direction");
            SerializedProperty seedOffset =
                feature.FindPropertyRelative("seedOffset");

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(
                enabled,
                new GUIContent(
                    $"Enable {label}",
                    "Disables this feature while preserving its authored values. Shared variant edit; material-only refresh."));
            EditorGUILayout.PropertyField(
                costClass,
                new GUIContent(
                    "Execution Path",
                    "Shader Only is the currently rendered path. Other cost classes are reserved and do not produce Ground shader output."));
            EditorGUILayout.Slider(
                strength,
                0f,
                1f,
                new GUIContent(
                    "Intensity",
                    "Primary visible feature contribution. Zero makes this recipe runtime-inapplicable. Shared variant edit; material-only refresh."));
            EditorGUILayout.Slider(
                scale,
                0.1f,
                30f,
                new GUIContent(
                    "Scale (m)",
                    "World-space scale of this shader feature."));
            EditorGUILayout.Slider(
                contrast,
                0f,
                1f,
                new GUIContent(
                    "Contrast",
                    "Shape contrast inside this feature's shader mask."));
            EditorGUILayout.Slider(
                maskInfluence,
                0f,
                1f,
                new GUIContent(
                    "Surface Mask Influence",
                    "How strongly generated semantic Ground masks gate this feature."));

            if (drawDirection)
            {
                EditorGUILayout.PropertyField(
                    direction,
                    new GUIContent(
                        "Direction",
                        "Stable world X/Z directional bias consumed by Directional Streaks."));
            }

            EditorGUILayout.PropertyField(
                seedOffset,
                new GUIContent(
                    "Pattern Seed Offset",
                    "Stable feature-specific seed mixed with the Ground material seed."));

            bool changed = EditorGUI.EndChangeCheck();
            EditorGUI.indentLevel--;
            return changed;
        }

        private void DrawSurfaceVariantPopup(
            GroundSurfaceStyleProfile style)
        {
            if (style == null)
            {
                EditorGUILayout.HelpBox(
                    "Assign a Surface Style Profile to choose visual variants. " +
                    "GeneratedGround will attempt to assign the Snowfield style automatically if it exists in the project.",
                    MessageType.Info);
                return;
            }

            if (style.Variants == null || style.Variants.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "The selected Surface Style Profile has no valid variants.",
                    MessageType.Warning);
                return;
            }

            string currentId = surfaceVariantId.stringValue;
            int validCount = 0;

            for (int index = 0; index < style.Variants.Count; index++)
            {
                GroundSurfaceVariantRecipe variant = style.Variants[index];

                if (variant != null && variant.HasValidId)
                {
                    validCount++;
                }
            }

            if (validCount == 0)
            {
                EditorGUILayout.HelpBox(
                    "The selected Surface Style Profile contains only empty or invalid variant ids.",
                    MessageType.Warning);
                return;
            }

            string[] ids = new string[validCount];
            GUIContent[] labels = new GUIContent[validCount];
            int writeIndex = 0;
            int selectedIndex = 0;
            for (int index = 0; index < style.Variants.Count; index++)
            {
                GroundSurfaceVariantRecipe variant = style.Variants[index];

                if (variant == null || !variant.HasValidId)
                {
                    continue;
                }

                ids[writeIndex] = variant.Id;
                labels[writeIndex] = new GUIContent(variant.DisplayName);

                if (variant.Id == currentId)
                {
                    selectedIndex = writeIndex;
                }

                writeIndex++;
            }

            EditorGUI.showMixedValue =
                surfaceVariantId.hasMultipleDifferentValues;
            EditorGUI.BeginChangeCheck();

            int newSelectedIndex = EditorGUILayout.Popup(
                new GUIContent(
                    "Surface Variant",
                    "Variant recipe inside the selected surface style asset."),
                selectedIndex,
                labels);

            EditorGUI.showMixedValue = false;

            if (EditorGUI.EndChangeCheck())
            {
                string selectedId = ids[Mathf.Clamp(
                    newSelectedIndex,
                    0,
                    ids.Length - 1)];

                serializedObject.ApplyModifiedProperties();
                ApplyToTargets(
                    "Select Ground Surface Variant",
                    ground => ground.SetSurfaceVariant(selectedId));
            }
        }

        private void DrawResolvedFeatureSummary()
        {
            if (targets.Length != 1 ||
                !DrawSubsectionFoldout(
                    ref showResolvedFeatureSummary,
                    "Resolved Feature Summary"))
            {
                return;
            }

            GeneratedGround ground = target as GeneratedGround;
            if (ground == null)
            {
                return;
            }

            EditorGUI.indentLevel++;
            EditorGUILayout.HelpBox(
                ground.ResolvedSurfaceFeatureSummary,
                MessageType.None);
            EditorGUI.indentLevel--;
        }

        private void DrawSurfaceProfileOverride(
            GroundSurfaceStyleProfile style)
        {
            EditorGUILayout.Space(3f);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(
                overrideSurfaceProfile,
                new GUIContent(
                    "Override Surface Profile",
                    "Select a different GroundSurfaceProfile asset for this GeneratedGround instead of the family default. The referenced profile asset may still be shared by other Grounds."));

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                ApplyToTargets(
                    "Toggle Ground Surface Profile Override",
                    ground => ground.RefreshSurfaceStyleState());
            }

            if (overrideSurfaceProfile.hasMultipleDifferentValues ||
                overrideSurfaceProfile.boolValue)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(
                    surfaceProfile,
                    new GUIContent(
                        "Surface Profile Override",
                        "Profile asset selected by this GeneratedGround. Editing that asset affects every Ground that references it."));

                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    ApplyToTargets(
                        "Change Ground Surface Profile Override",
                        ground => ground.RefreshSurfaceStyleState());
                }

                return;
            }

            using (new EditorGUI.DisabledScope(true))
            {
                Object defaultProfile =
                    style != null ? style.DefaultSurfaceProfile : null;

                EditorGUILayout.ObjectField(
                    new GUIContent(
                        "Resolved Surface Profile",
                        "Semantic/mask-generation profile inherited from the selected style."),
                    defaultProfile,
                    typeof(GroundSurfaceProfile),
                    false);
            }
        }

        private void DrawResolvedMaterialControls()
        {
            showMaterialControls = EditorGUILayout.Foldout(
                showMaterialControls,
                "Material Controls",
                true);

            if (!showMaterialControls)
            {
                return;
            }

            EditorGUI.indentLevel++;

            if (targets.Length != 1)
            {
                EditorGUILayout.HelpBox(
                    "Material ownership can differ across selected Grounds. Select one GeneratedGround to edit its resolved shared variant or create a safe local override.",
                    MessageType.Info);
                EditorGUI.indentLevel--;
                return;
            }

            EditorGUI.BeginChangeCheck();
            bool enabled = EditorGUILayout.Toggle(
                new GUIContent(
                    "Use Local Material Override",
                    "Copies the currently resolved shared variant material values onto this GeneratedGround, or returns it to shared variant ownership."),
                overrideMaterialControls.boolValue);

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                if (enabled)
                {
                    ApplyToTargets(
                        "Enable Ground Material Override",
                        ground => ground.EnableMaterialControlOverrideFromResolved());
                }
                else
                {
                    ApplyToTargets(
                        "Disable Ground Material Override",
                        ground => ground.DisableMaterialControlOverride());
                }
            }

            if (overrideMaterialControls.boolValue)
            {
                GeneratedGround selectedGround =
                    target as GeneratedGround;
                string sceneName =
                    selectedGround != null &&
                    selectedGround.gameObject.scene.IsValid() &&
                    !string.IsNullOrEmpty(
                        selectedGround.gameObject.scene.name)
                        ? selectedGround.gameObject.scene.name
                        : "Unsaved Scene";
                string objectName =
                    selectedGround != null
                        ? selectedGround.name
                        : "GeneratedGround";

                DrawMaterialStorageLine(
                    $"Local Scene Override — {sceneName} / {objectName}",
                    "These values are serialized on this GeneratedGround component. Save the scene to persist them.");

                EditorGUILayout.HelpBox(
                    "Editing Local Material Override — changes affect this GeneratedGround only.",
                    MessageType.Info);

                bool localMaterialChanged =
                    DrawLocalMaterialControlGroups();

                if (localMaterialChanged)
                {
                    serializedObject.ApplyModifiedProperties();
                    ApplyToTargets(
                        "Customize Ground Material Controls",
                        ground => ground.MarkGroundVisualControlsCustom());
                }

                EditorGUI.indentLevel--;
                return;
            }

            if (!TryGetSingleSelectedVariantContext(
                    out GroundSurfaceStyleProfile style,
                    out GroundSurfaceVariantRecipe variant,
                    out SerializedObject styleObject,
                    out SerializedProperty serializedVariant))
            {
                EditorGUILayout.HelpBox(
                    "No valid shared style variant resolves for these material controls.",
                    MessageType.Warning);
                EditorGUI.indentLevel--;
                return;
            }

            DrawSharedVariantAuthoringScope(style, variant, true);

            string styleAssetPath = AssetDatabase.GetAssetPath(style);
            DrawMaterialStorageLine(
                $"Shared Style Asset — {style.name} / {variant.DisplayName}",
                string.IsNullOrEmpty(styleAssetPath)
                    ? "These values are serialized in the selected GroundSurfaceStyleProfile asset."
                    : styleAssetPath);

            SerializedProperty materialControls =
                serializedVariant.FindPropertyRelative("materialControls");

            if (materialControls == null)
            {
                EditorGUILayout.HelpBox(
                    "The selected variant has no serialized material-control data.",
                    MessageType.Error);
                EditorGUI.indentLevel--;
                return;
            }

            bool sharedMaterialChanged =
                DrawSharedMaterialControlGroups(materialControls);

            bool sharedMaterialApplied =
                styleObject.ApplyModifiedProperties();

            if (sharedMaterialChanged || sharedMaterialApplied)
            {
                EditorUtility.SetDirty(style);
                QueueSharedStyleSave(style);
                RefreshLoadedGroundsUsingStyleVariant(
                    style,
                    variant.Id,
                    false);
            }

            EditorGUI.indentLevel--;
        }

        private bool DrawLocalMaterialControlGroups()
        {
            bool materialChanged = false;
            materialChanged |= DrawRiverBankResponseSubsection(
                ref showMaterialRiverCoupledBank,
                bankSurfaceLayer,
                bankMaterialStrength,
                bankMaterialReach,
                immediateBankExposure,
                waterlineMaterialStrength,
                bankTransitionSoftness,
                outerBankExtension,
                outerBankStrength,
                outerBankFade,
                bankDetailScaleMultiplier,
                bankAuthoredColorStrengthMultiplier,
                bankAuthoredColorLightingMultiplier,
                bankDetailNormalStrengthMultiplier,
                bankDetailCavityStrengthMultiplier,
                bankDetailValueFormMultiplier,
                bankDetailFinishVariationMultiplier,
                bankLegacyPixelCellInfluenceMultiplier,
                vegetationRetreatStrength,
                snowMeltStrength,
                frostRetreatStrength,
                paintedAccentRetreatStrength,
                shoreHydrologyModifier,
                shoreWetnessStrength,
                shoreWetnessReach,
                shoreWetnessFade,
                broadBankSaturation,
                immediateBankSaturation,
                waterlineSaturation,
                shoreWetHighlightWidth,
                shoreWetHighlightFeather,
                shoreWetHighlightStrength,
                shoreWetHighlightTightness,
                shoreWetHighlightCameraBias,
                shoreWetHighlightVerticalFalloff);

            materialChanged |= DrawRiverbedResponseSubsection(
                ref showMaterialRiverCoupledRiverbed,
                riverbedSurfaceSource,
                bankSurfaceLayer,
                riverbedSurfaceLayer,
                riverbedMaterialStrength,
                riverbedDetailScaleMultiplier,
                riverbedAuthoredColorStrengthMultiplier,
                riverbedAuthoredColorLightingMultiplier,
                riverbedDetailNormalStrengthMultiplier,
                riverbedDetailCavityStrengthMultiplier,
                riverbedDetailValueFormMultiplier,
                riverbedDetailFinishVariationMultiplier,
                riverbedLegacyPixelCellInfluenceMultiplier,
                riverbedHydrologySource,
                shoreHydrologyModifier,
                riverbedHydrologyModifier,
                riverbedWetnessStrength,
                riverbedToBankWetnessBlendDistance,
                riverbedToBankWetnessBlendSoftness,
                riverbedWetSmoothnessResponse,
                riverbedWetSpecularResponse);

            materialChanged |= DrawMaterialSubsection(
                ref showMaterialMacroPatchComposition,
                "Macro Patch Composition",
                groundMacroPatchScale,
                groundMacroPatchPatternSeed,
                broadVariation,
                groundMacroPatchTransitionSoftness,
                groundMacroPatchSeparation);

            materialChanged |= DrawMaterialSubsection(
                ref showMaterialElevationReadability,
                "Elevation Readability",
                reliefShadingStrength,
                relativeHeightContrast);

            materialChanged |= DrawMaterialSubsection(
                ref showMaterialPalette,
                "Palette",
                baseColor,
                frostColor,
                dampTint,
                dampTintStrength,
                rockyDryTint,
                rockyDryTintStrength,
                vegetationTint,
                vegetationTintStrength);

            materialChanged |= DrawMaterialSubsection(
                ref showMaterialPixelVariation,
                "Pixel Variation",
                pixelCellSize,
                pixelToneCount,
                pixelClusterStrength,
                pixelVariation,
                vertexVariation,
                pixelEffectStrength,
                cellWarpStrength);

            materialChanged |= DrawMaterialSubsection(
                ref showMaterialSemanticResponse,
                "Semantic Response",
                profileContrastScale,
                profilePixelContrastScale,
                groundSnowResponseScale,
                groundDampResponseScale,
                groundVegetationResponseScale,
                groundRockyDryResponseScale,
                groundPatchBlendStrength,
                groundSnowTintStrength,
                groundSnowBrightness,
                groundDampDarkenStrength);

            materialChanged |= DrawMaterialSubsection(
                ref showMaterialWeatherFinish,
                "Weather and Finish",
                wetness,
                wetDarkenStrength,
                wetPixelSoftening,
                wetSmoothnessBoost,
                frostStrength,
                frostContrast,
                monolithicFlatten,
                monolithicSmoothnessBoost,
                smoothness,
                specularStrength);

            return materialChanged;
        }

        private bool DrawSharedMaterialControlGroups(
            SerializedProperty materialControls)
        {
            bool materialChanged = false;
            SerializedProperty sharedBankSurfaceLayer =
                materialControls.FindPropertyRelative("bankSurfaceLayer");
            SerializedProperty sharedShoreHydrologyModifier =
                materialControls.FindPropertyRelative(
                    "shoreHydrologyModifier");

            materialChanged |= DrawRiverBankResponseSubsection(
                ref showMaterialRiverCoupledBank,
                sharedBankSurfaceLayer,
                materialControls.FindPropertyRelative("bankMaterialStrength"),
                materialControls.FindPropertyRelative("bankMaterialReach"),
                materialControls.FindPropertyRelative("immediateBankExposure"),
                materialControls.FindPropertyRelative("waterlineMaterialStrength"),
                materialControls.FindPropertyRelative("bankTransitionSoftness"),
                materialControls.FindPropertyRelative("outerBankExtension"),
                materialControls.FindPropertyRelative("outerBankStrength"),
                materialControls.FindPropertyRelative("outerBankFade"),
                materialControls.FindPropertyRelative(
                    "bankDetailScaleMultiplier"),
                materialControls.FindPropertyRelative(
                    "bankAuthoredColorStrengthMultiplier"),
                materialControls.FindPropertyRelative(
                    "bankAuthoredColorLightingMultiplier"),
                materialControls.FindPropertyRelative(
                    "bankDetailNormalStrengthMultiplier"),
                materialControls.FindPropertyRelative(
                    "bankDetailCavityStrengthMultiplier"),
                materialControls.FindPropertyRelative(
                    "bankDetailValueFormMultiplier"),
                materialControls.FindPropertyRelative(
                    "bankDetailFinishVariationMultiplier"),
                materialControls.FindPropertyRelative(
                    "bankLegacyPixelCellInfluenceMultiplier"),
                materialControls.FindPropertyRelative("vegetationRetreatStrength"),
                materialControls.FindPropertyRelative("snowMeltStrength"),
                materialControls.FindPropertyRelative("frostRetreatStrength"),
                materialControls.FindPropertyRelative("paintedAccentRetreatStrength"),
                sharedShoreHydrologyModifier,
                materialControls.FindPropertyRelative("shoreWetnessStrength"),
                materialControls.FindPropertyRelative("shoreWetnessReach"),
                materialControls.FindPropertyRelative("shoreWetnessFade"),
                materialControls.FindPropertyRelative("broadBankSaturation"),
                materialControls.FindPropertyRelative("immediateBankSaturation"),
                materialControls.FindPropertyRelative("waterlineSaturation"),
                materialControls.FindPropertyRelative("shoreWetHighlightWidth"),
                materialControls.FindPropertyRelative("shoreWetHighlightFeather"),
                materialControls.FindPropertyRelative("shoreWetHighlightStrength"),
                materialControls.FindPropertyRelative("shoreWetHighlightTightness"),
                materialControls.FindPropertyRelative("shoreWetHighlightCameraBias"),
                materialControls.FindPropertyRelative(
                    "shoreWetHighlightVerticalFalloff"));

            materialChanged |= DrawRiverbedResponseSubsection(
                ref showMaterialRiverCoupledRiverbed,
                materialControls.FindPropertyRelative("riverbedSurfaceSource"),
                sharedBankSurfaceLayer,
                materialControls.FindPropertyRelative("riverbedSurfaceLayer"),
                materialControls.FindPropertyRelative("riverbedMaterialStrength"),
                materialControls.FindPropertyRelative(
                    "riverbedDetailScaleMultiplier"),
                materialControls.FindPropertyRelative(
                    "riverbedAuthoredColorStrengthMultiplier"),
                materialControls.FindPropertyRelative(
                    "riverbedAuthoredColorLightingMultiplier"),
                materialControls.FindPropertyRelative(
                    "riverbedDetailNormalStrengthMultiplier"),
                materialControls.FindPropertyRelative(
                    "riverbedDetailCavityStrengthMultiplier"),
                materialControls.FindPropertyRelative(
                    "riverbedDetailValueFormMultiplier"),
                materialControls.FindPropertyRelative(
                    "riverbedDetailFinishVariationMultiplier"),
                materialControls.FindPropertyRelative(
                    "riverbedLegacyPixelCellInfluenceMultiplier"),
                materialControls.FindPropertyRelative("riverbedHydrologySource"),
                sharedShoreHydrologyModifier,
                materialControls.FindPropertyRelative("riverbedHydrologyModifier"),
                materialControls.FindPropertyRelative("riverbedWetnessStrength"),
                materialControls.FindPropertyRelative(
                    "riverbedToBankWetnessBlendDistance"),
                materialControls.FindPropertyRelative(
                    "riverbedToBankWetnessBlendSoftness"),
                materialControls.FindPropertyRelative(
                    "riverbedWetSmoothnessResponse"),
                materialControls.FindPropertyRelative(
                    "riverbedWetSpecularResponse"));

            materialChanged |= DrawMaterialSubsection(
                ref showMaterialMacroPatchComposition,
                "Macro Patch Composition",
                materialControls.FindPropertyRelative("groundMacroPatchScale"),
                materialControls.FindPropertyRelative(
                    "groundMacroPatchPatternSeed"),
                materialControls.FindPropertyRelative("broadVariation"),
                materialControls.FindPropertyRelative(
                    "groundMacroPatchTransitionSoftness"),
                materialControls.FindPropertyRelative(
                    "groundMacroPatchSeparation"));

            materialChanged |= DrawMaterialSubsection(
                ref showMaterialElevationReadability,
                "Elevation Readability",
                materialControls.FindPropertyRelative(
                    "reliefShadingStrength"),
                materialControls.FindPropertyRelative(
                    "relativeHeightContrast"));

            materialChanged |= DrawMaterialSubsection(
                ref showMaterialPalette,
                "Palette",
                materialControls.FindPropertyRelative("baseColor"),
                materialControls.FindPropertyRelative("frostColor"),
                materialControls.FindPropertyRelative("dampTint"),
                materialControls.FindPropertyRelative("dampTintStrength"),
                materialControls.FindPropertyRelative("rockyDryTint"),
                materialControls.FindPropertyRelative("rockyDryTintStrength"),
                materialControls.FindPropertyRelative("vegetationTint"),
                materialControls.FindPropertyRelative("vegetationTintStrength"));

            materialChanged |= DrawMaterialSubsection(
                ref showMaterialPixelVariation,
                "Pixel Variation",
                materialControls.FindPropertyRelative("pixelCellSize"),
                materialControls.FindPropertyRelative("pixelToneCount"),
                materialControls.FindPropertyRelative("pixelClusterStrength"),
                materialControls.FindPropertyRelative("pixelVariation"),
                materialControls.FindPropertyRelative("vertexVariation"),
                materialControls.FindPropertyRelative("pixelEffectStrength"),
                materialControls.FindPropertyRelative("cellWarpStrength"));

            materialChanged |= DrawMaterialSubsection(
                ref showMaterialSemanticResponse,
                "Semantic Response",
                materialControls.FindPropertyRelative("profileContrastScale"),
                materialControls.FindPropertyRelative("profilePixelContrastScale"),
                materialControls.FindPropertyRelative("groundSnowResponseScale"),
                materialControls.FindPropertyRelative("groundDampResponseScale"),
                materialControls.FindPropertyRelative("groundVegetationResponseScale"),
                materialControls.FindPropertyRelative("groundRockyDryResponseScale"),
                materialControls.FindPropertyRelative("groundPatchBlendStrength"),
                materialControls.FindPropertyRelative("groundSnowTintStrength"),
                materialControls.FindPropertyRelative("groundSnowBrightness"),
                materialControls.FindPropertyRelative("groundDampDarkenStrength"));

            materialChanged |= DrawMaterialSubsection(
                ref showMaterialWeatherFinish,
                "Weather and Finish",
                materialControls.FindPropertyRelative("wetness"),
                materialControls.FindPropertyRelative("wetDarkenStrength"),
                materialControls.FindPropertyRelative("wetPixelSoftening"),
                materialControls.FindPropertyRelative("wetSmoothnessBoost"),
                materialControls.FindPropertyRelative("frostStrength"),
                materialControls.FindPropertyRelative("frostContrast"),
                materialControls.FindPropertyRelative("monolithicFlatten"),
                materialControls.FindPropertyRelative("monolithicSmoothnessBoost"),
                materialControls.FindPropertyRelative("smoothness"),
                materialControls.FindPropertyRelative("specularStrength"));

            return materialChanged;
        }

        private bool DrawRiverBankResponseSubsection(
            ref bool expanded,
            SerializedProperty bankLayer,
            SerializedProperty bankMaterialStrengthProperty,
            SerializedProperty coreBankReach,
            SerializedProperty immediateExposure,
            SerializedProperty waterlineStrength,
            SerializedProperty transitionSoftness,
            SerializedProperty extension,
            SerializedProperty extensionStrength,
            SerializedProperty extensionFade,
            SerializedProperty detailScaleMultiplier,
            SerializedProperty authoredColorStrengthMultiplier,
            SerializedProperty authoredColorLightingMultiplier,
            SerializedProperty detailNormalStrengthMultiplier,
            SerializedProperty detailCavityStrengthMultiplier,
            SerializedProperty detailValueFormMultiplier,
            SerializedProperty detailFinishVariationMultiplier,
            SerializedProperty legacyPixelCellInfluenceMultiplier,
            SerializedProperty vegetationRetreat,
            SerializedProperty snowRetreat,
            SerializedProperty frostRetreat,
            SerializedProperty paintedAccentRetreat,
            SerializedProperty shoreModifier,
            SerializedProperty shoreStrength,
            SerializedProperty shoreReach,
            SerializedProperty shoreFade,
            SerializedProperty broadSaturation,
            SerializedProperty immediateSaturation,
            SerializedProperty waterlineSaturationProperty,
            SerializedProperty wetHighlightWidth,
            SerializedProperty wetHighlightFeather,
            SerializedProperty wetHighlightStrength,
            SerializedProperty wetHighlightTightness,
            SerializedProperty wetHighlightCameraBias,
            SerializedProperty wetHighlightVerticalFalloff)
        {
            expanded = EditorGUILayout.Foldout(
                expanded,
                "River-Coupled Ground Response — River Bank",
                true);

            if (!expanded)
            {
                return false;
            }

            EditorGUI.indentLevel++;
            bool changed = false;

            EditorGUILayout.LabelField(
                "Substrate",
                EditorStyles.miniBoldLabel);
            changed |= DrawSurfaceLayerSelector(
                "Bank Surface Layer",
                "Reusable dry substrate exposed across the River bank. Inherit Primary Ground disables secondary Bank material composition.",
                bankLayer,
                ref showBankSurfaceLayerSettings,
                "Material & Layer Settings");

            bool hasBankLayer =
                bankLayer != null &&
                bankLayer.objectReferenceValue != null;

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField(
                "This River Application",
                EditorStyles.miniBoldLabel);
            EditorGUILayout.HelpBox(
                "These neutral multipliers affect only this Bank application. Shared palette, packed detail, cavity shaping, natural scale, and dry finish remain owned by the reusable material definition above.",
                MessageType.None);
            using (new EditorGUI.DisabledScope(!hasBankLayer))
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(detailScaleMultiplier);
                EditorGUILayout.PropertyField(authoredColorStrengthMultiplier);
                EditorGUILayout.PropertyField(authoredColorLightingMultiplier);
                EditorGUILayout.PropertyField(detailNormalStrengthMultiplier);
                EditorGUILayout.PropertyField(detailCavityStrengthMultiplier);
                EditorGUILayout.PropertyField(detailValueFormMultiplier);
                EditorGUILayout.PropertyField(detailFinishVariationMultiplier);
                EditorGUILayout.PropertyField(
                    legacyPixelCellInfluenceMultiplier);
                changed |= EditorGUI.EndChangeCheck();
            }

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField(
                "Material Coverage",
                EditorStyles.miniBoldLabel);
            if (!hasBankLayer)
            {
                EditorGUILayout.HelpBox(
                    "Select a Bank Surface Layer to enable secondary Bank material coverage. Shore wetness remains independently available below.",
                    MessageType.Info);
            }

            using (new EditorGUI.DisabledScope(!hasBankLayer))
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(bankMaterialStrengthProperty);

                EditorGUILayout.Space(2f);
                EditorGUILayout.LabelField(
                    "Core Bank",
                    EditorStyles.miniBoldLabel);
                EditorGUILayout.PropertyField(coreBankReach);
                EditorGUILayout.PropertyField(immediateExposure);
                EditorGUILayout.PropertyField(waterlineStrength);
                EditorGUILayout.PropertyField(transitionSoftness);

                EditorGUILayout.Space(2f);
                EditorGUILayout.LabelField(
                    "Outer Bank Extension",
                    EditorStyles.miniBoldLabel);
                EditorGUILayout.HelpBox(
                    "Starts at the Ground Riverbed Support edge and extends outward across the generated River corridor toward its terrain handoff.",
                    MessageType.None);
                EditorGUILayout.PropertyField(extension);
                using (new EditorGUI.DisabledScope(
                    extension == null ||
                    extension.floatValue <= 0.0001f))
                {
                    EditorGUILayout.PropertyField(extensionStrength);
                    EditorGUILayout.PropertyField(extensionFade);
                }

                changed |= EditorGUI.EndChangeCheck();
            }

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField(
                "Surface Cover",
                EditorStyles.miniBoldLabel);
            if (!hasBankLayer)
            {
                EditorGUILayout.HelpBox(
                    "Surface-cover retreat requires a selected Bank Surface Layer because retention values are authored by that layer.",
                    MessageType.Info);
            }

            using (new EditorGUI.DisabledScope(!hasBankLayer))
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(vegetationRetreat);
                EditorGUILayout.PropertyField(snowRetreat);
                EditorGUILayout.PropertyField(frostRetreat);
                EditorGUILayout.PropertyField(paintedAccentRetreat);
                changed |= EditorGUI.EndChangeCheck();
            }

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField(
                "Shore Wetness",
                EditorStyles.miniBoldLabel);
            changed |= DrawHydrologyModifierSelector(
                shoreModifier,
                ref showShoreHydrologyModifierSettings,
                "Shore Hydrology Modifier",
                "Reusable wetness character applied independently from Bank substrate reach.",
                "Disabled",
                "Disable local Shore hydrology while preserving global Ground wetness.",
                "Wetness Character");

            bool hasShoreModifier =
                shoreModifier != null &&
                shoreModifier.objectReferenceValue != null;
            if (!hasShoreModifier)
            {
                EditorGUILayout.HelpBox(
                    "Select a Shore Hydrology Modifier to enable Bank-side local wetness. Bank substrate selection is not required.",
                    MessageType.Info);
            }
            else
            {
                EditorGUILayout.LabelField(
                    "Spatial Application",
                    EditorStyles.miniBoldLabel);
                EditorGUILayout.HelpBox(
                    "Shore Wetness Reach is measured independently from Bank material reach.",
                    MessageType.None);
            }

            using (new EditorGUI.DisabledScope(!hasShoreModifier))
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(shoreStrength);
                EditorGUILayout.PropertyField(shoreReach);
                EditorGUILayout.PropertyField(shoreFade);
                EditorGUILayout.PropertyField(broadSaturation);
                EditorGUILayout.PropertyField(immediateSaturation);
                EditorGUILayout.PropertyField(waterlineSaturationProperty);

                EditorGUILayout.Space(2f);
                EditorGUILayout.LabelField(
                    "Wet Highlight Shaping",
                    EditorStyles.miniBoldLabel);
                EditorGUILayout.HelpBox(
                    "Highlight Width and Highlight Feather confine the stylized Shore highlight to a narrow waterline band. Camera-Centred Bias then transfers Shore wet finish from the broad physical PBR response into that active-camera band. The effect remains masked by local Shore wetness and does not apply to the Riverbed.",
                    MessageType.None);
                EditorGUILayout.PropertyField(wetHighlightWidth);
                EditorGUILayout.PropertyField(wetHighlightFeather);
                EditorGUILayout.PropertyField(wetHighlightStrength);
                EditorGUILayout.PropertyField(wetHighlightTightness);
                EditorGUILayout.PropertyField(wetHighlightCameraBias);
                EditorGUILayout.PropertyField(wetHighlightVerticalFalloff);
                changed |= EditorGUI.EndChangeCheck();
            }

            EditorGUI.indentLevel--;
            return changed;
        }

        private bool DrawRiverbedResponseSubsection(
            ref bool expanded,
            SerializedProperty surfaceSource,
            SerializedProperty bankLayer,
            SerializedProperty customRiverbedLayer,
            SerializedProperty materialStrength,
            SerializedProperty detailScaleMultiplier,
            SerializedProperty authoredColorStrengthMultiplier,
            SerializedProperty authoredColorLightingMultiplier,
            SerializedProperty detailNormalStrengthMultiplier,
            SerializedProperty detailCavityStrengthMultiplier,
            SerializedProperty detailValueFormMultiplier,
            SerializedProperty detailFinishVariationMultiplier,
            SerializedProperty legacyPixelCellInfluenceMultiplier,
            SerializedProperty hydrologySource,
            SerializedProperty shoreHydrologyModifierProperty,
            SerializedProperty customRiverbedHydrologyModifier,
            SerializedProperty wetnessStrength,
            SerializedProperty wetnessBlendDistance,
            SerializedProperty wetnessBlendSoftness,
            SerializedProperty smoothnessResponse,
            SerializedProperty specularResponse)
        {
            expanded = EditorGUILayout.Foldout(
                expanded,
                "River-Coupled Ground Response — Riverbed",
                true);

            if (!expanded)
            {
                return false;
            }

            EditorGUI.indentLevel++;
            bool changed = false;

            EditorGUILayout.LabelField(
                "Substrate",
                EditorStyles.miniBoldLabel);
            GroundRiverbedSurfaceSource resolvedSurfaceSource =
                ResolveRiverbedSurfaceSource(
                    surfaceSource,
                    customRiverbedLayer);
            int displayedSurfaceSource = resolvedSurfaceSource switch
            {
                GroundRiverbedSurfaceSource.InheritBankSurfaceLayer => 1,
                GroundRiverbedSurfaceSource.CustomRiverbedSurfaceLayer => 2,
                _ => 0
            };
            GUIContent[] surfaceSourceOptions =
            {
                new GUIContent(
                    "Primary Ground",
                    "Use the ordinary primary Ground substrate on Ground Riverbed Support."),
                new GUIContent(
                    "Inherit Bank Surface Layer",
                    "Use the currently resolved Bank Surface Layer. If Bank inherits primary Ground, Riverbed also resolves to primary Ground."),
                new GUIContent(
                    "Custom Riverbed Surface Layer",
                    "Use an independently selected reusable Riverbed substrate.")
            };

            EditorGUI.BeginChangeCheck();
            int selectedSurfaceSource = EditorGUILayout.Popup(
                new GUIContent(
                    "Riverbed Surface Source",
                    "Controls which dry substrate is resolved on Ground Riverbed Support."),
                displayedSurfaceSource,
                surfaceSourceOptions);
            if (EditorGUI.EndChangeCheck() && surfaceSource != null)
            {
                surfaceSource.intValue = selectedSurfaceSource switch
                {
                    1 => (int)GroundRiverbedSurfaceSource.InheritBankSurfaceLayer,
                    2 => (int)GroundRiverbedSurfaceSource.CustomRiverbedSurfaceLayer,
                    _ => (int)GroundRiverbedSurfaceSource.PrimaryGround
                };
                resolvedSurfaceSource =
                    (GroundRiverbedSurfaceSource)surfaceSource.intValue;
                changed = true;
            }

            if (resolvedSurfaceSource ==
                GroundRiverbedSurfaceSource.CustomRiverbedSurfaceLayer)
            {
                changed |= DrawSurfaceLayerSelector(
                    "Custom Riverbed Surface Layer",
                    "Reusable dry substrate applied only on Ground Riverbed Support.",
                    customRiverbedLayer,
                    ref showRiverbedSurfaceLayerSettings,
                    "Material & Layer Settings",
                    "No Custom Riverbed Surface Layer",
                    "Select or create a custom Riverbed substrate.");
            }

            bool hasResolvedRiverbedLayer =
                resolvedSurfaceSource switch
                {
                    GroundRiverbedSurfaceSource.InheritBankSurfaceLayer =>
                        bankLayer != null &&
                        bankLayer.objectReferenceValue != null,
                    GroundRiverbedSurfaceSource.CustomRiverbedSurfaceLayer =>
                        customRiverbedLayer != null &&
                        customRiverbedLayer.objectReferenceValue != null,
                    _ => false
                };

            if (resolvedSurfaceSource ==
                    GroundRiverbedSurfaceSource.InheritBankSurfaceLayer &&
                !hasResolvedRiverbedLayer)
            {
                EditorGUILayout.HelpBox(
                    "The Bank Surface Layer currently inherits Primary Ground, so the Riverbed also resolves to Primary Ground.",
                    MessageType.Info);
            }
            else if (resolvedSurfaceSource ==
                     GroundRiverbedSurfaceSource.PrimaryGround)
            {
                EditorGUILayout.HelpBox(
                    "Primary Ground applies no secondary Riverbed substrate. Submerged cover exclusion and Riverbed wetness remain independent.",
                    MessageType.None);
            }
            else if (!hasResolvedRiverbedLayer)
            {
                EditorGUILayout.HelpBox(
                    "Select a Custom Riverbed Surface Layer to enable secondary Riverbed material composition.",
                    MessageType.Info);
            }

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField(
                "This River Application",
                EditorStyles.miniBoldLabel);
            if (resolvedSurfaceSource ==
                GroundRiverbedSurfaceSource.InheritBankSurfaceLayer &&
                hasResolvedRiverbedLayer)
            {
                EditorGUILayout.HelpBox(
                    "The shared material definition is editable in the River Bank subsection because this Riverbed inherits that layer. The multipliers below remain Riverbed-specific.",
                    MessageType.None);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "These neutral multipliers affect only this Riverbed application. Shared material identity remains owned by the selected reusable material.",
                    MessageType.None);
            }

            using (new EditorGUI.DisabledScope(!hasResolvedRiverbedLayer))
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(detailScaleMultiplier);
                EditorGUILayout.PropertyField(authoredColorStrengthMultiplier);
                EditorGUILayout.PropertyField(authoredColorLightingMultiplier);
                EditorGUILayout.PropertyField(detailNormalStrengthMultiplier);
                EditorGUILayout.PropertyField(detailCavityStrengthMultiplier);
                EditorGUILayout.PropertyField(detailValueFormMultiplier);
                EditorGUILayout.PropertyField(detailFinishVariationMultiplier);
                EditorGUILayout.PropertyField(
                    legacyPixelCellInfluenceMultiplier);
                changed |= EditorGUI.EndChangeCheck();
            }

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField(
                "Material Coverage",
                EditorStyles.miniBoldLabel);
            using (new EditorGUI.DisabledScope(!hasResolvedRiverbedLayer))
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(materialStrength);
                changed |= EditorGUI.EndChangeCheck();
            }

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField(
                "Submerged Cover",
                EditorStyles.miniBoldLabel);
            EditorGUILayout.LabelField(
                "Vegetation",
                "Excluded on Ground Riverbed Support");
            EditorGUILayout.LabelField(
                "Snow",
                "Excluded on Ground Riverbed Support");
            EditorGUILayout.LabelField(
                "Frost",
                "Excluded on Ground Riverbed Support");
            EditorGUILayout.LabelField(
                "Painted Accents",
                "Excluded from final rendering on Ground Riverbed Support");

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField(
                "Wetness",
                EditorStyles.miniBoldLabel);
            GroundRiverbedHydrologySource resolvedHydrologySource =
                ResolveRiverbedHydrologySource(hydrologySource);
            int displayedHydrologySource = resolvedHydrologySource switch
            {
                GroundRiverbedHydrologySource.CustomHydrologyModifier => 1,
                GroundRiverbedHydrologySource.Disabled => 2,
                _ => 0
            };
            GUIContent[] hydrologySourceOptions =
            {
                new GUIContent(
                    "Inherit Shore Hydrology Modifier",
                    "Use the Shore Hydrology Modifier's wetness character on exact Ground Riverbed Support."),
                new GUIContent(
                    "Custom Hydrology Modifier",
                    "Use an independently selected reusable wetness character on Ground Riverbed Support."),
                new GUIContent(
                    "Disabled",
                    "Disable local Riverbed wetness while preserving global Ground wetness.")
            };

            EditorGUI.BeginChangeCheck();
            int selectedHydrologySource = EditorGUILayout.Popup(
                new GUIContent(
                    "Riverbed Hydrology Source",
                    "Controls the wetness character applied on exact Ground Riverbed Support."),
                displayedHydrologySource,
                hydrologySourceOptions);
            if (EditorGUI.EndChangeCheck() && hydrologySource != null)
            {
                hydrologySource.intValue = selectedHydrologySource switch
                {
                    1 => (int)GroundRiverbedHydrologySource.CustomHydrologyModifier,
                    2 => (int)GroundRiverbedHydrologySource.Disabled,
                    _ => (int)GroundRiverbedHydrologySource.InheritShoreHydrologyModifier
                };
                resolvedHydrologySource =
                    (GroundRiverbedHydrologySource)hydrologySource.intValue;
                changed = true;
            }

            if (resolvedHydrologySource ==
                GroundRiverbedHydrologySource.CustomHydrologyModifier)
            {
                changed |= DrawHydrologyModifierSelector(
                    customRiverbedHydrologyModifier,
                    ref showRiverbedHydrologyModifierSettings,
                    "Custom Riverbed Hydrology Modifier",
                    "Reusable wetness character applied only on Ground Riverbed Support.",
                    "No Custom Hydrology Modifier",
                    "Select or create a custom Riverbed wetness modifier.",
                    "Wetness Character");
            }

            bool hasResolvedHydrologyModifier =
                resolvedHydrologySource switch
                {
                    GroundRiverbedHydrologySource.InheritShoreHydrologyModifier =>
                        shoreHydrologyModifierProperty != null &&
                        shoreHydrologyModifierProperty.objectReferenceValue != null,
                    GroundRiverbedHydrologySource.CustomHydrologyModifier =>
                        customRiverbedHydrologyModifier != null &&
                        customRiverbedHydrologyModifier.objectReferenceValue != null,
                    _ => false
                };

            if (resolvedHydrologySource ==
                    GroundRiverbedHydrologySource.InheritShoreHydrologyModifier &&
                !hasResolvedHydrologyModifier)
            {
                EditorGUILayout.HelpBox(
                    "No Shore Hydrology Modifier is selected, so inherited Riverbed wetness is disabled.",
                    MessageType.Info);
            }
            else if (resolvedHydrologySource ==
                     GroundRiverbedHydrologySource.Disabled)
            {
                EditorGUILayout.HelpBox(
                    "Local Riverbed wetness is disabled. Global Ground wetness can still affect the surface.",
                    MessageType.None);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "Riverbed wetness uses Ground Riverbed Support and can transition inward from resolved Bank-edge wetness to full Riverbed wetness in the interior.",
                    MessageType.None);
            }

            using (new EditorGUI.DisabledScope(!hasResolvedHydrologyModifier))
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(wetnessStrength);

                EditorGUILayout.Space(2f);
                EditorGUILayout.LabelField(
                    "Wetness Transition",
                    EditorStyles.miniBoldLabel);
                EditorGUILayout.HelpBox(
                    "Transitions Riverbed wetness inward from resolved Bank wetness at the Ground Riverbed Support edge to full Riverbed wetness in the interior. The transition remains entirely inside the Riverbed. Zero distance preserves the hard boundary.",
                    MessageType.None);
                EditorGUILayout.PropertyField(wetnessBlendDistance);
                using (new EditorGUI.DisabledScope(
                    wetnessBlendDistance == null ||
                    wetnessBlendDistance.floatValue <= 0.0001f))
                {
                    EditorGUILayout.PropertyField(wetnessBlendSoftness);
                }

                EditorGUILayout.Space(2f);
                EditorGUILayout.LabelField(
                    "Submerged Finish",
                    EditorStyles.miniBoldLabel);
                EditorGUILayout.HelpBox(
                    "These responses scale only the Riverbed modifier's smoothness and specular finish. Wet tint, darkening, pattern softening, and cover interaction remain active at zero.",
                    MessageType.None);
                EditorGUILayout.PropertyField(smoothnessResponse);
                EditorGUILayout.PropertyField(specularResponse);
                changed |= EditorGUI.EndChangeCheck();
            }

            EditorGUI.indentLevel--;
            return changed;
        }

        private static GroundRiverbedSurfaceSource ResolveRiverbedSurfaceSource(
            SerializedProperty source,
            SerializedProperty customLayer)
        {
            GroundRiverbedSurfaceSource serializedSource =
                source != null
                    ? (GroundRiverbedSurfaceSource)source.intValue
                    : GroundRiverbedSurfaceSource.LegacyAuto;
            if (serializedSource != GroundRiverbedSurfaceSource.LegacyAuto)
            {
                return serializedSource;
            }

            return customLayer != null &&
                   customLayer.objectReferenceValue != null
                ? GroundRiverbedSurfaceSource.CustomRiverbedSurfaceLayer
                : GroundRiverbedSurfaceSource.PrimaryGround;
        }

        private static GroundRiverbedHydrologySource ResolveRiverbedHydrologySource(
            SerializedProperty source)
        {
            if (source == null)
            {
                return GroundRiverbedHydrologySource.InheritShoreHydrologyModifier;
            }

            int value = source.intValue;
            if (value < (int)GroundRiverbedHydrologySource.InheritShoreHydrologyModifier ||
                value > (int)GroundRiverbedHydrologySource.Disabled)
            {
                return GroundRiverbedHydrologySource.InheritShoreHydrologyModifier;
            }

            return (GroundRiverbedHydrologySource)value;
        }

        private bool DrawSurfaceLayerAuthoringSubsection(
            ref bool expanded,
            SerializedProperty bankLayer,
            SerializedProperty riverbedLayer)
        {
            expanded = EditorGUILayout.Foldout(
                expanded,
                "River-Coupled Ground Response — Surface Layers",
                true);

            if (!expanded)
            {
                return false;
            }

            EditorGUI.indentLevel++;
            EditorGUILayout.HelpBox(
                "Select and edit reusable dry Bank and Riverbed substrate assets here. Wetness character is authored separately in the Hydrology Modifier section.",
                MessageType.Info);

            bool changed = false;
            changed |= DrawSurfaceLayerSelector(
                "Bank Surface Layer",
                "Secondary substrate exposed across River banks.",
                bankLayer,
                ref showBankSurfaceLayerSettings,
                "Bank Layer Settings");

            EditorGUILayout.Space(4f);

            changed |= DrawSurfaceLayerSelector(
                "Riverbed Surface Layer",
                "Secondary substrate used on the visible submerged River corridor.",
                riverbedLayer,
                ref showRiverbedSurfaceLayerSettings,
                "Riverbed Layer Settings");

            EditorGUI.indentLevel--;
            return changed;
        }

        private static bool DrawSurfaceLayerSelector(
            string label,
            string tooltip,
            SerializedProperty layerProperty,
            ref bool settingsExpanded,
            string settingsLabel,
            string nullOptionLabel = "Inherit Primary Ground",
            string nullOptionTooltip =
                "Use the ordinary Ground material without a secondary layer asset.")
        {
            if (layerProperty == null)
            {
                EditorGUILayout.HelpBox(
                    $"The serialized {label} reference is unavailable.",
                    MessageType.Error);
                return false;
            }

            List<GroundSurfaceLayerProfile> profiles =
                GetSurfaceLayerProfiles();
            GroundSurfaceLayerProfile current =
                layerProperty.objectReferenceValue as
                    GroundSurfaceLayerProfile;

            int currentIndex = 0;
            for (int index = 0; index < profiles.Count; index++)
            {
                if (profiles[index] == current)
                {
                    currentIndex = index + 1;
                    break;
                }
            }

            if (current != null && currentIndex == 0)
            {
                profiles = new List<GroundSurfaceLayerProfile>(profiles)
                {
                    current
                };
                currentIndex = profiles.Count;
            }

            GUIContent[] options =
                new GUIContent[profiles.Count + 1];
            options[0] = new GUIContent(
                nullOptionLabel,
                nullOptionTooltip);

            for (int index = 0; index < profiles.Count; index++)
            {
                GroundSurfaceLayerProfile profile = profiles[index];
                string assetName = profile != null
                    ? profile.name
                    : "Missing Asset";
                string displayName = profile != null
                    ? profile.DisplayName
                    : assetName;
                string optionLabel = displayName == assetName
                    ? displayName
                    : $"{displayName} — {assetName}";
                string assetPath = profile != null
                    ? AssetDatabase.GetAssetPath(profile)
                    : string.Empty;

                options[index + 1] =
                    new GUIContent(optionLabel, assetPath);
            }

            EditorGUI.BeginChangeCheck();
            int selectedIndex = EditorGUILayout.Popup(
                new GUIContent(label, tooltip),
                currentIndex,
                options);
            bool selectionChanged = EditorGUI.EndChangeCheck();

            if (selectionChanged)
            {
                layerProperty.objectReferenceValue =
                    selectedIndex <= 0
                        ? null
                        : profiles[selectedIndex - 1];
                current = layerProperty.objectReferenceValue as
                    GroundSurfaceLayerProfile;
                settingsExpanded = current != null;
            }

            bool createRequested;
            bool duplicateRequested;
            using (new EditorGUILayout.HorizontalScope())
            {
                createRequested = GUILayout.Button("Create New Layer…");

                using (new EditorGUI.DisabledScope(current == null))
                {
                    duplicateRequested = GUILayout.Button(
                        "Duplicate Selected Layer…");
                }
            }

            if (createRequested || duplicateRequested)
            {
                settingsExpanded = true;
                ScheduleSurfaceLayerAssetCreation(
                    layerProperty,
                    duplicateRequested ? current : null);
            }

            if (current == null)
            {
                EditorGUILayout.LabelField(
                    "Layer Definition Stored In",
                    "Primary Ground material controls");
                return selectionChanged;
            }

            string path = AssetDatabase.GetAssetPath(current);
            EditorGUILayout.LabelField(
                new GUIContent(
                    "Layer Definition Stored In",
                    path),
                new GUIContent(
                    $"Surface Layer Asset — {current.name}",
                    path));

            settingsExpanded = EditorGUILayout.Foldout(
                settingsExpanded,
                settingsLabel,
                true);

            if (settingsExpanded)
            {
                EditorGUI.indentLevel++;
                DrawSurfaceLayerProfileEditor(current);
                EditorGUI.indentLevel--;
            }

            return selectionChanged;
        }

        private static List<GroundSurfaceLayerProfile>
            GetSurfaceLayerProfiles()
        {
            if (cachedSurfaceLayerProfiles != null)
            {
                return cachedSurfaceLayerProfiles;
            }

            string[] guids = AssetDatabase.FindAssets(
                "t:GroundSurfaceLayerProfile");
            cachedSurfaceLayerProfiles =
                new List<GroundSurfaceLayerProfile>(guids.Length);

            for (int index = 0; index < guids.Length; index++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[index]);
                GroundSurfaceLayerProfile profile =
                    AssetDatabase.LoadAssetAtPath<GroundSurfaceLayerProfile>(
                        path);
                if (profile != null)
                {
                    cachedSurfaceLayerProfiles.Add(profile);
                }
            }

            cachedSurfaceLayerProfiles.Sort(
                (left, right) =>
                {
                    int displayComparison = string.Compare(
                        left.DisplayName,
                        right.DisplayName,
                        System.StringComparison.OrdinalIgnoreCase);
                    return displayComparison != 0
                        ? displayComparison
                        : string.Compare(
                            left.name,
                            right.name,
                            System.StringComparison.OrdinalIgnoreCase);
                });

            return cachedSurfaceLayerProfiles;
        }

        private static void ScheduleSurfaceLayerAssetCreation(
            SerializedProperty layerProperty,
            GroundSurfaceLayerProfile source)
        {
            if (layerProperty == null)
            {
                return;
            }

            Object[] targets =
                (Object[])layerProperty.serializedObject.targetObjects.Clone();
            string propertyPath = layerProperty.propertyPath;

            EditorApplication.delayCall += () =>
            {
                GroundSurfaceLayerProfile created =
                    CreateSurfaceLayerAsset(source);
                if (created == null)
                {
                    return;
                }

                if (AssignCreatedProfileAsset(
                        targets,
                        propertyPath,
                        created,
                        "Assign Ground Surface Layer"))
                {
                    RefreshLoadedGroundsUsingSurfaceLayer(created);
                }
            };
        }

        private static bool AssignCreatedProfileAsset(
            Object[] targets,
            string propertyPath,
            Object createdAsset,
            string undoName)
        {
            if (targets == null ||
                targets.Length == 0 ||
                string.IsNullOrWhiteSpace(propertyPath) ||
                createdAsset == null)
            {
                return false;
            }

            List<Object> validTargets = new List<Object>(targets.Length);
            for (int index = 0; index < targets.Length; index++)
            {
                if (targets[index] != null)
                {
                    validTargets.Add(targets[index]);
                }
            }

            if (validTargets.Count == 0)
            {
                return false;
            }

            Object[] assignmentTargets = validTargets.ToArray();
            Undo.RecordObjects(assignmentTargets, undoName);

            SerializedObject assignmentObject =
                new SerializedObject(assignmentTargets);
            assignmentObject.UpdateIfRequiredOrScript();
            SerializedProperty assignmentProperty =
                assignmentObject.FindProperty(propertyPath);

            if (assignmentProperty == null)
            {
                Debug.LogError(
                    $"GeneratedGroundEditor could not assign '{createdAsset.name}' because serialized property '{propertyPath}' is unavailable.");
                return false;
            }

            assignmentProperty.objectReferenceValue = createdAsset;
            assignmentObject.ApplyModifiedProperties();

            for (int index = 0; index < assignmentTargets.Length; index++)
            {
                Object assignmentTarget = assignmentTargets[index];
                if (assignmentTarget is GeneratedGround ground)
                {
                    ground.MarkGroundVisualControlsCustom();
                    EditorUtility.SetDirty(ground);
                }
                else if (assignmentTarget is GroundSurfaceStyleProfile style)
                {
                    EditorUtility.SetDirty(style);
                    QueueSharedStyleSave(style);
                }
            }

            SceneView.RepaintAll();
            return true;
        }

        private static GroundSurfaceLayerProfile CreateSurfaceLayerAsset(
            GroundSurfaceLayerProfile source)
        {
            EnsureSurfaceLayerFolderExists();

            string suggestedName = source == null
                ? "GSLP_NewGroundSurfaceLayer"
                : source.name + "_Copy";
            string title = source == null
                ? "Create Ground Surface Layer"
                : "Duplicate Ground Surface Layer";
            string path = EditorUtility.SaveFilePanelInProject(
                title,
                suggestedName,
                "asset",
                "Choose where the reusable Ground surface-layer asset is stored.",
                DefaultSurfaceLayerFolder);

            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            path = AssetDatabase.GenerateUniqueAssetPath(path);

            GroundSurfaceLayerProfile layer = source == null
                ? ScriptableObject.CreateInstance<GroundSurfaceLayerProfile>()
                : Object.Instantiate(source);
            layer.name = Path.GetFileNameWithoutExtension(path);
            layer.SetDisplayName(
                source == null
                    ? ObjectNames.NicifyVariableName(
                        layer.name.StartsWith("GSLP_")
                            ? layer.name.Substring(5)
                            : layer.name)
                    : source.DisplayName + " Copy");

            AssetDatabase.CreateAsset(layer, path);
            EditorUtility.SetDirty(layer);
            AssetDatabase.SaveAssetIfDirty(layer);
            InvalidateSurfaceLayerProfileCache();
            return layer;
        }

        private static void EnsureSurfaceLayerFolderExists()
        {
            if (AssetDatabase.IsValidFolder(DefaultSurfaceLayerFolder))
            {
                return;
            }

            string[] parts = DefaultSurfaceLayerFolder.Split('/');
            string current = parts[0];

            for (int index = 1; index < parts.Length; index++)
            {
                string next = current + "/" + parts[index];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[index]);
                }

                current = next;
            }
        }

        private static void DrawSurfaceLayerProfileEditor(
            GroundSurfaceLayerProfile profile)
        {
            if (profile == null)
            {
                return;
            }

            SerializedObject layerObject = new SerializedObject(profile);
            layerObject.UpdateIfRequiredOrScript();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.LabelField(
                "Identity",
                EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(
                layerObject.FindProperty("displayName"));

            EditorGUILayout.Space(2f);
            EditorGUILayout.LabelField(
                "Reusable Material",
                EditorStyles.miniBoldLabel);
            SerializedProperty surfaceMaterialProperty =
                layerObject.FindProperty("surfaceMaterial");
            EditorGUILayout.PropertyField(surfaceMaterialProperty);
            StylizedSurfaceMaterialProfile surfaceMaterial =
                surfaceMaterialProperty != null
                    ? surfaceMaterialProperty.objectReferenceValue as
                        StylizedSurfaceMaterialProfile
                    : null;

            if (surfaceMaterial != null)
            {
                string materialPath =
                    AssetDatabase.GetAssetPath(surfaceMaterial);
                EditorGUILayout.LabelField(
                    new GUIContent("Material Definition Stored In", materialPath),
                    new GUIContent(
                        $"Stylized Surface Material — {surfaceMaterial.name}",
                        materialPath));

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Select Material Asset"))
                    {
                        Selection.activeObject = surfaceMaterial;
                    }

                    if (GUILayout.Button("Ping Material Asset"))
                    {
                        EditorGUIUtility.PingObject(surfaceMaterial);
                    }
                }

                EditorGUILayout.HelpBox(
                    "Palette, structural detail, cavity, natural scale, and dry finish are owned by the reusable material asset. This Ground layer retains only cover compatibility.",
                    MessageType.Info);

                DrawStylizedSurfaceMaterialInlineEditor(surfaceMaterial);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "No reusable material is assigned. The legacy serialized appearance below remains active for compatibility.",
                    MessageType.None);

                EditorGUILayout.Space(2f);
                EditorGUILayout.LabelField(
                    "Legacy Appearance Fallback",
                    EditorStyles.miniBoldLabel);
                EditorGUILayout.PropertyField(
                    layerObject.FindProperty("baseColor"));
                EditorGUILayout.PropertyField(
                    layerObject.FindProperty("darkColor"));
                EditorGUILayout.PropertyField(
                    layerObject.FindProperty("lightColor"));
                EditorGUILayout.PropertyField(
                    layerObject.FindProperty("macroContrast"));
                EditorGUILayout.PropertyField(
                    layerObject.FindProperty("pixelContrast"));
                EditorGUILayout.PropertyField(
                    layerObject.FindProperty("drySmoothness"));
                EditorGUILayout.PropertyField(
                    layerObject.FindProperty("drySpecularStrength"));
            }


            EditorGUILayout.Space(2f);
            EditorGUILayout.LabelField(
                "Cover Compatibility",
                EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(
                layerObject.FindProperty("vegetationRetention"));
            EditorGUILayout.PropertyField(
                layerObject.FindProperty("snowRetention"));
            EditorGUILayout.PropertyField(
                layerObject.FindProperty("frostRetention"));
            EditorGUILayout.PropertyField(
                layerObject.FindProperty("paintedAccentRetention"));

            bool changed = EditorGUI.EndChangeCheck();
            bool applied = layerObject.ApplyModifiedProperties();

            if (changed || applied)
            {
                EditorUtility.SetDirty(profile);
                QueueSurfaceLayerSave(profile);
                RefreshLoadedGroundsUsingSurfaceLayer(profile);
                InvalidateSurfaceLayerProfileCache();
            }
        }

        private static void DrawStylizedSurfaceMaterialInlineEditor(
            StylizedSurfaceMaterialProfile profile)
        {
            if (profile == null)
            {
                return;
            }

            EntityId foldoutKey = profile.GetEntityId();
            bool expanded =
                SharedSurfaceMaterialFoldouts.TryGetValue(
                    foldoutKey,
                    out bool storedExpanded) &&
                storedExpanded;
            expanded = EditorGUILayout.Foldout(
                expanded,
                "Shared Material Definition",
                true);
            SharedSurfaceMaterialFoldouts[foldoutKey] = expanded;

            if (!expanded)
            {
                return;
            }

            EditorGUI.indentLevel++;
            EditorGUILayout.HelpBox(
                $"Editing Shared Material — changes every Ground, River, road, wall, or other consumer of {profile.name}.",
                MessageType.Warning);

            SerializedObject materialObject = new SerializedObject(profile);
            materialObject.UpdateIfRequiredOrScript();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.LabelField(
                "Payload",
                EditorStyles.miniBoldLabel);
            DrawSerializedProperties(materialObject, "payloadMode");
            SerializedProperty payloadMode =
                materialObject.FindProperty("payloadMode");
            bool usesAuthoredColor =
                payloadMode != null &&
                payloadMode.enumValueIndex ==
                    (int)StylizedSurfaceMaterialPayloadMode.AuthoredColor;
            if (usesAuthoredColor)
            {
                DrawSerializedProperties(
                    materialObject,
                    "authoredColorStrength",
                    "authoredColorTint",
                    "authoredColorTintStrength",
                    "authoredColorLightingStrength",
                    "authoredRoughnessStrength");
            }

            EditorGUILayout.Space(2f);
            EditorGUILayout.LabelField(
                "Palette",
                EditorStyles.miniBoldLabel);
            DrawSerializedProperties(
                materialObject,
                "baseColor",
                "darkColor",
                "lightColor",
                "cavityColor");

            EditorGUILayout.Space(2f);
            EditorGUILayout.LabelField(
                "Broad Response",
                EditorStyles.miniBoldLabel);
            DrawSerializedProperties(
                materialObject,
                "macroContrast",
                "legacyPixelCellInfluence");
            if (!usesAuthoredColor)
            {
                DrawSerializedProperties(
                    materialObject,
                    "detailValueStrength");
            }

            EditorGUILayout.Space(2f);
            EditorGUILayout.LabelField(
                "Structural Detail",
                EditorStyles.miniBoldLabel);
            DrawSerializedProperties(
                materialObject,
                "detailEnabled",
                "detailLibrary",
                "detailEntryId",
                "detailWorldScale",
                "detailNormalStrength",
                "detailCavityStrength",
                "detailCavityBias");
            if (!usesAuthoredColor)
            {
                DrawSerializedProperties(
                    materialObject,
                    "detailFormHighlightStrength");
            }

            EditorGUILayout.Space(2f);
            EditorGUILayout.LabelField(
                "Dry Finish",
                EditorStyles.miniBoldLabel);
            DrawSerializedProperties(
                materialObject,
                "drySmoothness",
                "drySpecularStrength");
            if (!usesAuthoredColor)
            {
                DrawSerializedProperties(
                    materialObject,
                    "finishVariationStrength");
            }

            bool changed = EditorGUI.EndChangeCheck();
            bool applied = materialObject.ApplyModifiedProperties();
            if (changed || applied)
            {
                EditorUtility.SetDirty(profile);
                profile.NotifyEditorChanged();
                SceneView.RepaintAll();
            }

            EditorGUI.indentLevel--;
        }

        private static void DrawSerializedProperties(
            SerializedObject source,
            params string[] propertyNames)
        {
            if (source == null || propertyNames == null)
            {
                return;
            }

            for (int index = 0; index < propertyNames.Length; index++)
            {
                SerializedProperty property =
                    source.FindProperty(propertyNames[index]);
                if (property != null)
                {
                    EditorGUILayout.PropertyField(property);
                }
            }
        }

        private static bool DrawHydrologyModifierSelector(
            SerializedProperty modifierProperty,
            ref bool settingsExpanded,
            string label = "Shore Hydrology Modifier",
            string tooltip =
                "Reusable wetness character applied independently from Bank substrate reach.",
            string nullOptionLabel = "Disabled",
            string nullOptionTooltip =
                "Disable local Shore hydrology while preserving global Ground wetness.",
            string settingsLabel = "Wetness Character")
        {
            if (modifierProperty == null)
            {
                EditorGUILayout.HelpBox(
                    $"The serialized {label} reference is unavailable.",
                    MessageType.Error);
                return false;
            }

            List<GroundHydrologyModifierProfile> profiles =
                GetHydrologyModifierProfiles();
            GroundHydrologyModifierProfile current =
                modifierProperty.objectReferenceValue as
                    GroundHydrologyModifierProfile;

            int currentIndex = 0;
            for (int index = 0; index < profiles.Count; index++)
            {
                if (profiles[index] == current)
                {
                    currentIndex = index + 1;
                    break;
                }
            }

            if (current != null && currentIndex == 0)
            {
                profiles = new List<GroundHydrologyModifierProfile>(profiles)
                {
                    current
                };
                currentIndex = profiles.Count;
            }

            GUIContent[] options = new GUIContent[profiles.Count + 1];
            options[0] = new GUIContent(
                nullOptionLabel,
                nullOptionTooltip);

            for (int index = 0; index < profiles.Count; index++)
            {
                GroundHydrologyModifierProfile profile = profiles[index];
                string assetName = profile != null
                    ? profile.name
                    : "Missing Asset";
                string displayName = profile != null
                    ? profile.DisplayName
                    : assetName;
                string optionLabel = displayName == assetName
                    ? displayName
                    : $"{displayName} — {assetName}";
                string assetPath = profile != null
                    ? AssetDatabase.GetAssetPath(profile)
                    : string.Empty;
                options[index + 1] =
                    new GUIContent(optionLabel, assetPath);
            }

            EditorGUI.BeginChangeCheck();
            int selectedIndex = EditorGUILayout.Popup(
                new GUIContent(label, tooltip),
                currentIndex,
                options);
            bool selectionChanged = EditorGUI.EndChangeCheck();

            if (selectionChanged)
            {
                modifierProperty.objectReferenceValue =
                    selectedIndex <= 0
                        ? null
                        : profiles[selectedIndex - 1];
                current = modifierProperty.objectReferenceValue as
                    GroundHydrologyModifierProfile;
                settingsExpanded = current != null;
            }

            bool createRequested;
            bool duplicateRequested;
            using (new EditorGUILayout.HorizontalScope())
            {
                createRequested = GUILayout.Button(
                    "Create New Hydrology Modifier…");

                using (new EditorGUI.DisabledScope(current == null))
                {
                    duplicateRequested = GUILayout.Button(
                        "Duplicate Selected Modifier…");
                }
            }

            if (createRequested || duplicateRequested)
            {
                settingsExpanded = true;
                ScheduleHydrologyModifierAssetCreation(
                    modifierProperty,
                    duplicateRequested ? current : null);
            }

            if (current == null)
            {
                EditorGUILayout.LabelField(
                    "Modifier Definition Stored In",
                    nullOptionLabel);
                return selectionChanged;
            }

            string path = AssetDatabase.GetAssetPath(current);
            EditorGUILayout.LabelField(
                new GUIContent("Modifier Definition Stored In", path),
                new GUIContent(
                    $"Hydrology Modifier Asset — {current.name}",
                    path));

            settingsExpanded = EditorGUILayout.Foldout(
                settingsExpanded,
                settingsLabel,
                true);

            if (settingsExpanded)
            {
                EditorGUI.indentLevel++;
                DrawHydrologyModifierProfileEditor(current);
                EditorGUI.indentLevel--;
            }

            return selectionChanged;
        }

        private static List<GroundHydrologyModifierProfile>
            GetHydrologyModifierProfiles()
        {
            if (cachedHydrologyModifierProfiles != null)
            {
                return cachedHydrologyModifierProfiles;
            }

            string[] guids = AssetDatabase.FindAssets(
                "t:GroundHydrologyModifierProfile");
            cachedHydrologyModifierProfiles =
                new List<GroundHydrologyModifierProfile>(guids.Length);

            for (int index = 0; index < guids.Length; index++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[index]);
                GroundHydrologyModifierProfile profile =
                    AssetDatabase.LoadAssetAtPath<
                        GroundHydrologyModifierProfile>(path);
                if (profile != null)
                {
                    cachedHydrologyModifierProfiles.Add(profile);
                }
            }

            cachedHydrologyModifierProfiles.Sort(
                (left, right) =>
                {
                    int displayComparison = string.Compare(
                        left.DisplayName,
                        right.DisplayName,
                        System.StringComparison.OrdinalIgnoreCase);
                    return displayComparison != 0
                        ? displayComparison
                        : string.Compare(
                            left.name,
                            right.name,
                            System.StringComparison.OrdinalIgnoreCase);
                });

            return cachedHydrologyModifierProfiles;
        }

        private static void ScheduleHydrologyModifierAssetCreation(
            SerializedProperty modifierProperty,
            GroundHydrologyModifierProfile source)
        {
            if (modifierProperty == null)
            {
                return;
            }

            Object[] targets =
                (Object[])modifierProperty.serializedObject.targetObjects.Clone();
            string propertyPath = modifierProperty.propertyPath;

            EditorApplication.delayCall += () =>
            {
                GroundHydrologyModifierProfile created =
                    CreateHydrologyModifierAsset(source);
                if (created == null)
                {
                    return;
                }

                if (AssignCreatedProfileAsset(
                        targets,
                        propertyPath,
                        created,
                        "Assign Ground Hydrology Modifier"))
                {
                    RefreshLoadedGroundsUsingHydrologyModifier(created);
                }
            };
        }

        private static GroundHydrologyModifierProfile
            CreateHydrologyModifierAsset(
                GroundHydrologyModifierProfile source)
        {
            string suggestedName = source == null
                ? "GHMP_NewGroundHydrologyModifier"
                : source.name + "_Copy";
            string title = source == null
                ? "Create Ground Hydrology Modifier"
                : "Duplicate Ground Hydrology Modifier";
            string path = EditorUtility.SaveFilePanelInProject(
                title,
                suggestedName,
                "asset",
                "Choose where the reusable Ground hydrology modifier asset is stored.",
                DefaultHydrologyModifierFolder);

            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            path = AssetDatabase.GenerateUniqueAssetPath(path);
            GroundHydrologyModifierProfile modifier = source == null
                ? ScriptableObject.CreateInstance<
                    GroundHydrologyModifierProfile>()
                : Object.Instantiate(source);
            modifier.name = Path.GetFileNameWithoutExtension(path);
            modifier.SetDisplayName(
                source == null
                    ? ObjectNames.NicifyVariableName(
                        modifier.name.StartsWith("GHMP_")
                            ? modifier.name.Substring(5)
                            : modifier.name)
                    : source.DisplayName + " Copy");

            AssetDatabase.CreateAsset(modifier, path);
            EditorUtility.SetDirty(modifier);
            AssetDatabase.SaveAssetIfDirty(modifier);
            InvalidateSurfaceLayerProfileCache();
            return modifier;
        }

        private static void DrawHydrologyModifierProfileEditor(
            GroundHydrologyModifierProfile profile)
        {
            if (profile == null)
            {
                return;
            }

            SerializedObject modifierObject = new SerializedObject(profile);
            modifierObject.UpdateIfRequiredOrScript();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.LabelField(
                "Identity",
                EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(
                modifierObject.FindProperty("displayName"));

            EditorGUILayout.Space(2f);
            EditorGUILayout.LabelField(
                "Wet Colour Response",
                EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(
                modifierObject.FindProperty("wetTintColor"));
            EditorGUILayout.PropertyField(
                modifierObject.FindProperty("wetTintStrength"));
            EditorGUILayout.PropertyField(
                modifierObject.FindProperty("wetDarkening"));

            EditorGUILayout.Space(2f);
            EditorGUILayout.LabelField(
                "Surface Finish",
                EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(
                modifierObject.FindProperty("pixelPatternSoftening"));
            EditorGUILayout.PropertyField(
                modifierObject.FindProperty("smoothnessBoost"));
            EditorGUILayout.PropertyField(
                modifierObject.FindProperty("specularBoost"),
                new GUIContent(
                    "Specular Boost",
                    "Absolute neutral specular increase contributed at full local Shore wetness."));

            EditorGUILayout.Space(2f);
            EditorGUILayout.LabelField(
                "Cover Interaction",
                EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(
                modifierObject.FindProperty("snowMeltInfluence"));
            EditorGUILayout.PropertyField(
                modifierObject.FindProperty("frostMeltInfluence"));

            bool changed = EditorGUI.EndChangeCheck();
            bool applied = modifierObject.ApplyModifiedProperties();
            if (changed || applied)
            {
                EditorUtility.SetDirty(profile);
                QueueHydrologyModifierSave(profile);
                RefreshLoadedGroundsUsingHydrologyModifier(profile);
                InvalidateSurfaceLayerProfileCache();
            }
        }

        private bool DrawShoreHydrologySubsection(
            ref bool expanded,
            SerializedProperty modifierProperty,
            SerializedProperty strength,
            SerializedProperty reach,
            SerializedProperty fade,
            SerializedProperty broadSaturation,
            SerializedProperty immediateSaturation,
            SerializedProperty waterlineSaturation)
        {
            expanded = EditorGUILayout.Foldout(
                expanded,
                "River-Coupled Ground Response — Shore Hydrology",
                true);

            if (!expanded)
            {
                return false;
            }

            EditorGUI.indentLevel++;
            EditorGUILayout.HelpBox(
                "Wetness character and spatial reach are independent from Bank and Riverbed substrate identity.",
                MessageType.Info);

            EditorGUILayout.LabelField(
                "Hydrology Modifier",
                EditorStyles.miniBoldLabel);
            bool modifierChanged = DrawHydrologyModifierSelector(
                modifierProperty,
                ref showShoreHydrologyModifierSettings);

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField(
                "Spatial Application",
                EditorStyles.miniBoldLabel);

            bool hasModifier =
                modifierProperty != null &&
                modifierProperty.objectReferenceValue != null;

            if (!hasModifier)
            {
                EditorGUILayout.HelpBox(
                    "Select a Shore Hydrology Modifier to enable local wetness. Bank Surface Layer selection is not required.",
                    MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "Wetness reach is measured independently from the Riverbed Support boundary and does not follow Bank material reach.",
                    MessageType.None);
            }

            EditorGUI.BeginDisabledGroup(!hasModifier);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(strength);
            EditorGUILayout.PropertyField(reach);
            EditorGUILayout.PropertyField(fade);
            EditorGUILayout.PropertyField(broadSaturation);
            EditorGUILayout.PropertyField(immediateSaturation);
            EditorGUILayout.PropertyField(waterlineSaturation);
            bool spatialChanged = EditorGUI.EndChangeCheck();
            EditorGUI.EndDisabledGroup();
            EditorGUI.indentLevel--;
            return modifierChanged || spatialChanged;
        }

        private static bool DrawBankCompositionSubsection(
            ref bool expanded,
            SerializedProperty bankLayer,
            SerializedProperty bankMaterialStrength,
            SerializedProperty coreBankReach,
            SerializedProperty immediateBankExposure,
            SerializedProperty waterlineMaterialStrength,
            SerializedProperty coreTransitionSoftness,
            SerializedProperty outerBankExtension,
            SerializedProperty outerBankStrength,
            SerializedProperty outerBankFade)
        {
            expanded = EditorGUILayout.Foldout(
                expanded,
                "River-Coupled Ground Response — Bank Composition",
                true);

            if (!expanded)
            {
                return false;
            }

            EditorGUI.indentLevel++;
            bool hasBankLayer =
                bankLayer != null &&
                bankLayer.objectReferenceValue != null;

            if (!hasBankLayer)
            {
                EditorGUILayout.HelpBox(
                    "Select a Bank Surface Layer above to enable bank material-composition controls. Inherit Primary Ground intentionally produces no secondary bank substrate.",
                    MessageType.Info);
            }

            EditorGUI.BeginDisabledGroup(!hasBankLayer);
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(bankMaterialStrength);

            EditorGUILayout.Space(2f);
            EditorGUILayout.LabelField(
                "Core Bank",
                EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(coreBankReach);
            EditorGUILayout.PropertyField(immediateBankExposure);
            EditorGUILayout.PropertyField(waterlineMaterialStrength);
            EditorGUILayout.PropertyField(coreTransitionSoftness);

            EditorGUILayout.Space(2f);
            EditorGUILayout.LabelField(
                "Outer Bank Extension",
                EditorStyles.miniBoldLabel);
            EditorGUILayout.HelpBox(
                "Starts at the Riverbed Support edge and extends outward across the generated River corridor toward its terrain handoff.",
                MessageType.None);
            EditorGUILayout.PropertyField(outerBankExtension);
            EditorGUI.BeginDisabledGroup(
                outerBankExtension == null ||
                outerBankExtension.floatValue <= 0.0001f);
            EditorGUILayout.PropertyField(outerBankStrength);
            EditorGUILayout.PropertyField(outerBankFade);
            EditorGUI.EndDisabledGroup();

            bool changed = EditorGUI.EndChangeCheck();
            EditorGUI.EndDisabledGroup();
            EditorGUI.indentLevel--;
            return changed;
        }

        private static bool DrawRiverbedCompositionSubsection(
            ref bool expanded,
            SerializedProperty riverbedLayer,
            SerializedProperty riverbedMaterialStrength)
        {
            expanded = EditorGUILayout.Foldout(
                expanded,
                "River-Coupled Ground Response — Riverbed Composition",
                true);

            if (!expanded)
            {
                return false;
            }

            EditorGUI.indentLevel++;
            bool hasRiverbedLayer =
                riverbedLayer != null &&
                riverbedLayer.objectReferenceValue != null;

            if (!hasRiverbedLayer)
            {
                EditorGUILayout.HelpBox(
                    "Select a Riverbed Surface Layer above to enable dry Riverbed material composition. Submerged vegetation, snow, frost, and rendered Painted Accents remain excluded by exact Riverbed Support.",
                    MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "The selected dry substrate is applied only on exact Riverbed Support. Wetness remains outside this A4A control.",
                    MessageType.None);
            }

            EditorGUI.BeginDisabledGroup(!hasRiverbedLayer);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(riverbedMaterialStrength);
            bool changed = EditorGUI.EndChangeCheck();
            EditorGUI.EndDisabledGroup();
            EditorGUI.indentLevel--;
            return changed;
        }

        private static bool DrawBankCoverResponseSubsection(
            ref bool expanded,
            SerializedProperty bankLayer,
            SerializedProperty vegetationRetreatStrength,
            SerializedProperty snowMeltStrength,
            SerializedProperty frostRetreatStrength,
            SerializedProperty paintedAccentRetreatStrength)
        {
            expanded = EditorGUILayout.Foldout(
                expanded,
                "River-Coupled Ground Response — Surface-Cover Response",
                true);

            if (!expanded)
            {
                return false;
            }

            EditorGUI.indentLevel++;
            bool hasBankLayer =
                bankLayer != null &&
                bankLayer.objectReferenceValue != null;

            if (!hasBankLayer)
            {
                EditorGUILayout.HelpBox(
                    "Select a Bank Surface Layer above to author vegetation, snow, frost, and Painted Accent retention. Inherit Primary Ground preserves all ordinary cover.",
                    MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "Each control scales the selected layer's authored retention fraction across the existing Bank material blend. Zero preserves the current response.",
                    MessageType.None);
            }

            EditorGUI.BeginDisabledGroup(!hasBankLayer);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(vegetationRetreatStrength);
            EditorGUILayout.PropertyField(snowMeltStrength);
            EditorGUILayout.PropertyField(frostRetreatStrength);
            EditorGUILayout.PropertyField(paintedAccentRetreatStrength);
            bool changed = EditorGUI.EndChangeCheck();
            EditorGUI.EndDisabledGroup();
            EditorGUI.indentLevel--;
            return changed;
        }

        private static bool DrawMaterialSubsection(
            ref bool expanded,
            string label,
            params SerializedProperty[] properties)
        {
            expanded = EditorGUILayout.Foldout(
                expanded,
                label,
                true);

            if (!expanded)
            {
                return false;
            }

            EditorGUI.indentLevel++;
            EditorGUI.BeginChangeCheck();

            for (int index = 0; index < properties.Length; index++)
            {
                SerializedProperty property = properties[index];
                if (property != null)
                {
                    EditorGUILayout.PropertyField(property);
                }
            }

            bool changed = EditorGUI.EndChangeCheck();
            EditorGUI.indentLevel--;
            return changed;
        }

        private void DrawGroundInteractionSection()
        {
            if (!DrawSectionFoldout(
                    ref showGroundInteraction,
                    "Ground and Environment Interaction"))
            {
                return;
            }

            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(
                useModifiers,
                new GUIContent("Use Modifiers"));

            if (targets.Length == 1)
            {
                GeneratedGround ground = target as GeneratedGround;
                if (ground != null)
                {
                    EditorGUILayout.LabelField(
                        "Found Ground Modifiers",
                        ground.ModifierCount.ToString());
                    EditorGUILayout.LabelField(
                        "Found River Channels",
                        ground.RiverCount.ToString());
                }
            }

            EditorGUILayout.HelpBox(
                "GroundModifier and StylizedRiver components are discovered below this GeneratedGround object in the Hierarchy. Their own artistic controls remain on those components.",
                MessageType.Info);
            EditorGUI.indentLevel--;
        }

        private void DrawDebugAndDiagnosticsSection()
        {
            if (!DrawSectionFoldout(
                    ref showDebugAndDiagnostics,
                    "Debug and Diagnostics"))
            {
                return;
            }

            EditorGUI.indentLevel++;
            DrawGroundDebugControls();
            DrawCurrentRegenerationTimingControls();
            DrawSurfaceDiagnosticsControls();
            DrawPaintedAccentPlacementDebugControls();
            DrawPaintedAccentDiagnosticsControls();
            DrawRegenerationAccountingControls();

            EditorGUILayout.Space(4f);
            using (new EditorGUI.DisabledScope(Application.isPlaying))
            {
                if (GUILayout.Button(
                        "Validate Painted Accent Production in Build Scenes"))
                {
                    GroundPaintedAccentProductionValidator
                        .ShowBuildSceneValidationDialog();
                }

                EditorGUILayout.Space(4f);
                if (GUILayout.Button(
                        "Audit Generated Painted Accent Assets"))
                {
                    GroundPaintedAccentGeneratedAssetCleanupWindow
                        .OpenAndAudit();
                }

                using (new EditorGUI.DisabledScope(
                           !GroundPaintedAccentGeneratedAssetCleanup
                               .HasLastReport))
                {
                    if (GUILayout.Button(
                            "Copy Generated Asset Audit"))
                    {
                        GroundPaintedAccentGeneratedAssetCleanup
                            .CopyLastReport();
                    }
                }

                using (new EditorGUI.DisabledScope(
                           !GroundPaintedAccentGeneratedAssetCleanup
                               .HasLastReport ||
                           GroundPaintedAccentGeneratedAssetCleanup
                               .LastReport == null ||
                           !GroundPaintedAccentGeneratedAssetCleanup
                               .LastReport.CanDeleteConfirmedOrphans))
                {
                    if (GUILayout.Button(
                            "Delete Confirmed Painted Accent Orphans"))
                    {
                        GroundPaintedAccentGeneratedAssetCleanupWindow
                            .OpenAndPrepareDeletion();
                    }
                }
            }

            if (targets.Length == 1 && target is GeneratedGround ground)
            {
                EditorGUILayout.Space(4f);
                if (GUILayout.Button("Run Surface Material Validation"))
                {
                    ProgrammaticStylized3D.Rendering.PixelSurface.Editor
                        .StylizedSurfaceMaterialValidation.RunAndCopy(ground);
                }

                if (GUILayout.Button("Copy All Ground Diagnostics"))
                {
                    EditorGUIUtility.systemCopyBuffer =
                        BuildAllGroundDiagnosticsClipboardReport(ground);
                }
            }

            EditorGUI.indentLevel--;
        }

        private void DrawSurfaceDiagnosticsControls()
        {
            if (targets.Length != 1 ||
                !DrawSubsectionFoldout(
                    ref showSurfaceDiagnostics,
                    "Last Surface Mask Diagnostics"))
            {
                return;
            }

            GeneratedGround ground = target as GeneratedGround;
            if (ground == null)
            {
                return;
            }

            EditorGUI.indentLevel++;
            EditorGUILayout.HelpBox(
                ground.LastSurfaceMaskDiagnostics,
                MessageType.None);
            if (GUILayout.Button("Copy Surface Mask Diagnostics"))
            {
                EditorGUIUtility.systemCopyBuffer =
                    BuildSurfaceMaskDiagnosticsClipboardReport(ground);
            }
            EditorGUI.indentLevel--;
        }

        private void OnSceneGUI()
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            GeneratedGround ground = target as GeneratedGround;
            if (ground == null)
            {
                return;
            }

            bool showDistribution =
                ground.ShowPaintedAccentDistributionOverlay;
            bool showProposals =
                ground.ShowPaintedAccentWeightedProposals;
            bool showAccepted =
                ground.ShowPaintedAccentLastAcceptedPositions;
            bool showComposition =
                ground.ShowPaintedAccentCompositionDebug;
            bool showProjectedGlyphs =
                ground.ShowPaintedAccentProjectedGlyphDebug;
            PaintedAccentPlacementOverlayWeightMode overlayWeightMode =
                ground.PaintedAccentPlacementOverlayWeight;

            if (!showDistribution &&
                !showProposals &&
                !showAccepted &&
                !showComposition &&
                !showProjectedGlyphs)
            {
                return;
            }

            if (showDistribution || showProposals)
            {
                int signature =
                    ground.CalculatePaintedAccentPlacementDebugSignature();

                if (signature != paintedAccentPlacementDebugSignature)
                {
                    paintedAccentPlacementDebugSignature = signature;
                    bool built =
                        ground.TryBuildPaintedAccentPlacementDebugSnapshot(
                            out paintedAccentPlacementDebugSnapshot);
                    paintedAccentPlacementDebugSnapshotBuildFailed = !built;

                    if (!built)
                    {
                        paintedAccentPlacementDebugSnapshot =
                            GroundPaintedAccentPlacementDebugSnapshot.Empty;
                    }

                    Repaint();
                }
            }

            if (showProjectedGlyphs)
            {
                int projectedSignature =
                    ground.CalculatePaintedAccentProjectedGlyphDebugSignature();

                if (projectedSignature !=
                        paintedAccentProjectedGlyphDebugSignature ||
                    paintedAccentProjectedGlyphDebugSnapshotBuildFailed)
                {
                    paintedAccentProjectedGlyphDebugSignature =
                        projectedSignature;
                    bool built =
                        ground.TryBuildPaintedAccentProjectedGlyphDebugSnapshot(
                            out paintedAccentProjectedGlyphDebugSnapshot);
                    paintedAccentProjectedGlyphDebugSnapshotBuildFailed = !built;

                    if (!built)
                    {
                        paintedAccentProjectedGlyphDebugSnapshot =
                            GroundPaintedAccentProjectedGlyphDebugSnapshot.Empty;
                    }

                    Repaint();
                }
            }

            Vector3[] acceptedLocalPositions =
                showAccepted
                    ? ground.GetLastPaintedAccentAcceptedLocalPositions()
                    : System.Array.Empty<Vector3>();
            GroundPaintedAccentCompositionDebugSnapshot compositionSnapshot =
                showComposition
                    ? ground.GetLastPaintedAccentCompositionDebugSnapshot()
                    : GroundPaintedAccentCompositionDebugSnapshot.Empty;

            Color previousColor = Handles.color;
            CompareFunction previousZTest = Handles.zTest;
            Handles.zTest = CompareFunction.Always;

            if (showDistribution)
            {
                DrawPaintedAccentDistributionOverlay(
                    ground,
                    paintedAccentPlacementDebugSnapshot,
                    overlayWeightMode);
            }

            if (showProposals)
            {
                DrawPaintedAccentProposalOverlay(
                    ground,
                    paintedAccentPlacementDebugSnapshot.ProposedPoints);
            }

            if (showAccepted)
            {
                DrawPaintedAccentAcceptedOverlay(
                    ground,
                    acceptedLocalPositions);
            }

            if (showComposition)
            {
                DrawPaintedAccentCompositionOverlay(
                    ground,
                    compositionSnapshot);
            }

            if (showProjectedGlyphs)
            {
                DrawPaintedAccentProjectedGlyphOverlay(
                    ground,
                    paintedAccentProjectedGlyphDebugSnapshot);
            }

            Handles.color = previousColor;
            Handles.zTest = previousZTest;

            DrawPaintedAccentPlacementLegend(
                showDistribution,
                showProposals,
                showAccepted,
                showComposition,
                showProjectedGlyphs,
                overlayWeightMode,
                paintedAccentPlacementDebugSnapshot,
                acceptedLocalPositions,
                paintedAccentPlacementDebugSnapshotBuildFailed,
                paintedAccentProjectedGlyphDebugSnapshotBuildFailed,
                paintedAccentProjectedGlyphDebugSnapshot,
                compositionSnapshot);
        }

        private static void DrawPaintedAccentDistributionOverlay(
            GeneratedGround ground,
            GroundPaintedAccentPlacementDebugSnapshot snapshot,
            PaintedAccentPlacementOverlayWeightMode overlayWeightMode)
        {
            if (!snapshot.IsValid)
            {
                return;
            }

            GroundPaintedAccentDistributionDebugSample[] samples =
                snapshot.DistributionSamples;
            int resolution = snapshot.DistributionSampleResolution;
            Transform groundTransform = ground.transform;

            for (int z = 0; z < resolution - 1; z++)
            {
                for (int x = 0; x < resolution - 1; x++)
                {
                    int i00 = z * resolution + x;
                    int i10 = i00 + 1;
                    int i01 = (z + 1) * resolution + x;
                    int i11 = i01 + 1;

                    GroundPaintedAccentDistributionDebugSample s00 =
                        samples[i00];
                    GroundPaintedAccentDistributionDebugSample s10 =
                        samples[i10];
                    GroundPaintedAccentDistributionDebugSample s11 =
                        samples[i11];
                    GroundPaintedAccentDistributionDebugSample s01 =
                        samples[i01];

                    if (!s00.IsValid ||
                        !s10.IsValid ||
                        !s11.IsValid ||
                        !s01.IsValid)
                    {
                        continue;
                    }

                    float weight =
                        (ResolvePaintedAccentDebugSampleWeight(
                             s00,
                             overlayWeightMode) +
                         ResolvePaintedAccentDebugSampleWeight(
                             s10,
                             overlayWeightMode) +
                         ResolvePaintedAccentDebugSampleWeight(
                             s11,
                             overlayWeightMode) +
                         ResolvePaintedAccentDebugSampleWeight(
                             s01,
                             overlayWeightMode)) * 0.25f;
                    Handles.color =
                        ResolvePaintedAccentDistributionHeatmapColor(weight);

                    Handles.DrawAAConvexPolygon(
                        groundTransform.TransformPoint(s00.LocalPosition),
                        groundTransform.TransformPoint(s10.LocalPosition),
                        groundTransform.TransformPoint(s11.LocalPosition),
                        groundTransform.TransformPoint(s01.LocalPosition));
                }
            }
        }

        private static Color ResolvePaintedAccentDistributionHeatmapColor(
            float weight)
        {
            Color sparseColor =
                new Color(0.05f, 0.22f, 1.00f, 0.22f);
            Color middleColor =
                new Color(0.05f, 0.90f, 0.82f, 0.30f);
            Color denseColor =
                new Color(1.00f, 0.18f, 0.03f, 0.52f);
            float clampedWeight = Mathf.Clamp01(weight);

            return clampedWeight < 0.5f
                ? Color.Lerp(
                    sparseColor,
                    middleColor,
                    clampedWeight * 2f)
                : Color.Lerp(
                    middleColor,
                    denseColor,
                    (clampedWeight - 0.5f) * 2f);
        }

        private static void DrawPaintedAccentProposalOverlay(
            GeneratedGround ground,
            GroundPaintedAccentProposalDebugPoint[] points)
        {
            if (points == null)
            {
                return;
            }

            Transform groundTransform = ground.transform;

            for (int index = 0; index < points.Length; index++)
            {
                GroundPaintedAccentProposalDebugPoint point = points[index];
                Vector3 worldPosition =
                    groundTransform.TransformPoint(point.LocalPosition);
                float size =
                    HandleUtility.GetHandleSize(worldPosition) * 0.075f;
                Vector3 right = groundTransform.right * size;
                Vector3 forward = groundTransform.forward * size;

                Handles.color = new Color(0f, 0f, 0f, 0.90f);
                Handles.DrawAAPolyLine(
                    5f,
                    worldPosition - right,
                    worldPosition + right);
                Handles.DrawAAPolyLine(
                    5f,
                    worldPosition - forward,
                    worldPosition + forward);

                Handles.color =
                    Color.Lerp(
                        new Color(0.05f, 0.90f, 1.00f, 1.00f),
                        new Color(1.00f, 0.92f, 0.05f, 1.00f),
                        point.EffectiveProposalWeight);
                Handles.DrawAAPolyLine(
                    2.5f,
                    worldPosition - right,
                    worldPosition + right);
                Handles.DrawAAPolyLine(
                    2.5f,
                    worldPosition - forward,
                    worldPosition + forward);
            }
        }

        private static void DrawPaintedAccentAcceptedOverlay(
            GeneratedGround ground,
            Vector3[] localPositions)
        {
            if (localPositions == null)
            {
                return;
            }

            Transform groundTransform = ground.transform;
            Vector3 normal = groundTransform.up;

            for (int index = 0; index < localPositions.Length; index++)
            {
                Vector3 worldPosition =
                    groundTransform.TransformPoint(localPositions[index]);
                float size =
                    HandleUtility.GetHandleSize(worldPosition) * 0.090f;

                DrawPaintedAccentAcceptedRing(
                    worldPosition,
                    normal,
                    size * 0.62f);
            }
        }

        private static void DrawPaintedAccentCompositionOverlay(
            GeneratedGround ground,
            GroundPaintedAccentCompositionDebugSnapshot snapshot)
        {
            if (!snapshot.IsValid)
            {
                return;
            }

            Transform groundTransform = ground.transform;
            Vector3 groundNormal = groundTransform.up;
            GroundPaintedAccentCompositionProposalDebugPoint[] proposals =
                snapshot.Proposals;

            for (int index = 0;
                 proposals != null && index < proposals.Length;
                 index++)
            {
                GroundPaintedAccentCompositionProposalDebugPoint proposal =
                    proposals[index];
                Vector3 worldPosition =
                    groundTransform.TransformPoint(proposal.LocalPosition);
                float size =
                    HandleUtility.GetHandleSize(worldPosition) * 0.030f;
                Vector3 right = groundTransform.right * size;
                Vector3 forward = groundTransform.forward * size;
                Color modeColor =
                    ResolvePaintedAccentCompositionRegionColor(
                        proposal.RegionMode);
                modeColor.a = 0.92f;
                Handles.color = modeColor;
                const float lineWidth = 2.5f;
                Handles.DrawAAPolyLine(
                    lineWidth,
                    worldPosition - right,
                    worldPosition + right);
                Handles.DrawAAPolyLine(
                    lineWidth,
                    worldPosition - forward,
                    worldPosition + forward);
            }

            GroundPaintedAccentCompositionRegionDebug[] regions =
                snapshot.Regions;
            for (int index = 0;
                 regions != null && index < regions.Length;
                 index++)
            {
                GroundPaintedAccentCompositionRegionDebug region =
                    regions[index];
                if (!region.IsOccupied)
                {
                    continue;
                }

                Vector3 worldPosition =
                    groundTransform.TransformPoint(region.LocalPosition);
                Vector3 worldDirection =
                    groundTransform.TransformDirection(
                        new Vector3(
                            region.LocalDirection.x,
                            0f,
                            region.LocalDirection.y));
                if (worldDirection.sqrMagnitude <= 0.000001f)
                {
                    worldDirection = groundTransform.right;
                }
                else
                {
                    worldDirection.Normalize();
                }

                float halfLength =
                    HandleUtility.GetHandleSize(worldPosition) * 0.18f;
                Vector3 start = worldPosition - worldDirection * halfLength;
                Vector3 end = worldPosition + worldDirection * halfLength;
                Handles.color = new Color(0f, 0f, 0f, 0.90f);
                Handles.DrawAAPolyLine(5f, start, end);
                Handles.color =
                    ResolvePaintedAccentCompositionRegionColor(
                        region.RegionMode);
                Handles.DrawAAPolyLine(2.5f, start, end);
                Handles.DrawWireDisc(
                    worldPosition,
                    groundNormal,
                    halfLength * 0.22f);
            }

            GroundPaintedAccentCompositionMarkDebugPoint[] marks =
                snapshot.AcceptedMarks;
            for (int index = 0;
                 marks != null && index < marks.Length;
                 index++)
            {
                GroundPaintedAccentCompositionMarkDebugPoint mark = marks[index];
                Vector3 worldPosition =
                    groundTransform.TransformPoint(mark.LocalPosition);
                float handleSize = HandleUtility.GetHandleSize(worldPosition);
                float radius;

                switch (mark.Role)
                {
                    case GroundPaintedAccentCompositionRole.Dominant:
                        radius = handleSize * 0.065f;
                        break;
                    case GroundPaintedAccentCompositionRole.Support:
                        radius = handleSize * 0.028f;
                        break;
                    case GroundPaintedAccentCompositionRole.Standard:
                    default:
                        radius = handleSize * 0.044f;
                        break;
                }

                Handles.color = new Color(0f, 0f, 0f, 0.95f);
                Handles.DrawWireDisc(
                    worldPosition,
                    groundNormal,
                    radius * 1.28f);
                Handles.color = ResolvePaintedAccentGlyphFamilyColor(mark.Family);
                Handles.DrawWireDisc(worldPosition, groundNormal, radius);
            }
        }

        private static Color ResolvePaintedAccentCompositionRegionColor(
            GroundPaintedAccentCompositionRegionMode mode)
        {
            switch (mode)
            {
                case GroundPaintedAccentCompositionRegionMode.Quiet:
                    return new Color(0.25f, 0.55f, 1.00f, 0.95f);
                case GroundPaintedAccentCompositionRegionMode.Accent:
                    return new Color(1.00f, 0.42f, 0.06f, 0.98f);
                case GroundPaintedAccentCompositionRegionMode.Supporting:
                default:
                    return new Color(0.12f, 1.00f, 0.65f, 0.95f);
            }
        }

        private static void DrawPaintedAccentProjectedGlyphOverlay(
            GeneratedGround ground,
            GroundPaintedAccentProjectedGlyphDebugSnapshot snapshot)
        {
            if (!snapshot.IsValid)
            {
                return;
            }

            Transform groundTransform = ground.transform;
            GroundPaintedAccentProjectedGlyph[] glyphs = snapshot.Glyphs;

            for (int glyphIndex = 0;
                 glyphs != null && glyphIndex < glyphs.Length;
                 glyphIndex++)
            {
                GroundPaintedAccentProjectedGlyph glyph = glyphs[glyphIndex];
                if (!glyph.IsValid ||
                    !ShouldDrawPaintedAccentGlyphFamily(
                        ground.PaintedAccentGlyphFamilyFilter,
                        glyph.Family))
                {
                    continue;
                }

                Vector3[] localPoints = glyph.LocalSurfacePoints;
                float[] halfWidths = glyph.HalfWidths;
                Vector3[] centerWorld = new Vector3[localPoints.Length];
                Vector3[] leftWorld = new Vector3[localPoints.Length];
                Vector3[] rightWorld = new Vector3[localPoints.Length];

                for (int pointIndex = 0;
                     pointIndex < localPoints.Length;
                     pointIndex++)
                {
                    Vector3 localPoint = localPoints[pointIndex];
                    localPoint.y += 0.035f;
                    Vector2 tangent =
                        ResolvePaintedAccentProjectedGlyphTangent(
                            localPoints,
                            pointIndex);
                    Vector2 side = new Vector2(-tangent.y, tangent.x);
                    float halfWidth = Mathf.Max(0f, halfWidths[pointIndex]);
                    Vector3 leftLocal =
                        localPoint +
                        new Vector3(side.x, 0f, side.y) * halfWidth;
                    Vector3 rightLocal =
                        localPoint -
                        new Vector3(side.x, 0f, side.y) * halfWidth;

                    centerWorld[pointIndex] =
                        groundTransform.TransformPoint(localPoint);
                    leftWorld[pointIndex] =
                        groundTransform.TransformPoint(leftLocal);
                    rightWorld[pointIndex] =
                        groundTransform.TransformPoint(rightLocal);
                }

                // Use deliberately high-contrast debug colours. The ground can be
                // turquoise, pale snow, dark mud, or selection-tinted, so a single
                // bright cyan pass is not reliably legible. Draw a black outline
                // beneath the projected centreline and crest marker, then use a
                // saturated red/yellow foreground. Width boundaries remain distinct
                // dark purple so they do not visually merge with the centreline.
                Handles.color = new Color(0f, 0f, 0f, 0.98f);
                Handles.DrawAAPolyLine(6.5f, centerWorld);
                Handles.color = new Color(1.00f, 0.05f, 0.04f, 1.00f);
                Handles.DrawAAPolyLine(3.5f, centerWorld);

                Handles.color = new Color(0f, 0f, 0f, 0.88f);
                Handles.DrawAAPolyLine(3.25f, leftWorld);
                Handles.DrawAAPolyLine(3.25f, rightWorld);
                Handles.color = new Color(0.48f, 0.08f, 0.72f, 0.98f);
                Handles.DrawAAPolyLine(1.55f, leftWorld);
                Handles.DrawAAPolyLine(1.55f, rightWorld);

                int crestIndex = Mathf.Clamp(
                    Mathf.RoundToInt(glyph.CrestT * (centerWorld.Length - 1)),
                    0,
                    centerWorld.Length - 1);
                Vector3 crestWorld = centerWorld[crestIndex];
                float crestRadius =
                    HandleUtility.GetHandleSize(crestWorld) * 0.050f;
                Handles.color = new Color(0f, 0f, 0f, 0.98f);
                Handles.DrawWireDisc(
                    crestWorld,
                    groundTransform.up,
                    crestRadius * 1.45f);
                Handles.color = new Color(1.00f, 0.92f, 0.05f, 1.00f);
                Handles.DrawWireDisc(
                    crestWorld,
                    groundTransform.up,
                    crestRadius);
            }

            GroundPaintedAccentProjectedGlyphRejectionDebugPoint[] rejections =
                snapshot.Rejections;

            for (int rejectionIndex = 0;
                 rejections != null && rejectionIndex < rejections.Length;
                 rejectionIndex++)
            {
                GroundPaintedAccentProjectedGlyphRejectionDebugPoint rejection =
                    rejections[rejectionIndex];
                if (!ShouldDrawPaintedAccentGlyphFamily(
                        ground.PaintedAccentGlyphFamilyFilter,
                        rejection.Family))
                {
                    continue;
                }

                Vector3 worldPosition =
                    groundTransform.TransformPoint(rejection.LocalPosition);
                float size =
                    HandleUtility.GetHandleSize(worldPosition) * 0.065f;
                Vector3 right = groundTransform.right * size;
                Vector3 forward = groundTransform.forward * size;

                Handles.color = new Color(0f, 0f, 0f, 0.92f);
                Handles.DrawAAPolyLine(5f,
                    worldPosition - right - forward,
                    worldPosition + right + forward);
                Handles.DrawAAPolyLine(5f,
                    worldPosition - right + forward,
                    worldPosition + right - forward);

                Handles.color =
                    ResolvePaintedAccentProjectedGlyphRejectionColor(
                        rejection.Reason);
                Handles.DrawAAPolyLine(2.5f,
                    worldPosition - right - forward,
                    worldPosition + right + forward);
                Handles.DrawAAPolyLine(2.5f,
                    worldPosition - right + forward,
                    worldPosition + right - forward);
            }
        }

        private static bool ShouldDrawPaintedAccentGlyphFamily(
            PaintedAccentGlyphFamilyPreview preview,
            GroundPaintedAccentGlyphFamily family)
        {
            switch (preview)
            {
                case PaintedAccentGlyphFamilyPreview.CompleteMound:
                    return family == GroundPaintedAccentGlyphFamily.CompleteMound;
                case PaintedAccentGlyphFamilyPreview.AsymmetricMound:
                    return family == GroundPaintedAccentGlyphFamily.AsymmetricMound;
                case PaintedAccentGlyphFamilyPreview.SingleShoulder:
                    return family == GroundPaintedAccentGlyphFamily.SingleShoulder;
                case PaintedAccentGlyphFamilyPreview.ShallowCrest:
                    return family == GroundPaintedAccentGlyphFamily.ShallowCrest;
                case PaintedAccentGlyphFamilyPreview.All:
                default:
                    return true;
            }
        }

        private static Color ResolvePaintedAccentGlyphFamilyColor(
            GroundPaintedAccentGlyphFamily family)
        {
            switch (family)
            {
                case GroundPaintedAccentGlyphFamily.AsymmetricMound:
                    return new Color(0.20f, 0.85f, 1.00f, 1.00f);
                case GroundPaintedAccentGlyphFamily.SingleShoulder:
                    return new Color(1.00f, 0.55f, 0.12f, 1.00f);
                case GroundPaintedAccentGlyphFamily.ShallowCrest:
                    return new Color(0.38f, 1.00f, 0.42f, 1.00f);
                case GroundPaintedAccentGlyphFamily.CompleteMound:
                default:
                    return new Color(1.00f, 0.25f, 0.75f, 1.00f);
            }
        }

        private static Vector2 ResolvePaintedAccentProjectedGlyphTangent(
            Vector3[] points,
            int pointIndex)
        {
            int beforeIndex = Mathf.Max(0, pointIndex - 1);
            int afterIndex = Mathf.Min(points.Length - 1, pointIndex + 1);
            Vector2 tangent =
                new Vector2(
                    points[afterIndex].x - points[beforeIndex].x,
                    points[afterIndex].z - points[beforeIndex].z);

            return tangent.sqrMagnitude > 0.000001f
                ? tangent.normalized
                : Vector2.right;
        }

        private static Color ResolvePaintedAccentProjectedGlyphRejectionColor(
            GroundPaintedAccentProjectedGlyphRejectionReason reason)
        {
            switch (reason)
            {
                case GroundPaintedAccentProjectedGlyphRejectionReason.River:
                    return new Color(0.10f, 0.45f, 1.00f, 1.00f);
                case GroundPaintedAccentProjectedGlyphRejectionReason.ModifierExclusion:
                    return new Color(1.00f, 0.90f, 0.08f, 1.00f);
                case GroundPaintedAccentProjectedGlyphRejectionReason.BroadSlope:
                    return new Color(1.00f, 0.10f, 0.08f, 1.00f);
                case GroundPaintedAccentProjectedGlyphRejectionReason.LocalGrade:
                    return new Color(0.75f, 0.18f, 1.00f, 1.00f);
                case GroundPaintedAccentProjectedGlyphRejectionReason.FamilyShape:
                    return new Color(1.00f, 0.10f, 0.75f, 1.00f);
                // Keep the editor compatible with a runtime assembly that may still expose
                // rejection value 7 without the newer symbolic enum member during Unity's
                // incremental compile pass. The projected generator owns the stable value.
                case (GroundPaintedAccentProjectedGlyphRejectionReason)7:
                    return new Color(1.00f, 0.28f, 0.28f, 1.00f);
                case GroundPaintedAccentProjectedGlyphRejectionReason.Sampling:
                default:
                    return new Color(1.00f, 0.45f, 0.05f, 1.00f);
            }
        }

        private static float ResolvePaintedAccentDebugSampleWeight(
            GroundPaintedAccentDistributionDebugSample sample,
            PaintedAccentPlacementOverlayWeightMode overlayWeightMode)
        {
            return
                overlayWeightMode ==
                    PaintedAccentPlacementOverlayWeightMode.EffectiveProposalWeight
                    ? sample.EffectiveProposalWeight
                    : sample.PatchWeight;
        }

        private static void ResolvePaintedAccentDebugWeightStatistics(
            GroundPaintedAccentDistributionDebugSample[] samples,
            PaintedAccentPlacementOverlayWeightMode overlayWeightMode,
            out float minimum,
            out float mean,
            out float maximum)
        {
            minimum = 0f;
            mean = 0f;
            maximum = 0f;

            if (samples == null || samples.Length == 0)
            {
                return;
            }

            float minimumValue = float.PositiveInfinity;
            float maximumValue = float.NegativeInfinity;
            double total = 0.0;
            int count = 0;

            for (int index = 0; index < samples.Length; index++)
            {
                GroundPaintedAccentDistributionDebugSample sample =
                    samples[index];

                if (!sample.IsValid)
                {
                    continue;
                }

                float value =
                    ResolvePaintedAccentDebugSampleWeight(
                        sample,
                        overlayWeightMode);
                minimumValue = Mathf.Min(minimumValue, value);
                maximumValue = Mathf.Max(maximumValue, value);
                total += value;
                count++;
            }

            if (count <= 0)
            {
                return;
            }

            minimum = minimumValue;
            mean = (float)(total / count);
            maximum = maximumValue;
        }

        private static void DrawPaintedAccentAcceptedRing(
            Vector3 worldPosition,
            Vector3 normal,
            float radius)
        {
            Vector3 normalizedNormal =
                normal.sqrMagnitude > 0.0001f
                    ? normal.normalized
                    : Vector3.up;

            Handles.color = new Color(0f, 0f, 0f, 0.95f);
            Handles.DrawWireDisc(
                worldPosition,
                normalizedNormal,
                radius * 1.10f);
            Handles.color = new Color(0.16f, 1.00f, 0.24f, 0.98f);
            Handles.DrawWireDisc(
                worldPosition,
                normalizedNormal,
                radius);
        }

        private static void DrawPaintedAccentPlacementLegend(
            bool showDistribution,
            bool showProposals,
            bool showAccepted,
            bool showComposition,
            bool showProjectedGlyphs,
            PaintedAccentPlacementOverlayWeightMode overlayWeightMode,
            GroundPaintedAccentPlacementDebugSnapshot snapshot,
            Vector3[] acceptedPositions,
            bool snapshotBuildFailed,
            bool projectedSnapshotBuildFailed,
            GroundPaintedAccentProjectedGlyphDebugSnapshot projectedSnapshot,
            GroundPaintedAccentCompositionDebugSnapshot compositionSnapshot)
        {
            int validSampleCount = 0;
            GroundPaintedAccentDistributionDebugSample[] samples =
                snapshot.DistributionSamples;

            if (samples != null)
            {
                for (int index = 0; index < samples.Length; index++)
                {
                    if (samples[index].IsValid)
                    {
                        validSampleCount++;
                    }
                }
            }

            ResolvePaintedAccentDebugWeightStatistics(
                samples,
                overlayWeightMode,
                out float minimumWeight,
                out float meanWeight,
                out float maximumWeight);

            int proposedCount =
                snapshot.ProposedPoints != null
                    ? snapshot.ProposedPoints.Length
                    : 0;
            int acceptedCount =
                acceptedPositions != null
                    ? acceptedPositions.Length
                    : 0;

            System.Text.StringBuilder text =
                new System.Text.StringBuilder(320);
            text.AppendLine("Painted Accent Placement");

            if (showDistribution)
            {
                text.Append("Blue → red: ");
                text.AppendLine(
                    overlayWeightMode ==
                        PaintedAccentPlacementOverlayWeightMode.EffectiveProposalWeight
                        ? "effective proposal weight"
                        : "patch preference");
                text.Append("Weight min/mean/max: ");
                text.Append(minimumWeight.ToString("F3"));
                text.Append(" / ");
                text.Append(meanWeight.ToString("F3"));
                text.Append(" / ");
                text.AppendLine(maximumWeight.ToString("F3"));
            }

            if (showProposals)
            {
                text.AppendLine("Cyan/yellow cross: weighted proposal");
            }

            if (showAccepted)
            {
                text.AppendLine("Green ring: accepted base stroke");
            }

            if (showComposition)
            {
                text.AppendLine(
                    "Blue/green/orange crosses: quiet/supporting/accent proposals");
                text.AppendLine(
                    "Region bars: occupied-region direction");
                text.AppendLine(
                    "Ring size: dominant/standard/support; ring colour: glyph family");
                int regionCount =
                    compositionSnapshot.Regions != null
                        ? compositionSnapshot.Regions.Length
                        : 0;
                int markCount =
                    compositionSnapshot.AcceptedMarks != null
                        ? compositionSnapshot.AcceptedMarks.Length
                        : 0;
                text.Append("Composition occupied regions/marks: ");
                text.Append(regionCount);
                text.Append(" / ");
                text.AppendLine(markCount.ToString());
            }

            if (showProjectedGlyphs)
            {
                text.AppendLine("Red/purple: accepted projected glyphs");
                text.AppendLine("Yellow ring: projected peak; Family Preview filters debug only");
                GroundPaintedAccentProjectedGlyphDiagnostics diagnostics =
                    projectedSnapshot.Diagnostics;
                text.Append("Projected accepted/rejected: ");
                text.Append(diagnostics.ProjectedGlyphsAccepted);
                text.Append(" / ");
                text.AppendLine(
                    diagnostics.ProjectedGlyphsRejectedTotal.ToString());
            }

            if (snapshotBuildFailed && (showDistribution || showProposals))
            {
                text.AppendLine("PLACEMENT SNAPSHOT UNAVAILABLE");
            }

            if (projectedSnapshotBuildFailed && showProjectedGlyphs)
            {
                text.AppendLine("PROJECTED GLYPH SNAPSHOT UNAVAILABLE");
            }

            text.Append("Samples: ");
            text.Append(validSampleCount);
            text.Append('/');
            text.Append(samples != null ? samples.Length : 0);
            text.Append("   Proposals: ");
            text.Append(proposedCount);
            text.Append("   Accepted: ");
            text.Append(acceptedCount);

            Handles.BeginGUI();
            float boxHeight = showProjectedGlyphs ? 210f : 124f;
            Rect boxRect = new Rect(12f, 12f, 430f, boxHeight);
            GUI.Box(boxRect, GUIContent.none, EditorStyles.helpBox);
            GUI.Label(
                new Rect(
                    boxRect.x + 9f,
                    boxRect.y + 7f,
                    boxRect.width - 18f,
                    boxRect.height - 14f),
                text.ToString(),
                EditorStyles.wordWrappedMiniLabel);
            Handles.EndGUI();
        }

        private static GroundSurfaceStyleProfile[] LoadAvailableStyleProfiles()
        {
            string[] searchFolders = { "Assets/Game/Demo/Profiles/Ground/Styles" };
            string[] guids = AssetDatabase.FindAssets(
                "t:GroundSurfaceStyleProfile",
                searchFolders);

            if (guids == null || guids.Length == 0)
            {
                guids = AssetDatabase.FindAssets(
                    "t:GroundSurfaceStyleProfile");
            }

            if (guids == null || guids.Length == 0)
            {
                return new GroundSurfaceStyleProfile[0];
            }

            System.Collections.Generic.List<GroundSurfaceStyleProfile> styles =
                new System.Collections.Generic.List<GroundSurfaceStyleProfile>();

            for (int index = 0; index < guids.Length; index++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[index]);
                GroundSurfaceStyleProfile style =
                    AssetDatabase.LoadAssetAtPath<GroundSurfaceStyleProfile>(
                        path);

                if (style == null || styles.Contains(style))
                {
                    continue;
                }

                styles.Add(style);
            }

            styles.Sort((left, right) =>
                string.Compare(
                    left != null ? left.DisplayName : string.Empty,
                    right != null ? right.DisplayName : string.Empty,
                    System.StringComparison.OrdinalIgnoreCase));

            return styles.ToArray();
        }

        private static GroundSurfaceStyleProfile[] AppendStyle(
            GroundSurfaceStyleProfile[] styles,
            GroundSurfaceStyleProfile style)
        {
            GroundSurfaceStyleProfile[] expanded =
                new GroundSurfaceStyleProfile[styles.Length + 1];

            for (int index = 0; index < styles.Length; index++)
            {
                expanded[index] = styles[index];
            }

            expanded[styles.Length] = style;
            return expanded;
        }

        private static string FindDuplicateVariantId(
            GroundSurfaceStyleProfile style)
        {
            if (style == null || style.Variants == null)
            {
                return null;
            }

            System.Collections.Generic.HashSet<string> seen =
                new System.Collections.Generic.HashSet<string>();

            for (int index = 0; index < style.Variants.Count; index++)
            {
                GroundSurfaceVariantRecipe variant = style.Variants[index];

                if (variant == null || !variant.HasValidId)
                {
                    continue;
                }

                if (!seen.Add(variant.Id))
                {
                    return variant.Id;
                }
            }

            return null;
        }

        private static void RefreshLoadedGroundsUsingSurfaceProfile(
            GroundSurfaceProfile profile)
        {
            if (profile == null)
            {
                return;
            }

            GeneratedGround[] grounds =
                Object.FindObjectsByType<GeneratedGround>(
                    FindObjectsInactive.Include);

            for (int index = 0; index < grounds.Length; index++)
            {
                GeneratedGround ground = grounds[index];

                if (!IsLoadedSceneGround(ground) ||
                    ground.SurfaceProfile != profile)
                {
                    continue;
                }

                ground.RefreshSurfaceStyleState();
            }

            SceneView.RepaintAll();
        }

        private static void RefreshLoadedGroundsUsingStyleVariant(
            GroundSurfaceStyleProfile style,
            string variantId,
            bool includeLocalMaterialOverrides)
        {
            if (style == null || string.IsNullOrWhiteSpace(variantId))
            {
                return;
            }

            GeneratedGround[] grounds =
                Object.FindObjectsByType<GeneratedGround>(
                    FindObjectsInactive.Include);

            for (int index = 0; index < grounds.Length; index++)
            {
                GeneratedGround ground = grounds[index];

                if (!IsLoadedSceneGround(ground) ||
                    ground.SurfaceStyleProfile != style ||
                    ground.SurfaceVariantId != variantId ||
                    (!includeLocalMaterialOverrides &&
                     ground.OverrideMaterialControls))
                {
                    continue;
                }

                ground.RefreshSurfaceMaterialProperties();
            }

            SceneView.RepaintAll();
        }

        private static void RefreshLoadedGroundsUsingSurfaceMaterial(
            StylizedSurfaceMaterialProfile profile)
        {
            if (profile == null)
            {
                return;
            }

            List<GroundSurfaceLayerProfile> layers =
                GetSurfaceLayerProfiles();
            for (int index = 0; index < layers.Count; index++)
            {
                GroundSurfaceLayerProfile layer = layers[index];
                if (layer != null && layer.SurfaceMaterial == profile)
                {
                    RefreshLoadedGroundsUsingSurfaceLayer(layer);
                }
            }
        }

        private static void RefreshLoadedGroundsUsingSurfaceLayer(
            GroundSurfaceLayerProfile profile)
        {
            if (profile == null)
            {
                return;
            }

            GeneratedGround[] grounds =
                Object.FindObjectsByType<GeneratedGround>(
                    FindObjectsInactive.Include);

            for (int index = 0; index < grounds.Length; index++)
            {
                GeneratedGround ground = grounds[index];

                if (!IsLoadedSceneGround(ground) ||
                    (ground.BankSurfaceLayer != profile &&
                     ground.RiverbedSurfaceLayer != profile))
                {
                    continue;
                }

                ground.RefreshSurfaceMaterialProperties();
            }

            SceneView.RepaintAll();
        }

        private static void RefreshLoadedGroundsUsingHydrologyModifier(
            GroundHydrologyModifierProfile profile)
        {
            if (profile == null)
            {
                return;
            }

            GeneratedGround[] grounds =
                Object.FindObjectsByType<GeneratedGround>(
                    FindObjectsInactive.Include);

            for (int index = 0; index < grounds.Length; index++)
            {
                GeneratedGround ground = grounds[index];
                if (!IsLoadedSceneGround(ground) ||
                    (ground.ShoreHydrologyModifier != profile &&
                     ground.RiverbedHydrologyModifier != profile))
                {
                    continue;
                }

                ground.RefreshSurfaceMaterialProperties();
            }

            SceneView.RepaintAll();
        }

        private static bool IsLoadedSceneGround(GeneratedGround ground)
        {
            return ground != null &&
                !EditorUtility.IsPersistent(ground) &&
                ground.gameObject.scene.IsValid() &&
                ground.gameObject.scene.isLoaded;
        }

        private void ApplyToTargets(
            string undoName,
            GroundAction action)
        {
            for (int i = 0; i < targets.Length; i++)
            {
                GeneratedGround ground =
                    targets[i] as GeneratedGround;

                if (ground == null)
                {
                    continue;
                }

                Undo.RecordObject(
                    ground,
                    undoName);

                action(ground);

                EditorUtility.SetDirty(ground);
            }

            serializedObject.Update();
            Repaint();
            SceneView.RepaintAll();
        }

        private delegate void GroundAction(
            GeneratedGround ground);
    }

}
