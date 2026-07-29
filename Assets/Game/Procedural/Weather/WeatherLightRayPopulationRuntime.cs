using System;
using System.Text;
using UnityEngine;

namespace ProgrammaticStylized3D.Weather
{
    public enum WeatherLightRayPopulationCandidateState
    {
        Pending = 0,
        Active = 1,
        Retiring = 2,
        Cooldown = 3
    }

    public readonly struct WeatherLightRayPopulationDebugRecord
    {
        public readonly long StableIdentity;
        public readonly Vector3 PositionWorld;
        public readonly WeatherLightRayPopulationCandidateState State;
        public readonly float Clearance;
        public readonly WeatherLightRayHandle Handle;

        public WeatherLightRayPopulationDebugRecord(
            long stableIdentity,
            Vector3 positionWorld,
            WeatherLightRayPopulationCandidateState state,
            float clearance,
            WeatherLightRayHandle handle)
        {
            StableIdentity = stableIdentity;
            PositionWorld = positionWorld;
            State = state;
            Clearance = Mathf.Clamp01(clearance);
            Handle = handle;
        }
    }

    /// <summary>
    /// Bounded deterministic automatic-population runtime for one resolved
    /// population rule. It owns candidate state only. Source, preset, cloud,
    /// and budget authority are resolved by the Controller before Tick. Never
    /// inspect WeatherLightRayPreset.SourceKind here, never evict authored or
    /// gameplay rays, and never add a per-frame cookie scan or GPU readback.
    /// </summary>
    internal sealed class WeatherLightRayPopulationRuntime
    {
        /// <summary>
        /// WEATHER LIGHTRAY POPULATION SETTINGS CONTRACT.
        ///
        /// Settings are fully resolved by the Controller/Selection layer.
        /// This runtime must not inspect visual-preset SourceKind metadata or
        /// invent source/cloud policy. Rules that ignore clouds execute zero
        /// cloud queries. Optional clouds treat only an absent or disabled
        /// producer as clear sky; an enabled but invalid producer suspends.
        /// </summary>
        internal readonly struct Settings
        {
            internal readonly string Label;
            internal readonly bool Enabled;
            internal readonly bool GlobalLightRaysEnabled;
            internal readonly int Seed;
            internal readonly ulong IdentitySalt;
            internal readonly Transform FocusOverride;
            internal readonly Camera RenderCamera;
            internal readonly LayerMask GroundMask;
            internal readonly int DesiredCount;
            internal readonly int MaximumCount;
            internal readonly float MinimumSpacingMetres;
            internal readonly float OffscreenMarginMetres;
            internal readonly float FallbackActiveRadiusMetres;
            internal readonly float EvaluationRateHz;
            internal readonly int CandidateChecksPerTick;
            internal readonly float MinimumClearance;
            internal readonly float MinimumDistinctOpeningContrast;
            internal readonly float SurroundingSampleRadiusMetres;
            internal readonly float QualificationDurationSeconds;
            internal readonly float InvalidGraceDurationSeconds;
            internal readonly float MinimumViableOpeningDurationSeconds;
            internal readonly float MaximumGroundSlopeDegrees;
            internal readonly float GroundSearchDistanceMetres;
            internal readonly float CloudEvolutionResumeThreshold;
            internal readonly WeatherLightRayPreset ActivePreset;
            internal readonly WeatherLightRaySourceKind SourceKind;
            internal readonly Vector3 RayDirectionWorld;
            internal readonly WeatherLightRaySourceGatePolicy SourceGatePolicy;
            internal readonly bool DependencyAvailable;
            internal readonly string DependencyFailureReason;
            internal readonly Light CloudProjectionLight;
            internal readonly WeatherLightRayCloudDataRequirement
                CloudDataRequirement;
            internal readonly WeatherLightRaySpatialCloudPolicy
                SpatialCloudPolicy;
            internal readonly WeatherCloudShadowController CloudController;

            internal Settings(
                string label,
                bool enabled,
                bool globalLightRaysEnabled,
                int seed,
                ulong identitySalt,
                Transform focusOverride,
                Camera renderCamera,
                LayerMask groundMask,
                int desiredCount,
                int maximumCount,
                float minimumSpacingMetres,
                float offscreenMarginMetres,
                float fallbackActiveRadiusMetres,
                float evaluationRateHz,
                int candidateChecksPerTick,
                float minimumClearance,
                float minimumDistinctOpeningContrast,
                float surroundingSampleRadiusMetres,
                float qualificationDurationSeconds,
                float invalidGraceDurationSeconds,
                float minimumViableOpeningDurationSeconds,
                float maximumGroundSlopeDegrees,
                float groundSearchDistanceMetres,
                float cloudEvolutionResumeThreshold,
                WeatherLightRayPreset activePreset,
                WeatherLightRaySourceKind sourceKind,
                Vector3 rayDirectionWorld,
                WeatherLightRaySourceGatePolicy sourceGatePolicy,
                bool dependencyAvailable,
                string dependencyFailureReason,
                Light cloudProjectionLight,
                WeatherLightRayCloudDataRequirement cloudDataRequirement,
                WeatherLightRaySpatialCloudPolicy spatialCloudPolicy,
                WeatherCloudShadowController cloudController)
            {
                Label = string.IsNullOrWhiteSpace(label)
                    ? "Automatic Population"
                    : label;
                Enabled = enabled;
                GlobalLightRaysEnabled = globalLightRaysEnabled;
                Seed = seed;
                IdentitySalt = identitySalt;
                FocusOverride = focusOverride;
                RenderCamera = renderCamera;
                GroundMask = groundMask;
                DesiredCount = desiredCount;
                MaximumCount = maximumCount;
                MinimumSpacingMetres = minimumSpacingMetres;
                OffscreenMarginMetres = offscreenMarginMetres;
                FallbackActiveRadiusMetres = fallbackActiveRadiusMetres;
                EvaluationRateHz = evaluationRateHz;
                CandidateChecksPerTick = candidateChecksPerTick;
                MinimumClearance = minimumClearance;
                MinimumDistinctOpeningContrast =
                    minimumDistinctOpeningContrast;
                SurroundingSampleRadiusMetres = surroundingSampleRadiusMetres;
                QualificationDurationSeconds = qualificationDurationSeconds;
                InvalidGraceDurationSeconds = invalidGraceDurationSeconds;
                MinimumViableOpeningDurationSeconds =
                    minimumViableOpeningDurationSeconds;
                MaximumGroundSlopeDegrees = maximumGroundSlopeDegrees;
                GroundSearchDistanceMetres = groundSearchDistanceMetres;
                CloudEvolutionResumeThreshold = cloudEvolutionResumeThreshold;
                ActivePreset = activePreset;
                SourceKind = sourceKind;
                RayDirectionWorld = rayDirectionWorld.sqrMagnitude > 0.000001f
                    ? rayDirectionWorld.normalized
                    : Vector3.down;
                SourceGatePolicy = sourceGatePolicy;
                DependencyAvailable = dependencyAvailable;
                DependencyFailureReason = dependencyFailureReason ?? string.Empty;
                CloudProjectionLight = cloudProjectionLight;
                CloudDataRequirement = cloudDataRequirement;
                SpatialCloudPolicy = spatialCloudPolicy;
                CloudController = cloudController;
            }
        }

