using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

namespace ProgrammaticStylized3D.Rivers.Editor
{
    internal sealed partial class StylizedRiverEditor
    {
        private enum RiverDebugFeature
        {
            FinalRender,
            WaterBody,
            SurfaceMotion,
            Refraction,
            Disturbances,
            Foam
        }

        private enum DisturbanceDebugCategory
        {
            PrimaryField,
            StaticPressureAndWake,
            RippleValidation
        }

        private enum FoamDebugCategory
        {
            LayerATopology,
            LayerBVelocity,
            LayerCMaterial,
            LayerDPrimary,
            LayerDAdvancedInternals,
            LayerDComparisons,
            LayerERendering
        }

        private struct RiverDebugState
        {
            public int Body;
            public int Motion;
            public int Refraction;
            public int Disturbance;
            public int Foam;
        }

        private static readonly string[] DebugFeatureLabels =
        {
            "Final Render",
            "Water Body",
            "Surface Motion",
            "Refraction",
            "Disturbances",
            "Foam"
        };

        private static readonly string[] BodyDebugLabels =
        {
            "Vertical Depth",
            "Depth Blend",
            "Transmission",
            "Body Coverage",
            "Scene Colour",
            "Depth Validity",
            "Surface Coverage",
            "Combined Lighting",
            "Ambient Lighting",
            "Sun Lighting",
            "Local Lighting",
            "Freeze Amount"
        };

        private static readonly int[] BodyDebugValues =
        {
            (int)StylizedRiverBodyDebugView.VerticalDepth,
            (int)StylizedRiverBodyDebugView.DepthBlend,
            (int)StylizedRiverBodyDebugView.Transmission,
            (int)StylizedRiverBodyDebugView.BodyCoverage,
            (int)StylizedRiverBodyDebugView.SceneColour,
            (int)StylizedRiverBodyDebugView.DepthValidity,
            (int)StylizedRiverBodyDebugView.SurfaceCoverage,
            (int)StylizedRiverBodyDebugView.CombinedLighting,
            (int)StylizedRiverBodyDebugView.AmbientLighting,
            (int)StylizedRiverBodyDebugView.SunLighting,
            (int)StylizedRiverBodyDebugView.LocalLighting,
            (int)StylizedRiverBodyDebugView.FreezeAmount
        };

        private static readonly string[] MotionDebugLabels =
        {
            "Bank Mask",
            "Macro Height",
            "Surface Normal",
            "Current Accent",
            "Liquid Factor"
        };

        private static readonly int[] MotionDebugValues =
        {
            (int)StylizedRiverMotionDebugView.BankMask,
            (int)StylizedRiverMotionDebugView.MacroHeight,
            (int)StylizedRiverMotionDebugView.SurfaceNormal,
            (int)StylizedRiverMotionDebugView.CurrentAccent,
            (int)StylizedRiverMotionDebugView.LiquidFactor
        };

        private static readonly string[] RefractionDebugLabels =
        {
            "Refracted Scene",
            "Offset",
            "Depth Influence",
            "Shore Mask",
            "Sample Validity",
            "Ice Diffusion"
        };

        private static readonly int[] RefractionDebugValues =
        {
            (int)StylizedRiverRefractionDebugView.RefractedScene,
            (int)StylizedRiverRefractionDebugView.Offset,
            (int)StylizedRiverRefractionDebugView.DepthInfluence,
            (int)StylizedRiverRefractionDebugView.ShoreMask,
            (int)StylizedRiverRefractionDebugView.SampleValidity,
            (int)StylizedRiverRefractionDebugView.IceDiffusion
        };

        private static readonly string[] DisturbanceCategoryLabels =
        {
            "Primary Field",
            "Static Pressure & Wake",
            "Ripple Validation"
        };

        private static readonly string[] DisturbancePrimaryLabels =
        {
            "Height",
            "Velocity",
            "Normal",
            "Intensity",
            "Field Coordinates"
        };

        private static readonly int[] DisturbancePrimaryValues =
        {
            (int)StylizedRiverDisturbanceDebugView.Height,
            (int)StylizedRiverDisturbanceDebugView.Velocity,
            (int)StylizedRiverDisturbanceDebugView.Normal,
            (int)StylizedRiverDisturbanceDebugView.Intensity,
            (int)StylizedRiverDisturbanceDebugView.FieldCoordinates
        };

        private static readonly string[] DisturbanceWakeLabels =
        {
            "Static Pressure Target",
            "Static Wake Source",
            "Wake Energy",
            "Final Wake Geometry Height"
        };

        private static readonly int[] DisturbanceWakeValues =
        {
            (int)StylizedRiverDisturbanceDebugView.StaticPressureTarget,
            (int)StylizedRiverDisturbanceDebugView.StaticWakeSource,
            (int)StylizedRiverDisturbanceDebugView.WakeEnergy,
            (int)StylizedRiverDisturbanceDebugView.FinalWakeGeometryHeight
        };

        private static readonly string[] DisturbanceRippleLabels =
        {
            "Ripple Boundary"
        };

        private static readonly int[] DisturbanceRippleValues =
        {
            (int)StylizedRiverDisturbanceDebugView.RippleBoundary
        };

        private static readonly string[] FoamCategoryLabels =
        {
            "Layer A — Topology",
            "Layer B — Velocity",
            "Layer C — Persistent Material",
            "Layer D — Primary",
            "Layer D — Advanced Internals",
            "Layer D — Comparisons",
            "Layer E — Rendering"
        };

        private static readonly string[] FoamLayerALabels =
        {
            "Foam + Aging Topology"
        };

        private static readonly int[] FoamLayerAValues =
        {
            (int)StylizedRiverFoamDebugView.FoamAndAgingTopology
        };

        private static readonly string[] FoamLayerBLabels =
        {
            "Foam Motion Field",
            "Foam Motion Field + Cell Grid"
        };

