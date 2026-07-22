using System;
using System.Collections.Generic;
using System.Text;
using ProgrammaticStylized3D.Geometry.Ground;
using UnityEngine;

namespace ProgrammaticStylized3D.Vegetation
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [AddComponentMenu("PS3D/Vegetation/Ground Vegetation")]
    public sealed class GroundVegetation : MonoBehaviour
    {
        [NonSerialized]
        private GeneratedGround surfaceGround;

        public GeneratedGround SurfaceGround
        {
            get
            {
                SynchronizeSurfaceGroundFromHierarchy();
                return surfaceGround;
            }
        }

        public int DirectLayerCount
        {
            get
            {
                var layers = new List<VegetationLayer>();
                CollectDirectLayers(layers);
                return layers.Count;
            }
        }

        private void OnEnable()
        {
            SynchronizeSurfaceGroundFromHierarchy();
        }

        private void OnValidate()
        {
            SynchronizeSurfaceGroundFromHierarchy();
        }

        private void OnTransformParentChanged()
        {
            SynchronizeSurfaceGroundFromHierarchy();
        }

        public void CollectDirectLayers(List<VegetationLayer> results)
        {
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }

            results.Clear();
            for (int childIndex = 0; childIndex < transform.childCount; childIndex++)
            {
                Transform child = transform.GetChild(childIndex);
                if (child != null &&
                    child.TryGetComponent(out VegetationLayer layer))
                {
                    results.Add(layer);
                }
            }
        }

        public string BuildLayerStackReport()
        {
            var layers = new List<VegetationLayer>();
            CollectDirectLayers(layers);
            var builder = new StringBuilder(4096);
            builder.AppendLine("[Vegetation INFRA.1B Ground Layer Stack]");
            builder.Append("Vegetation root: ").AppendLine(name);
            builder.Append("Resolved Ground: ")
                .AppendLine(SurfaceGround != null ? SurfaceGround.name : "None");
            builder.Append("Direct recipe layers: ").AppendLine(layers.Count.ToString());

            long totalInstances = 0L;
            long totalTriangles = 0L;
            long totalBufferBytes = 0L;
            for (int index = 0; index < layers.Count; index++)
            {
                VegetationLayer layer = layers[index];
                long submittedTriangles =
                    (long)layer.InstanceCount * layer.ClusterTriangleCount;
                builder.Append(index + 1).Append(". ").AppendLine(layer.name);
                builder.Append("   Active / enabled: ")
                    .AppendLine(layer.isActiveAndEnabled ? "Yes" : "No");
                builder.AppendLine("   Geometry: CrossedCards (production fixed)");
                builder.Append("   Density: ")
                    .Append(layer.DensityPerSquareMetre)
                    .AppendLine(" clusters/m²");
                builder.Append("   Coverage: ")
                    .Append(layer.CoverageInitialized ? "Initialized" : "Uninitialized")
                    .Append(" / ")
                    .Append(layer.CoverageResolution)
                    .Append("² / ")
                    .Append((layer.AverageCoverage * 100f).ToString("0.0"))
                    .AppendLine("%");
                builder.Append("   Instances: ").AppendLine(layer.InstanceCount.ToString("N0"));
                builder.Append("   Submitted triangles: ")
                    .AppendLine(submittedTriangles.ToString("N0"));
                builder.Append("   Instance buffer: ")
                    .Append(layer.InstanceBufferBytes.ToString("N0"))
                    .AppendLine(" bytes");
                builder.Append("   Resources ready: ")
                    .AppendLine(layer.ResourcesReady ? "Yes" : "No");
                if (!string.IsNullOrEmpty(layer.LastBuildError))
                {
                    builder.Append("   Build error: ").AppendLine(layer.LastBuildError);
                }

                if (layer.isActiveAndEnabled)
                {
                    totalInstances += layer.InstanceCount;
                    totalTriangles += submittedTriangles;
                    totalBufferBytes += layer.InstanceBufferBytes;
                }
            }

            builder.Append("Enabled-stack instances: ")
                .AppendLine(totalInstances.ToString("N0"));
            builder.Append("Enabled-stack submitted triangles: ")
                .AppendLine(totalTriangles.ToString("N0"));
            builder.Append("Enabled-stack instance buffers: ")
                .Append(totalBufferBytes.ToString("N0"))
                .AppendLine(" bytes");
            return builder.ToString();
        }

        private void SynchronizeSurfaceGroundFromHierarchy()
        {
            surfaceGround = GetComponentInParent<GeneratedGround>(true);
        }
    }
}