        private enum RejectionReason
        {
            OutsideActiveRegion = 0,
            DesiredCountMet = 1,
            MaximumCountReached = 2,
            NoFreeSlot = 3,
            TooClose = 4,
            NoGroundHit = 5,
            GroundTooSteep = 6,
            CloudEvolutionUnstable = 7,
            CloudUnavailable = 8,
            InsufficientClearance = 9,
            QualificationPending = 10,
            CooldownActive = 11,
            SourceUnavailable = 12,
            CandidateStorageFull = 13,
            SpawnOrUpdateFailed = 14,
            Count = 15
        }

        private struct Candidate
        {
            internal bool Occupied;
            internal long Identity;
            internal int CellX;
            internal int CellZ;
            internal Vector3 GroundPosition;
            internal WeatherLightRayPopulationCandidateState State;
            internal WeatherLightRayHandle Handle;
            internal float Clearance;
            internal double ValidSince;
            internal double InvalidSince;
            internal double CooldownUntil;
            internal int ConsecutiveValidEvaluations;
            internal uint CloudDataVersion;
        }

        private const int MinimumCandidateCapacity = 64;
        private const int CandidateCapacityMultiplier = 8;
        private const int MinimumQualificationEvaluations = 2;
        private const float AutomaticFadeInSeconds = 0.75f;
        private const float AutomaticFadeOutSeconds = 0.75f;
        private const float ReleaseIntensityThreshold = 0.001f;
        private const float CandidateCooldownSeconds = 1f;
        private const float RegionChangeEpsilonSquared = 0.25f;
        private const int FootprintSampleCount = 13;
        private const int ForecastSampleCount = 4;

        private Candidate[] candidates;
        private int[] rejectionCounts;
        private long[] totalRejectionCounts;
        private int pendingCursor;
        private int cellCursorX;
        private int cellCursorZ;
        private int minimumCellX;
        private int maximumCellX;
        private int minimumCellZ;
        private int maximumCellZ;
        private bool cellCursorInitialized;
        private double nextEvaluationTime;
        private Vector3 focusWorld;
        private float activeRadiusMetres;
        private Vector3 previousFocusWorld;
        private float previousActiveRadiusMetres;
        private string suspensionReason = "Automatic population is disabled.";
        private int activeCount;
        private int pendingCount;
        private int retiringCount;
        private int cooldownCount;
        private int candidateChecksLastTick;
        private int groundRaycastsLastTick;
        private int cloudSamplesLastTick;
        private int cellsInActiveRegion;
        private int currentCloudSeed;
        private bool evolutionInProgress;
        private float evolutionProgress;
        private long totalEvaluationTicks;

        internal bool IsEnabledAndRunning =>
            string.IsNullOrEmpty(suspensionReason);
        internal string SuspensionReason => suspensionReason;
        internal Vector3 FocusWorld => focusWorld;
        internal float ActiveRadiusMetres => activeRadiusMetres;
        internal int ActiveCount => activeCount;
        internal int PendingCount => pendingCount;
        internal int RetiringCount => retiringCount;
        internal int CooldownCount => cooldownCount;
        internal int CandidateChecksLastTick => candidateChecksLastTick;
        internal int GroundRaycastsLastTick => groundRaycastsLastTick;
        internal int CloudSamplesLastTick => cloudSamplesLastTick;
        internal int CellsInActiveRegion => cellsInActiveRegion;
        internal int CurrentCloudSeed => currentCloudSeed;
        internal bool EvolutionInProgress => evolutionInProgress;
        internal float EvolutionProgress => evolutionProgress;
        internal long TotalEvaluationTicks => totalEvaluationTicks;

        /// <summary>
        /// Executes one bounded population-rule evaluation. All policy in the
        /// Settings value is already resolved by the Controller. This function
        /// may qualify, update, retire, or release only candidates owned by this
        /// runtime; it must not inspect selection assets, visual-preset source
        /// metadata, or evict authored/gameplay rays.
        /// </summary>
        internal void Tick(
            WeatherLightRayController controller,
            in Settings settings,
            double now)
        {
            EnsureStorage(settings.MaximumCount);

            double interval = 1.0 / Math.Max(1.0, settings.EvaluationRateHz);
            if (now < nextEvaluationTime)
            {
                return;
            }

            nextEvaluationTime = now + interval;
            UpdateCounts(controller, now);
            totalEvaluationTicks++;
            candidateChecksLastTick = 0;
            groundRaycastsLastTick = 0;
            cloudSamplesLastTick = 0;
            Array.Clear(rejectionCounts, 0, rejectionCounts.Length);

            currentCloudSeed = settings.CloudController != null
                ? settings.CloudController.CurrentCookieSeed
                : 0;
            evolutionInProgress = settings.CloudController != null &&
                settings.CloudController.EvolutionInProgress;
            evolutionProgress = settings.CloudController != null
                ? settings.CloudController.EvolutionProgress
                : 0f;

            suspensionReason = ResolveSuspensionReason(settings);
            if (!string.IsNullOrEmpty(suspensionReason))
            {
                RetireAllAutomaticCandidates(controller, now);
                ProcessRetiringAndCooldown(controller, now);
                UpdateCounts(controller, now);
                return;
            }

            if (!TryResolveFocus(settings, out focusWorld, out activeRadiusMetres))
            {
                suspensionReason =
                    "No valid population focus could be projected onto the configured Ground Mask.";
                RetireAllAutomaticCandidates(controller, now);
                ProcessRetiringAndCooldown(controller, now);
                UpdateCounts(controller, now);
                return;
            }

            UpdateCellBounds(settings.MinimumSpacingMetres);
            ProcessRetiringAndCooldown(controller, now);
            RetireOutsideBudget(controller, settings, now);
            RevalidateActiveCandidates(controller, settings, now);

            int remainingChecks = settings.CandidateChecksPerTick;
            remainingChecks -= EvaluatePendingCandidates(
                controller,
                settings,
                now,
                remainingChecks);
            if (remainingChecks > 0)
            {
                EvaluateNewCandidates(
                    controller,
                    settings,
                    now,
                    remainingChecks);
            }

            UpdateCounts(controller, now);
        }

