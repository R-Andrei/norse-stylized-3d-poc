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
        public bool EmitImpact(
            Vector3 worldPosition,
            float radius,
            float strength,
            float geometryContribution = 1f,
            float normalContribution = 1f)
        {
            return EmitImpact(
                worldPosition,
                ImpactRippleEventSettings.CreateLegacy(
                    radius,
                    strength,
                    geometryContribution,
                    normalContribution));
        }

        public bool EmitImpact(
            Vector3 worldPosition,
            ImpactRippleEventSettings eventSettings)
        {
            if (river == null ||
                !river.isActiveAndEnabled ||
                !river.RuntimeDisturbancesEnabled ||
                river.LiquidFactor <= 0.0001f ||
                !river.TryProjectWorldPoint(
                    worldPosition,
                    out StylizedRiverProjection projection) ||
                !projection.IsInside)
            {
                return false;
            }

            ImpactRippleEventSettings sanitized =
                eventSettings.Sanitized();
            StylizedRiverSplineSample sample =
                river.SampleAtLocalDistance(projection.LocalDistance);
            float surfaceHalfWidth = Mathf.Max(
                0.05f,
                sample.GetSurfaceHalfWidth(projection.AcrossMetres));
            Vector3 projectedSurfacePosition =
                projection.SurfacePoint +
                projection.Side * projection.AcrossMetres;

            pendingImpacts.Add(
                new ImpactCommand
                {
                    Distance = projection.GlobalDistance,
                    AcrossNormalized = Mathf.Clamp(
                        projection.AcrossMetres / surfaceHalfWidth,
                        -1f,
                        1f),
                    WorldPositionXZ = new Vector2(
                        projectedSurfacePosition.x,
                        projectedSurfacePosition.z),
                    Radius = sanitized.Radius,
                    SignedImpulse = sanitized.SignedImpulse,
                    InitialElevation = sanitized.InitialElevation,
                    Shape = sanitized.Shape,
                    Sharpness = sanitized.Sharpness,
                    GeometryContribution =
                        sanitized.GeometryContribution,
                    NormalContribution =
                        sanitized.NormalContribution
                });

            lastActivityTime = Time.realtimeSinceStartupAsDouble;
            return true;
        }

        public bool EmitDebugImpact(
            float distanceNormalized,
            float acrossNormalized,
            ImpactRippleEventSettings eventSettings)
        {
            if (!TryResolveDebugImpactWorldPosition(
                    distanceNormalized,
                    acrossNormalized,
                    out Vector3 worldPosition))
            {
                return false;
            }

            return EmitImpact(worldPosition, eventSettings);
        }

        public bool EmitDebugOppositeSignImpact(
            float distanceNormalized,
            float acrossNormalized,
            ImpactRippleEventSettings eventSettings)
        {
            return EmitDebugImpact(
                distanceNormalized,
                acrossNormalized,
                eventSettings.WithSignsReversed());
        }

        public bool EmitDebugOverlappingPair(
            float distanceNormalized,
            float acrossNormalized,
            ImpactRippleEventSettings eventSettings)
        {
            if (river == null || !river.Domain.IsValid)
            {
                return false;
            }

            float localDistance =
                river.Domain.LocalLength *
                Mathf.Clamp01(distanceNormalized);
            StylizedRiverSplineSample sample =
                river.Domain.SampleAtLocalDistance(localDistance);
            float baseHalfWidth =
                acrossNormalized < 0f
                    ? sample.LeftSurfaceHalfWidth
                    : sample.RightSurfaceHalfWidth;
            float baseAcrossMetres =
                Mathf.Clamp(acrossNormalized, -1f, 1f) *
                Mathf.Max(0.05f, baseHalfWidth);
            float availableLeft =
                Mathf.Max(0f, sample.LeftSurfaceHalfWidth + baseAcrossMetres);
            float availableRight =
                Mathf.Max(0f, sample.RightSurfaceHalfWidth - baseAcrossMetres);
            float maximumOffset =
                Mathf.Max(0f, Mathf.Min(availableLeft, availableRight) - 0.05f);
            float offset = Mathf.Min(
                eventSettings.Sanitized().Radius * 0.45f,
                maximumOffset * 0.5f);

            if (offset <= 0.001f)
            {
                return EmitDebugImpact(
                    distanceNormalized,
                    acrossNormalized,
                    eventSettings);
            }

            Vector3 leftPosition =
                sample.SurfacePoint +
                sample.Side * (baseAcrossMetres - offset);
            Vector3 rightPosition =
                sample.SurfacePoint +
                sample.Side * (baseAcrossMetres + offset);

            bool leftEmitted = EmitImpact(leftPosition, eventSettings);
            bool rightEmitted = EmitImpact(rightPosition, eventSettings);
            return leftEmitted || rightEmitted;
        }

        public bool EmitDebugNearShore(
            float distanceNormalized,
            float acrossNormalized,
            ImpactRippleEventSettings eventSettings)
        {
            float side = acrossNormalized < 0f ? -0.82f : 0.82f;
            return EmitDebugImpact(
                distanceNormalized,
                side,
                eventSettings);
        }

        private bool TryResolveDebugImpactWorldPosition(
            float distanceNormalized,
            float acrossNormalized,
            out Vector3 worldPosition)
        {
            worldPosition = default;
            if (river == null || !river.Domain.IsValid)
            {
                return false;
            }

            float localDistance =
                river.Domain.LocalLength *
                Mathf.Clamp01(distanceNormalized);
            StylizedRiverSplineSample sample =
                river.Domain.SampleAtLocalDistance(localDistance);
            float clampedAcross = Mathf.Clamp(
                acrossNormalized,
                -0.95f,
                0.95f);
            float halfWidth =
                clampedAcross < 0f
                    ? sample.LeftSurfaceHalfWidth
                    : sample.RightSurfaceHalfWidth;
            float acrossMetres =
                clampedAcross * Mathf.Max(0.05f, halfWidth);
            worldPosition =
                sample.SurfacePoint +
                sample.Side * acrossMetres;
            return true;
        }

        private ImpactReservation CreateImpactReservation(
            ImpactCommand impact,
            double now)
        {
            float resolvedStrength =
                river.ResolvedImpactRippleStrength;
            float geometryContribution =
                Mathf.Clamp01(impact.GeometryContribution);
            float normalContribution =
                Mathf.Clamp01(impact.NormalContribution);
            float ridgeReservationScale = Mathf.Max(
                1f,
                river.ImpactRippleRidgeEmphasis);
            float impulseMagnitude =
                Mathf.Abs(impact.SignedImpulse) *
                resolvedStrength *
                Mathf.Max(geometryContribution, normalContribution) *
                ridgeReservationScale;
            float elevationMagnitude =
                Mathf.Abs(impact.InitialElevation) *
                resolvedStrength *
                geometryContribution /
                0.028f;
            float initialMagnitude = Mathf.Max(
                0.0001f,
                Mathf.Max(impulseMagnitude, elevationMagnitude));
            float minimumVisibleEnergy = Mathf.Max(
                0.0001f,
                river.ImpactRippleMinimumVisibleEnergy);
            float effectiveDecay =
                river.ResolvedImpactRippleDecay;
            float analyticLifetime = initialMagnitude > minimumVisibleEnergy
                ? Mathf.Log(initialMagnitude / minimumVisibleEnergy) /
                  effectiveDecay
                : MinimumImpactReservationLifetime;
            float maximumLifetime = Mathf.Max(
                MinimumImpactReservationLifetime,
                river.ImpactRippleMaximumLifetime);
            float lifetime = Mathf.Clamp(
                analyticLifetime,
                MinimumImpactReservationLifetime,
                maximumLifetime);
            float initialRadius = Mathf.Max(
                ImpactRippleEventSettings.MinimumRadius,
                impact.Radius * RippleInjectionEnvelopeRadius);

            return new ImpactReservation
            {
                EndTime = now + lifetime,
                AgeSeconds = 0f,
                MinimumLifetime = MinimumImpactReservationLifetime,
                MaximumLifetime = maximumLifetime,
                CurrentDistance = impact.Distance,
                CurrentRadius = initialRadius,
                CurrentMagnitude = initialMagnitude,
                MinimumReservedDistance =
                    impact.Distance - initialRadius,
                MaximumReservedDistance =
                    impact.Distance + initialRadius
            };
        }

        private float ResolveImpactReservationLookAhead(
            float deltaTime)
        {
            float updateInterval = 1f / Mathf.Max(
                1f,
                ResolveSimulationRate());
            return Mathf.Max(deltaTime, updateInterval) *
                   RippleReservationLookAheadSteps;
        }

        private void UpdateImpactReservations(
            double now,
            float simulationDeltaTime,
            float lookAhead)
        {
            for (int index = activeImpactReservations.Count - 1;
                 index >= 0;
                 index--)
            {
                ImpactReservation reservation =
                    activeImpactReservations[index];
                if (!UpdateImpactReservation(
                        ref reservation,
                        now,
                        simulationDeltaTime,
                        lookAhead))
                {
                    activeImpactReservations.RemoveAt(index);
                    continue;
                }

                activeImpactReservations[index] = reservation;
            }
        }

        private bool UpdateImpactReservation(
            ref ImpactReservation reservation,
            double now,
            float simulationDeltaTime,
            float lookAhead)
        {
            float elapsed = Mathf.Max(0f, simulationDeltaTime);
            float propagationSpeed = Mathf.Max(
                0.01f,
                river.ImpactRipplePropagation);
            float advectionSpeed = Mathf.Abs(
                river.FlowSpeedMetresPerSecond);
            float effectiveDecay =
                river.ResolvedImpactRippleDecay;
            float minimumVisibleEnergy = Mathf.Max(
                0.0001f,
                river.ImpactRippleMinimumVisibleEnergy);

            reservation.AgeSeconds += elapsed;
            reservation.CurrentDistance += advectionSpeed * elapsed;
            reservation.CurrentRadius += propagationSpeed * elapsed;
            reservation.CurrentMagnitude *= Mathf.Exp(
                -effectiveDecay * elapsed);

            bool minimumLifetimeElapsed =
                reservation.AgeSeconds >= reservation.MinimumLifetime;
            if (reservation.AgeSeconds >= reservation.MaximumLifetime ||
                (minimumLifetimeElapsed &&
                 reservation.CurrentMagnitude <= minimumVisibleEnergy))
            {
                return false;
            }

            float remainingAnalyticLifetime =
                reservation.CurrentMagnitude > minimumVisibleEnergy
                    ? Mathf.Log(
                        reservation.CurrentMagnitude /
                        minimumVisibleEnergy) /
                      effectiveDecay
                    : 0f;
            float minimumRemainingLifetime = Mathf.Max(
                0f,
                reservation.MinimumLifetime - reservation.AgeSeconds);
            float maximumRemainingLifetime = Mathf.Max(
                0f,
                reservation.MaximumLifetime - reservation.AgeSeconds);
            float remainingLifetime = Mathf.Clamp(
                remainingAnalyticLifetime,
                minimumRemainingLifetime,
                maximumRemainingLifetime);
            reservation.EndTime = now + remainingLifetime;

            float predictedTime = Mathf.Min(
                lookAhead,
                remainingLifetime);
            float predictedCentre =
                reservation.CurrentDistance +
                advectionSpeed * predictedTime;
            float predictedRadius =
                reservation.CurrentRadius +
                propagationSpeed * predictedTime;
            float padding = ResolveRippleReservationPaddingMetres(
                predictedCentre);

            reservation.MinimumReservedDistance = Mathf.Min(
                reservation.MinimumReservedDistance,
                predictedCentre - predictedRadius - padding);
            reservation.MaximumReservedDistance = Mathf.Max(
                reservation.MaximumReservedDistance,
                predictedCentre + predictedRadius + padding);

            MarkActiveInterval(
                reservation.MinimumReservedDistance,
                reservation.MaximumReservedDistance,
                reservation.EndTime,
                now);
            return true;
        }

        private float ResolveRippleReservationPaddingMetres(
            float globalDistance)
        {
            if (rippleMetricMinimumAlongCell.Length == 0 ||
                fieldWidth <= 1)
            {
                return 0.25f;
            }

            int row = Mathf.Clamp(
                Mathf.RoundToInt(GlobalDistanceToPixel(globalDistance)),
                0,
                rippleMetricMinimumAlongCell.Length - 1);
            float cellSize = Mathf.Max(
                rippleMetricMinimumAlongCell[row],
                rippleMetricMinimumLateralCell.Length > row
                    ? rippleMetricMinimumLateralCell[row]
                    : 0f);
            return Mathf.Max(
                0.05f,
                cellSize * RippleReservationPaddingCells);
        }

        private void ResetRippleChunkReservationDeadlines(double now)
        {
            for (int chunk = 0; chunk < chunkActiveUntil.Length; chunk++)
            {
                if (chunkActive[chunk])
                {
                    chunkActiveUntil[chunk] = now;
                }
            }
        }

        private float ResolveLongestImpactReservationRemainingSeconds()
        {
            if (activeImpactReservations.Count == 0)
            {
                return 0f;
            }

            double now = Time.realtimeSinceStartupAsDouble;
            double longest = 0.0;
            for (int index = 0;
                 index < activeImpactReservations.Count;
                 index++)
            {
                longest = Math.Max(
                    longest,
                    activeImpactReservations[index].EndTime - now);
            }

            return Mathf.Max(0f, (float)longest);
        }
    }
}
