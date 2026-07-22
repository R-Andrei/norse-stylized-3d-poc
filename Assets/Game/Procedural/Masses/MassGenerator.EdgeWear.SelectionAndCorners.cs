using System;
using System.Collections.Generic;
using UnityEngine;
using ProgrammaticStylized3D.Geometry;

namespace ProgrammaticStylized3D.Geometry.Masses
{
    public static partial class MassGenerator
    {
        #region Edge wear selection, topology, and corner solving


        private const float EdgeWearMinimumViableDihedralDegrees = 15f;
        private const float EdgeWearMinimumFootprintLengthMultiplier = 2f;
        private const float EdgeWearMinimumFeasibleWidthFraction = 0.25f;
        private const float EdgeWearMaterialWidthRecoveryFootprintMultiplier =
            2f;
        private const float EdgeWearMinimumStyleWidthSetting = 0.05f;
        private const float EdgeWearMinimumCentralSpanWidthMultiplier = 0.5f;
        private const float EdgeWearMacroMinimumSampledMultiplier = 0.55f;
        private const float EdgeWearMacroMaximumCertifiedStrength = 0.55f;
        private const float EdgeWearMacroShallowAngleDegrees =
            EdgeWearMinimumViableDihedralDegrees;
        private const float EdgeWearMacroSharpAngleDegrees = 90f;
        private const float EdgeWearMacroSharpReductionPermission = 0.35f;
        private const int EdgeWearMacroVariationSalt = 0x6D31;
        private const int EdgeWearMacroParticipationSalt = 0x4B17;
        private const int CornerDamageSelectionSalt = 0x2C51;
        private const int CornerDamageDepthSalt = 0x57A3;
        private const int CornerDamageIdentitySalt = 0x6E19;
        private static readonly float[] CornerDamageDepthTrialFactors =
        {
            1f,
            0.75f,
            0.5625f,
            0.421875f
        };

        private static float ResolveEdgeWearMacroAnglePermission(
            float dihedralDegrees)
        {
            if (float.IsNaN(dihedralDegrees) ||
                float.IsInfinity(dihedralDegrees))
            {
                return 1f;
            }

            float angle01 = Mathf.InverseLerp(
                EdgeWearMacroShallowAngleDegrees,
                EdgeWearMacroSharpAngleDegrees,
                dihedralDegrees);
            float sharpness = Mathf.SmoothStep(0f, 1f, angle01);
            return Mathf.Lerp(
                1f,
                EdgeWearMacroSharpReductionPermission,
                sharpness);
        }

        private static void ApplyResolvedEdgeWearMacroWidth(
            EdgeWearEdgeViabilityRecord viability,
            float edgeLength,
            float footprintGuard,
            float participationIdentity01,
            bool participates,
            float identity01,
            float sampledMultiplier,
            float effectiveMultiplier,
            float variedRequestedWidth,
            bool minimumStyleClamped)
        {
            viability.MacroParticipationIdentity01 =
                participationIdentity01;
            viability.MacroVariationParticipates = participates;
            viability.MacroIdentity01 = identity01;
            viability.MacroSampledMultiplier = sampledMultiplier;
            viability.MacroEffectiveMultiplier = effectiveMultiplier;
            viability.MacroMinimumStyleClamped = minimumStyleClamped;
            viability.RequestedWidth = variedRequestedWidth;
            viability.RequiredFootprintLength =
                variedRequestedWidth *
                    EdgeWearMinimumFootprintLengthMultiplier +
                footprintGuard;
            viability.LengthToWidthRatio = variedRequestedWidth > 0f
                ? edgeLength / variedRequestedWidth
                : 0f;
        }

        private static void ResolveEdgeWearMacroRequestedWidth(
            int shapeSeed,
            int canonicalSourceEdgeIndex,
            float macroVariationCoverage,
            float macroVariationStrength,
            float baseRequestedWidth,
            float minimumStyleWidth,
            float dihedralDegrees,
            bool generatedTransition,
            out float participationIdentity01,
            out bool participates,
            out float identity01,
            out float sampledMultiplier,
            out float effectiveMultiplier,
            out float variedRequestedWidth,
            out bool minimumStyleClamped)
        {
            int stableIdentity = Mathf.Max(0, canonicalSourceEdgeIndex);
            participationIdentity01 = generatedTransition
                ? 1f
                : Hash01(
                    unchecked(shapeSeed + EdgeWearMacroParticipationSalt),
                    stableIdentity);
            identity01 = generatedTransition
                ? 0f
                : Hash01(
                    unchecked(shapeSeed + EdgeWearMacroVariationSalt),
                    stableIdentity);
            float smoothIdentity =
                identity01 * identity01 * (3f - 2f * identity01);
            sampledMultiplier = generatedTransition
                ? 1f
                : Mathf.Lerp(
                    EdgeWearMacroMinimumSampledMultiplier,
                    1f,
                    smoothIdentity);

            float coverage = generatedTransition
                ? 0f
                : Mathf.Clamp01(macroVariationCoverage);
            float strength = generatedTransition
                ? 0f
                : Mathf.Clamp01(macroVariationStrength) *
                    EdgeWearMacroMaximumCertifiedStrength;
            participates = coverage > 0f &&
                strength > 0f &&
                (coverage >= 1f ||
                 participationIdentity01 < coverage);
            if (!participates)
            {
                effectiveMultiplier = 1f;
                variedRequestedWidth = baseRequestedWidth;
                minimumStyleClamped = false;
                return;
            }

            float anglePermission =
                ResolveEdgeWearMacroAnglePermission(dihedralDegrees);
            float requestedMultiplier = 1f -
                (1f - sampledMultiplier) *
                strength *
                anglePermission;
            float unboundedWidth =
                baseRequestedWidth * requestedMultiplier;
            variedRequestedWidth =
                Mathf.Max(minimumStyleWidth, unboundedWidth);
            minimumStyleClamped = variedRequestedWidth >
                unboundedWidth + PointMergeDistance;
            effectiveMultiplier =
                baseRequestedWidth > PointMergeDistance
                    ? variedRequestedWidth / baseRequestedWidth
                    : 1f;
        }

        private static List<EdgeWearBevelCandidate> BuildEdgeWearBevelCandidates(
            List<PolygonFace> faces,
            Bounds bounds,
            float maximumDimension,
            MassRecipe recipe,
            MassSurfaceFeatureSettings settings,
            float amount01,
            float requestedWidth,
            bool includeAllGeometricCandidates,
            EdgeWearMicroTopologyNormalizationResult
                microTopologyNormalization,
            CornerDamageTransactionAuditResult cornerDamageTransaction,
            float capRingRequestedWidth,
            out EdgeWearCoverageAudit coverageAudit)
        {
            bool maximumCoverageMode =
                settings.EdgeWearCoverage >= 2f - 0.0001f;
            coverageAudit = new EdgeWearCoverageAudit(
                maximumCoverageMode,
                includeAllGeometricCandidates)
            {
                MicroTopologyNormalization = microTopologyNormalization,
                MacroVariationCoverage =
                    settings.EdgeWearMacroVariationCoverage,
                MacroVariation = settings.EdgeWearMacroVariation,
                MacroBaseRequestedWidth = requestedWidth
            };
            System.Diagnostics.Stopwatch viabilityStopwatch =
                System.Diagnostics.Stopwatch.StartNew();

            List<EdgeWearEdgeAggregate> edges =
                new List<EdgeWearEdgeAggregate>();
            Dictionary<EdgeKey, int> edgeIndexByKey =
                new Dictionary<EdgeKey, int>();
            int coincidentBoundarySeamPairCount = 0;

            for (int faceIndex = 0; faceIndex < faces.Count; faceIndex++)
            {
                PolygonFace face = faces[faceIndex];
                if (face.Feature != PolygonFaceFeature.Base)
                {
                    continue;
                }

                for (int vertexIndex = 0;
                     vertexIndex < face.Vertices.Count;
                     vertexIndex++)
                {
                    Vector3 start = face.Vertices[vertexIndex];
                    Vector3 end = face.Vertices[
                        (vertexIndex + 1) % face.Vertices.Count];
                    if ((end - start).sqrMagnitude <= MinimumEdgeLengthSqr)
                    {
                        continue;
                    }

                    EdgeWearEdgeAggregate edge =
                        GetOrAddEdgeWearSourceEdgeAggregate(
                            edges,
                            edgeIndexByKey,
                            start,
                            end,
                            faceIndex,
                            ref coincidentBoundarySeamPairCount);
                    edge.AddFace(faceIndex);
                }
            }

            coverageAudit.RawSourceEdgeCount =
                microTopologyNormalization != null &&
                microTopologyNormalization.OriginalEdgeCount > 0
                    ? microTopologyNormalization.OriginalEdgeCount
                    : edgeIndexByKey.Count;
            AddEdgeWearMicroTopologySuppressedRecords(
                coverageAudit,
                microTopologyNormalization);
            coverageAudit.CoincidentBoundarySeamPairCount =
                coincidentBoundarySeamPairCount;

            List<EdgeWearBevelCandidate> provisionalCandidates =
                new List<EdgeWearBevelCandidate>(edges.Count);
            Vector3 solidCentre = CalculatePlaneCutFaceVertexCentre(faces);
            List<Vector3> sourceVertices =
                BuildEdgeWearViabilitySourceVertexList(faces);
            float minimumStableEdgeLength = maximumDimension * 0.0012f;
            float minimumStableFaceArea =
                maximumDimension * maximumDimension * 0.000001f;
            float minimumStyleWidth = ResolveGeneratedEdgeWearWidth(
                maximumDimension,
                EdgeWearMinimumStyleWidthSetting);
            float structuralTolerance = Mathf.Max(
                PointMergeDistance * 8f,
                maximumDimension * 0.00001f);
            float structuralMinimumLength = Mathf.Max(
                PointMergeDistance * 4f,
                maximumDimension * 0.00001f);
            float artisticMinimumLength = Mathf.Max(
                0.0001f,
                maximumDimension * 0.015f);
            float footprintGuard = Mathf.Max(
                PointMergeDistance * 4f,
                minimumStableEdgeLength * 0.01f);
            int provisionalCandidateIndex = 0;

            for (int edgeIndex = 0; edgeIndex < edges.Count; edgeIndex++)
            {
                EdgeWearEdgeAggregate edge = edges[edgeIndex];
                EdgeKey edgeKey = new EdgeKey(edge.Start, edge.End);
                Vector3 midpoint = (edge.Start + edge.End) * 0.5f;
                float length = (edge.End - edge.Start).magnitude;
                bool cornerDamageCapRing =
                    cornerDamageTransaction != null &&
                    cornerDamageTransaction.CapRingKeys.Contains(edgeKey);
                int originalSourceEdgeIndex;
                if (cornerDamageTransaction != null &&
                    cornerDamageTransaction.StableIdentityByOutputKey.
                        TryGetValue(
                            edgeKey,
                            out int committedIdentity))
                {
                    originalSourceEdgeIndex = committedIdentity;
                }
                else
                {
                    originalSourceEdgeIndex =
                        microTopologyNormalization == null
                            ? edgeIndex
                            : microTopologyNormalization.
                                ResolveOriginalSourceEdgeIndex(
                                    edgeKey,
                                    edgeIndex);
                }
                bool microGeneratedTransition =
                    !cornerDamageCapRing &&
                    microTopologyNormalization != null &&
                    microTopologyNormalization.
                        GeneratedTransitionKeys.Contains(edgeKey);
                float macroParticipationIdentity01;
                bool macroVariationParticipates;
                float macroIdentity01;
                float macroSampledMultiplier;
                float macroEffectiveMultiplier;
                float variedRequestedWidth;
                bool macroMinimumStyleClamped;
                if (cornerDamageCapRing)
                {
                    macroParticipationIdentity01 = 0f;
                    macroVariationParticipates = false;
                    macroIdentity01 = 0f;
                    macroSampledMultiplier = 1f;
                    macroEffectiveMultiplier = 1f;
                    variedRequestedWidth = capRingRequestedWidth;
                    macroMinimumStyleClamped = false;
                }
                else
                {
                    ResolveEdgeWearMacroRequestedWidth(
                        recipe.ShapeSeed,
                        originalSourceEdgeIndex,
                        settings.EdgeWearMacroVariationCoverage,
                        settings.EdgeWearMacroVariation,
                        requestedWidth,
                        minimumStyleWidth,
                        EdgeWearMacroShallowAngleDegrees,
                        microGeneratedTransition,
                        out macroParticipationIdentity01,
                        out macroVariationParticipates,
                        out macroIdentity01,
                        out macroSampledMultiplier,
                        out macroEffectiveMultiplier,
                        out variedRequestedWidth,
                        out macroMinimumStyleClamped);
                }
                EdgeWearEdgeLifecycleRecord lifecycle =
                    new EdgeWearEdgeLifecycleRecord
                    {
                        Key = edgeKey,
                        Start = edge.Start,
                        End = edge.End,
                        Midpoint = midpoint,
                        FaceCount = edge.FaceIndices.Count,
                        Length = length,
                        CoincidentBoundarySeamReconciled =
                            edge.CoincidentBoundarySeamReconciled,
                        Vertical01 = Mathf.InverseLerp(
                            bounds.min.y,
                            bounds.max.y,
                            midpoint.y),
                        OriginalSourceEdgeIndex =
                            originalSourceEdgeIndex,
                        CandidateClass = cornerDamageCapRing
                            ? EdgeWearCandidateClass.CornerDamageCapRing
                            : EdgeWearCandidateClass.Ordinary,
                        Mandatory = cornerDamageCapRing,
                        MicroTopologyGeneratedTransition =
                            microGeneratedTransition
                    };
                EdgeWearEdgeViabilityRecord viability =
                    new EdgeWearEdgeViabilityRecord
                    {
                        Key = edgeKey,
                        MinimumDihedralDegrees =
                            EdgeWearMinimumViableDihedralDegrees,
                        BaseRequestedWidth = cornerDamageCapRing
                            ? capRingRequestedWidth
                            : requestedWidth,
                        MacroVariationCoverage = cornerDamageCapRing
                            ? 0f
                            : settings.EdgeWearMacroVariationCoverage,
                        MacroVariation = cornerDamageCapRing
                            ? 0f
                            : settings.EdgeWearMacroVariation
                    };
                ApplyResolvedEdgeWearMacroWidth(
                    viability,
                    length,
                    footprintGuard,
                    macroParticipationIdentity01,
                    macroVariationParticipates,
                    macroIdentity01,
                    macroSampledMultiplier,
                    macroEffectiveMultiplier,
                    variedRequestedWidth,
                    macroMinimumStyleClamped);
                lifecycle.Viability = viability;
                coverageAudit.Records.Add(lifecycle);
                coverageAudit.RecordByKey[edgeKey] = lifecycle;
                coverageAudit.ViabilityByKey[edgeKey] = viability;

                if (lifecycle.MicroTopologyGeneratedTransition)
                {
                    lifecycle.ViabilityState =
                        EdgeWearViabilityState.StructuralIneligible;
                    lifecycle.CandidateReason =
                        "micro-topology-generated-transition";
                    lifecycle.FinalReason = lifecycle.CandidateReason;
                    viability.FailureReason = lifecycle.CandidateReason;
                    continue;
                }

                if (edge.FaceIndices.Count != 2)
                {
                    lifecycle.ViabilityState =
                        EdgeWearViabilityState.StructuralIneligible;
                    lifecycle.CandidateReason = edge.FaceIndices.Count < 2
                        ? "boundary-edge"
                        : "non-manifold-edge";
                    lifecycle.FinalReason = lifecycle.CandidateReason;
                    viability.FailureReason = lifecycle.CandidateReason;
                    continue;
                }

                int faceA = edge.FaceIndices[0];
                int faceB = edge.FaceIndices[1];
                lifecycle.FaceA = faceA;
                lifecycle.FaceB = faceB;
                PolygonFace first = faces[faceA];
                PolygonFace second = faces[faceB];
                if (first == null || second == null ||
                    !IsFinite(first.Normal) || !IsFinite(second.Normal) ||
                    first.Normal.sqrMagnitude <= MinimumEdgeLengthSqr ||
                    second.Normal.sqrMagnitude <= MinimumEdgeLengthSqr)
                {
                    lifecycle.ViabilityState =
                        EdgeWearViabilityState.StructuralIneligible;
                    lifecycle.CandidateReason = "invalid-owner-normal";
                    lifecycle.FinalReason = lifecycle.CandidateReason;
                    viability.FailureReason = lifecycle.CandidateReason;
                    continue;
                }

                Vector3 normalSum = first.Normal + second.Normal;
                if (!IsFinite(normalSum) ||
                    normalSum.sqrMagnitude <= MinimumEdgeLengthSqr)
                {
                    lifecycle.ViabilityState =
                        EdgeWearViabilityState.StructuralIneligible;
                    lifecycle.CandidateReason = "opposed-owner-normals";
                    lifecycle.FinalReason = lifecycle.CandidateReason;
                    viability.FailureReason = lifecycle.CandidateReason;
                    continue;
                }

                if (length <= structuralMinimumLength)
                {
                    lifecycle.ViabilityState =
                        EdgeWearViabilityState.StructuralIneligible;
                    lifecycle.CandidateReason = "numerically-short-edge";
                    lifecycle.FinalReason = lifecycle.CandidateReason;
                    viability.FailureReason = lifecycle.CandidateReason;
                    continue;
                }

                if (!TryClassifyEdgeWearStructuralEdge(
                        faces,
                        faceA,
                        faceB,
                        edge.Start,
                        edge.End,
                        solidCentre,
                        structuralTolerance,
                        out BoundedEdgeClassificationEvidence evidence))
                {
                    lifecycle.Classification =
                        BoundedEdgeClassification.Ambiguous;
                    lifecycle.ViabilityState =
                        EdgeWearViabilityState.StructuralIneligible;
                    lifecycle.CandidateReason =
                        "structural-classification-failed";
                    lifecycle.FinalReason = lifecycle.CandidateReason;
                    viability.FailureReason = lifecycle.CandidateReason;
                    continue;
                }

                lifecycle.Classification = evidence.Classification;
                lifecycle.DihedralDegrees = evidence.DihedralDegrees;
                if (evidence.Classification !=
                    BoundedEdgeClassification.Convex)
                {
                    lifecycle.ViabilityState =
                        EdgeWearViabilityState.StructuralIneligible;
                    lifecycle.CandidateReason =
                        "structurally-" +
                        evidence.Classification.ToString().ToLowerInvariant();
                    lifecycle.FinalReason = lifecycle.CandidateReason;
                    viability.FailureReason = lifecycle.CandidateReason;
                    continue;
                }

                if (!cornerDamageCapRing)
                {
                    ResolveEdgeWearMacroRequestedWidth(
                        recipe.ShapeSeed,
                        originalSourceEdgeIndex,
                        settings.EdgeWearMacroVariationCoverage,
                        settings.EdgeWearMacroVariation,
                        requestedWidth,
                        minimumStyleWidth,
                        evidence.DihedralDegrees,
                        microGeneratedTransition,
                        out macroParticipationIdentity01,
                        out macroVariationParticipates,
                        out macroIdentity01,
                        out macroSampledMultiplier,
                        out macroEffectiveMultiplier,
                        out variedRequestedWidth,
                        out macroMinimumStyleClamped);
                }
                ApplyResolvedEdgeWearMacroWidth(
                    viability,
                    length,
                    footprintGuard,
                    macroParticipationIdentity01,
                    macroVariationParticipates,
                    macroIdentity01,
                    macroSampledMultiplier,
                    macroEffectiveMultiplier,
                    variedRequestedWidth,
                    macroMinimumStyleClamped);

                lifecycle.StructuralEligible = true;
                Vector3 bevelNormal = normalSum.normalized;
                Vector3 edgeDirection = (edge.End - edge.Start) / length;
                lifecycle.OwnerNormalA = first.Normal;
                lifecycle.OwnerNormalB = second.Normal;
                lifecycle.BevelNormal = bevelNormal;
                float angleScore = Mathf.Clamp01(
                    (1f - Vector3.Dot(first.Normal, second.Normal)) * 0.72f);
                float baseSuppression = Mathf.SmoothStep(
                    0.06f,
                    0.20f,
                    lifecycle.Vertical01);
                bool artisticLengthEligible =
                    length > artisticMinimumLength;
                bool artisticAngleEligible = angleScore > 0.055f;
                bool artisticBaseEligible = baseSuppression > 0.001f;
                lifecycle.ArtisticMinimumLength = artisticMinimumLength;
                lifecycle.ArtisticAngleScore = angleScore;
                lifecycle.ArtisticBaseSuppression = baseSuppression;
                lifecycle.ArtisticEdgeAxisVertical01 = Mathf.Clamp01(
                    Mathf.Abs(Vector3.Dot(edgeDirection, Vector3.up)));
                lifecycle.ArtisticEdgeAxisAbsX = Mathf.Abs(edgeDirection.x);
                lifecycle.ArtisticEdgeAxisAbsY = Mathf.Abs(edgeDirection.y);
                lifecycle.ArtisticEdgeAxisAbsZ = Mathf.Abs(edgeDirection.z);
                lifecycle.ArtisticSilhouettePotential = Mathf.Clamp01(
                    Mathf.Max(
                        new Vector2(first.Normal.x, first.Normal.z).magnitude,
                        new Vector2(second.Normal.x, second.Normal.z).magnitude));
                lifecycle.ArtisticLengthEligible = artisticLengthEligible;
                lifecycle.ArtisticAngleEligible = artisticAngleEligible;
                lifecycle.ArtisticBaseEligible = artisticBaseEligible;
                lifecycle.ArtisticEligible = cornerDamageCapRing ||
                    (artisticLengthEligible &&
                     artisticAngleEligible &&
                     artisticBaseEligible);
                lifecycle.ArtisticFilterReason = lifecycle.ArtisticEligible
                    ? "eligible"
                    : ResolveEdgeWearArtisticFilterReason(
                        lifecycle,
                        artisticMinimumLength);

                viability.DihedralValid =
                    lifecycle.DihedralDegrees + 0.0001f >=
                    EdgeWearMinimumViableDihedralDegrees;
                viability.FootprintValid =
                    length + PointMergeDistance >=
                    viability.RequiredFootprintLength;

                if (!viability.DihedralValid)
                {
                    SetEdgeWearGeometricIneligibility(
                        lifecycle,
                        viability,
                        "dihedral-below-bevel-viability");
                    continue;
                }
                if (!viability.FootprintValid)
                {
                    SetEdgeWearGeometricIneligibility(
                        lifecycle,
                        viability,
                        "edge-too-short-for-bevel-footprint");
                    continue;
                }

                coverageAudit.ViabilityLocalityEvaluationCount++;
                EvaluateIndependentPlaneLocalityViability(
                    sourceVertices,
                    edge.Start,
                    edge.End,
                    bevelNormal,
                    solidCentre,
                    minimumStableEdgeLength,
                    viability);
                if (!viability.LocalityValid)
                {
                    SetEdgeWearGeometricIneligibility(
                        lifecycle,
                        viability,
                        "independent-plane-locality-infeasible");
                    continue;
                }

                float lengthScore = Mathf.Clamp01(
                    length /
                    Mathf.Max(0.0001f, maximumDimension * 0.34f));
                float upwardEdgeBoost = Mathf.Lerp(
                    0.82f,
                    1.08f,
                    Mathf.Clamp01(
                        (first.Normal.y + second.Normal.y) * 0.5f + 0.5f));
                float basePriorityFactor = Mathf.Lerp(
                    0.60f,
                    1.00f,
                    Mathf.InverseLerp(0.06f, 0.20f, baseSuppression));
                float upwardPriorityFactor = Mathf.Lerp(
                    0.925f,
                    1.075f,
                    Mathf.InverseLerp(0.82f, 1.08f, upwardEdgeBoost));
                float characterBoost = recipe.EdgeCharacter switch
                {
                    EdgeCharacter.Sharp => 1.08f,
                    EdgeCharacter.Chipped => 1.22f,
                    EdgeCharacter.Worn => 0.86f,
                    EdgeCharacter.Polished => 0.62f,
                    _ => 1f
                };
                float random = HashPosition01(
                    settings.SurfaceSeed + 0x4A17,
                    midpoint + bevelNormal * 0.173f);
                float score =
                    (angleScore * 0.60f +
                     lengthScore * 0.35f +
                     random * 0.05f) *
                    basePriorityFactor *
                    upwardPriorityFactor;
                lifecycle.ArtisticLengthScore = lengthScore;
                lifecycle.ArtisticUpwardEdgeBoost = upwardEdgeBoost;
                lifecycle.ArtisticCharacterBoost = characterBoost;
                lifecycle.ArtisticRandomScore = random;

                int deterministicVariationIdentity =
                    lifecycle.OriginalSourceEdgeIndex >= 0
                        ? lifecycle.OriginalSourceEdgeIndex
                        : provisionalCandidateIndex;
                float deterministicVariation = Mathf.Lerp(
                    0.90f,
                    1.08f,
                    Hash01(
                        settings.SurfaceSeed + 0x29AF,
                        deterministicVariationIdentity));
                float strength = cornerDamageCapRing
                    ? Mathf.Clamp01(
                        amount01 *
                        settings.CornerChipCapRingWearStrength)
                    : Mathf.Clamp01(
                        amount01 *
                        Mathf.Lerp(0.86f, 1.06f, random) *
                        deterministicVariation);
                float depthMultiplier = cornerDamageCapRing
                    ? 1f
                    : Mathf.Clamp(
                        Mathf.Lerp(0.88f, 1.08f, random) *
                        Mathf.Lerp(0.96f, 1.04f, angleScore),
                        0.78f,
                        1.15f);

                lifecycle.ArtisticDeterministicVariation =
                    deterministicVariation;
                lifecycle.ArtisticStrength = strength;
                lifecycle.ArtisticDepthMultiplier = depthMultiplier;
                lifecycle.Score = score;
                provisionalCandidates.Add(
                    new EdgeWearBevelCandidate(
                        provisionalCandidateIndex,
                        originalSourceEdgeIndex,
                        lifecycle.CandidateClass,
                        lifecycle.Mandatory,
                        edge.Start,
                        edge.End,
                        faceA,
                        faceB,
                        first.Normal,
                        second.Normal,
                        midpoint,
                        bevelNormal,
                        score,
                        strength,
                        depthMultiplier));
                provisionalCandidateIndex++;
            }

            if (provisionalCandidates.Count > 0)
            {
                ChamferReadinessStats preflightStats =
                    new ChamferReadinessStats(
                        provisionalCandidates.Count,
                        provisionalCandidates.Count);
                if (TryBuildChamferTopologyContext(
                        faces,
                        provisionalCandidates,
                        provisionalCandidates.Count,
                        minimumStableEdgeLength,
                        ref preflightStats,
                        out ChamferTopologyContext preflightContext,
                        out string preflightBlocker))
                {
                    MapEdgeWearCoverageAuditSourceIndices(
                        coverageAudit,
                        preflightContext.Graph);
                    RunEdgeWearIsolatedViabilityPreflight(
                        faces,
                        preflightContext,
                        requestedWidth,
                        minimumStyleWidth,
                        minimumStableEdgeLength,
                        minimumStableFaceArea,
                        coverageAudit);
                }
                else
                {
                    for (int candidateIndex = 0;
                         candidateIndex < provisionalCandidates.Count;
                         candidateIndex++)
                    {
                        EdgeWearBevelCandidate candidate =
                            provisionalCandidates[candidateIndex];
                        EdgeKey key = new EdgeKey(
                            candidate.Start,
                            candidate.End);
                        if (coverageAudit.RecordByKey.TryGetValue(
                                key,
                                out EdgeWearEdgeLifecycleRecord lifecycle))
                        {
                            SetEdgeWearGeometricIneligibility(
                                lifecycle,
                                lifecycle.Viability,
                                string.IsNullOrEmpty(preflightBlocker)
                                    ? "viability-preflight-topology-unavailable"
                                    : "viability-preflight-topology-unavailable:" +
                                        preflightBlocker);
                        }
                    }
                }
            }

            PopulateEdgeWearArtisticContextMetrics(
                coverageAudit,
                maximumDimension,
                requestedWidth);
            CaptureEdgeWearCollateralBaseline(coverageAudit);

            List<EdgeWearBevelCandidate> candidates =
                new List<EdgeWearBevelCandidate>(
                    provisionalCandidates.Count);
            for (int provisionalIndex = 0;
                 provisionalIndex < provisionalCandidates.Count;
                 provisionalIndex++)
            {
                EdgeWearBevelCandidate provisional =
                    provisionalCandidates[provisionalIndex];
                EdgeKey key = new EdgeKey(
                    provisional.Start,
                    provisional.End);
                if (!coverageAudit.RecordByKey.TryGetValue(
                        key,
                        out EdgeWearEdgeLifecycleRecord lifecycle) ||
                    !lifecycle.GeometricEligible)
                {
                    continue;
                }

                bool includeCandidate =
                    lifecycle.Mandatory ||
                    includeAllGeometricCandidates ||
                    lifecycle.ArtisticEligible;
                if (!includeCandidate)
                {
                    lifecycle.ViabilityState =
                        EdgeWearViabilityState.ViableUnselected;
                    lifecycle.CandidateReason =
                        ResolveEdgeWearArtisticFilterReason(
                            lifecycle,
                            artisticMinimumLength);
                    lifecycle.FinalReason = "viable-artistic-filtered";
                    continue;
                }

                int finalCandidateIndex = candidates.Count;
                lifecycle.Candidate = true;
                lifecycle.CandidateIndex = finalCandidateIndex;
                lifecycle.CandidateReason = lifecycle.ArtisticEligible
                    ? "eligible"
                    : ResolveEdgeWearArtisticFilterReason(
                        lifecycle,
                        artisticMinimumLength);
                lifecycle.FinalReason = "not-selected-by-coverage";
                candidates.Add(
                    new EdgeWearBevelCandidate(
                        finalCandidateIndex,
                        provisional.StableIdentity,
                        provisional.CandidateClass,
                        provisional.Mandatory,
                        provisional.Start,
                        provisional.End,
                        provisional.FaceA,
                        provisional.FaceB,
                        provisional.NormalA,
                        provisional.NormalB,
                        provisional.Midpoint,
                        provisional.BevelNormal,
                        provisional.Score,
                        provisional.Strength,
                        provisional.DepthMultiplier));
            }

            viabilityStopwatch.Stop();
            coverageAudit.ViabilityPreflightMilliseconds =
                viabilityStopwatch.Elapsed.TotalMilliseconds;
            RecalculateEdgeWearCoverageAudit(coverageAudit);
            return candidates;
        }