        internal void Shutdown(
            WeatherLightRayController controller,
            bool releaseImmediately)
        {
            if (candidates == null)
            {
                return;
            }

            double now = Time.realtimeSinceStartupAsDouble;
            for (int index = 0; index < candidates.Length; index++)
            {
                Candidate candidate = candidates[index];
                if (!candidate.Occupied)
                {
                    continue;
                }

                if (releaseImmediately)
                {
                    if (candidate.Handle.IsValid &&
                        controller.IsValid(candidate.Handle))
                    {
                        controller.TryReleaseProceduralRay(
                            candidate.Handle,
                            out _);
                    }
                    candidate = default;
                }
                else if (candidate.State ==
                    WeatherLightRayPopulationCandidateState.Active)
                {
                    BeginRetirement(controller, ref candidate, now);
                }
                else if (candidate.State ==
                    WeatherLightRayPopulationCandidateState.Pending)
                {
                    EnterCooldown(ref candidate, now);
                }

                candidates[index] = candidate;
            }

            if (releaseImmediately)
            {
                activeCount = 0;
                pendingCount = 0;
                retiringCount = 0;
                cooldownCount = 0;
                cellCursorInitialized = false;
                pendingCursor = 0;
            }
            else
            {
                UpdateCounts(controller, now);
            }

            suspensionReason = "Automatic population is disabled.";
        }

        internal int CopyDebugRecords(
            WeatherLightRayPopulationDebugRecord[] destination,
            int destinationOffset = 0)
        {
            int count = 0;
            if (candidates == null)
            {
                return count;
            }

            for (int index = 0; index < candidates.Length; index++)
            {
                Candidate candidate = candidates[index];
                if (!candidate.Occupied)
                {
                    continue;
                }

                int destinationIndex = destinationOffset + count;
                if (destination != null &&
                    destinationIndex >= 0 &&
                    destinationIndex < destination.Length)
                {
                    destination[destinationIndex] =
                        new WeatherLightRayPopulationDebugRecord(
                            candidate.Identity,
                            candidate.GroundPosition,
                            candidate.State,
                            candidate.Clearance,
                            candidate.Handle);
                }

                count++;
            }

            return count;
        }

        internal void AppendReport(
            StringBuilder builder,
            in Settings settings,
            int freeSlotCount)
        {
            builder.Append('[')
                .Append(settings.Label)
                .AppendLine("]");
            builder.Append("Enabled / running: ")
                .Append(settings.Enabled ? "Yes" : "No")
                .Append(" / ")
                .AppendLine(IsEnabledAndRunning ? "Yes" : "No");
            builder.Append("Suspension reason: ")
                .AppendLine(string.IsNullOrEmpty(suspensionReason)
                    ? "None"
                    : suspensionReason);
            builder.Append("Focus / active radius: ")
                .Append(focusWorld.ToString("F3"))
                .Append(" / ")
                .Append(activeRadiusMetres.ToString("0.###"))
                .AppendLine(" m");
            builder.Append("Seed / desired / maximum / free slots: ")
                .Append(settings.Seed)
                .Append(" / ")
                .Append(settings.DesiredCount)
                .Append(" / ")
                .Append(settings.MaximumCount)
                .Append(" / ")
                .AppendLine(freeSlotCount.ToString());
            builder.Append("Active / pending / retiring / cooldown: ")
                .Append(activeCount)
                .Append(" / ")
                .Append(pendingCount)
                .Append(" / ")
                .Append(retiringCount)
                .Append(" / ")
                .AppendLine(cooldownCount.ToString());
            builder.Append("Cloud seed / evolution / progress: ")
                .Append(currentCloudSeed)
                .Append(" / ")
                .Append(evolutionInProgress ? "Active" : "Idle")
                .Append(" / ")
                .AppendLine(evolutionProgress.ToString("P0"));
            builder.Append("Evaluation Hz / candidate budget / cells: ")
                .Append(settings.EvaluationRateHz.ToString("0.###"))
                .Append(" / ")
                .Append(settings.CandidateChecksPerTick)
                .Append(" / ")
                .AppendLine(cellsInActiveRegion.ToString());
            builder.Append("Last tick candidate checks / ground raycasts / cloud samples: ")
                .Append(candidateChecksLastTick)
                .Append(" / ")
                .Append(groundRaycastsLastTick)
                .Append(" / ")
                .AppendLine(cloudSamplesLastTick.ToString());
            builder.Append("Total evaluation ticks: ")
                .AppendLine(totalEvaluationTicks.ToString());

            AppendRejectionReport(builder);
            if (candidates == null)
            {
                return;
            }

            for (int index = 0; index < candidates.Length; index++)
            {
                Candidate candidate = candidates[index];
                if (!candidate.Occupied)
                {
                    continue;
                }

                builder.Append("Candidate ")
                    .Append(candidate.Identity)
                    .Append(" | cell ")
                    .Append(candidate.CellX)
                    .Append(',')
                    .Append(candidate.CellZ)
                    .Append(" | ")
                    .Append(candidate.State)
                    .Append(" | ")
                    .Append(candidate.Handle)
                    .Append(" | clearance ")
                    .Append(candidate.Clearance.ToString("0.###"))
                    .Append(" | WS ")
                    .AppendLine(candidate.GroundPosition.ToString("F3"));
            }
        }

        private void EnsureStorage(int maximumCount)
        {
            int requiredCapacity = Mathf.Max(
                MinimumCandidateCapacity,
                Mathf.Max(1, maximumCount) * CandidateCapacityMultiplier);
            if (candidates == null || candidates.Length < requiredCapacity)
            {
                Candidate[] replacement = new Candidate[requiredCapacity];
                if (candidates != null)
                {
                    Array.Copy(candidates, replacement, candidates.Length);
                }

                candidates = replacement;
                pendingCursor = 0;
            }

            if (rejectionCounts == null ||
                rejectionCounts.Length != (int)RejectionReason.Count)
            {
                rejectionCounts = new int[(int)RejectionReason.Count];
            }

            if (totalRejectionCounts == null ||
                totalRejectionCounts.Length != (int)RejectionReason.Count)
            {
                totalRejectionCounts = new long[(int)RejectionReason.Count];
            }
        }

        private static string ResolveSuspensionReason(in Settings settings)
        {
            if (!settings.Enabled)
            {
                return "Automatic population is disabled.";
            }

            if (!settings.GlobalLightRaysEnabled)
            {
                return "Weather LightRays are globally disabled.";
            }

            if (settings.ActivePreset == null)
            {
                return "Automatic population requires a resolved visual preset.";
            }

            if (!settings.DependencyAvailable)
            {
                return string.IsNullOrEmpty(settings.DependencyFailureReason)
                    ? "The selected LightRay dependencies are unavailable."
                    : settings.DependencyFailureReason;
            }

            if (settings.GroundMask.value == 0)
            {
                return "Automatic population requires a non-empty Ground Mask.";
            }

            if (settings.FocusOverride == null &&
                settings.RenderCamera == null)
            {
                return "No population focus override or resolved render camera is available.";
            }

            if (settings.CloudDataRequirement ==
                    WeatherLightRayCloudDataRequirement.Ignored)
            {
                return settings.SpatialCloudPolicy ==
                        WeatherLightRaySpatialCloudPolicy.AnyPosition
                    ? string.Empty
                    : "A cloud-ignored rule may use only Any Position.";
            }

            WeatherCloudShadowController cloud = settings.CloudController;
            bool producerAbsentOrDisabled = cloud == null ||
                !cloud.IsPublished ||
                !cloud.CloudShadowsEnabled;
            if (producerAbsentOrDisabled)
            {
                return settings.CloudDataRequirement ==
                        WeatherLightRayCloudDataRequirement.Optional
                    ? string.Empty
                    : "The population rule requires an enabled published cloud field.";
            }

            if (!cloud.CookieReady)
            {
                return "The enabled published cloud field is not ready.";
            }

            if (settings.SpatialCloudPolicy !=
                    WeatherLightRaySpatialCloudPolicy.AnyPosition &&
                settings.CloudProjectionLight == null)
            {
                return "The spatially cloud-qualified population rule has no valid projection source.";
            }

            if (cloud.EvolutionInProgress &&
                cloud.EvolutionProgress <
                    settings.CloudEvolutionResumeThreshold)
            {
                return "Cloud seed evolution is below the LightRay resume threshold.";
            }

            return string.Empty;
        }

