using System;
using UnityEditor;
using UnityEngine;

namespace ProgrammaticStylized3D.Vegetation.Editor
{
    public static class VegetationLayerAuthoring
    {
        private static readonly string[] ProductionPropertyNames =
        {
            "densityPerSquareMetre",
            "seed",
            "minimumCoverage",
            "clusterDiameter",
            "grassHeight",
            "masterBladeWidth",
            "tipWidthRatio",
            "taperStart",
            "enableWidthStabilization",
            "widthStabilizationStartDistance",
            "widthStabilizationMaximumMultiplier",
            "heightScaleRange",
            "widthScaleRange",
            "stiffnessRange",
            "grassPatchScale",
            "grassPatchPatternSeed",
            "grassPatchTransitionSoftness",
            "averageGrassPatchSeparation",
            "darkPatchStrength",
            "lightPatchStrength",
            "interactionBendResponse",
            "interactionFlattenResponse",
            "interactionHeightExponent",
            "maximumInteractionBendMetres",
            "interactionNormalResponse",
            "rootColor",
            "baseColor",
            "tipColor",
            "ambientResponse",
            "sunResponse",
            "localLightResponse",
            "minimumNightVisibility",
            "diffuseWrap",
            "normalUpBias",
            "windNormalResponse",
            "windBendShadingResponse",
            "lightColourInfluence",
            "stylizedEdgeAccent",
            "edgeAccentWidth",
            "minimumStableAccentPixels",
            "edgeHighlightWhiteness",
            "localEdgeFalloffPower",
            "localEdgeActivationThreshold",
            "targetCamera",
            "renderBenchmark",
            "sceneViewPreview"
        };

        public static int ProductionPropertyCount =>
            ProductionPropertyNames.Length;

        public static VegetationLayer CreateEmptyLayer(
            GroundVegetation root,
            string requestedName)
        {
            ValidateRoot(root, "Create Empty Layer");

            VegetationLayer layer = CreateLayerObject(root, requestedName);
            Undo.RegisterCompleteObjectUndo(
                layer,
                "Initialize Empty Vegetation Layer");
            layer.InitializeCoverage(false);
            EditorUtility.SetDirty(layer);
            return layer;
        }

        public static VegetationLayer DuplicateLayerAsEmpty(
            GroundVegetation root,
            VegetationLayer source)
        {
            ValidateRoot(root, "Duplicate Recipe as Empty");
            if (source == null || source.transform.parent != root.transform)
            {
                throw new InvalidOperationException(
                    "Duplicate Recipe as Empty requires one direct " +
                    "VegetationLayer child of this GroundVegetation root.");
            }

            VegetationLayer destination = CreateLayerObject(
                root,
                source.name + " Copy");
            CopyProductionConfiguration(source, destination);
            destination.SetCoverageAuthoringSettings(
                source.CoveragePaintMode,
                source.CoverageBrushRadius,
                source.CoverageBrushStrength,
                source.CoverageEraseMode,
                source.ShowCoverageOverlay);
            destination.InitializeCoverage(false);
            EditorUtility.SetDirty(destination);
            return destination;
        }

        public static void CopyProductionConfiguration(
            VegetationLayer source,
            VegetationLayer destination)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            var sourceObject = new SerializedObject(source);
            var destinationObject = new SerializedObject(destination);
            sourceObject.UpdateIfRequiredOrScript();
            destinationObject.UpdateIfRequiredOrScript();
            for (int index = 0; index < ProductionPropertyNames.Length; index++)
            {
                string propertyName = ProductionPropertyNames[index];
                SerializedProperty sourceProperty =
                    sourceObject.FindProperty(propertyName);
                SerializedProperty destinationProperty =
                    destinationObject.FindProperty(propertyName);
                if (sourceProperty == null || destinationProperty == null)
                {
                    throw new InvalidOperationException(
                        "Production recipe property is missing: " +
                        propertyName);
                }

                CopyPropertyValue(sourceProperty, destinationProperty);
            }
            destinationObject.ApplyModifiedProperties();
        }

        private static void ValidateRoot(
            GroundVegetation root,
            string actionName)
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }
            if (root.SurfaceGround == null)
            {
                throw new InvalidOperationException(
                    actionName + " requires a GroundVegetation component " +
                    "beneath a GeneratedGround ancestor.");
            }
        }

        private static VegetationLayer CreateLayerObject(
            GroundVegetation root,
            string requestedName)
        {
            string baseName = string.IsNullOrWhiteSpace(requestedName)
                ? "Vegetation Layer"
                : requestedName;
            string uniqueName = ResolveUniqueDirectChildName(
                root.transform,
                baseName);
            var layerObject = new GameObject(uniqueName);
            Undo.RegisterCreatedObjectUndo(
                layerObject,
                "Create Vegetation Recipe Layer");
            Undo.SetTransformParent(
                layerObject.transform,
                root.transform,
                "Parent Vegetation Recipe Layer");
            NormalizeLocalTransform(layerObject.transform);
            return Undo.AddComponent<VegetationLayer>(layerObject);
        }

        private static string ResolveUniqueDirectChildName(
            Transform parent,
            string requestedName)
        {
            string candidate = requestedName;
            int suffix = 2;
            while (FindDirectChild(parent, candidate) != null)
            {
                candidate = requestedName + " " + suffix;
                suffix++;
            }
            return candidate;
        }

        private static Transform FindDirectChild(
            Transform parent,
            string childName)
        {
            for (int index = 0; index < parent.childCount; index++)
            {
                Transform child = parent.GetChild(index);
                if (child.name == childName)
                {
                    return child;
                }
            }
            return null;
        }

        private static void NormalizeLocalTransform(Transform value)
        {
            value.localPosition = Vector3.zero;
            value.localRotation = Quaternion.identity;
            value.localScale = Vector3.one;
        }

        private static void CopyPropertyValue(
            SerializedProperty source,
            SerializedProperty destination)
        {
            if (source.propertyType != destination.propertyType)
            {
                throw new InvalidOperationException(
                    "Serialized property type mismatch for " +
                    source.propertyPath);
            }

            switch (source.propertyType)
            {
                case SerializedPropertyType.Integer:
                    destination.intValue = source.intValue;
                    break;
                case SerializedPropertyType.Boolean:
                    destination.boolValue = source.boolValue;
                    break;
                case SerializedPropertyType.Float:
                    destination.floatValue = source.floatValue;
                    break;
                case SerializedPropertyType.String:
                    destination.stringValue = source.stringValue;
                    break;
                case SerializedPropertyType.Color:
                    destination.colorValue = source.colorValue;
                    break;
                case SerializedPropertyType.ObjectReference:
                    destination.objectReferenceValue =
                        source.objectReferenceValue;
                    break;
                case SerializedPropertyType.Enum:
                    destination.enumValueIndex = source.enumValueIndex;
                    break;
                case SerializedPropertyType.Vector2:
                    destination.vector2Value = source.vector2Value;
                    break;
                case SerializedPropertyType.Vector3:
                    destination.vector3Value = source.vector3Value;
                    break;
                case SerializedPropertyType.Vector4:
                    destination.vector4Value = source.vector4Value;
                    break;
                case SerializedPropertyType.Rect:
                    destination.rectValue = source.rectValue;
                    break;
                case SerializedPropertyType.Bounds:
                    destination.boundsValue = source.boundsValue;
                    break;
                case SerializedPropertyType.Quaternion:
                    destination.quaternionValue = source.quaternionValue;
                    break;
                default:
                    throw new InvalidOperationException(
                        "Unsupported production property type " +
                        source.propertyType + " for " + source.propertyPath);
            }
        }
    }
}