        private static readonly int[] FoamLayerBValues =
        {
            (int)StylizedRiverFoamDebugView.FoamMotionField,
            (int)StylizedRiverFoamDebugView.FoamMotionFieldCellGrid
        };

        private static readonly string[] FoamLayerCLabels =
        {
            "Material Presence",
            "Material Remaining Life",
            "Progressive Birth Source (Test Source)"
        };

        private static readonly int[] FoamLayerCValues =
        {
            (int)StylizedRiverFoamDebugView.MaterialPresence,
            (int)StylizedRiverFoamDebugView.MaterialRemainingLife,
            (int)StylizedRiverFoamDebugView.ProgressiveBirthSource
        };

        private static readonly string[] FoamLayerDPrimaryLabels =
        {
            "Foam Evaluated Shape",
            "Foam Evaluated Final Preview"
        };

        private static readonly int[] FoamLayerDPrimaryValues =
        {
            (int)StylizedRiverFoamDebugView.FoamEvaluatedShape,
            (int)StylizedRiverFoamDebugView.FoamEvaluatedFinalPreview
        };

        private static readonly string[] FoamLayerDAdvancedLabels =
        {
            "Foam Film Source",
            "Foam Film Support",
            "Foam Instantaneous Film Target",
            "Foam Temporal Occupancy"
        };

        private static readonly int[] FoamLayerDAdvancedValues =
        {
            (int)StylizedRiverFoamDebugView.FoamFilmSource,
            (int)StylizedRiverFoamDebugView.FoamFilmSupport,
            (int)StylizedRiverFoamDebugView.FoamFilmTarget,
            (int)StylizedRiverFoamDebugView.FoamTemporalOccupancy
        };

        private static readonly string[] FoamLayerDComparisonLabels =
        {
            "Foam Shape Difference",
            "Foam Temporal Difference"
        };

        private static readonly int[] FoamLayerDComparisonValues =
        {
            (int)StylizedRiverFoamDebugView.FoamShapeDifference,
            (int)StylizedRiverFoamDebugView.FoamTemporalDifference
        };

        private static readonly string[] FoamLayerELabels =
        {
            "Foam Shader Detail Probe",
            "Foam Shader Detail Difference"
        };

        private static readonly int[] FoamLayerEValues =
        {
            (int)StylizedRiverFoamDebugView.FoamShaderDetailProbe,
            (int)StylizedRiverFoamDebugView.FoamShaderDetailDifference
        };

        private void DrawDebugViews()
        {
            SerializedProperty bodyProperty = Find("bodyDebugView");
            SerializedProperty motionProperty = Find("motionDebugView");
            SerializedProperty refractionProperty = Find("refractionDebugView");
            SerializedProperty disturbanceProperty = Find("disturbanceDebugView");
            SerializedProperty foamProperty = Find("foamDebugView");

            if (bodyProperty == null ||
                motionProperty == null ||
                refractionProperty == null ||
                disturbanceProperty == null ||
                foamProperty == null)
            {
                EditorGUILayout.HelpBox(
                    "The River debug hub could not find all five serialized " +
                    "debug fields. The Inspector and StylizedRiver component " +
                    "do not match.",
                    MessageType.Error);
                return;
            }

            EditorGUILayout.HelpBox(
                "One debug view is rendered at a time. Selecting a view here " +
                "sets the other River debug systems to Final without changing " +
                "simulation or authored production values.",
                MessageType.None);

            bool hasMixedSelection =
                bodyProperty.hasMultipleDifferentValues ||
                motionProperty.hasMultipleDifferentValues ||
                refractionProperty.hasMultipleDifferentValues ||
                disturbanceProperty.hasMultipleDifferentValues ||
                foamProperty.hasMultipleDifferentValues;

            RiverDebugFeature selectedFeature = hasMixedSelection
                ? RiverDebugFeature.FinalRender
                : ResolveRenderedDebugFeature(
                    ReadDebugState(serializedObject));

            EditorGUI.showMixedValue = hasMixedSelection;
            EditorGUI.BeginChangeCheck();
            int selectedFeatureIndex = EditorGUILayout.Popup(
                new GUIContent(
                    "Debug Feature",
                    "Final Render clears every debug substitution. Choosing " +
                    "another feature activates one diagnostic view and resets " +
                    "the other four feature selectors to Final."),
                (int)selectedFeature,
                DebugFeatureLabels);
            bool featureChanged = EditorGUI.EndChangeCheck();
            EditorGUI.showMixedValue = false;

            if (featureChanged)
            {
                selectedFeature =
                    (RiverDebugFeature)selectedFeatureIndex;
                SetExclusiveDebugView(
                    selectedFeature,
                    GetDefaultDebugView(selectedFeature));
                hasMixedSelection = false;
            }

            int selectedView = 0;
            if (!hasMixedSelection &&
                selectedFeature != RiverDebugFeature.FinalRender)
            {
                selectedView = DrawDebugViewSelector(selectedFeature);
            }
            else if (hasMixedSelection)
            {
                DrawReadOnlyRow(
                    new GUIContent("Debug View"),
                    "Mixed across selected rivers");
            }

            int conflictCount = DrawDebugSelectionStatus(hasMixedSelection);

            if (!hasMixedSelection &&
                selectedFeature != RiverDebugFeature.FinalRender)
            {
                string description =
                    GetDebugViewDescription(selectedFeature, selectedView);
                if (!string.IsNullOrEmpty(description))
                {
                    EditorGUILayout.HelpBox(
                        description,
                        MessageType.None);
                }
            }
            else if (!hasMixedSelection)
            {
                EditorGUILayout.HelpBox(
                    "The normal player-facing River render is active. No " +
                    "debug substitution is selected.",
                    MessageType.None);
            }

            EditorGUILayout.Space(3f);

            if (conflictCount > 0 &&
                GUILayout.Button(
                    new GUIContent(
                        "Normalize to Rendered View",
                        "Keeps the shader-winning diagnostic view on each " +
                        "selected river and resets every hidden lower-priority " +
                        "debug selector to Final.")))
            {
                NormalizeDebugViewsToRendered();
            }

            if (GUILayout.Button(
                    new GUIContent(
                        "Reset All Debug Views",
                        "Sets Water Body, Surface Motion, Refraction, " +
                        "Disturbances, and Foam debug selectors to Final on " +
                        "every selected river.")))
            {
                SetExclusiveDebugView(
                    RiverDebugFeature.FinalRender,
                    0);
            }
        }