        private static void AddEdgeWearMicroTopologySuppressedRecords(
            EdgeWearCoverageAudit audit,
            EdgeWearMicroTopologyNormalizationResult normalization)
        {
            if (audit == null || normalization == null ||
                normalization.SuppressedEdges.Count == 0)
            {
                return;
            }

            for (int suppressedIndex = 0;
                 suppressedIndex < normalization.SuppressedEdges.Count;
                 suppressedIndex++)
            {
                EdgeWearMicroTopologySuppressedEdge suppressed =
                    normalization.SuppressedEdges[suppressedIndex];
                EdgeKey key = new EdgeKey(
                    suppressed.Start,
                    suppressed.End);
                EdgeWearEdgeViabilityRecord viability =
                    new EdgeWearEdgeViabilityRecord
                    {
                        Key = key,
                        SourceEdgeIndex = -1,
                        Evaluated = true,
                        Viable = false,
                        FailureReason = "micro-topology-suppressed"
                    };
                EdgeWearEdgeLifecycleRecord lifecycle =
                    new EdgeWearEdgeLifecycleRecord
                    {
                        Key = key,
                        SourceEdgeIndex = -1,
                        OriginalSourceEdgeIndex =
                            suppressed.OriginalSourceEdgeIndex,
                        MicroTopologySuppressed = true,
                        Start = suppressed.Start,
                        End = suppressed.End,
                        Midpoint =
                            (suppressed.Start + suppressed.End) * 0.5f,
                        Length = suppressed.Length,
                        Viability = viability,
                        ViabilityState =
                            EdgeWearViabilityState.StructuralIneligible,
                        CandidateReason = "micro-topology-suppressed",
                        FinalReason = "micro-topology-suppressed"
                    };
                audit.Records.Add(lifecycle);
                audit.RecordByKey[key] = lifecycle;
                audit.ViabilityByKey[key] = viability;
            }
        }

        private static EdgeWearEdgeAggregate
            GetOrAddEdgeWearSourceEdgeAggregate(
                List<EdgeWearEdgeAggregate> edges,
                Dictionary<EdgeKey, int> edgeIndexByKey,
                Vector3 start,
                Vector3 end,
                int faceIndex,
                ref int coincidentBoundarySeamPairCount)
        {
            EdgeKey key = new EdgeKey(start, end);
            if (edgeIndexByKey.TryGetValue(key, out int exactIndex))
            {
                return edges[exactIndex];
            }

            for (int edgeIndex = 0; edgeIndex < edges.Count; edgeIndex++)
            {
                EdgeWearEdgeAggregate candidate = edges[edgeIndex];
                if (candidate.FaceIndices.Count != 1 ||
                    candidate.FaceIndices[0] == faceIndex ||
                    !AreSamePoint(candidate.Start, end) ||
                    !AreSamePoint(candidate.End, start))
                {
                    continue;
                }

                edgeIndexByKey.Add(key, edgeIndex);
                candidate.CoincidentBoundarySeamReconciled = true;
                coincidentBoundarySeamPairCount++;
                return candidate;
            }

            int newIndex = edges.Count;
            EdgeWearEdgeAggregate created =
                new EdgeWearEdgeAggregate(start, end);
            edges.Add(created);
            edgeIndexByKey.Add(key, newIndex);
            return created;
        }

        private static string ResolveEdgeWearArtisticFilterReason(
            EdgeWearEdgeLifecycleRecord lifecycle,
            float artisticMinimumLength)
        {
            if (lifecycle.Length <= artisticMinimumLength)
            {
                return "artistically-short-edge";
            }
            float normalDot = Mathf.Cos(
                lifecycle.DihedralDegrees * Mathf.Deg2Rad);
            float angleScore = Mathf.Clamp01(
                (1f - normalDot) * 0.72f);
            if (angleScore <= 0.035f)
            {
                return "artistically-shallow-edge";
            }
            float baseSuppression = Mathf.SmoothStep(
                0.06f,
                0.20f,
                lifecycle.Vertical01);
            return baseSuppression <= 0.001f
                ? "artistically-base-suppressed"
                : "artistically-filtered";
        }

        private static void PopulateEdgeWearArtisticContextMetrics(
            EdgeWearCoverageAudit audit,
            float maximumDimension,
            float requestedWidth)
        {
            if (audit == null || audit.Records == null)
            {
                return;
            }

            List<EdgeWearEdgeLifecycleRecord> geometric =
                new List<EdgeWearEdgeLifecycleRecord>();
            for (int recordIndex = 0;
                 recordIndex < audit.Records.Count;
                 recordIndex++)
            {
                EdgeWearEdgeLifecycleRecord record =
                    audit.Records[recordIndex];
                if (record.GeometricEligible)
                {
                    geometric.Add(record);
                }
            }

            float densityRadius = Mathf.Max(
                maximumDimension * 0.34f,
                PointMergeDistance * 16f);
            float densityRadiusSqr = densityRadius * densityRadius;
            for (int recordIndex = 0;
                 recordIndex < geometric.Count;
                 recordIndex++)
            {
                EdgeWearEdgeLifecycleRecord record = geometric[recordIndex];
                int degreeA = 0;
                int degreeB = 0;
                int nearby = 0;
                Vector3 midpoint = (record.Start + record.End) * 0.5f;
                for (int otherIndex = 0;
                     otherIndex < geometric.Count;
                     otherIndex++)
                {
                    if (otherIndex == recordIndex)
                    {
                        continue;
                    }
                    EdgeWearEdgeLifecycleRecord other =
                        geometric[otherIndex];
                    if (AreSamePoint(record.Start, other.Start) ||
                        AreSamePoint(record.Start, other.End))
                    {
                        degreeA++;
                    }
                    if (AreSamePoint(record.End, other.Start) ||
                        AreSamePoint(record.End, other.End))
                    {
                        degreeB++;
                    }
                    Vector3 otherMidpoint =
                        (other.Start + other.End) * 0.5f;
                    if ((otherMidpoint - midpoint).sqrMagnitude <=
                        densityRadiusSqr)
                    {
                        nearby++;
                    }
                }

                record.ArtisticSharedVertexDegreeA = degreeA;
                record.ArtisticSharedVertexDegreeB = degreeB;
                record.ArtisticLocalDensity01 = geometric.Count > 1
                    ? Mathf.Clamp01((float)nearby / (geometric.Count - 1))
                    : 0f;
                float edgeRequestedWidth = record.Viability == null
                    ? requestedWidth
                    : record.Viability.RequestedWidth;
                record.ArtisticFeasibleWidthFraction =
                    record.Viability == null ||
                    edgeRequestedWidth <= 0f
                        ? 0f
                        : Mathf.Clamp01(
                            record.Viability.MaximumLocallyFeasibleWidth /
                            edgeRequestedWidth);
            }
        }

        private static void CaptureEdgeWearArtisticSelectionAudit(
            EdgeWearCoverageAudit audit,
            List<EdgeWearBevelCandidate> orderedCandidates,
            int selectedCount)
        {
            if (audit == null)
            {
                return;
            }

            audit.ArtisticAuditCaptured = true;
            audit.ArtisticSelectionTargetCount = Mathf.Max(0, selectedCount);
            audit.ArtisticSelectionThreshold =
                orderedCandidates != null &&
                selectedCount > 0 &&
                selectedCount <= orderedCandidates.Count
                    ? orderedCandidates[selectedCount - 1].Score
                    : 0f;

            for (int recordIndex = 0;
                 recordIndex < audit.Records.Count;
                 recordIndex++)
            {
                EdgeWearEdgeLifecycleRecord record =
                    audit.Records[recordIndex];
                record.ArtisticSelectionRank = -1;
                record.ArtisticSelectionThreshold =
                    audit.ArtisticSelectionThreshold;
                record.ArtisticSelectionDelta =
                    record.Score - audit.ArtisticSelectionThreshold;
            }

            if (orderedCandidates == null)
            {
                return;
            }

            EdgeWearEdgeLifecycleRecord[] recordByCandidateIndex =
                new EdgeWearEdgeLifecycleRecord[orderedCandidates.Count];
            for (int recordIndex = 0;
                 recordIndex < audit.Records.Count;
                 recordIndex++)
            {
                EdgeWearEdgeLifecycleRecord record =
                    audit.Records[recordIndex];
                if (!record.Candidate ||
                    record.CandidateIndex < 0 ||
                    record.CandidateIndex >=
                        recordByCandidateIndex.Length ||
                    recordByCandidateIndex[record.CandidateIndex] != null)
                {
                    continue;
                }
                recordByCandidateIndex[record.CandidateIndex] = record;
            }

            for (int rank = 0; rank < orderedCandidates.Count; rank++)
            {
                EdgeWearBevelCandidate candidate = orderedCandidates[rank];
                if (candidate.CandidateIndex < 0 ||
                    candidate.CandidateIndex >=
                        recordByCandidateIndex.Length)
                {
                    continue;
                }
                EdgeWearEdgeLifecycleRecord record =
                    recordByCandidateIndex[candidate.CandidateIndex];
                if (record == null)
                {
                    continue;
                }
                record.ArtisticSelectionRank = rank + 1;
                record.ArtisticSelectionThreshold =
                    audit.ArtisticSelectionThreshold;
                record.ArtisticSelectionDelta =
                    record.Score - audit.ArtisticSelectionThreshold;
            }
        }

        private static List<Vector3> BuildEdgeWearViabilitySourceVertexList(
            List<PolygonFace> faces)
        {
            Dictionary<VertexKey, Vector3> unique =
                new Dictionary<VertexKey, Vector3>();
            for (int faceIndex = 0; faceIndex < faces.Count; faceIndex++)
            {
                PolygonFace face = faces[faceIndex];
                if (face == null || face.Vertices == null)
                {
                    continue;
                }
                for (int vertexIndex = 0;
                     vertexIndex < face.Vertices.Count;
                     vertexIndex++)
                {
                    Vector3 position = face.Vertices[vertexIndex];
                    VertexKey key = new VertexKey(position);
                    if (!unique.ContainsKey(key))
                    {
                        unique.Add(key, position);
                    }
                }
            }
            return new List<Vector3>(unique.Values);
        }

        private static void EvaluateIndependentPlaneLocalityViability(
            List<Vector3> sourceVertices,
            Vector3 sourceA,
            Vector3 sourceB,
            Vector3 bevelNormal,
            Vector3 solidCentre,
            float minimumStableEdgeLength,
            EdgeWearEdgeViabilityRecord viability)
        {
            if (viability == null)
            {
                return;
            }
            viability.LocalityValid = false;
            if (sourceVertices == null ||
                !IsFinite(sourceA) || !IsFinite(sourceB) ||
                !IsFinite(bevelNormal) || !IsFinite(solidCentre) ||
                bevelNormal.sqrMagnitude <= MinimumEdgeLengthSqr)
            {
                return;
            }

            Vector3 normal = bevelNormal.normalized;
            float guardMargin = Mathf.Max(
                PointMergeDistance * 2f,
                minimumStableEdgeLength * 0.001f);
            float minimumRemoval = Mathf.Max(
                PointMergeDistance * 2f,
                minimumStableEdgeLength * 0.02f);
            float retainFloor = float.NegativeInfinity;
            int limitingVertex = -1;
            Vector3 limitingPosition = default;
            float limitingProjection = float.NegativeInfinity;
            for (int vertexIndex = 0;
                 vertexIndex < sourceVertices.Count;
                 vertexIndex++)
            {
                Vector3 position = sourceVertices[vertexIndex];
                if (AreSamePoint(position, sourceA) ||
                    AreSamePoint(position, sourceB))
                {
                    continue;
                }
                float projection = Vector3.Dot(normal, position);
                float guarded = projection + guardMargin;
                if (guarded <= retainFloor)
                {
                    continue;
                }
                retainFloor = guarded;
                limitingVertex = vertexIndex;
                limitingPosition = position;
                limitingProjection = projection;
            }

            float sourceProjectionA = Vector3.Dot(normal, sourceA);
            float sourceProjectionB = Vector3.Dot(normal, sourceB);
            float removalCeiling = Mathf.Min(
                sourceProjectionA,
                sourceProjectionB) - minimumRemoval;
            float centreProjection = Vector3.Dot(normal, solidCentre);
            if (float.IsNegativeInfinity(retainFloor))
            {
                retainFloor = centreProjection + guardMargin;
            }
            else
            {
                retainFloor = Mathf.Max(
                    retainFloor,
                    centreProjection + guardMargin);
            }

            viability.LocalityRetainPlaneFloor = retainFloor;
            viability.LocalityRemovalPlaneCeiling = removalCeiling;
            viability.LocalityFeasibleMargin =
                removalCeiling - retainFloor;
            viability.LocalityGuardMargin = guardMargin;
            viability.LocalityMinimumRemoval = minimumRemoval;
            viability.LocalityLimitingVertex = limitingVertex;
            viability.LocalityLimitingPosition = limitingPosition;
            viability.LocalityLimitingProjection = limitingProjection;
            viability.LocalityValid =
                retainFloor <= removalCeiling +
                    PointMergeDistance * 0.25f;
        }

        private static void SetEdgeWearGeometricIneligibility(
            EdgeWearEdgeLifecycleRecord lifecycle,
            EdgeWearEdgeViabilityRecord viability,
            string reason)
        {
            if (lifecycle == null)
            {
                return;
            }
            lifecycle.GeometricEligible = false;
            lifecycle.CoexistenceEligible = false;
            lifecycle.CoexistenceFailureReason = string.Empty;
            lifecycle.ViabilityState =
                EdgeWearViabilityState.GeometricIneligible;
            lifecycle.Candidate = false;
            lifecycle.Selected = false;
            lifecycle.Active = false;
            lifecycle.CandidateReason = reason ??
                "geometrically-ineligible";
            lifecycle.FinalReason = lifecycle.CandidateReason;
            if (viability != null)
            {
                viability.Evaluated = true;
                viability.Viable = false;
                viability.FailureReason = lifecycle.CandidateReason;
            }
        }

        private static void MapEdgeWearCoverageAuditSourceIndices(
            EdgeWearCoverageAudit audit,
            EdgeWearTopologyGraph graph)
        {
            if (audit == null || graph == null)
            {
                return;
            }
            audit.RecordByGraphEdge.Clear();
            audit.ViabilityByGraphEdge.Clear();
            audit.CoincidentGraphVertexReconciliationCount =
                graph.CoincidentVertexReconciliationCount;
            audit.CoincidentGraphBoundarySeamPairCount =
                graph.CoincidentBoundarySeamPairCount;
            for (int recordIndex = 0;
                 recordIndex < audit.Records.Count;
                 recordIndex++)
            {
                EdgeWearEdgeLifecycleRecord record =
                    audit.Records[recordIndex];
                if (record.MicroTopologySuppressed)
                {
                    record.SourceEdgeIndex = -1;
                    continue;
                }
                if (!graph.EdgeByKey.TryGetValue(
                        record.Key,
                        out int graphEdgeIndex))
                {
                    record.SourceEdgeIndex = -1;
                    record.FinalReason = "source-edge-not-mapped";
                    continue;
                }
                record.SourceEdgeIndex = graphEdgeIndex;
                if (record.OriginalSourceEdgeIndex < 0 &&
                    !record.MicroTopologyGeneratedTransition)
                {
                    record.OriginalSourceEdgeIndex = graphEdgeIndex;
                }
                audit.RecordByGraphEdge[graphEdgeIndex] = record;
                if (record.Viability != null)
                {
                    record.Viability.SourceEdgeIndex = graphEdgeIndex;
                    if (record.Viability.LocalityLimitingVertex >= 0)
                    {
                        record.Viability.LocalityLimitingVertex =
                            graph.VertexByKey.TryGetValue(
                                new VertexKey(
                                    record.Viability.LocalityLimitingPosition),
                                out int limitingGraphVertex)
                                ? limitingGraphVertex
                                : -1;
                    }
                    audit.ViabilityByGraphEdge[graphEdgeIndex] =
                        record.Viability;
                }
            }
        }

