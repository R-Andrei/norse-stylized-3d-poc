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

    public enum WeatherLightRayPopulationRuntimeState
    {
        Disabled = 0,
        Suspended = 1,
        SpawningPaused = 2,
        Running = 3
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

    internal sealed class WeatherLightRayPopulationRuntime
    {
        internal readonly struct Settings
        {
            internal readonly bool Enabled;
            internal readonly bool GlobalLightRaysEnabled;
            internal readonly int Seed;
            internal readonly Transform FocusOverride;
            internal readonly Camera RenderCamera;
            internal readonly LayerMask GroundMask;
            internal readonly int DesiredCount;
            internal readonly int MaximumCount;
            internal readonly float MinimumSpacingMetres;
            internal readonly float OffscreenMarginMetres;
            internal readonly float EvaluationRateHz;
            internal readonly float MinimumClearance;
            internal readonly float InvalidGraceDurationSeconds;
            internal readonly float SpawnFadeDurationSeconds;
            internal readonly float DespawnFadeDurationSeconds;
            internal readonly float MinimumRayLifetimeSeconds;
            internal readonly float MaximumRayLifetimeSeconds;
            internal readonly float ReplacementDelaySeconds;
            internal readonly float MaximumGroundSlopeDegrees;
            internal readonly float CloudEvolutionResumeThreshold;
            internal readonly WeatherLightRayPreset ActivePreset;
            internal readonly WeatherLightRaySourceState DirectionalSource;
            internal readonly WeatherCloudShadowController CloudController;
            internal readonly int CandidateChecksPerUpdate;
            internal readonly float GroundRaycastDistanceMetres;

            internal Settings(
                bool enabled,
                bool globalLightRaysEnabled,
                int seed,
                Transform focusOverride,
                Camera renderCamera,
                LayerMask groundMask,
                int desiredCount,
                int maximumCount,
                float minimumSpacingMetres,
                float offscreenMarginMetres,
                float evaluationRateHz,
                float minimumClearance,
                float invalidGraceDurationSeconds,
                float spawnFadeDurationSeconds,
                float despawnFadeDurationSeconds,
                float minimumRayLifetimeSeconds,
                float maximumRayLifetimeSeconds,
                float replacementDelaySeconds,
                float maximumGroundSlopeDegrees,
                float cloudEvolutionResumeThreshold,
                WeatherLightRayPreset activePreset,
                in WeatherLightRaySourceState directionalSource,
                WeatherCloudShadowController cloudController)
            {
                Enabled = enabled;
                GlobalLightRaysEnabled = globalLightRaysEnabled;
                Seed = seed;
                FocusOverride = focusOverride;
                RenderCamera = renderCamera;
                GroundMask = groundMask;
                DesiredCount = desiredCount;
                MaximumCount = maximumCount;
                MinimumSpacingMetres = minimumSpacingMetres;
                OffscreenMarginMetres = offscreenMarginMetres;
                EvaluationRateHz = evaluationRateHz;
                MinimumClearance = minimumClearance;
                InvalidGraceDurationSeconds = invalidGraceDurationSeconds;
                SpawnFadeDurationSeconds = spawnFadeDurationSeconds;
                DespawnFadeDurationSeconds = despawnFadeDurationSeconds;
                MinimumRayLifetimeSeconds = minimumRayLifetimeSeconds;
                MaximumRayLifetimeSeconds = maximumRayLifetimeSeconds;
                ReplacementDelaySeconds = replacementDelaySeconds;
                MaximumGroundSlopeDegrees = maximumGroundSlopeDegrees;
                CloudEvolutionResumeThreshold = cloudEvolutionResumeThreshold;
                ActivePreset = activePreset;
                DirectionalSource = directionalSource;
                CandidateChecksPerUpdate = Mathf.Clamp(
                    MaximumCount * 2,
                    4,
                    64);
                float farClip = RenderCamera != null &&
                    !float.IsNaN(RenderCamera.farClipPlane) &&
                    !float.IsInfinity(RenderCamera.farClipPlane)
                        ? RenderCamera.farClipPlane
                        : 100f;
                GroundRaycastDistanceMetres = Mathf.Max(100f, farClip);
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
            InsufficientPresentCentre = 9,
            InsufficientPresentSurrounding = 10,
            InsufficientPredictedCentre = 11,
            CooldownActive = 12,
            SourceUnavailable = 13,
            CandidateStorageFull = 14,
            SpawnOrUpdateFailed = 15,
            Count = 16
        }

        private struct Candidate
        {
            internal bool Occupied;
            internal long Identity;
            internal long ActivationIdentity;
            internal int CellX;
            internal int CellZ;
            internal Vector3 GroundPosition;
            internal WeatherLightRayPopulationCandidateState State;
            internal WeatherLightRayHandle Handle;
            internal float Clearance;
            internal double InvalidSince;
            internal double CooldownUntil;
            internal double SpawnedAt;
            internal float AssignedLifetimeSeconds;
            internal float PresentCentreOpenness;
            internal int PresentSurroundingPassCount;
            internal float PredictedCentreOpenness;
            internal float PredictionHorizonSeconds;
            internal string RetirementReason;
            internal uint CloudDataVersion;
        }

        private const int MinimumCandidateCapacity = 64;
        private const int CandidateCapacityMultiplier = 8;
        private const int RequiredPresentSurroundingPassCount = 2;
        private const float PredictionStableWindowFraction = 0.70f;
        private const float ReleaseIntensityThreshold = 0.001f;
        private const float RegionChangeEpsilonSquared = 0.25f;
        private const int CameraFootprintPointCount = 8;
        private static readonly Vector2[] CameraViewportSamples =
        {
            new Vector2(0f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(1f, 0f),
            new Vector2(1f, 0.5f),
            new Vector2(1f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0f, 1f),
            new Vector2(0f, 0.5f)
        };

        private Candidate[] candidates;
        private int[] rejectionCounts;
        private long[] totalRejectionCounts;
        private int traversalCursor;
        private int traversalOffset;
        private int traversalStep = 1;
        private int traversalCellCount;
        private int traversalSeed;
        private ulong turnoverEpoch;
        private int minimumCellX;
        private int maximumCellX;
        private int minimumCellZ;
        private int maximumCellZ;
        private bool traversalInitialized;
        private double nextEvaluationTime;
        private Vector3 focusWorld;
        private float activeRadiusMetres;
        private readonly Vector2[] activeFootprint =
            new Vector2[CameraFootprintPointCount];
        private int activeFootprintCount;
        private bool useFootprintRegion;
        private float activeMinimumX;
        private float activeMaximumX;
        private float activeMinimumZ;
        private float activeMaximumZ;
        private float replacementDelaySeconds = 1.5f;
        private Vector3 previousFocusWorld;
        private float previousActiveRadiusMetres;
        private WeatherLightRayPopulationRuntimeState runtimeState =
            WeatherLightRayPopulationRuntimeState.Disabled;
        private string statusReason = "Automatic population is disabled.";
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

        internal WeatherLightRayPopulationRuntimeState RuntimeState =>
            runtimeState;
        internal bool IsOperating =>
            runtimeState == WeatherLightRayPopulationRuntimeState.Running ||
            runtimeState == WeatherLightRayPopulationRuntimeState.SpawningPaused;
        internal string StatusReason => statusReason;
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

        internal void Tick(
            WeatherLightRayController controller,
            in Settings settings,
            double now)
        {
            EnsureStorage(settings.MaximumCount);
            replacementDelaySeconds = Mathf.Max(
                0f,
                settings.ReplacementDelaySeconds);

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

            string dependencyFailure = ResolveSuspensionReason(settings);
            if (!string.IsNullOrEmpty(dependencyFailure))
            {
                runtimeState = settings.Enabled
                    ? WeatherLightRayPopulationRuntimeState.Suspended
                    : WeatherLightRayPopulationRuntimeState.Disabled;
                statusReason = dependencyFailure;
                RetireAllAutomaticCandidates(
                    controller,
                    now,
                    dependencyFailure);
                ProcessRetiringAndCooldown(controller, now);
                UpdateCounts(controller, now);
                return;
            }

            if (!TryResolveFocus(settings, out focusWorld, out activeRadiusMetres))
            {
                runtimeState = WeatherLightRayPopulationRuntimeState.Suspended;
                statusReason =
                    "No usable ground reference or camera-plane footprint could be resolved for automatic population.";
                RetireAllAutomaticCandidates(
                    controller,
                    now,
                    statusReason);
                ProcessRetiringAndCooldown(controller, now);
                UpdateCounts(controller, now);
                return;
            }

            UpdateCellBounds(
                settings.MinimumSpacingMetres,
                settings.Seed);
            ProcessRetiringAndCooldown(controller, now);
            RetireOutsideBudget(controller, settings, now);
            RevalidateActiveCandidates(controller, settings, now);

            if (ShouldPauseSpawning(settings))
            {
                runtimeState =
                    WeatherLightRayPopulationRuntimeState.SpawningPaused;
                statusReason =
                    "New atmospheric-ray spawning is paused for the active cloud-pattern transition.";
                UpdateCounts(controller, now);
                return;
            }

            runtimeState = WeatherLightRayPopulationRuntimeState.Running;
            statusReason = string.Empty;
            EvaluateNewCandidates(
                controller,
                settings,
                now,
                settings.CandidateChecksPerUpdate);
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

            for (int index = 0; index < candidates.Length; index++)
            {
                Candidate candidate = candidates[index];
                if (!candidate.Occupied)
                {
                    continue;
                }

                if (candidate.Handle.IsValid &&
                    controller.IsValid(candidate.Handle))
                {
                    if (releaseImmediately)
                    {
                        controller.TryReleaseProceduralRay(
                            candidate.Handle,
                            out _);
                    }
                    else
                    {
                        controller.TrySetProceduralRayVisible(
                            candidate.Handle,
                            false,
                            out _);
                    }
                }

                candidate = default;
                candidates[index] = candidate;
            }

            activeCount = 0;
            pendingCount = 0;
            retiringCount = 0;
            cooldownCount = 0;
            runtimeState = WeatherLightRayPopulationRuntimeState.Disabled;
            statusReason = "Automatic population is disabled.";
            turnoverEpoch++;
            traversalCursor = 0;
            traversalCellCount = 0;
            traversalInitialized = false;
        }

        internal int CopyDebugRecords(
            WeatherLightRayPopulationDebugRecord[] destination)
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

                if (destination != null && count < destination.Length)
                {
                    destination[count] =
                        new WeatherLightRayPopulationDebugRecord(
                            candidate.ActivationIdentity,
                            candidate.GroundPosition,
                            candidate.State,
                            candidate.Clearance,
                            candidate.Handle);
                }

                count++;
            }

            return count;
        }

        internal int CopyActiveFootprint(Vector3[] destination)
        {
            if (!useFootprintRegion || activeFootprintCount < 3)
            {
                return 0;
            }

            if (destination != null)
            {
                int count = Mathf.Min(destination.Length, activeFootprintCount);
                for (int index = 0; index < count; index++)
                {
                    Vector2 point = activeFootprint[index];
                    destination[index] = new Vector3(
                        point.x,
                        focusWorld.y + 0.05f,
                        point.y);
                }
            }

            return activeFootprintCount;
        }

        internal void AppendReport(
            StringBuilder builder,
            in Settings settings,
            int freeSlotCount)
        {
            builder.AppendLine("[Automatic Atmospheric Population]");
            builder.Append("Enabled / runtime state: ")
                .Append(settings.Enabled ? "Yes" : "No")
                .Append(" / ")
                .AppendLine(runtimeState.ToString());
            builder.Append("Status reason: ")
                .AppendLine(string.IsNullOrEmpty(statusReason)
                    ? "None"
                    : statusReason);
            builder.Append("Focus / region / enclosing radius: ")
                .Append(focusWorld.ToString("F3"))
                .Append(" / ")
                .Append("Camera footprint")
                .Append(" / ")
                .Append(activeRadiusMetres.ToString("0.###"))
                .AppendLine(" m");
            builder.Append("Lifetime min / max / replacement delay: ")
                .Append(settings.MinimumRayLifetimeSeconds.ToString("0.###"))
                .Append(" / ")
                .Append(settings.MaximumRayLifetimeSeconds.ToString("0.###"))
                .Append(" / ")
                .Append(settings.ReplacementDelaySeconds.ToString("0.###"))
                .AppendLine(" s");
            builder.Append("Spawn / despawn fade: ")
                .Append(settings.SpawnFadeDurationSeconds.ToString("0.###"))
                .Append(" / ")
                .Append(settings.DespawnFadeDurationSeconds.ToString("0.###"))
                .AppendLine(" s");
            builder.Append("Minimum openness / present surrounding rule: ")
                .Append(settings.MinimumClearance.ToString("0.###"))
                .Append(" / ")
                .Append(RequiredPresentSurroundingPassCount)
                .AppendLine(" of 4");
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
                .Append(settings.CandidateChecksPerUpdate)
                .Append(" / ")
                .AppendLine(cellsInActiveRegion.ToString());
            builder.Append("Turnover epoch / traversal progress / permutation offset-step: ")
                .Append(turnoverEpoch)
                .Append(" / ")
                .Append(traversalCursor)
                .Append(" of ")
                .Append(traversalCellCount)
                .Append(" / ")
                .Append(traversalOffset)
                .Append('-')
                .AppendLine(traversalStep.ToString());
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

                builder.Append("Candidate cell / activation ")
                    .Append(candidate.Identity)
                    .Append(" / ")
                    .Append(candidate.ActivationIdentity)
                    .Append(" | cell ")
                    .Append(candidate.CellX)
                    .Append(',')
                    .Append(candidate.CellZ)
                    .Append(" | ")
                    .Append(candidate.State)
                    .Append(" | ")
                    .Append(candidate.Handle)
                    .Append(" | openness centre / surrounding / predicted: ")
                    .Append(candidate.PresentCentreOpenness.ToString("0.###"))
                    .Append(" / ")
                    .Append(candidate.PresentSurroundingPassCount)
                    .Append(" of 4 / ")
                    .Append(candidate.PredictedCentreOpenness.ToString("0.###"))
                    .Append(" | prediction ")
                    .Append(candidate.PredictionHorizonSeconds.ToString("0.###"))
                    .Append(" s | placement strength ")
                    .Append(candidate.Clearance.ToString("0.###"))
                    .Append(" | age / lifetime ")
                    .Append(candidate.SpawnedAt > 0.0
                        ? Math.Max(0.0, Time.realtimeSinceStartupAsDouble - candidate.SpawnedAt).ToString("0.###")
                        : "-")
                    .Append(" / ")
                    .Append(candidate.AssignedLifetimeSeconds > 0f
                        ? candidate.AssignedLifetimeSeconds.ToString("0.###")
                        : "-")
                    .Append(" s | remaining ")
                    .Append(candidate.SpawnedAt > 0.0 &&
                        candidate.AssignedLifetimeSeconds > 0f
                        ? Math.Max(
                            0.0,
                            candidate.AssignedLifetimeSeconds -
                                (Time.realtimeSinceStartupAsDouble - candidate.SpawnedAt)).ToString("0.###")
                        : "-")
                    .Append(" s | retirement ")
                    .Append(string.IsNullOrEmpty(candidate.RetirementReason)
                        ? "None"
                        : candidate.RetirementReason)
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
                return "Automatic population requires an Active Preset.";
            }

            if (!settings.DirectionalSource.Available ||
                settings.DirectionalSource.SourceLight == null)
            {
                return "The atmospheric population directional source is unavailable.";
            }

            if (settings.GroundMask.value == 0)
            {
                return "Automatic population requires a non-empty Ground Mask.";
            }

            if (settings.CloudController == null ||
                !settings.CloudController.IsPublished)
            {
                return "No published Weather Cloud Shadow Controller is available.";
            }

            if (!settings.CloudController.CookieReady)
            {
                return "The published cloud transmission cookie is not ready.";
            }

            if (settings.RenderCamera == null)
            {
                return "No resolved render camera is available for the automatic-population footprint.";
            }

            return string.Empty;
        }

        private static bool ShouldPauseSpawning(in Settings settings)
        {
            return settings.CloudController != null &&
                settings.CloudController.EvolutionInProgress &&
                settings.CloudController.EvolutionProgress <
                    settings.CloudEvolutionResumeThreshold;
        }

        private bool TryResolveFocus(
            in Settings settings,
            out Vector3 resolvedFocus,
            out float resolvedRadius)
        {
            activeFootprintCount = 0;
            useFootprintRegion = false;

            Camera camera = settings.RenderCamera;
            if (camera == null ||
                !TryResolveGroundReference(
                    camera,
                    settings,
                    out Vector3 groundReference))
            {
                resolvedFocus = Vector3.zero;
                resolvedRadius = 0f;
                return false;
            }

            Plane groundPlane = new Plane(Vector3.up, groundReference);
            if (!TryProjectViewportToPlane(
                    camera,
                    0.5f,
                    0.5f,
                    groundPlane,
                    settings.GroundRaycastDistanceMetres,
                    out Vector3 projectedFocus))
            {
                resolvedFocus = Vector3.zero;
                resolvedRadius = 0f;
                return false;
            }

            float maximumDistance = 0f;
            for (int index = 0; index < CameraViewportSamples.Length; index++)
            {
                Vector2 viewport = CameraViewportSamples[index];
                if (!TryProjectViewportToPlane(
                        camera,
                        viewport.x,
                        viewport.y,
                        groundPlane,
                        settings.GroundRaycastDistanceMetres,
                        out Vector3 projectedPoint))
                {
                    activeFootprintCount = 0;
                    resolvedFocus = Vector3.zero;
                    resolvedRadius = 0f;
                    return false;
                }

                Vector2 point = new Vector2(
                    projectedPoint.x,
                    projectedPoint.z);
                Vector2 focus = new Vector2(
                    projectedFocus.x,
                    projectedFocus.z);
                Vector2 outward = point - focus;
                if (outward.sqrMagnitude > 0.000001f)
                {
                    point += outward.normalized *
                        settings.OffscreenMarginMetres;
                }

                activeFootprint[activeFootprintCount++] = point;
            }

            Vector2 translation = Vector2.zero;
            if (settings.FocusOverride != null)
            {
                translation = new Vector2(
                    settings.FocusOverride.position.x - projectedFocus.x,
                    settings.FocusOverride.position.z - projectedFocus.z);
            }

            resolvedFocus = new Vector3(
                projectedFocus.x + translation.x,
                groundReference.y,
                projectedFocus.z + translation.y);
            Vector2 resolvedFocusXZ = new Vector2(
                resolvedFocus.x,
                resolvedFocus.z);
            for (int index = 0; index < activeFootprintCount; index++)
            {
                activeFootprint[index] += translation;
                maximumDistance = Mathf.Max(
                    maximumDistance,
                    Vector2.Distance(
                        activeFootprint[index],
                        resolvedFocusXZ));
            }

            useFootprintRegion = true;
            resolvedRadius = maximumDistance;
            UpdateFootprintBounds();
            return true;
        }

        private bool TryResolveGroundReference(
            Camera camera,
            in Settings settings,
            out Vector3 groundReference)
        {
            if (TryRaycastViewportGround(
                    camera,
                    0.5f,
                    0.5f,
                    settings,
                    out RaycastHit centreHit))
            {
                groundReference = centreHit.point;
                return true;
            }

            for (int index = 0; index < CameraViewportSamples.Length; index++)
            {
                Vector2 viewport = CameraViewportSamples[index];
                if (TryRaycastViewportGround(
                        camera,
                        viewport.x,
                        viewport.y,
                        settings,
                        out RaycastHit sampleHit))
                {
                    groundReference = sampleHit.point;
                    return true;
                }
            }

            groundReference = Vector3.zero;
            return false;
        }

        private static bool TryProjectViewportToPlane(
            Camera camera,
            float viewportX,
            float viewportY,
            in Plane plane,
            float maximumDistance,
            out Vector3 projectedPoint)
        {
            Ray ray = camera.ViewportPointToRay(
                new Vector3(viewportX, viewportY, 0f));
            if (!plane.Raycast(ray, out float distance) ||
                distance < 0f ||
                distance > maximumDistance)
            {
                projectedPoint = Vector3.zero;
                return false;
            }

            projectedPoint = ray.GetPoint(distance);
            return IsFinite(projectedPoint);
        }

        private static bool IsFinite(Vector3 value)
        {
            return !float.IsNaN(value.x) &&
                !float.IsInfinity(value.x) &&
                !float.IsNaN(value.y) &&
                !float.IsInfinity(value.y) &&
                !float.IsNaN(value.z) &&
                !float.IsInfinity(value.z);
        }

        private void UpdateFootprintBounds()
        {
            activeMinimumX = float.PositiveInfinity;
            activeMaximumX = float.NegativeInfinity;
            activeMinimumZ = float.PositiveInfinity;
            activeMaximumZ = float.NegativeInfinity;
            for (int index = 0; index < activeFootprintCount; index++)
            {
                Vector2 point = activeFootprint[index];
                activeMinimumX = Mathf.Min(activeMinimumX, point.x);
                activeMaximumX = Mathf.Max(activeMaximumX, point.x);
                activeMinimumZ = Mathf.Min(activeMinimumZ, point.y);
                activeMaximumZ = Mathf.Max(activeMaximumZ, point.y);
            }
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
                settings.GroundRaycastDistanceMetres,
                settings.GroundMask,
                QueryTriggerInteraction.Ignore);
        }

        private void UpdateCellBounds(
            float minimumSpacingMetres,
            int populationSeed)
        {
            float cellSize = Mathf.Max(0.5f, minimumSpacingMetres);
            int newMinimumX = Mathf.FloorToInt(
                activeMinimumX / cellSize);
            int newMaximumX = Mathf.FloorToInt(
                activeMaximumX / cellSize);
            int newMinimumZ = Mathf.FloorToInt(
                activeMinimumZ / cellSize);
            int newMaximumZ = Mathf.FloorToInt(
                activeMaximumZ / cellSize);
            cellsInActiveRegion = Mathf.Max(
                0,
                newMaximumX - newMinimumX + 1) *
                Mathf.Max(0, newMaximumZ - newMinimumZ + 1);

            bool regionChanged = !traversalInitialized ||
                populationSeed != traversalSeed ||
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

            BeginCellTraversal(
                populationSeed,
                traversalInitialized);
        }

        private void BeginCellTraversal(
            int populationSeed,
            bool advanceEpoch)
        {
            if (advanceEpoch)
            {
                turnoverEpoch++;
            }

            traversalSeed = populationSeed;
            traversalCursor = 0;
            traversalCellCount = cellsInActiveRegion;
            if (traversalCellCount <= 0)
            {
                traversalOffset = 0;
                traversalStep = 1;
                traversalInitialized = false;
                return;
            }

            ulong epochHash = MixHash(
                unchecked((ulong)(uint)populationSeed) ^
                MixHash(turnoverEpoch + 0x9E3779B97F4A7C15UL));
            traversalOffset = (int)(epochHash %
                unchecked((ulong)traversalCellCount));
            traversalStep = ResolvePermutationStep(
                traversalCellCount,
                MixHash(epochHash + 0xD1B54A32D192ED03UL));
            traversalInitialized = true;
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
                BeginRetirement(
                    controller,
                    ref candidateToRetire,
                    now,
                    "Automatic budget reduced");
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

                if (candidate.SpawnedAt > 0.0 &&
                    candidate.AssignedLifetimeSeconds > 0f &&
                    now - candidate.SpawnedAt >=
                        candidate.AssignedLifetimeSeconds)
                {
                    BeginRetirement(
                        controller,
                        ref candidate,
                        now,
                        "Lifetime expired");
                    candidates[index] = candidate;
                    continue;
                }

                if (!IsInsideActiveRegion(candidate.GroundPosition))
                {
                    if (candidate.InvalidSince <= 0.0)
                    {
                        candidate.InvalidSince = now;
                    }
                    else if (now - candidate.InvalidSince >=
                        settings.InvalidGraceDurationSeconds)
                    {
                        BeginRetirement(
                            controller,
                            ref candidate,
                            now,
                            "Outside camera footprint");
                    }

                    candidates[index] = candidate;
                    continue;
                }

                candidate.InvalidSince = 0.0;
                UpdateActiveOpening(
                    controller,
                    settings,
                    ref candidate);
                candidates[index] = candidate;
            }
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
                long identity = ResolveStableIdentity(
                    settings.Seed,
                    cellX,
                    cellZ,
                    settings.DirectionalSource.Kind);
                long activationIdentity = ResolveActivationIdentity(
                    identity,
                    turnoverEpoch);
                Vector3 candidatePosition = ResolveCandidatePosition(
                    activationIdentity,
                    cellX,
                    cellZ,
                    settings.MinimumSpacingMetres);
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
                        activationIdentity) ||
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

                float assignedLifetimeSeconds = ResolveAssignedLifetime(
                    activationIdentity,
                    settings.MinimumRayLifetimeSeconds,
                    settings.MaximumRayLifetimeSeconds);
                if (!TryEvaluateCloudPlacement(
                        settings,
                        groundHit.point,
                        assignedLifetimeSeconds,
                        out float clearance,
                        out float presentCentreOpenness,
                        out int presentSurroundingPassCount,
                        out float predictedCentreOpenness,
                        out float predictionHorizonSeconds,
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

                Candidate candidate = new Candidate
                {
                    Occupied = true,
                    Identity = identity,
                    ActivationIdentity = activationIdentity,
                    CellX = cellX,
                    CellZ = cellZ,
                    GroundPosition = groundHit.point,
                    State = WeatherLightRayPopulationCandidateState.Pending,
                    Handle = default,
                    Clearance = clearance,
                    InvalidSince = 0.0,
                    CooldownUntil = 0.0,
                    SpawnedAt = 0.0,
                    AssignedLifetimeSeconds = assignedLifetimeSeconds,
                    PresentCentreOpenness = presentCentreOpenness,
                    PresentSurroundingPassCount =
                        presentSurroundingPassCount,
                    PredictedCentreOpenness = predictedCentreOpenness,
                    PredictionHorizonSeconds = predictionHorizonSeconds,
                    RetirementReason = string.Empty,
                    CloudDataVersion = dataVersion
                };
                if (!SpawnCandidate(
                        controller,
                        settings,
                        ref candidate,
                        now))
                {
                    Reject(RejectionReason.SpawnOrUpdateFailed);
                    EnterCooldown(ref candidate, now);
                }

                candidates[freeCandidateIndex] = candidate;
            }
        }

        private bool TryAcquireGround(
            in Settings settings,
            Vector3 candidatePosition,
            out RaycastHit hit)
        {
            float halfDistance = settings.GroundRaycastDistanceMetres * 0.5f;
            Vector3 origin = new Vector3(
                candidatePosition.x,
                focusWorld.y + halfDistance,
                candidatePosition.z);
            groundRaycastsLastTick++;
            if (!Physics.Raycast(
                    origin,
                    Vector3.down,
                    out hit,
                    settings.GroundRaycastDistanceMetres,
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

        private bool TryEvaluateCloudPlacement(
            in Settings settings,
            Vector3 centre,
            float assignedLifetimeSeconds,
            out float minimumOpenness,
            out float presentCentreOpenness,
            out int presentSurroundingPassCount,
            out float predictedCentreOpenness,
            out float predictionHorizonSeconds,
            out uint dataVersion)
        {
            minimumOpenness = 0f;
            presentCentreOpenness = 0f;
            presentSurroundingPassCount = 0;
            predictedCentreOpenness = 0f;
            predictionHorizonSeconds = 0f;
            dataVersion = settings.CloudController != null
                ? unchecked((uint)settings.CloudController.CurrentCookieSeed)
                : 0u;

            if (settings.CloudController == null ||
                settings.DirectionalSource.SourceLight == null)
            {
                Reject(RejectionReason.CloudUnavailable);
                return false;
            }

            if (settings.CloudController.EvolutionInProgress &&
                settings.CloudController.EvolutionProgress <
                    settings.CloudEvolutionResumeThreshold)
            {
                Reject(RejectionReason.CloudEvolutionUnstable);
                return false;
            }

            if (!TrySampleNormalizedOpenness(
                    settings,
                    centre,
                    0f,
                    out presentCentreOpenness))
            {
                return false;
            }

            minimumOpenness = presentCentreOpenness;
            if (presentCentreOpenness < settings.MinimumClearance)
            {
                Reject(RejectionReason.InsufficientPresentCentre);
                return false;
            }

            float radius = settings.ActivePreset.DefaultAreaDiameterMetres *
                0.5f;
            for (int sampleIndex = 0; sampleIndex < 4; sampleIndex++)
            {
                float angle = sampleIndex * 90f * Mathf.Deg2Rad;
                Vector3 samplePosition = centre + new Vector3(
                    Mathf.Cos(angle) * radius,
                    0f,
                    Mathf.Sin(angle) * radius);
                if (!TrySampleNormalizedOpenness(
                        settings,
                        samplePosition,
                        0f,
                        out float surroundingOpenness))
                {
                    return false;
                }

                if (surroundingOpenness >= settings.MinimumClearance)
                {
                    presentSurroundingPassCount++;
                }
            }

            if (presentSurroundingPassCount <
                RequiredPresentSurroundingPassCount)
            {
                Reject(RejectionReason.InsufficientPresentSurrounding);
                return false;
            }

            float lifetime = Mathf.Max(0.1f, assignedLifetimeSeconds);
            float spawnFade = Mathf.Clamp(
                settings.SpawnFadeDurationSeconds,
                0f,
                lifetime);
            float stableWindow = Mathf.Max(0f, lifetime - spawnFade);
            predictionHorizonSeconds = Mathf.Min(
                lifetime,
                spawnFade +
                    PredictionStableWindowFraction * stableWindow);

            if (!TrySampleNormalizedOpenness(
                    settings,
                    centre,
                    predictionHorizonSeconds,
                    out predictedCentreOpenness))
            {
                return false;
            }

            minimumOpenness = Mathf.Min(
                minimumOpenness,
                predictedCentreOpenness);
            if (predictedCentreOpenness < settings.MinimumClearance)
            {
                Reject(RejectionReason.InsufficientPredictedCentre);
                return false;
            }

            return true;
        }

        private bool TrySampleNormalizedOpenness(
            in Settings settings,
            Vector3 samplePosition,
            float futureSeconds,
            out float normalizedOpenness)
        {
            normalizedOpenness = 0f;
            cloudSamplesLastTick++;
            if (!settings.CloudController.
                    TrySampleCloudTransmissionAtTimeOffset(
                        samplePosition,
                        settings.DirectionalSource.SourceLight,
                        futureSeconds,
                        out WeatherCloudTransmissionSample sample) ||
                !sample.IsUsable)
            {
                Reject(RejectionReason.CloudUnavailable);
                return false;
            }

            if (!sample.IsStable &&
                settings.CloudController.EvolutionProgress <
                    settings.CloudEvolutionResumeThreshold)
            {
                Reject(RejectionReason.CloudEvolutionUnstable);
                return false;
            }

            float shaded = settings.CloudController.ShadedTransmission;
            normalizedOpenness = Mathf.Clamp01(
                (sample.Transmission - shaded) /
                Mathf.Max(0.0001f, 1f - shaded));
            return true;
        }

        private bool SpawnCandidate(
            WeatherLightRayController controller,
            in Settings settings,
            ref Candidate candidate,
            double now)
        {
            WeatherLightRayCloudOpening opening = BuildOpening(
                settings,
                candidate);
            WeatherLightRayCloudSpawnSettings spawnSettings =
                BuildSpawnSettings(
                    settings,
                    candidate.ActivationIdentity);
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
            candidate.SpawnedAt = now;
            candidate.RetirementReason = string.Empty;
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
                BuildSpawnSettings(
                    settings,
                    candidate.ActivationIdentity);
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
                    Time.realtimeSinceStartupAsDouble,
                    "Opening update failed");
                return;
            }

            candidate.Handle = handle;
        }

        private static WeatherLightRayCloudOpening BuildOpening(
            in Settings settings,
            in Candidate candidate)
        {
            return new WeatherLightRayCloudOpening(
                candidate.ActivationIdentity,
                settings.DirectionalSource.Kind,
                candidate.GroundPosition,
                Vector3.zero,
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
                fadeInDurationSeconds: settings.SpawnFadeDurationSeconds,
                holdDurationSeconds: 0f,
                fadeOutDurationSeconds: settings.DespawnFadeDurationSeconds,
                initiallyVisible: true,
                runtimeCloudPolicy: WeatherLightRayCloudPolicy.IgnoreClouds,
                sourceGatePolicy:
                    WeatherLightRaySourceGatePolicy.RequireActiveSource,
                movementPolicy: WeatherLightRayMovementPolicy.Static,
                gameplayChannel: 0,
                priority: WeatherLightRaySpawnPriority.Low,
                resetLifecycleOnUpdate: false);
        }

        private void BeginRetirement(
            WeatherLightRayController controller,
            ref Candidate candidate,
            double now,
            string reason = "Invalidated")
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
                candidate.RetirementReason = reason;
                return;
            }

            EnterCooldown(ref candidate, now);
        }

        private void EnterCooldown(
            ref Candidate candidate,
            double now)
        {
            candidate.State = WeatherLightRayPopulationCandidateState.Cooldown;
            candidate.Handle = default;
            candidate.InvalidSince = 0.0;
            candidate.SpawnedAt = 0.0;
            candidate.CooldownUntil = now + replacementDelaySeconds;
        }

        private void RetireAllAutomaticCandidates(
            WeatherLightRayController controller,
            double now,
            string reason)
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
                    BeginRetirement(
                        controller,
                        ref candidate,
                        now,
                        reason);
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
            Vector2 point = new Vector2(position.x, position.z);
            if (point.x < activeMinimumX || point.x > activeMaximumX ||
                point.y < activeMinimumZ || point.y > activeMaximumZ)
            {
                return false;
            }

            bool inside = false;
            int previous = activeFootprintCount - 1;
            for (int current = 0; current < activeFootprintCount; current++)
            {
                Vector2 a = activeFootprint[current];
                Vector2 b = activeFootprint[previous];
                bool crosses = (a.y > point.y) != (b.y > point.y) &&
                    point.x < (b.x - a.x) * (point.y - a.y) /
                        (b.y - a.y) + a.x;
                if (crosses)
                {
                    inside = !inside;
                }

                previous = current;
            }

            return inside;
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
            if (!traversalInitialized || traversalCellCount <= 0)
            {
                return false;
            }

            if (traversalCursor >= traversalCellCount)
            {
                BeginCellTraversal(
                    traversalSeed,
                    advanceEpoch: true);
                if (!traversalInitialized)
                {
                    return false;
                }
            }

            long permutedIndex =
                (traversalOffset +
                    (long)traversalCursor * traversalStep) %
                traversalCellCount;
            traversalCursor++;

            int width = maximumCellX - minimumCellX + 1;
            if (width <= 0)
            {
                return false;
            }

            cellX = minimumCellX + (int)(permutedIndex % width);
            cellZ = minimumCellZ + (int)(permutedIndex / width);
            return true;
        }

        private static int ResolvePermutationStep(
            int cellCount,
            ulong hash)
        {
            if (cellCount <= 1)
            {
                return 1;
            }

            int step = 1 + (int)(hash %
                unchecked((ulong)(cellCount - 1)));
            while (GreatestCommonDivisor(step, cellCount) != 1)
            {
                step++;
                if (step >= cellCount)
                {
                    step = 1;
                }
            }

            return step;
        }

        private static int GreatestCommonDivisor(int left, int right)
        {
            left = Mathf.Abs(left);
            right = Mathf.Abs(right);
            while (right != 0)
            {
                int remainder = left % right;
                left = right;
                right = remainder;
            }

            return Mathf.Max(1, left);
        }

        private static Vector3 ResolveCandidatePosition(
            long activationIdentity,
            int cellX,
            int cellZ,
            float cellSize)
        {
            ulong hash = MixHash(
                unchecked((ulong)activationIdentity) ^
                0xA24BAED4963EE407UL);
            float offsetX = 0.15f + HashToUnitFloat(hash) * 0.70f;
            hash = MixHash(hash + 0x9E3779B97F4A7C15UL);
            float offsetZ = 0.15f + HashToUnitFloat(hash) * 0.70f;
            return new Vector3(
                (cellX + offsetX) * cellSize,
                0f,
                (cellZ + offsetZ) * cellSize);
        }

        private static float ResolveAssignedLifetime(
            long identity,
            float minimumSeconds,
            float maximumSeconds)
        {
            float minimum = Mathf.Max(0.1f, minimumSeconds);
            float maximum = Mathf.Max(minimum, maximumSeconds);
            ulong hash = MixHash(unchecked((ulong)identity) ^
                0xD6E8FEB86659FD93UL);
            return Mathf.Lerp(minimum, maximum, HashToUnitFloat(hash));
        }

        private static long ResolveStableIdentity(
            int seed,
            int cellX,
            int cellZ,
            WeatherLightRaySourceKind sourceKind)
        {
            ulong value = unchecked((ulong)(uint)seed);
            value ^= unchecked((ulong)(uint)cellX) *
                0x9E3779B185EBCA87UL;
            value ^= unchecked((ulong)(uint)cellZ) *
                0xC2B2AE3D27D4EB4FUL;
            value ^= (ulong)sourceKind + 1UL;
            value = MixHash(value) | 0x4000000000000000UL;
            long identity = unchecked((long)value);
            return identity == 0L ? 1L : identity;
        }

        private static long ResolveActivationIdentity(
            long cellIdentity,
            ulong epoch)
        {
            ulong value = unchecked((ulong)cellIdentity);
            value ^= MixHash(epoch + 0x94D049BB133111EBUL);
            value = MixHash(value) | 0x2000000000000000UL;
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