        private int DrawDebugViewSelector(
            RiverDebugFeature feature)
        {
            switch (feature)
            {
                case RiverDebugFeature.WaterBody:
                    return DrawSimpleDebugViewSelector(
                        feature,
                        BodyDebugLabels,
                        BodyDebugValues);

                case RiverDebugFeature.SurfaceMotion:
                    return DrawSimpleDebugViewSelector(
                        feature,
                        MotionDebugLabels,
                        MotionDebugValues);

                case RiverDebugFeature.Refraction:
                    return DrawSimpleDebugViewSelector(
                        feature,
                        RefractionDebugLabels,
                        RefractionDebugValues);

                case RiverDebugFeature.Disturbances:
                    return DrawDisturbanceDebugViewSelector();

                case RiverDebugFeature.Foam:
                    return DrawFoamDebugViewSelector();

                default:
                    return 0;
            }
        }

        private int DrawSimpleDebugViewSelector(
            RiverDebugFeature feature,
            string[] labels,
            int[] values)
        {
            SerializedProperty property =
                GetDebugProperty(serializedObject, feature);
            int currentIndex =
                System.Array.IndexOf(values, property.intValue);
            if (currentIndex < 0)
            {
                currentIndex = 0;
            }

            EditorGUI.BeginChangeCheck();
            int selectedIndex = EditorGUILayout.Popup(
                new GUIContent(
                    "Debug View",
                    "The selected view becomes the only active River debug " +
                    "substitution."),
                currentIndex,
                labels);
            if (EditorGUI.EndChangeCheck())
            {
                SetExclusiveDebugView(
                    feature,
                    values[selectedIndex]);
            }

            return values[selectedIndex];
        }

        private int DrawDisturbanceDebugViewSelector()
        {
            SerializedProperty property =
                Find("disturbanceDebugView");
            DisturbanceDebugCategory category =
                GetDisturbanceDebugCategory(property.intValue);

            EditorGUI.BeginChangeCheck();
            int selectedCategoryIndex = EditorGUILayout.Popup(
                new GUIContent(
                    "Debug Category",
                    "Groups disturbance diagnostics by primary fields, " +
                    "static pressure and wake construction, or ripple " +
                    "boundary validation."),
                (int)category,
                DisturbanceCategoryLabels);
            if (EditorGUI.EndChangeCheck())
            {
                category =
                    (DisturbanceDebugCategory)selectedCategoryIndex;
                SetExclusiveDebugView(
                    RiverDebugFeature.Disturbances,
                    GetDefaultDisturbanceDebugView(category));
            }

            GetDisturbanceDebugOptions(
                category,
                out string[] labels,
                out int[] values);

            return DrawSimpleDebugViewSelector(
                RiverDebugFeature.Disturbances,
                labels,
                values);
        }

        private int DrawFoamDebugViewSelector()
        {
            SerializedProperty property = Find("foamDebugView");
            FoamDebugCategory category =
                GetFoamDebugCategory(property.intValue);

            EditorGUI.BeginChangeCheck();
            int selectedCategoryIndex = EditorGUILayout.Popup(
                new GUIContent(
                    "Debug Layer / Category",
                    "Organizes Foam diagnostics by the accepted Layer A–E " +
                    "architecture. Layer D internals and comparisons remain " +
                    "available without crowding primary views."),
                (int)category,
                FoamCategoryLabels);
            if (EditorGUI.EndChangeCheck())
            {
                category = (FoamDebugCategory)selectedCategoryIndex;
                SetExclusiveDebugView(
                    RiverDebugFeature.Foam,
                    GetDefaultFoamDebugView(category));
            }

            GetFoamDebugOptions(
                category,
                out string[] labels,
                out int[] values);

            return DrawSimpleDebugViewSelector(
                RiverDebugFeature.Foam,
                labels,
                values);
        }