        private static void RunEdgeWearIsolatedViabilityPreflight(
            List<PolygonFace> sourceFaces,
            ChamferTopologyContext context,
            float requestedWidth,
            float minimumStyleWidth,
            float minimumStableEdgeLength,
            float minimumStableFaceArea,
            EdgeWearCoverageAudit audit)
        {
            List<EdgeWearSelectedGraphEdge> eligible =
                BuildBoundedSingleEdgeEligibleList(context);
            for (int ordinal = 0; ordinal < eligible.Count; ordinal++)
            {
                audit.ViabilityIsolatedEvaluationCount++;
                int graphEdgeIndex = eligible[ordinal].GraphEdgeIndex;
                float edgeRequestedWidth =
                    ResolveEdgeWearRequestedWidth(
                        audit,
                        graphEdgeIndex,
                        requestedWidth);
                BoundedSingleEdgeAuditResult isolated =
                    AuditBoundedSingleEdgeBevel(
                        sourceFaces,
                        context,
                        ordinal,
                        edgeRequestedWidth,
                        minimumStableEdgeLength,
                        minimumStableFaceArea,
                        false,
                        out _);
                if (!audit.RecordByGraphEdge.TryGetValue(
                        isolated.SourceEdgeIndex,
                        out EdgeWearEdgeLifecycleRecord lifecycle) ||
                    lifecycle.Viability == null)
                {
                    continue;
                }

                EdgeWearEdgeViabilityRecord viability =
                    lifecycle.Viability;
                // Preserve the accepted viability decision inputs, but do not
                // describe a failed or partial attempt as a certified width.
                viability.MaximumLocallyFeasibleWidth =
                    isolated.SolvedWidth;
                viability.FeasibleWidthFraction = edgeRequestedWidth > 0f
                    ? isolated.SolvedWidth / edgeRequestedWidth
                    : 0f;
                viability.MinimumStyleWidth = minimumStyleWidth;
                viability.MinimumRequiredCertifiedWidth =
                    minimumStyleWidth *
                    EdgeWearMinimumFeasibleWidthFraction;
                viability.IsolatedSucceeded =
                    isolated.GeometryValid == 1;
                viability.IsolatedWidthAttemptCount =
                    isolated.WidthAttemptCount;
                viability.IsolatedLastAttemptedWidth =
                    isolated.SolvedWidth;
                viability.IsolatedAttemptScheduleComplete =
                    isolated.IsolatedAttemptScheduleComplete == 1;
                viability.IsolatedTerminalConstructionAtMinimum =
                    isolated.IsolatedTerminalConstructionAtMinimum == 1;
                viability.IsolatedAttemptScheduleResolution =
                    isolated.IsolatedAttemptScheduleResolution ??
                        string.Empty;
                bool retainWidthAttemptEvidence =
                    isolated.GeometryValid != 1 ||
                    isolated.WidthAttemptCount > 1 ||
                    isolated.MultiSupportSinglePlaneAttempted == 1 ||
                    isolated.MultiSupportRetainedHullAttempted == 1;
                viability.IsolatedWidthAttemptEvidence =
                    retainWidthAttemptEvidence
                        ? FormatBoundedIsolatedWidthAttemptEvidence(
                            isolated)
                        : string.Empty;
                viability.IsolatedMaximumCertifiedWidth =
                    viability.IsolatedSucceeded
                        ? isolated.SolvedWidth
                        : 0f;
                viability.IsolatedMaximumCertifiedWidthFraction =
                    viability.IsolatedSucceeded && edgeRequestedWidth > 0f
                        ? isolated.SolvedWidth / edgeRequestedWidth
                        : 0f;
                viability.IsolatedAlternateBoundaryRailCount =
                    isolated.AlternateBoundaryRailCount;
                viability.IsolatedMaximumBoundaryCandidateCount =
                    isolated.MaximumBoundaryCandidateCount;
                viability.IsolatedMaximumBoundarySnapDistance =
                    isolated.MaximumBoundarySnapDistance;
                viability.IsolatedMaximumBoundaryPointTolerance =
                    isolated.MaximumBoundaryPointTolerance;
                viability.IsolatedMaximumBoundaryDiagnosticRailIndex =
                    isolated.MaximumBoundaryDiagnosticRailIndex;
                viability.IsolatedMaximumBoundaryOriginalAdjacentEdgeIndex =
                    isolated.MaximumBoundaryOriginalAdjacentEdgeIndex;
                viability.IsolatedMaximumBoundaryResolvedEdgeIndex =
                    isolated.MaximumBoundaryResolvedEdgeIndex;
                viability.IsolatedMaximumBoundaryOriginalRawParameter =
                    isolated.MaximumBoundaryOriginalRawParameter;
                viability.IsolatedMaximumBoundaryOriginalSegmentDistance =
                    isolated.MaximumBoundaryOriginalSegmentDistance;
                viability.IsolatedMinimumBoundaryEndpointDistance =
                    isolated.MinimumBoundaryEndpointDistance;
                viability.EndpointConsumptionA =
                    isolated.EndpointConsumptionA;
                viability.EndpointConsumptionB =
                    isolated.EndpointConsumptionB;
                viability.RemainingCentralSpan =
                    isolated.RemainingCentralSpan;
                viability.MinimumCentralSpan = Mathf.Max(
                    isolated.MinimumCentralSpan,
                    edgeRequestedWidth *
                        EdgeWearMinimumCentralSpanWidthMultiplier);
                viability.IsolatedOpenEdgeCount =
                    isolated.OpenEdgeCount;
                viability.IsolatedNonManifoldEdgeCount =
                    isolated.NonManifoldEdgeCount;
                viability.IsolatedTJunctionCount =
                    isolated.TJunctionCount;
                viability.IsolatedInvalidFaceCount =
                    isolated.InvalidFaceCount;
                viability.IsolatedDiagnostic =
                    isolated.Diagnostic ?? string.Empty;
                if (!string.IsNullOrEmpty(
                        viability.IsolatedWidthAttemptEvidence))
                {
                    viability.IsolatedDiagnostic =
                        string.IsNullOrEmpty(viability.IsolatedDiagnostic)
                            ? viability.IsolatedWidthAttemptEvidence
                            : viability.IsolatedDiagnostic + "; " +
                                viability.IsolatedWidthAttemptEvidence;
                }
                viability.IsolatedConstructionValid =
                    isolated.GeometryValid == 1;
                viability.FeasibleWidthFractionValid =
                    viability.FeasibleWidthFraction + 0.0001f >=
                    EdgeWearMinimumFeasibleWidthFraction;
                viability.WidthRecoveryProvisional =
                    viability.IsolatedSucceeded &&
                    !viability.FeasibleWidthFractionValid &&
                    viability.IsolatedMaximumCertifiedWidth +
                        PointMergeDistance >=
                    viability.MinimumRequiredCertifiedWidth;
                viability.MaterialWidthRecoveryRequiredLength =
                    viability.RequiredFootprintLength *
                    EdgeWearMaterialWidthRecoveryFootprintMultiplier;
                viability.MaterialWidthRecoveryEligible =
                    IsEdgeWearMaterialWidthRecoveryEligible(lifecycle);
                viability.MultiSupportHullRecovery =
                    isolated.MultiSupportHullPointCount > 0 &&
                    isolated.GeometryValid == 1;
                viability.EndpointSpanValid =
                    viability.RemainingCentralSpan +
                        PointMergeDistance >=
                    viability.MinimumCentralSpan;

                string failureReason =
                    ResolveEdgeWearIsolatedViabilityFailure(
                        isolated,
                        viability,
                        minimumStableEdgeLength);
                if (!string.IsNullOrEmpty(failureReason))
                {
                    SetEdgeWearGeometricIneligibility(
                        lifecycle,
                        viability,
                        failureReason);
                    continue;
                }

                viability.Evaluated = true;
                viability.Viable = true;
                viability.FailureReason = string.Empty;
                lifecycle.GeometricEligible = true;
                lifecycle.CoexistenceEligible = true;
                lifecycle.CoexistenceFailureReason = string.Empty;
                lifecycle.ViabilityState =
                    EdgeWearViabilityState.ViableUnselected;
                lifecycle.FinalReason = "viable-unselected";
            }
        }

        private static bool IsEdgeWearMaterialWidthRecoveryEligible(
            EdgeWearEdgeLifecycleRecord lifecycle)
        {
            if (lifecycle == null || lifecycle.Viability == null)
            {
                return false;
            }

            EdgeWearEdgeViabilityRecord viability = lifecycle.Viability;
            return viability.WidthRecoveryProvisional &&
                viability.IsolatedSucceeded &&
                viability.IsolatedConstructionValid &&
                !viability.FeasibleWidthFractionValid &&
                viability.IsolatedMaximumCertifiedWidth +
                    PointMergeDistance >=
                    viability.MinimumRequiredCertifiedWidth &&
                lifecycle.ArtisticEligible &&
                lifecycle.Length + PointMergeDistance >=
                    viability.MaterialWidthRecoveryRequiredLength;
        }

        private static string ResolveEdgeWearIsolatedViabilityFailure(
            BoundedSingleEdgeAuditResult isolated,
            EdgeWearEdgeViabilityRecord viability,
            float minimumStableEdgeLength)
        {
            if (isolated.IsolatedRailSolved != 1)
            {
                return "isolated-rail-solve-failed";
            }
            bool reducedToStableWidthFloor =
                viability.IsolatedSucceeded &&
                viability.IsolatedMaximumCertifiedWidth <=
                    minimumStableEdgeLength + PointMergeDistance &&
                viability.IsolatedMaximumCertifiedWidth +
                    PointMergeDistance < viability.RequestedWidth;
            if (reducedToStableWidthFloor)
            {
                return "maximum-certified-width-at-stable-width-floor";
            }
            if (!viability.FeasibleWidthFractionValid &&
                !viability.WidthRecoveryProvisional)
            {
                return "maximum-feasible-width-below-minimum-scale";
            }
            if (!viability.EndpointSpanValid)
            {
                return "endpoint-star-consumes-edge-span";
            }
            if (isolated.OwnerClipCount != 2 ||
                isolated.EndpointSupportClipCount != 2 ||
                isolated.EndpointSupportRemovedVertexCount != 2 ||
                isolated.EndpointSupportRailInsertionCount != 4)
            {
                return "owner-face-support-insufficient";
            }
            if (isolated.OpenEdgeCount > 0 ||
                isolated.NonManifoldEdgeCount > 0 ||
                isolated.TJunctionCount > 0)
            {
                return "isolated-topology-invalid";
            }
            if (isolated.InvalidFaceCount > 0 ||
                isolated.PrepareDegenerateCount > 0 ||
                isolated.PrepareNonPlanarCount > 0 ||
                isolated.PrepareNonSimpleCount > 0 ||
                isolated.PrepareNonConvexCount > 0 ||
                isolated.PrepareWindingFailureCount > 0)
            {
                return "isolated-face-quality-invalid";
            }
            if (isolated.ResultContainmentViolationCount > 0 ||
                isolated.ResultConvexityViolationCount > 0 ||
                isolated.IntroducedImproperInteriorIntersectionPairCount > 0)
            {
                return "isolated-containment-invalid";
            }
            if (isolated.BoundsValid != 1 || isolated.VolumeValid != 1)
            {
                return "isolated-volume-or-bounds-invalid";
            }
            if (isolated.GeometryValid != 1)
            {
                return "isolated-construction-invalid";
            }
            return string.Empty;
        }

        private static bool TryClassifyEdgeWearStructuralEdge(
            List<PolygonFace> sourceFaces,
            int sourceFaceA,
            int sourceFaceB,
            Vector3 edgeA,
            Vector3 edgeB,
            Vector3 solidCentre,
            float tolerance,
            out BoundedEdgeClassificationEvidence evidence)
        {
            evidence = new BoundedEdgeClassificationEvidence
            {
                SourceFaceA = sourceFaceA,
                SourceFaceB = sourceFaceB,
                Classification = BoundedEdgeClassification.Ambiguous
            };
            if (sourceFaces == null ||
                sourceFaceA < 0 || sourceFaceB < 0 ||
                sourceFaceA >= sourceFaces.Count ||
                sourceFaceB >= sourceFaces.Count ||
                sourceFaceA == sourceFaceB ||
                !IsFinite(edgeA) || !IsFinite(edgeB) ||
                !IsFinite(solidCentre))
            {
                return false;
            }

            PolygonFace faceA = sourceFaces[sourceFaceA];
            PolygonFace faceB = sourceFaces[sourceFaceB];
            if (faceA == null || faceB == null ||
                !IsFinite(faceA.Normal) || !IsFinite(faceB.Normal) ||
                faceA.Normal.sqrMagnitude <= MinimumEdgeLengthSqr ||
                faceB.Normal.sqrMagnitude <= MinimumEdgeLengthSqr)
            {
                return false;
            }

            Vector3 normalA = faceA.Normal.normalized;
            Vector3 normalB = faceB.Normal.normalized;
            float planeA = CalculateBoundedFacePlaneDistance(faceA, normalA);
            float planeB = CalculateBoundedFacePlaneDistance(faceB, normalB);
            bool measuredA = TryMeasureBoundedFaceInteriorAgainstPlane(
                faceA,
                edgeA,
                edgeB,
                normalB,
                planeB,
                tolerance,
                out float faceAAgainstB);
            bool measuredB = TryMeasureBoundedFaceInteriorAgainstPlane(
                faceB,
                edgeA,
                edgeB,
                normalA,
                planeA,
                tolerance,
                out float faceBAgainstA);

            float normalDot = Mathf.Clamp(
                Vector3.Dot(normalA, normalB),
                -1f,
                1f);
            float solidAgainstA =
                Vector3.Dot(normalA, solidCentre) - planeA;
            float solidAgainstB =
                Vector3.Dot(normalB, solidCentre) - planeB;
            BoundedEdgeClassification classification;
            if (solidAgainstA > tolerance || solidAgainstB > tolerance)
            {
                classification =
                    BoundedEdgeClassification.InvalidOrientation;
            }
            else if (!measuredA || !measuredB)
            {
                classification = BoundedEdgeClassification.Ambiguous;
            }
            else if (normalDot >= 0.9999f &&
                Mathf.Abs(faceAAgainstB) <= tolerance &&
                Mathf.Abs(faceBAgainstA) <= tolerance)
            {
                classification = BoundedEdgeClassification.Coplanar;
            }
            else if (faceAAgainstB <= tolerance &&
                faceBAgainstA <= tolerance)
            {
                classification = BoundedEdgeClassification.Convex;
            }
            else if (faceAAgainstB > tolerance ||
                faceBAgainstA > tolerance)
            {
                classification = BoundedEdgeClassification.Concave;
            }
            else
            {
                classification = BoundedEdgeClassification.Ambiguous;
            }

            evidence.NormalA = normalA;
            evidence.NormalB = normalB;
            evidence.NormalDot = normalDot;
            evidence.DihedralDegrees =
                Mathf.Acos(normalDot) * Mathf.Rad2Deg;
            evidence.FaceAInteriorAgainstFaceB = faceAAgainstB;
            evidence.FaceBInteriorAgainstFaceA = faceBAgainstA;
            evidence.SolidCentreAgainstFaceA = solidAgainstA;
            evidence.SolidCentreAgainstFaceB = solidAgainstB;
            evidence.Classification = classification;
            return true;
        }

        private static SortedSet<int>
            CaptureImmutableMaterialWidthRecoveryTargets(
                ChamferTopologyContext context,
                EdgeWearCoverageAudit audit)
        {
            SortedSet<int> targets = new SortedSet<int>();
            if (context == null || audit == null)
            {
                return targets;
            }

            for (int recordIndex = 0;
                 recordIndex < audit.Records.Count;
                 recordIndex++)
            {
                audit.Records[recordIndex].MaterialWidthRecoveryTarget = false;
            }

            for (int selectedIndex = 0;
                 selectedIndex < context.SelectedEdges.Count;
                 selectedIndex++)
            {
                int graphEdgeIndex =
                    context.SelectedEdges[selectedIndex].GraphEdgeIndex;
                if (!audit.RecordByGraphEdge.TryGetValue(
                        graphEdgeIndex,
                        out EdgeWearEdgeLifecycleRecord record) ||
                    record == null || record.Viability == null)
                {
                    continue;
                }

                record.Viability.MaterialWidthRecoveryEligible =
                    IsEdgeWearMaterialWidthRecoveryEligible(record);
                if (!record.Viability.MaterialWidthRecoveryEligible)
                {
                    continue;
                }

                record.MaterialWidthRecoveryTarget = true;
                targets.Add(graphEdgeIndex);
            }
            return targets;
        }

        private static void MapEdgeWearCoverageAuditToGraph(
            EdgeWearCoverageAudit audit,
            ChamferTopologyContext context)
        {
            if (audit == null || context == null)
            {
                return;
            }

            MapEdgeWearCoverageAuditSourceIndices(
                audit,
                context.Graph);
            for (int recordIndex = 0;
                 recordIndex < audit.Records.Count;
                 recordIndex++)
            {
                EdgeWearEdgeLifecycleRecord record =
                    audit.Records[recordIndex];
                if (record.SourceEdgeIndex < 0)
                {
                    continue;
                }
                if (!record.StructuralEligible)
                {
                    record.ViabilityState =
                        EdgeWearViabilityState.StructuralIneligible;
                    record.FinalReason = string.IsNullOrEmpty(
                            record.CandidateReason)
                        ? "structurally-ineligible"
                        : record.CandidateReason;
                }
                else if (!record.GeometricEligible)
                {
                    record.ViabilityState =
                        EdgeWearViabilityState.GeometricIneligible;
                    record.FinalReason = record.Viability != null &&
                        !string.IsNullOrEmpty(
                            record.Viability.FailureReason)
                            ? record.Viability.FailureReason
                            : "geometrically-ineligible";
                }
                else if (!record.CoexistenceEligible)
                {
                    record.ViabilityState =
                        EdgeWearViabilityState.CoexistenceIneligible;
                    record.FinalReason = string.IsNullOrEmpty(
                            record.CoexistenceFailureReason)
                        ? "coexistence-ineligible"
                        : record.CoexistenceFailureReason;
                }
                else if (record.Candidate)
                {
                    record.ViabilityState =
                        EdgeWearViabilityState.ViableUnselected;
                    record.FinalReason = "not-selected-by-coverage";
                }
                else
                {
                    record.ViabilityState =
                        EdgeWearViabilityState.ViableUnselected;
                    record.FinalReason = "viable-artistic-filtered";
                }
            }

            for (int selectedIndex = 0;
                 selectedIndex < context.SelectedEdges.Count;
                 selectedIndex++)
            {
                EdgeWearSelectedGraphEdge selected =
                    context.SelectedEdges[selectedIndex];
                if (audit.RecordByGraphEdge.TryGetValue(
                        selected.GraphEdgeIndex,
                        out EdgeWearEdgeLifecycleRecord record) &&
                    record.CoexistenceEligible)
                {
                    record.Selected = true;
                    record.ViabilityState =
                        EdgeWearViabilityState.ViableSelected;
                    record.FinalReason = "selected";
                }
            }

            RecalculateEdgeWearCoverageAudit(audit);
        }

        private static void ApplyEdgeWearCoverageCornerSolution(
            EdgeWearCoverageAudit audit,
            ChamferTopologyContext context,
            ChamferCornerSolution solution)
        {
            if (audit == null || context == null || solution == null)
            {
                return;
            }

            audit.CoexistenceSearchExclusionCount = 0;
            for (int selectedIndex = 0;
                 selectedIndex < context.SelectedEdges.Count;
                 selectedIndex++)
            {
                EdgeWearSelectedGraphEdge selected =
                    context.SelectedEdges[selectedIndex];
                if (!audit.RecordByGraphEdge.TryGetValue(
                        selected.GraphEdgeIndex,
                        out EdgeWearEdgeLifecycleRecord record))
                {
                    continue;
                }

                if (!solution.WidthByEdge.TryGetValue(
                        selected.GraphEdgeIndex,
                        out float width))
                {
                    SetEdgeWearCornerWidthCoexistenceIneligibility(
                        record,
                        0f,
                        "corner-width-missing");
                    continue;
                }
                if (width <= PointMergeDistance)
                {
                    bool forcedDeferred =
                        solution.ForcedDeferredEdges.Contains(
                            selected.GraphEdgeIndex);
                    bool recoveryBaselineDeferred = forcedDeferred &&
                        record.Viability != null &&
                        record.Viability.WidthRecoveryProvisional;
                    if (recoveryBaselineDeferred)
                    {
                        record.RecoveryBaselineDeferred = true;
                        SetEdgeWearCornerWidthCoexistenceIneligibility(
                            record,
                            width,
                            "recovery-baseline-deferred");
                    }
                    else
                    {
                        CaptureFinalCornerInactiveRecoveryEvidence(
                            record,
                            solution,
                            selected.GraphEdgeIndex);
                        SetEdgeWearCornerWidthCoexistenceIneligibility(
                            record,
                            width,
                            forcedDeferred
                                ? "augmentation-forced-deferred"
                                : "corner-width-inactive");
                    }
                    continue;
                }

                record.RecoveryBaselineDeferred = false;
                record.SolvedWidth = width;
                record.ArtisticSolvedWidthFraction =
                    record.Viability == null ||
                    record.Viability.RequestedWidth <= 0f
                        ? 0f
                        : width / record.Viability.RequestedWidth;
                record.WidthInactive = false;
                record.Active = true;
                record.FinalReason = "active";
            }

            RecalculateEdgeWearCoverageAudit(audit);
        }


        private static void CaptureFinalCornerInactiveRecoveryEvidence(
            EdgeWearEdgeLifecycleRecord record,
            ChamferCornerSolution solution,
            int graphEdgeIndex)
        {
            if (record == null || solution == null)
            {
                return;
            }
            for (int conflictIndex = 0;
                 conflictIndex < solution.Conflicts.Count;
                 conflictIndex++)
            {
                ChamferCornerConflictRecord conflict =
                    solution.Conflicts[conflictIndex];
                if (conflict == null ||
                    !conflict.ZeroedSelectedEdges.Contains(
                        graphEdgeIndex))
                {
                    continue;
                }
                record.CornerRecoveryProvisional = true;
                record.CornerRecoveryCollapsedSourceEdgeIndex =
                    conflict.UnselectedSourceEdgeIndex;
                conflict.ParticipantWidthBeforeScale.TryGetValue(
                    graphEdgeIndex,
                    out record.CornerRecoveryLastPositiveWidth);
                record.CornerRecoveryUniformScale =
                    conflict.UniformScale;
                record.CornerRecoveryZeroingStage =
                    conflict.ZeroingStage;
                record.CornerRecoveryParticipants =
                    FormatChamferForcedDeferralKey(
                        conflict.ParticipatingSelectedEdges);
                record.CornerRecoveryZeroedParticipants =
                    FormatChamferForcedDeferralKey(
                        conflict.ZeroedSelectedEdges);
                return;
            }
        }

        private static void
            SetEdgeWearCornerWidthCoexistenceIneligibility(
                EdgeWearEdgeLifecycleRecord record,
                float solvedWidth,
                string reason)
        {
            if (record == null)
            {
                return;
            }

            string resolvedReason = string.IsNullOrEmpty(reason)
                ? "corner-width-inactive"
                : reason;
            record.CoexistenceEligible = false;
            record.CoexistenceFailureReason = resolvedReason;
            record.ViabilityState =
                EdgeWearViabilityState.CoexistenceIneligible;
            record.Candidate = false;
            record.CandidateIndex = -1;
            record.CandidateReason = resolvedReason;
            record.Selected = false;
            record.SolvedWidth = solvedWidth;
            record.ArtisticSolvedWidthFraction =
                record.Viability == null ||
                record.Viability.RequestedWidth <= 0f
                    ? 0f
                    : solvedWidth / record.Viability.RequestedWidth;
            record.WidthInactive = true;
            record.Active = false;
            record.AttemptedBuilt = false;
            record.Built = false;
            record.TrialRejected = false;
            record.Deferred = false;
            record.Rejected = false;
            record.MaterializedWidth = 0f;
            record.MaterializedWidthScale = 0f;
            record.WidthReduced = false;
            record.FinalReason = resolvedReason;
        }

        private static bool TryGetEdgeWearCoverageRecord(
            EdgeWearCoverageAudit audit,
            int graphEdgeIndex,
            out EdgeWearEdgeLifecycleRecord record)
        {
            record = null;
            return audit != null &&
                audit.RecordByGraphEdge.TryGetValue(
                    graphEdgeIndex,
                    out record);
        }

        private static void CaptureEdgeWearCollateralBaseline(
            EdgeWearCoverageAudit audit)
        {
            if (audit == null)
            {
                return;
            }

            audit.CollateralBaselineByKey.Clear();
            audit.BaselineGeometricEligibleCount = 0;
            for (int recordIndex = 0;
                 recordIndex < audit.Records.Count;
                 recordIndex++)
            {
                EdgeWearEdgeLifecycleRecord record =
                    audit.Records[recordIndex];
                EdgeWearCollateralBaselineRecord baseline =
                    new EdgeWearCollateralBaselineRecord(record);
                audit.CollateralBaselineByKey[record.Key] = baseline;
                if (baseline.GeometricEligible)
                {
                    audit.BaselineGeometricEligibleCount++;
                }
            }
            audit.CollateralBaselineCaptured = true;
            EvaluateEdgeWearCollateralPreservation(audit);
        }