        private bool TryResolveFocus(
            in Settings settings,
            out Vector3 resolvedFocus,
            out float resolvedRadius)
        {
            if (settings.FocusOverride != null)
            {
                resolvedFocus = settings.FocusOverride.position;
                resolvedRadius = settings.FallbackActiveRadiusMetres +
                    settings.OffscreenMarginMetres;
                return true;
            }

            Camera camera = settings.RenderCamera;
            if (camera == null)
            {
                resolvedFocus = Vector3.zero;
                resolvedRadius = 0f;
                return false;
            }

            if (!TryRaycastViewportGround(
                    camera,
                    0.5f,
                    0.5f,
                    settings,
                    out RaycastHit centreHit))
            {
                resolvedFocus = Vector3.zero;
                resolvedRadius = 0f;
                return false;
            }

            resolvedFocus = centreHit.point;
            float maximumDistance = 0f;
            int cornerHitCount = 0;
            maximumDistance = ResolveCornerDistance(
                camera,
                0f,
                0f,
                settings,
                resolvedFocus,
                maximumDistance,
                ref cornerHitCount);
            maximumDistance = ResolveCornerDistance(
                camera,
                1f,
                0f,
                settings,
                resolvedFocus,
                maximumDistance,
                ref cornerHitCount);
            maximumDistance = ResolveCornerDistance(
                camera,
                0f,
                1f,
                settings,
                resolvedFocus,
                maximumDistance,
                ref cornerHitCount);
            maximumDistance = ResolveCornerDistance(
                camera,
                1f,
                1f,
                settings,
                resolvedFocus,
                maximumDistance,
                ref cornerHitCount);
            resolvedRadius = cornerHitCount > 0
                ? maximumDistance + settings.OffscreenMarginMetres
                : settings.FallbackActiveRadiusMetres +
                    settings.OffscreenMarginMetres;
            return true;
        }

        private float ResolveCornerDistance(
            Camera camera,
            float viewportX,
            float viewportY,
            in Settings settings,
            Vector3 resolvedFocus,
            float currentMaximum,
            ref int hitCount)
        {
            if (!TryRaycastViewportGround(
                    camera,
                    viewportX,
                    viewportY,
                    settings,
                    out RaycastHit hit))
            {
                return currentMaximum;
            }

            hitCount++;
            Vector2 delta = new Vector2(
                hit.point.x - resolvedFocus.x,
                hit.point.z - resolvedFocus.z);
            return Mathf.Max(currentMaximum, delta.magnitude);
        }

        private bool TryRaycastViewportGround(
            Camera camera,
            float viewportX,
            float viewportY,
            in Settings settings,
            out RaycastHit hit)
        {
            groundRaycastsLastTick++;
            Ray ray = camera.ViewportPointToRay(
                new Vector3(viewportX, viewportY, 0f));
            return Physics.Raycast(
                ray,
                out hit,
                settings.GroundSearchDistanceMetres,
                settings.GroundMask,
                QueryTriggerInteraction.Ignore);
        }

        private void UpdateCellBounds(float minimumSpacingMetres)
        {
            float cellSize = Mathf.Max(0.5f, minimumSpacingMetres);
            int newMinimumX = Mathf.FloorToInt(
                (focusWorld.x - activeRadiusMetres) / cellSize);
            int newMaximumX = Mathf.FloorToInt(
                (focusWorld.x + activeRadiusMetres) / cellSize);
            int newMinimumZ = Mathf.FloorToInt(
                (focusWorld.z - activeRadiusMetres) / cellSize);
            int newMaximumZ = Mathf.FloorToInt(
                (focusWorld.z + activeRadiusMetres) / cellSize);
            cellsInActiveRegion = Mathf.Max(
                0,
                newMaximumX - newMinimumX + 1) *
                Mathf.Max(0, newMaximumZ - newMinimumZ + 1);

            bool regionChanged = !cellCursorInitialized ||
                (focusWorld - previousFocusWorld).sqrMagnitude >
                    RegionChangeEpsilonSquared ||
                Mathf.Abs(activeRadiusMetres - previousActiveRadiusMetres) >
                    0.5f ||
                newMinimumX != minimumCellX ||
                newMaximumX != maximumCellX ||
                newMinimumZ != minimumCellZ ||
                newMaximumZ != maximumCellZ;

            minimumCellX = newMinimumX;
            maximumCellX = newMaximumX;
            minimumCellZ = newMinimumZ;
            maximumCellZ = newMaximumZ;
            previousFocusWorld = focusWorld;
            previousActiveRadiusMetres = activeRadiusMetres;
            if (!regionChanged)
            {
                return;
            }

            if (!cellCursorInitialized ||
                cellCursorX < minimumCellX ||
                cellCursorX > maximumCellX ||
                cellCursorZ < minimumCellZ ||
                cellCursorZ > maximumCellZ)
            {
                cellCursorX = minimumCellX;
                cellCursorZ = minimumCellZ;
            }

            cellCursorInitialized = true;
        }

        private void ProcessRetiringAndCooldown(
            WeatherLightRayController controller,
            double now)
        {
            for (int index = 0; index < candidates.Length; index++)
            {
                Candidate candidate = candidates[index];
                if (!candidate.Occupied)
                {
                    continue;
                }

                if (candidate.State ==
                    WeatherLightRayPopulationCandidateState.Retiring)
                {
                    if (!candidate.Handle.IsValid ||
                        !controller.IsValid(candidate.Handle))
                    {
                        EnterCooldown(ref candidate, now);
                    }
                    else if (controller.TryGetSnapshot(
                            candidate.Handle,
                            out WeatherLightRaySnapshot snapshot) &&
                        snapshot.CurrentIntensity <=
                            ReleaseIntensityThreshold)
                    {
                        controller.TryReleaseProceduralRay(
                            candidate.Handle,
                            out _);
                        EnterCooldown(ref candidate, now);
                    }
                }
                else if (candidate.State ==
                        WeatherLightRayPopulationCandidateState.Cooldown &&
                    now >= candidate.CooldownUntil)
                {
                    candidate = default;
                }

                candidates[index] = candidate;
            }
        }