        private int DrawDebugSelectionStatus(
            bool hasMixedSelection)
        {
            int conflictCount = 0;
            bool renderedMixed = false;
            bool hiddenMixed = false;
            string renderedLabel = null;
            string hiddenLabel = null;

            if (!hasMixedSelection)
            {
                RiverDebugState sharedState =
                    ReadDebugState(serializedObject);
                RiverDebugFeature sharedRenderedFeature =
                    ResolveRenderedDebugFeature(sharedState);
                renderedLabel = GetRenderedDebugViewLabel(
                    sharedRenderedFeature,
                    GetDebugViewValue(
                        sharedState,
                        sharedRenderedFeature));
                hiddenLabel = GetHiddenDebugViewsLabel(
                    sharedState,
                    sharedRenderedFeature);
                if (CountActiveDebugViews(sharedState) > 1)
                {
                    conflictCount = targets.Length;
                }
            }

            foreach (Object selectedTarget in targets)
            {
                if (!hasMixedSelection)
                {
                    break;
                }

                if (selectedTarget is not StylizedRiver)
                {
                    continue;
                }

                SerializedObject targetObject =
                    new SerializedObject(selectedTarget);
                targetObject.Update();

                RiverDebugState state = ReadDebugState(targetObject);
                RiverDebugFeature renderedFeature =
                    ResolveRenderedDebugFeature(state);
                string targetRenderedLabel =
                    GetRenderedDebugViewLabel(
                        renderedFeature,
                        GetDebugViewValue(
                            state,
                            renderedFeature));
                string targetHiddenLabel =
                    GetHiddenDebugViewsLabel(
                        state,
                        renderedFeature);

                if (CountActiveDebugViews(state) > 1)
                {
                    conflictCount++;
                }

                if (renderedLabel == null)
                {
                    renderedLabel = targetRenderedLabel;
                }
                else if (renderedLabel != targetRenderedLabel)
                {
                    renderedMixed = true;
                }

                if (hiddenLabel == null)
                {
                    hiddenLabel = targetHiddenLabel;
                }
                else if (hiddenLabel != targetHiddenLabel)
                {
                    hiddenMixed = true;
                }
            }

            if (string.IsNullOrEmpty(renderedLabel))
            {
                renderedLabel = "Unavailable";
            }

            if (string.IsNullOrEmpty(hiddenLabel))
            {
                hiddenLabel = "None";
            }

            DrawReadOnlyRow(
                new GUIContent("Active Rendered View"),
                renderedMixed
                    ? "Mixed across selection"
                    : renderedLabel);
            DrawReadOnlyRow(
                new GUIContent("Conflict State"),
                conflictCount > 0
                    ? $"{conflictCount} selected river(s) contain multiple " +
                      "active debug views"
                    : renderedMixed
                        ? "No per-river conflict; selected rivers differ"
                        : "Exclusive");
            DrawReadOnlyRow(
                new GUIContent("Hidden Active Views"),
                conflictCount == 0
                    ? "None"
                    : hiddenMixed
                        ? "Mixed across selection"
                        : hiddenLabel);
            DrawReadOnlyRow(
                new GUIContent("Shader Priority"),
                "Foam > Disturbances > Refraction > Motion > Water Body");

            return conflictCount;
        }

        private void SetExclusiveDebugView(
            RiverDebugFeature feature,
            int viewValue)
        {
            SetAllDebugPropertiesFinal(serializedObject);

            SerializedProperty selectedProperty =
                GetDebugProperty(serializedObject, feature);
            if (selectedProperty != null)
            {
                selectedProperty.intValue = viewValue;
            }

            RepaintScene();
        }

        private void NormalizeDebugViewsToRendered()
        {
            serializedObject.ApplyModifiedProperties();
            Undo.RecordObjects(
                targets,
                "Normalize River Debug Views");

            foreach (Object selectedTarget in targets)
            {
                if (selectedTarget is not StylizedRiver)
                {
                    continue;
                }

                SerializedObject targetObject =
                    new SerializedObject(selectedTarget);
                targetObject.Update();

                RiverDebugState state = ReadDebugState(targetObject);
                RiverDebugFeature renderedFeature =
                    ResolveRenderedDebugFeature(state);
                int renderedValue =
                    GetDebugViewValue(state, renderedFeature);

                SetAllDebugPropertiesFinal(targetObject);

                SerializedProperty renderedProperty =
                    GetDebugProperty(
                        targetObject,
                        renderedFeature);
                if (renderedProperty != null)
                {
                    renderedProperty.intValue = renderedValue;
                }

                targetObject.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(selectedTarget);
            }

            serializedObject.Update();
            Repaint();
            RepaintScene();
        }

        private static void SetAllDebugPropertiesFinal(
            SerializedObject source)
        {
            source.FindProperty("bodyDebugView").intValue =
                (int)StylizedRiverBodyDebugView.Final;
            source.FindProperty("motionDebugView").intValue =
                (int)StylizedRiverMotionDebugView.Final;
            source.FindProperty("refractionDebugView").intValue =
                (int)StylizedRiverRefractionDebugView.Final;
            source.FindProperty("disturbanceDebugView").intValue =
                (int)StylizedRiverDisturbanceDebugView.Final;
            source.FindProperty("foamDebugView").intValue =
                (int)StylizedRiverFoamDebugView.Final;
        }

        private static SerializedProperty GetDebugProperty(
            SerializedObject source,
            RiverDebugFeature feature)
        {
            return feature switch
            {
                RiverDebugFeature.WaterBody =>
                    source.FindProperty("bodyDebugView"),
                RiverDebugFeature.SurfaceMotion =>
                    source.FindProperty("motionDebugView"),
                RiverDebugFeature.Refraction =>
                    source.FindProperty("refractionDebugView"),
                RiverDebugFeature.Disturbances =>
                    source.FindProperty("disturbanceDebugView"),
                RiverDebugFeature.Foam =>
                    source.FindProperty("foamDebugView"),
                _ => null
            };
        }

        private static RiverDebugState ReadDebugState(
            SerializedObject source)
        {
            return new RiverDebugState
            {
                Body = source.FindProperty("bodyDebugView").intValue,
                Motion = source.FindProperty("motionDebugView").intValue,
                Refraction =
                    source.FindProperty("refractionDebugView").intValue,
                Disturbance =
                    source.FindProperty("disturbanceDebugView").intValue,
                Foam = source.FindProperty("foamDebugView").intValue
            };
        }

        private static RiverDebugFeature ResolveRenderedDebugFeature(
            RiverDebugState state)
        {
            if (state.Foam != (int)StylizedRiverFoamDebugView.Final)
            {
                return RiverDebugFeature.Foam;
            }

            if (state.Disturbance !=
                (int)StylizedRiverDisturbanceDebugView.Final)
            {
                return RiverDebugFeature.Disturbances;
            }

            if (state.Refraction !=
                (int)StylizedRiverRefractionDebugView.Final)
            {
                return RiverDebugFeature.Refraction;
            }

            if (state.Motion !=
                (int)StylizedRiverMotionDebugView.Final)
            {
                return RiverDebugFeature.SurfaceMotion;
            }

            if (state.Body != (int)StylizedRiverBodyDebugView.Final)
            {
                return RiverDebugFeature.WaterBody;
            }

            return RiverDebugFeature.FinalRender;
        }