        private static void EvaluateEdgeWearCollateralPreservation(
            EdgeWearCoverageAudit audit)
        {
            if (audit == null)
            {
                return;
            }

            audit.RecoveredGeometricEdgeCount = 0;
            audit.CollateralLostEdgeCount = 0;
            audit.CollateralChangedEdgeCount = 0;
            audit.RecoveredGeometricEdgeIndices.Clear();
            audit.CollateralLostEdgeIndices.Clear();
            audit.CollateralChangedEdgeIndices.Clear();

            if (!audit.CollateralBaselineCaptured)
            {
                audit.CollateralPreservationValid = false;
                return;
            }

            Dictionary<EdgeKey, EdgeWearEdgeLifecycleRecord> currentByKey =
                new Dictionary<EdgeKey, EdgeWearEdgeLifecycleRecord>();
            for (int recordIndex = 0;
                 recordIndex < audit.Records.Count;
                 recordIndex++)
            {
                EdgeWearEdgeLifecycleRecord record =
                    audit.Records[recordIndex];
                currentByKey[record.Key] = record;
                if (!record.GeometricEligible)
                {
                    continue;
                }

                if (!audit.CollateralBaselineByKey.TryGetValue(
                        record.Key,
                        out EdgeWearCollateralBaselineRecord baseline) ||
                    !baseline.GeometricEligible)
                {
                    audit.RecoveredGeometricEdgeIndices.Add(
                        record.OriginalSourceEdgeIndex >= 0
                            ? record.OriginalSourceEdgeIndex
                            : record.SourceEdgeIndex);
                }
            }

            foreach (KeyValuePair<EdgeKey,
                         EdgeWearCollateralBaselineRecord> pair in
                     audit.CollateralBaselineByKey)
            {
                EdgeWearCollateralBaselineRecord baseline = pair.Value;
                if (!baseline.GeometricEligible)
                {
                    continue;
                }

                if (!currentByKey.TryGetValue(
                        pair.Key,
                        out EdgeWearEdgeLifecycleRecord current) ||
                    !current.GeometricEligible)
                {
                    audit.CollateralLostEdgeIndices.Add(
                        baseline.OriginalSourceEdgeIndex);
                    continue;
                }

                if (HasEdgeWearCollateralStateChanged(
                        baseline,
                        current))
                {
                    audit.CollateralChangedEdgeIndices.Add(
                        baseline.OriginalSourceEdgeIndex);
                }
            }

            audit.RecoveredGeometricEdgeIndices.Sort();
            audit.CollateralLostEdgeIndices.Sort();
            audit.CollateralChangedEdgeIndices.Sort();
            audit.RecoveredGeometricEdgeCount =
                audit.RecoveredGeometricEdgeIndices.Count;
            audit.CollateralLostEdgeCount =
                audit.CollateralLostEdgeIndices.Count;
            audit.CollateralChangedEdgeCount =
                audit.CollateralChangedEdgeIndices.Count;
            audit.CollateralPreservationValid =
                audit.CollateralLostEdgeCount == 0 &&
                audit.CollateralChangedEdgeCount == 0;
        }

        private static bool HasEdgeWearCollateralStateChanged(
            EdgeWearCollateralBaselineRecord baseline,
            EdgeWearEdgeLifecycleRecord current)
        {
            int currentFaceA = Mathf.Min(current.FaceA, current.FaceB);
            int currentFaceB = Mathf.Max(current.FaceA, current.FaceB);
            float currentMaximumWidth = current.Viability == null
                ? 0f
                : current.Viability.MaximumLocallyFeasibleWidth;
            float currentWidthFraction = current.Viability == null
                ? 0f
                : current.Viability.FeasibleWidthFraction;
            return current.SourceEdgeIndex != baseline.SourceEdgeIndex ||
                currentFaceA != baseline.FaceA ||
                currentFaceB != baseline.FaceB ||
                current.Classification != baseline.Classification ||
                Mathf.Abs(current.Length - baseline.Length) >
                    PointMergeDistance ||
                Mathf.Abs(
                    current.DihedralDegrees -
                    baseline.DihedralDegrees) > 0.001f ||
                Mathf.Abs(
                    currentMaximumWidth -
                    baseline.MaximumLocallyFeasibleWidth) >
                    PointMergeDistance ||
                Mathf.Abs(
                    currentWidthFraction -
                    baseline.FeasibleWidthFraction) > 0.0001f;
        }

        private static void RecalculateEdgeWearCoverageAudit(
            EdgeWearCoverageAudit audit)
        {
            if (audit == null)
            {
                return;
            }

            audit.SourceEdgeCount = audit.Records.Count;
            audit.StructuralEligibleCount = 0;
            audit.GeometricEligibleCount = 0;
            audit.GeometricIneligibleCount = 0;
            audit.CoexistenceEligibleCount = 0;
            audit.CoexistenceIneligibleCount = 0;
            audit.CoexistencePreShellExclusionCount = 0;
            audit.CornerWidthMissingExclusionCount = 0;
            audit.CornerWidthInactiveExclusionCount = 0;
            audit.ArtisticEligibleCount = 0;
            audit.ArtisticFilteredCount = 0;
            audit.CandidateCount = 0;
            audit.SelectedCount = 0;
            audit.WidthInactiveCount = 0;
            audit.UnresolvedWidthInactiveCount = 0;
            audit.WidthReducedCount = 0;
            audit.ActiveCount = 0;
            audit.AttemptedBuiltCount = 0;
            audit.BuiltCount = 0;
            audit.TrialRejectedCount = 0;
            audit.DeferredCount = 0;
            audit.RejectedCount = 0;
            audit.UnmappedCount = 0;
            for (int recordIndex = 0;
                 recordIndex < audit.Records.Count;
                 recordIndex++)
            {
                EdgeWearEdgeLifecycleRecord record =
                    audit.Records[recordIndex];
                if (record.StructuralEligible)
                {
                    audit.StructuralEligibleCount++;
                    if (record.GeometricEligible)
                    {
                        audit.GeometricEligibleCount++;
                        if (record.CoexistenceEligible)
                        {
                            audit.CoexistenceEligibleCount++;
                        }
                        else
                        {
                            audit.CoexistenceIneligibleCount++;
                        }
                        if (!record.ArtisticEligible)
                        {
                            audit.ArtisticFilteredCount++;
                        }
                    }
                    else
                    {
                        audit.GeometricIneligibleCount++;
                    }
                }
                if (record.GeometricEligible && record.ArtisticEligible)
                {
                    audit.ArtisticEligibleCount++;
                }
                if (record.Candidate)
                {
                    audit.CandidateCount++;
                }
                if (record.Selected)
                {
                    audit.SelectedCount++;
                }
                if (record.WidthInactive)
                {
                    audit.WidthInactiveCount++;
                    if (record.CoexistenceEligible || record.Selected)
                    {
                        audit.UnresolvedWidthInactiveCount++;
                    }
                }
                string coexistenceReason = string.IsNullOrEmpty(
                        record.CoexistenceFailureReason)
                    ? record.FinalReason
                    : record.CoexistenceFailureReason;
                if (record.ViabilityState ==
                        EdgeWearViabilityState.CoexistenceIneligible &&
                    string.Equals(
                        coexistenceReason,
                        "corner-width-missing",
                        StringComparison.Ordinal))
                {
                    audit.CornerWidthMissingExclusionCount++;
                    audit.CoexistencePreShellExclusionCount++;
                }
                else if (record.ViabilityState ==
                        EdgeWearViabilityState.CoexistenceIneligible &&
                    string.Equals(
                        coexistenceReason,
                        "corner-width-inactive",
                        StringComparison.Ordinal))
                {
                    audit.CornerWidthInactiveExclusionCount++;
                    audit.CoexistencePreShellExclusionCount++;
                }
                else if (record.ViabilityState ==
                        EdgeWearViabilityState.CoexistenceIneligible &&
                    (string.Equals(
                         coexistenceReason,
                         "recovery-baseline-deferred",
                         StringComparison.Ordinal) ||
                     string.Equals(
                         coexistenceReason,
                         "augmentation-forced-deferred",
                         StringComparison.Ordinal)))
                {
                    audit.CoexistencePreShellExclusionCount++;
                }
                if (record.WidthReduced)
                {
                    audit.WidthReducedCount++;
                }
                if (record.Active)
                {
                    audit.ActiveCount++;
                }
                if (record.AttemptedBuilt)
                {
                    audit.AttemptedBuiltCount++;
                }
                if (record.Built)
                {
                    audit.BuiltCount++;
                }
                if (record.TrialRejected)
                {
                    audit.TrialRejectedCount++;
                }
                if (record.Deferred)
                {
                    audit.DeferredCount++;
                }
                if (record.Rejected)
                {
                    audit.RejectedCount++;
                }
                if (record.SourceEdgeIndex < 0 &&
                    !record.MicroTopologySuppressed)
                {
                    audit.UnmappedCount++;
                }
            }
            audit.CoexistenceExclusionCount =
                audit.CoexistenceIneligibleCount;
            EvaluateEdgeWearCollateralPreservation(audit);
        }

        private static bool TryBuildChamferTopologyContext(
            List<PolygonFace> sourceFaces,
            List<EdgeWearBevelCandidate> candidates,
            int selectedCount,
            float minimumStableEdgeLength,
            ref ChamferReadinessStats stats,
            out ChamferTopologyContext context,
            out string blocker)
        {
            context = null;
            blocker = string.Empty;
            if (!TryBuildEdgeWearTopologyGraph(
                    sourceFaces,
                    out EdgeWearTopologyGraph graph,
                    out EdgeWearGraphBuildStats graphStats))
            {
                stats.ApplyGraphStats(graphStats);
                blocker = "source topology graph failed validation";
                return false;
            }

            if (!TryMapSelectedCandidatesToGraph(
                    graph,
                    candidates,
                    selectedCount,
                    out List<EdgeWearSelectedGraphEdge> selectedEdges,
                    ref graphStats))
            {
                stats.ApplyGraphStats(graphStats);
                blocker = "selected candidates did not map cleanly to source graph edges";
                return false;
            }

            stats.ApplyGraphStats(graphStats);
            stats.SelectedBoundaryEdgeCount = 0;
            stats.SelectedNonManifoldEdgeCount = 0;

            List<ChamferHalfEdge> halfEdges = BuildChamferHalfEdges(graph);

            for (int i = 0; i < selectedEdges.Count; i++)
            {
                EdgeWearGraphEdge edge = graph.Edges[selectedEdges[i].GraphEdgeIndex];
                if (edge.ExtraFaceCount > 0)
                {
                    stats.SelectedNonManifoldEdgeCount++;
                }
                else if (edge.FaceA < 0 || edge.FaceB < 0)
                {
                    stats.SelectedBoundaryEdgeCount++;
                }
            }

            TraceChamferBoundaryLoops(
                graph,
                halfEdges,
                ref stats);
            AuditChamferVertexFans(
                graph,
                halfEdges,
                ref stats);

            EdgeWearTopologyStats sourceTopology = AuditEdgeWearTopology(
                sourceFaces,
                minimumStableEdgeLength);
            stats.SourceNonManifoldEdgeCount = sourceTopology.NonManifoldEdgeCount;
            stats.SourceTJunctionCount = sourceTopology.TJunctionCount;

            bool ready =
                graphStats.InvalidFaceCount == 0 &&
                graphStats.InvalidEdgeCount == 0 &&
                graphStats.MissingSelectedGraphEdgeCount == 0 &&
                graphStats.MismatchedSelectedGraphFaceCount == 0 &&
                graphStats.DuplicateSelectedGraphEdgeCount == 0 &&
                stats.SourceNonManifoldEdgeCount == 0 &&
                stats.SourceTJunctionCount == 0 &&
                stats.SelectedBoundaryEdgeCount == 0 &&
                stats.SelectedNonManifoldEdgeCount == 0 &&
                stats.BoundaryTraceFailureCount == 0 &&
                stats.DisconnectedVertexFanCount == 0;

            stats.Ready = ready ? 1 : 0;
            if (!ready && string.IsNullOrEmpty(blocker))
            {
                blocker = "one or more EW-C topology readiness invariants failed";
            }

            if (ready)
            {
                context = new ChamferTopologyContext(
                    graph,
                    selectedEdges,
                    halfEdges);
            }

            return ready;
        }

        private static List<ChamferHalfEdge> BuildChamferHalfEdges(
            EdgeWearTopologyGraph graph)
        {
            List<ChamferHalfEdge> halfEdges = new List<ChamferHalfEdge>();
            Dictionary<long, int> directedByPair = new Dictionary<long, int>();

            for (int faceIndex = 0; faceIndex < graph.Faces.Count; faceIndex++)
            {
                EdgeWearGraphFace face = graph.Faces[faceIndex];
                int count = face.VertexIndices.Count;
                int firstHalfEdge = halfEdges.Count;
                for (int i = 0; i < count; i++)
                {
                    int origin = face.VertexIndices[i];
                    int destination = face.VertexIndices[(i + 1) % count];
                    ChamferHalfEdge halfEdge = new ChamferHalfEdge
                    {
                        Index = halfEdges.Count,
                        OriginVertex = origin,
                        DestinationVertex = destination,
                        FaceIndex = faceIndex,
                        SourceEdgeIndex = face.EdgeIndices[i],
                        Next = firstHalfEdge + ((i + 1) % count),
                        Previous = firstHalfEdge + ((i + count - 1) % count),
                        Opposite = -1,
                        IsSelected = graph.Edges[face.EdgeIndices[i]].Selected
                    };
                    halfEdges.Add(halfEdge);
                    directedByPair[PackDirectedVertexPair(origin, destination)] = halfEdge.Index;
                }
            }

            for (int i = 0; i < halfEdges.Count; i++)
            {
                ChamferHalfEdge halfEdge = halfEdges[i];
                if (directedByPair.TryGetValue(
                        PackDirectedVertexPair(
                            halfEdge.DestinationVertex,
                            halfEdge.OriginVertex),
                        out int opposite))
                {
                    halfEdge.Opposite = opposite;
                }
            }

            return halfEdges;
        }

        private static long PackDirectedVertexPair(int origin, int destination)
        {
            return ((long)origin << 32) | (uint)destination;
        }

        private static void TraceChamferBoundaryLoops(
            EdgeWearTopologyGraph graph,
            List<ChamferHalfEdge> halfEdges,
            ref ChamferReadinessStats stats)
        {
            Dictionary<int, List<int>> outgoingBoundaryByVertex =
                new Dictionary<int, List<int>>();
            for (int i = 0; i < halfEdges.Count; i++)
            {
                ChamferHalfEdge halfEdge = halfEdges[i];
                if (halfEdge.Opposite >= 0)
                {
                    continue;
                }

                if (!outgoingBoundaryByVertex.TryGetValue(
                        halfEdge.OriginVertex,
                        out List<int> outgoing))
                {
                    outgoing = new List<int>();
                    outgoingBoundaryByVertex.Add(halfEdge.OriginVertex, outgoing);
                }
                outgoing.Add(i);
            }

            HashSet<int> visited = new HashSet<int>();
            for (int i = 0; i < halfEdges.Count; i++)
            {
                if (halfEdges[i].Opposite >= 0 || visited.Contains(i))
                {
                    continue;
                }

                int current = i;
                int guard = 0;
                while (guard++ <= halfEdges.Count)
                {
                    if (!visited.Add(current))
                    {
                        if (current != i)
                        {
                            stats.BoundaryTraceFailureCount++;
                        }
                        break;
                    }

                    int destination = halfEdges[current].DestinationVertex;
                    if (!outgoingBoundaryByVertex.TryGetValue(
                            destination,
                            out List<int> nextCandidates) ||
                        nextCandidates.Count != 1)
                    {
                        stats.BoundaryTraceFailureCount++;
                        break;
                    }

                    current = nextCandidates[0];
                    if (current == i)
                    {
                        break;
                    }
                }

                if (guard > halfEdges.Count)
                {
                    stats.BoundaryTraceFailureCount++;
                }
            }
        }

        private static void AuditChamferVertexFans(
            EdgeWearTopologyGraph graph,
            List<ChamferHalfEdge> halfEdges,
            ref ChamferReadinessStats stats)
        {
            List<List<int>> outgoingByVertex = new List<List<int>>(graph.Vertices.Count);
            for (int i = 0; i < graph.Vertices.Count; i++)
            {
                outgoingByVertex.Add(new List<int>());
            }
            for (int i = 0; i < halfEdges.Count; i++)
            {
                outgoingByVertex[halfEdges[i].OriginVertex].Add(i);
            }

            for (int vertexIndex = 0; vertexIndex < graph.Vertices.Count; vertexIndex++)
            {
                List<int> outgoing = outgoingByVertex[vertexIndex];
                if (outgoing.Count == 0)
                {
                    continue;
                }

                bool affected = false;
                for (int i = 0; i < outgoing.Count; i++)
                {
                    affected |= halfEdges[outgoing[i]].IsSelected;
                }
                if (!affected)
                {
                    continue;
                }

                stats.AffectedVertexCount++;

                int start = -1;
                for (int i = 0; i < outgoing.Count; i++)
                {
                    ChamferHalfEdge candidate = halfEdges[outgoing[i]];
                    int previousOpposite = halfEdges[candidate.Previous].Opposite;
                    if (previousOpposite < 0)
                    {
                        start = candidate.Index;
                        break;
                    }
                }
                if (start < 0)
                {
                    start = outgoing[0];
                }

                List<int> ordered = new List<int>(outgoing.Count);
                HashSet<int> visited = new HashSet<int>();
                int current = start;
                int guard = 0;
                while (current >= 0 && guard++ <= outgoing.Count)
                {
                    if (!visited.Add(current))
                    {
                        break;
                    }
                    ordered.Add(current);

                    ChamferHalfEdge currentHalfEdge = halfEdges[current];
                    int next = currentHalfEdge.Opposite >= 0
                        ? halfEdges[currentHalfEdge.Opposite].Next
                        : -1;
                    if (next < 0 || next == start)
                    {
                        break;
                    }
                    if (halfEdges[next].OriginVertex != vertexIndex)
                    {
                        stats.DisconnectedVertexFanCount++;
                        break;
                    }
                    current = next;
                }

                if (ordered.Count != outgoing.Count)
                {
                    stats.DisconnectedVertexFanCount++;
                    continue;
                }


            }
        }



        private static bool AuditExplicitChamferCornerSolution(
            List<PolygonFace> sourceFaces,
            ChamferTopologyContext context,
            float requestedWidth,
            float minimumStableEdgeLength,
            float minimumStableFaceArea,
            EdgeWearCoverageAudit coverageAudit,
            HashSet<int> forcedDeferredEdges,
            ref ChamferCornerStats stats,
            out ChamferCornerSolution solution,
            out string blocker)
        {
            solution = null;
            blocker = string.Empty;
            stats.SourceFaceCount = context.Graph.Faces.Count;
            stats.ExpectedCornerCount = context.HalfEdges.Count;
            stats.SelectedEdgeCount = context.SelectedSourceEdges.Count;

            Dictionary<int, float> widthByEdge =
                new Dictionary<int, float>(context.SelectedSourceEdges.Count);
            foreach (int edgeIndex in context.SelectedSourceEdges)
            {
                if (forcedDeferredEdges != null &&
                    forcedDeferredEdges.Contains(edgeIndex))
                {
                    widthByEdge.Add(edgeIndex, 0f);
                    continue;
                }

                float edgeRequestedWidth =
                    ResolveEdgeWearRequestedWidth(
                        coverageAudit,
                        edgeIndex,
                        requestedWidth);
                float locallyCertifiedWidth = edgeRequestedWidth;
                if (coverageAudit != null &&
                    coverageAudit.ViabilityByGraphEdge.TryGetValue(
                        edgeIndex,
                        out EdgeWearEdgeViabilityRecord viability) &&
                    viability.MaximumLocallyFeasibleWidth >
                        PointMergeDistance)
                {
                    locallyCertifiedWidth = Mathf.Min(
                        locallyCertifiedWidth,
                        viability.MaximumLocallyFeasibleWidth);
                }
                float solvedWidth = CalculateChamferEdgeWidth(
                    context.Graph,
                    edgeIndex,
                    locallyCertifiedWidth,
                    minimumStableEdgeLength,
                    out bool clamped);
                if (solvedWidth < minimumStableEdgeLength ||
                    float.IsNaN(solvedWidth) ||
                    float.IsInfinity(solvedWidth))
                {
                    stats.WidthSolveFailures++;
                    blocker = "one or more selected edges have no stable chamfer width";
                    return false;
                }

                widthByEdge.Add(edgeIndex, solvedWidth);
            }

            List<ChamferCornerConflictRecord> cornerConflicts =
                new List<ChamferCornerConflictRecord>();
            if (!TrySolveCornerAwareChamferWidths(
                    sourceFaces,
                    context,
                    coverageAudit,
                    requestedWidth,
                    minimumStableEdgeLength,
                    widthByEdge,
                    cornerConflicts,
                    ref stats,
                    out blocker))
            {
                return false;
            }

            stats.MinimumSolvedWidth = float.PositiveInfinity;
            stats.MaximumSolvedWidth = 0f;
            foreach (KeyValuePair<int, float> pair in widthByEdge)
            {
                if (pair.Value <= PointMergeDistance)
                {
                    stats.DeferredSelectedEdgeCount++;
                    continue;
                }

                stats.ActiveSelectedEdgeCount++;
                stats.MinimumSolvedWidth = Mathf.Min(
                    stats.MinimumSolvedWidth,
                    pair.Value);
                stats.MaximumSolvedWidth = Mathf.Max(
                    stats.MaximumSolvedWidth,
                    pair.Value);
            }

            Dictionary<ChamferFaceCornerKey, ChamferSolvedCorner> corners =
                new Dictionary<ChamferFaceCornerKey, ChamferSolvedCorner>(
                    stats.ExpectedCornerCount);

            for (int faceIndex = 0;
                 faceIndex < context.Graph.Faces.Count;
                 faceIndex++)
            {
                EdgeWearGraphFace graphFace = context.Graph.Faces[faceIndex];
                PolygonFace sourceFace = sourceFaces[graphFace.SourceFaceIndex];
                Vector3 faceCentre = CalculateAverage(sourceFace.Vertices);
                int count = graphFace.VertexIndices.Count;

                for (int localIndex = 0; localIndex < count; localIndex++)
                {
                    int sourceVertexIndex = graphFace.VertexIndices[localIndex];
                    int previousEdgeIndex = graphFace.EdgeIndices[
                        (localIndex + count - 1) % count];
                    int nextEdgeIndex = graphFace.EdgeIndices[localIndex];
                    float previousWidthValue = 0f;
                    float nextWidthValue = 0f;
                    bool previousSelected =
                        context.SelectedSourceEdges.Contains(previousEdgeIndex) &&
                        widthByEdge.TryGetValue(previousEdgeIndex, out previousWidthValue) &&
                        previousWidthValue > PointMergeDistance;
                    bool nextSelected =
                        context.SelectedSourceEdges.Contains(nextEdgeIndex) &&
                        widthByEdge.TryGetValue(nextEdgeIndex, out nextWidthValue) &&
                        nextWidthValue > PointMergeDistance;
                    float previousWidth = previousSelected
                        ? previousWidthValue
                        : 0f;
                    float nextWidth = nextSelected
                        ? nextWidthValue
                        : 0f;

                    if (!TryBuildChamferFaceLine(
                            context.Graph,
                            previousEdgeIndex,
                            sourceFace.Normal,
                            faceCentre,
                            previousWidth,
                            out ChamferFaceLine previousLine) ||
                        !TryBuildChamferFaceLine(
                            context.Graph,
                            nextEdgeIndex,
                            sourceFace.Normal,
                            faceCentre,
                            nextWidth,
                            out ChamferFaceLine nextLine))
                    {
                        stats.CornerSolveFailures++;
                        blocker = "failed to build a stable face-edge support line";
                        return false;
                    }

                    Vector3 sourceVertex =
                        context.Graph.Vertices[sourceVertexIndex].Position;
                    if (!TrySolveChamferFaceCorner(
                            sourceVertex,
                            previousLine,
                            nextLine,
                            sourceFace.Normal,
                            minimumStableEdgeLength * 0.001f,
                            out Vector3 solved))
                    {
                        stats.CornerSolveFailures++;
                        blocker = "one or more face corners have parallel or unstable offset lines";
                        return false;
                    }

                    if (!IsFinite(solved))
                    {
                        blocker = "one or more solved face corners are non-finite";
                        return false;
                    }

                    float previousLength = GetGraphEdgeLength(
                        context.Graph,
                        previousEdgeIndex);
                    float nextLength = GetGraphEdgeLength(
                        context.Graph,
                        nextEdgeIndex);
                    float localRequestedWidth =
                        ResolveChamferCornerRequestedWidth(
                            coverageAudit,
                            widthByEdge,
                            previousEdgeIndex,
                            nextEdgeIndex,
                            requestedWidth);
                    float localLimit = CalculateChamferCornerDisplacementLimit(
                        localRequestedWidth,
                        minimumStableEdgeLength,
                        previousLength,
                        nextLength);
                    float displacement = (solved - sourceVertex).magnitude;
                    UpdateChamferFinalWorstCorner(
                        faceIndex,
                        sourceVertexIndex,
                        previousEdgeIndex,
                        nextEdgeIndex,
                        displacement,
                        localLimit,
                        ref stats);
                    if (displacement > localLimit + PointMergeDistance)
                    {
                        blocker = "one or more solved corners still exceed the conservative local displacement limit after width solving";
                        return false;
                    }


                    corners.Add(
                        new ChamferFaceCornerKey(faceIndex, sourceVertexIndex),
                        new ChamferSolvedCorner(
                            solved,
                            faceIndex,
                            sourceVertexIndex,
                            previousEdgeIndex,
                            nextEdgeIndex,
                            previousSelected,
                            nextSelected));
                }
            }

            if (!TryBuildChamferSharedEdgeSpans(
                    context,
                    corners,
                    widthByEdge,
                    minimumStableEdgeLength,
                    out Dictionary<int, ChamferSharedEdgeSpan> sharedSpans,
                    ref stats,
                    out blocker))
            {
                return false;
            }

            if (!AuditChamferReplacementFaces(
                    sourceFaces,
                    context,
                    corners,
                    sharedSpans,
                    minimumStableEdgeLength,
                    minimumStableFaceArea,
                    ref stats,
                    out blocker))
            {
                return false;
            }

            if (!AuditChamferSelectedRails(
                    context,
                    corners,
                    widthByEdge,
                    minimumStableEdgeLength,
                    ref stats,
                    out blocker))
            {
                return false;
            }

            if (!AuditChamferSolvedBoundary(
                    context,
                    corners,
                    minimumStableEdgeLength,
                    ref stats,
                    out blocker))
            {
                return false;
            }

            if (float.IsPositiveInfinity(stats.MinimumSolvedWidth))
            {
                stats.MinimumSolvedWidth = 0f;
            }
            solution = new ChamferCornerSolution(
                corners,
                widthByEdge,
                sharedSpans,
                cornerConflicts,
                forcedDeferredEdges);
            return true;
        }

