using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

namespace ProgrammaticStylized3D.Rivers.Editor
{
    [CustomEditor(typeof(StylizedRiver))]
    [CanEditMultipleObjects]
    internal sealed partial class StylizedRiverEditor : UnityEditor.Editor
    {
        private enum InspectorSection
        {
            Setup,
            RiverDomain,
            ChannelShape,
            ShorelineSafety,
            NaturalVariation,
            SurfaceMesh,
            WaterBodyAndLighting,
            SurfaceMotion,
            Refraction,
            RuntimeDisturbances,
            Foam,
            DebugViews,
            RuntimeDiagnostics,
            GeneratedStatus,
            Actions,

            WaterSurfaceState,
            WaterLiquidBody,
            WaterFrozenBody,
            WaterLightingResponse,
            WaterAdvancedMaterial,
            MotionGeneralFlow,
            MotionMacroWaves,
            MotionDetail,
            MotionCurrentAccents,
            MotionShoreMotion,
            MotionShoreWaveProfile,
            RefractionLiquid,
            RefractionShoreDepth,
            RefractionFrozen,
            DisturbanceMasterPreset,
            DisturbancePressure,
            DisturbanceWake,
            DisturbanceRipples,
            FoamLayerA,
            FoamLayerAMajorSupport,
            FoamLayerAConnectors,
            FoamLayerANegativeTopology,
            FoamLayerB,
            FoamLayerC,
            FoamLayerCLifecycle,
            FoamLayerCAutomaticBirth,
            FoamLayerD,
            FoamLayerE,
            FoamRuntimeQuality,
            FoamManualSourceMotion,
            FoamBirthShore,
            FoamBirthShoreRibbonPattern,
            FoamBirthInwardWashPattern,
            FoamBirthObject,
            FoamBirthObjectContactArcPattern,
            FoamBirthObjectContactSemiArcPattern,
            FoamBirthObjectContactFleckPattern,
            FoamBirthFreeWater,
            FoamBirthFreeWaterLacePattern,
            FoamBirthFreeWaterCrossLacePattern,
            FoamBirthFreeWaterFragmentPattern,

            DiagnosticsDomainGeometry,
            DiagnosticsDisturbances,
            DiagnosticsDisturbanceSummary,
            DiagnosticsDisturbanceDispatches,
            DiagnosticsDisturbanceSources,
            DiagnosticsDisturbanceMemory,
            DiagnosticsFoam,
            DiagnosticsFoamSummary,
            DiagnosticsFoamLayerA,
            DiagnosticsFoamLayerB,
            DiagnosticsFoamLayerC,
            DiagnosticsFoamLayerCTransport,
            DiagnosticsFoamLayerCLifecycle,
            DiagnosticsFoamLayerCBirth,
            DiagnosticsFoamLayerCProbe,
            DiagnosticsFoamLayerD,
            DiagnosticsFoamResources,
            DiagnosticsFoamAdvanced,

            ActionsGeneration,
            ActionsDomainValidation,
            ActionsDisturbanceTests,
            ActionsFoamLayerACache,
            ActionsFoamHistoricalDiagnostics,
            ActionsFoamLayerCTests,
            ActionsFoamLifecycleProbes,
            ActionsRuntimeClearReset
        }

        private readonly HashSet<InspectorSection> openInspectorSections =
            new HashSet<InspectorSection>();
        private bool structuralAuthoringChanged;
        private StylizedRiverFoamMajorCandidate majorCandidatePreview;
        private Texture2D majorCandidatePreviewTexture;
        private Color32[] majorCandidatePreviewPixels;
        private int majorCandidatePreviewSeed = int.MinValue;
        private StylizedRiverFoamMajorCandidatePreviewStage
            majorCandidatePreviewStage =
                StylizedRiverFoamMajorCandidatePreviewStage.FinalSupport;

        public override bool RequiresConstantRepaint()
        {
            if (!Application.isPlaying || targets.Length != 1 ||
                target is not StylizedRiver river ||
                !IsSectionOpen(InspectorSection.RuntimeDiagnostics))
            {
                return false;
            }

            if (HasVisibleLiveDisturbanceDiagnostics(river))
            {
                return true;
            }

            return HasVisibleLiveFoamDiagnostics(river);
        }

        private bool HasVisibleLiveDisturbanceDiagnostics(
            StylizedRiver river)
        {
            if (!IsSectionOpen(InspectorSection.DiagnosticsDisturbances))
            {
                return false;
            }

            bool hasVisibleLivePanel =
                IsSectionOpen(InspectorSection.DiagnosticsDisturbanceSummary) ||
                IsSectionOpen(InspectorSection.DiagnosticsDisturbanceDispatches) ||
                IsSectionOpen(InspectorSection.DiagnosticsDisturbanceSources) ||
                IsSectionOpen(InspectorSection.DiagnosticsDisturbanceMemory);
            if (!hasVisibleLivePanel)
            {
                return false;
            }

            StylizedRiverDisturbanceRuntime runtime =
                river.GetComponent<StylizedRiverDisturbanceRuntime>();
            return runtime != null && runtime.isActiveAndEnabled;
        }