        private static int CountActiveDebugViews(
            RiverDebugState state)
        {
            int count = 0;
            count += state.Body !=
                (int)StylizedRiverBodyDebugView.Final ? 1 : 0;
            count += state.Motion !=
                (int)StylizedRiverMotionDebugView.Final ? 1 : 0;
            count += state.Refraction !=
                (int)StylizedRiverRefractionDebugView.Final ? 1 : 0;
            count += state.Disturbance !=
                (int)StylizedRiverDisturbanceDebugView.Final ? 1 : 0;
            count += state.Foam !=
                (int)StylizedRiverFoamDebugView.Final ? 1 : 0;
            return count;
        }

        private static int GetDebugViewValue(
            RiverDebugState state,
            RiverDebugFeature feature)
        {
            return feature switch
            {
                RiverDebugFeature.WaterBody => state.Body,
                RiverDebugFeature.SurfaceMotion => state.Motion,
                RiverDebugFeature.Refraction => state.Refraction,
                RiverDebugFeature.Disturbances => state.Disturbance,
                RiverDebugFeature.Foam => state.Foam,
                _ => 0
            };
        }

        private static int GetDefaultDebugView(
            RiverDebugFeature feature)
        {
            return feature switch
            {
                RiverDebugFeature.WaterBody =>
                    (int)StylizedRiverBodyDebugView.VerticalDepth,
                RiverDebugFeature.SurfaceMotion =>
                    (int)StylizedRiverMotionDebugView.BankMask,
                RiverDebugFeature.Refraction =>
                    (int)StylizedRiverRefractionDebugView.RefractedScene,
                RiverDebugFeature.Disturbances =>
                    (int)StylizedRiverDisturbanceDebugView.Height,
                RiverDebugFeature.Foam =>
                    (int)StylizedRiverFoamDebugView.MaterialPresence,
                _ => 0
            };
        }

        private static DisturbanceDebugCategory
            GetDisturbanceDebugCategory(int viewValue)
        {
            if (System.Array.IndexOf(
                    DisturbanceWakeValues,
                    viewValue) >= 0)
            {
                return DisturbanceDebugCategory.StaticPressureAndWake;
            }

            if (System.Array.IndexOf(
                    DisturbanceRippleValues,
                    viewValue) >= 0)
            {
                return DisturbanceDebugCategory.RippleValidation;
            }

            return DisturbanceDebugCategory.PrimaryField;
        }

        private static int GetDefaultDisturbanceDebugView(
            DisturbanceDebugCategory category)
        {
            return category switch
            {
                DisturbanceDebugCategory.StaticPressureAndWake =>
                    DisturbanceWakeValues[0],
                DisturbanceDebugCategory.RippleValidation =>
                    DisturbanceRippleValues[0],
                _ => DisturbancePrimaryValues[0]
            };
        }

        private static void GetDisturbanceDebugOptions(
            DisturbanceDebugCategory category,
            out string[] labels,
            out int[] values)
        {
            switch (category)
            {
                case DisturbanceDebugCategory.StaticPressureAndWake:
                    labels = DisturbanceWakeLabels;
                    values = DisturbanceWakeValues;
                    break;

                case DisturbanceDebugCategory.RippleValidation:
                    labels = DisturbanceRippleLabels;
                    values = DisturbanceRippleValues;
                    break;

                default:
                    labels = DisturbancePrimaryLabels;
                    values = DisturbancePrimaryValues;
                    break;
            }
        }

        private static FoamDebugCategory GetFoamDebugCategory(
            int viewValue)
        {
            if (System.Array.IndexOf(FoamLayerAValues, viewValue) >= 0)
            {
                return FoamDebugCategory.LayerATopology;
            }

            if (System.Array.IndexOf(FoamLayerBValues, viewValue) >= 0)
            {
                return FoamDebugCategory.LayerBVelocity;
            }

            if (System.Array.IndexOf(FoamLayerDPrimaryValues, viewValue) >= 0)
            {
                return FoamDebugCategory.LayerDPrimary;
            }

            if (System.Array.IndexOf(
                    FoamLayerDAdvancedValues,
                    viewValue) >= 0)
            {
                return FoamDebugCategory.LayerDAdvancedInternals;
            }

            if (System.Array.IndexOf(
                    FoamLayerDComparisonValues,
                    viewValue) >= 0)
            {
                return FoamDebugCategory.LayerDComparisons;
            }

            if (System.Array.IndexOf(FoamLayerEValues, viewValue) >= 0)
            {
                return FoamDebugCategory.LayerERendering;
            }

            return FoamDebugCategory.LayerCMaterial;
        }

        private static int GetDefaultFoamDebugView(
            FoamDebugCategory category)
        {
            return category switch
            {
                FoamDebugCategory.LayerATopology =>
                    FoamLayerAValues[0],
                FoamDebugCategory.LayerBVelocity =>
                    FoamLayerBValues[0],
                FoamDebugCategory.LayerDPrimary =>
                    FoamLayerDPrimaryValues[0],
                FoamDebugCategory.LayerDAdvancedInternals =>
                    FoamLayerDAdvancedValues[0],
                FoamDebugCategory.LayerDComparisons =>
                    FoamLayerDComparisonValues[0],
                FoamDebugCategory.LayerERendering =>
                    FoamLayerEValues[0],
                _ => FoamLayerCValues[0]
            };
        }