        private void RetireOutsideBudget(
            WeatherLightRayController controller,
            in Settings settings,
            double now)
        {
            int permitted = Mathf.Min(
                settings.DesiredCount,
                settings.MaximumCount);
            int currentActive = CountState(
                WeatherLightRayPopulationCandidateState.Active);
            while (currentActive > permitted)
            {
                int farthestIndex = -1;
                float farthestDistanceSquared = -1f;
                for (int index = 0; index < candidates.Length; index++)
                {
                    Candidate candidate = candidates[index];
                    if (!candidate.Occupied ||
                        candidate.State !=
                            WeatherLightRayPopulationCandidateState.Active)
                    {
                        continue;
                    }

                    Vector2 delta = new Vector2(
                        candidate.GroundPosition.x - focusWorld.x,
                        candidate.GroundPosition.z - focusWorld.z);
                    if (delta.sqrMagnitude > farthestDistanceSquared)
                    {
                        farthestDistanceSquared = delta.sqrMagnitude;
                        farthestIndex = index;
                    }
                }

                if (farthestIndex < 0)
                {
                    break;
                }

                Candidate candidateToRetire = candidates[farthestIndex];
                BeginRetirement(controller, ref candidateToRetire, now);
                candidates[farthestIndex] = candidateToRetire;
                currentActive--;
            }
        }

        private void RevalidateActiveCandidates(
            WeatherLightRayController controller,
            in Settings settings,
            double now)
        {
            for (int index = 0; index < candidates.Length; index++)
            {
                Candidate candidate = candidates[index];
                if (!candidate.Occupied ||
                    candidate.State !=
                        WeatherLightRayPopulationCandidateState.Active)
                {
                    continue;
                }

                bool insideRegion = IsInsideActiveRegion(
                    candidate.GroundPosition);
                float clearance = 0.0f;
                uint dataVersion = 0u;
                bool valid = insideRegion &&
                    TryEvaluateCloudFootprint(
                        settings,
                        candidate.GroundPosition,
                        out clearance,
                        out dataVersion);
                if (valid)
                {
                    candidate.Clearance = clearance;
                    candidate.CloudDataVersion = dataVersion;
                    candidate.InvalidSince = 0.0;
                    UpdateActiveOpening(
                        controller,
                        settings,
                        ref candidate);
                }
                else
                {
                    if (!insideRegion)
                    {
                        Reject(RejectionReason.OutsideActiveRegion);
                    }

                    if (candidate.InvalidSince <= 0.0)
                    {
                        candidate.InvalidSince = now;
                    }
                    else if (now - candidate.InvalidSince >=
                        settings.InvalidGraceDurationSeconds)
                    {
                        BeginRetirement(controller, ref candidate, now);
                    }
                }

                candidates[index] = candidate;
            }
        }

        private int EvaluatePendingCandidates(
            WeatherLightRayController controller,
            in Settings settings,
            double now,
            int budget)
        {
            if (budget <= 0)
            {
                return 0;
            }

            int consumed = 0;
            int visited = 0;
            while (visited < candidates.Length && consumed < budget)
            {
                if (pendingCursor >= candidates.Length)
                {
                    pendingCursor = 0;
                }

                int index = pendingCursor++;
                visited++;
                Candidate candidate = candidates[index];
                if (!candidate.Occupied ||
                    candidate.State !=
                        WeatherLightRayPopulationCandidateState.Pending)
                {
                    continue;
                }

                consumed++;
                candidateChecksLastTick++;
                if (!IsInsideActiveRegion(candidate.GroundPosition))
                {
                    Reject(RejectionReason.OutsideActiveRegion);
                    EnterCooldown(ref candidate, now);
                    candidates[index] = candidate;
                    continue;
                }

                if (!TryEvaluateCloudFootprint(
                        settings,
                        candidate.GroundPosition,
                        out float clearance,
                        out uint dataVersion))
                {
                    candidate.ValidSince = 0.0;
                    candidate.ConsecutiveValidEvaluations = 0;
                    EnterCooldown(ref candidate, now);
                    candidates[index] = candidate;
                    continue;
                }

                candidate.Clearance = clearance;
                candidate.CloudDataVersion = dataVersion;
                candidate.ConsecutiveValidEvaluations++;
                if (candidate.ValidSince <= 0.0)
                {
                    candidate.ValidSince = now;
                }

                bool durationPassed = now - candidate.ValidSince >=
                    settings.QualificationDurationSeconds;
                if (!durationPassed ||
                    candidate.ConsecutiveValidEvaluations <
                        MinimumQualificationEvaluations)
                {
                    Reject(RejectionReason.QualificationPending);
                    candidates[index] = candidate;
                    continue;
                }

                if (CountState(
                        WeatherLightRayPopulationCandidateState.Active) >=
                    Mathf.Min(
                        settings.DesiredCount,
                        settings.MaximumCount))
                {
                    Reject(RejectionReason.DesiredCountMet);
                    EnterCooldown(ref candidate, now);
                    candidates[index] = candidate;
                    continue;
                }

                if (controller.AutomaticPopulationFreeSlotCount <= 0)
                {
                    Reject(RejectionReason.NoFreeSlot);
                    EnterCooldown(ref candidate, now);
                    candidates[index] = candidate;
                    continue;
                }

                if (!controller.IsAutomaticPopulationPositionClear(
                        candidate.GroundPosition,
                        settings.MinimumSpacingMetres,
                        candidate.Identity))
                {
                    Reject(RejectionReason.TooClose);
                    EnterCooldown(ref candidate, now);
                    candidates[index] = candidate;
                    continue;
                }

                if (!SpawnCandidate(
                        controller,
                        settings,
                        ref candidate))
                {
                    Reject(RejectionReason.SpawnOrUpdateFailed);
                    EnterCooldown(ref candidate, now);
                }

                candidates[index] = candidate;
            }

            return consumed;
        }