        private bool HasVisibleLiveFoamDiagnostics(StylizedRiver river)
        {
            if (!river.FoamEnabled ||
                !IsSectionOpen(InspectorSection.DiagnosticsFoam))
            {
                return false;
            }

            bool hasVisibleLayerCPanel =
                IsSectionOpen(InspectorSection.DiagnosticsFoamLayerC) &&
                (IsSectionOpen(InspectorSection.DiagnosticsFoamLayerCTransport) ||
                 IsSectionOpen(InspectorSection.DiagnosticsFoamLayerCLifecycle) ||
                 IsSectionOpen(InspectorSection.DiagnosticsFoamLayerCBirth) ||
                 IsSectionOpen(InspectorSection.DiagnosticsFoamLayerCProbe));
            bool hasVisibleLivePanel =
                IsSectionOpen(InspectorSection.DiagnosticsFoamSummary) ||
                IsSectionOpen(InspectorSection.DiagnosticsFoamLayerA) ||
                IsSectionOpen(InspectorSection.DiagnosticsFoamLayerB) ||
                hasVisibleLayerCPanel ||
                IsSectionOpen(InspectorSection.DiagnosticsFoamLayerD) ||
                IsSectionOpen(InspectorSection.DiagnosticsFoamResources) ||
                IsSectionOpen(InspectorSection.DiagnosticsFoamAdvanced);
            if (!hasVisibleLivePanel)
            {
                return false;
            }

            StylizedRiverFoamRuntime runtime =
                river.GetComponent<StylizedRiverFoamRuntime>();
            return runtime != null &&
                runtime.ShouldRepaintInspectorForFoamDebug;
        }

        private void OnDisable()
        {
            if (majorCandidatePreviewTexture != null)
            {
                DestroyImmediate(majorCandidatePreviewTexture);
                majorCandidatePreviewTexture = null;
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            structuralAuthoringChanged = false;

            DrawTopLevelSection(
                InspectorSection.Setup,
                "Setup",
                DrawSetup,
                true);
            DrawTopLevelSection(
                InspectorSection.RiverDomain,
                "River Domain",
                DrawRiverDomain,
                true);
            DrawTopLevelSection(
                InspectorSection.ChannelShape,
                "Channel Shape",
                DrawChannel,
                true);
            DrawTopLevelSection(
                InspectorSection.ShorelineSafety,
                "Shoreline Safety",
                DrawAdvancedShoreline,
                true);
            DrawTopLevelSection(
                InspectorSection.NaturalVariation,
                "Natural Variation",
                DrawNaturalVariation,
                true);
            DrawTopLevelSection(
                InspectorSection.SurfaceMesh,
                "Surface Mesh",
                DrawSurfaceMesh,
                true);
            DrawTopLevelSection(
                InspectorSection.WaterBodyAndLighting,
                "Water Body & Lighting",
                DrawWaterBodyAndLighting);
            DrawTopLevelSection(
                InspectorSection.SurfaceMotion,
                "Surface Motion",
                DrawSurfaceMotion);
            DrawTopLevelSection(
                InspectorSection.Refraction,
                "Refraction",
                DrawRefraction);
            DrawTopLevelSection(
                InspectorSection.RuntimeDisturbances,
                "Runtime Disturbances",
                DrawRuntimeDisturbances);
            DrawTopLevelSection(
                InspectorSection.Foam,
                "Foam",
                DrawFoam);
            DrawTopLevelSection(
                InspectorSection.DebugViews,
                "Debug Views",
                DrawDebugViews);
            DrawTopLevelSection(
                InspectorSection.RuntimeDiagnostics,
                "Runtime Diagnostics",
                DrawRuntimeDiagnostics);
            DrawTopLevelSection(
                InspectorSection.GeneratedStatus,
                "Generated Status",
                DrawGeneratedStatusSection);
            DrawTopLevelSection(
                InspectorSection.Actions,
                "Actions",
                DrawActions);

            bool riverChanged = serializedObject.ApplyModifiedProperties();

            if (structuralAuthoringChanged)
            {
                foreach (Object selectedTarget in targets)
                {
                    if (selectedTarget is StylizedRiver river)
                    {
                        river.RequestStructuralRegenerationFromInspector();
                    }
                }
            }

            if (riverChanged || structuralAuthoringChanged)
            {
                RepaintScene();
            }
        }

        [MenuItem("GameObject/PS3D/Stylized River", false, 10)]
        private static void CreateStylizedRiver(MenuCommand command)
        {
            GameObject riverObject = new GameObject("River_Main");
            GameObjectUtility.SetParentAndAlign(
                riverObject,
                command.context as GameObject);
            Undo.RegisterCreatedObjectUndo(
                riverObject,
                "Create Stylized River");
            Undo.AddComponent<SplineContainer>(riverObject);
            Undo.AddComponent<StylizedRiver>(riverObject);
            Selection.activeGameObject = riverObject;
        }
    }
}