        private static bool TrySolveCornerAwareChamferWidths(
            List<PolygonFace> sourceFaces,
            ChamferTopologyContext context,
            EdgeWearCoverageAudit coverageAudit,
            float requestedWidth,
            float minimumStableEdgeLength,
            Dictionary<int, float> widthByEdge,
            List<ChamferCornerConflictRecord> cornerConflicts,
            ref ChamferCornerStats stats,
            out string blocker)
        {
            blocker = string.Empty;
            const int MaximumPasses = 12;
            const float SafetyScale = 0.95f;
            HashSet<int> cornerClampedEdges = new HashSet<int>();
            HashSet<int> sharedEdgeClampedEdges = new HashSet<int>();
            stats.MinimumCornerWidthScale = 1f;
            stats.MinimumSharedEdgeWidthScale = 1f;

            for (int pass = 0; pass < MaximumPasses; pass++)
            {
                if (!TryBuildChamferCornerTable(
                        sourceFaces,
                        context,
                        widthByEdge,
                        minimumStableEdgeLength,
                        out Dictionary<ChamferFaceCornerKey, ChamferSolvedCorner> passCorners,
                        out blocker))
                {
                    return false;
                }

                bool changed = false;
                foreach (ChamferSolvedCorner corner in passCorners.Values)
                {
                    Vector3 sourceVertex =
                        context.Graph.Vertices[corner.SourceVertexIndex].Position;
                    float previousLength = GetGraphEdgeLength(
                        context.Graph,
                        corner.PreviousSourceEdgeIndex);
                    float nextLength = GetGraphEdgeLength(
                        context.Graph,
                        corner.NextSourceEdgeIndex);
                    float localRequestedWidth =
                        ResolveChamferCornerRequestedWidth(
                            coverageAudit,
                            widthByEdge,
                            corner.PreviousSourceEdgeIndex,
                            corner.NextSourceEdgeIndex,
                            requestedWidth);
                    float localLimit = CalculateChamferCornerDisplacementLimit(
                        localRequestedWidth,
                        minimumStableEdgeLength,
                        previousLength,
                        nextLength);
                    float displacement = (corner.Position - sourceVertex).magnitude;

                    if (pass == 0)
                    {
                        UpdateChamferInitialWorstCorner(
                            corner.FaceIndex,
                            corner.SourceVertexIndex,
                            corner.PreviousSourceEdgeIndex,
                            corner.NextSourceEdgeIndex,
                            displacement,
                            localLimit,
                            ref stats);
                    }

                    if (displacement <= localLimit + PointMergeDistance)
                    {
                        continue;
                    }

                    float scale = Mathf.Clamp01(
                        SafetyScale * localLimit / displacement);
                    bool cornerChanged = false;
                    if (corner.PreviousSelected &&
                        TryClampChamferEdgeWidth(
                            corner.PreviousSourceEdgeIndex,
                            scale,
                            ResolveEdgeWearRequestedWidth(
                                coverageAudit,
                                corner.PreviousSourceEdgeIndex,
                                requestedWidth),
                            minimumStableEdgeLength,
                            widthByEdge,
                            cornerClampedEdges,
                            ref stats))
                    {
                        cornerChanged = true;
                    }
                    if (corner.NextSelected &&
                        corner.NextSourceEdgeIndex != corner.PreviousSourceEdgeIndex &&
                        TryClampChamferEdgeWidth(
                            corner.NextSourceEdgeIndex,
                            scale,
                            ResolveEdgeWearRequestedWidth(
                                coverageAudit,
                                corner.NextSourceEdgeIndex,
                                requestedWidth),
                            minimumStableEdgeLength,
                            widthByEdge,
                            cornerClampedEdges,
                            ref stats))
                    {
                        cornerChanged = true;
                    }

                    if (!cornerChanged)
                    {
                        blocker = "a corner remains over its displacement limit at the minimum stable chamfer width";
                        return false;
                    }
                    changed = true;
                }

                if (changed)
                {
                    continue;
                }

                for (int edgeIndex = 0;
                     edgeIndex < context.Graph.Edges.Count;
                     edgeIndex++)
                {
                    EdgeWearGraphEdge edge = context.Graph.Edges[edgeIndex];
                    bool activeSelected = edge.Selected &&
                        widthByEdge.TryGetValue(edgeIndex, out float activeWidth) &&
                        activeWidth > PointMergeDistance;
                    if (activeSelected || edge.FaceA < 0 || edge.FaceB < 0)
                    {
                        continue;
                    }

                    if (HasStableChamferSharedInterval(
                            context,
                            edgeIndex,
                            passCorners,
                            minimumStableEdgeLength))
                    {
                        continue;
                    }

                    HashSet<int> participatingEdges =
                        CollectChamferSharedEdgeParticipatingSelectedEdges(
                            edge,
                            passCorners);
                    if (participatingEdges.Count == 0 ||
                        !TryFindChamferSharedEdgeWidthScale(
                            sourceFaces,
                            context,
                            edgeIndex,
                            participatingEdges,
                            widthByEdge,
                            minimumStableEdgeLength,
                            out float solvedScale,
                            out blocker))
                    {
                        if (string.IsNullOrEmpty(blocker))
                        {
                            blocker = "an unselected internal edge has no stable common interval even at zero adjacent chamfer width";
                        }
                        return false;
                    }

                    List<int> orderedParticipants =
                        new List<int>(participatingEdges);
                    orderedParticipants.Sort();
                    Dictionary<int, float> participantWidthsBeforeScale =
                        new Dictionary<int, float>(
                            orderedParticipants.Count);
                    for (int participantIndex = 0;
                         participantIndex < orderedParticipants.Count;
                         participantIndex++)
                    {
                        int participantEdge =
                            orderedParticipants[participantIndex];
                        participantWidthsBeforeScale[participantEdge] =
                            widthByEdge[participantEdge];
                    }

                    bool edgeChanged = false;
                    List<int> uniformlyZeroedParticipants =
                        new List<int>();
                    foreach (int selectedEdgeIndex in participatingEdges)
                    {
                        float oldWidth = widthByEdge[selectedEdgeIndex];
                        float scaledWidth = oldWidth * solvedScale;
                        float newWidth = scaledWidth < minimumStableEdgeLength
                            ? 0f
                            : scaledWidth;
                        if (newWidth >= oldWidth - PointMergeDistance)
                        {
                            continue;
                        }

                        widthByEdge[selectedEdgeIndex] = newWidth;
                        sharedEdgeClampedEdges.Add(selectedEdgeIndex);
                        if (oldWidth > PointMergeDistance &&
                            newWidth <= PointMergeDistance)
                        {
                            uniformlyZeroedParticipants.Add(
                                selectedEdgeIndex);
                        }
                        float edgeRequestedWidth =
                            ResolveEdgeWearRequestedWidth(
                                coverageAudit,
                                selectedEdgeIndex,
                                requestedWidth);
                        float relativeScale = edgeRequestedWidth >
                            PointMergeDistance
                                ? newWidth / edgeRequestedWidth
                                : 1f;
                        stats.MinimumSharedEdgeWidthScale = Mathf.Min(
                            stats.MinimumSharedEdgeWidthScale,
                            relativeScale);
                        edgeChanged = true;
                    }

                    if (uniformlyZeroedParticipants.Count > 0)
                    {
                        RecordChamferCornerConflict(
                            cornerConflicts,
                            edgeIndex,
                            solvedScale,
                            ChamferCornerZeroingStage
                                .SharedEdgeUniformScale,
                            orderedParticipants,
                            uniformlyZeroedParticipants,
                            participantWidthsBeforeScale);
                    }

                    if (!edgeChanged)
                    {
                        List<int> forcedZeroedParticipants =
                            new List<int>();
                        foreach (int selectedEdgeIndex in participatingEdges)
                        {
                            if (widthByEdge.TryGetValue(
                                    selectedEdgeIndex,
                                    out float currentWidth) &&
                                currentWidth > PointMergeDistance)
                            {
                                forcedZeroedParticipants.Add(
                                    selectedEdgeIndex);
                            }
                        }
                        if (forcedZeroedParticipants.Count == 0)
                        {
                            blocker = "an unselected internal edge remains unstable after all participating chamfers were deferred";
                            return false;
                        }

                        RecordChamferCornerConflict(
                            cornerConflicts,
                            edgeIndex,
                            0f,
                            ChamferCornerZeroingStage
                                .SharedEdgeForcedDeferral,
                            orderedParticipants,
                            forcedZeroedParticipants,
                            participantWidthsBeforeScale);
                        for (int deferredIndex = 0;
                             deferredIndex <
                                 forcedZeroedParticipants.Count;
                             deferredIndex++)
                        {
                            int selectedEdgeIndex =
                                forcedZeroedParticipants[deferredIndex];
                            widthByEdge[selectedEdgeIndex] = 0f;
                            sharedEdgeClampedEdges.Add(selectedEdgeIndex);
                        }
                    }
                    changed = true;
                }

                if (!changed)
                {
                    return true;
                }
            }

            blocker = "unified corner and shared-edge width solving did not converge";
            return false;
        }

        private static void RecordChamferCornerConflict(
            List<ChamferCornerConflictRecord> cornerConflicts,
            int unselectedSourceEdgeIndex,
            float uniformScale,
            ChamferCornerZeroingStage zeroingStage,
            List<int> orderedParticipants,
            List<int> zeroedParticipants,
            Dictionary<int, float> participantWidthsBeforeScale)
        {
            if (cornerConflicts == null ||
                orderedParticipants == null ||
                orderedParticipants.Count == 0)
            {
                return;
            }

            ChamferCornerConflictRecord conflict =
                new ChamferCornerConflictRecord
                {
                    UnselectedSourceEdgeIndex =
                        unselectedSourceEdgeIndex,
                    UniformScale = uniformScale,
                    ZeroingStage = zeroingStage
                };
            conflict.ParticipatingSelectedEdges.AddRange(
                orderedParticipants);
            if (zeroedParticipants != null)
            {
                zeroedParticipants.Sort();
                conflict.ZeroedSelectedEdges.AddRange(
                    zeroedParticipants);
            }
            for (int participantIndex = 0;
                 participantIndex < orderedParticipants.Count;
                 participantIndex++)
            {
                int participantEdge =
                    orderedParticipants[participantIndex];
                if (participantWidthsBeforeScale != null &&
                    participantWidthsBeforeScale.TryGetValue(
                        participantEdge,
                        out float participantWidth))
                {
                    conflict.ParticipantWidthBeforeScale[
                        participantEdge] = participantWidth;
                }
            }
            cornerConflicts.Add(conflict);
        }

        private static bool TryBuildChamferCornerTable(
            List<PolygonFace> sourceFaces,
            ChamferTopologyContext context,
            Dictionary<int, float> widthByEdge,
            float minimumStableEdgeLength,
            out Dictionary<ChamferFaceCornerKey, ChamferSolvedCorner> corners,
            out string blocker)
        {
            blocker = string.Empty;
            corners = new Dictionary<ChamferFaceCornerKey, ChamferSolvedCorner>(
                context.HalfEdges.Count);

            for (int faceIndex = 0;
                 faceIndex < context.Graph.Faces.Count;
                 faceIndex++)
            {
                EdgeWearGraphFace graphFace = context.Graph.Faces[faceIndex];
                PolygonFace sourceFace = sourceFaces[graphFace.SourceFaceIndex];
                Vector3 faceCentre = CalculateAverage(sourceFace.Vertices);
                int count = graphFace.VertexIndices.Count;

                for (int localIndex = 0; localIndex < count; localIndex++)
                {
                    int sourceVertexIndex = graphFace.VertexIndices[localIndex];
                    int previousEdgeIndex = graphFace.EdgeIndices[
                        (localIndex + count - 1) % count];
                    int nextEdgeIndex = graphFace.EdgeIndices[localIndex];
                    float previousWidthValue = 0f;
                    float nextWidthValue = 0f;
                    bool previousSelected =
                        context.SelectedSourceEdges.Contains(previousEdgeIndex) &&
                        widthByEdge.TryGetValue(previousEdgeIndex, out previousWidthValue) &&
                        previousWidthValue > PointMergeDistance;
                    bool nextSelected =
                        context.SelectedSourceEdges.Contains(nextEdgeIndex) &&
                        widthByEdge.TryGetValue(nextEdgeIndex, out nextWidthValue) &&
                        nextWidthValue > PointMergeDistance;
                    float previousWidth = previousSelected
                        ? previousWidthValue
                        : 0f;
                    float nextWidth = nextSelected
                        ? nextWidthValue
                        : 0f;

                    if (!TryBuildChamferFaceLine(
                            context.Graph,
                            previousEdgeIndex,
                            sourceFace.Normal,
                            faceCentre,
                            previousWidth,
                            out ChamferFaceLine previousLine) ||
                        !TryBuildChamferFaceLine(
                            context.Graph,
                            nextEdgeIndex,
                            sourceFace.Normal,
                            faceCentre,
                            nextWidth,
                            out ChamferFaceLine nextLine))
                    {
                        blocker = "failed to build a stable support line during chamfer width solving";
                        return false;
                    }

                    Vector3 sourceVertex =
                        context.Graph.Vertices[sourceVertexIndex].Position;
                    if (!TrySolveChamferFaceCorner(
                            sourceVertex,
                            previousLine,
                            nextLine,
                            sourceFace.Normal,
                            minimumStableEdgeLength * 0.001f,
                            out Vector3 solved) ||
                        !IsFinite(solved))
                    {
                        blocker = "failed to solve a finite corner during chamfer width solving";
                        return false;
                    }

                    corners.Add(
                        new ChamferFaceCornerKey(faceIndex, sourceVertexIndex),
                        new ChamferSolvedCorner(
                            solved,
                            faceIndex,
                            sourceVertexIndex,
                            previousEdgeIndex,
                            nextEdgeIndex,
                            previousSelected,
                            nextSelected));
                }
            }

            return true;
        }

        private static bool HasStableChamferSharedInterval(
            ChamferTopologyContext context,
            int edgeIndex,
            Dictionary<ChamferFaceCornerKey, ChamferSolvedCorner> corners,
            float minimumStableEdgeLength)
        {
            EdgeWearGraphEdge edge = context.Graph.Edges[edgeIndex];
            if (edge.FaceA < 0 || edge.FaceB < 0)
            {
                return true;
            }

            Vector3 sourceA = context.Graph.Vertices[edge.VertexA].Position;
            Vector3 sourceB = context.Graph.Vertices[edge.VertexB].Position;
            Vector3 edgeVector = sourceB - sourceA;
            float edgeLength = edgeVector.magnitude;
            if (edgeLength <= PointMergeDistance)
            {
                return false;
            }
            Vector3 direction = edgeVector / edgeLength;

            ChamferSolvedCorner aA = corners[
                new ChamferFaceCornerKey(edge.FaceA, edge.VertexA)];
            ChamferSolvedCorner aB = corners[
                new ChamferFaceCornerKey(edge.FaceA, edge.VertexB)];
            ChamferSolvedCorner bA = corners[
                new ChamferFaceCornerKey(edge.FaceB, edge.VertexA)];
            ChamferSolvedCorner bB = corners[
                new ChamferFaceCornerKey(edge.FaceB, edge.VertexB)];

            float a0 = Vector3.Dot(aA.Position - sourceA, direction);
            float a1 = Vector3.Dot(aB.Position - sourceA, direction);
            float b0 = Vector3.Dot(bA.Position - sourceA, direction);
            float b1 = Vector3.Dot(bB.Position - sourceA, direction);
            float sharedStart = Mathf.Max(Mathf.Min(a0, a1), Mathf.Min(b0, b1));
            float sharedEnd = Mathf.Min(Mathf.Max(a0, a1), Mathf.Max(b0, b1));
            float requiredSharedLength = Mathf.Min(
                minimumStableEdgeLength,
                edgeLength);
            return sharedEnd - sharedStart + PointMergeDistance >=
                requiredSharedLength;
        }

        private static HashSet<int> CollectChamferSharedEdgeParticipatingSelectedEdges(
            EdgeWearGraphEdge edge,
            Dictionary<ChamferFaceCornerKey, ChamferSolvedCorner> corners)
        {
            HashSet<int> selectedEdges = new HashSet<int>();
            ChamferSolvedCorner[] relatedCorners =
            {
                corners[new ChamferFaceCornerKey(edge.FaceA, edge.VertexA)],
                corners[new ChamferFaceCornerKey(edge.FaceA, edge.VertexB)],
                corners[new ChamferFaceCornerKey(edge.FaceB, edge.VertexA)],
                corners[new ChamferFaceCornerKey(edge.FaceB, edge.VertexB)]
            };

            for (int i = 0; i < relatedCorners.Length; i++)
            {
                ChamferSolvedCorner corner = relatedCorners[i];
                if (corner.PreviousSelected)
                {
                    selectedEdges.Add(corner.PreviousSourceEdgeIndex);
                }
                if (corner.NextSelected)
                {
                    selectedEdges.Add(corner.NextSourceEdgeIndex);
                }
            }
            return selectedEdges;
        }

        private static bool TryFindChamferSharedEdgeWidthScale(
            List<PolygonFace> sourceFaces,
            ChamferTopologyContext context,
            int unselectedEdgeIndex,
            HashSet<int> participatingEdges,
            Dictionary<int, float> widthByEdge,
            float minimumStableEdgeLength,
            out float solvedScale,
            out string blocker)
        {
            blocker = string.Empty;
            solvedScale = 0f;
            Dictionary<int, float> testWidths =
                new Dictionary<int, float>(widthByEdge);

            foreach (int edgeIndex in participatingEdges)
            {
                testWidths[edgeIndex] = 0f;
            }
            if (!TryBuildChamferCornerTable(
                    sourceFaces,
                    context,
                    testWidths,
                    minimumStableEdgeLength,
                    out Dictionary<ChamferFaceCornerKey, ChamferSolvedCorner> zeroCorners,
                    out blocker) ||
                !HasStableChamferSharedInterval(
                    context,
                    unselectedEdgeIndex,
                    zeroCorners,
                    minimumStableEdgeLength))
            {
                return false;
            }

            float low = 0f;
            float high = 1f;
            for (int iteration = 0; iteration < 12; iteration++)
            {
                float middle = (low + high) * 0.5f;
                testWidths.Clear();
                foreach (KeyValuePair<int, float> pair in widthByEdge)
                {
                    testWidths.Add(pair.Key, pair.Value);
                }
                foreach (int edgeIndex in participatingEdges)
                {
                    testWidths[edgeIndex] = widthByEdge[edgeIndex] * middle;
                }

                if (!TryBuildChamferCornerTable(
                        sourceFaces,
                        context,
                        testWidths,
                        minimumStableEdgeLength,
                        out Dictionary<ChamferFaceCornerKey, ChamferSolvedCorner> testCorners,
                        out blocker))
                {
                    return false;
                }

                if (HasStableChamferSharedInterval(
                        context,
                        unselectedEdgeIndex,
                        testCorners,
                        minimumStableEdgeLength))
                {
                    low = middle;
                }
                else
                {
                    high = middle;
                }
            }

            solvedScale = low * 0.95f;
            return solvedScale > 0f;
        }

        private static void UpdateChamferInitialWorstCorner(
            int faceIndex,
            int sourceVertexIndex,
            int previousEdgeIndex,
            int nextEdgeIndex,
            float displacement,
            float limit,
            ref ChamferCornerStats stats)
        {
            if (displacement <= stats.InitialMaximumCornerDisplacement)
            {
                return;
            }

            stats.InitialMaximumCornerDisplacement = displacement;
        }

        private static float ResolveEdgeWearRequestedWidth(
            EdgeWearCoverageAudit coverageAudit,
            int graphEdgeIndex,
            float fallbackRequestedWidth)
        {
            if (coverageAudit != null &&
                coverageAudit.ViabilityByGraphEdge.TryGetValue(
                    graphEdgeIndex,
                    out EdgeWearEdgeViabilityRecord viability) &&
                viability != null &&
                viability.RequestedWidth > PointMergeDistance)
            {
                return viability.RequestedWidth;
            }

            return fallbackRequestedWidth;
        }