        private void EvaluateNewCandidates(
            WeatherLightRayController controller,
            in Settings settings,
            double now,
            int budget)
        {
            int target = Mathf.Min(settings.DesiredCount, settings.MaximumCount);
            if (CountState(
                    WeatherLightRayPopulationCandidateState.Active) >= target)
            {
                Reject(RejectionReason.DesiredCountMet);
                return;
            }

            for (int check = 0; check < budget; check++)
            {
                if (!TryGetNextCell(out int cellX, out int cellZ))
                {
                    return;
                }

                candidateChecksLastTick++;
                Vector3 candidatePosition = ResolveCandidatePosition(
                    settings.Seed,
                    settings.IdentitySalt,
                    cellX,
                    cellZ,
                    settings.MinimumSpacingMetres);
                long identity = ResolveStableIdentity(
                    settings.Seed,
                    settings.IdentitySalt,
                    settings.SourceKind,
                    cellX,
                    cellZ);
                if (!IsInsideActiveRegion(candidatePosition))
                {
                    Reject(RejectionReason.OutsideActiveRegion);
                    continue;
                }

                int existingIndex = FindCandidate(identity);
                if (existingIndex >= 0)
                {
                    Candidate existing = candidates[existingIndex];
                    if (existing.State ==
                        WeatherLightRayPopulationCandidateState.Cooldown)
                    {
                        Reject(RejectionReason.CooldownActive);
                    }
                    continue;
                }

                if (CountState(
                        WeatherLightRayPopulationCandidateState.Active) >= target)
                {
                    Reject(RejectionReason.DesiredCountMet);
                    return;
                }

                if (CountState(
                        WeatherLightRayPopulationCandidateState.Active) >=
                    settings.MaximumCount)
                {
                    Reject(RejectionReason.MaximumCountReached);
                    return;
                }

                if (controller.AutomaticPopulationFreeSlotCount <= 0)
                {
                    Reject(RejectionReason.NoFreeSlot);
                    return;
                }

                if (!controller.IsAutomaticPopulationPositionClear(
                        candidatePosition,
                        settings.MinimumSpacingMetres,
                        identity) ||
                    !IsSeparatedFromCandidateStates(
                        candidatePosition,
                        settings.MinimumSpacingMetres,
                        identity))
                {
                    Reject(RejectionReason.TooClose);
                    continue;
                }

                if (!TryAcquireGround(
                        settings,
                        candidatePosition,
                        out RaycastHit groundHit))
                {
                    continue;
                }

                if (!TryEvaluateCloudFootprint(
                        settings,
                        groundHit.point,
                        out float clearance,
                        out uint dataVersion))
                {
                    continue;
                }

                int freeCandidateIndex = FindFreeCandidateIndex();
                if (freeCandidateIndex < 0)
                {
                    Reject(RejectionReason.CandidateStorageFull);
                    return;
                }

                candidates[freeCandidateIndex] = new Candidate
                {
                    Occupied = true,
                    Identity = identity,
                    CellX = cellX,
                    CellZ = cellZ,
                    GroundPosition = groundHit.point,
                    State = WeatherLightRayPopulationCandidateState.Pending,
                    Handle = default,
                    Clearance = clearance,
                    ValidSince = now,
                    InvalidSince = 0.0,
                    CooldownUntil = 0.0,
                    ConsecutiveValidEvaluations = 1,
                    CloudDataVersion = dataVersion
                };
                Reject(RejectionReason.QualificationPending);
            }
        }