        private static void GetFoamDebugOptions(
            FoamDebugCategory category,
            out string[] labels,
            out int[] values)
        {
            switch (category)
            {
                case FoamDebugCategory.LayerATopology:
                    labels = FoamLayerALabels;
                    values = FoamLayerAValues;
                    break;

                case FoamDebugCategory.LayerBVelocity:
                    labels = FoamLayerBLabels;
                    values = FoamLayerBValues;
                    break;

                case FoamDebugCategory.LayerDPrimary:
                    labels = FoamLayerDPrimaryLabels;
                    values = FoamLayerDPrimaryValues;
                    break;

                case FoamDebugCategory.LayerDAdvancedInternals:
                    labels = FoamLayerDAdvancedLabels;
                    values = FoamLayerDAdvancedValues;
                    break;

                case FoamDebugCategory.LayerDComparisons:
                    labels = FoamLayerDComparisonLabels;
                    values = FoamLayerDComparisonValues;
                    break;

                case FoamDebugCategory.LayerERendering:
                    labels = FoamLayerELabels;
                    values = FoamLayerEValues;
                    break;

                default:
                    labels = FoamLayerCLabels;
                    values = FoamLayerCValues;
                    break;
            }
        }

        private static string GetRenderedDebugViewLabel(
            RiverDebugFeature feature,
            int viewValue)
        {
            if (feature == RiverDebugFeature.FinalRender)
            {
                return "Final Render";
            }

            return
                $"{GetDebugFeatureLabel(feature)} / " +
                GetDebugViewLabel(feature, viewValue);
        }

        private static string GetHiddenDebugViewsLabel(
            RiverDebugState state,
            RiverDebugFeature renderedFeature)
        {
            List<string> hiddenViews = new List<string>();

            AddHiddenDebugView(
                hiddenViews,
                state,
                RiverDebugFeature.Foam,
                renderedFeature);
            AddHiddenDebugView(
                hiddenViews,
                state,
                RiverDebugFeature.Disturbances,
                renderedFeature);
            AddHiddenDebugView(
                hiddenViews,
                state,
                RiverDebugFeature.Refraction,
                renderedFeature);
            AddHiddenDebugView(
                hiddenViews,
                state,
                RiverDebugFeature.SurfaceMotion,
                renderedFeature);
            AddHiddenDebugView(
                hiddenViews,
                state,
                RiverDebugFeature.WaterBody,
                renderedFeature);

            return hiddenViews.Count > 0
                ? string.Join("; ", hiddenViews)
                : "None";
        }

        private static void AddHiddenDebugView(
            List<string> hiddenViews,
            RiverDebugState state,
            RiverDebugFeature candidate,
            RiverDebugFeature renderedFeature)
        {
            if (candidate == renderedFeature)
            {
                return;
            }

            int value = GetDebugViewValue(state, candidate);
            if (value == 0)
            {
                return;
            }

            hiddenViews.Add(
                $"{GetDebugFeatureLabel(candidate)} / " +
                GetDebugViewLabel(candidate, value));
        }

        private static string GetDebugFeatureLabel(
            RiverDebugFeature feature)
        {
            return feature switch
            {
                RiverDebugFeature.WaterBody => "Water Body",
                RiverDebugFeature.SurfaceMotion => "Surface Motion",
                RiverDebugFeature.Refraction => "Refraction",
                RiverDebugFeature.Disturbances => "Disturbances",
                RiverDebugFeature.Foam => "Foam",
                _ => "Final Render"
            };
        }

        private static string GetDebugViewLabel(
            RiverDebugFeature feature,
            int viewValue)
        {
            switch (feature)
            {
                case RiverDebugFeature.WaterBody:
                    return GetOptionLabel(
                        BodyDebugLabels,
                        BodyDebugValues,
                        viewValue);

                case RiverDebugFeature.SurfaceMotion:
                    return GetOptionLabel(
                        MotionDebugLabels,
                        MotionDebugValues,
                        viewValue);

                case RiverDebugFeature.Refraction:
                    return GetOptionLabel(
                        RefractionDebugLabels,
                        RefractionDebugValues,
                        viewValue);

                case RiverDebugFeature.Disturbances:
                    if (System.Array.IndexOf(
                            DisturbanceWakeValues,
                            viewValue) >= 0)
                    {
                        return GetOptionLabel(
                            DisturbanceWakeLabels,
                            DisturbanceWakeValues,
                            viewValue);
                    }

                    if (System.Array.IndexOf(
                            DisturbanceRippleValues,
                            viewValue) >= 0)
                    {
                        return GetOptionLabel(
                            DisturbanceRippleLabels,
                            DisturbanceRippleValues,
                            viewValue);
                    }

                    return GetOptionLabel(
                        DisturbancePrimaryLabels,
                        DisturbancePrimaryValues,
                        viewValue);

                case RiverDebugFeature.Foam:
                    GetFoamDebugOptions(
                        GetFoamDebugCategory(viewValue),
                        out string[] labels,
                        out int[] values);
                    return GetOptionLabel(
                        labels,
                        values,
                        viewValue);

                default:
                    return "Final Render";
            }
        }

        private static string GetOptionLabel(
            string[] labels,
            int[] values,
            int viewValue)
        {
            int index = System.Array.IndexOf(values, viewValue);
            return index >= 0 && index < labels.Length
                ? labels[index]
                : $"Unknown ({viewValue})";
        }

        private static string GetDebugViewDescription(
            RiverDebugFeature feature,
            int viewValue)
        {
            return feature switch
            {
                RiverDebugFeature.WaterBody =>
                    GetBodyDebugViewDescription(
                        (StylizedRiverBodyDebugView)viewValue),
                RiverDebugFeature.SurfaceMotion =>
                    GetMotionDebugViewDescription(
                        (StylizedRiverMotionDebugView)viewValue),
                RiverDebugFeature.Refraction =>
                    GetRefractionDebugViewDescription(
                        (StylizedRiverRefractionDebugView)viewValue),
                RiverDebugFeature.Disturbances =>
                    GetDisturbanceDebugViewDescription(
                        (StylizedRiverDisturbanceDebugView)viewValue),
                RiverDebugFeature.Foam =>
                    GetFoamDebugViewDescription(
                        (StylizedRiverFoamDebugView)viewValue),
                _ => string.Empty
            };
        }