        private static float ResolveChamferCornerRequestedWidth(
            EdgeWearCoverageAudit coverageAudit,
            Dictionary<int, float> widthByEdge,
            int previousEdgeIndex,
            int nextEdgeIndex,
            float fallbackRequestedWidth)
        {
            float localRequestedWidth = 0f;
            if (widthByEdge != null &&
                widthByEdge.TryGetValue(
                    previousEdgeIndex,
                    out float previousWidth) &&
                previousWidth > PointMergeDistance)
            {
                localRequestedWidth = Mathf.Max(
                    localRequestedWidth,
                    ResolveEdgeWearRequestedWidth(
                        coverageAudit,
                        previousEdgeIndex,
                        fallbackRequestedWidth));
            }
            if (widthByEdge != null &&
                widthByEdge.TryGetValue(
                    nextEdgeIndex,
                    out float nextWidth) &&
                nextWidth > PointMergeDistance)
            {
                localRequestedWidth = Mathf.Max(
                    localRequestedWidth,
                    ResolveEdgeWearRequestedWidth(
                        coverageAudit,
                        nextEdgeIndex,
                        fallbackRequestedWidth));
            }

            return localRequestedWidth > PointMergeDistance
                ? localRequestedWidth
                : fallbackRequestedWidth;
        }

        private static bool TryClampChamferEdgeWidth(
            int edgeIndex,
            float scale,
            float requestedWidth,
            float minimumStableEdgeLength,
            Dictionary<int, float> widthByEdge,
            HashSet<int> clampedEdges,
            ref ChamferCornerStats stats)
        {
            float oldWidth = widthByEdge[edgeIndex];
            float newWidth = Mathf.Max(
                minimumStableEdgeLength,
                oldWidth * scale);
            if (newWidth >= oldWidth - PointMergeDistance)
            {
                return false;
            }

            widthByEdge[edgeIndex] = newWidth;
            clampedEdges.Add(edgeIndex);
            float relativeScale = requestedWidth > PointMergeDistance
                ? newWidth / requestedWidth
                : 1f;
            stats.MinimumCornerWidthScale = Mathf.Min(
                stats.MinimumCornerWidthScale,
                relativeScale);
            return true;
        }

        private static float CalculateChamferCornerDisplacementLimit(
            float requestedWidth,
            float minimumStableEdgeLength,
            float previousLength,
            float nextLength)
        {
            return Mathf.Max(
                requestedWidth * 4f,
                Mathf.Max(
                    minimumStableEdgeLength,
                    Mathf.Min(previousLength, nextLength) * 0.45f));
        }

        private static void UpdateChamferFinalWorstCorner(
            int faceIndex,
            int sourceVertexIndex,
            int previousEdgeIndex,
            int nextEdgeIndex,
            float displacement,
            float limit,
            ref ChamferCornerStats stats)
        {
            if (displacement <= stats.FinalMaximumCornerDisplacement)
            {
                return;
            }

            stats.FinalMaximumCornerDisplacement = displacement;
        }

        private static float CalculateChamferEdgeWidth(
            EdgeWearTopologyGraph graph,
            int edgeIndex,
            float requestedWidth,
            float minimumStableEdgeLength,
            out bool clamped)
        {
            EdgeWearGraphEdge edge = graph.Edges[edgeIndex];
            float maximumWidth = requestedWidth;
            AccumulateChamferEndpointWidthLimit(
                graph,
                edge.FaceA,
                edge.VertexA,
                edgeIndex,
                ref maximumWidth);
            AccumulateChamferEndpointWidthLimit(
                graph,
                edge.FaceA,
                edge.VertexB,
                edgeIndex,
                ref maximumWidth);
            AccumulateChamferEndpointWidthLimit(
                graph,
                edge.FaceB,
                edge.VertexA,
                edgeIndex,
                ref maximumWidth);
            AccumulateChamferEndpointWidthLimit(
                graph,
                edge.FaceB,
                edge.VertexB,
                edgeIndex,
                ref maximumWidth);

            clamped = maximumWidth < requestedWidth - PointMergeDistance;
            return Mathf.Max(minimumStableEdgeLength, maximumWidth);
        }

        private static void AccumulateChamferEndpointWidthLimit(
            EdgeWearTopologyGraph graph,
            int faceIndex,
            int vertexIndex,
            int selectedEdgeIndex,
            ref float maximumWidth)
        {
            if (faceIndex < 0 || faceIndex >= graph.Faces.Count)
            {
                return;
            }

            EdgeWearGraphFace face = graph.Faces[faceIndex];
            int localIndex = face.VertexIndices.IndexOf(vertexIndex);
            if (localIndex < 0)
            {
                return;
            }

            int count = face.VertexIndices.Count;
            int previousEdge = face.EdgeIndices[(localIndex + count - 1) % count];
            int nextEdge = face.EdgeIndices[localIndex];
            int adjacentEdge = previousEdge == selectedEdgeIndex
                ? nextEdge
                : previousEdge;
            maximumWidth = Mathf.Min(
                maximumWidth,
                GetGraphEdgeLength(graph, adjacentEdge) * 0.25f);
        }

        private static float GetGraphEdgeLength(
            EdgeWearTopologyGraph graph,
            int edgeIndex)
        {
            EdgeWearGraphEdge edge = graph.Edges[edgeIndex];
            return Vector3.Distance(
                graph.Vertices[edge.VertexA].Position,
                graph.Vertices[edge.VertexB].Position);
        }

        private static bool TryBuildChamferFaceLine(
            EdgeWearTopologyGraph graph,
            int edgeIndex,
            Vector3 faceNormal,
            Vector3 faceCentre,
            float offset,
            out ChamferFaceLine line)
        {
            EdgeWearGraphEdge edge = graph.Edges[edgeIndex];
            Vector3 start = graph.Vertices[edge.VertexA].Position;
            Vector3 end = graph.Vertices[edge.VertexB].Position;
            Vector3 edgeVector = end - start;
            float length = edgeVector.magnitude;
            if (length <= PointMergeDistance || !IsFinite(edgeVector))
            {
                line = default;
                return false;
            }

            Vector3 direction = edgeVector / length;
            Vector3 inward = Vector3.Cross(faceNormal, direction).normalized;
            Vector3 midpoint = (start + end) * 0.5f;
            if (Vector3.Dot(faceCentre - midpoint, inward) < 0f)
            {
                inward = -inward;
            }

            line = new ChamferFaceLine(
                start + inward * offset,
                direction,
                edgeIndex,
                offset);
            return IsFinite(line.Point) && IsFinite(line.Direction);
        }

        private static bool TrySolveChamferFaceCorner(
            Vector3 sourceVertex,
            ChamferFaceLine previousLine,
            ChamferFaceLine nextLine,
            Vector3 faceNormal,
            float parallelTolerance,
            out Vector3 solved)
        {
            if (previousLine.Offset <= 0f && nextLine.Offset <= 0f)
            {
                solved = sourceVertex;
                return true;
            }

            float denominator = Vector3.Dot(
                Vector3.Cross(previousLine.Direction, nextLine.Direction),
                faceNormal);
            if (Mathf.Abs(denominator) <= parallelTolerance)
            {
                solved = Vector3.zero;
                return false;
            }

            float t = Vector3.Dot(
                Vector3.Cross(
                    nextLine.Point - previousLine.Point,
                    nextLine.Direction),
                faceNormal) / denominator;
            solved = previousLine.Point + previousLine.Direction * t;
            return IsFinite(solved);
        }

        private static bool TryBuildChamferSharedEdgeSpans(
            ChamferTopologyContext context,
            Dictionary<ChamferFaceCornerKey, ChamferSolvedCorner> corners,
            Dictionary<int, float> widthByEdge,
            float minimumStableEdgeLength,
            out Dictionary<int, ChamferSharedEdgeSpan> sharedSpans,
            ref ChamferCornerStats stats,
            out string blocker)
        {
            blocker = string.Empty;
            sharedSpans = new Dictionary<int, ChamferSharedEdgeSpan>();
            for (int edgeIndex = 0;
                 edgeIndex < context.Graph.Edges.Count;
                 edgeIndex++)
            {
                EdgeWearGraphEdge edge = context.Graph.Edges[edgeIndex];
                bool activeSelected = edge.Selected &&
                    widthByEdge.TryGetValue(edgeIndex, out float activeWidth) &&
                    activeWidth > PointMergeDistance;
                if (activeSelected || edge.FaceA < 0 || edge.FaceB < 0)
                {
                    continue;
                }

                Vector3 sourceA = context.Graph.Vertices[edge.VertexA].Position;
                Vector3 sourceB = context.Graph.Vertices[edge.VertexB].Position;
                Vector3 edgeVector = sourceB - sourceA;
                float edgeLength = edgeVector.magnitude;
                if (edgeLength <= PointMergeDistance)
                {
                    blocker = "an inactive internal source edge is degenerate";
                    return false;
                }
                Vector3 direction = edgeVector / edgeLength;

                ChamferSolvedCorner aA = corners[
                    new ChamferFaceCornerKey(edge.FaceA, edge.VertexA)];
                ChamferSolvedCorner aB = corners[
                    new ChamferFaceCornerKey(edge.FaceA, edge.VertexB)];
                ChamferSolvedCorner bA = corners[
                    new ChamferFaceCornerKey(edge.FaceB, edge.VertexA)];
                ChamferSolvedCorner bB = corners[
                    new ChamferFaceCornerKey(edge.FaceB, edge.VertexB)];

                float a0 = Vector3.Dot(aA.Position - sourceA, direction);
                float a1 = Vector3.Dot(aB.Position - sourceA, direction);
                float b0 = Vector3.Dot(bA.Position - sourceA, direction);
                float b1 = Vector3.Dot(bB.Position - sourceA, direction);
                float sharedStart = Mathf.Max(Mathf.Min(a0, a1), Mathf.Min(b0, b1));
                float sharedEnd = Mathf.Min(Mathf.Max(a0, a1), Mathf.Max(b0, b1));
                float requiredSharedLength = Mathf.Min(minimumStableEdgeLength, edgeLength);
                if (sharedEnd - sharedStart + PointMergeDistance < requiredSharedLength)
                {
                    blocker = "incident faces have no stable common span on an inactive internal edge";
                    return false;
                }

                Vector3 sharedPointA = sourceA + direction * sharedStart;
                Vector3 sharedPointB = sourceA + direction * sharedEnd;
                sharedSpans.Add(
                    edgeIndex,
                    new ChamferSharedEdgeSpan(
                        edgeIndex,
                        edge.FaceA,
                        edge.FaceB,
                        edge.VertexA,
                        edge.VertexB,
                        sharedPointA,
                        sharedPointB));

            }
            return true;
        }

        private static bool AuditChamferReplacementFaces(
            List<PolygonFace> sourceFaces,
            ChamferTopologyContext context,
            Dictionary<ChamferFaceCornerKey, ChamferSolvedCorner> corners,
            Dictionary<int, ChamferSharedEdgeSpan> sharedSpans,
            float minimumStableEdgeLength,
            float minimumStableFaceArea,
            ref ChamferCornerStats stats,
            out string blocker)
        {
            blocker = string.Empty;
            for (int faceIndex = 0;
                 faceIndex < context.Graph.Faces.Count;
                 faceIndex++)
            {
                EdgeWearGraphFace graphFace = context.Graph.Faces[faceIndex];
                PolygonFace sourceFace = sourceFaces[graphFace.SourceFaceIndex];
                List<Vector3> solvedFace = new List<Vector3>(
                    graphFace.VertexIndices.Count * 3);
                for (int i = 0; i < graphFace.VertexIndices.Count; i++)
                {
                    int startVertex = graphFace.VertexIndices[i];
                    int endVertex = graphFace.VertexIndices[
                        (i + 1) % graphFace.VertexIndices.Count];
                    int sourceEdgeIndex = graphFace.EdgeIndices[i];
                    AppendChamferReplacementEdgeChain(
                        faceIndex,
                        startVertex,
                        endVertex,
                        sourceEdgeIndex,
                        corners,
                        sharedSpans,
                        solvedFace,
                        null);
                }
                ReduceChamferFaceRetraces(solvedFace, null);

                if (solvedFace.Count < 3)
                {
                    stats.ReplacementEdgeCollapseFailureCount++;
                    blocker = "a replacement face collapses below three vertices after exact retrace removal";
                    return false;
                }
                if (!TryFindDuplicateChamferFaceEdge(
                        solvedFace,
                        out _,
                        out _,
                        out _))
                {
                    blocker = "a replacement face contains a repeated non-retrace topology edge";
                    return false;
                }

                if (CalculatePolygonArea(solvedFace) <= minimumStableFaceArea)
                {
                    stats.ReplacementFaceAreaFailureCount++;
                    blocker = "one or more hypothetical replacement faces have insufficient area";
                    return false;
                }
                Vector3 normal = CalculatePolygonNormal(solvedFace);
                if (!IsFinite(normal) || Vector3.Dot(normal, sourceFace.Normal) <= 0.25f)
                {
                    stats.ReplacementFaceWindingFailureCount++;
                    blocker = "one or more hypothetical replacement faces invert or lose stable winding";
                    return false;
                }
                for (int i = 0; i < solvedFace.Count; i++)
                {
                    Vector3 start = solvedFace[i];
                    Vector3 end = solvedFace[(i + 1) % solvedFace.Count];
                    if (new VertexKey(start).Equals(new VertexKey(end)))
                    {
                        stats.ReplacementEdgeCollapseFailureCount++;
                        blocker = "a replacement face contains a collapsed emitted edge";
                        return false;
                    }
                }
            }
            return true;
        }

        private static bool AuditChamferSelectedRails(
            ChamferTopologyContext context,
            Dictionary<ChamferFaceCornerKey, ChamferSolvedCorner> corners,
            Dictionary<int, float> widthByEdge,
            float minimumStableEdgeLength,
            ref ChamferCornerStats stats,
            out string blocker)
        {
            blocker = string.Empty;
            foreach (int edgeIndex in context.SelectedSourceEdges)
            {
                if (!widthByEdge.TryGetValue(edgeIndex, out float activeWidth) ||
                    activeWidth <= PointMergeDistance)
                {
                    continue;
                }

                EdgeWearGraphEdge edge = context.Graph.Edges[edgeIndex];
                Vector3 a0 = corners[
                    new ChamferFaceCornerKey(edge.FaceA, edge.VertexA)].Position;
                Vector3 b0 = corners[
                    new ChamferFaceCornerKey(edge.FaceA, edge.VertexB)].Position;
                Vector3 a1 = corners[
                    new ChamferFaceCornerKey(edge.FaceB, edge.VertexA)].Position;
                Vector3 b1 = corners[
                    new ChamferFaceCornerKey(edge.FaceB, edge.VertexB)].Position;

                if (Vector3.Distance(a0, a1) < minimumStableEdgeLength ||
                    Vector3.Distance(b0, b1) < minimumStableEdgeLength)
                {
                    blocker = "one or more selected edge strips have insufficient endpoint span";
                    return false;
                }
                if (Vector3.Distance(a0, b0) < minimumStableEdgeLength ||
                    Vector3.Distance(a1, b1) < minimumStableEdgeLength)
                {
                    blocker = "one or more selected edge strips have insufficient rail length";
                    return false;
                }
            }
            return true;
        }

        private static bool AuditChamferSolvedBoundary(
            ChamferTopologyContext context,
            Dictionary<ChamferFaceCornerKey, ChamferSolvedCorner> corners,
            float minimumStableEdgeLength,
            ref ChamferCornerStats stats,
            out string blocker)
        {
            blocker = string.Empty;
            for (int edgeIndex = 0;
                 edgeIndex < context.Graph.Edges.Count;
                 edgeIndex++)
            {
                EdgeWearGraphEdge edge = context.Graph.Edges[edgeIndex];
                if (edge.FaceA >= 0 && edge.FaceB >= 0)
                {
                    continue;
                }
                int faceIndex = edge.FaceA >= 0 ? edge.FaceA : edge.FaceB;
                Vector3 a = corners[
                    new ChamferFaceCornerKey(faceIndex, edge.VertexA)].Position;
                Vector3 b = corners[
                    new ChamferFaceCornerKey(faceIndex, edge.VertexB)].Position;
                float sourceLength = GetGraphEdgeLength(
                    context.Graph,
                    edgeIndex);
                if (Vector3.Distance(a, b) < minimumStableEdgeLength &&
                    sourceLength >= minimumStableEdgeLength)
                {
                    blocker = "one or more preserved source-boundary edges collapse after corner solving";
                    return false;
                }
            }
            return true;
        }

        private static void AppendChamferReplacementEdgeChain(
            int faceIndex,
            int orientedStartVertex,
            int orientedEndVertex,
            int sourceEdgeIndex,
            Dictionary<ChamferFaceCornerKey, ChamferSolvedCorner> corners,
            Dictionary<int, ChamferSharedEdgeSpan> sharedSpans,
            List<Vector3> output,
            List<ChamferExpectedVertexBoundary> vertexBoundaries)
        {
            Vector3 start = corners[
                new ChamferFaceCornerKey(
                    faceIndex,
                    orientedStartVertex)].Position;
            Vector3 end = corners[
                new ChamferFaceCornerKey(
                    faceIndex,
                    orientedEndVertex)].Position;
            AppendUniquePoint(output, start);

            if (sharedSpans.TryGetValue(
                    sourceEdgeIndex,
                    out ChamferSharedEdgeSpan span))
            {
                bool forward = orientedStartVertex == span.VertexA &&
                    orientedEndVertex == span.VertexB;
                Vector3 sharedStart = forward
                    ? span.SharedAtVertexA
                    : span.SharedAtVertexB;
                Vector3 sharedEnd = forward
                    ? span.SharedAtVertexB
                    : span.SharedAtVertexA;

                if (!new VertexKey(start).Equals(new VertexKey(sharedStart)))
                {
                    if (vertexBoundaries != null)
                    {
                        AddExpectedVertexBoundary(
                            vertexBoundaries,
                            orientedStartVertex,
                            sourceEdgeIndex,
                            faceIndex,
                            ChamferVertexBoundaryKind.UnselectedEdgeTail,
                            start,
                            sharedStart);
                    }
                    AppendUniquePoint(output, sharedStart);
                }
                else
                {
                    AppendUniquePoint(output, sharedStart);
                }

                AppendUniquePoint(output, sharedEnd);
                if (!new VertexKey(sharedEnd).Equals(new VertexKey(end)))
                {
                    if (vertexBoundaries != null)
                    {
                        AddExpectedVertexBoundary(
                            vertexBoundaries,
                            orientedEndVertex,
                            sourceEdgeIndex,
                            faceIndex,
                            ChamferVertexBoundaryKind.UnselectedEdgeTail,
                            sharedEnd,
                            end);
                    }
                }
            }

            AppendUniquePoint(output, end);
        }

        private static void AppendUniquePoint(
            List<Vector3> points,
            Vector3 point)
        {
            if (points.Count == 0 ||
                !new VertexKey(points[points.Count - 1]).Equals(
                    new VertexKey(point)))
            {
                points.Add(point);
            }
        }

        private static int ReduceChamferFaceRetraces(
            List<Vector3> points,
            HashSet<TopologyEdgeKey> removedEdgeKeys)
        {
            if (points == null || points.Count < 2)
            {
                return 0;
            }

            Dictionary<TopologyEdgeKey, int> originalEdgeCounts =
                removedEdgeKeys != null
                    ? BuildChamferFaceEdgeUseCounts(points)
                    : null;
            int retracePairsRemoved = 0;
            // Only exact topology-key backtracks and duplicate vertices are
            // removed. Each successful pass shrinks the walk, so termination
            // is bounded by the original vertex count.
            bool changed = true;
            while (changed && points.Count > 1)
            {
                changed = false;
                if (points.Count >= 3)
                {
                    for (int i = 0; i < points.Count; i++)
                    {
                        int previousIndex = (i - 1 + points.Count) % points.Count;
                        int nextIndex = (i + 1) % points.Count;
                        VertexKey previous = new VertexKey(points[previousIndex]);
                        VertexKey next = new VertexKey(points[nextIndex]);
                        if (!previous.Equals(next))
                        {
                            continue;
                        }

                        RemoveChamferFaceVertexPair(points, i, nextIndex);
                        retracePairsRemoved++;
                        changed = true;
                        break;
                    }
                }
                if (changed)
                {
                    continue;
                }

                for (int i = 0; i < points.Count; i++)
                {
                    int nextIndex = (i + 1) % points.Count;
                    if (!new VertexKey(points[i]).Equals(
                            new VertexKey(points[nextIndex])))
                    {
                        continue;
                    }

                    points.RemoveAt(nextIndex);
                    changed = true;
                    break;
                }
            }

            if (removedEdgeKeys != null && originalEdgeCounts != null)
            {
                Dictionary<TopologyEdgeKey, int> reducedEdgeCounts =
                    BuildChamferFaceEdgeUseCounts(points);
                foreach (KeyValuePair<TopologyEdgeKey, int> pair
                         in originalEdgeCounts)
                {
                    reducedEdgeCounts.TryGetValue(pair.Key, out int reducedCount);
                    if (reducedCount < pair.Value)
                    {
                        removedEdgeKeys.Add(pair.Key);
                    }
                }
            }
            return retracePairsRemoved;
        }

        private static void RemoveChamferFaceVertexPair(
            List<Vector3> points,
            int firstIndex,
            int secondIndex)
        {
            int high = Mathf.Max(firstIndex, secondIndex);
            int low = Mathf.Min(firstIndex, secondIndex);
            points.RemoveAt(high);
            points.RemoveAt(low);
        }

        private static Dictionary<TopologyEdgeKey, int>
            BuildChamferFaceEdgeUseCounts(List<Vector3> points)
        {
            Dictionary<TopologyEdgeKey, int> counts =
                new Dictionary<TopologyEdgeKey, int>();
            if (points == null || points.Count < 2)
            {
                return counts;
            }

            for (int i = 0; i < points.Count; i++)
            {
                VertexKey start = new VertexKey(points[i]);
                VertexKey end = new VertexKey(points[(i + 1) % points.Count]);
                if (start.Equals(end))
                {
                    continue;
                }
                TopologyEdgeKey key = new TopologyEdgeKey(start, end);
                counts.TryGetValue(key, out int count);
                counts[key] = count + 1;
            }
            return counts;
        }

        private static bool TryFindDuplicateChamferFaceEdge(
            List<Vector3> points,
            out TopologyEdgeKey duplicateKey,
            out int firstLocalEdgeIndex,
            out int secondLocalEdgeIndex)
        {
            duplicateKey = default;
            firstLocalEdgeIndex = -1;
            secondLocalEdgeIndex = -1;
            if (points == null || points.Count < 3)
            {
                return false;
            }

            Dictionary<TopologyEdgeKey, int> firstUseByKey =
                new Dictionary<TopologyEdgeKey, int>();
            for (int i = 0; i < points.Count; i++)
            {
                VertexKey start = new VertexKey(points[i]);
                VertexKey end = new VertexKey(points[(i + 1) % points.Count]);
                if (start.Equals(end))
                {
                    duplicateKey = new TopologyEdgeKey(start, end);
                    firstLocalEdgeIndex = i;
                    secondLocalEdgeIndex = i;
                    return false;
                }

                TopologyEdgeKey key = new TopologyEdgeKey(start, end);
                if (firstUseByKey.TryGetValue(key, out int firstUse))
                {
                    duplicateKey = key;
                    firstLocalEdgeIndex = firstUse;
                    secondLocalEdgeIndex = i;
                    return false;
                }
                firstUseByKey.Add(key, i);
            }
            return true;
        }

        private static HashSet<TopologyEdgeKey>
            BuildChamferFaceEdgeKeySet(List<Vector3> points)
        {
            return new HashSet<TopologyEdgeKey>(
                BuildChamferFaceEdgeUseCounts(points).Keys);
        }

        private static void AddExpectedVertexBoundary(
            List<ChamferExpectedVertexBoundary> boundaries,
            int sourceVertexIndex,
            int sourceEdgeIndex,
            int faceIndex,
            ChamferVertexBoundaryKind kind,
            Vector3 start,
            Vector3 end)
        {
            VertexKey startKey = new VertexKey(start);
            VertexKey endKey = new VertexKey(end);
            if (startKey.Equals(endKey))
            {
                return;
            }
            boundaries.Add(new ChamferExpectedVertexBoundary(
                sourceVertexIndex,
                sourceEdgeIndex,
                faceIndex,
                kind,
                start,
                end,
                new TopologyEdgeKey(startKey, endKey)));
        }