        private bool TryAcquireGround(
            in Settings settings,
            Vector3 candidatePosition,
            out RaycastHit hit)
        {
            float halfDistance = settings.GroundSearchDistanceMetres * 0.5f;
            Vector3 origin = new Vector3(
                candidatePosition.x,
                focusWorld.y + halfDistance,
                candidatePosition.z);
            groundRaycastsLastTick++;
            if (!Physics.Raycast(
                    origin,
                    Vector3.down,
                    out hit,
                    settings.GroundSearchDistanceMetres,
                    settings.GroundMask,
                    QueryTriggerInteraction.Ignore))
            {
                Reject(RejectionReason.NoGroundHit);
                return false;
            }

            float minimumUpDot = Mathf.Cos(
                settings.MaximumGroundSlopeDegrees * Mathf.Deg2Rad);
            if (Vector3.Dot(hit.normal, Vector3.up) < minimumUpDot)
            {
                Reject(RejectionReason.GroundTooSteep);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Evaluates only the cloud policy supplied by the active population
        /// rule. Ignored/Any rules return immediately and perform no cloud
        /// sampling. Clear Footprint retains the bounded 13 x 4 forecast
        /// contract. Distinct Cloud Opening adds only a bounded surrounding
        /// ring contrast check; no full-cookie scan is permitted here.
        /// </summary>
        private bool TryEvaluateCloudFootprint(
            in Settings settings,
            Vector3 centre,
            out float minimumClearance,
            out uint dataVersion)
        {
            minimumClearance = 1f;
            dataVersion = settings.CloudController != null
                ? unchecked((uint)settings.CloudController.CurrentCookieSeed)
                : 0u;

            if (settings.CloudDataRequirement ==
                    WeatherLightRayCloudDataRequirement.Ignored ||
                settings.SpatialCloudPolicy ==
                    WeatherLightRaySpatialCloudPolicy.AnyPosition)
            {
                return true;
            }

            WeatherCloudShadowController cloud = settings.CloudController;
            bool producerAbsentOrDisabled = cloud == null ||
                !cloud.IsPublished ||
                !cloud.CloudShadowsEnabled;
            if (producerAbsentOrDisabled)
            {
                if (settings.CloudDataRequirement ==
                    WeatherLightRayCloudDataRequirement.Optional)
                {
                    dataVersion = 0u;
                    return true;
                }

                Reject(RejectionReason.CloudUnavailable);
                return false;
            }

            if (!cloud.CookieReady || settings.CloudProjectionLight == null)
            {
                Reject(RejectionReason.CloudUnavailable);
                return false;
            }

            if (cloud.EvolutionInProgress &&
                cloud.EvolutionProgress <
                    settings.CloudEvolutionResumeThreshold)
            {
                Reject(RejectionReason.CloudEvolutionUnstable);
                return false;
            }

            float radius = settings.ActivePreset.DefaultAreaDiameterMetres *
                0.5f;
            float halfRadius = radius * 0.5f;
            float middleForecast = Mathf.Lerp(
                AutomaticFadeInSeconds,
                settings.MinimumViableOpeningDurationSeconds,
                0.5f);

            for (int forecastIndex = 0;
                forecastIndex < ForecastSampleCount;
                forecastIndex++)
            {
                float forecastSeconds;
                switch (forecastIndex)
                {
                    case 0:
                        forecastSeconds = 0f;
                        break;
                    case 1:
                        forecastSeconds = AutomaticFadeInSeconds;
                        break;
                    case 2:
                        forecastSeconds = middleForecast;
                        break;
                    default:
                        forecastSeconds =
                            settings.MinimumViableOpeningDurationSeconds;
                        break;
                }

                for (int sampleIndex = 0;
                    sampleIndex < FootprintSampleCount;
                    sampleIndex++)
                {
                    Vector3 samplePosition = centre +
                        ResolveFootprintOffset(
                            sampleIndex,
                            radius,
                            halfRadius);
                    cloudSamplesLastTick++;
                    if (!cloud.TrySampleCloudTransmissionAtTimeOffset(
                            samplePosition,
                            settings.CloudProjectionLight,
                            forecastSeconds,
                            out WeatherCloudTransmissionSample sample) ||
                        !sample.IsUsable)
                    {
                        Reject(RejectionReason.CloudUnavailable);
                        return false;
                    }

                    if (!sample.IsStable &&
                        cloud.EvolutionProgress <
                            settings.CloudEvolutionResumeThreshold)
                    {
                        Reject(RejectionReason.CloudEvolutionUnstable);
                        return false;
                    }

                    float normalizedOpen = NormalizeTransmission(
                        sample.Transmission,
                        cloud.ShadedTransmission);
                    minimumClearance = Mathf.Min(
                        minimumClearance,
                        normalizedOpen);
                    if (minimumClearance < settings.MinimumClearance)
                    {
                        Reject(RejectionReason.InsufficientClearance);
                        return false;
                    }
                }
            }

            if (settings.SpatialCloudPolicy !=
                WeatherLightRaySpatialCloudPolicy.DistinctCloudOpening)
            {
                return true;
            }

            float surroundingRadius = radius +
                Mathf.Max(0f, settings.SurroundingSampleRadiusMetres);
            float surroundingOpenSum = 0f;
            const int surroundingDirectionCount = 8;
            for (int timeIndex = 0; timeIndex < 2; timeIndex++)
            {
                float forecastSeconds = timeIndex == 0
                    ? 0f
                    : settings.MinimumViableOpeningDurationSeconds;
                for (int directionIndex = 0;
                    directionIndex < surroundingDirectionCount;
                    directionIndex++)
                {
                    float angle = directionIndex * 45f * Mathf.Deg2Rad;
                    Vector3 position = centre + new Vector3(
                        Mathf.Cos(angle) * surroundingRadius,
                        0f,
                        Mathf.Sin(angle) * surroundingRadius);
                    cloudSamplesLastTick++;
                    if (!cloud.TrySampleCloudTransmissionAtTimeOffset(
                            position,
                            settings.CloudProjectionLight,
                            forecastSeconds,
                            out WeatherCloudTransmissionSample sample) ||
                        !sample.IsUsable)
                    {
                        Reject(RejectionReason.CloudUnavailable);
                        return false;
                    }

                    surroundingOpenSum += NormalizeTransmission(
                        sample.Transmission,
                        cloud.ShadedTransmission);
                }
            }

            float surroundingOpenAverage = surroundingOpenSum /
                (surroundingDirectionCount * 2f);
            float contrast = minimumClearance - surroundingOpenAverage;
            if (contrast < settings.MinimumDistinctOpeningContrast)
            {
                Reject(RejectionReason.InsufficientClearance);
                return false;
            }

            return true;
        }

        private static float NormalizeTransmission(
            float transmission,
            float shadedTransmission)
        {
            return Mathf.Clamp01(
                (transmission - shadedTransmission) /
                Mathf.Max(0.0001f, 1f - shadedTransmission));
        }

        private static Vector3 ResolveFootprintOffset(
            int sampleIndex,
            float radius,
            float halfRadius)
        {
            if (sampleIndex == 0)
            {
                return Vector3.zero;
            }

            if (sampleIndex <= 4)
            {
                float angle = (sampleIndex - 1) * 90f * Mathf.Deg2Rad;
                return new Vector3(
                    Mathf.Cos(angle) * halfRadius,
                    0f,
                    Mathf.Sin(angle) * halfRadius);
            }

            float perimeterAngle = (sampleIndex - 5) * 45f *
                Mathf.Deg2Rad;
            return new Vector3(
                Mathf.Cos(perimeterAngle) * radius,
                0f,
                Mathf.Sin(perimeterAngle) * radius);
        }

        private bool SpawnCandidate(
            WeatherLightRayController controller,
            in Settings settings,
            ref Candidate candidate)
        {
            WeatherLightRayCloudOpening opening = BuildOpening(
                settings,
                candidate);
            WeatherLightRayCloudSpawnSettings spawnSettings =
                BuildSpawnSettings(settings, candidate.Identity);
            WeatherLightRayHandle handle = candidate.Handle;
            if (!controller.TrySpawnOrUpdateResolvedCloudOpening(
                    ref handle,
                    opening,
                    spawnSettings,
                    out _,
                    out _))
            {
                return false;
            }

            candidate.Handle = handle;
            candidate.State = WeatherLightRayPopulationCandidateState.Active;
            candidate.InvalidSince = 0.0;
            return true;
        }

        private void UpdateActiveOpening(
            WeatherLightRayController controller,
            in Settings settings,
            ref Candidate candidate)
        {
            WeatherLightRayCloudOpening opening = BuildOpening(
                settings,
                candidate);
            WeatherLightRayCloudSpawnSettings spawnSettings =
                BuildSpawnSettings(settings, candidate.Identity);
            WeatherLightRayHandle handle = candidate.Handle;
            if (!controller.TrySpawnOrUpdateResolvedCloudOpening(
                    ref handle,
                    opening,
                    spawnSettings,
                    out _,
                    out _))
            {
                Reject(RejectionReason.SpawnOrUpdateFailed);
                BeginRetirement(
                    controller,
                    ref candidate,
                    Time.realtimeSinceStartupAsDouble);
                return;
            }

            candidate.Handle = handle;
        }

        private static WeatherLightRayCloudOpening BuildOpening(
            in Settings settings,
            in Candidate candidate)
        {
            return new WeatherLightRayCloudOpening(
                candidate.Identity,
                settings.SourceKind,
                candidate.GroundPosition,
                settings.RayDirectionWorld,
                settings.ActivePreset.DefaultAreaDiameterMetres,
                candidate.Clearance,
                0.5f,
                candidate.Clearance,
                candidate.CloudDataVersion);
        }

        private static WeatherLightRayCloudSpawnSettings BuildSpawnSettings(
            in Settings settings,
            long identity)
        {
            uint variationSeed = unchecked((uint)identity);
            if (variationSeed == 0u)
            {
                variationSeed = 1u;
            }

            return new WeatherLightRayCloudSpawnSettings(
                variationSeed,
                localIntensityMultiplier: 1f,
                lifetimePolicy:
                    WeatherLightRayLifetimePolicy.ExternallyControlled,
                fadeInDurationSeconds: AutomaticFadeInSeconds,
                holdDurationSeconds: 0f,
                fadeOutDurationSeconds: AutomaticFadeOutSeconds,
                initiallyVisible: true,
                runtimeCloudPolicy: WeatherLightRayCloudPolicy.IgnoreClouds,
                sourceGatePolicy: settings.SourceGatePolicy,
                movementPolicy: WeatherLightRayMovementPolicy.Static,
                gameplayChannel: 0,
                priority: WeatherLightRaySpawnPriority.Low,
                resetLifecycleOnUpdate: false);
        }

        private void BeginRetirement(
            WeatherLightRayController controller,
            ref Candidate candidate,
            double now)
        {
            if (candidate.State ==
                WeatherLightRayPopulationCandidateState.Retiring)
            {
                return;
            }

            if (candidate.Handle.IsValid &&
                controller.IsValid(candidate.Handle))
            {
                controller.TrySetProceduralRayVisible(
                    candidate.Handle,
                    false,
                    out _);
                candidate.State =
                    WeatherLightRayPopulationCandidateState.Retiring;
                candidate.InvalidSince = now;
                return;
            }

            EnterCooldown(ref candidate, now);
        }

        private static void EnterCooldown(
            ref Candidate candidate,
            double now)
        {
            candidate.State = WeatherLightRayPopulationCandidateState.Cooldown;
            candidate.Handle = default;
            candidate.ValidSince = 0.0;
            candidate.InvalidSince = 0.0;
            candidate.ConsecutiveValidEvaluations = 0;
            candidate.CooldownUntil = now + CandidateCooldownSeconds;
        }

        private void RetireAllAutomaticCandidates(
            WeatherLightRayController controller,
            double now)
        {
            if (candidates == null)
            {
                return;
            }

            for (int index = 0; index < candidates.Length; index++)
            {
                Candidate candidate = candidates[index];
                if (!candidate.Occupied)
                {
                    continue;
                }

                if (candidate.State ==
                    WeatherLightRayPopulationCandidateState.Active)
                {
                    BeginRetirement(controller, ref candidate, now);
                }
                else if (candidate.State ==
                    WeatherLightRayPopulationCandidateState.Pending)
                {
                    EnterCooldown(ref candidate, now);
                }

                candidates[index] = candidate;
            }
        }

        private void UpdateCounts(
            WeatherLightRayController controller,
            double now)
        {
            activeCount = 0;
            pendingCount = 0;
            retiringCount = 0;
            cooldownCount = 0;
            if (candidates == null)
            {
                return;
            }

            for (int index = 0; index < candidates.Length; index++)
            {
                Candidate candidate = candidates[index];
                if (!candidate.Occupied)
                {
                    continue;
                }

                if ((candidate.State ==
                        WeatherLightRayPopulationCandidateState.Active ||
                    candidate.State ==
                        WeatherLightRayPopulationCandidateState.Retiring) &&
                    (!candidate.Handle.IsValid ||
                        !controller.IsValid(candidate.Handle)))
                {
                    EnterCooldown(ref candidate, now);
                    candidates[index] = candidate;
                }

                switch (candidate.State)
                {
                    case WeatherLightRayPopulationCandidateState.Pending:
                        pendingCount++;
                        break;
                    case WeatherLightRayPopulationCandidateState.Active:
                        activeCount++;
                        break;
                    case WeatherLightRayPopulationCandidateState.Retiring:
                        retiringCount++;
                        break;
                    case WeatherLightRayPopulationCandidateState.Cooldown:
                        cooldownCount++;
                        break;
                }
            }
        }

        private int CountState(
            WeatherLightRayPopulationCandidateState state)
        {
            int count = 0;
            if (candidates == null)
            {
                return count;
            }

            for (int index = 0; index < candidates.Length; index++)
            {
                if (candidates[index].Occupied &&
                    candidates[index].State == state)
                {
                    count++;
                }
            }

            return count;
        }

        private bool IsInsideActiveRegion(Vector3 position)
        {
            Vector2 delta = new Vector2(
                position.x - focusWorld.x,
                position.z - focusWorld.z);
            return delta.sqrMagnitude <=
                activeRadiusMetres * activeRadiusMetres;
        }

        private bool IsSeparatedFromCandidateStates(
            Vector3 position,
            float spacing,
            long identity)
        {
            float spacingSquared = spacing * spacing;
            for (int index = 0; index < candidates.Length; index++)
            {
                Candidate candidate = candidates[index];
                if (!candidate.Occupied ||
                    candidate.Identity == identity ||
                    candidate.State ==
                        WeatherLightRayPopulationCandidateState.Cooldown)
                {
                    continue;
                }

                Vector2 delta = new Vector2(
                    candidate.GroundPosition.x - position.x,
                    candidate.GroundPosition.z - position.z);
                if (delta.sqrMagnitude < spacingSquared)
                {
                    return false;
                }
            }

            return true;
        }

        private bool TryGetNextCell(out int cellX, out int cellZ)
        {
            cellX = 0;
            cellZ = 0;
            if (!cellCursorInitialized || cellsInActiveRegion <= 0)
            {
                return false;
            }

            cellX = cellCursorX;
            cellZ = cellCursorZ;
            cellCursorX++;
            if (cellCursorX > maximumCellX)
            {
                cellCursorX = minimumCellX;
                cellCursorZ++;
                if (cellCursorZ > maximumCellZ)
                {
                    cellCursorZ = minimumCellZ;
                }
            }

            return true;
        }

        private static Vector3 ResolveCandidatePosition(
            int seed,
            ulong identitySalt,
            int cellX,
            int cellZ,
            float cellSize)
        {
            ulong hash = MixHash(
                unchecked((ulong)(uint)seed) ^
                identitySalt ^
                (unchecked((ulong)(uint)cellX) << 32) ^
                unchecked((uint)cellZ));
            float offsetX = 0.15f + HashToUnitFloat(hash) * 0.70f;
            hash = MixHash(hash + 0x9E3779B97F4A7C15UL);
            float offsetZ = 0.15f + HashToUnitFloat(hash) * 0.70f;
            return new Vector3(
                (cellX + offsetX) * cellSize,
                0f,
                (cellZ + offsetZ) * cellSize);
        }

        private static long ResolveStableIdentity(
            int seed,
            ulong identitySalt,
            WeatherLightRaySourceKind sourceKind,
            int cellX,
            int cellZ)
        {
            ulong value = unchecked((ulong)(uint)seed) ^ identitySalt;
            value ^= unchecked((ulong)(uint)cellX) *
                0x9E3779B185EBCA87UL;
            value ^= unchecked((ulong)(uint)cellZ) *
                0xC2B2AE3D27D4EB4FUL;
            value ^= (ulong)sourceKind + 1UL;
            value = MixHash(value) | 0x4000000000000000UL;
            long identity = unchecked((long)value);
            return identity == 0L ? 1L : identity;
        }

        private static ulong MixHash(ulong value)
        {
            value ^= value >> 30;
            value *= 0xBF58476D1CE4E5B9UL;
            value ^= value >> 27;
            value *= 0x94D049BB133111EBUL;
            value ^= value >> 31;
            return value;
        }

        private static float HashToUnitFloat(ulong value)
        {
            return (float)((value >> 40) & 0xFFFFFFUL) /
                16777215f;
        }

        private int FindCandidate(long identity)
        {
            for (int index = 0; index < candidates.Length; index++)
            {
                if (candidates[index].Occupied &&
                    candidates[index].Identity == identity)
                {
                    return index;
                }
            }

            return -1;
        }

        private int FindFreeCandidateIndex()
        {
            for (int index = 0; index < candidates.Length; index++)
            {
                if (!candidates[index].Occupied)
                {
                    return index;
                }
            }

            return -1;
        }

        private void Reject(RejectionReason reason)
        {
            int index = (int)reason;
            if (rejectionCounts != null &&
                index >= 0 &&
                index < rejectionCounts.Length)
            {
                rejectionCounts[index]++;
                if (totalRejectionCounts != null &&
                    index < totalRejectionCounts.Length)
                {
                    totalRejectionCounts[index]++;
                }
            }
        }

        private void AppendRejectionReport(StringBuilder builder)
        {
            builder.AppendLine("Rejection counts (last tick / cumulative):");
            for (int index = 0;
                index < (int)RejectionReason.Count;
                index++)
            {
                builder.Append("- ")
                    .Append((RejectionReason)index)
                    .Append(": ")
                    .Append(rejectionCounts[index])
                    .Append(" / ")
                    .AppendLine(totalRejectionCounts[index].ToString());
            }
        }
    }
}