        private static string GetBodyDebugViewDescription(
            StylizedRiverBodyDebugView view)
        {
            return view switch
            {
                StylizedRiverBodyDebugView.VerticalDepth =>
                    "Shows resolved vertical water-body depth.",
                StylizedRiverBodyDebugView.DepthBlend =>
                    "Shows the depth-driven body-colour blend.",
                StylizedRiverBodyDebugView.Transmission =>
                    "Shows light and scene transmission through the body.",
                StylizedRiverBodyDebugView.BodyCoverage =>
                    "Shows valid generated water-body coverage.",
                StylizedRiverBodyDebugView.SceneColour =>
                    "Shows the sampled scene colour used by the body.",
                StylizedRiverBodyDebugView.DepthValidity =>
                    "Shows whether scene-depth information is valid.",
                StylizedRiverBodyDebugView.SurfaceCoverage =>
                    "Shows generated surface coverage.",
                StylizedRiverBodyDebugView.CombinedLighting =>
                    "Shows the combined ambient, sun, and local-light response.",
                StylizedRiverBodyDebugView.AmbientLighting =>
                    "Shows only the ambient-light contribution.",
                StylizedRiverBodyDebugView.SunLighting =>
                    "Shows only the main directional-light contribution.",
                StylizedRiverBodyDebugView.LocalLighting =>
                    "Shows only local and additional-light contribution.",
                StylizedRiverBodyDebugView.FreezeAmount =>
                    "Shows the resolved liquid-to-frozen blend.",
                _ => "Shows a Water Body rendering diagnostic."
            };
        }

        private static string GetMotionDebugViewDescription(
            StylizedRiverMotionDebugView view)
        {
            return view switch
            {
                StylizedRiverMotionDebugView.BankMask =>
                    "Shows the shoreline attenuation mask used by surface motion.",
                StylizedRiverMotionDebugView.MacroHeight =>
                    "Shows resolved macro vertical displacement.",
                StylizedRiverMotionDebugView.SurfaceNormal =>
                    "Shows the final animated surface normal.",
                StylizedRiverMotionDebugView.CurrentAccent =>
                    "Shows flow-aligned current-accent coverage.",
                StylizedRiverMotionDebugView.LiquidFactor =>
                    "Shows the liquid-versus-frozen motion factor.",
                _ => "Shows a Surface Motion rendering diagnostic."
            };
        }

        private static string GetRefractionDebugViewDescription(
            StylizedRiverRefractionDebugView view)
        {
            return view switch
            {
                StylizedRiverRefractionDebugView.RefractedScene =>
                    "Shows the sampled refracted scene before normal body composition.",
                StylizedRiverRefractionDebugView.Offset =>
                    "Shows the resolved screen-space refraction offset.",
                StylizedRiverRefractionDebugView.DepthInfluence =>
                    "Shows the depth contribution to refraction strength.",
                StylizedRiverRefractionDebugView.ShoreMask =>
                    "Shows shoreline protection applied to refraction.",
                StylizedRiverRefractionDebugView.SampleValidity =>
                    "Shows whether the displaced scene sample is valid.",
                StylizedRiverRefractionDebugView.IceDiffusion =>
                    "Shows frozen-surface transmission diffusion.",
                _ => "Shows a Refraction rendering diagnostic."
            };
        }

        private static string GetDisturbanceDebugViewDescription(
            StylizedRiverDisturbanceDebugView debugView)
        {
            return debugView switch
            {
                StylizedRiverDisturbanceDebugView.Height =>
                    "Shows the persistent signed disturbance-height field.",
                StylizedRiverDisturbanceDebugView.Velocity =>
                    "Shows the persistent disturbance-velocity field.",
                StylizedRiverDisturbanceDebugView.Normal =>
                    "Shows the surface-normal response produced by runtime disturbances.",
                StylizedRiverDisturbanceDebugView.Intensity =>
                    "Shows composed disturbance intensity before final water shading.",
                StylizedRiverDisturbanceDebugView.FieldCoordinates =>
                    "Shows the runtime disturbance field coordinate mapping.",
                StylizedRiverDisturbanceDebugView.StaticPressureTarget =>
                    "Shows the static pressure target derived from registered obstructions.",
                StylizedRiverDisturbanceDebugView.StaticWakeSource =>
                    "Red is rear-release energy, green is the attached geometry-aware lee, and blue is reach/persistence metadata.",
                StylizedRiverDisturbanceDebugView.WakeEnergy =>
                    "Shows the shared persistent wake field after injection, transport, widening, decay, bank masking, and freeze suppression.",
                StylizedRiverDisturbanceDebugView.RippleBoundary =>
                    "Green is open water, black is fully absorbing coverage, and red shows reflection hardness. Shores appear as soft absorption bands; participating solids appear as compact harder boundaries.",
                StylizedRiverDisturbanceDebugView.FinalWakeGeometryHeight =>
                    "Mid-gray is zero, darker values are the attached lee depression, and brighter values are positive transported trail height. The fixed encoding spans -0.40 m to +0.40 m.",
                _ => "Shows a Runtime Disturbance diagnostic."
            };
        }