        private static Dictionary<TopologyEdgeKey, int>
            BuildTopologyEdgeUseCounts(List<PolygonFace> faces)
        {
            Dictionary<TopologyEdgeKey, int> counts =
                new Dictionary<TopologyEdgeKey, int>();
            for (int faceIndex = 0; faceIndex < faces.Count; faceIndex++)
            {
                List<Vector3> vertices = faces[faceIndex].Vertices;
                if (vertices == null || vertices.Count < 3)
                {
                    continue;
                }
                for (int i = 0; i < vertices.Count; i++)
                {
                    VertexKey a = new VertexKey(vertices[i]);
                    VertexKey b = new VertexKey(vertices[(i + 1) % vertices.Count]);
                    if (a.Equals(b))
                    {
                        continue;
                    }
                    TopologyEdgeKey key = new TopologyEdgeKey(a, b);
                    counts.TryGetValue(key, out int count);
                    counts[key] = count + 1;
                }
            }
            return counts;
        }

        private static void AuditExpectedVertexBoundaryComponents(
            List<ChamferExpectedVertexBoundary> boundaries,
            ref ChamferEmissionStats stats)
        {
            Dictionary<int, List<ChamferExpectedVertexBoundary>> byVertex =
                new Dictionary<int, List<ChamferExpectedVertexBoundary>>();
            for (int i = 0; i < boundaries.Count; i++)
            {
                ChamferExpectedVertexBoundary boundary = boundaries[i];
                if (!byVertex.TryGetValue(
                        boundary.SourceVertexIndex,
                        out List<ChamferExpectedVertexBoundary> list))
                {
                    list = new List<ChamferExpectedVertexBoundary>();
                    byVertex.Add(boundary.SourceVertexIndex, list);
                }
                list.Add(boundary);
            }

            foreach (KeyValuePair<int, List<ChamferExpectedVertexBoundary>> pair
                     in byVertex)
            {
                List<ChamferExpectedVertexBoundary> edges = pair.Value;
                Dictionary<VertexKey, List<int>> adjacency =
                    new Dictionary<VertexKey, List<int>>();
                HashSet<TopologyEdgeKey> unique =
                    new HashSet<TopologyEdgeKey>();
                for (int i = 0; i < edges.Count; i++)
                {
                    if (!unique.Add(edges[i].Key))
                    {
                        stats.VertexBoundaryDuplicateFailureCount++;
                        continue;
                    }
                    AddBoundaryAdjacency(adjacency, edges[i].Key.First, i);
                    AddBoundaryAdjacency(adjacency, edges[i].Key.Second, i);
                }

                foreach (KeyValuePair<VertexKey, List<int>> degree in adjacency)
                {
                    if (degree.Value.Count > 2)
                    {
                        stats.VertexBoundaryBranchFailureCount++;
                    }
                }

                HashSet<int> visited = new HashSet<int>();
                for (int edgeIndex = 0; edgeIndex < edges.Count; edgeIndex++)
                {
                    if (!visited.Add(edgeIndex))
                    {
                        continue;
                    }
                    Queue<int> queue = new Queue<int>();
                    queue.Enqueue(edgeIndex);
                    HashSet<VertexKey> componentVertices =
                        new HashSet<VertexKey>();
                    while (queue.Count > 0)
                    {
                        int current = queue.Dequeue();
                        TopologyEdgeKey key = edges[current].Key;
                        componentVertices.Add(key.First);
                        componentVertices.Add(key.Second);
                        EnqueueAdjacentBoundaryEdges(
                            key.First,
                            adjacency,
                            visited,
                            queue);
                        EnqueueAdjacentBoundaryEdges(
                            key.Second,
                            adjacency,
                            visited,
                            queue);
                    }

                    int degreeOne = 0;
                    bool invalid = false;
                    foreach (VertexKey vertex in componentVertices)
                    {
                        int degree = adjacency[vertex].Count;
                        if (degree == 1)
                        {
                            degreeOne++;
                        }
                        else if (degree != 2)
                        {
                            invalid = true;
                        }
                    }
                    if (!invalid && degreeOne == 0)
                    {
                    }
                    else if (!invalid && degreeOne == 2)
                    {
                    }
                    else
                    {
                        stats.VertexBoundaryBranchFailureCount++;
                    }
                }
            }
        }

        private static CornerDamageTransactionAuditResult
            EvaluateCornerDamageTransaction(
                List<PolygonFace> normalizedFaces,
                EdgeWearMicroTopologyNormalizationResult normalization,
                Bounds normalizedBounds,
                float maximumDimension,
                MassRecipe recipe,
                MassSurfaceFeatureSettings settings)
        {
            CornerDamageTransactionAuditResult result =
                new CornerDamageTransactionAuditResult
                {
                    Attempted = true,
                    ShapeSeed = recipe == null ? 0 : recipe.ShapeSeed,
                    MaximumDimension = maximumDimension,
                    MinimumStableEdgeLength = maximumDimension * 0.0012f,
                    MinimumStableFaceArea =
                        maximumDimension * maximumDimension * 0.000001f,
                    RequestedDepthFraction = settings.CornerChipDepth,
                    DepthVariation = settings.CornerChipDepthVariation,
                    TopFacingPreference =
                        settings.CornerChipTopFacingPreference
                };
            result.SourceVolume =
                CalculatePlaneCutPolyhedronVolume(normalizedFaces);

            if (normalizedFaces == null || normalizedFaces.Count < 4 ||
                recipe == null ||
                !TryBuildEdgeWearTopologyGraph(
                    normalizedFaces,
                    out EdgeWearTopologyGraph graph,
                    out EdgeWearGraphBuildStats graphStats) ||
                graphStats.GraphBoundaryEdgeCount != 0 ||
                graphStats.GraphNonManifoldEdgeCount != 0)
            {
                result.Diagnostic =
                    "normalized corner-damage topology graph is unavailable";
                return result;
            }

            result.GraphAvailable = true;
            result.NormalizedVertexCount = graph.Vertices.Count;
            result.NormalizedEdgeCount = graph.Edges.Count;
            result.NormalizedFaceCount = graph.Faces.Count;
            Vector3 solidCentre =
                CalculatePlaneCutFaceVertexCentre(normalizedFaces);
            float massBoundsDiagonal = Mathf.Max(
                PointMergeDistance,
                normalizedBounds.size.magnitude);
            float structuralTolerance = Mathf.Max(
                PointMergeDistance * 8f,
                maximumDimension * 0.00001f);

            List<CornerDamageCandidateRecord> eligibleCandidates =
                new List<CornerDamageCandidateRecord>();
            for (int vertexIndex = 0;
                 vertexIndex < graph.Vertices.Count;
                 vertexIndex++)
            {
                EdgeWearGraphVertex vertex = graph.Vertices[vertexIndex];
                CornerDamageCandidateRecord candidate =
                    BuildCornerDamageCandidate(
                        normalizedFaces,
                        graph,
                        normalization,
                        solidCentre,
                        massBoundsDiagonal,
                        result.MinimumStableEdgeLength,
                        structuralTolerance,
                        recipe.ShapeSeed,
                        settings.CornerChipTopFacingPreference,
                        vertexIndex,
                        vertex);
                result.Candidates.Add(candidate);
                if (candidate.Eligible)
                {
                    eligibleCandidates.Add(candidate);
                }
            }

            eligibleCandidates = RankCornerDamageCandidates(
                eligibleCandidates);
            result.EligibleCandidateCount = eligibleCandidates.Count;
            if (eligibleCandidates.Count == 0)
            {
                result.Diagnostic =
                    "no normalized source corner satisfies the C1A.1 eligibility contract";
                return result;
            }

            int selectedCandidateRank =
                ResolveCornerDamageCandidateRankOverride();
            if (selectedCandidateRank < 0 ||
                selectedCandidateRank >= eligibleCandidates.Count)
            {
                result.Diagnostic =
                    "requested corner candidate rank is outside the eligible set";
                return result;
            }

            CornerDamageCandidateRecord selected =
                eligibleCandidates[selectedCandidateRank];
            result.CandidateFound = true;
            result.SelectedCandidateRank = selectedCandidateRank;
            result.SelectedGraphVertexIndex =
                selected.GraphVertexIndex;
            result.SelectedPosition = selected.Position;
            Vector3 outwardNormal = Vector3.zero;
            for (int faceListIndex = 0;
                 faceListIndex < selected.IncidentFaceIndices.Count;
                 faceListIndex++)
            {
                int faceIndex = selected.IncidentFaceIndices[faceListIndex];
                if (faceIndex < 0 || faceIndex >= normalizedFaces.Count)
                {
                    continue;
                }
                PolygonFace face = normalizedFaces[faceIndex];
                float area = CalculatePolygonArea(face.Vertices);
                outwardNormal += face.Normal * area;
            }
            if (!IsFinite(outwardNormal) ||
                outwardNormal.sqrMagnitude <= MinimumEdgeLengthSqr)
            {
                result.Diagnostic =
                    "selected corner has no finite area-weighted outward normal";
                return result;
            }
            outwardNormal.Normalize();
            result.OutwardNormal = outwardNormal;

            float depthIdentity = Hash01(
                unchecked(recipe.ShapeSeed + CornerDamageDepthSalt),
                selected.GraphVertexIndex);
            float requestedDepthFraction = Mathf.Clamp(
                settings.CornerChipDepth,
                0.04f,
                0.35f);
            float depthVariation = Mathf.Clamp(
                settings.CornerChipDepthVariation,
                0f,
                0.50f);
            float signedDepthVariation = depthIdentity * 2f - 1f;
            float resolvedDepthFraction = Mathf.Clamp(
                requestedDepthFraction *
                    (1f + signedDepthVariation * depthVariation),
                0.04f,
                0.35f);
            result.RequestedDepthFraction = requestedDepthFraction;
            result.DepthVariation = depthVariation;
            result.DepthVariationIdentity = depthIdentity;
            result.ResolvedDepthFraction = resolvedDepthFraction;
            result.ShortestIncidentEdgeLength =
                selected.MinimumIncidentEdgeLength;
            result.BaseDepth =
                selected.MinimumIncidentEdgeLength *
                resolvedDepthFraction;

            for (int trialIndex = 0;
                 trialIndex < CornerDamageDepthTrialFactors.Length;
                 trialIndex++)
            {
                float factor = CornerDamageDepthTrialFactors[trialIndex];
                float depth = Mathf.Clamp(
                    result.BaseDepth * factor,
                    2f * result.MinimumStableEdgeLength,
                    selected.MinimumIncidentEdgeLength * 0.35f);
                Vector3 planePoint =
                    selected.Position - outwardNormal * depth;
                CutPlane plane = new CutPlane(
                    outwardNormal,
                    Vector3.Dot(outwardNormal, planePoint));
                CornerDamageTrialRecord trial =
                    EvaluateCornerDamageTrial(
                        normalizedFaces,
                        graph,
                        normalization,
                        selected,
                        normalizedBounds,
                        result.MinimumStableEdgeLength,
                        result.MinimumStableFaceArea,
                        structuralTolerance,
                        plane,
                        planePoint,
                        trialIndex,
                        factor,
                        depth,
                        out List<PolygonFace> acceptedFaces,
                        out PolygonFace acceptedCapFace);
                result.Trials.Add(trial);
                if (!trial.Succeeded)
                {
                    continue;
                }
                if (!TryCommitCornerDamageTransactionResult(
                        result,
                        trial,
                        acceptedFaces,
                        acceptedCapFace,
                        out string commitBlocker))
                {
                    trial.Succeeded = false;
                    trial.Blocker = commitBlocker;
                    continue;
                }

                result.Succeeded = true;
                result.AcceptedTrialIndex = trialIndex;
                result.Diagnostic =
                    "corner-damage transaction certified on trial " +
                    trialIndex;
                break;
            }

            if (!result.Succeeded)
            {
                result.Diagnostic =
                    "all four bounded corner-damage depth trials were rejected";
            }
            return result;
        }

        private static List<CornerDamageCandidateRecord>
            RankCornerDamageCandidates(
                List<CornerDamageCandidateRecord> candidates)
        {
            List<CornerDamageCandidateRecord> remaining =
                candidates == null
                    ? new List<CornerDamageCandidateRecord>()
                    : new List<CornerDamageCandidateRecord>(candidates);
            List<CornerDamageCandidateRecord> ranked =
                new List<CornerDamageCandidateRecord>(remaining.Count);
            while (remaining.Count > 0)
            {
                int bestIndex = 0;
                for (int candidateIndex = 1;
                     candidateIndex < remaining.Count;
                     candidateIndex++)
                {
                    if (IsCornerDamageCandidatePreferred(
                            remaining[candidateIndex],
                            remaining[bestIndex]))
                    {
                        bestIndex = candidateIndex;
                    }
                }

                ranked.Add(remaining[bestIndex]);
                remaining.RemoveAt(bestIndex);
            }
            return ranked;
        }

        private static bool IsCornerDamageCandidatePreferred(
            CornerDamageCandidateRecord candidate,
            CornerDamageCandidateRecord selected)
        {
            return selected == null ||
                candidate.Score > selected.Score + 0.0000001f ||
                (Mathf.Abs(candidate.Score - selected.Score) <=
                     0.0000001f &&
                 candidate.GraphVertexIndex <
                     selected.GraphVertexIndex);
        }

        private static CornerDamageCandidateRecord
            BuildCornerDamageCandidate(
                List<PolygonFace> normalizedFaces,
                EdgeWearTopologyGraph graph,
                EdgeWearMicroTopologyNormalizationResult normalization,
                Vector3 solidCentre,
                float massBoundsDiagonal,
                float minimumStableEdgeLength,
                float tolerance,
                int shapeSeed,
                float topFacingPreference,
                int vertexIndex,
                EdgeWearGraphVertex vertex)
        {
            CornerDamageCandidateRecord candidate =
                new CornerDamageCandidateRecord
                {
                    GraphVertexIndex = vertexIndex,
                    Position = vertex.Position,
                    IncidentFaceCount = vertex.FaceIndices.Count,
                    IncidentEdgeCount = vertex.EdgeIndices.Count,
                    MinimumIncidentEdgeLength = float.PositiveInfinity
                };
            candidate.IncidentFaceIndices.AddRange(vertex.FaceIndices);
            candidate.IncidentGraphEdgeIndices.AddRange(vertex.EdgeIndices);
            candidate.IncidentFaceIndices.Sort();
            candidate.IncidentGraphEdgeIndices.Sort();

            if (candidate.IncidentFaceCount != 3)
            {
                candidate.Blocker = "incident-face-count-is-not-three";
                return candidate;
            }
            if (candidate.IncidentEdgeCount != 3)
            {
                candidate.Blocker = "incident-edge-count-is-not-three";
                return candidate;
            }

            for (int incidentIndex = 0;
                 incidentIndex < candidate.IncidentGraphEdgeIndices.Count;
                 incidentIndex++)
            {
                int graphEdgeIndex =
                    candidate.IncidentGraphEdgeIndices[incidentIndex];
                if (graphEdgeIndex < 0 ||
                    graphEdgeIndex >= graph.Edges.Count)
                {
                    candidate.Blocker = "incident-edge-index-is-invalid";
                    return candidate;
                }
                EdgeWearGraphEdge edge = graph.Edges[graphEdgeIndex];
                if (edge.FaceA < 0 || edge.FaceB < 0 ||
                    edge.ExtraFaceCount != 0)
                {
                    candidate.Blocker = "incident-edge-is-not-manifold";
                    return candidate;
                }
                Vector3 edgeA = graph.Vertices[edge.VertexA].Position;
                Vector3 edgeB = graph.Vertices[edge.VertexB].Position;
                EdgeKey edgeKey = new EdgeKey(edgeA, edgeB);
                if (normalization != null &&
                    normalization.GeneratedTransitionKeys.Contains(edgeKey))
                {
                    candidate.Blocker =
                        "corner-touches-a-micro-topology-generated-transition";
                    return candidate;
                }

                float edgeLength = (edgeB - edgeA).magnitude;
                candidate.MinimumIncidentEdgeLength = Mathf.Min(
                    candidate.MinimumIncidentEdgeLength,
                    edgeLength);
                int originalEdgeIndex = normalization == null
                    ? graphEdgeIndex
                    : normalization.ResolveOriginalSourceEdgeIndex(
                        edgeKey,
                        graphEdgeIndex);
                candidate.IncidentOriginalEdgeIndices.Add(
                    originalEdgeIndex);

                if (!TryClassifyEdgeWearStructuralEdge(
                        normalizedFaces,
                        edge.FaceA,
                        edge.FaceB,
                        edgeA,
                        edgeB,
                        solidCentre,
                        tolerance,
                        out BoundedEdgeClassificationEvidence evidence))
                {
                    candidate.Blocker =
                        "incident-edge-classification-failed";
                    return candidate;
                }
                candidate.MaximumIncidentDihedral = Mathf.Max(
                    candidate.MaximumIncidentDihedral,
                    evidence.DihedralDegrees);
                if (evidence.Classification ==
                    BoundedEdgeClassification.Convex)
                {
                    candidate.ConvexIncidentEdgeCount++;
                }
            }

            candidate.IncidentOriginalEdgeIndices.Sort();
            if (candidate.ConvexIncidentEdgeCount < 2)
            {
                candidate.Blocker =
                    "fewer-than-two-convex-incident-edges";
                return candidate;
            }
            if (candidate.MaximumIncidentDihedral < 55f)
            {
                candidate.Blocker =
                    "maximum-incident-dihedral-below-55-degrees";
                return candidate;
            }
            if (float.IsNaN(candidate.MinimumIncidentEdgeLength) ||
                float.IsInfinity(candidate.MinimumIncidentEdgeLength) ||
                candidate.MinimumIncidentEdgeLength <
                    minimumStableEdgeLength * 8f)
            {
                candidate.Blocker =
                    "minimum-incident-edge-length-below-eight-stable-lengths";
                return candidate;
            }

            candidate.SharpnessScore = Mathf.Clamp01(
                (candidate.MaximumIncidentDihedral - 35f) / 65f);
            candidate.SizeScore = Mathf.Clamp01(
                candidate.MinimumIncidentEdgeLength /
                massBoundsDiagonal);
            Vector3 radial = candidate.Position - solidCentre;
            candidate.UpwardExposureScore = radial.sqrMagnitude >
                MinimumEdgeLengthSqr
                ? Mathf.Clamp01(
                    Vector3.Dot(radial.normalized, Vector3.up) *
                        0.5f + 0.5f)
                : 0.5f;
            candidate.RandomScore = Hash01(
                unchecked(shapeSeed + CornerDamageSelectionSalt),
                vertexIndex);
            float upwardWeight =
                0.30f * Mathf.Clamp01(topFacingPreference);
            float nonUpwardWeight = 1f - upwardWeight;
            candidate.Score =
                nonUpwardWeight *
                    (candidate.SharpnessScore * 0.6470588235f +
                     candidate.SizeScore * 0.2941176471f +
                     candidate.RandomScore * 0.0588235294f) +
                candidate.UpwardExposureScore * upwardWeight;
            candidate.Eligible = true;
            return candidate;
        }

        private static CornerDamageTrialRecord EvaluateCornerDamageTrial(
            List<PolygonFace> normalizedFaces,
            EdgeWearTopologyGraph sourceGraph,
            EdgeWearMicroTopologyNormalizationResult normalization,
            CornerDamageCandidateRecord selected,
            Bounds sourceBounds,
            float minimumStableEdgeLength,
            float minimumStableFaceArea,
            float tolerance,
            CutPlane plane,
            Vector3 planePoint,
            int trialIndex,
            float depthFactor,
            float depth,
            out List<PolygonFace> acceptedFaces,
            out PolygonFace acceptedCapFace)
        {
            acceptedFaces = null;
            acceptedCapFace = null;
            CornerDamageTrialRecord trial =
                new CornerDamageTrialRecord
                {
                    TrialIndex = trialIndex,
                    DepthFactor = depthFactor,
                    Depth = depth,
                    PlanePoint = planePoint,
                    PlaneDistance = plane.Distance,
                    SourceVolume =
                        CalculatePlaneCutPolyhedronVolume(normalizedFaces)
                };

            if (!TryClipCornerDamageTransaction(
                    normalizedFaces,
                    plane,
                    selected.GraphVertexIndex,
                    out List<PolygonFace> clippedFaces,
                    out PolygonFace capFace,
                    out PlaneCutNumericalRepairTelemetry numericalRepairs,
                    out string clipBlocker))
            {
                trial.ExactConstructionFailureCount =
                    numericalRepairs == null
                        ? 0
                        : numericalRepairs.ExactConstructionFailureCount;
                trial.ExactConstructionFailure =
                    numericalRepairs == null
                        ? string.Empty
                        : numericalRepairs.FirstExactFailureReason ??
                            string.Empty;
                trial.Blocker = clipBlocker;
                return trial;
            }

            trial.ExactConstructionFailureCount =
                numericalRepairs.ExactConstructionFailureCount;
            trial.ExactConstructionFailure =
                numericalRepairs.FirstExactFailureReason ?? string.Empty;
            if (!TryPrepareBoundedFaces(
                    clippedFaces,
                    minimumStableEdgeLength,
                    minimumStableFaceArea,
                    out List<PolygonFace> preparedFaces,
                    out _,
                    out string preparationBlocker))
            {
                trial.Blocker =
                    "corner-damage face preparation failed: " +
                    preparationBlocker;
                return trial;
            }

            trial.FaceCount = preparedFaces.Count;
            capFace = null;
            for (int faceIndex = 0;
                 faceIndex < preparedFaces.Count;
                 faceIndex++)
            {
                PolygonFace face = preparedFaces[faceIndex];
                if (face.ProvenanceKind ==
                    PolygonFaceProvenanceKind.CornerDamageCap)
                {
                    trial.CapFaceCount++;
                    capFace = face;
                }
                AuditCornerDamageFaceQuality(
                    face,
                    minimumStableFaceArea,
                    tolerance,
                    ref trial);
            }
            if (trial.CapFaceCount != 1 || capFace == null)
            {
                trial.Blocker =
                    "prepared corner transaction does not contain exactly one cap";
                return trial;
            }

            trial.CapVertexCount = capFace.Vertices.Count;
            trial.CapArea = CalculatePolygonArea(capFace.Vertices);
            for (int capVertexIndex = 0;
                 capVertexIndex < capFace.Vertices.Count;
                 capVertexIndex++)
            {
                trial.MaximumCapPlaneResidual = Mathf.Max(
                    trial.MaximumCapPlaneResidual,
                    Mathf.Abs(plane.SignedDistance(
                        capFace.Vertices[capVertexIndex])));
            }

            EdgeWearTopologyStats topology = AuditEdgeWearTopology(
                preparedFaces,
                minimumStableEdgeLength);
            trial.OpenEdgeCount = topology.OpenEdgeCount;
            trial.NonManifoldEdgeCount = topology.NonManifoldEdgeCount;
            trial.TJunctionCount = topology.TJunctionCount;
            Bounds resultBounds = CalculateFaceBounds(preparedFaces);
            trial.BoundsValid = ArePlaneCutBoundsContained(
                sourceBounds,
                resultBounds,
                tolerance)
                ? 1
                : 0;
            trial.ResultVolume =
                CalculatePlaneCutPolyhedronVolume(preparedFaces);
            trial.VolumeLoss = trial.SourceVolume - trial.ResultVolume;
            trial.VolumeLossFraction = trial.SourceVolume > 0.000000001
                ? trial.VolumeLoss / trial.SourceVolume
                : 0.0;

            if (!TryBuildEdgeWearTopologyGraph(
                    preparedFaces,
                    out EdgeWearTopologyGraph outputGraph,
                    out EdgeWearGraphBuildStats outputStats) ||
                outputStats.GraphBoundaryEdgeCount != 0 ||
                outputStats.GraphNonManifoldEdgeCount != 0)
            {
                trial.Blocker =
                    "corner-damage output graph is not closed manifold";
                return trial;
            }

            trial.OutputVertexCount = outputGraph.Vertices.Count;
            trial.OutputTriangleCount =
                CalculateCornerDamagePolygonTriangleCount(preparedFaces);
            int sourceTriangleCount =
                CalculateCornerDamagePolygonTriangleCount(normalizedFaces);
            trial.BudgetValid =
                trial.OutputVertexCount <=
                    sourceGraph.Vertices.Count + 2 &&
                trial.OutputTriangleCount <= sourceTriangleCount + 4
                    ? 1
                    : 0;

            AuditCornerDamageIdentityMapping(
                sourceGraph,
                outputGraph,
                normalization,
                selected,
                capFace,
                tolerance,
                trial);

            if (trial.CapVertexCount < 3 ||
                trial.CapArea <= minimumStableFaceArea ||
                trial.MaximumCapPlaneResidual > tolerance ||
                trial.OpenEdgeCount != 0 ||
                trial.NonManifoldEdgeCount != 0 ||
                trial.TJunctionCount != 0 ||
                trial.InvalidFaceCount != 0 ||
                trial.NonPlanarFaceCount != 0 ||
                trial.NonConvexFaceCount != 0 ||
                trial.WindingFailureCount != 0 ||
                trial.BoundsValid != 1 ||
                trial.BudgetValid != 1 ||
                trial.SourceVolume <= 0.000000001 ||
                trial.ResultVolume <= 0.000000001 ||
                trial.VolumeLoss <= 0.000000001 ||
                trial.VolumeLossFraction > 0.12 ||
                trial.MissingOriginalEdgeCount != 0 ||
                trial.AmbiguousIdentityCount != 0 ||
                trial.GeneratedIdentityCollisionCount != 0 ||
                trial.ShortenedDescendantEdgeCount !=
                    selected.IncidentGraphEdgeIndices.Count ||
                trial.CapRingEdgeCount != capFace.Vertices.Count)
            {
                trial.Blocker = BuildCornerDamageTrialBlocker(
                    trial,
                    tolerance,
                    selected.IncidentGraphEdgeIndices.Count,
                    capFace.Vertices.Count);
                return trial;
            }

            trial.Succeeded = true;
            acceptedFaces = preparedFaces;
            acceptedCapFace = capFace;
            return trial;
        }

