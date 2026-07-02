using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ProgrammaticStylized3D.Geometry;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProgrammaticStylized3D.Rivers
{
    public sealed partial class StylizedRiverDisturbanceRuntime
    {
        public void CopyObstacleExclusionMeshFiltersTo(
            List<MeshFilter> output)
        {
            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            output.Clear();
            if (river == null || !river.Domain.IsValid)
            {
                return;
            }

            foreach (KeyValuePair<EntityId, ContinuousSource> pair in
                     continuousSources)
            {
                ContinuousSource source = pair.Value;
                MeshFilter meshFilter = source.ObstacleExclusionMeshFilter;
                if (!source.IsStatic ||
                    meshFilter == null ||
                    meshFilter.sharedMesh == null ||
                    !meshFilter.gameObject.activeInHierarchy ||
                    output.Contains(meshFilter))
                {
                    continue;
                }

                output.Add(meshFilter);
            }

            output.Sort((left, right) =>
                left.GetEntityId().CompareTo(right.GetEntityId()));
        }

        public bool PrepareGeneratedGeometrySourcesForCacheValidation(
            out string status)
        {
            river ??= GetComponent<StylizedRiver>();
            if (river == null || !river.Domain.IsValid)
            {
                status = "A valid river domain is required.";
                return false;
            }

            if (!river.RuntimeDisturbancesEnabled)
            {
                continuousSources.Clear();
                continuousSourceIdsByOwner.Clear();
                automaticGeneratedSourceIds.Clear();
                refreshedAutomaticGeneratedSourceIds.Clear();
                generatedGeometryScratch.Clear();
                generatedGeometryRefreshInProgress = false;
                generatedGeometryRefreshIndex = 0;
                status =
                    "Runtime disturbances are disabled; the obstacle-source set is empty.";
                return true;
            }

            if (!river.isActiveAndEnabled)
            {
                status =
                    "The river must be active and enabled for exact obstacle-source validation.";
                return false;
            }

            if (!river.TryGetSurfaceBounds(out _))
            {
                status =
                    "The river surface bounds are unavailable for exact obstacle-source validation.";
                return false;
            }

            generatedGeometryRegistryDirty = true;
            generatedGeometryRefreshInProgress = false;
            generatedGeometryRefreshIndex = 0;

            int remainingPasses = Mathf.Max(
                8,
                GeneratedGeometryRegistry.Sources.Count + 8);
            do
            {
                RefreshGeneratedGeometrySources();
                remainingPasses--;
            }
            while (generatedGeometryRefreshInProgress &&
                   remainingPasses > 0);

            if (generatedGeometryRefreshInProgress)
            {
                status =
                    "Generated obstacle-source validation did not settle within its bounded pass.";
                return false;
            }

            status =
                $"Prepared {automaticGeneratedSourceIds.Count} generated obstacle source(s).";
            return true;
        }

        public bool TryCopyObstacleExclusionStableFingerprintsTo(
            List<GeneratedGeometryStableFingerprint> output,
            out int sourceCount,
            out string status)
        {
            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            output.Clear();
            sourceCount = 0;
            if (river == null || !river.Domain.IsValid)
            {
                status = "A valid river domain is required.";
                return false;
            }

            if (!GeneratedObstacleRegistryReady)
            {
                status =
                    "The generated obstacle registry is still refreshing " +
                    $"({GeneratedObstacleRegistryProcessedCount:N0} / " +
                    $"{GeneratedObstacleRegistryTotalCount:N0} sources).";
                return false;
            }

            HashSet<EntityId> seenMeshFilters = new();
            foreach (KeyValuePair<EntityId, ContinuousSource> pair in
                     continuousSources)
            {
                ContinuousSource source = pair.Value;
                MeshFilter meshFilter = source.ObstacleExclusionMeshFilter;
                if (!source.IsStatic || meshFilter == null ||
                    meshFilter.sharedMesh == null ||
                    !meshFilter.gameObject.activeInHierarchy)
                {
                    continue;
                }

                EntityId meshId = meshFilter.GetEntityId();
                if (!seenMeshFilters.Add(meshId))
                {
                    continue;
                }

                sourceCount++;
                IGeneratedGeometryStableFingerprintSource provider =
                    source.ObstacleExclusionFingerprintSource;
                GeneratedGeometryStableFingerprint fingerprint;
                string providerStatus = string.Empty;
                if (provider == null ||
                    !provider.TryGetStableWorldGeometryFingerprint(
                        out fingerprint,
                        out providerStatus))
                {
                    output.Clear();
                    status =
                        $"Static obstacle source {sourceCount - 1} does not " +
                        "provide a prepared exact world-geometry fingerprint. " +
                        (providerStatus ?? string.Empty);
                    return false;
                }

                output.Add(fingerprint);
            }

            output.Sort();
            status =
                $"Collected {output.Count:N0} prepared exact obstacle fingerprint(s) without scanning mesh triangles.";
            return true;
        }

        private void HandleGeneratedGeometrySourceAdded(
            IGeneratedGeometrySource source)
        {
            generatedGeometryRegistryDirty = true;
        }

        private void HandleGeneratedGeometrySourceRemoved(
            IGeneratedGeometrySource source)
        {
            generatedGeometryRegistryDirty = true;
        }

        private void HandleGeneratedGeometrySourceChanged(
            IGeneratedGeometrySource source)
        {
            generatedGeometryRegistryDirty = true;
        }

        private void RefreshGeneratedGeometrySources()
        {
            if (river == null ||
                !river.isActiveAndEnabled ||
                !river.RuntimeDisturbancesEnabled ||
                !river.Domain.IsValid ||
                !river.TryGetSurfaceBounds(out Bounds currentRiverBounds))
            {
                generatedGeometryRefreshInProgress = false;
                generatedGeometryRefreshIndex = 0;
                return;
            }

            if (!generatedGeometryRefreshInProgress)
            {
                refreshedAutomaticGeneratedSourceIds.Clear();
                GeneratedGeometryRegistry.CopySourcesTo(
                    generatedGeometryScratch);

                currentRiverBounds.Expand(
                    new Vector3(
                        AutomaticBoundsHorizontalPadding * 2f,
                        AutomaticBoundsVerticalPadding * 2f,
                        AutomaticBoundsHorizontalPadding * 2f));

                generatedGeometryRefreshBounds = currentRiverBounds;
                generatedGeometryRefreshIndex = 0;
                generatedGeometryRefreshInProgress = true;

                // New registry events may set this back to true while the
                // current refresh is in flight. In that case another refresh
                // begins after this budgeted pass completes.
                generatedGeometryRegistryDirty = false;
            }

            int processedThisFrame = 0;
            while (generatedGeometryRefreshIndex <
                       generatedGeometryScratch.Count &&
                   processedThisFrame < GeneratedSourcesPerFrame)
            {
                IGeneratedGeometrySource source =
                    generatedGeometryScratch[generatedGeometryRefreshIndex++];
                ProcessGeneratedGeometrySource(source);
                processedThisFrame++;
            }

            if (generatedGeometryRefreshIndex <
                generatedGeometryScratch.Count)
            {
                return;
            }

            foreach (EntityId sourceId in automaticGeneratedSourceIds)
            {
                if (!refreshedAutomaticGeneratedSourceIds.Contains(sourceId))
                {
                    RemoveContinuousSource(sourceId);
                    RemoveGeneratedDiagnostic(sourceId);
                }
            }

            automaticGeneratedSourceIds.Clear();
            automaticGeneratedSourceIds.UnionWith(
                refreshedAutomaticGeneratedSourceIds);
            generatedGeometryRefreshInProgress = false;
            generatedGeometryRefreshIndex = 0;
            obstacleGeometryVersion++;
            staticPressureTargetDirty = true;
            staticWakeSourceDirty = true;
            rippleBoundaryDirty = true;
            lastActivityTime = Time.realtimeSinceStartupAsDouble;
        }

        private ResolvedGeneratedRiverInteraction ResolveGeneratedInteraction(
            GeneratedRiverInteractionSettings settings)
        {
            settings?.Validate();

            GeneratedRiverFeatureMode pressureMode = settings != null
                ? settings.StaticPressureMode
                : GeneratedRiverFeatureMode.Inherit;
            GeneratedRiverFeatureMode wakeMode = settings != null
                ? settings.ObstructionWakeMode
                : GeneratedRiverFeatureMode.Inherit;
            GeneratedRiverRippleCollisionMode rippleCollisionMode =
                settings != null
                    ? settings.ImpactRippleCollisionMode
                    : GeneratedRiverRippleCollisionMode.Inherit;

            bool pressureEnabled =
                pressureMode != GeneratedRiverFeatureMode.Disabled;
            bool wakeEnabled =
                wakeMode != GeneratedRiverFeatureMode.Disabled;
            bool rippleCollisionEnabled =
                rippleCollisionMode !=
                GeneratedRiverRippleCollisionMode.Disabled;

            float pressureStrength =
                pressureMode == GeneratedRiverFeatureMode.Custom
                    ? settings.StaticPressureStrength
                    : river.PressureStrength;
            float contactSharpness =
                pressureMode == GeneratedRiverFeatureMode.Custom
                    ? settings.StaticPressureContactSharpness
                    : river.PressureContactSharpness;
            float profileVariation =
                pressureMode == GeneratedRiverFeatureMode.Custom
                    ? settings.StaticPressureProfileVariation
                    : river.PressureProfileVariation;
            float profileChangeIntervalMin =
                pressureMode == GeneratedRiverFeatureMode.Custom
                    ? settings.StaticPressureProfileChangeIntervalMin
                    : river.PressureProfileChangeIntervalMin;
            float profileChangeIntervalMax =
                pressureMode == GeneratedRiverFeatureMode.Custom
                    ? settings.StaticPressureProfileChangeIntervalMax
                    : river.PressureProfileChangeIntervalMax;
            float wakeStrength =
                wakeMode == GeneratedRiverFeatureMode.Custom
                    ? settings.ObstructionWakeStrength
                    : river.WakeStrength;
            float wakeReach =
                wakeMode == GeneratedRiverFeatureMode.Custom
                    ? settings.ObstructionWakeReach
                    : river.WakeReach;
            float wakeSpread =
                wakeMode == GeneratedRiverFeatureMode.Custom
                    ? settings.ObstructionWakeSpread
                    : river.WakeSpread;
            float wakeVariation =
                wakeMode == GeneratedRiverFeatureMode.Custom
                    ? settings.ObstructionWakeVariation
                    : river.WakeVariation;

            return new ResolvedGeneratedRiverInteraction(
                pressureEnabled,
                pressureStrength,
                contactSharpness,
                profileVariation,
                profileChangeIntervalMin,
                profileChangeIntervalMax,
                wakeEnabled,
                wakeStrength,
                wakeReach,
                wakeSpread,
                wakeVariation,
                rippleCollisionEnabled);
        }

        private void ProcessGeneratedGeometrySource(
            IGeneratedGeometrySource source)
        {
            if (source == null ||
                (source is UnityEngine.Object unityObject &&
                 unityObject == null) ||
                !source.IsSolidGeometry ||
                !source.IsStaticGeometry)
            {
                return;
            }

            GeneratedRiverInteractionSettings authoredSettings =
                source is IGeneratedRiverInteractionSource interactionSource
                    ? interactionSource.RiverInteractionSettings
                    : null;

            if (authoredSettings != null &&
                authoredSettings.Participation ==
                GeneratedRiverInteractionParticipation.Disabled)
            {
                return;
            }

            ResolvedGeneratedRiverInteraction interaction =
                ResolveGeneratedInteraction(authoredSettings);

            if (!interaction.StaticPressureEnabled &&
                !interaction.ObstructionWakeEnabled &&
                !interaction.ImpactRippleCollisionEnabled)
            {
                return;
            }

            MeshFilter meshFilter = source.GeometryMeshFilter;
            if (meshFilter == null ||
                meshFilter.sharedMesh == null ||
                !meshFilter.gameObject.activeInHierarchy ||
                !RiverDisturbanceFootprintResolver.TryGetWorldBounds(
                    meshFilter,
                    out Bounds sourceBounds) ||
                !generatedGeometryRefreshBounds.Intersects(sourceBounds) ||
                !river.TryProjectWorldPoint(
                    sourceBounds.center,
                    out StylizedRiverProjection boundsProjection))
            {
                return;
            }

            StylizedRiverSplineSample boundsSample =
                river.SampleAtLocalDistance(
                    boundsProjection.LocalDistance);
            float preliminaryRiverWidth = Mathf.Max(
                0.10f,
                boundsSample.LeftSurfaceHalfWidth +
                boundsSample.RightSurfaceHalfWidth);
            float effectivePadding = ResolveAutomaticFootprintPadding(
                preliminaryRiverWidth,
                DefaultGeneratedFootprintPadding);

            if (!RiverDisturbanceFootprintResolver.TryResolveBoundsOnly(
                    river,
                    meshFilter,
                    effectivePadding,
                    out RiverDisturbanceFootprint footprint,
                    out string footprintStatus) ||
                !river.TryProjectWorldPoint(
                    footprint.WorldPosition,
                    out StylizedRiverProjection footprintProjection) ||
                !footprintProjection.IsInside)
            {
                return;
            }

            StylizedRiverSplineSample sample =
                river.SampleAtLocalDistance(
                    footprintProjection.LocalDistance);
            float localRiverWidth = Mathf.Max(
                0.10f,
                sample.LeftSurfaceHalfWidth +
                sample.RightSurfaceHalfWidth);
            float unpaddedAcrossHalfWidth = Mathf.Max(
                0.05f,
                footprint.AcrossHalfWidth - effectivePadding);
            float blockageRatio = Mathf.Clamp01(
                unpaddedAcrossHalfWidth * 2f / localRiverWidth);
            float blockageInfluence = Mathf.SmoothStep(
                0f,
                1f,
                Mathf.InverseLerp(
                    0.04f,
                    0.55f,
                    blockageRatio));

            RiverDisturbanceFootprint pressureFootprint = footprint;
            RiverDisturbanceFootprint collisionFootprint = footprint;
            if ((interaction.StaticPressureEnabled ||
                 interaction.ImpactRippleCollisionEnabled) &&
                RiverDisturbanceFootprintResolver.TryResolveBoundsOnly(
                    river,
                    meshFilter,
                    0f,
                    out RiverDisturbanceFootprint rawFootprint,
                    out _))
            {
                pressureFootprint = rawFootprint;
                collisionFootprint = rawFootprint;
            }

            RiverDisturbancePressureBakeProfile pressureProfile = default;
            float waveAllowance = 0f;
            float representativeSupportHeight = 0f;
            float minimumAllowedPressureHeight = 0f;
            float maximumAllowedPressureHeight = 0f;
            float targetPressureHeight = 0f;
            float unboundedPressureMaximum = 0f;
            float supportInspectionHeight = 0f;
            bool heightClampReached = false;
            string pressureStatus = "Static pressure disabled.";

            if (!TryResolveGeneratedStaticPressure(
                    interaction,
                    pressureFootprint,
                    localRiverWidth,
                    blockageInfluence,
                    out pressureProfile,
                    out waveAllowance,
                    out representativeSupportHeight,
                    out minimumAllowedPressureHeight,
                    out maximumAllowedPressureHeight,
                    out targetPressureHeight,
                    out unboundedPressureMaximum,
                    out supportInspectionHeight,
                    out heightClampReached,
                    out pressureStatus))
            {
                return;
            }

            float wakeAmplitude = 0f;
            if (interaction.ObstructionWakeEnabled)
            {
                float wakeFlowFactor = Mathf.Lerp(
                    0.20f,
                    1.35f,
                    Mathf.InverseLerp(
                        0.05f,
                        2.5f,
                        Mathf.Abs(river.FlowSpeedMetresPerSecond)));
                wakeAmplitude = Mathf.Max(
                    0f,
                    (0.55f + blockageInfluence * 1.15f) *
                    wakeFlowFactor *
                    interaction.ObstructionWakeStrength);
            }

            EntityId sourceId = meshFilter.GetEntityId();
            EntityId ownerId = meshFilter.gameObject.GetEntityId();
            if (!RegisterStaticSource(
                    sourceId,
                    ownerId,
                    footprint.WorldPosition,
                    footprint.AcrossHalfWidth,
                    footprint.AlongHalfLength,
                    1f,
                    1f,
                    1f,
                    -1f,
                    wakeAmplitude,
                    interaction.StaticPressureContactSharpness,
                    interaction.ObstructionWakeReach,
                    interaction.StaticPressureProfileVariation,
                    footprint.Contour,
                    targetPressureHeight,
                    pressureFootprint.AcrossHalfWidth,
                    pressureFootprint.AlongHalfLength,
                    pressureFootprint.Contour,
                    pressureProfile,
                    meshFilter,
                    true,
                    interaction.ObstructionWakeSpread,
                    interaction.StaticPressureProfileChangeIntervalMin,
                    interaction.StaticPressureProfileChangeIntervalMax,
                    interaction.ObstructionWakeVariation,
                    river.WakeVariationIntervalMin,
                    river.WakeVariationIntervalMax,
                    interaction.ImpactRippleCollisionEnabled,
                    collisionFootprint.AcrossHalfWidth,
                    collisionFootprint.AlongHalfLength,
                    collisionFootprint.Contour,
                    source as IGeneratedGeometryStableFingerprintSource))
            {
                return;
            }

            refreshedAutomaticGeneratedSourceIds.Add(sourceId);
            GeneratedSourceDiagnostics[sourceId] =
                new GeneratedRiverDisturbanceDiagnostics(
                    river,
                    true,
                    footprint.AcrossHalfWidth * 2f,
                    footprint.AlongHalfLength * 2f,
                    localRiverWidth,
                    blockageRatio,
                    effectivePadding,
                    targetPressureHeight,
                    wakeAmplitude,
                    maximumAllowedPressureHeight,
                    heightClampReached,
                    representativeSupportHeight,
                    minimumAllowedPressureHeight,
                    maximumAllowedPressureHeight,
                    interaction.StaticPressureStrength,
                    waveAllowance,
                    supportInspectionHeight,
                    interaction.StaticPressureEnabled,
                    interaction.StaticPressureContactSharpness,
                    interaction.StaticPressureProfileVariation,
                    interaction.ObstructionWakeEnabled,
                    interaction.ObstructionWakeReach,
                    interaction.ObstructionWakeSpread,
                    interaction.ObstructionWakeVariation,
                    footprintStatus + " " + pressureStatus + " " +
                    $"Contour {footprint.Contour.Length} points; " +
                    $"blockage {blockageRatio:P0}; " +
                    $"pressure strength {interaction.StaticPressureStrength:P0}; " +
                    "ripple collision " +
                    (interaction.ImpactRippleCollisionEnabled
                        ? "enabled."
                        : "disabled."));
        }

        private bool TryResolveGeneratedStaticPressure(
            ResolvedGeneratedRiverInteraction interaction,
            RiverDisturbanceFootprint pressureFootprint,
            float localRiverWidth,
            float blockageInfluence,
            out RiverDisturbancePressureBakeProfile pressureProfile,
            out float waveAllowance,
            out float representativeSupportHeight,
            out float minimumAllowedPressureHeight,
            out float maximumAllowedPressureHeight,
            out float targetPressureHeight,
            out float unboundedPressureMaximum,
            out float supportInspectionHeight,
            out bool heightClampReached,
            out string pressureStatus)
        {
            pressureProfile = default;
            waveAllowance = 0f;
            representativeSupportHeight = 0f;
            minimumAllowedPressureHeight = 0f;
            maximumAllowedPressureHeight = 0f;
            targetPressureHeight = 0f;
            unboundedPressureMaximum = 0f;
            supportInspectionHeight = 0f;
            heightClampReached = false;
            pressureStatus = "Static pressure disabled.";

            if (!interaction.StaticPressureEnabled)
            {
                return true;
            }

            waveAllowance = Mathf.Clamp(
                river.MotionWaveHeight * 1.15f + 0.04f,
                0.04f,
                0.45f);
            float absoluteFlowSpeed =
                Mathf.Abs(river.FlowSpeedMetresPerSecond);
            float velocityHead =
                absoluteFlowSpeed * absoluteFlowSpeed /
                (2f * Mathf.Max(0.001f, Physics.gravity.magnitude));
            float blockageCoefficient = Mathf.Lerp(
                0.90f,
                2.60f,
                blockageInfluence);

            // Flow determines demand; local height-aware support remains the
            // hard ceiling. The stylized coefficient deliberately makes the
            // former Strong result approximately the new safe lower response
            // for ordinary gameplay-speed rivers.
            unboundedPressureMaximum =
                velocityHead * blockageCoefficient * 5.00f;

            // Focus the fixed eight support slices on the height range that
            // this river can actually request. The wave allowance and safety
            // margin preserve headroom without spending vertical resolution on
            // irrelevant upper geometry.
            supportInspectionHeight =
                Mathf.Min(
                    MaximumStaticPressureHeightMetres,
                    unboundedPressureMaximum *
                    MaximumStaticPressureModulation) +
                waveAllowance + 0.10f;
            int pressureLateralSampleCount =
                ResolveStaticPressureLateralSampleCount(
                    pressureFootprint.AcrossHalfWidth,
                    localRiverWidth);

            // Performance cap: automatic generated sources may not height-slice
            // or rescan triangles on Play startup. Use the cached footprint
            // contour as the pressure support source.
            if (!RiverDisturbanceFootprintResolver
                .TryResolvePressureSupportFromFootprint(
                    pressureFootprint,
                    supportInspectionHeight,
                    pressureLateralSampleCount,
                    out RiverDisturbancePressureSupportProfile pressureSupport,
                    out pressureStatus))
            {
                return false;
            }

            representativeSupportHeight =
                pressureSupport.RepresentativeHeight;
            float supportBudget = Mathf.Max(
                0f,
                representativeSupportHeight - waveAllowance);
            float supportCeiling = Mathf.Min(
                supportBudget / MaximumStaticPressureModulation,
                MaximumStaticPressureHeightMetres /
                MaximumStaticPressureModulation);
            maximumAllowedPressureHeight = Mathf.Min(
                supportCeiling,
                unboundedPressureMaximum);
            minimumAllowedPressureHeight = Mathf.Min(
                maximumAllowedPressureHeight,
                maximumAllowedPressureHeight * 0.35f +
                Mathf.Min(0.050f, supportCeiling * 0.10f));
            targetPressureHeight = Mathf.Lerp(
                minimumAllowedPressureHeight,
                maximumAllowedPressureHeight,
                interaction.StaticPressureStrength);
            heightClampReached =
                unboundedPressureMaximum > supportCeiling + 0.0001f;

            return targetPressureHeight <= 0.0001f ||
                   RiverDisturbanceFootprintResolver.TryBuildPressureBakeProfile(
                       pressureSupport,
                       targetPressureHeight,
                       MaximumStaticPressureModulation,
                       out pressureProfile);
        }

        private int ResolveStaticPressureLateralSampleCount(
            float pressureAcrossHalfWidth,
            float localRiverWidth)
        {
            int localFieldHeight = river.Quality switch
            {
                StylizedRiverQuality.Low => 32,
                StylizedRiverQuality.Medium => 48,
                StylizedRiverQuality.High => 64,
                _ => 48
            };
            float profilePixelWidth =
                Mathf.Max(0.10f, pressureAcrossHalfWidth * 2f) /
                Mathf.Max(0.10f, localRiverWidth) *
                localFieldHeight;
            return RiverDisturbanceFootprintResolver.
                ResolvePressureSupportLateralSampleCount(
                    Mathf.CeilToInt(profilePixelWidth));
        }

        private int ResolveStaticWakeVariationLateralSampleCount(
            float wakeAcrossHalfWidth,
            float localRiverWidth)
        {
            int localFieldHeight = wakeFieldHeight > 0
                ? wakeFieldHeight
                : river.Quality switch
                {
                    StylizedRiverQuality.Low => 32,
                    StylizedRiverQuality.Medium => 48,
                    StylizedRiverQuality.High => 64,
                    _ => 48
                };
            float profilePixelWidth =
                Mathf.Max(0.10f, wakeAcrossHalfWidth * 2f) /
                Mathf.Max(0.10f, localRiverWidth) *
                localFieldHeight;
            return RiverDisturbanceFootprintResolver.
                ResolvePressureSupportLateralSampleCount(
                    Mathf.CeilToInt(profilePixelWidth));
        }

        private float ResolveAutomaticFootprintPadding(
            float localRiverWidth,
            float authoredPadding)
        {
            int localResolutionPerChunk = river.Quality switch
            {
                StylizedRiverQuality.Low => 64,
                StylizedRiverQuality.Medium => 96,
                StylizedRiverQuality.High => 128,
                _ => 96
            };
            int localFieldHeight = river.Quality switch
            {
                StylizedRiverQuality.Low => 32,
                StylizedRiverQuality.Medium => 48,
                StylizedRiverQuality.High => 64,
                _ => 48
            };
            float longitudinalFieldCell =
                ChunkLengthMetres / localResolutionPerChunk;
            float lateralFieldCell =
                localRiverWidth / Mathf.Max(1, localFieldHeight);
            float surfaceSpacing = Mathf.Max(
                0.05f,
                river.ResolvedSurfaceLongitudinalSpacing);
            float resolutionMinimum = Mathf.Max(
                0.12f,
                longitudinalFieldCell * 0.70f,
                lateralFieldCell * 0.65f,
                surfaceSpacing * 0.55f);
            return Mathf.Max(
                Mathf.Max(0f, authoredPadding),
                resolutionMinimum);
        }

        private void RemoveGeneratedDiagnostic(EntityId sourceId)
        {
            if (GeneratedSourceDiagnostics.TryGetValue(
                    sourceId,
                    out GeneratedRiverDisturbanceDiagnostics diagnostics) &&
                diagnostics.River == river)
            {
                GeneratedSourceDiagnostics.Remove(sourceId);
            }
        }

        private void RemoveOwnedGeneratedDiagnostics()
        {
            foreach (EntityId sourceId in automaticGeneratedSourceIds)
            {
                RemoveGeneratedDiagnostic(sourceId);
            }
        }
    }
}