        private static string GetFoamDebugViewDescription(
            StylizedRiverFoamDebugView view)
        {
            switch (view)
            {
                case StylizedRiverFoamDebugView.Final:
                    return
                        "The exact normal player-facing Foam result from the current committed Layer C state after conservative transport, topology-adjusted Remaining Life, surface coupling, lighting, and final Presence coverage. Rejected render-only residual prediction is no longer active.";

                case StylizedRiverFoamDebugView.FoamAndAgingTopology:
                    return
                        "One combined lifecycle-validation view. Dark water is neutral valid fluid. Green is the maximum positive lifespan support from Major, Connector, Pressure, Lee, and Shore Support. Red is Negative Aging Pressure. Yellow is their overlap. Blue is the canonical current-water Obstacle Footprint. Bright cyan/white is the exact final Foam mask used by normal rendering. Remaining Life is verified through the Material Lifetime and Topology Interaction summaries, not by broad Foam opacity.";

                case StylizedRiverFoamDebugView.ProgressiveBirthSource:
                    return
                        "Source isolation before persistent transport and aging. Blue is the complete planned accepted source, green is cumulative accepted source geometry since the latest idle start, red is source submitted during the latest material update, and yellow is the current emission head. Amount selects deterministic coherent area rather than persistent intensity.";

                case StylizedRiverFoamDebugView.MaterialPresence:
                    return
                        "Raw committed persistent material Presence sampled directly from the current Layer C texture at the unshifted field coordinate. Brightness is literal stored amount, so low-density donor-cell tails may remain dark even when presence-gated comparison views still acknowledge meaningful material.";

                case StylizedRiverFoamDebugView.MaterialRemainingLife:
                    return
                        "Normalized Remaining Life decoded from the committed Layer C life moment, then multiplied by the shared meaningful-Presence visibility gate (0.02 to 0.16). Brightness still represents life inside meaningful material, while tiny low-density transport tails no longer appear as full white coverage.";

                case StylizedRiverFoamDebugView.FoamMotionField:
                    return
                        "Unified resolved Foam velocity contract. Bright neutral gray is straight full-speed downstream motion, red is rightward lateral velocity, blue is leftward lateral velocity, darker values are downstream slowdown/stagnation, and yellow marks obstacle-routing influence. Semi-transparent white uses the shared meaningful-Presence visibility gate at the committed unshifted coordinate; it is an ownership overlay, not raw Presence amplitude.";

                case StylizedRiverFoamDebugView.FoamMotionFieldCellGrid:
                    return
                        "Unified resolved Foam velocity contract plus the committed persistent simulation-cell grid. Brightness shows downstream speed factor, red/blue show signed lateral velocity, yellow shows obstacle routing, and white uses the shared meaningful-Presence visibility gate. Fine dark lines show individual Foam cells; pale lines show eight-cell blocks. Neither overlay nor grid follows render-only residual displacement.";

                case StylizedRiverFoamDebugView.FoamEvaluatedShape:
                    return
                        "Layer D evaluated Foam Shape sampled from _FoamShapeMask. It intentionally combines full-resolution committed Presence with broader half-resolution temporal occupancy, so its footprint may exceed raw material. It is diagnostic-only, does not mutate FoamState, and is not yet consumed by Final Foam.";

                case StylizedRiverFoamDebugView.FoamShapeDifference:
                    return
                        "Layer D difference diagnostic. Black means _FoamShapeMask matches raw persistent Material Presence, green means evaluated shape adds visual coverage, and magenta/red means evaluated shape removes visual coverage. This exists so Layer D changes are visible without guessing between two similar masks.";

                case StylizedRiverFoamDebugView.FoamShaderDetailProbe:
                    return
                        "Layer E production breakup mask after the committed Final Foam silhouette receives the current Chip, Fray, and derived short-cut controls. It is render-only: it does not write FoamState or _FoamShapeMask, but it is the same local morphology consumed by normal Final Foam.";

                case StylizedRiverFoamDebugView.FoamShaderDetailDifference:
                    return
                        "Layer E production breakup difference. Black means the committed Final Foam silhouette is unchanged; magenta/red means coverage is removed by chips, fray, or short cuts. This proof never adds coverage, so the green channel remains zero.";

                case StylizedRiverFoamDebugView.FoamFilmSource:
                    return
                        "Layer D half-resolution Film Source diagnostic. After 4.11C.5.13C this is material-derived: persistent material creates source coverage, while external support/contact fields only bias or suppress it. It is visual-film source, not durable FoamState and not raw topology support.";

                case StylizedRiverFoamDebugView.FoamFilmSupport:
                    return
                        "Layer D half-resolution Film Support diagnostic after directional spread. It spreads material-derived Film Source; external support/contact fields may bias or suppress spread but cannot seed visual film from zero. It does not mutate persistent material.";

                case StylizedRiverFoamDebugView.FoamFilmTarget:
                    return
                        "Instantaneous half-resolution Layer D target derived from Film Source and Film Support before temporal memory. Compare this against Temporal Occupancy to see acquisition and release lag.";

                case StylizedRiverFoamDebugView.FoamTemporalOccupancy:
                    return
                        "The advected half-resolution Layer D visual sheet. One occupancy texel covers four full-resolution material cells, so narrow edges may appear broader or coarser than Material Presence. It moves through the canonical local velocity and closed-face rules, builds/releases toward the instantaneous film target, and never changes Presence or Remaining Life.";

                case StylizedRiverFoamDebugView.FoamTemporalDifference:
                    return
                        "Temporal occupancy difference. Green is visual coverage retained beyond the current instantaneous target; magenta is target coverage not yet acquired. Black means temporal occupancy and the instantaneous target agree.";

                case StylizedRiverFoamDebugView.FoamEvaluatedFinalPreview:
                    return
                        "Production-style Layer D preview. Renders the existing _FoamShapeMask with committed Presence, Remaining Life, and Material Pattern sampled at the same unshifted coordinate. It remains diagnostic-only while Final Foam uses committed Layer C state directly.";

                default:
                    return
                        "The exact normal player-facing Foam result. No diagnostic substitution is active.";
            }
        }
    }
}