        private static void AuditCornerDamageFaceQuality(
            PolygonFace face,
            float minimumStableFaceArea,
            float tolerance,
            ref CornerDamageTrialRecord trial)
        {
            if (face == null || face.Vertices == null ||
                face.Vertices.Count < 3 || !IsFinite(face.Normal))
            {
                trial.InvalidFaceCount++;
                return;
            }
            float area = CalculatePolygonArea(face.Vertices);
            if (float.IsNaN(area) || float.IsInfinity(area) ||
                area <= minimumStableFaceArea)
            {
                trial.InvalidFaceCount++;
            }
            Vector3 measured = CalculatePolygonNormal(face.Vertices);
            if (!IsFinite(measured) ||
                measured.sqrMagnitude <= MinimumEdgeLengthSqr)
            {
                trial.InvalidFaceCount++;
                return;
            }
            measured.Normalize();
            if (Vector3.Dot(measured, face.Normal) <= 0f)
            {
                trial.WindingFailureCount++;
            }
            float planeDistance = CalculateAuthoredFacePlaneDistance(face);
            float maximumResidual = 0f;
            for (int vertexIndex = 0;
                 vertexIndex < face.Vertices.Count;
                 vertexIndex++)
            {
                Vector3 vertex = face.Vertices[vertexIndex];
                if (!IsFinite(vertex))
                {
                    trial.InvalidFaceCount++;
                    continue;
                }
                maximumResidual = Mathf.Max(
                    maximumResidual,
                    Mathf.Abs(
                        Vector3.Dot(face.Normal, vertex) -
                        planeDistance));
            }
            if (maximumResidual > tolerance)
            {
                trial.NonPlanarFaceCount++;
            }
            if (!IsBoundedPolygonConvex(
                    BuildBoundedConvexityCheckLoop(face.Vertices),
                    face.Normal))
            {
                trial.NonConvexFaceCount++;
            }
        }

        private static void AuditCornerDamageIdentityMapping(
            EdgeWearTopologyGraph sourceGraph,
            EdgeWearTopologyGraph outputGraph,
            EdgeWearMicroTopologyNormalizationResult normalization,
            CornerDamageCandidateRecord selected,
            PolygonFace capFace,
            float tolerance,
            CornerDamageTrialRecord trial)
        {
            HashSet<int> selectedEdges = new HashSet<int>(
                selected.IncidentGraphEdgeIndices);
            HashSet<int> originalIdentities = new HashSet<int>();
            for (int sourceEdgeIndex = 0;
                 sourceEdgeIndex < sourceGraph.Edges.Count;
                 sourceEdgeIndex++)
            {
                EdgeWearGraphEdge sourceEdge =
                    sourceGraph.Edges[sourceEdgeIndex];
                Vector3 sourceA = sourceGraph.Vertices[
                    sourceEdge.VertexA].Position;
                Vector3 sourceB = sourceGraph.Vertices[
                    sourceEdge.VertexB].Position;
                EdgeKey sourceKey = new EdgeKey(sourceA, sourceB);
                originalIdentities.Add(normalization == null
                    ? sourceEdgeIndex
                    : normalization.ResolveOriginalSourceEdgeIndex(
                        sourceKey,
                        sourceEdgeIndex));
            }
            Dictionary<int, int> capParentByVertex =
                new Dictionary<int, int>();

            for (int sourceEdgeIndex = 0;
                 sourceEdgeIndex < sourceGraph.Edges.Count;
                 sourceEdgeIndex++)
            {
                EdgeWearGraphEdge sourceEdge =
                    sourceGraph.Edges[sourceEdgeIndex];
                Vector3 sourceA = sourceGraph.Vertices[
                    sourceEdge.VertexA].Position;
                Vector3 sourceB = sourceGraph.Vertices[
                    sourceEdge.VertexB].Position;
                EdgeKey sourceKey = new EdgeKey(sourceA, sourceB);
                bool generatedTransition = normalization != null &&
                    normalization.GeneratedTransitionKeys.Contains(
                        sourceKey);
                int originalIdentity = normalization == null
                    ? sourceEdgeIndex
                    : normalization.ResolveOriginalSourceEdgeIndex(
                        sourceKey,
                        sourceEdgeIndex);

                if (!selectedEdges.Contains(sourceEdgeIndex))
                {
                    if (!outputGraph.EdgeByKey.TryGetValue(
                            sourceKey,
                            out int outputEdgeIndex))
                    {
                        trial.MissingOriginalEdgeCount++;
                        continue;
                    }
                    trial.UntouchedOriginalEdgeCount++;
                    trial.IdentityRecords.Add(
                        new CornerDamageEdgeIdentityRecord
                        {
                            Kind = generatedTransition
                                ? "untouched-generated-transition"
                                : "untouched-original",
                            OutputGraphEdgeIndex = outputEdgeIndex,
                            ParentOriginalEdgeA = originalIdentity,
                            Start = sourceA,
                            End = sourceB
                        });
                    continue;
                }

                Vector3 retainedEndpoint =
                    sourceEdge.VertexA == selected.GraphVertexIndex
                        ? sourceB
                        : sourceA;
                int matchingCapVertex = -1;
                int matchingCapVertexCount = 0;
                for (int capVertexIndex = 0;
                     capVertexIndex < capFace.Vertices.Count;
                     capVertexIndex++)
                {
                    float distanceSqr =
                        DistanceCornerDamagePointToSegmentSquared(
                            capFace.Vertices[capVertexIndex],
                            sourceA,
                            sourceB,
                            out float segmentT);
                    if (distanceSqr <= tolerance * tolerance &&
                        segmentT > 0f && segmentT < 1f)
                    {
                        matchingCapVertex = capVertexIndex;
                        matchingCapVertexCount++;
                    }
                }
                if (matchingCapVertexCount != 1)
                {
                    trial.AmbiguousIdentityCount++;
                    continue;
                }

                Vector3 intersection =
                    capFace.Vertices[matchingCapVertex];
                EdgeKey descendantKey = new EdgeKey(
                    retainedEndpoint,
                    intersection);
                if (!outputGraph.EdgeByKey.TryGetValue(
                        descendantKey,
                        out int descendantEdgeIndex))
                {
                    trial.MissingOriginalEdgeCount++;
                    continue;
                }
                trial.ShortenedDescendantEdgeCount++;
                capParentByVertex[matchingCapVertex] = originalIdentity;
                trial.IdentityRecords.Add(
                    new CornerDamageEdgeIdentityRecord
                    {
                        Kind = "shortened-descendant",
                        OutputGraphEdgeIndex = descendantEdgeIndex,
                        ParentOriginalEdgeA = originalIdentity,
                        Start = retainedEndpoint,
                        End = intersection
                    });
            }

            HashSet<int> generatedIdentities = new HashSet<int>();
            for (int capVertexIndex = 0;
                 capVertexIndex < capFace.Vertices.Count;
                 capVertexIndex++)
            {
                int nextIndex =
                    (capVertexIndex + 1) % capFace.Vertices.Count;
                if (!capParentByVertex.TryGetValue(
                        capVertexIndex,
                        out int parentA) ||
                    !capParentByVertex.TryGetValue(
                        nextIndex,
                        out int parentB))
                {
                    trial.AmbiguousIdentityCount++;
                    continue;
                }
                Vector3 start = capFace.Vertices[capVertexIndex];
                Vector3 end = capFace.Vertices[nextIndex];
                EdgeKey capKey = new EdgeKey(start, end);
                if (!outputGraph.EdgeByKey.TryGetValue(
                        capKey,
                        out int capGraphEdgeIndex))
                {
                    trial.MissingOriginalEdgeCount++;
                    continue;
                }
                int generatedIdentity =
                    ResolveCornerDamageCapRingIdentity(
                        selected.GraphVertexIndex,
                        parentA,
                        parentB);
                if (originalIdentities.Contains(generatedIdentity))
                {
                    trial.GeneratedIdentityCollisionCount++;
                    continue;
                }
                if (!generatedIdentities.Add(generatedIdentity))
                {
                    trial.AmbiguousIdentityCount++;
                    continue;
                }
                trial.CapRingEdgeCount++;
                trial.IdentityRecords.Add(
                    new CornerDamageEdgeIdentityRecord
                    {
                        Kind = "cap-ring",
                        OutputGraphEdgeIndex = capGraphEdgeIndex,
                        ParentOriginalEdgeA = parentA,
                        ParentOriginalEdgeB = parentB,
                        GeneratedIdentity = generatedIdentity,
                        Start = start,
                        End = end
                    });
            }
        }

        private static bool TryCommitCornerDamageTransactionResult(
            CornerDamageTransactionAuditResult result,
            CornerDamageTrialRecord trial,
            List<PolygonFace> acceptedFaces,
            PolygonFace acceptedCapFace,
            out string blocker)
        {
            blocker = string.Empty;
            if (result == null || trial == null || acceptedFaces == null ||
                acceptedCapFace == null || !trial.Succeeded)
            {
                blocker = "certified corner transaction has no committed geometry";
                return false;
            }

            result.StableIdentityByOutputKey.Clear();
            result.CapRingKeys.Clear();
            result.CapRingGeneratedIdentities.Clear();
            result.AffectedOriginalEdgeIndices.Clear();
            for (int recordIndex = 0;
                 recordIndex < trial.IdentityRecords.Count;
                 recordIndex++)
            {
                CornerDamageEdgeIdentityRecord record =
                    trial.IdentityRecords[recordIndex];
                EdgeKey key = new EdgeKey(record.Start, record.End);
                int stableIdentity = record.Kind == "cap-ring"
                    ? record.GeneratedIdentity
                    : record.ParentOriginalEdgeA;
                if (stableIdentity < 0 ||
                    result.StableIdentityByOutputKey.ContainsKey(key))
                {
                    blocker =
                        "certified corner transaction produced duplicate or invalid committed edge identity";
                    return false;
                }
                result.StableIdentityByOutputKey.Add(key, stableIdentity);
                if (record.Kind == "shortened-descendant")
                {
                    result.AffectedOriginalEdgeIndices.Add(
                        record.ParentOriginalEdgeA);
                }
                else if (record.Kind == "cap-ring")
                {
                    if (!result.CapRingGeneratedIdentities.Add(
                            record.GeneratedIdentity))
                    {
                        blocker =
                            "certified corner transaction produced duplicate cap-ring identity";
                        return false;
                    }
                    result.CapRingKeys.Add(key);
                }
            }

            if (result.CapRingKeys.Count != trial.CapRingEdgeCount ||
                result.AffectedOriginalEdgeIndices.Count !=
                    trial.ShortenedDescendantEdgeCount)
            {
                blocker =
                    "certified corner transaction committed identity counts do not match audit evidence";
                return false;
            }

            result.CapEdgeLengths.Clear();
            float shortestCapEdgeLength = float.PositiveInfinity;
            for (int vertexIndex = 0;
                 vertexIndex < acceptedCapFace.Vertices.Count;
                 vertexIndex++)
            {
                Vector3 start = acceptedCapFace.Vertices[vertexIndex];
                Vector3 end = acceptedCapFace.Vertices[
                    (vertexIndex + 1) % acceptedCapFace.Vertices.Count];
                float capEdgeLength = (end - start).magnitude;
                result.CapEdgeLengths.Add(capEdgeLength);
                shortestCapEdgeLength = Mathf.Min(
                    shortestCapEdgeLength,
                    capEdgeLength);
            }
            if (float.IsNaN(shortestCapEdgeLength) ||
                float.IsInfinity(shortestCapEdgeLength) ||
                shortestCapEdgeLength <= PointMergeDistance)
            {
                blocker =
                    "certified corner transaction cap has no stable edge length";
                return false;
            }

            if (!TryBuildCornerDamageConstructionFaces(
                    acceptedFaces,
                    out List<PolygonFace> acceptedConstructionFaces,
                    out int constructionSourceFaceCountAttributed,
                    out string constructionBlocker))
            {
                blocker = constructionBlocker;
                return false;
            }

            result.AcceptedFaces = acceptedFaces;
            result.AcceptedConstructionFaces =
                acceptedConstructionFaces;
            result.AcceptedCapFace = acceptedCapFace;
            result.ConstructionSourceFaceCountExpected =
                acceptedFaces.Count;
            result.ConstructionSourceFaceCountAttributed =
                constructionSourceFaceCountAttributed;
            result.AcceptedDepth = trial.Depth;
            result.AcceptedRetryFactor = trial.DepthFactor;
            result.ShortestCapEdgeLength = shortestCapEdgeLength;
            return true;
        }

        private static bool TryBuildCornerDamageConstructionFaces(
            List<PolygonFace> semanticFaces,
            out List<PolygonFace> constructionFaces,
            out int attributedSourceFaceCount,
            out string blocker)
        {
            constructionFaces = null;
            attributedSourceFaceCount = 0;
            blocker = string.Empty;
            if (semanticFaces == null || semanticFaces.Count < 4)
            {
                blocker =
                    "certified corner transaction has no construction face set";
                return false;
            }

            List<PolygonFace> cloned =
                new List<PolygonFace>(semanticFaces.Count);
            for (int faceIndex = 0;
                 faceIndex < semanticFaces.Count;
                 faceIndex++)
            {
                PolygonFace semanticFace = semanticFaces[faceIndex];
                if (semanticFace == null ||
                    semanticFace.Vertices == null ||
                    semanticFace.Vertices.Count < 3)
                {
                    blocker =
                        "certified corner transaction contains an invalid semantic face";
                    return false;
                }

                PolygonFace constructionFace = new PolygonFace(
                    new List<Vector3>(semanticFace.Vertices),
                    semanticFace.Normal,
                    semanticFace.Feature,
                    semanticFace.FeatureStrength,
                    PolygonFaceProvenanceKind.SourceFace,
                    faceIndex);
                if (constructionFace.Vertices.Count !=
                        semanticFace.Vertices.Count ||
                    constructionFace.Feature != semanticFace.Feature ||
                    Mathf.Abs(
                        constructionFace.FeatureStrength -
                        semanticFace.FeatureStrength) > 0.0000001f ||
                    Vector3.Dot(
                        constructionFace.Normal,
                        semanticFace.Normal) < 0.999999f ||
                    constructionFace.ProvenanceKind !=
                        PolygonFaceProvenanceKind.SourceFace ||
                    constructionFace.ProvenanceIndex != faceIndex)
                {
                    blocker =
                        "corner construction face attribution did not preserve the semantic face contract";
                    return false;
                }

                for (int vertexIndex = 0;
                     vertexIndex < semanticFace.Vertices.Count;
                     vertexIndex++)
                {
                    if (!constructionFace.Vertices[vertexIndex].Equals(
                            semanticFace.Vertices[vertexIndex]))
                    {
                        blocker =
                            "corner construction face attribution changed semantic geometry";
                        return false;
                    }
                }

                cloned.Add(constructionFace);
                attributedSourceFaceCount++;
            }

            if (attributedSourceFaceCount != semanticFaces.Count)
            {
                blocker =
                    "corner construction source-face attribution is incomplete";
                return false;
            }

            constructionFaces = cloned;
            return true;
        }

        private static float ResolveCornerDamageCapRingRequestedWidth(
            CornerDamageTransactionAuditResult transaction,
            float ordinaryRequestedWidth,
            float widthScale,
            out float ordinaryLimit,
            out float depthLimit,
            out float edgeLimit,
            out string winningLimit)
        {
            ordinaryLimit = 0f;
            depthLimit = 0f;
            edgeLimit = 0f;
            winningLimit = "none";
            if (transaction == null || !transaction.Succeeded)
            {
                return 0f;
            }

            ordinaryLimit = ordinaryRequestedWidth * Mathf.Clamp(
                widthScale,
                0.20f,
                1.25f);
            depthLimit = transaction.AcceptedDepth * 0.25f;
            edgeLimit = transaction.ShortestCapEdgeLength * 0.20f;
            float limitingWidth = Mathf.Min(
                ordinaryLimit,
                depthLimit,
                edgeLimit);
            if (limitingWidth == ordinaryLimit)
            {
                winningLimit = "ordinary-width";
            }
            else if (limitingWidth == depthLimit)
            {
                winningLimit = "accepted-depth";
            }
            else
            {
                winningLimit = "shortest-cap-edge";
            }
            return limitingWidth *
                ResolveCornerDamageCapRingScaleOverride();
        }

        private static int CompareCornerDamagePreviewCandidates(
            EdgeWearBevelCandidate left,
            EdgeWearBevelCandidate right)
        {
            if (left.Mandatory != right.Mandatory)
            {
                return left.Mandatory ? -1 : 1;
            }
            if (left.Mandatory)
            {
                return left.StableIdentity.CompareTo(right.StableIdentity);
            }
            int scoreOrder = right.Score.CompareTo(left.Score);
            return scoreOrder != 0
                ? scoreOrder
                : left.CandidateIndex.CompareTo(right.CandidateIndex);
        }

        private static int CalculateCornerDamagePolygonTriangleCount(
            List<PolygonFace> faces)
        {
            int triangleCount = 0;
            if (faces == null)
            {
                return triangleCount;
            }
            for (int faceIndex = 0; faceIndex < faces.Count; faceIndex++)
            {
                PolygonFace face = faces[faceIndex];
                if (face != null && face.Vertices != null &&
                    face.Vertices.Count >= 3)
                {
                    triangleCount += face.Vertices.Count - 2;
                }
            }
            return triangleCount;
        }

        private static int ResolveCornerDamageCapRingIdentity(
            int selectedGraphVertexIndex,
            int parentEdgeA,
            int parentEdgeB)
        {
            int minimumParent = Mathf.Min(parentEdgeA, parentEdgeB);
            int maximumParent = Mathf.Max(parentEdgeA, parentEdgeB);
            int identity;
            unchecked
            {
                identity = CornerDamageIdentitySalt;
                identity = identity * 486187739 +
                    selectedGraphVertexIndex;
                identity = identity * 16777619 + minimumParent;
                identity = identity * 16777619 + maximumParent;
            }
            return identity & 0x7fffffff;
        }

        private static float DistanceCornerDamagePointToSegmentSquared(
            Vector3 point,
            Vector3 start,
            Vector3 end,
            out float segmentT)
        {
            Vector3 axis = end - start;
            float lengthSqr = axis.sqrMagnitude;
            if (lengthSqr <= MinimumEdgeLengthSqr)
            {
                segmentT = 0f;
                return (point - start).sqrMagnitude;
            }
            segmentT = Mathf.Clamp01(
                Vector3.Dot(point - start, axis) / lengthSqr);
            Vector3 closest = start + axis * segmentT;
            return (point - closest).sqrMagnitude;
        }

        private static string BuildCornerDamageTrialBlocker(
            CornerDamageTrialRecord trial,
            float tolerance,
            int expectedShortenedDescendantCount,
            int expectedCapRingEdgeCount)
        {
            if (trial.CapFaceCount != 1)
            {
                return "cap-face-count-invalid";
            }
            if (trial.CapVertexCount < 3 || trial.CapArea <= 0f)
            {
                return "cap-geometry-invalid";
            }
            if (trial.MaximumCapPlaneResidual > tolerance)
            {
                return "cap-plane-residual-exceeded";
            }
            if (trial.OpenEdgeCount != 0)
            {
                return "topology-open-edge";
            }
            if (trial.NonManifoldEdgeCount != 0)
            {
                return "topology-non-manifold-edge";
            }
            if (trial.TJunctionCount != 0)
            {
                return "topology-t-junction";
            }
            if (trial.InvalidFaceCount != 0)
            {
                return "face-invalid-or-below-stable-area";
            }
            if (trial.NonPlanarFaceCount != 0)
            {
                return "face-non-planar";
            }
            if (trial.NonConvexFaceCount != 0)
            {
                return "face-non-convex";
            }
            if (trial.WindingFailureCount != 0)
            {
                return "face-winding-invalid";
            }
            if (trial.BoundsValid != 1)
            {
                return "bounds-expanded";
            }
            if (trial.BudgetValid != 1)
            {
                return "corner-cut-budget-delta-exceeded";
            }
            if (trial.VolumeLoss <= 0.000000001 ||
                trial.VolumeLossFraction > 0.12)
            {
                return "volume-loss-outside-bounds";
            }
            if (trial.MissingOriginalEdgeCount != 0)
            {
                return "original-edge-identity-missing";
            }
            if (trial.AmbiguousIdentityCount != 0)
            {
                return "edge-identity-ambiguous";
            }
            if (trial.GeneratedIdentityCollisionCount != 0)
            {
                return "cap-ring-generated-identity-collision";
            }
            if (trial.ShortenedDescendantEdgeCount !=
                expectedShortenedDescendantCount)
            {
                return "shortened-descendant-count-mismatch";
            }
            if (trial.CapRingEdgeCount != expectedCapRingEdgeCount)
            {
                return "cap-ring-identity-count-mismatch";
            }
            return "corner-damage-certification-failed";
        }

        private static void AddBoundaryAdjacency(
            Dictionary<VertexKey, List<int>> adjacency,
            VertexKey key,
            int edgeIndex)
        {
            if (!adjacency.TryGetValue(key, out List<int> list))
            {
                list = new List<int>();
                adjacency.Add(key, list);
            }
            list.Add(edgeIndex);
        }

        private static void EnqueueAdjacentBoundaryEdges(
            VertexKey vertex,
            Dictionary<VertexKey, List<int>> adjacency,
            HashSet<int> visited,
            Queue<int> queue)
        {
            List<int> adjacent = adjacency[vertex];
            for (int i = 0; i < adjacent.Count; i++)
            {
                if (visited.Add(adjacent[i]))
                {
                    queue.Enqueue(adjacent[i]);
                }
            }
        }

        #endregion
    }
}
