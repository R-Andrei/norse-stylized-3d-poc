using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;
using ProgrammaticStylized3D.Geometry;

namespace ProgrammaticStylized3D.Geometry.Masses
{
    public static partial class MassGenerator
    {
        #region Edge wear diagnostic logging

        private sealed class PendingEdgeWearStableFingerprint
        {
            public bool Valid;
            public int SourceEdgeCount;
            public int StructuralEligibleCount;
            public int GeometricEligibleCount;
            public int CoexistenceEligibleCount;
            public int SelectedCount;
            public int CertifiedCount;
            public GeneratedGeometryStableFingerprint ExclusionReasons;
            public GeneratedGeometryStableFingerprint SelectedEdges;
            public GeneratedGeometryStableFingerprint CertifiedEdges;
            public GeneratedGeometryStableFingerprint GeometryTopology;
        }

        private static PendingEdgeWearStableFingerprint
            pendingEdgeWearStableFingerprint;

        private static CornerDamageTransactionAuditResult
            capturedCornerDamageTransactionAudit;

        private static void ResetCornerDamageTransactionAuditCapture()
        {
            capturedCornerDamageTransactionAudit = null;
        }

        private static void CaptureCornerDamageTransactionAudit(
            CornerDamageTransactionAuditResult audit)
        {
            capturedCornerDamageTransactionAudit = audit;
        }

        private static string CompleteCornerDamageTransactionAuditCapture(
            MassRecipe recipe)
        {
            CornerDamageTransactionAuditResult audit =
                capturedCornerDamageTransactionAudit;
            capturedCornerDamageTransactionAudit = null;
            return BuildCornerDamageTransactionAuditReport(recipe, audit);
        }

        private static CornerDamagePreviewConstructionRecord
            capturedCornerDamagePreview;

        private static void ResetCornerDamagePreviewCapture()
        {
            capturedCornerDamagePreview = null;
        }

        private static void BeginCornerDamagePreviewCapture(
            CornerDamagePreviewKind previewKind,
            CornerDamageTransactionAuditResult transaction,
            MassSurfaceFeatureSettings settings,
            float ordinaryRequestedWidth,
            float capRingOrdinaryLimit,
            float capRingDepthLimit,
            float capRingEdgeLimit,
            string capRingWinningLimit,
            float capRingRequestedWidth)
        {
            capturedCornerDamagePreview =
                new CornerDamagePreviewConstructionRecord
                {
                    PreviewKind = previewKind,
                    Transaction = transaction,
                    AuthoringEnabled = settings.CornerChippingEnabled,
                    OrdinaryRequestedWidth = ordinaryRequestedWidth,
                    CapRingWidthScale =
                        settings.CornerChipCapRingWidthScale,
                    CapRingOrdinaryLimit = capRingOrdinaryLimit,
                    CapRingDepthLimit = capRingDepthLimit,
                    CapRingEdgeLimit = capRingEdgeLimit,
                    CapRingWinningLimit =
                        capRingWinningLimit ?? string.Empty,
                    CapRingWearStrength =
                        settings.CornerChipCapRingWearStrength,
                    CapRingRequestedWidth = capRingRequestedWidth,
                    ExpectedMandatoryCount = transaction == null
                        ? 0
                        : transaction.CapRingKeys.Count
                };
        }

        private static void CaptureCornerDamagePreviewCandidateSelection(
            int expectedMandatoryCount,
            int mandatoryCandidateCount,
            int mandatorySelectedCount)
        {
            if (capturedCornerDamagePreview == null)
            {
                capturedCornerDamagePreview =
                    new CornerDamagePreviewConstructionRecord();
            }
            capturedCornerDamagePreview.ExpectedMandatoryCount =
                expectedMandatoryCount;
            capturedCornerDamagePreview.MandatoryCandidateCount =
                mandatoryCandidateCount;
            capturedCornerDamagePreview.MandatorySelectedCount =
                mandatorySelectedCount;
        }

        private static void CaptureCornerDamagePreviewBlocker(
            string blocker)
        {
            if (capturedCornerDamagePreview == null)
            {
                capturedCornerDamagePreview =
                    new CornerDamagePreviewConstructionRecord();
            }
            capturedCornerDamagePreview.Blocker = blocker ?? string.Empty;
        }

        private static void CaptureCornerDamagePreviewOutcome(
            UnifiedEdgeWearPreviewStatus previewStatus,
            string blocker)
        {
            if (capturedCornerDamagePreview == null)
            {
                capturedCornerDamagePreview =
                    new CornerDamagePreviewConstructionRecord();
            }
            capturedCornerDamagePreview.CornerPreviewStatus = previewStatus;
            if (!previewStatus.PreviewApplied)
            {
                capturedCornerDamagePreview.Blocker =
                    !string.IsNullOrEmpty(blocker)
                        ? blocker
                        : previewStatus.Diagnostic ?? string.Empty;
            }
        }

#if UNITY_EDITOR
        private static List<Vector3>
            ExtractCornerDamagePreviewMarkerPositions()
        {
            CornerDamageTransactionAuditResult transaction =
                capturedCornerDamagePreview == null
                    ? null
                    : capturedCornerDamagePreview.Transaction;
            List<Vector3> positions = new List<Vector3>();
            if (transaction == null || !transaction.Succeeded ||
                transaction.AcceptedCapFace == null)
            {
                return positions;
            }

            positions.Add(transaction.SelectedPosition);
            positions.AddRange(transaction.AcceptedCapFace.Vertices);
            return positions;
        }

        private static void ApplyCornerDamagePreviewMarkerPositions(
            List<Vector3> positions)
        {
            if (capturedCornerDamagePreview == null ||
                positions == null || positions.Count < 2)
            {
                return;
            }

            capturedCornerDamagePreview.SelectedCornerLocalPosition =
                positions[0];
            capturedCornerDamagePreview.CapVerticesLocal.Clear();
            capturedCornerDamagePreview.CapEdgeLengthsLocal.Clear();
            for (int positionIndex = 1;
                 positionIndex < positions.Count;
                 positionIndex++)
            {
                capturedCornerDamagePreview.CapVerticesLocal.Add(
                    positions[positionIndex]);
            }
            int capVertexCount =
                capturedCornerDamagePreview.CapVerticesLocal.Count;
            for (int vertexIndex = 0;
                 vertexIndex < capVertexCount;
                 vertexIndex++)
            {
                Vector3 start = capturedCornerDamagePreview.
                    CapVerticesLocal[vertexIndex];
                Vector3 end = capturedCornerDamagePreview.
                    CapVerticesLocal[(vertexIndex + 1) % capVertexCount];
                capturedCornerDamagePreview.CapEdgeLengthsLocal.Add(
                    (end - start).magnitude);
            }
        }

        private static CornerDamagePreviewStatus
            BuildDisabledCornerDamagePreviewStatus(
                MassRecipe recipe,
                MassSurfaceFeatureSettings settings,
                CornerDamagePreviewKind previewKind)
        {
            CornerDamagePreviewStatus result =
                new CornerDamagePreviewStatus
                {
                    PreviewKind = previewKind,
                    AuthoringEnabled = false,
                    ShapeSeed = recipe == null ? 0 : recipe.ShapeSeed,
                    RequestedDepthFraction = settings.CornerChipDepth,
                    DepthVariation = settings.CornerChipDepthVariation,
                    TopFacingPreference =
                        settings.CornerChipTopFacingPreference,
                    CapRingWidthScale =
                        settings.CornerChipCapRingWidthScale,
                    CapRingWearStrength =
                        settings.CornerChipCapRingWearStrength,
                    Diagnostic =
                        "corner chipping authoring is disabled"
                };
            result.Report = BuildCornerDamagePreviewReport(
                result,
                default,
                default,
                null);
            return result;
        }

        private static CornerDamagePreviewStatus
            CompleteCornerDamagePreviewCapture(
                MassRecipe recipe,
                UnifiedEdgeWearPreviewStatus baselineStatus,
                UnifiedEdgeWearPreviewStatus cornerStatus,
                double baselineMilliseconds,
                double cornerMilliseconds)
        {
            CornerDamagePreviewConstructionRecord capture =
                capturedCornerDamagePreview;
            capturedCornerDamagePreview = null;
            CornerDamagePreviewStatus result =
                new CornerDamagePreviewStatus
                {
                    PreviewKind = capture == null
                        ? CornerDamagePreviewKind.GeometryOnly
                        : capture.PreviewKind,
                    ShapeSeed = recipe == null ? 0 : recipe.ShapeSeed,
                    AuthoringEnabled =
                        capture != null && capture.AuthoringEnabled,
                    OrdinaryRequestedWidth = capture == null
                        ? 0f
                        : capture.OrdinaryRequestedWidth,
                    CapRingWidthScale = capture == null
                        ? 0f
                        : capture.CapRingWidthScale,
                    CapRingOrdinaryLimit = capture == null
                        ? 0f
                        : capture.CapRingOrdinaryLimit,
                    CapRingDepthLimit = capture == null
                        ? 0f
                        : capture.CapRingDepthLimit,
                    CapRingEdgeLimit = capture == null
                        ? 0f
                        : capture.CapRingEdgeLimit,
                    CapRingWinningLimit = capture == null
                        ? string.Empty
                        : capture.CapRingWinningLimit ?? string.Empty,
                    CapRingWearStrength = capture == null
                        ? 0f
                        : capture.CapRingWearStrength,
                    SelectedCornerLocalPosition = capture == null
                        ? Vector3.zero
                        : capture.SelectedCornerLocalPosition,
                    CapVerticesLocal = capture == null
                        ? Array.Empty<Vector3>()
                        : capture.CapVerticesLocal.ToArray(),
                    CapEdgeLengths = capture == null
                        ? Array.Empty<float>()
                        : capture.CapEdgeLengthsLocal.ToArray(),
                    BaselineMilliseconds = baselineMilliseconds,
                    CornerMilliseconds = cornerMilliseconds,
                    CandidateCount = cornerStatus.CandidateCount,
                    ActiveEdgeCount = cornerStatus.ActiveEdgeCount,
                    DeferredEdgeCount = cornerStatus.DeferredEdgeCount,
                    RejectedEdgeCount = cornerStatus.RejectedEdgeCount,
                    BevelFaceCount = cornerStatus.BevelFaceCount,
                    TriangleCount = cornerStatus.TriangleCount,
                    GeometryFaceCount = 0,
                    DebugEdges = cornerStatus.DebugEdges ??
                        Array.Empty<EdgeWearDebugEdgeRecord>()
                };

            CornerDamageTransactionAuditResult transaction =
                capture == null ? null : capture.Transaction;
            result.TransactionCertified =
                transaction != null && transaction.Succeeded;
            result.CandidateCornerCount = transaction == null
                ? 0
                : transaction.EligibleCandidateCount;
            result.AcceptedCornerRank = transaction == null
                ? -1
                : transaction.SelectedCandidateRank;
            result.SelectedGraphVertexIndex = transaction == null
                ? -1
                : transaction.SelectedGraphVertexIndex;
            result.AcceptedTrialIndex = transaction == null
                ? -1
                : transaction.AcceptedTrialIndex;
            result.RequestedDepthFraction = transaction == null
                ? 0f
                : transaction.RequestedDepthFraction;
            result.DepthVariation = transaction == null
                ? 0f
                : transaction.DepthVariation;
            result.DepthVariationIdentity = transaction == null
                ? 0f
                : transaction.DepthVariationIdentity;
            result.ResolvedDepthFraction = transaction == null
                ? 0f
                : transaction.ResolvedDepthFraction;
            result.TopFacingPreference = transaction == null
                ? 0f
                : transaction.TopFacingPreference;
            result.ShortestIncidentEdgeLength = transaction == null
                ? 0f
                : transaction.ShortestIncidentEdgeLength;
            result.RequestedDepthAbsolute = transaction == null
                ? 0f
                : transaction.BaseDepth;
            result.AcceptedDepth = transaction == null
                ? 0f
                : transaction.AcceptedDepth;
            result.AcceptedDepthFraction =
                result.ShortestIncidentEdgeLength > PointMergeDistance
                    ? result.AcceptedDepth /
                        result.ShortestIncidentEdgeLength
                    : 0f;
            result.AcceptedRetryFactor = transaction == null
                ? 0f
                : transaction.AcceptedRetryFactor;
            result.AcceptedVsRequestedRatio =
                result.RequestedDepthAbsolute > PointMergeDistance
                    ? result.AcceptedDepth /
                        result.RequestedDepthAbsolute
                    : 0f;
            result.CapRingRequestedWidth = capture == null
                ? 0f
                : capture.CapRingRequestedWidth;
            result.ExpectedCapRingEdgeCount = capture == null
                ? 0
                : capture.ExpectedMandatoryCount;
            result.MandatoryCandidateCount = capture == null
                ? 0
                : capture.MandatoryCandidateCount;
            result.MandatorySelectedCount = capture == null
                ? 0
                : capture.MandatorySelectedCount;

            if (transaction != null)
            {
                result.SemanticCapFaceCount = transaction.AcceptedCapFace == null
                    ? 0
                    : 1;
                result.ConstructionSourceFaceCountExpected =
                    transaction.ConstructionSourceFaceCountExpected;
                result.ConstructionSourceFaceCountAttributed =
                    transaction.ConstructionSourceFaceCountAttributed;
                result.GeometryFaceCount = transaction.AcceptedFaces == null
                    ? 0
                    : transaction.AcceptedFaces.Count;
                List<int> affected = new List<int>(
                    transaction.AffectedOriginalEdgeIndices);
                affected.Sort();
                result.AffectedOriginalEdgeIndices = affected.ToArray();
                List<int> mandatory = new List<int>(
                    transaction.CapRingGeneratedIdentities);
                mandatory.Sort();
                result.MandatoryCapRingIdentities = mandatory.ToArray();
                if (transaction.AcceptedTrialIndex >= 0 &&
                    transaction.AcceptedTrialIndex <
                        transaction.Trials.Count)
                {
                    result.CapFaceCount = transaction.Trials[
                        transaction.AcceptedTrialIndex].CapFaceCount;
                }
            }

            HashSet<int> affectedIdentities = new HashSet<int>(
                result.AffectedOriginalEdgeIndices);
            HashSet<int> baselineBuilt =
                CollectCertifiedOrdinaryEdgeIdentities(
                    baselineStatus.DebugEdges);
            HashSet<int> cornerBuilt =
                CollectCertifiedOrdinaryEdgeIdentities(
                    cornerStatus.DebugEdges);
            HashSet<int> mandatoryBuilt =
                CollectCertifiedCapRingEdgeIdentities(
                    cornerStatus.DebugEdges);
            result.BaselineBuiltOrdinaryCount = baselineBuilt.Count;

            List<int> collateralLost = new List<int>();
            foreach (int identity in baselineBuilt)
            {
                if (affectedIdentities.Contains(identity))
                {
                    continue;
                }
                result.UnrelatedBaselineBuiltCount++;
                if (cornerBuilt.Contains(identity))
                {
                    result.UnrelatedRetainedCount++;
                }
                else
                {
                    collateralLost.Add(identity);
                }
            }
            collateralLost.Sort();
            result.CollateralLostIdentities = collateralLost.ToArray();
            result.CollateralLostCount = collateralLost.Count;

            for (int identityIndex = 0;
                 identityIndex < result.MandatoryCapRingIdentities.Length;
                 identityIndex++)
            {
                if (mandatoryBuilt.Contains(
                        result.MandatoryCapRingIdentities[identityIndex]))
                {
                    result.MandatoryBuiltCount++;
                }
            }

            string blocker = capture == null
                ? "corner preview capture was unavailable"
                : capture.Blocker ?? string.Empty;
            bool geometryOnly = result.PreviewKind ==
                CornerDamagePreviewKind.GeometryOnly;
            bool accepted = geometryOnly
                ? result.AuthoringEnabled &&
                    result.TransactionCertified &&
                    transaction != null &&
                    transaction.AcceptedFaces != null &&
                    transaction.AcceptedCapFace != null &&
                    result.CapFaceCount == 1 &&
                    result.GeometryFaceCount > 0 &&
                    result.TriangleCount > 0 &&
                    cornerStatus.PreviewApplied &&
                    result.CandidateCount == 0 &&
                    result.BevelFaceCount == 0 &&
                    string.IsNullOrEmpty(blocker)
                : result.AuthoringEnabled &&
                    baselineStatus.PreviewApplied &&
                    result.TransactionCertified &&
                    transaction != null &&
                    transaction.AcceptedFaces != null &&
                    transaction.AcceptedConstructionFaces != null &&
                    transaction.AcceptedCapFace != null &&
                    transaction.ConstructionSourceFaceCountExpected > 0 &&
                    transaction.ConstructionSourceFaceCountAttributed ==
                        transaction.ConstructionSourceFaceCountExpected &&
                    result.CapFaceCount == 1 &&
                    result.ExpectedCapRingEdgeCount > 0 &&
                    result.MandatoryCandidateCount ==
                        result.ExpectedCapRingEdgeCount &&
                    result.MandatorySelectedCount ==
                        result.ExpectedCapRingEdgeCount &&
                    result.MandatoryBuiltCount ==
                        result.ExpectedCapRingEdgeCount &&
                    cornerStatus.PreviewApplied &&
                    result.CollateralLostCount == 0 &&
                    string.IsNullOrEmpty(blocker);
            result.PreviewApplied = accepted;
            result.Diagnostic = accepted
                ? geometryOnly
                    ? "certified corner chip geometry applied without edge-wear bevel construction"
                    : "certified corner chip and complete cap-ring bevel applied; unrelated baseline bevels retained"
                : ResolveCornerDamagePreviewFailure(
                    result,
                    baselineStatus,
                    cornerStatus,
                    blocker);
            result.Report = BuildCornerDamagePreviewReport(
                result,
                baselineStatus,
                cornerStatus,
                transaction);
            return result;
        }

        private static void ApplyCornerDamageSearchSummary(
            CornerDamagePreviewStatus status,
            int candidateCornerCount,
            int attemptedCornerCount,
            int attemptedConfigurationCount,
            int acceptedCornerRank,
            float capRingCommittedScale,
            string searchFailureStage,
            string searchFailureReason,
            string searchAttemptSummary)
        {
            if (status == null)
            {
                return;
            }

            status.CandidateCornerCount = Mathf.Max(
                status.CandidateCornerCount,
                candidateCornerCount);
            status.AttemptedCornerCount = Mathf.Max(
                0,
                attemptedCornerCount);
            status.AttemptedConfigurationCount = Mathf.Max(
                0,
                attemptedConfigurationCount);
            status.AcceptedCornerRank = acceptedCornerRank;
            status.CapRingCommittedScale = capRingCommittedScale;
            status.SearchFailureStage = string.IsNullOrEmpty(
                searchFailureStage)
                    ? "none"
                    : searchFailureStage;
            status.SearchFailureReason = string.IsNullOrEmpty(
                searchFailureReason)
                    ? "none"
                    : searchFailureReason;
            status.SearchAttemptSummary =
                searchAttemptSummary ?? string.Empty;

            StringBuilder builder = new StringBuilder(
                status.Report == null ? 256 : status.Report.Length + 256);
            if (!string.IsNullOrEmpty(status.Report))
            {
                builder.Append(status.Report.TrimEnd());
                builder.AppendLine();
            }
            builder.Append("candidateCorners=");
            builder.AppendLine(status.CandidateCornerCount.ToString(
                CultureInfo.InvariantCulture));
            builder.Append("attemptedCorners=");
            builder.AppendLine(status.AttemptedCornerCount.ToString(
                CultureInfo.InvariantCulture));
            builder.Append("attemptedConfigurations=");
            builder.AppendLine(status.AttemptedConfigurationCount.ToString(
                CultureInfo.InvariantCulture));
            builder.Append("acceptedCornerRank=");
            builder.AppendLine(status.AcceptedCornerRank.ToString(
                CultureInfo.InvariantCulture));
            builder.Append("capRingCommittedScale=");
            builder.AppendLine(status.CapRingCommittedScale.ToString(
                "G12",
                CultureInfo.InvariantCulture));
            builder.Append("searchFailureStage=");
            builder.AppendLine(status.SearchFailureStage);
            builder.Append("searchFailureReason=");
            builder.AppendLine(status.SearchFailureReason);
            builder.Append("searchAttempts=");
            builder.AppendLine(status.SearchAttemptSummary);
            status.Report = builder.ToString();
        }

        private static HashSet<int>
            CollectCertifiedOrdinaryEdgeIdentities(
                EdgeWearDebugEdgeRecord[] records)
        {
            HashSet<int> identities = new HashSet<int>();
            if (records == null)
            {
                return identities;
            }
            for (int recordIndex = 0;
                 recordIndex < records.Length;
                 recordIndex++)
            {
                EdgeWearDebugEdgeRecord record = records[recordIndex];
                if (!record.CornerDamageCapRing &&
                    record.State == EdgeWearDebugEdgeState.Certified)
                {
                    identities.Add(record.EdgeIndex);
                }
            }
            return identities;
        }

        private static HashSet<int>
            CollectCertifiedCapRingEdgeIdentities(
                EdgeWearDebugEdgeRecord[] records)
        {
            HashSet<int> identities = new HashSet<int>();
            if (records == null)
            {
                return identities;
            }
            for (int recordIndex = 0;
                 recordIndex < records.Length;
                 recordIndex++)
            {
                EdgeWearDebugEdgeRecord record = records[recordIndex];
                if (record.CornerDamageCapRing &&
                    record.Mandatory &&
                    record.State == EdgeWearDebugEdgeState.Certified)
                {
                    identities.Add(record.EdgeIndex);
                }
            }
            return identities;
        }

        private static string ResolveCornerDamagePreviewFailure(
            CornerDamagePreviewStatus result,
            UnifiedEdgeWearPreviewStatus baselineStatus,
            UnifiedEdgeWearPreviewStatus cornerStatus,
            string blocker)
        {
            if (!result.AuthoringEnabled)
            {
                return "corner chipping authoring is disabled";
            }
            if (!string.IsNullOrEmpty(blocker))
            {
                return blocker;
            }
            if (result.PreviewKind == CornerDamagePreviewKind.GeometryOnly)
            {
                if (!result.TransactionCertified)
                {
                    return "corner-damage transaction did not certify";
                }
                if (result.CapFaceCount != 1)
                {
                    return "corner transaction did not produce exactly one cap face";
                }
                if (!cornerStatus.PreviewApplied)
                {
                    return string.IsNullOrEmpty(cornerStatus.Diagnostic)
                        ? "corner chip geometry did not triangulate"
                        : cornerStatus.Diagnostic;
                }
                if (result.CandidateCount != 0 ||
                    result.BevelFaceCount != 0)
                {
                    return "geometry-only preview entered edge-wear bevel construction";
                }
                return "corner chip geometry preview failed an unspecified acceptance invariant";
            }
            if (!baselineStatus.PreviewApplied)
            {
                return "ordinary unified baseline preview did not apply";
            }
            if (!result.TransactionCertified)
            {
                return "corner-damage transaction did not certify";
            }
            if (result.CapFaceCount != 1)
            {
                return "corner transaction did not produce exactly one cap face";
            }
            if (result.ExpectedCapRingEdgeCount <= 0)
            {
                return "corner transaction produced no cap-ring identities";
            }
            if (result.MandatoryCandidateCount !=
                result.ExpectedCapRingEdgeCount)
            {
                return "not every cap-ring edge became a mandatory bevel candidate";
            }
            if (result.MandatorySelectedCount !=
                result.ExpectedCapRingEdgeCount)
            {
                return "not every mandatory cap-ring candidate was selected";
            }
            if (!cornerStatus.PreviewApplied)
            {
                return string.IsNullOrEmpty(cornerStatus.Diagnostic)
                    ? "corner preview geometry did not certify"
                    : cornerStatus.Diagnostic;
            }
            if (result.MandatoryBuiltCount !=
                result.ExpectedCapRingEdgeCount)
            {
                return "one or more mandatory cap-ring bevels failed to build";
            }
            if (result.CollateralLostCount != 0)
            {
                return "unrelated accepted bevel identities were lost";
            }
            return "corner preview failed an unspecified acceptance invariant";
        }

        private static string BuildCornerDamagePreviewReport(
            CornerDamagePreviewStatus result,
            UnifiedEdgeWearPreviewStatus baselineStatus,
            UnifiedEdgeWearPreviewStatus cornerStatus,
            CornerDamageTransactionAuditResult transaction)
        {
            bool geometryOnly = result.PreviewKind ==
                CornerDamagePreviewKind.GeometryOnly;
            StringBuilder builder = new StringBuilder(8192);
            builder.AppendLine(geometryOnly
                ? "GeneratedMass EW-C1A.3 corner-chip preview"
                : "GeneratedMass EW-C1A.3 corner-chip and edge-wear preview");
            builder.AppendLine(geometryOnly
                ? "contract=EW-C1A.3-corner-chip-preview"
                : "contract=EW-C1A.3-corner-chip-edge-wear");
            builder.Append("previewMode=");
            builder.AppendLine(geometryOnly
                ? "geometry-only"
                : "with-edge-wear");
            builder.Append("status=");
            builder.AppendLine(result.PreviewApplied ? "passed" : "failed");
            builder.Append("shapeSeed=");
            builder.AppendLine(result.ShapeSeed.ToString(
                CultureInfo.InvariantCulture));
            builder.Append("diagnostic=");
            builder.AppendLine(result.Diagnostic ?? string.Empty);
            builder.Append("cornerChippingEnabled=");
            builder.AppendLine(result.AuthoringEnabled ? "1" : "0");
            builder.Append("cornerChipDepthRequested=");
            builder.AppendLine(result.RequestedDepthFraction.ToString(
                "G12", CultureInfo.InvariantCulture));
            builder.Append("cornerChipDepthVariation=");
            builder.AppendLine(result.DepthVariation.ToString(
                "G12", CultureInfo.InvariantCulture));
            builder.Append("cornerChipDepthVariationIdentity=");
            builder.AppendLine(result.DepthVariationIdentity.ToString(
                "G12", CultureInfo.InvariantCulture));
            builder.Append("cornerChipDepthResolved=");
            builder.AppendLine(result.ResolvedDepthFraction.ToString(
                "G12", CultureInfo.InvariantCulture));
            builder.Append("topFacingPreference=");
            builder.AppendLine(result.TopFacingPreference.ToString(
                "G12", CultureInfo.InvariantCulture));
            builder.Append("shortestIncidentEdgeLength=");
            builder.AppendLine(result.ShortestIncidentEdgeLength.ToString(
                "G12", CultureInfo.InvariantCulture));
            builder.Append("requestedDepthAbsolute=");
            builder.AppendLine(result.RequestedDepthAbsolute.ToString(
                "G12", CultureInfo.InvariantCulture));
            builder.Append("acceptedDepth=");
            builder.AppendLine(result.AcceptedDepth.ToString(
                "G12", CultureInfo.InvariantCulture));
            builder.Append("acceptedDepthAbsolute=");
            builder.AppendLine(result.AcceptedDepth.ToString(
                "G12", CultureInfo.InvariantCulture));
            builder.Append("acceptedDepthFraction=");
            builder.AppendLine(result.AcceptedDepthFraction.ToString(
                "G12", CultureInfo.InvariantCulture));
            builder.Append("acceptedRetryFactor=");
            builder.AppendLine(result.AcceptedRetryFactor.ToString(
                "G12", CultureInfo.InvariantCulture));
            builder.Append("acceptedVsRequestedRatio=");
            builder.AppendLine(result.AcceptedVsRequestedRatio.ToString(
                "G12", CultureInfo.InvariantCulture));
            builder.Append("selectedCornerLocalPosition=");
            AppendCornerDamageVector3(
                builder,
                result.SelectedCornerLocalPosition);
            builder.AppendLine();
            builder.Append("capEdgeLengths=");
            AppendCornerDamageFloatArray(
                builder,
                result.CapEdgeLengths);
            builder.AppendLine();
            builder.Append("transactionCertified=");
            builder.AppendLine(result.TransactionCertified ? "1" : "0");
            builder.Append("selectedGraphVertex=");
            builder.AppendLine(result.SelectedGraphVertexIndex.ToString(
                CultureInfo.InvariantCulture));
            builder.Append("acceptedTrial=");
            builder.AppendLine(result.AcceptedTrialIndex.ToString(
                CultureInfo.InvariantCulture));
            builder.Append("capFaces=");
            builder.AppendLine(result.CapFaceCount.ToString(
                CultureInfo.InvariantCulture));
            builder.Append("geometryFaces=");
            builder.AppendLine(result.GeometryFaceCount.ToString(
                CultureInfo.InvariantCulture));
            builder.Append("geometryTriangles=");
            builder.AppendLine(result.TriangleCount.ToString(
                CultureInfo.InvariantCulture));

            if (geometryOnly)
            {
                builder.Append("ordinaryBevelCandidates=");
                builder.AppendLine(result.CandidateCount.ToString(
                    CultureInfo.InvariantCulture));
                builder.Append("bevelFaces=");
                builder.AppendLine(result.BevelFaceCount.ToString(
                    CultureInfo.InvariantCulture));
                builder.Append("timingsMilliseconds=geometry:");
                builder.AppendLine(result.CornerMilliseconds.ToString(
                    "F3", CultureInfo.InvariantCulture));
                builder.Append("geometryDiagnostic=");
                builder.AppendLine(cornerStatus.Diagnostic ?? string.Empty);
                return builder.ToString();
            }

            builder.Append("ordinaryRequestedWidth=");
            builder.AppendLine(result.OrdinaryRequestedWidth.ToString(
                "G12", CultureInfo.InvariantCulture));
            builder.Append("capRingWidthScale=");
            builder.AppendLine(result.CapRingWidthScale.ToString(
                "G12", CultureInfo.InvariantCulture));
            builder.Append("capRingOrdinaryLimit=");
            builder.AppendLine(result.CapRingOrdinaryLimit.ToString(
                "G12", CultureInfo.InvariantCulture));
            builder.Append("capRingDepthLimit=");
            builder.AppendLine(result.CapRingDepthLimit.ToString(
                "G12", CultureInfo.InvariantCulture));
            builder.Append("capRingEdgeLimit=");
            builder.AppendLine(result.CapRingEdgeLimit.ToString(
                "G12", CultureInfo.InvariantCulture));
            builder.Append("capRingWinningLimit=");
            builder.AppendLine(result.CapRingWinningLimit ?? string.Empty);
            builder.Append("capRingWearStrength=");
            builder.AppendLine(result.CapRingWearStrength.ToString(
                "G12", CultureInfo.InvariantCulture));
            builder.Append("capRingRequestedWidth=");
            builder.AppendLine(result.CapRingRequestedWidth.ToString(
                "G12", CultureInfo.InvariantCulture));
            builder.Append("baselinePreviewApplied=");
            builder.AppendLine(baselineStatus.PreviewApplied ? "1" : "0");
            builder.Append("cornerPreviewApplied=");
            builder.AppendLine(cornerStatus.PreviewApplied ? "1" : "0");
            builder.Append("semanticCapFaces=");
            builder.AppendLine(result.CapFaceCount.ToString(
                CultureInfo.InvariantCulture));
            builder.Append("constructionSourceProvenance=");
            builder.Append(transaction == null
                ? 0
                : transaction.ConstructionSourceFaceCountAttributed);
            builder.Append('/');
            builder.AppendLine((transaction == null
                ? 0
                : transaction.ConstructionSourceFaceCountExpected).ToString(
                    CultureInfo.InvariantCulture));
            builder.Append("mandatoryCapRing=");
            builder.Append(result.ExpectedCapRingEdgeCount);
            builder.Append('/');
            builder.Append(result.MandatoryCandidateCount);
            builder.Append('/');
            builder.Append(result.MandatorySelectedCount);
            builder.Append('/');
            builder.AppendLine(result.MandatoryBuiltCount.ToString(
                CultureInfo.InvariantCulture));
            builder.Append("ordinaryRetention=");
            builder.Append(result.BaselineBuiltOrdinaryCount);
            builder.Append('/');
            builder.Append(result.UnrelatedBaselineBuiltCount);
            builder.Append('/');
            builder.Append(result.UnrelatedRetainedCount);
            builder.Append('/');
            builder.AppendLine(result.CollateralLostCount.ToString(
                CultureInfo.InvariantCulture));
            builder.Append("cornerConstruction=");
            builder.Append(result.CandidateCount);
            builder.Append('/');
            builder.Append(result.ActiveEdgeCount);
            builder.Append('/');
            builder.Append(result.DeferredEdgeCount);
            builder.Append('/');
            builder.Append(result.RejectedEdgeCount);
            builder.Append('/');
            builder.Append(result.BevelFaceCount);
            builder.Append('/');
            builder.AppendLine(result.TriangleCount.ToString(
                CultureInfo.InvariantCulture));
            builder.Append("timingsMilliseconds=baseline:");
            builder.Append(result.BaselineMilliseconds.ToString(
                "F3", CultureInfo.InvariantCulture));
            builder.Append(",corner:");
            builder.AppendLine(result.CornerMilliseconds.ToString(
                "F3", CultureInfo.InvariantCulture));
            builder.Append("affectedOriginalIdentities=");
            AppendCornerDamageIntArray(
                builder,
                result.AffectedOriginalEdgeIndices);
            builder.AppendLine();
            builder.Append("mandatoryCapRingIdentities=");
            AppendCornerDamageIntArray(
                builder,
                result.MandatoryCapRingIdentities);
            builder.AppendLine();
            builder.Append("collateralLostIdentities=");
            AppendCornerDamageIntArray(
                builder,
                result.CollateralLostIdentities);
            builder.AppendLine();
            builder.Append("baselineDiagnostic=");
            builder.AppendLine(baselineStatus.Diagnostic ?? string.Empty);
            builder.Append("cornerDiagnostic=");
            builder.AppendLine(cornerStatus.Diagnostic ?? string.Empty);
            return builder.ToString();
        }

        private static void AppendCornerDamageVector3(
            StringBuilder builder,
            Vector3 value)
        {
            builder.Append('(');
            builder.Append(value.x.ToString(
                "G12", CultureInfo.InvariantCulture));
            builder.Append('/');
            builder.Append(value.y.ToString(
                "G12", CultureInfo.InvariantCulture));
            builder.Append('/');
            builder.Append(value.z.ToString(
                "G12", CultureInfo.InvariantCulture));
            builder.Append(')');
        }

        private static void AppendCornerDamageFloatArray(
            StringBuilder builder,
            float[] values)
        {
            builder.Append('{');
            if (values != null)
            {
                for (int index = 0; index < values.Length; index++)
                {
                    if (index > 0)
                    {
                        builder.Append('/');
                    }
                    builder.Append(values[index].ToString(
                        "G12", CultureInfo.InvariantCulture));
                }
            }
            builder.Append('}');
        }

        private static void AppendCornerDamageIntArray(
            StringBuilder builder,
            int[] values)
        {
            builder.Append('{');
            if (values != null)
            {
                for (int index = 0; index < values.Length; index++)
                {
                    if (index > 0)
                    {
                        builder.Append('/');
                    }
                    builder.Append(values[index]);
                }
            }
            builder.Append('}');
        }
#endif

        private static string BuildCornerDamageTransactionAuditReport(
            MassRecipe recipe,
            CornerDamageTransactionAuditResult audit)
        {
            StringBuilder builder = new StringBuilder(16384);
            builder.AppendLine("GeneratedMass EW-C1A.1 corner transaction audit");
            builder.AppendLine("contract=EW-C1A.1-transaction");
            builder.Append("shapeSeed=");
            builder.AppendLine((recipe == null ? 0 : recipe.ShapeSeed)
                .ToString(CultureInfo.InvariantCulture));
            if (audit == null)
            {
                builder.AppendLine("status=failed");
                builder.AppendLine("diagnostic=audit capture was unavailable");
                return builder.ToString();
            }

            builder.Append("status=");
            builder.AppendLine(audit.Succeeded ? "passed" : "failed");
            builder.Append("attempted=");
            builder.AppendLine(audit.Attempted ? "1" : "0");
            builder.Append("graphAvailable=");
            builder.AppendLine(audit.GraphAvailable ? "1" : "0");
            builder.Append("candidateFound=");
            builder.AppendLine(audit.CandidateFound ? "1" : "0");
            builder.Append("transactionCertified=");
            builder.AppendLine(audit.Succeeded ? "1" : "0");
            builder.Append("diagnostic=");
            builder.AppendLine(audit.Diagnostic ?? string.Empty);
            builder.Append("normalizedTopology=");
            builder.Append(audit.NormalizedVertexCount);
            builder.Append('/');
            builder.Append(audit.NormalizedEdgeCount);
            builder.Append('/');
            builder.AppendLine(audit.NormalizedFaceCount.ToString(
                CultureInfo.InvariantCulture));
            builder.Append("eligibleCandidates=");
            builder.AppendLine(audit.EligibleCandidateCount.ToString(
                CultureInfo.InvariantCulture));
            builder.Append("selectedCandidateRank=");
            builder.AppendLine(audit.SelectedCandidateRank.ToString(
                CultureInfo.InvariantCulture));
            builder.Append("selectedGraphVertex=");
            builder.AppendLine(audit.SelectedGraphVertexIndex.ToString(
                CultureInfo.InvariantCulture));
            builder.Append("selectedPosition=");
            builder.AppendLine(FormatCornerDamageVector(
                audit.SelectedPosition));
            builder.Append("outwardNormal=");
            builder.AppendLine(FormatCornerDamageVector(
                audit.OutwardNormal));
            builder.Append("baseDepth=");
            builder.AppendLine(audit.BaseDepth.ToString(
                "G12",
                CultureInfo.InvariantCulture));
            builder.Append("acceptedTrial=");
            builder.AppendLine(audit.AcceptedTrialIndex.ToString(
                CultureInfo.InvariantCulture));
            builder.Append("thresholds=minimumStableEdge:");
            builder.Append(audit.MinimumStableEdgeLength.ToString(
                "G12",
                CultureInfo.InvariantCulture));
            builder.Append(",minimumStableFaceArea:");
            builder.Append(audit.MinimumStableFaceArea.ToString(
                "G12",
                CultureInfo.InvariantCulture));
            builder.Append(",maximumDimension:");
            builder.AppendLine(audit.MaximumDimension.ToString(
                "G12",
                CultureInfo.InvariantCulture));
            builder.Append("sourceVolume=");
            builder.AppendLine(audit.SourceVolume.ToString(
                "G12",
                CultureInfo.InvariantCulture));
            builder.Append("semanticCapFaces=");
            builder.AppendLine((audit.AcceptedCapFace == null ? 0 : 1)
                .ToString(CultureInfo.InvariantCulture));
            builder.Append("constructionSourceProvenance=");
            builder.Append(audit.ConstructionSourceFaceCountAttributed);
            builder.Append('/');
            builder.AppendLine(
                audit.ConstructionSourceFaceCountExpected.ToString(
                    CultureInfo.InvariantCulture));

            builder.AppendLine();
            builder.AppendLine("[Candidates]");
            builder.Append("count=");
            builder.AppendLine(audit.Candidates.Count.ToString(
                CultureInfo.InvariantCulture));
            for (int candidateIndex = 0;
                 candidateIndex < audit.Candidates.Count;
                 candidateIndex++)
            {
                CornerDamageCandidateRecord candidate =
                    audit.Candidates[candidateIndex];
                builder.Append("vertex=");
                builder.Append(candidate.GraphVertexIndex);
                builder.Append(",eligible=");
                builder.Append(candidate.Eligible ? 1 : 0);
                builder.Append(",position=");
                builder.Append(FormatCornerDamageVector(
                    candidate.Position));
                builder.Append(",faces=");
                AppendCornerDamageIntList(
                    builder,
                    candidate.IncidentFaceIndices);
                builder.Append(",graphEdges=");
                AppendCornerDamageIntList(
                    builder,
                    candidate.IncidentGraphEdgeIndices);
                builder.Append(",originalEdges=");
                AppendCornerDamageIntList(
                    builder,
                    candidate.IncidentOriginalEdgeIndices);
                builder.Append(",convex=");
                builder.Append(candidate.ConvexIncidentEdgeCount);
                builder.Append(",maxDihedral=");
                builder.Append(candidate.MaximumIncidentDihedral.ToString(
                    "G9",
                    CultureInfo.InvariantCulture));
                builder.Append(",minEdgeLength=");
                builder.Append(candidate.MinimumIncidentEdgeLength.ToString(
                    "G9",
                    CultureInfo.InvariantCulture));
                builder.Append(",score=");
                builder.Append(candidate.Score.ToString(
                    "G9",
                    CultureInfo.InvariantCulture));
                builder.Append("{sharp:");
                builder.Append(candidate.SharpnessScore.ToString(
                    "G9",
                    CultureInfo.InvariantCulture));
                builder.Append(",size:");
                builder.Append(candidate.SizeScore.ToString(
                    "G9",
                    CultureInfo.InvariantCulture));
                builder.Append(",up:");
                builder.Append(candidate.UpwardExposureScore.ToString(
                    "G9",
                    CultureInfo.InvariantCulture));
                builder.Append(",random:");
                builder.Append(candidate.RandomScore.ToString(
                    "G9",
                    CultureInfo.InvariantCulture));
                builder.Append('}');
                builder.Append(",blocker=");
                builder.AppendLine(candidate.Blocker ?? string.Empty);
            }

            builder.AppendLine();
            builder.AppendLine("[Depth Trials]");
            builder.Append("count=");
            builder.AppendLine(audit.Trials.Count.ToString(
                CultureInfo.InvariantCulture));
            for (int trialIndex = 0;
                 trialIndex < audit.Trials.Count;
                 trialIndex++)
            {
                CornerDamageTrialRecord trial = audit.Trials[trialIndex];
                builder.Append("trial=");
                builder.Append(trial.TrialIndex);
                builder.Append(",factor=");
                builder.Append(trial.DepthFactor.ToString(
                    "G9",
                    CultureInfo.InvariantCulture));
                builder.Append(",depth=");
                builder.Append(trial.Depth.ToString(
                    "G12",
                    CultureInfo.InvariantCulture));
                builder.Append(",planePoint=");
                builder.Append(FormatCornerDamageVector(
                    trial.PlanePoint));
                builder.Append(",planeDistance=");
                builder.Append(trial.PlaneDistance.ToString(
                    "G12",
                    CultureInfo.InvariantCulture));
                builder.Append(",succeeded=");
                builder.Append(trial.Succeeded ? 1 : 0);
                builder.Append(",faces=");
                builder.Append(trial.FaceCount);
                builder.Append(",cap=");
                builder.Append(trial.CapFaceCount);
                builder.Append('/');
                builder.Append(trial.CapVertexCount);
                builder.Append('/');
                builder.Append(trial.CapArea.ToString(
                    "G12",
                    CultureInfo.InvariantCulture));
                builder.Append(",capResidual=");
                builder.Append(trial.MaximumCapPlaneResidual.ToString(
                    "G12",
                    CultureInfo.InvariantCulture));
                builder.Append(",topology=");
                builder.Append(trial.OpenEdgeCount);
                builder.Append('/');
                builder.Append(trial.NonManifoldEdgeCount);
                builder.Append('/');
                builder.Append(trial.TJunctionCount);
                builder.Append(",faceQuality=");
                builder.Append(trial.InvalidFaceCount);
                builder.Append('/');
                builder.Append(trial.NonPlanarFaceCount);
                builder.Append('/');
                builder.Append(trial.NonConvexFaceCount);
                builder.Append('/');
                builder.Append(trial.WindingFailureCount);
                builder.Append(",boundsValid=");
                builder.Append(trial.BoundsValid);
                builder.Append(",budget=");
                builder.Append(trial.OutputVertexCount);
                builder.Append('/');
                builder.Append(trial.OutputTriangleCount);
                builder.Append('/');
                builder.Append(trial.BudgetValid);
                builder.Append(",volume=");
                builder.Append(trial.SourceVolume.ToString(
                    "G12",
                    CultureInfo.InvariantCulture));
                builder.Append('/');
                builder.Append(trial.ResultVolume.ToString(
                    "G12",
                    CultureInfo.InvariantCulture));
                builder.Append('/');
                builder.Append(trial.VolumeLoss.ToString(
                    "G12",
                    CultureInfo.InvariantCulture));
                builder.Append('/');
                builder.Append(trial.VolumeLossFraction.ToString(
                    "G12",
                    CultureInfo.InvariantCulture));
                builder.Append(",identity=");
                builder.Append(trial.UntouchedOriginalEdgeCount);
                builder.Append('/');
                builder.Append(trial.ShortenedDescendantEdgeCount);
                builder.Append('/');
                builder.Append(trial.CapRingEdgeCount);
                builder.Append('/');
                builder.Append(trial.MissingOriginalEdgeCount);
                builder.Append('/');
                builder.Append(trial.AmbiguousIdentityCount);
                builder.Append(",generatedIdentityCollisions=");
                builder.Append(trial.GeneratedIdentityCollisionCount);
                builder.Append(",exactFailures=");
                builder.Append(trial.ExactConstructionFailureCount);
                builder.Append(",exactReason=");
                builder.Append(trial.ExactConstructionFailure ??
                    string.Empty);
                builder.Append(",blocker=");
                builder.AppendLine(trial.Blocker ?? string.Empty);

                for (int identityIndex = 0;
                     identityIndex < trial.IdentityRecords.Count;
                     identityIndex++)
                {
                    CornerDamageEdgeIdentityRecord identity =
                        trial.IdentityRecords[identityIndex];
                    builder.Append("  identity=");
                    builder.Append(identity.Kind);
                    builder.Append(",outputGraphEdge=");
                    builder.Append(identity.OutputGraphEdgeIndex);
                    builder.Append(",parents=");
                    builder.Append(identity.ParentOriginalEdgeA);
                    builder.Append('/');
                    builder.Append(identity.ParentOriginalEdgeB);
                    builder.Append(",generated=");
                    builder.Append(identity.GeneratedIdentity);
                    builder.Append(",segment=");
                    builder.Append(FormatCornerDamageVector(identity.Start));
                    builder.Append("->");
                    builder.AppendLine(FormatCornerDamageVector(identity.End));
                }
            }
            return builder.ToString();
        }

        private static string FormatCornerDamageVector(Vector3 value)
        {
            return "(" +
                value.x.ToString("G9", CultureInfo.InvariantCulture) +
                "/" +
                value.y.ToString("G9", CultureInfo.InvariantCulture) +
                "/" +
                value.z.ToString("G9", CultureInfo.InvariantCulture) +
                ")";
        }

        private static void AppendCornerDamageIntList(
            StringBuilder builder,
            List<int> values)
        {
            builder.Append('{');
            if (values != null)
            {
                for (int index = 0; index < values.Count; index++)
                {
                    if (index > 0)
                    {
                        builder.Append('/');
                    }
                    builder.Append(values[index]);
                }
            }
            builder.Append('}');
        }

#if UNITY_EDITOR
        private sealed class EdgeWearBatchAuditCapture
        {
            public int ShapeSeed;
            public float EdgeWearWidth;
            public float EdgeWearMacroVariationCoverage;
            public float EdgeWearMacroVariation;
            public bool RequireAllGeometricCandidates;
            public bool AuditCaptured;
            public PlaneCutBevelAuditResult Audit;
            public bool CornerSolutionValid;
            public string CornerBlocker = string.Empty;
            public bool PlacementCaptured;
            public MassPlacementFrame PlacementFrame;
            public bool UsesImmutableSourcePlacementFrame;
            public bool PreviewApplied;
        }

        private static EdgeWearBatchAuditCapture
            activeEdgeWearBatchAuditCapture;

        private static bool TryBeginEdgeWearBatchAuditCapture(
            int shapeSeed,
            float edgeWearWidth,
            float edgeWearMacroVariationCoverage,
            float edgeWearMacroVariation,
            bool requireAllGeometricCandidates,
            out EdgeWearBatchAuditCaseResult immediateFailure)
        {
            immediateFailure = null;
            if (activeEdgeWearBatchAuditCapture != null)
            {
                immediateFailure = new EdgeWearBatchAuditCaseResult
                {
                    ShapeSeed = shapeSeed,
                    EdgeWearWidth = edgeWearWidth,
                    EdgeWearMacroVariationCoverage =
                        edgeWearMacroVariationCoverage,
                    EdgeWearMacroVariation = edgeWearMacroVariation,
                    RequireAllGeometricCandidates =
                        requireAllGeometricCandidates,
                    PrimaryFailure =
                        "another edge-wear batch evaluation is already active"
                };
                return false;
            }

            activeEdgeWearBatchAuditCapture =
                new EdgeWearBatchAuditCapture
                {
                    ShapeSeed = shapeSeed,
                    EdgeWearWidth = edgeWearWidth,
                    EdgeWearMacroVariationCoverage =
                        edgeWearMacroVariationCoverage,
                    EdgeWearMacroVariation = edgeWearMacroVariation,
                    RequireAllGeometricCandidates =
                        requireAllGeometricCandidates
                };
            return true;
        }

        private static EdgeWearBatchAuditCaseResult
            CompleteEdgeWearBatchAuditCapture(
                double totalMilliseconds,
                Exception evaluationException)
        {
            EdgeWearBatchAuditCapture capture =
                activeEdgeWearBatchAuditCapture;
            activeEdgeWearBatchAuditCapture = null;

            EdgeWearBatchAuditCaseResult result =
                new EdgeWearBatchAuditCaseResult
                {
                    ShapeSeed = capture == null ? 0 : capture.ShapeSeed,
                    EdgeWearWidth = capture == null
                        ? 0f
                        : capture.EdgeWearWidth,
                    EdgeWearMacroVariationCoverage = capture == null
                        ? 0f
                        : capture.EdgeWearMacroVariationCoverage,
                    EdgeWearMacroVariation = capture == null
                        ? 0f
                        : capture.EdgeWearMacroVariation,
                    RequireAllGeometricCandidates = capture != null &&
                        capture.RequireAllGeometricCandidates,
                    TotalMilliseconds = totalMilliseconds,
                    ObjectTransformChanged = 0
                };

            if (capture == null)
            {
                result.PrimaryFailure =
                    "edge-wear batch capture state was lost";
                return result;
            }

            result.AuditCaptured = capture.AuditCaptured;
            result.PlacementCaptured = capture.PlacementCaptured;
            result.CornerSolutionValid = capture.CornerSolutionValid;
            result.PreviewApplied = capture.PreviewApplied;
            result.PlacementFrameUsesImmutableSource =
                capture.UsesImmutableSourcePlacementFrame ? 1 : 0;
            result.PreviewDerivedPlacementParameters =
                capture.PreviewApplied &&
                !capture.UsesImmutableSourcePlacementFrame
                    ? 1
                    : 0;
            result.PreviewUsesCanonicalFrame =
                capture.PreviewApplied &&
                capture.UsesImmutableSourcePlacementFrame
                    ? 1
                    : 0;

            if (capture.AuditCaptured)
            {
                PopulateEdgeWearBatchAuditResult(
                    result,
                    capture.Audit);
            }
            if (capture.PlacementCaptured)
            {
                PopulateEdgeWearBatchPlacementFingerprints(
                    result,
                    capture.Audit,
                    capture.PlacementFrame);
            }

            result.Completed = evaluationException == null;
            if (evaluationException != null)
            {
                result.PrimaryFailure =
                    evaluationException.GetType().Name + ":" +
                    evaluationException.Message;
            }
            else if ((string.IsNullOrEmpty(result.PrimaryFailure) ||
                string.Equals(
                    result.PrimaryFailure,
                    "none",
                    StringComparison.Ordinal)) &&
                !string.IsNullOrEmpty(capture.CornerBlocker))
            {
                result.PrimaryFailure = capture.CornerBlocker;
            }
            if (string.IsNullOrEmpty(result.PrimaryFailure))
            {
                result.PrimaryFailure = "none";
            }
            return result;
        }

        private static void PopulateEdgeWearBatchAuditResult(
            EdgeWearBatchAuditCaseResult result,
            PlaneCutBevelAuditResult audit)
        {
            EdgeWearCoverageAudit coverage = audit.CoverageAudit;
            result.RawSourceEdgeCount = coverage == null
                ? 0
                : coverage.RawSourceEdgeCount;
            result.SourceEdgeCount = coverage == null
                ? 0
                : coverage.SourceEdgeCount;
            result.CoincidentBoundarySeamPairCount = coverage == null
                ? 0
                : coverage.CoincidentBoundarySeamPairCount;
            result.CoincidentGraphVertexReconciliationCount =
                coverage == null
                    ? 0
                    : coverage.CoincidentGraphVertexReconciliationCount;
            result.CoincidentGraphBoundarySeamPairCount =
                coverage == null
                    ? 0
                    : coverage.CoincidentGraphBoundarySeamPairCount;
            result.BaselineGeometricEligibleCount = coverage == null
                ? 0
                : coverage.BaselineGeometricEligibleCount;
            result.RecoveredGeometricEdgeCount = coverage == null
                ? 0
                : coverage.RecoveredGeometricEdgeCount;
            result.CollateralLostEdgeCount = coverage == null
                ? 0
                : coverage.CollateralLostEdgeCount;
            result.CollateralChangedEdgeCount = coverage == null
                ? 0
                : coverage.CollateralChangedEdgeCount;
            result.CollateralPreservationValid = coverage != null &&
                coverage.CollateralPreservationValid
                    ? 1
                    : 0;
            result.RecoveredGeometricEdgeIds = coverage == null
                ? "none"
                : FormatEdgeWearIndexList(
                    coverage.RecoveredGeometricEdgeIndices);
            result.CollateralLostEdgeIds = coverage == null
                ? "none"
                : FormatEdgeWearIndexList(
                    coverage.CollateralLostEdgeIndices);
            result.CollateralChangedEdgeIds = coverage == null
                ? "none"
                : FormatEdgeWearIndexList(
                    coverage.CollateralChangedEdgeIndices);
            result.StructuralEligibleCount = coverage == null
                ? 0
                : coverage.StructuralEligibleCount;
            result.GeometricEligibleCount = coverage == null
                ? 0
                : coverage.GeometricEligibleCount;
            result.CoexistenceEligibleCount = coverage == null
                ? 0
                : coverage.CoexistenceEligibleCount;
            result.CoexistenceIneligibleCount = coverage == null
                ? 0
                : coverage.CoexistenceIneligibleCount;
            result.ArtisticEligibleCount = coverage == null
                ? 0
                : coverage.ArtisticEligibleCount;
            PopulateEdgeWearMacroAuditResult(result, coverage);
            PopulateEdgeWearArtisticAuditResult(result, coverage);
            PopulateEdgeWearArtisticEdgeRecords(result, coverage);
            result.CandidateCount = coverage == null
                ? 0
                : coverage.CandidateCount;
            result.SelectedCount = coverage == null
                ? audit.SelectedEdgeCount
                : coverage.SelectedCount;
            result.CertifiedCount = audit.CertifiedPlanesBuilt;
            result.DeferredCount = audit.PlanesDeferred;
            result.RejectedCount = audit.PlanesRejected;
            result.TrialRejectedCount = audit.TrialRejectedPlanes;
            result.SolverPassCount = audit.EdgeConflictPassCount;
            result.WidthReductionCount =
                audit.EdgeConflictWidthReductionCount;
            result.MinimumWidthScale =
                audit.EdgeConflictMinimumWidthScale;
            result.CoexistenceTrialCount = audit.CoexistenceTrialCount;
            result.CoexistenceCacheUseCount =
                audit.CoexistenceTrialCacheUseCount;
            result.CoexistenceSearchStatesEvaluated =
                audit.CoexistenceSearchStatesEvaluated;
            result.CoexistenceSearchStatesDeduplicated =
                audit.CoexistenceSearchStatesDeduplicated;
            result.CoexistenceSearchMaximumDepth =
                audit.CoexistenceSearchMaximumDepth;
            result.CoexistenceSearchFrontierRemaining =
                audit.CoexistenceSearchFrontierRemaining;
            result.CoexistenceSearchWinningDepth =
                audit.CoexistenceSearchWinningDepth;
            result.CandidateConservationFailureCount =
                audit.CoexistenceCandidateConservationFailureCount;
            result.OpenEdgeCount = audit.OpenEdgeCount;
            result.NonManifoldEdgeCount = audit.NonManifoldEdgeCount;
            result.TJunctionCount = audit.TJunctionCount;
            result.InvalidFaceCount = audit.InvalidFaceCount;
            result.NonPlanarFaceCount =
                audit.FaceQualityNonPlanarCount;
            result.SurfaceRenderValid = audit.BevelRegionRenderValid;
            result.MeshValid = audit.PreviewGeometryValid;
            result.GeometryValid = audit.GeometryValid;
            result.CoverageValid =
                audit.MaterializedEdgeCoverageValid;
            result.StableFingerprintPrepared =
                audit.StableFingerprintPrepared;
            result.PreflightMilliseconds = coverage == null
                ? 0.0
                : coverage.ViabilityPreflightMilliseconds;
            result.LocalityEvaluationCount = coverage == null
                ? 0
                : coverage.ViabilityLocalityEvaluationCount;
            result.LocalityConstructionUseCount = coverage == null
                ? 0
                : coverage.ViabilityLocalityCacheUseCount;
            result.LocalityCacheMissCount = coverage == null
                ? 0
                : coverage.ViabilityLocalityCacheMissCount;
            result.LocalitySolverRecomputationCount = coverage == null
                ? 0
                : coverage.ViabilityLocalityRecomputationCount;
            result.ExclusionReasonHash =
                audit.ExclusionReasonFingerprint.ToString();
            result.SelectedEdgeHash =
                audit.SelectedEdgeFingerprint.ToString();
            result.CertifiedEdgeHash =
                audit.CertifiedEdgeFingerprint.ToString();
            result.GeometryTopologyHash =
                audit.GeometryTopologyFingerprint.ToString();
            result.CoexistenceSearchTrace =
                FormatPlaneCutCoexistenceSearchTrace(audit);
            result.PrimaryFailure =
                FormatPlaneCutPrimaryFailure(audit);
            if (string.IsNullOrEmpty(result.PrimaryFailure) ||
                string.Equals(
                    result.PrimaryFailure,
                    "none",
                    StringComparison.Ordinal))
            {
                result.PrimaryFailure = string.IsNullOrEmpty(
                        audit.Diagnostic)
                    ? "none"
                    : audit.Diagnostic;
            }
            if (result.CollateralPreservationValid != 1 ||
                result.CollateralLostEdgeCount != 0 ||
                result.CollateralChangedEdgeCount != 0)
            {
                string collateralFailure =
                    "collateral-preservation-failed:" +
                    "baseline/current/recovered/lost/changed=" +
                    result.BaselineGeometricEligibleCount + "/" +
                    result.GeometricEligibleCount + "/" +
                    result.RecoveredGeometricEdgeCount + "/" +
                    result.CollateralLostEdgeCount + "/" +
                    result.CollateralChangedEdgeCount +
                    ",lost={" + result.CollateralLostEdgeIds + "}" +
                    ",changed={" +
                        result.CollateralChangedEdgeIds + "}";
                result.PrimaryFailure = string.IsNullOrEmpty(
                        result.PrimaryFailure) ||
                    string.Equals(
                        result.PrimaryFailure,
                        "none",
                        StringComparison.Ordinal)
                            ? collateralFailure
                            : result.PrimaryFailure + "|" +
                                collateralFailure;
            }

            if (coverage == null || coverage.Records == null)
            {
                return;
            }
            for (int recordIndex = 0;
                 recordIndex < coverage.Records.Count;
                 recordIndex++)
            {
                EdgeWearEdgeLifecycleRecord record =
                    coverage.Records[recordIndex];
                if (record.ViabilityState ==
                    EdgeWearViabilityState.CoexistenceIneligible)
                {
                    switch (ResolveEdgeWearCoexistenceExclusionCategory(
                        record.FinalReason))
                    {
                        case 0:
                            result.SourceVertexStarExclusionCount++;
                            break;
                        case 1:
                            result.PlanePairExclusionCount++;
                            break;
                        case 2:
                            result.PlaneBandExclusionCount++;
                            break;
                        case 3:
                            result.GlobalWidthFloorExclusionCount++;
                            break;
                        case 4:
                            result.CandidateConservationExclusionCount++;
                            break;
                        case 5:
                            result.CornerWidthMissingExclusionCount++;
                            break;
                        case 6:
                            result.CornerWidthInactiveExclusionCount++;
                            break;
                        default:
                            result.OtherExclusionCount++;
                            break;
                    }
                    continue;
                }
                if (record.ViabilityState !=
                        EdgeWearViabilityState.StructuralIneligible &&
                    record.ViabilityState !=
                        EdgeWearViabilityState.GeometricIneligible)
                {
                    continue;
                }
                switch (ResolveEdgeWearViabilityExclusionCategory(
                    record.FinalReason))
                {
                    case 0:
                        result.BoundaryExclusionCount++;
                        break;
                    case 1:
                        result.DihedralExclusionCount++;
                        break;
                    case 2:
                        result.FootprintExclusionCount++;
                        break;
                    case 3:
                        result.LocalityExclusionCount++;
                        break;
                    case 4:
                        result.IsolatedRailExclusionCount++;
                        break;
                    case 5:
                        result.SupportExclusionCount++;
                        break;
                    case 6:
                        result.WidthFractionExclusionCount++;
                        break;
                    case 7:
                        result.EndpointSpanExclusionCount++;
                        break;
                    default:
                        result.OtherExclusionCount++;
                        break;
                }
            }
        }

        private static void PopulateEdgeWearBatchPlacementFingerprints(
            EdgeWearBatchAuditCaseResult result,
            PlaneCutBevelAuditResult audit,
            MassPlacementFrame placementFrame)
        {
            GeneratedGeometryStableFingerprint placementFingerprint =
                BuildEdgeWearPlacementFrameFingerprint(placementFrame);
            result.PlacementFrameHash = placementFingerprint.ToString();

            EdgeWearCoverageAudit coverage = audit.CoverageAudit;
            GeneratedGeometryStableHashBuilder evaluation =
                GeneratedGeometryStableHashBuilder.Create(
                    "PS3D.GeneratedMass.EdgeWear.Evaluation.v3");
            evaluation.AddInt32(coverage == null
                ? 0
                : coverage.SourceEdgeCount);
            evaluation.AddInt32(coverage == null
                ? 0
                : coverage.StructuralEligibleCount);
            evaluation.AddInt32(coverage == null
                ? 0
                : coverage.GeometricEligibleCount);
            evaluation.AddInt32(coverage == null
                ? 0
                : coverage.CoexistenceEligibleCount);
            evaluation.AddInt32(coverage == null
                ? 0
                : coverage.SelectedCount);
            evaluation.AddInt32(coverage == null
                ? 0
                : coverage.BuiltCount);
            evaluation.AddSingle(
                result.EdgeWearMacroVariationCoverage);
            evaluation.AddSingle(result.EdgeWearMacroVariation);
            evaluation.AddString(result.MacroSignature);
            evaluation.AddFingerprint(
                audit.ExclusionReasonFingerprint);
            evaluation.AddFingerprint(audit.SelectedEdgeFingerprint);
            evaluation.AddFingerprint(audit.CertifiedEdgeFingerprint);
            evaluation.AddFingerprint(audit.GeometryTopologyFingerprint);
            evaluation.AddFingerprint(placementFingerprint);
            result.EvaluationHash = evaluation.Finish().ToString();
        }

        private static GeneratedGeometryStableFingerprint
            BuildEdgeWearPlacementFrameFingerprint(
                MassPlacementFrame placementFrame)
        {
            GeneratedGeometryStableHashBuilder placement =
                GeneratedGeometryStableHashBuilder.Create(
                    "PS3D.GeneratedMass.EdgeWear.PlacementFrame.v1");
            placement.AddInt32(placementFrame.ReferenceVertexCount);
            placement.AddSingle(placementFrame.LeanMinimumY);
            placement.AddSingle(placementFrame.LeanHeight);
            placement.AddVector3(placementFrame.LeanDirection);
            placement.AddSingle(placementFrame.LeanDistance);
            placement.AddSingle(placementFrame.GroundingMinimumY);
            placement.AddSingle(placementFrame.GroundingHeight);
            placement.AddSingle(placementFrame.GroundingTop);
            placement.AddSingle(
                placementFrame.GroundingFlatteningStrength);
            placement.AddSingle(
                placementFrame.GroundingBroadeningStrength);
            placement.AddSingle(placementFrame.RecenterMinimumY);
            placement.AddSingle(placementFrame.ContactBand);
            placement.AddVector2(placementFrame.ContactCentre);
            placement.AddVector3(placementFrame.RecenterOffset);
            return placement.Finish();
        }
        private readonly struct EdgeWearMacroMappingProbe
        {
            public readonly float ParticipationIdentity01;
            public readonly bool Participates;
            public readonly float Identity01;
            public readonly float SampledMultiplier;
            public readonly float EffectiveMultiplier;
            public readonly float RequestedWidth;
            public readonly bool MinimumStyleClamped;

            public EdgeWearMacroMappingProbe(
                float participationIdentity01,
                bool participates,
                float identity01,
                float sampledMultiplier,
                float effectiveMultiplier,
                float requestedWidth,
                bool minimumStyleClamped)
            {
                ParticipationIdentity01 = participationIdentity01;
                Participates = participates;
                Identity01 = identity01;
                SampledMultiplier = sampledMultiplier;
                EffectiveMultiplier = effectiveMultiplier;
                RequestedWidth = requestedWidth;
                MinimumStyleClamped = minimumStyleClamped;
            }
        }

        private static EdgeWearMacroMappingProbe
            EvaluateEdgeWearMacroMappingProbe(
                int shapeSeed,
                int canonicalSourceEdgeIndex,
                float coverage,
                float controlStrength,
                float dihedralDegrees,
                bool generatedTransition)
        {
            ResolveEdgeWearMacroRequestedWidth(
                shapeSeed,
                canonicalSourceEdgeIndex,
                coverage,
                controlStrength,
                1f,
                EdgeWearMinimumStyleWidthSetting,
                dihedralDegrees,
                generatedTransition,
                out float participationIdentity01,
                out bool participates,
                out float identity01,
                out float sampledMultiplier,
                out float effectiveMultiplier,
                out float requestedWidth,
                out bool minimumStyleClamped);
            return new EdgeWearMacroMappingProbe(
                participationIdentity01,
                participates,
                identity01,
                sampledMultiplier,
                effectiveMultiplier,
                requestedWidth,
                minimumStyleClamped);
        }

        private static bool EdgeWearMacroMappingProbeEquals(
            EdgeWearMacroMappingProbe left,
            EdgeWearMacroMappingProbe right)
        {
            return left.ParticipationIdentity01 ==
                    right.ParticipationIdentity01 &&
                left.Participates == right.Participates &&
                left.Identity01 == right.Identity01 &&
                left.SampledMultiplier == right.SampledMultiplier &&
                left.EffectiveMultiplier == right.EffectiveMultiplier &&
                left.RequestedWidth == right.RequestedWidth &&
                left.MinimumStyleClamped == right.MinimumStyleClamped;
        }

        public static bool EvaluateEdgeWearMacroAngleMappingContract(
            out string report)
        {
            const int AngleSampleCount = 10001;
            int[] sourceEdgeIndices = { 0, 1, 7, 38, 97 };
            float[] controlStrengths = { 0f, 0.25f, 1f };
            bool bounded = true;
            bool endpointParity = true;
            bool permissionMonotonic = true;
            bool formulaParity = true;
            bool widthMonotonic = true;
            bool dependentFieldParity = true;
            bool deterministic = true;
            bool zeroParity = true;
            bool coverageParity = true;
            bool generatedTransitionParity = true;
            string firstFailure = "none";
            int evaluatedProbeCount = 0;
            float previousPermission = float.PositiveInfinity;
            endpointParity =
                Mathf.Abs(
                    ResolveEdgeWearMacroAnglePermission(
                        EdgeWearMacroShallowAngleDegrees) - 1f) <=
                    0.000001f &&
                Mathf.Abs(
                    ResolveEdgeWearMacroAnglePermission(
                        EdgeWearMacroSharpAngleDegrees) -
                    EdgeWearMacroSharpReductionPermission) <= 0.000001f &&
                Mathf.Abs(
                    ResolveEdgeWearMacroAnglePermission(0f) - 1f) <=
                    0.000001f &&
                Mathf.Abs(
                    ResolveEdgeWearMacroAnglePermission(180f) -
                    EdgeWearMacroSharpReductionPermission) <= 0.000001f;
            if (!endpointParity)
            {
                firstFailure = "angle-endpoint-parity";
            }

            for (int angleIndex = 0;
                 angleIndex < AngleSampleCount;
                 angleIndex++)
            {
                float angle = 180f * angleIndex /
                    (AngleSampleCount - 1f);
                float permission =
                    ResolveEdgeWearMacroAnglePermission(angle);
                if (permission <
                        EdgeWearMacroSharpReductionPermission - 0.000001f ||
                    permission > 1.000001f)
                {
                    bounded = false;
                    if (firstFailure == "none")
                    {
                        firstFailure = "angle-permission-out-of-bounds@" +
                            angle.ToString("R", CultureInfo.InvariantCulture);
                    }
                }
                if (permission > previousPermission + 0.000001f)
                {
                    permissionMonotonic = false;
                    if (firstFailure == "none")
                    {
                        firstFailure = "angle-permission-increased@" +
                            angle.ToString("R", CultureInfo.InvariantCulture);
                    }
                }
                previousPermission = permission;

                for (int edgeIndex = 0;
                     edgeIndex < sourceEdgeIndices.Length;
                     edgeIndex++)
                {
                    for (int strengthIndex = 0;
                         strengthIndex < controlStrengths.Length;
                         strengthIndex++)
                    {
                        int sourceEdgeIndex = sourceEdgeIndices[edgeIndex];
                        float controlStrength =
                            controlStrengths[strengthIndex];
                        EdgeWearMacroMappingProbe current =
                            EvaluateEdgeWearMacroMappingProbe(
                                7319,
                                sourceEdgeIndex,
                                1f,
                                controlStrength,
                                angle,
                                false);
                        EdgeWearMacroMappingProbe repeated =
                            EvaluateEdgeWearMacroMappingProbe(
                                7319,
                                sourceEdgeIndex,
                                1f,
                                controlStrength,
                                angle,
                                false);
                        evaluatedProbeCount += 2;
                        if (!EdgeWearMacroMappingProbeEquals(
                                current,
                                repeated))
                        {
                            deterministic = false;
                            if (firstFailure == "none")
                            {
                                firstFailure = "nondeterministic@edge:" +
                                    sourceEdgeIndex + ",angle:" +
                                    angle.ToString(
                                        "R",
                                        CultureInfo.InvariantCulture);
                            }
                        }
                        float expectedStrength =
                            Mathf.Clamp01(controlStrength) *
                            EdgeWearMacroMaximumCertifiedStrength;
                        float expectedMultiplier = current.Participates
                            ? 1f -
                                (1f - current.SampledMultiplier) *
                                expectedStrength *
                                permission
                            : 1f;
                        if (Mathf.Abs(
                                current.EffectiveMultiplier -
                                expectedMultiplier) > 0.000001f ||
                            Mathf.Abs(
                                current.RequestedWidth -
                                Mathf.Max(
                                    EdgeWearMinimumStyleWidthSetting,
                                    expectedMultiplier)) > 0.000001f)
                        {
                            formulaParity = false;
                            if (firstFailure == "none")
                            {
                                firstFailure = "resolver-formula-parity@edge:" +
                                    sourceEdgeIndex + ",angle:" +
                                    angle.ToString(
                                        "R",
                                        CultureInfo.InvariantCulture);
                            }
                        }
                        if (current.EffectiveMultiplier <
                                EdgeWearMacroMinimumSampledMultiplier -
                                    0.000001f ||
                            current.EffectiveMultiplier > 1.000001f ||
                            current.RequestedWidth <
                                EdgeWearMinimumStyleWidthSetting - 0.000001f ||
                            current.RequestedWidth > 1.000001f)
                        {
                            bounded = false;
                            if (firstFailure == "none")
                            {
                                firstFailure = "resolved-output-out-of-bounds";
                            }
                        }
                        if (controlStrength <= 0f &&
                            (current.Participates ||
                             current.EffectiveMultiplier != 1f ||
                             current.RequestedWidth != 1f ||
                             current.MinimumStyleClamped))
                        {
                            zeroParity = false;
                            if (firstFailure == "none")
                            {
                                firstFailure = "zero-strength-parity";
                            }
                        }

                        if (angleIndex > 0)
                        {
                            float previousAngle = 180f *
                                (angleIndex - 1) /
                                (AngleSampleCount - 1f);
                            EdgeWearMacroMappingProbe previous =
                                EvaluateEdgeWearMacroMappingProbe(
                                    7319,
                                    sourceEdgeIndex,
                                    1f,
                                    controlStrength,
                                    previousAngle,
                                    false);
                            evaluatedProbeCount++;
                            if (current.SampledMultiplier !=
                                    previous.SampledMultiplier ||
                                current.EffectiveMultiplier + 0.000001f <
                                    previous.EffectiveMultiplier ||
                                current.RequestedWidth + 0.000001f <
                                    previous.RequestedWidth)
                            {
                                widthMonotonic = false;
                                if (firstFailure == "none")
                                {
                                    firstFailure = "width-decreased@edge:" +
                                        sourceEdgeIndex + ",angle:" +
                                        angle.ToString(
                                            "R",
                                            CultureInfo.InvariantCulture);
                                }
                            }
                        }
                    }
                }
            }

            EdgeWearMacroMappingProbe coverageZero =
                EvaluateEdgeWearMacroMappingProbe(
                    7319,
                    38,
                    0f,
                    1f,
                    90f,
                    false);
            evaluatedProbeCount++;
            coverageParity = !coverageZero.Participates &&
                coverageZero.EffectiveMultiplier == 1f &&
                coverageZero.RequestedWidth == 1f &&
                !coverageZero.MinimumStyleClamped;
            if (!coverageParity && firstFailure == "none")
            {
                firstFailure = "zero-coverage-parity";
            }

            EdgeWearMacroMappingProbe generatedTransition =
                EvaluateEdgeWearMacroMappingProbe(
                    7319,
                    38,
                    1f,
                    1f,
                    180f,
                    true);
            evaluatedProbeCount++;
            generatedTransitionParity =
                !generatedTransition.Participates &&
                generatedTransition.SampledMultiplier == 1f &&
                generatedTransition.EffectiveMultiplier == 1f &&
                generatedTransition.RequestedWidth == 1f &&
                !generatedTransition.MinimumStyleClamped;
            if (!generatedTransitionParity && firstFailure == "none")
            {
                firstFailure = "generated-transition-parity";
            }

            EdgeWearEdgeViabilityRecord dependentFields =
                new EdgeWearEdgeViabilityRecord();
            ApplyResolvedEdgeWearMacroWidth(
                dependentFields,
                3f,
                0.01f,
                generatedTransition.ParticipationIdentity01,
                generatedTransition.Participates,
                generatedTransition.Identity01,
                generatedTransition.SampledMultiplier,
                generatedTransition.EffectiveMultiplier,
                generatedTransition.RequestedWidth,
                generatedTransition.MinimumStyleClamped);
            dependentFieldParity =
                dependentFields.RequestedWidth == 1f &&
                Mathf.Abs(
                    dependentFields.RequiredFootprintLength - 2.01f) <=
                    0.000001f &&
                Mathf.Abs(dependentFields.LengthToWidthRatio - 3f) <=
                    0.000001f;
            if (!dependentFieldParity && firstFailure == "none")
            {
                firstFailure = "dependent-width-field-parity";
            }

            bool passed = bounded &&
                endpointParity &&
                permissionMonotonic &&
                formulaParity &&
                widthMonotonic &&
                dependentFieldParity &&
                deterministic &&
                zeroParity &&
                coverageParity &&
                generatedTransitionParity;
            float sharpMinimumMultiplier = 1f -
                (1f - EdgeWearMacroMinimumSampledMultiplier) *
                EdgeWearMacroMaximumCertifiedStrength *
                EdgeWearMacroSharpReductionPermission;
            StringBuilder builder = new StringBuilder(768);
            builder.Append("status=");
            builder.AppendLine(passed ? "passed" : "failed");
            builder.Append("policy=actual-runtime-resolver/dense-angle-monotonicity");
            builder.Append(",angleSamples=");
            builder.Append(AngleSampleCount);
            builder.Append(",probeEvaluations=");
            builder.AppendLine(evaluatedProbeCount.ToString());
            builder.Append("mapping=shallow:");
            builder.Append(
                EdgeWearMacroShallowAngleDegrees.ToString(
                    "G9",
                    CultureInfo.InvariantCulture));
            builder.Append(",sharp:");
            builder.Append(
                EdgeWearMacroSharpAngleDegrees.ToString(
                    "G9",
                    CultureInfo.InvariantCulture));
            builder.Append(",sharpPermission:");
            builder.AppendLine(
                EdgeWearMacroSharpReductionPermission.ToString(
                    "G9",
                    CultureInfo.InvariantCulture));
            builder.Append("sharpMinimumMultiplier=");
            builder.AppendLine(sharpMinimumMultiplier.ToString(
                "G9",
                CultureInfo.InvariantCulture));
            builder.Append("checks=bounded:");
            builder.Append(bounded ? '1' : '0');
            builder.Append(",endpointParity:");
            builder.Append(endpointParity ? '1' : '0');
            builder.Append(",permissionMonotonic:");
            builder.Append(permissionMonotonic ? '1' : '0');
            builder.Append(",formulaParity:");
            builder.Append(formulaParity ? '1' : '0');
            builder.Append(",widthMonotonic:");
            builder.Append(widthMonotonic ? '1' : '0');
            builder.Append(",dependentFieldParity:");
            builder.Append(dependentFieldParity ? '1' : '0');
            builder.Append(",deterministic:");
            builder.Append(deterministic ? '1' : '0');
            builder.Append(",zeroParity:");
            builder.Append(zeroParity ? '1' : '0');
            builder.Append(",coverageParity:");
            builder.Append(coverageParity ? '1' : '0');
            builder.Append(",generatedTransitionParity:");
            builder.AppendLine(generatedTransitionParity ? "1" : "0");
            builder.Append("firstFailure=");
            builder.Append(firstFailure);
            report = builder.ToString();
            return passed;
        }

#endif

        private static void CapturePendingEdgeWearStableFingerprint(
            PlaneCutBevelAuditResult audit)
        {
            EdgeWearCoverageAudit coverage = audit.CoverageAudit;
            pendingEdgeWearStableFingerprint =
                new PendingEdgeWearStableFingerprint
                {
                    Valid = audit.StableFingerprintPrepared == 1,
                    SourceEdgeCount = coverage == null
                        ? 0
                        : coverage.SourceEdgeCount,
                    StructuralEligibleCount = coverage == null
                        ? 0
                        : coverage.StructuralEligibleCount,
                    GeometricEligibleCount = coverage == null
                        ? 0
                        : coverage.GeometricEligibleCount,
                    CoexistenceEligibleCount = coverage == null
                        ? 0
                        : coverage.CoexistenceEligibleCount,
                    SelectedCount = coverage == null
                        ? 0
                        : coverage.SelectedCount,
                    CertifiedCount = coverage == null
                        ? 0
                        : coverage.BuiltCount,
                    ExclusionReasons = audit.ExclusionReasonFingerprint,
                    SelectedEdges = audit.SelectedEdgeFingerprint,
                    CertifiedEdges = audit.CertifiedEdgeFingerprint,
                    GeometryTopology = audit.GeometryTopologyFingerprint
                };
        }

        private static int BuildChamferDiagnosticGeometrySignature(
            List<ChamferProvisionalFaceRecord> records)
        {
            unchecked
            {
                int hash = 17;
                if (records == null)
                {
                    return hash;
                }
                hash = hash * 31 + records.Count;
                for (int recordIndex = 0;
                     recordIndex < records.Count;
                     recordIndex++)
                {
                    ChamferProvisionalFaceRecord record = records[recordIndex];
                    hash = hash * 31 + (int)record.Kind;
                    PolygonFace face = record.Face;
                    if (face == null || face.Vertices == null)
                    {
                        continue;
                    }
                    hash = hash * 31 + face.Vertices.Count;
                    for (int vertexIndex = 0;
                         vertexIndex < face.Vertices.Count;
                         vertexIndex++)
                    {
                        hash = hash * 31 +
                            new VertexKey(
                                face.Vertices[vertexIndex]).GetHashCode();
                    }
                }
                return hash;
            }
        }

        private static void AppendChamferCompactDiagnostic(
            ref string target,
            string value,
            int maximumEntries)
        {
            if (string.IsNullOrEmpty(value) || maximumEntries <= 0)
            {
                return;
            }
            int existingEntries = string.IsNullOrEmpty(target)
                ? 0
                : target.Split(';').Length;
            if (existingEntries >= maximumEntries)
            {
                return;
            }
            target = string.IsNullOrEmpty(target)
                ? value
                : target + ";" + value;
        }

        private static void LogChamferNoStackTrace(
            string message,
            bool warning)
        {
#if UNITY_EDITOR
            if (activeEdgeWearBatchAuditCapture != null)
            {
                return;
            }
            Debug.LogFormat(
                warning ? LogType.Warning : LogType.Log,
                LogOption.NoStacktrace,
                null,
                "{0}",
                message);
#endif
        }

        private static string GetChamferGenerationCaller()
        {
#if UNITY_EDITOR
            System.Diagnostics.StackTrace stack =
                new System.Diagnostics.StackTrace(1, false);
            System.Diagnostics.StackFrame[] frames = stack.GetFrames();
            if (frames == null)
            {
                return string.Empty;
            }
            for (int i = 0; i < frames.Length; i++)
            {
                System.Reflection.MethodBase method =
                    frames[i].GetMethod();
                if (method == null || method.DeclaringType == null ||
                    method.DeclaringType.Name != "GeneratedMass")
                {
                    continue;
                }
                if (method.Name == "OnValidate" ||
                    method.Name == "OnEnable")
                {
                    return method.Name;
                }
            }
#endif
            return string.Empty;
        }

        private static bool ShouldSuppressChamferCompactSummary(
            int geometrySignature,
            string message)
        {
            if (geometrySignature == 0)
            {
                return false;
            }
            string origin = GetChamferGenerationCaller();
            long now = DateTime.UtcNow.Ticks;
            if (!string.IsNullOrEmpty(origin) &&
                LastChamferCompactSummaryByGeometry.TryGetValue(
                    geometrySignature,
                    out string previous) &&
                previous == message &&
                LastChamferCompactSummaryTicksByGeometry.TryGetValue(
                    geometrySignature,
                    out long previousTicks) &&
                LastChamferCompactSummaryOriginByGeometry.TryGetValue(
                    geometrySignature,
                    out string previousOrigin) &&
                !string.IsNullOrEmpty(previousOrigin) &&
                previousOrigin != origin &&
                ((previousOrigin == "OnValidate" && origin == "OnEnable") ||
                 (previousOrigin == "OnEnable" && origin == "OnValidate")) &&
                now - previousTicks <= TimeSpan.TicksPerSecond * 2)
            {
                return true;
            }
            if (LastChamferCompactSummaryByGeometry.Count >= 512)
            {
                LastChamferCompactSummaryByGeometry.Clear();
                LastChamferCompactSummaryTicksByGeometry.Clear();
                LastChamferCompactSummaryOriginByGeometry.Clear();
            }
            LastChamferCompactSummaryByGeometry[geometrySignature] = message;
            LastChamferCompactSummaryTicksByGeometry[geometrySignature] = now;
            LastChamferCompactSummaryOriginByGeometry[geometrySignature] =
                origin;
            return false;
        }

        private static string FormatPlaneCutBandAudit(
            PlaneCutBevelAuditResult audit)
        {
            return "retained:" + audit.BandRetainedEdgeCount +
                ",single:" + audit.BandSingleFaceCount +
                ",split:" + audit.BandSplitCount +
                ",interrupted:" + audit.BandInterruptedCount +
                ",foreignCut:" + audit.BandForeignCutCount +
                ",overlongJunction:" +
                    audit.BandOverlongJunctionCount +
                ",collapsed:" + audit.BandCollapsedCount +
                ",minCoverage:" +
                    audit.BandMinimumCoverageRatio.ToString("G6") +
                ",maxJunctionInfluence:" +
                    audit.BandMaximumJunctionInfluenceRatio
                        .ToString("G6") +
                ",maxSharedAxisSpan:" +
                    audit.BandMaximumSharedAxisSpanRatio
                        .ToString("G6");
        }

        private static string FormatPlaneCutEdgeConflictAudit(
            PlaneCutBevelAuditResult audit)
        {
            bool evaluated = audit.EdgeConflictPassCount > 0;
            bool widthReduction = audit.CoverageAudit != null &&
                audit.CoverageAudit.MaximumCoverageMode;
            return "mode:" +
                    (widthReduction
                        ? "clusterWidthReduction"
                        : "candidateDeferral") +
                ",passes:" + audit.EdgeConflictPassCount +
                ",clusters:" + audit.EdgeConflictClusterCount +
                ",reductions:" +
                    audit.EdgeConflictWidthReductionCount +
                ",minimumWidthScale:" +
                    audit.EdgeConflictMinimumWidthScale.ToString("G6") +
                ",unresolved:" + audit.EdgeConflictUnresolvedCount +
                ",deferred:" + audit.EdgeConflictEdgesDeferredCount +
                ",resolved:" + audit.EdgeConflictResolvedCount +
                ",topologyRejected:" +
                    audit.EdgeConflictTopologyRejectedPassCount +
                ",topologyExpanded:" +
                    audit.EdgeConflictTopologyExpandedClusterCount +
                ",topologyRollbacks:" +
                    audit.EdgeConflictTopologyRollbackCount +
                ",budgetExhausted:" +
                    audit.EdgeConflictBudgetExhausted +
                ",victim:" +
                    (evaluated
                        ? audit.EdgeConflictVictimEdgeIndex
                        : -1) +
                ",foreign:" +
                    (evaluated
                        ? audit.EdgeConflictForeignEdgeIndex
                        : -1) +
                ",vertex:" +
                    (evaluated
                        ? audit.EdgeConflictVertexIndex
                        : -1) +
                ",deferredEdge:" +
                    (evaluated
                        ? audit.EdgeConflictDeferredEdgeIndex
                        : -1) +
                ",victimCoverage:" +
                    audit.EdgeConflictVictimCoverageRatio
                        .ToString("G6") +
                ",foreignAxial:" +
                    audit.EdgeConflictForeignAxialParameter
                        .ToString("G6") +
                ",foreignSpan:" +
                    audit.EdgeConflictForeignSharedSpanRatio
                        .ToString("G6");
        }

        private static string FormatPlaneCutTopologyScaleSearchAudit(
            PlaneCutBevelAuditResult audit)
        {
            return "mode:" +
                    (string.IsNullOrEmpty(
                            audit.TopologyScaleSearchMode)
                        ? "none"
                        : audit.TopologyScaleSearchMode) +
                ",trigger:{" +
                    (string.IsNullOrEmpty(
                            audit.TopologyScaleSearchTriggerEvidence)
                        ? "none"
                        : audit.TopologyScaleSearchTriggerEvidence) + "}" +
                ",topologyLinked:{" +
                    (string.IsNullOrEmpty(
                            audit.TopologyScaleSearchTopologyLinkedEvidence)
                        ? "none"
                        : audit.TopologyScaleSearchTopologyLinkedEvidence) +
                    "}" +
                ",baseState:" +
                    (audit.TopologyScaleSearchBasePass >= 0
                        ? "topologyClean:" +
                            audit.TopologyScaleSearchBasePass.ToString()
                        : "none") +
                ",retreatEdges:{" +
                    (string.IsNullOrEmpty(
                            audit.TopologyScaleSearchClusterEvidence)
                        ? "none"
                        : audit.TopologyScaleSearchClusterEvidence) + "}" +
                ",protectedEdges:{" +
                    (string.IsNullOrEmpty(
                            audit.TopologyScaleSearchProtectedEvidence)
                        ? "none"
                        : audit.TopologyScaleSearchProtectedEvidence) + "}" +
                ",activeSearchFailure:{stage:" +
                    (string.IsNullOrEmpty(
                            audit.ActiveSearchFailureStage)
                        ? "none"
                        : audit.ActiveSearchFailureStage) +
                    ",cause:" +
                    (string.IsNullOrEmpty(
                            audit.ActiveSearchFailureCause)
                        ? "none"
                        : audit.ActiveSearchFailureCause) +
                    ",evidence:{" +
                    (string.IsNullOrEmpty(
                            audit.ActiveSearchFailureEvidence)
                        ? "none"
                        : audit.ActiveSearchFailureEvidence) + "}}" +
                ",trials:" + audit.TopologyScaleSearchTrialCount +
                ",committedFactor:" +
                    (audit.TopologyScaleSearchCommittedFactor >= 0f
                        ? audit.TopologyScaleSearchCommittedFactor
                            .ToString("G6")
                        : "none") +
                ",highestValidFactor:" +
                    (audit.TopologyScaleSearchHighestValidFactor >= 0f
                        ? audit.TopologyScaleSearchHighestValidFactor
                            .ToString("G6")
                        : "none") +
                ",bandFailures:" +
                    audit.TopologyScaleSearchBandFailureCount +
                ",topologyFailures:" +
                    audit.TopologyScaleSearchTopologyFailureCount +
                ",faceQualityFailures:" +
                    audit.TopologyScaleSearchFaceQualityFailureCount +
                ",collateralFailures:" +
                    audit.TopologyScaleSearchCollateralFailureCount +
                ",collateralChanged:{" +
                    (string.IsNullOrEmpty(
                            audit.TopologyScaleSearchCollateralChangedEvidence)
                        ? "none"
                        : audit.TopologyScaleSearchCollateralChangedEvidence) +
                    "}" +
                ",failedStateScalesReused:" +
                    audit.TopologyScaleSearchFailedStateScalesReused +
                ",fallbackState:" +
                    (audit.TopologyScaleSearchUnresolved == 1 &&
                     audit.TopologyScaleSearchBasePass >= 0
                        ? "topologyClean:" +
                            audit.TopologyScaleSearchBasePass.ToString()
                        : "none") +
                ",unresolved:" +
                    audit.TopologyScaleSearchUnresolved;
        }

        private static string FormatPlaneCutLocalJunctionAudit(
            PlaneCutBevelAuditResult audit)
        {
            return "candidates:" +
                    audit.LocalJunctionCandidateCount +
                ",extracted:" +
                    audit.LocalJunctionStarsExtractedCount +
                ",closed:" + audit.LocalJunctionClosedLoopCount +
                ",branched:" + audit.LocalJunctionBranchedCount +
                ",selfX:" +
                    audit.LocalJunctionSelfIntersectingCount +
                ",foreign:" +
                    audit.LocalJunctionForeignFaceCount +
                ",missing:" +
                    audit.LocalJunctionMissingIncidentBevelCount +
                ",duplicate:" +
                    audit.LocalJunctionDuplicateIncidentBevelCount +
                ",loopVertices:" +
                    audit.LocalJunctionMinimumLoopVertexCount + "-" +
                    audit.LocalJunctionMaximumLoopVertexCount +
                ",maxExtent:" +
                    audit.LocalJunctionMaximumExtentRatio
                        .ToString("G6");
        }

        private static string FormatPlaneCutBevelAuditFields(
            PlaneCutBevelAuditResult planeCutAudit)
        {
            return
                "planeBevel=" +
                    planeCutAudit.SelectedEdgeCount + "/" +
                    planeCutAudit.ActiveEdgeCount + "/" +
                    planeCutAudit.PlanesBuilt + "/" +
                    planeCutAudit.PlanesLocalized + "/" +
                    planeCutAudit.PlanesDeferred + "/" +
                    planeCutAudit.PlanesRejected + "/" +
                    planeCutAudit.CapsBuilt + "/" +
                    planeCutAudit.CapsMissing + "/" +
                    planeCutAudit.CapsRedundant + "/" +
                    planeCutAudit.ConformalSplitCount + "/" +
                    planeCutAudit.SeamPairCount + "/" +
                    planeCutAudit.OpenEdgeCount + "/" +
                    planeCutAudit.NonManifoldEdgeCount + "/" +
                    planeCutAudit.TJunctionCount + "/" +
                    planeCutAudit.InvalidFaceCount + "/" +
                    planeCutAudit.GeometryValid +
                ",augmentation=" +
                    "baselineCertified:" +
                        planeCutAudit.BaselineCertified +
                    ",baselineApplied:" +
                        planeCutAudit.BaselineApplied +
                    ",attempted:" +
                        planeCutAudit.AugmentationAttempted +
                    ",applied:" +
                        planeCutAudit.AugmentationApplied +
                    ",states:" +
                        planeCutAudit.AugmentationStatesEvaluated +
                    ",frontier:" +
                        planeCutAudit.AugmentationFrontierRemaining +
                    ",elapsedMs:" +
                        planeCutAudit.AugmentationElapsedMilliseconds
                            .ToString("G9", CultureInfo.InvariantCulture) +
                    ",timeBudgetExceeded:" +
                        planeCutAudit.AugmentationTimeBudgetExceeded +
                    ",cancelled:" +
                        planeCutAudit.AugmentationCancelled +
                    ",failure:{" +
                        (string.IsNullOrEmpty(
                                planeCutAudit.AugmentationFailure)
                            ? "none"
                            : planeCutAudit.AugmentationFailure) + "}" +
                    ",lastFailure:{" +
                        (string.IsNullOrEmpty(
                                planeCutAudit.AugmentationLastFailure)
                            ? "none"
                            : planeCutAudit.AugmentationLastFailure) + "}" +
                    ",implicated:{" +
                        (string.IsNullOrEmpty(
                                planeCutAudit
                                    .AugmentationImplicatedEdgeEvidence)
                            ? "none"
                            : planeCutAudit
                                .AugmentationImplicatedEdgeEvidence) + "}" +
                ",materialRecovery=" +
                    FormatMaterialWidthRecoveryAudit(
                        planeCutAudit.CoverageAudit) +
                ",planeTransaction=" +
                    "attempted:" +
                        planeCutAudit.AttemptedPlanesBuilt +
                    ",certified:" +
                        planeCutAudit.CertifiedPlanesBuilt +
                    ",trialRejected:" +
                        planeCutAudit.TrialRejectedPlanes +
                ",planeVertexJunction=" +
                    planeCutAudit.VertexJunctionCandidateCount + "/" +
                    planeCutAudit.VertexJunctionDirectBuiltCount + "/" +
                    planeCutAudit.VertexJunctionAdaptiveBuiltCount + "/" +
                    planeCutAudit.VertexJunctionBacktrackBuiltCount + "/" +
                    planeCutAudit.VertexJunctionCleanSharpCount + "/" +
                    planeCutAudit.VertexJunctionUnresolvedCount + "/" +
                    planeCutAudit.VertexJunctionTriangleCapCount + "/" +
                    planeCutAudit.VertexJunctionQuadCapCount + "/" +
                    planeCutAudit.VertexJunctionLargerCapCount + "/" +
                    planeCutAudit.VertexJunctionEdgesDeferredCount + "/" +
                    planeCutAudit.VertexJunctionRebuildPassCount +
                ",planeSolve=" +
                    planeCutAudit.SolveStatesEvaluated + "/" +
                    planeCutAudit.SolveJunctionsVisited + "/" +
                    planeCutAudit.SolveCandidateTrials + "/" +
                    planeCutAudit.SolveSystemRebuilds + "/" +
                    planeCutAudit.SolvePolygonAudits + "/" +
                    planeCutAudit.SolveTriangleAudits + "/" +
                    planeCutAudit.SolveEdgesDeferred + "/" +
                    planeCutAudit.SolveElapsedMilliseconds + "/" +
                    planeCutAudit.SolveTimedOut +
                ",planeFaceQuality=" +
                    planeCutAudit.FaceQualityFaceCount + "/" +
                    planeCutAudit.FaceQualitySeamTouchedFaceCount + "/" +
                    planeCutAudit.FaceQualityNonPlanarCount + "/" +
                    planeCutAudit.FaceQualityElongatedJunctionCount + "/" +
                    planeCutAudit.FaceQualityMaxPlaneDeviation
                        .ToString("G6") + "/" +
                    planeCutAudit.FaceQualityMaxNormalSpreadDegrees
                        .ToString("G6") + "/" +
                    planeCutAudit.FaceQualityMinimumJunctionCompactness
                        .ToString("G6") + "/" +
                    planeCutAudit.FaceQualityMaximumJunctionAspectRatio
                        .ToString("G6") + "/" +
                    planeCutAudit.FaceQualityWorstVertexCount +
                ",planeBand=" +
                    FormatPlaneCutBandAudit(planeCutAudit) +
                ",edgeConflict=" +
                    FormatPlaneCutEdgeConflictAudit(planeCutAudit) +
                ",topologyScaleSearch={" +
                    FormatPlaneCutTopologyScaleSearchAudit(
                        planeCutAudit) + "}" +
                ",localJunction=" +
                    FormatPlaneCutLocalJunctionAudit(planeCutAudit) +
                ",polygonSurface={" +
                    FormatPolygonSurfaceAudit(planeCutAudit) + "}" +
                ",planeSurface=" +
                    "faces:" + planeCutAudit.BevelRegionFaceCount +
                    FormatOneSurfaceTriangulationPolicy() +
                    ",boundaryVertices:" +
                        planeCutAudit.BevelRegionBoundaryVertexCount +
                    ",triangles:" +
                        planeCutAudit.BevelRegionTriangleCount +
                    ",authoredNormalTriangles:" +
                        planeCutAudit.BevelRegionAuthoredNormalTriangleCount +
                    ",authoredSurfaceGroupTriangles:" +
                        planeCutAudit.BevelRegionAuthoredSurfaceGroupTriangleCount +
                    ",internalFanVertices:" +
                        planeCutAudit.BevelRegionInternalFanVertexCount +
                    ",maxPlaneResidual:" +
                        planeCutAudit.BevelRegionMaximumPlaneResidual.ToString("G9") +
                    ",maxNormalDeviationDegrees:" +
                        planeCutAudit.BevelRegionMaximumNormalDeviationDegrees.ToString("G9") +
                    ",renderValid:" +
                        planeCutAudit.BevelRegionRenderValid +
                    ",materializedCoverage:" +
                        planeCutAudit.MaterializedEdgeCoverageValid +
                ",planeEdges=" +
                    "active:{" +
                        FormatCanonicalPlaneEdgeEvidence(
                            planeCutAudit.ActiveEdgeEvidence,
                            planeCutAudit.CoverageAudit) + "}" +
                    ",attempted:{" +
                        FormatCanonicalPlaneEdgeEvidence(
                            planeCutAudit.AttemptedEdgeEvidence,
                            planeCutAudit.CoverageAudit) + "}" +
                    ",certified:{" +
                        FormatCanonicalPlaneEdgeEvidence(
                            planeCutAudit.BuiltEdgeEvidence,
                            planeCutAudit.CoverageAudit) + "}" +
                    ",trialRejected:{" +
                        FormatCanonicalPlaneEdgeEvidence(
                            planeCutAudit.TrialRejectedEdgeEvidence,
                            planeCutAudit.CoverageAudit) + "}" +
                    ",deferred:{" +
                        FormatCanonicalPlaneEdgeEvidence(
                            planeCutAudit.DeferredEdgeEvidence,
                            planeCutAudit.CoverageAudit) + "}" +
                ",planeGraphEdges=" +
                    "active:{" +
                        FormatRawPlaneEdgeEvidence(
                            planeCutAudit.ActiveEdgeEvidence) + "}" +
                    ",attempted:{" +
                        FormatRawPlaneEdgeEvidence(
                            planeCutAudit.AttemptedEdgeEvidence) + "}" +
                    ",certified:{" +
                        FormatRawPlaneEdgeEvidence(
                            planeCutAudit.BuiltEdgeEvidence) + "}" +
                    ",trialRejected:{" +
                        FormatRawPlaneEdgeEvidence(
                            planeCutAudit.TrialRejectedEdgeEvidence) + "}" +
                    ",deferred:{" +
                        FormatRawPlaneEdgeEvidence(
                            planeCutAudit.DeferredEdgeEvidence) + "}" +
                ",planeMesh=" +
                    planeCutAudit.PreviewTriangleCount + "/" +
                    planeCutAudit.PreviewDegenerateTriangleCount + "/" +
                    planeCutAudit.PreviewOpenEdgeCount + "/" +
                    planeCutAudit.PreviewNonManifoldEdgeCount + "/" +
                    planeCutAudit.PreviewWindingFailureCount + "/" +
                    planeCutAudit.PreviewBoundsFailureCount + "/" +
                    planeCutAudit.PreviewVolumeFailureCount + "/" +
                    planeCutAudit.PreviewGeometryValid +
                (string.IsNullOrEmpty(planeCutAudit.Diagnostic)
                    ? string.Empty
                    : ",planeTrace=" + planeCutAudit.Diagnostic);
        }

        private static string FormatMaterialWidthRecoveryAudit(
            EdgeWearCoverageAudit audit)
        {
            SortedSet<int> eligible = new SortedSet<int>();
            SortedSet<int> baselineDeferred = new SortedSet<int>();
            SortedSet<int> attempted = new SortedSet<int>();
            SortedSet<int> completed = new SortedSet<int>();
            SortedSet<int> certified = new SortedSet<int>();
            SortedDictionary<int, string> failed =
                new SortedDictionary<int, string>();
            if (audit != null)
            {
                for (int recordIndex = 0;
                     recordIndex < audit.Records.Count;
                     recordIndex++)
                {
                    EdgeWearEdgeLifecycleRecord record =
                        audit.Records[recordIndex];
                    if (record == null ||
                        !record.MaterialWidthRecoveryTarget)
                    {
                        continue;
                    }

                    int sourceEdgeIndex =
                        ResolveEdgeWearDisplaySourceEdgeIndex(
                            audit,
                            record);
                    eligible.Add(sourceEdgeIndex);
                    if (record.MaterialWidthRecoveryBaselineDeferred)
                    {
                        baselineDeferred.Add(sourceEdgeIndex);
                    }
                    if (record.MaterialWidthRecoveryAttempted)
                    {
                        attempted.Add(sourceEdgeIndex);
                    }
                    if (record.MaterialWidthRecoveryTrialCompleted)
                    {
                        completed.Add(sourceEdgeIndex);
                    }
                    if (record.MaterialWidthRecoveryCertified &&
                        record.Built)
                    {
                        certified.Add(sourceEdgeIndex);
                    }
                    else
                    {
                        string reason =
                            !record.MaterialWidthRecoveryAttempted
                                ? "not-attempted"
                                : string.IsNullOrEmpty(
                                    record.MaterialWidthRecoveryFailure)
                                    ? record.WidthRecoveryResolution
                                    : record.MaterialWidthRecoveryFailure;
                        failed[sourceEdgeIndex] =
                            SanitizeMaterialWidthRecoveryEvidence(reason);
                    }
                }
            }

            return "eligible:{" +
                    FormatSortedEdgeIndices(eligible) + "}" +
                ",baselineDeferred:{" +
                    FormatSortedEdgeIndices(baselineDeferred) + "}" +
                ",attempted:{" +
                    FormatSortedEdgeIndices(attempted) + "}" +
                ",completed:{" +
                    FormatSortedEdgeIndices(completed) + "}" +
                ",certified:{" +
                    FormatSortedEdgeIndices(certified) + "}" +
                ",failed:{" +
                    FormatMaterialWidthRecoveryFailures(failed) + "}";
        }

        private static string FormatSortedEdgeIndices(
            IEnumerable<int> indices)
        {
            if (indices == null)
            {
                return "none";
            }
            StringBuilder builder = new StringBuilder();
            foreach (int index in indices)
            {
                if (builder.Length > 0)
                {
                    builder.Append('/');
                }
                builder.Append(index);
            }
            return builder.Length == 0 ? "none" : builder.ToString();
        }

        private static string FormatMaterialWidthRecoveryFailures(
            SortedDictionary<int, string> failures)
        {
            if (failures == null || failures.Count == 0)
            {
                return "none";
            }
            StringBuilder builder = new StringBuilder();
            foreach (KeyValuePair<int, string> pair in failures)
            {
                if (builder.Length > 0)
                {
                    builder.Append('|');
                }
                builder.Append(pair.Key);
                builder.Append('=');
                builder.Append(string.IsNullOrEmpty(pair.Value)
                    ? "unspecified"
                    : pair.Value);
            }
            return builder.ToString();
        }

        private static string SanitizeMaterialWidthRecoveryEvidence(
            string evidence)
        {
            if (string.IsNullOrEmpty(evidence))
            {
                return "unspecified";
            }
            return evidence
                .Replace("\r", " ")
                .Replace("\n", " ")
                .Replace("|", "/")
                .Replace("{", "(")
                .Replace("}", ")");
        }

        private static string FormatRawPlaneEdgeEvidence(
            string evidence)
        {
            return string.IsNullOrEmpty(evidence)
                ? "none"
                : evidence;
        }

        private static string FormatCanonicalPlaneEdgeEvidence(
            string graphEvidence,
            EdgeWearCoverageAudit coverageAudit)
        {
            if (string.IsNullOrEmpty(graphEvidence))
            {
                return "none";
            }
            if (coverageAudit == null)
            {
                return graphEvidence;
            }

            string[] tokens = graphEvidence.Split('/');
            List<int> canonical = new List<int>(tokens.Length);
            HashSet<int> seen = new HashSet<int>();
            for (int tokenIndex = 0;
                 tokenIndex < tokens.Length;
                 tokenIndex++)
            {
                if (!int.TryParse(
                        tokens[tokenIndex],
                        NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out int graphEdgeIndex))
                {
                    continue;
                }

                int displayIndex = graphEdgeIndex;
                if (coverageAudit.RecordByGraphEdge.TryGetValue(
                        graphEdgeIndex,
                        out EdgeWearEdgeLifecycleRecord record) &&
                    record != null)
                {
                    displayIndex =
                        ResolveEdgeWearDisplaySourceEdgeIndex(
                            coverageAudit,
                            record);
                }
                if (seen.Add(displayIndex))
                {
                    canonical.Add(displayIndex);
                }
            }

            if (canonical.Count == 0)
            {
                return "none";
            }
            canonical.Sort();
            return string.Join("/", canonical);
        }

        private static void LogPlaneCutBevelAudit(
            PlaneCutBevelAuditResult planeCutAudit)
        {
#if UNITY_EDITOR
            string message =
                "GeneratedMass plane-cut bevel compact audit. " +
                FormatPlaneCutBevelAuditFields(planeCutAudit) +
                ", geometryCommit=disabled";
            LogChamferNoStackTrace(
                message,
                planeCutAudit.GeometryValid != 1);
#endif
        }

        private static string FormatPlaneCutVector(Vector3 value)
        {
            return "(" + value.x.ToString("G9") + "/" +
                value.y.ToString("G9") + "/" +
                value.z.ToString("G9") + ")";
        }

        private static string FormatPlaneCutStageSnapshot(
            PlaneCutStageSnapshot snapshot)
        {
            if (string.IsNullOrEmpty(snapshot.Stage))
            {
                return "notCaptured";
            }
            return snapshot.Stage +
                "[faces=" + snapshot.FaceCount +
                ",vertices=" + snapshot.VertexCount +
                ",unique=" + snapshot.UniqueVertexCount +
                ",bevel=" + snapshot.BevelFaceCount +
                ",junction=" + snapshot.JunctionFaceCount +
                ",open=" + snapshot.OpenEdgeCount +
                ",nonManifold=" + snapshot.NonManifoldEdgeCount +
                ",tJunction=" + snapshot.TJunctionCount +
                ",invalid=" + snapshot.InvalidFaceCount +
                ",nonPlanar=" + snapshot.NonPlanarFaceCount +
                ",maxDeviation=" +
                    snapshot.MaximumPlaneDeviation.ToString("G9") +
                ",maxSpread=" +
                    snapshot.MaximumNormalSpreadDegrees.ToString("G9") +
                "]";
        }

        private static string FormatPlaneCutStageTimeline(
            PlaneCutBevelAuditResult audit)
        {
            return FormatPlaneCutStageSnapshot(
                    audit.StagePlaneConstruction) + ";" +
                FormatPlaneCutStageSnapshot(audit.StageSanitized) + ";" +
                FormatPlaneCutStageSnapshot(audit.StageWelded) + ";" +
                FormatPlaneCutStageSnapshot(audit.StageConformed) + ";" +
                FormatPlaneCutStageSnapshot(audit.StageSeamRepaired) + ";" +
                FormatPlaneCutStageSnapshot(
                    audit.StageFinalCertification);
        }

        private static string FormatPlaneCutFaceFailure(
            PlaneCutFaceQualityFailureRecord failure,
            bool includeVertices)
        {
            string result =
                "face=" + failure.FaceIndex +
                ",id=" + failure.ProvenanceKind + ":" +
                    failure.ProvenanceIndex +
                ",cause=" + failure.Cause +
                ",firstStage=" + failure.FirstFailureStage +
                ",vertices=" + failure.VertexCount +
                ",deviation=" +
                    failure.MaximumPlaneDeviation.ToString("G9") +
                "/" + failure.PlanarityTolerance.ToString("G9") +
                ",offendingVertex=" + failure.OffendingVertexIndex +
                ",signedResidual=" +
                    failure.OffendingSignedResidual.ToString("G9") +
                ",spread=" +
                    failure.MaximumNormalSpreadDegrees.ToString("G9") +
                "/" +
                    failure.NormalSpreadToleranceDegrees.ToString("G9") +
                ",offendingSegment=" +
                    failure.OffendingSegmentIndex +
                ",area=" + failure.Area.ToString("G9") +
                ",minEdge=" +
                    failure.MinimumEdgeLength.ToString("G9") +
                ",conformTouched=" +
                    failure.BoundaryConformityTouched +
                ",seamTouched=" + failure.SeamRepairTouched +
                ",seamMove=" +
                    failure.SeamRepairMaximumMovement.ToString("G9");
            if (!includeVertices)
            {
                return result;
            }
            return result +
                ",authoredNormal=" +
                    FormatPlaneCutVector(failure.AuthoredNormal) +
                ",measuredNormal=" +
                    FormatPlaneCutVector(failure.MeasuredNormal) +
                ",planeDistance=" +
                    failure.PlaneDistance.ToString("G9") +
                ",offendingPosition=" +
                    FormatPlaneCutVector(
                        failure.OffendingVertexPosition) +
                ",offendingTriangleNormal=" +
                    FormatPlaneCutVector(
                        failure.OffendingTriangleNormal) +
                ",vertexResiduals={" +
                    failure.VertexResidualEvidence + "}";
        }

        private static string FormatPlaneCutOpenEdgeFailure(
            PlaneCutOpenEdgeFailureRecord failure,
            bool includeNearestSegment)
        {
            string result =
                "open=" + failure.RecordIndex +
                ",owner=" + failure.FaceProvenanceKind + ":" +
                    failure.FaceProvenanceIndex +
                "#" + failure.FaceIndex +
                ",cause=" + failure.Cause +
                ",firstStage=" + failure.FirstFailureStage +
                ",length=" + failure.Length.ToString("G9") +
                ",sourceVertex=" + failure.AssociatedSourceVertex +
                ",sourceDistance=" +
                    failure.SourceVertexDistance.ToString("G9") +
                ",incidentEdges={" + failure.IncidentBuiltEdges + "}" +
                ",junctionExpected=" + failure.JunctionExpected +
                ",junctionFaces=" + failure.JunctionFaceCount +
                ",expected=" + failure.ExpectedNeighbour +
                ",nearest=" +
                    failure.NearestFaceProvenanceKind + ":" +
                    failure.NearestFaceProvenanceIndex +
                    "#" + failure.NearestFaceIndex +
                ",nearestDistance=" +
                    failure.NearestReversedEndpointDistance.ToString("G9") +
                ",edge=" + FormatPlaneCutVector(failure.Start) + "->" +
                    FormatPlaneCutVector(failure.End);
            if (!includeNearestSegment)
            {
                return result;
            }
            return result +
                ",sourcePosition=" +
                    FormatPlaneCutVector(
                        failure.AssociatedSourcePosition) +
                ",nearestSegment=" +
                    FormatPlaneCutVector(failure.NearestSegmentStart) +
                    "->" +
                    FormatPlaneCutVector(failure.NearestSegmentEnd);
        }

        private static string FormatPlaneCutTJunctionFailure(
            PlaneCutTJunctionFailureRecord failure,
            bool includeOwners)
        {
            string result =
                "record=" + failure.RecordIndex +
                ",stage=" + failure.Stage +
                ",cause=" + failure.Cause +
                ",vertex=" +
                    FormatPlaneCutVector(failure.JunctionVertex) +
                ",host=" + failure.HostProvenanceKind + ":" +
                    failure.HostProvenanceIndex + "#" +
                    failure.HostFaceIndex +
                ",hostSegment=" + failure.HostSegmentIndex +
                ",t=" + failure.SegmentParameter.ToString("G9") +
                ",distance=" + failure.Distance.ToString("G9") +
                    "/" + failure.Tolerance.ToString("G9") +
                ",hostLength=" + failure.HostLength.ToString("G9") +
                ",hostMatches=" + failure.MatchingHostSegmentCount +
                ",provenanceBevels={" +
                    failure.ProvenanceBevelEdges + "}" +
                ",candidatePlaneMatches={" +
                    failure.CandidatePlaneMatches + "}" +
                ",edgeScales={" +
                    failure.AssociatedEdgeScales + "}" +
                ",lastConflictPass=" + failure.LastConflictPass +
                ",lastConflictCluster={" +
                    failure.LastConflictCluster + "}";
            if (!includeOwners)
            {
                return result;
            }
            return result +
                ",closest=" +
                    FormatPlaneCutVector(failure.ClosestPoint) +
                ",hostEdge=" +
                    FormatPlaneCutVector(failure.HostStart) + "->" +
                    FormatPlaneCutVector(failure.HostEnd) +
                ",vertexOwnerCount=" + failure.VertexOwnerFaceCount +
                ",vertexOwners={" + failure.VertexOwnerFaces + "}";
        }

        private static string FormatPlaneCutLocalityDeferral(
            PlaneCutLocalityDeferralRecord record,
            bool includePositions)
        {
            string result =
                "edge=" + record.SourceEdgeIndex +
                ",vertices=" + record.VertexA + "/" + record.VertexB +
                ",faces=" + record.FaceA + "/" + record.FaceB +
                ",width=" + record.SolvedWidth.ToString("G9") +
                ",solvedDistance=" +
                    record.SolvedPlaneDistance.ToString("G9") +
                ",localizedDistance=" +
                    record.LocalizedPlaneDistance.ToString("G9") +
                ",localizationDelta=" +
                    record.LocalizationDelta.ToString("G9") +
                ",guardMargin=" +
                    record.LocalGuardMargin.ToString("G9") +
                ",limitingVertex=" +
                    record.LimitingUnrelatedVertex +
                ",solvedRemoval=" +
                    record.SolvedSourceRemovalA.ToString("G9") + "/" +
                    record.SolvedSourceRemovalB.ToString("G9") +
                ",localizedRemoval=" +
                    record.LocalizedSourceRemovalA.ToString("G9") + "/" +
                    record.LocalizedSourceRemovalB.ToString("G9") +
                ",minimumRemoval=" +
                    record.MinimumRequiredRemoval.ToString("G9") +
                ",cause=" + record.Cause;
            if (!includePositions)
            {
                return result;
            }
            return result +
                ",normal=" +
                    FormatPlaneCutVector(record.BevelNormal) +
                ",sourceA=" +
                    FormatPlaneCutVector(record.SourceA) +
                ",sourceB=" +
                    FormatPlaneCutVector(record.SourceB) +
                ",limitingPosition=" +
                    FormatPlaneCutVector(
                        record.LimitingUnrelatedPosition) +
                ",limitingProjection=" +
                    record.LimitingUnrelatedProjection.ToString("G9");
        }

        private static string FormatCappedPlaneCutTJunctions(
            PlaneCutBevelAuditResult audit,
            int cap)
        {
            if (audit.TJunctionFailures == null ||
                audit.TJunctionFailures.Count == 0)
            {
                return "none";
            }
            StringBuilder builder = new StringBuilder();
            int count = Mathf.Min(cap, audit.TJunctionFailures.Count);
            for (int index = 0; index < count; index++)
            {
                if (index > 0)
                {
                    builder.Append('|');
                }
                builder.Append(FormatPlaneCutTJunctionFailure(
                    audit.TJunctionFailures[index],
                    false));
            }
            if (audit.TJunctionFailures.Count > count)
            {
                builder.Append("|omitted=");
                builder.Append(audit.TJunctionFailures.Count - count);
            }
            return builder.ToString();
        }

        private static string FormatCappedPlaneCutLocalityDeferrals(
            PlaneCutBevelAuditResult audit,
            int cap)
        {
            if (audit.LocalityDeferrals == null ||
                audit.LocalityDeferrals.Count == 0)
            {
                return "none";
            }
            StringBuilder builder = new StringBuilder();
            int count = Mathf.Min(cap, audit.LocalityDeferrals.Count);
            for (int index = 0; index < count; index++)
            {
                if (index > 0)
                {
                    builder.Append('|');
                }
                builder.Append(FormatPlaneCutLocalityDeferral(
                    audit.LocalityDeferrals[index],
                    false));
            }
            if (audit.LocalityDeferrals.Count > count)
            {
                builder.Append("|omitted=");
                builder.Append(audit.LocalityDeferrals.Count - count);
            }
            return builder.ToString();
        }

        private static string FormatPlaneCutSolverTransactionState(
            PlaneCutSolverTransactionState state)
        {
            if (state == null)
            {
                return "none";
            }
            return "name=" + state.Name +
                ",pass=" + state.PassIndex +
                ",candidates=" + state.Candidates.Count +
                ",bandClean=" + state.BandClean +
                ",geometryClean=" + state.GeometryClean +
                ",edges={" +
                    FormatPlaneCutCandidateEdgeEvidence(
                        state.Candidates) + "}" +
                ",scales={" +
                    FormatPlaneCutScaleEvidence(
                        state.ScaleByEdge,
                        CollectPlaneCutCandidateEdgeIndices(
                            state.Candidates)) + "}" +
                ",stage={" +
                    FormatPlaneCutStageSnapshot(state.Stage) + "}";
        }

        private static string FormatPlaneCutRetryFailureDossier(
            PlaneCutRetryFailureDossier dossier,
            bool complete)
        {
            if (dossier == null)
            {
                return "none";
            }
            StringBuilder builder = new StringBuilder();
            builder.Append("pass=");
            builder.Append(dossier.PassIndex);
            builder.Append(",stage=");
            builder.Append(dossier.Stage);
            builder.Append(",cause=");
            builder.Append(dossier.Cause);
            builder.Append(",attemptedBuilt=");
            builder.Append(dossier.AttemptedBuiltCount);
            builder.Append(",topology=");
            builder.Append(dossier.OpenEdgeCount);
            builder.Append('/');
            builder.Append(dossier.NonManifoldEdgeCount);
            builder.Append('/');
            builder.Append(dossier.TJunctionCount);
            builder.Append('/');
            builder.Append(dossier.InvalidFaceCount);
            builder.Append(",nonPlanar=");
            builder.Append(dossier.NonPlanarFaceCount);
            builder.Append(",linked={");
            builder.Append(FormatPlaneCutEdgeIndexEvidence(
                dossier.LinkedEdgeIndices));
            builder.Append("},cluster={");
            builder.Append(string.IsNullOrEmpty(
                    dossier.GeneralizedClusterEvidence)
                ? "none"
                : dossier.GeneralizedClusterEvidence);
            builder.Append("},clusterReasons={");
            builder.Append(string.IsNullOrEmpty(
                    dossier.GeneralizedClusterReasonEvidence)
                ? "none"
                : dossier.GeneralizedClusterReasonEvidence);
            builder.Append('}');
            if (!complete)
            {
                if (dossier.NonPlanarFaceFailures.Count > 0)
                {
                    builder.Append(",face={");
                    builder.Append(FormatPlaneCutFaceFailure(
                        dossier.NonPlanarFaceFailures[0],
                        false));
                    builder.Append('}');
                }
                if (dossier.OpenEdgeFailures.Count > 0)
                {
                    builder.Append(",open={");
                    builder.Append(FormatPlaneCutOpenEdgeFailure(
                        dossier.OpenEdgeFailures[0],
                        false));
                    builder.Append('}');
                }
                if (dossier.TJunctionFailures.Count > 0)
                {
                    builder.Append(",tJunction={");
                    builder.Append(FormatPlaneCutTJunctionFailure(
                        dossier.TJunctionFailures[0],
                        false));
                    builder.Append('}');
                }
                return builder.ToString();
            }

            builder.Append(",candidateEdges={");
            builder.Append(string.IsNullOrEmpty(
                    dossier.CandidateEdgeEvidence)
                ? "none"
                : dossier.CandidateEdgeEvidence);
            builder.Append("},scales={");
            builder.Append(string.IsNullOrEmpty(dossier.ScaleEvidence)
                ? "none"
                : dossier.ScaleEvidence);
            builder.Append("},nonManifold={");
            builder.Append(string.IsNullOrEmpty(
                    dossier.NonManifoldEvidence)
                ? "none"
                : dossier.NonManifoldEvidence);
            builder.Append("},invalidFaces={");
            builder.Append(string.IsNullOrEmpty(
                    dossier.InvalidFaceEvidence)
                ? "none"
                : dossier.InvalidFaceEvidence);
            builder.Append('}');
            return builder.ToString();
        }

        private static string FormatCappedPlaneCutRetryFailures(
            PlaneCutBevelAuditResult audit,
            int cap)
        {
            if (audit.RetryFailureDossiers == null ||
                audit.RetryFailureDossiers.Count == 0)
            {
                return "none";
            }
            StringBuilder builder = new StringBuilder();
            int count = Mathf.Min(
                cap,
                audit.RetryFailureDossiers.Count);
            int startIndex =
                audit.RetryFailureDossiers.Count - count;
            for (int offset = 0; offset < count; offset++)
            {
                if (offset > 0)
                {
                    builder.Append('|');
                }
                builder.Append(FormatPlaneCutRetryFailureDossier(
                    audit.RetryFailureDossiers[startIndex + offset],
                    false));
            }
            int omitted = audit.RetryFailureDossiers.Count - count;
            if (omitted > 0)
            {
                builder.Append("|omitted=");
                builder.Append(omitted);
            }
            return builder.ToString();
        }

        private static void AppendPlaneCutRetryFailureDossiers(
            StringBuilder builder,
            PlaneCutBevelAuditResult audit)
        {
            int count = audit.RetryFailureDossiers == null
                ? 0
                : audit.RetryFailureDossiers.Count;
            builder.Append("count=");
            builder.AppendLine(count.ToString());
            if (audit.RetryFailureDossiers == null)
            {
                return;
            }
            for (int index = 0; index < count; index++)
            {
                PlaneCutRetryFailureDossier dossier =
                    audit.RetryFailureDossiers[index];
                builder.Append(index);
                builder.Append(':');
                builder.AppendLine(FormatPlaneCutRetryFailureDossier(
                    dossier,
                    true));
                for (int faceIndex = 0;
                     faceIndex < dossier.NonPlanarFaceFailures.Count;
                     faceIndex++)
                {
                    builder.Append("  face[");
                    builder.Append(faceIndex);
                    builder.Append("]=");
                    builder.AppendLine(FormatPlaneCutFaceFailure(
                        dossier.NonPlanarFaceFailures[faceIndex],
                        true));
                }
                for (int openIndex = 0;
                     openIndex < dossier.OpenEdgeFailures.Count;
                     openIndex++)
                {
                    builder.Append("  open[");
                    builder.Append(openIndex);
                    builder.Append("]=");
                    builder.AppendLine(FormatPlaneCutOpenEdgeFailure(
                        dossier.OpenEdgeFailures[openIndex],
                        true));
                }
                for (int tIndex = 0;
                     tIndex < dossier.TJunctionFailures.Count;
                     tIndex++)
                {
                    builder.Append("  tJunction[");
                    builder.Append(tIndex);
                    builder.Append("]=");
                    builder.AppendLine(FormatPlaneCutTJunctionFailure(
                        dossier.TJunctionFailures[tIndex],
                        true));
                }
            }
        }

        private static string FormatPlaneCutPrimaryFailure(
            PlaneCutBevelAuditResult audit)
        {
            if (audit.FaceQualityFailures != null &&
                audit.FaceQualityFailures.Count > 0)
            {
                PlaneCutFaceQualityFailureRecord failure =
                    audit.FaceQualityFailures[0];
                return "stage=" + failure.FirstFailureStage +
                    ",category=FaceQuality" +
                    ",face=" + failure.FaceIndex +
                    ",id=" + failure.ProvenanceKind + ":" +
                        failure.ProvenanceIndex +
                    ",cause=" + failure.Cause +
                    ",deviation=" +
                        failure.MaximumPlaneDeviation.ToString("G9") +
                        "/" +
                        failure.PlanarityTolerance.ToString("G9") +
                    ",spread=" +
                        failure.MaximumNormalSpreadDegrees.ToString("G9") +
                        "/" +
                        failure.NormalSpreadToleranceDegrees.ToString("G9");
            }
            if (audit.OpenEdgeFailures != null &&
                audit.OpenEdgeFailures.Count > 0)
            {
                PlaneCutOpenEdgeFailureRecord failure =
                    audit.OpenEdgeFailures[0];
                return "stage=" + failure.FirstFailureStage +
                    ",category=OpenEdge" +
                    ",owner=" + failure.FaceProvenanceKind + ":" +
                        failure.FaceProvenanceIndex +
                    ",cause=" + failure.Cause +
                    ",sourceVertex=" +
                        failure.AssociatedSourceVertex +
                    ",expected=" + failure.ExpectedNeighbour;
            }
            if (audit.TJunctionFailures != null &&
                audit.TJunctionFailures.Count > 0)
            {
                PlaneCutTJunctionFailureRecord failure =
                    audit.TJunctionFailures[0];
                return "stage=" + failure.Stage +
                    ",category=TJunction" +
                    ",vertex=" +
                        FormatPlaneCutVector(failure.JunctionVertex) +
                    ",host=" + failure.HostProvenanceKind + ":" +
                        failure.HostProvenanceIndex +
                    ",segment=" + failure.HostSegmentIndex +
                    ",t=" + failure.SegmentParameter.ToString("G9") +
                    ",distance=" + failure.Distance.ToString("G9") +
                        "/" + failure.Tolerance.ToString("G9") +
                    ",bevels={" +
                        failure.ProvenanceBevelEdges + "}" +
                    ",lastConflictPass=" +
                        failure.LastConflictPass;
            }
            if (audit.RetryFailureDossiers != null &&
                audit.RetryFailureDossiers.Count > 0)
            {
                PlaneCutRetryFailureDossier failure =
                    audit.RetryFailureDossiers[
                        audit.RetryFailureDossiers.Count - 1];
                return "stage=" + failure.Stage +
                    ",category=RetryFailure" +
                    ",cause=" + failure.Cause +
                    ",topology=" + failure.OpenEdgeCount + "/" +
                        failure.NonManifoldEdgeCount + "/" +
                        failure.TJunctionCount + "/" +
                        failure.InvalidFaceCount +
                    ",nonPlanar=" + failure.NonPlanarFaceCount +
                    ",linked={" +
                        FormatPlaneCutEdgeIndexEvidence(
                            failure.LinkedEdgeIndices) + "}" +
                    ",cluster={" +
                        (string.IsNullOrEmpty(
                                failure.GeneralizedClusterEvidence)
                            ? "none"
                            : failure.GeneralizedClusterEvidence) + "}";
            }
            if (audit.NumericalRepairs != null &&
                audit.NumericalRepairs.ExactConstructionFailureCount > 0)
            {
                return "stage=PlaneConstruction" +
                    ",category=StrictIntersection" +
                    ",cause={" +
                    FormatPlaneCutFirstExactFailure(
                        audit.NumericalRepairs) + "}";
            }
            if (audit.GeometryValid == 1 &&
                audit.MaterializedEdgeCoverageValid == 0)
            {
                return "category=Coverage,cause=selected bevels did not all materialize" +
                    ",built=" + audit.PlanesBuilt +
                    ",active=" + audit.ActiveEdgeCount +
                    ",deferred=" + audit.PlanesDeferred +
                    ",unresolvedConflicts=" +
                        audit.EdgeConflictUnresolvedCount;
            }
            if (audit.GeometryValid == 1 &&
                audit.MaterializedEdgeCoverageValid == 1)
            {
                return "none";
            }
            return string.IsNullOrEmpty(audit.Diagnostic)
                ? "none"
                : "category=General,cause=" + audit.Diagnostic;
        }

        private static string FormatCappedPlaneCutFaceFailures(
            PlaneCutBevelAuditResult audit,
            int cap)
        {
            if (audit.FaceQualityFailures == null ||
                audit.FaceQualityFailures.Count == 0)
            {
                return "none";
            }
            StringBuilder builder = new StringBuilder();
            int count = Mathf.Min(cap, audit.FaceQualityFailures.Count);
            for (int index = 0; index < count; index++)
            {
                if (index > 0)
                {
                    builder.Append('|');
                }
                builder.Append(FormatPlaneCutFaceFailure(
                    audit.FaceQualityFailures[index],
                    false));
            }
            if (audit.FaceQualityFailures.Count > count)
            {
                builder.Append("|omitted=");
                builder.Append(audit.FaceQualityFailures.Count - count);
            }
            return builder.ToString();
        }

        private static string FormatCappedPlaneCutOpenEdges(
            PlaneCutBevelAuditResult audit,
            int cap)
        {
            if (audit.OpenEdgeFailures == null ||
                audit.OpenEdgeFailures.Count == 0)
            {
                return "none";
            }
            StringBuilder builder = new StringBuilder();
            int count = Mathf.Min(cap, audit.OpenEdgeFailures.Count);
            for (int index = 0; index < count; index++)
            {
                if (index > 0)
                {
                    builder.Append('|');
                }
                builder.Append(FormatPlaneCutOpenEdgeFailure(
                    audit.OpenEdgeFailures[index],
                    false));
            }
            if (audit.OpenEdgeFailures.Count > count)
            {
                builder.Append("|omitted=");
                builder.Append(audit.OpenEdgeFailures.Count - count);
            }
            return builder.ToString();
        }

        private static string FormatPlaneCutNumericalRepairs(
            PlaneCutNumericalRepairTelemetry repairs)
        {
            if (repairs == null)
            {
                return "none";
            }
            return "intersections:" +
                    repairs.IntersectionRequestCount +
                ",strict:" +
                    repairs.StrictCrossingIntersectionCount +
                ",fallbackProjected:" +
                    repairs.ProjectedFallbackIntersectionCount +
                ",sameSideFallbackAttempts:" +
                    repairs.SameSideFallbackAttemptCount +
                ",classifications:" +
                    repairs.StrictInsideClassificationCount + "/" +
                    repairs.StrictOnPlaneClassificationCount + "/" +
                    repairs.StrictOutsideClassificationCount +
                ",onPlaneSnaps:" + repairs.OnPlaneSnapCount +
                ",maxOnPlaneSnap:" +
                    repairs.MaximumOnPlaneSnapDistance.ToString("G9") +
                ",cacheReuse:" +
                    repairs.CachedIntersectionReuseCount +
                ",cacheInvalidated:" +
                    repairs.CachedIntersectionInvalidationCount +
                ",cacheRecomputed:" +
                    repairs.CachedIntersectionRecomputeSuccessCount +
                ",twoPlaneCorrections:" +
                    repairs.IntersectionProjectionCount +
                ",maxCorrection:" +
                    repairs.MaximumIntersectionProjectionDistance
                        .ToString("G9") +
                ",cutResidual:" +
                    repairs.MaximumCutPlaneResidualBeforeCorrection
                        .ToString("G9") + "/" +
                    repairs.MaximumCutPlaneResidualAfterCorrection
                        .ToString("G9") +
                ",ownerResidual:" +
                    repairs.MaximumOwnerPlaneResidualBeforeCorrection
                        .ToString("G9") + "/" +
                    repairs.MaximumOwnerPlaneResidualAfterCorrection
                        .ToString("G9") +
                ",exactFailures:" +
                    repairs.ExactConstructionFailureCount +
                ",capProjected:" +
                    repairs.CapVertexProjectionCount +
                ",capValidated:" +
                    repairs.CapVertexValidationCount +
                ",capResidualBefore:" +
                    repairs.MaximumCapResidualBeforeProjection
                        .ToString("G9") +
                ",capResidualAfter:" +
                    repairs.MaximumCapResidualAfterProjection
                        .ToString("G9") +
                ",capRejected:" +
                    repairs.CapResidualRejectCount +
                ",weldComparisons:" +
                    repairs.DistanceWeldComparisonCount +
                ",weldMatches:" +
                    repairs.DistanceWeldMatchCount +
                ",weldMoved:" +
                    repairs.DistanceWeldMovedCount +
                ",maxWeldMove:" +
                    repairs.MaximumDistanceWeldMovement
                        .ToString("G9") +
                ",firstExactFailure:{" +
                    FormatPlaneCutFirstExactFailure(repairs) + "}";
        }

        private static string FormatPlaneCutFirstExactFailure(
            PlaneCutNumericalRepairTelemetry repairs)
        {
            if (repairs == null ||
                repairs.FirstExactFailureRecorded == 0)
            {
                return "none";
            }

            return "owner=" +
                    repairs.FirstExactFailureOwnerProvenanceKind + ":" +
                    repairs.FirstExactFailureOwnerProvenanceIndex +
                ",cut=" +
                    repairs.FirstExactFailureCutProvenanceKind + ":" +
                    repairs.FirstExactFailureCutProvenanceIndex +
                ",distance=" +
                    repairs.FirstExactFailureStartDistance
                        .ToString("G9") + "/" +
                    repairs.FirstExactFailureEndDistance
                        .ToString("G9") +
                ",classification=" +
                    repairs.FirstExactFailureStartClassification + "/" +
                    repairs.FirstExactFailureEndClassification +
                ",cutResidual=" +
                    repairs.FirstExactFailureCutResidualBefore
                        .ToString("G9") + "/" +
                    repairs.FirstExactFailureCutResidualAfter
                        .ToString("G9") +
                ",ownerResidual=" +
                    repairs.FirstExactFailureOwnerResidualBefore
                        .ToString("G9") + "/" +
                    repairs.FirstExactFailureOwnerResidualAfter
                        .ToString("G9") +
                ",reason=" +
                    (string.IsNullOrEmpty(
                        repairs.FirstExactFailureReason)
                            ? "none"
                            : repairs.FirstExactFailureReason);
        }

        private static void AppendPlaneCutNumericalRepairDossier(
            StringBuilder builder,
            PlaneCutNumericalRepairTelemetry repairs)
        {
            if (repairs == null)
            {
                builder.AppendLine("none");
                return;
            }

            builder.Append("strictClassificationTolerance=");
            builder.AppendLine(
                (PointMergeDistance * 0.25f).ToString("G9"));
            builder.Append("classificationsInsideOnOutside=");
            builder.Append(repairs.StrictInsideClassificationCount);
            builder.Append('/');
            builder.Append(repairs.StrictOnPlaneClassificationCount);
            builder.Append('/');
            builder.AppendLine(
                repairs.StrictOutsideClassificationCount.ToString());
            builder.Append("intersectionRequests=");
            builder.AppendLine(repairs.IntersectionRequestCount.ToString());
            builder.Append("strictCrossings=");
            builder.AppendLine(
                repairs.StrictCrossingIntersectionCount.ToString());
            builder.Append("sameSideFallbackAttempts=");
            builder.AppendLine(
                repairs.SameSideFallbackAttemptCount.ToString());
            builder.Append("legacyProjectedFallbacks=");
            builder.AppendLine(
                repairs.ProjectedFallbackIntersectionCount.ToString());
            builder.Append("onPlaneSnaps=");
            builder.Append(repairs.OnPlaneSnapCount);
            builder.Append(",maximumMovement=");
            builder.AppendLine(
                repairs.MaximumOnPlaneSnapDistance.ToString("G9"));
            builder.Append("twoPlaneCorrections=");
            builder.Append(repairs.IntersectionProjectionCount);
            builder.Append(",maximumMovement=");
            builder.AppendLine(
                repairs.MaximumIntersectionProjectionDistance
                    .ToString("G9"));
            builder.Append("cutPlaneResidualBeforeAfter=");
            builder.Append(
                repairs.MaximumCutPlaneResidualBeforeCorrection
                    .ToString("G9"));
            builder.Append('/');
            builder.AppendLine(
                repairs.MaximumCutPlaneResidualAfterCorrection
                    .ToString("G9"));
            builder.Append("ownerPlaneResidualBeforeAfter=");
            builder.Append(
                repairs.MaximumOwnerPlaneResidualBeforeCorrection
                    .ToString("G9"));
            builder.Append('/');
            builder.AppendLine(
                repairs.MaximumOwnerPlaneResidualAfterCorrection
                    .ToString("G9"));
            builder.Append("capValidatedProjectedRejected=");
            builder.Append(repairs.CapVertexValidationCount);
            builder.Append('/');
            builder.Append(repairs.CapVertexProjectionCount);
            builder.Append('/');
            builder.AppendLine(repairs.CapResidualRejectCount.ToString());
            builder.Append("capResidualBeforeAfter=");
            builder.Append(
                repairs.MaximumCapResidualBeforeProjection
                    .ToString("G9"));
            builder.Append('/');
            builder.AppendLine(
                repairs.MaximumCapResidualAfterProjection
                    .ToString("G9"));
            builder.Append("distanceWeldComparisonsMatchesMoved=");
            builder.Append(repairs.DistanceWeldComparisonCount);
            builder.Append('/');
            builder.Append(repairs.DistanceWeldMatchCount);
            builder.Append('/');
            builder.AppendLine(repairs.DistanceWeldMovedCount.ToString());
            builder.Append("maximumDistanceWeldMovement=");
            builder.AppendLine(
                repairs.MaximumDistanceWeldMovement.ToString("G9"));
            builder.Append("exactConstructionFailures=");
            builder.AppendLine(
                repairs.ExactConstructionFailureCount.ToString());
            builder.Append("firstExactFailure=");
            builder.AppendLine(FormatPlaneCutFirstExactFailure(repairs));
        }

        private static void AppendPlaneCutConflictWidthReductions(
            StringBuilder builder,
            PlaneCutBevelAuditResult audit)
        {
            if (builder == null)
            {
                return;
            }
            builder.AppendLine(FormatPlaneCutEdgeConflictAudit(audit));
            if (audit.EdgeConflictWidthReductions == null ||
                audit.EdgeConflictWidthReductions.Count == 0)
            {
                builder.AppendLine("records=none");
                return;
            }

            builder.Append("records=");
            builder.AppendLine(
                audit.EdgeConflictWidthReductions.Count.ToString());
            for (int recordIndex = 0;
                 recordIndex < audit.EdgeConflictWidthReductions.Count;
                 recordIndex++)
            {
                PlaneCutConflictWidthReductionRecord record =
                    audit.EdgeConflictWidthReductions[recordIndex];
                builder.Append("pass=");
                builder.Append(record.PassIndex);
                builder.Append(",victim=");
                builder.Append(record.VictimEdgeIndex);
                builder.Append(",foreign=");
                builder.Append(record.ForeignEdgeIndex);
                builder.Append(",vertex=");
                builder.Append(record.VertexIndex);
                builder.Append(",trigger=");
                builder.Append(string.IsNullOrEmpty(
                        record.TriggerCategory)
                    ? "none"
                    : record.TriggerCategory);
                builder.Append(",bandValid=");
                builder.Append(record.BandValid);
                builder.Append(",topologyValid=");
                builder.Append(record.TopologyValid);
                builder.Append(",topology=");
                builder.Append(record.OpenEdgeCount);
                builder.Append('/');
                builder.Append(record.NonManifoldEdgeCount);
                builder.Append('/');
                builder.Append(record.TJunctionCount);
                builder.Append('/');
                builder.Append(record.InvalidFaceCount);
                builder.Append(",nonPlanar=");
                builder.Append(record.NonPlanarFaceCount);
                builder.Append(",rollback=");
                builder.Append(record.TopologyRollbackApplied);
                builder.Append(",cluster={");
                for (int edgeIndex = 0;
                     edgeIndex < record.ClusterEdgeIndices.Count;
                     edgeIndex++)
                {
                    if (edgeIndex > 0)
                    {
                        builder.Append('/');
                    }
                    builder.Append(
                        record.ClusterEdgeIndices[edgeIndex]);
                }
                builder.Append("},clusterReasons={");
                builder.Append(string.IsNullOrEmpty(
                        record.ClusterReasonEvidence)
                    ? "none"
                    : record.ClusterReasonEvidence);
                builder.Append("},previousMinimumScale=");
                builder.Append(
                    record.PreviousMinimumScale.ToString("G9"));
                builder.Append(",requestedScale=");
                builder.Append(record.RequestedScale.ToString("G9"));
                builder.Append(",appliedMinimumScale=");
                builder.Append(
                    record.AppliedMinimumScale.ToString("G9"));
                builder.Append(",clusterFloorScale=");
                builder.Append(
                    record.ClusterFloorScale.ToString("G9"));
                builder.Append(",previousScales={");
                builder.Append(string.IsNullOrEmpty(
                        record.PreviousScaleEvidence)
                    ? "none"
                    : record.PreviousScaleEvidence);
                builder.Append("},rollbackScales={");
                builder.Append(string.IsNullOrEmpty(
                        record.RollbackScaleEvidence)
                    ? "none"
                    : record.RollbackScaleEvidence);
                builder.Append("},appliedScales={");
                builder.Append(string.IsNullOrEmpty(
                        record.AppliedScaleEvidence)
                    ? "none"
                    : record.AppliedScaleEvidence);
                builder.Append("},victimCoverage=");
                builder.Append(
                    record.VictimCoverageRatio.ToString("G9"));
                builder.Append(",foreignAxial=");
                builder.Append(
                    record.ForeignAxialParameter.ToString("G9"));
                builder.Append(",foreignSpan=");
                builder.Append(
                    record.ForeignSharedSpanRatio.ToString("G9"));
                builder.Append(",result=");
                builder.AppendLine(string.IsNullOrEmpty(record.Result)
                    ? "none"
                    : record.Result);
            }
        }

        private static string FormatPlaneCutTopologyTrialValidity(
            int evaluated,
            int valid)
        {
            return evaluated == 1
                ? valid.ToString()
                : "not-evaluated";
        }

        private static string FormatPlaneCutTopologyScaleTrial(
            PlaneCutTopologyScaleTrialRecord trial,
            bool complete)
        {
            if (trial == null)
            {
                return "none";
            }
            StringBuilder builder = new StringBuilder();
            builder.Append("trial=");
            builder.Append(trial.TrialIndex);
            builder.Append(",searchMode=");
            builder.Append(string.IsNullOrEmpty(trial.SearchMode)
                ? "none"
                : trial.SearchMode);
            builder.Append(",basePass=");
            builder.Append(trial.BasePassIndex);
            builder.Append(",retreatEdges={");
            for (int edgeIndex = 0;
                 edgeIndex < trial.ClusterEdgeIndices.Count;
                 edgeIndex++)
            {
                if (edgeIndex > 0)
                {
                    builder.Append('/');
                }
                builder.Append(trial.ClusterEdgeIndices[edgeIndex]);
            }
            builder.Append("},protectedEdges={");
            builder.Append(string.IsNullOrEmpty(
                    trial.ProtectedEdgeEvidence)
                ? "none"
                : trial.ProtectedEdgeEvidence);
            builder.Append("},factor=");
            builder.Append(trial.Factor.ToString("G9"));
            builder.Append(",baseScales={");
            builder.Append(string.IsNullOrEmpty(trial.BaseScaleEvidence)
                ? "none"
                : trial.BaseScaleEvidence);
            builder.Append("},requestedScales={");
            builder.Append(string.IsNullOrEmpty(
                    trial.RequestedScaleEvidence)
                ? "none"
                : trial.RequestedScaleEvidence);
            builder.Append("},effectiveScales={");
            builder.Append(string.IsNullOrEmpty(
                    trial.EffectiveScaleEvidence)
                ? "none"
                : trial.EffectiveScaleEvidence);
            builder.Append("},floorHits={");
            builder.Append(string.IsNullOrEmpty(trial.FloorHitEvidence)
                ? "none"
                : trial.FloorHitEvidence);
            builder.Append("},collateralChanged={");
            builder.Append(string.IsNullOrEmpty(
                    trial.CollateralChangedEvidence)
                ? "none"
                : trial.CollateralChangedEvidence);
            builder.Append("},attemptedBuilt=");
            builder.Append(trial.AttemptedBuiltCount);
            builder.Append(",bandValid=");
            builder.Append(FormatPlaneCutTopologyTrialValidity(
                trial.BandEvaluated,
                trial.BandValid));
            builder.Append(",bandVictim=");
            builder.Append(trial.BandVictimEdgeIndex);
            builder.Append(",bandForeign=");
            builder.Append(trial.BandForeignEdgeIndex);
            builder.Append(",bandForeignAxial=");
            builder.Append(trial.BandForeignAxialParameter.ToString("G9"));
            builder.Append(",bandForeignSpan=");
            builder.Append(trial.BandForeignSharedSpanRatio.ToString("G9"));
            builder.Append(",topologyValid=");
            builder.Append(FormatPlaneCutTopologyTrialValidity(
                trial.TopologyEvaluated,
                trial.TopologyValid));
            builder.Append(",faceQualityValid=");
            builder.Append(FormatPlaneCutTopologyTrialValidity(
                trial.FaceQualityEvaluated,
                trial.FaceQualityValid));
            builder.Append(",surfaceValid=");
            builder.Append(trial.SurfaceValid);
            builder.Append(",meshValid=");
            builder.Append(trial.MeshValid);
            builder.Append(",fullyValid=");
            builder.Append(trial.FullyValid);
            builder.Append(",topology=");
            builder.Append(trial.OpenEdgeCount);
            builder.Append('/');
            builder.Append(trial.NonManifoldEdgeCount);
            builder.Append('/');
            builder.Append(trial.TJunctionCount);
            builder.Append('/');
            builder.Append(trial.InvalidFaceCount);
            builder.Append(",nonPlanar=");
            builder.Append(trial.NonPlanarFaceCount);
            builder.Append(",maxDeviation=");
            builder.Append(trial.MaximumPlaneDeviation.ToString("G9"));
            builder.Append(",maxSpread=");
            builder.Append(
                trial.MaximumNormalSpreadDegrees.ToString("G9"));
            builder.Append(",failureStage=");
            builder.Append(string.IsNullOrEmpty(trial.FailureStage)
                ? "none"
                : trial.FailureStage);
            builder.Append(",failureCause=");
            builder.Append(string.IsNullOrEmpty(trial.FailureCause)
                ? "none"
                : trial.FailureCause);
            builder.Append(",result=");
            builder.Append(string.IsNullOrEmpty(trial.Result)
                ? "none"
                : trial.Result);
            if (!complete)
            {
                return builder.ToString();
            }
            if (trial.NonPlanarFaceFailures.Count > 0)
            {
                builder.Append(",faces={");
                for (int failureIndex = 0;
                     failureIndex < trial.NonPlanarFaceFailures.Count;
                     failureIndex++)
                {
                    if (failureIndex > 0)
                    {
                        builder.Append('|');
                    }
                    builder.Append(FormatPlaneCutFaceFailure(
                        trial.NonPlanarFaceFailures[failureIndex],
                        true));
                }
                builder.Append('}');
            }
            if (trial.OpenEdgeFailures.Count > 0)
            {
                builder.Append(",opens={");
                for (int failureIndex = 0;
                     failureIndex < trial.OpenEdgeFailures.Count;
                     failureIndex++)
                {
                    if (failureIndex > 0)
                    {
                        builder.Append('|');
                    }
                    builder.Append(FormatPlaneCutOpenEdgeFailure(
                        trial.OpenEdgeFailures[failureIndex],
                        true));
                }
                builder.Append('}');
            }
            if (trial.TJunctionFailures.Count > 0)
            {
                builder.Append(",tJunctions={");
                for (int failureIndex = 0;
                     failureIndex < trial.TJunctionFailures.Count;
                     failureIndex++)
                {
                    if (failureIndex > 0)
                    {
                        builder.Append('|');
                    }
                    builder.Append(FormatPlaneCutTJunctionFailure(
                        trial.TJunctionFailures[failureIndex],
                        true));
                }
                builder.Append('}');
            }
            return builder.ToString();
        }

        private static void AppendPlaneCutTopologyScaleTrials(
            StringBuilder builder,
            PlaneCutBevelAuditResult audit,
            string searchModeFilter)
        {
            builder.Append("sectionMode=");
            builder.AppendLine(string.IsNullOrEmpty(searchModeFilter)
                ? "all"
                : searchModeFilter);
            builder.Append("finalSearchMode=");
            builder.AppendLine(string.IsNullOrEmpty(
                    audit.TopologyScaleSearchMode)
                ? "none"
                : audit.TopologyScaleSearchMode);
            builder.Append("finalTrigger={");
            builder.Append(string.IsNullOrEmpty(
                    audit.TopologyScaleSearchTriggerEvidence)
                ? "none"
                : audit.TopologyScaleSearchTriggerEvidence);
            builder.AppendLine("}");
            builder.Append("topologyLinked={");
            builder.Append(string.IsNullOrEmpty(
                    audit.TopologyScaleSearchTopologyLinkedEvidence)
                ? "none"
                : audit.TopologyScaleSearchTopologyLinkedEvidence);
            builder.AppendLine("}");
            builder.Append("trialBaseState=");
            builder.AppendLine(audit.TopologyScaleSearchBasePass >= 0
                ? "topologyClean:" +
                    audit.TopologyScaleSearchBasePass.ToString()
                : "none");
            builder.Append("failedStateScalesReused=");
            builder.AppendLine(
                audit.TopologyScaleSearchFailedStateScalesReused
                    .ToString());
            builder.Append("finalRetreatEdges={");
            builder.Append(string.IsNullOrEmpty(
                    audit.TopologyScaleSearchClusterEvidence)
                ? "none"
                : audit.TopologyScaleSearchClusterEvidence);
            builder.AppendLine("}");
            builder.Append("finalProtectedEdges={");
            builder.Append(string.IsNullOrEmpty(
                    audit.TopologyScaleSearchProtectedEvidence)
                ? "none"
                : audit.TopologyScaleSearchProtectedEvidence);
            builder.AppendLine("}");
            builder.Append("finalActiveSearchFailure={stage:");
            builder.Append(string.IsNullOrEmpty(
                    audit.ActiveSearchFailureStage)
                ? "none"
                : audit.ActiveSearchFailureStage);
            builder.Append(",cause:");
            builder.Append(string.IsNullOrEmpty(
                    audit.ActiveSearchFailureCause)
                ? "none"
                : audit.ActiveSearchFailureCause);
            builder.Append(",evidence:{");
            builder.Append(string.IsNullOrEmpty(
                    audit.ActiveSearchFailureEvidence)
                ? "none"
                : audit.ActiveSearchFailureEvidence);
            builder.AppendLine("}}");
            builder.Append("committedFactor=");
            builder.AppendLine(
                audit.TopologyScaleSearchCommittedFactor >= 0f
                    ? audit.TopologyScaleSearchCommittedFactor
                        .ToString("G9")
                    : "none");
            builder.Append("highestFullyValidFactor=");
            builder.AppendLine(
                audit.TopologyScaleSearchHighestValidFactor >= 0f
                    ? audit.TopologyScaleSearchHighestValidFactor
                        .ToString("G9")
                    : "none");
            builder.Append("collateralChanged={");
            builder.Append(string.IsNullOrEmpty(
                    audit.TopologyScaleSearchCollateralChangedEvidence)
                ? "none"
                : audit.TopologyScaleSearchCollateralChangedEvidence);
            builder.AppendLine("}");
            builder.Append("fallbackState=");
            builder.AppendLine(
                audit.TopologyScaleSearchUnresolved == 1 &&
                audit.TopologyScaleSearchBasePass >= 0
                    ? "topologyClean:" +
                        audit.TopologyScaleSearchBasePass.ToString()
                    : "none");
            builder.Append("unresolved=");
            builder.AppendLine(
                audit.TopologyScaleSearchUnresolved.ToString());
            if (audit.TopologyScaleTrials == null ||
                audit.TopologyScaleTrials.Count == 0)
            {
                builder.AppendLine("trials=none");
                return;
            }
            int matchingTrialCount = 0;
            for (int trialIndex = 0;
                 trialIndex < audit.TopologyScaleTrials.Count;
                 trialIndex++)
            {
                PlaneCutTopologyScaleTrialRecord trial =
                    audit.TopologyScaleTrials[trialIndex];
                if (string.IsNullOrEmpty(searchModeFilter) ||
                    string.Equals(
                        trial.SearchMode,
                        searchModeFilter,
                        StringComparison.Ordinal))
                {
                    matchingTrialCount++;
                }
            }
            builder.Append("trials=");
            builder.AppendLine(matchingTrialCount.ToString());
            for (int trialIndex = 0;
                 trialIndex < audit.TopologyScaleTrials.Count;
                 trialIndex++)
            {
                PlaneCutTopologyScaleTrialRecord trial =
                    audit.TopologyScaleTrials[trialIndex];
                if (!string.IsNullOrEmpty(searchModeFilter) &&
                    !string.Equals(
                        trial.SearchMode,
                        searchModeFilter,
                        StringComparison.Ordinal))
                {
                    continue;
                }
                builder.AppendLine(FormatPlaneCutTopologyScaleTrial(
                    trial,
                    true));
            }
        }

        private static string BuildPlaneCutDetailedTelemetry(
            PlaneCutBevelAuditResult audit,
            bool cornerSolutionValid,
            string cornerBlocker)
        {
            StringBuilder builder = new StringBuilder(8192);
            builder.AppendLine(
                "GeneratedMass all-edge bevel rebuild telemetry");
            builder.AppendLine("mode=edge-plane-shell");
            builder.Append("cornerSolutionValid=");
            builder.AppendLine(cornerSolutionValid ? "1" : "0");
            builder.Append("cornerTrace=");
            builder.AppendLine(string.IsNullOrEmpty(cornerBlocker)
                ? "none"
                : cornerBlocker);
            builder.Append("primaryFailure=");
            builder.AppendLine(FormatPlaneCutPrimaryFailure(audit));
            builder.AppendLine();
            builder.AppendLine("[Evaluation Summary]");
            builder.AppendLine(FormatPlaneCutBevelAuditFields(audit));
            builder.AppendLine();
            builder.AppendLine("[Edge Coverage Summary]");
            builder.AppendLine(FormatEdgeWearCoverageSummary(
                audit.CoverageAudit));
            builder.AppendLine();
            builder.AppendLine("[Macro Width Variation]");
            builder.AppendLine(FormatEdgeWearMacroVariationSummary(
                audit.CoverageAudit));
            builder.AppendLine();
            builder.AppendLine("[Micro Topology Normalization]");
            builder.AppendLine(FormatEdgeWearMicroTopologyNormalization(
                audit.CoverageAudit));
            builder.AppendLine();
            builder.AppendLine("[Artistic Selection Audit]");
            AppendEdgeWearArtisticSelectionAudit(
                builder,
                audit.CoverageAudit);
            builder.AppendLine();
            builder.AppendLine("[Viability Exclusion Summary]");
            builder.AppendLine(FormatEdgeWearViabilityExclusionSummary(
                audit.CoverageAudit,
                true));
            builder.AppendLine();
            builder.AppendLine("[Coexistence Viability Closure]");
            builder.AppendLine(FormatEdgeWearCoexistenceSummary(audit, true));
            builder.AppendLine();
            builder.AppendLine("[Coexistence Conflict-Directed Search]");
            AppendPlaneCutCoexistenceSearchStates(builder, audit);
            builder.AppendLine();
            builder.AppendLine("[Locality Cache Contract]");
            builder.AppendLine(FormatEdgeWearLocalityCacheContract(
                audit.CoverageAudit));
            builder.AppendLine();
            builder.AppendLine("[Edge Viability Preflight]");
            AppendEdgeWearViabilityPreflight(
                builder,
                audit.CoverageAudit);
            builder.AppendLine();
            builder.AppendLine("[Edge Lifecycle]");
            AppendEdgeWearCoverageLifecycle(
                builder,
                audit.CoverageAudit);
            builder.AppendLine();
            builder.AppendLine("[Transactional Solver States]");
            builder.Append("latestAttempted=");
            builder.AppendLine(FormatPlaneCutSolverTransactionState(
                audit.LatestAttemptedState));
            builder.Append("latestBandClean=");
            builder.AppendLine(FormatPlaneCutSolverTransactionState(
                audit.LatestBandCleanState));
            builder.Append("latestTopologyClean=");
            builder.AppendLine(FormatPlaneCutSolverTransactionState(
                audit.LatestTopologyCleanState));
            builder.Append("latestCertified=");
            builder.AppendLine(FormatPlaneCutSolverTransactionState(
                audit.LatestCertifiedState));
            builder.AppendLine();
            builder.AppendLine("[Retry Failure Dossiers]");
            AppendPlaneCutRetryFailureDossiers(builder, audit);
            builder.AppendLine();
            builder.AppendLine("[Conflict Width Reduction]");
            AppendPlaneCutConflictWidthReductions(builder, audit);
            builder.AppendLine();
            builder.AppendLine("[Direct Foreign Band-Plane Retreat Search]");
            AppendPlaneCutTopologyScaleTrials(
                builder,
                audit,
                "direct-foreign-band-plane-retreat");
            builder.AppendLine();
            builder.AppendLine("[Dual-Endpoint Foreign-Plane Retreat Search]");
            AppendPlaneCutTopologyScaleTrials(
                builder,
                audit,
                "dual-endpoint-foreign-plane-retreat");
            builder.AppendLine();
            builder.AppendLine("[T-Junction Failures]");
            builder.Append("firstStage=");
            builder.AppendLine(string.IsNullOrEmpty(
                    audit.FirstTJunctionStage)
                ? "none"
                : audit.FirstTJunctionStage);
            builder.Append("count=");
            builder.AppendLine((audit.TJunctionFailures == null
                ? 0
                : audit.TJunctionFailures.Count).ToString());
            if (audit.TJunctionFailures != null)
            {
                for (int index = 0;
                     index < audit.TJunctionFailures.Count;
                     index++)
                {
                    builder.Append(index);
                    builder.Append(':');
                    builder.AppendLine(FormatPlaneCutTJunctionFailure(
                        audit.TJunctionFailures[index],
                        true));
                }
            }
            builder.AppendLine();
            builder.AppendLine("[Locality Deferrals]");
            builder.Append("count=");
            builder.AppendLine((audit.LocalityDeferrals == null
                ? 0
                : audit.LocalityDeferrals.Count).ToString());
            if (audit.LocalityDeferrals != null)
            {
                for (int index = 0;
                     index < audit.LocalityDeferrals.Count;
                     index++)
                {
                    builder.Append(index);
                    builder.Append(':');
                    builder.AppendLine(FormatPlaneCutLocalityDeferral(
                        audit.LocalityDeferrals[index],
                        true));
                }
            }
            builder.AppendLine();
            builder.AppendLine("[Numerical Repairs]");
            builder.AppendLine(FormatPlaneCutNumericalRepairs(
                audit.NumericalRepairs));
            builder.AppendLine();
            builder.AppendLine("[Strict Intersection Contract]");
            AppendPlaneCutNumericalRepairDossier(
                builder,
                audit.NumericalRepairs);
            builder.AppendLine();
            builder.AppendLine("[Stage Timeline]");
            builder.AppendLine(FormatPlaneCutStageSnapshot(
                audit.StagePlaneConstruction));
            builder.AppendLine(FormatPlaneCutStageSnapshot(
                audit.StageSanitized));
            builder.AppendLine(FormatPlaneCutStageSnapshot(
                audit.StageWelded));
            builder.AppendLine(FormatPlaneCutStageSnapshot(
                audit.StageConformed));
            builder.AppendLine(FormatPlaneCutStageSnapshot(
                audit.StageSeamRepaired));
            builder.AppendLine(FormatPlaneCutStageSnapshot(
                audit.StageFinalCertification));
            builder.Append("firstOpenEdgeStage=");
            builder.AppendLine(string.IsNullOrEmpty(audit.FirstOpenEdgeStage)
                ? "none"
                : audit.FirstOpenEdgeStage);
            builder.Append("firstTJunctionStage=");
            builder.AppendLine(string.IsNullOrEmpty(
                    audit.FirstTJunctionStage)
                ? "none"
                : audit.FirstTJunctionStage);
            builder.Append("firstNonPlanarStage=");
            builder.AppendLine(string.IsNullOrEmpty(
                    audit.FirstNonPlanarStage)
                ? "none"
                : audit.FirstNonPlanarStage);
            builder.AppendLine();
            builder.AppendLine("[Face Quality Failures]");
            builder.Append("count=");
            builder.AppendLine((audit.FaceQualityFailures == null
                ? 0
                : audit.FaceQualityFailures.Count).ToString());
            if (audit.FaceQualityFailures != null)
            {
                for (int index = 0;
                     index < audit.FaceQualityFailures.Count;
                     index++)
                {
                    builder.Append(index);
                    builder.Append(':');
                    builder.AppendLine(FormatPlaneCutFaceFailure(
                        audit.FaceQualityFailures[index],
                        true));
                }
            }
            builder.AppendLine();
            builder.AppendLine("[Open Edge Failures]");
            builder.Append("count=");
            builder.AppendLine((audit.OpenEdgeFailures == null
                ? 0
                : audit.OpenEdgeFailures.Count).ToString());
            if (audit.OpenEdgeFailures != null)
            {
                for (int index = 0;
                     index < audit.OpenEdgeFailures.Count;
                     index++)
                {
                    builder.Append(index);
                    builder.Append(':');
                    builder.AppendLine(FormatPlaneCutOpenEdgeFailure(
                        audit.OpenEdgeFailures[index],
                        true));
                }
            }
            builder.AppendLine();
            builder.AppendLine("[Preparation Movement]");
            builder.Append("boundaryConformityTouched=");
            builder.AppendLine(FormatPlaneCutStringSet(
                audit.BoundaryConformityTouchedFaces));
            builder.Append("seamRepairTouched=");
            builder.AppendLine(FormatPlaneCutStringSet(
                audit.SeamRepairTouchedFaces));
            builder.Append("seamRepairMovement=");
            builder.AppendLine(FormatPlaneCutFloatDictionary(
                audit.SeamRepairMaximumMovementByIdentity));
            builder.AppendLine();
            builder.AppendLine("[Geometry Commit]");
            builder.AppendLine("geometryCommit=disabled");
            return builder.ToString();
        }

        private static string FormatPlaneCutStringSet(
            HashSet<string> values)
        {
            if (values == null || values.Count == 0)
            {
                return "none";
            }
            List<string> ordered = new List<string>(values);
            ordered.Sort(StringComparer.Ordinal);
            return string.Join("/", ordered);
        }

        private static string FormatPlaneCutFloatDictionary(
            Dictionary<string, float> values)
        {
            if (values == null || values.Count == 0)
            {
                return "none";
            }
            List<string> keys = new List<string>(values.Keys);
            keys.Sort(StringComparer.Ordinal);
            StringBuilder builder = new StringBuilder();
            for (int index = 0; index < keys.Count; index++)
            {
                if (index > 0)
                {
                    builder.Append('/');
                }
                string key = keys[index];
                builder.Append(key);
                builder.Append('=');
                builder.Append(values[key].ToString("G9"));
            }
            return builder.ToString();
        }

        private enum EdgeWearArtisticBinMetric
        {
            LengthScore,
            DihedralDegrees,
            EdgeAxisVertical01,
            SilhouettePotential,
            LocalDensity01,
            SharedVertexCrowding
        }

        private sealed class EdgeWearArtisticAuditSummary
        {
            public int GeometricCount;
            public int EligibleCount;
            public int FilteredCount;
            public int SelectedCount;
            public int ShortFilteredCount;
            public int ShallowFilteredCount;
            public int BaseFilteredCount;
            public int OtherFilteredCount;
            public float SelectionThreshold;
            public float ScoreMinimum;
            public float ScoreMedian;
            public float ScoreMaximum;
            public float SelectedScoreMinimum;
            public float SelectedScoreMedian;
            public float SelectedScoreMaximum;
            public float FilteredScoreMinimum;
            public float FilteredScoreMedian;
            public float FilteredScoreMaximum;
            public string LengthBins = string.Empty;
            public string DihedralBins = string.Empty;
            public string OrientationBins = string.Empty;
            public string SilhouetteBins = string.Empty;
            public string LocalDensityBins = string.Empty;
            public string CrowdingBins = string.Empty;
        }

#if UNITY_EDITOR
        private static void PopulateEdgeWearArtisticEdgeRecords(
            EdgeWearBatchAuditCaseResult result,
            EdgeWearCoverageAudit coverage)
        {
            if (result == null || coverage == null ||
                coverage.Records == null)
            {
                if (result != null)
                {
                    result.ArtisticEdges =
                        Array.Empty<EdgeWearArtisticEdgeAuditRecord>();
                }
                return;
            }

            List<EdgeWearArtisticEdgeAuditRecord> records =
                new List<EdgeWearArtisticEdgeAuditRecord>(
                    coverage.Records.Count);
            for (int recordIndex = 0;
                 recordIndex < coverage.Records.Count;
                 recordIndex++)
            {
                EdgeWearEdgeLifecycleRecord source =
                    coverage.Records[recordIndex];
                EdgeWearEdgeViabilityRecord viability = source.Viability;
                EdgeWearArtisticEdgeAuditRecord target =
                    new EdgeWearArtisticEdgeAuditRecord
                    {
                        SourceEdgeIndex =
                            ResolveEdgeWearDisplaySourceEdgeIndex(
                                coverage,
                                source),
                        CandidateIndex = source.CandidateIndex,
                        Start = source.Start,
                        End = source.End,
                        Midpoint = source.Midpoint,
                        OwnerNormalA = source.OwnerNormalA,
                        OwnerNormalB = source.OwnerNormalB,
                        BevelNormal = source.BevelNormal,
                        FaceA = source.FaceA,
                        FaceB = source.FaceB,
                        FaceCount = source.FaceCount,
                        Length = source.Length,
                        DihedralDegrees = source.DihedralDegrees,
                        Vertical01 = source.Vertical01,
                        Classification = source.Classification.ToString(),
                        CoincidentBoundarySeamReconciled =
                            source.CoincidentBoundarySeamReconciled ? 1 : 0,
                        MicroTopologySuppressed =
                            source.MicroTopologySuppressed ? 1 : 0,
                        MicroTopologyGeneratedTransition =
                            source.MicroTopologyGeneratedTransition ? 1 : 0,
                        StructuralEligible =
                            source.StructuralEligible ? 1 : 0,
                        GeometricEligible = source.GeometricEligible ? 1 : 0,
                        CoexistenceEligible =
                            source.CoexistenceEligible ? 1 : 0,
                        ArtisticEligible = source.ArtisticEligible ? 1 : 0,
                        ArtisticLengthEligible =
                            source.ArtisticLengthEligible ? 1 : 0,
                        ArtisticAngleEligible =
                            source.ArtisticAngleEligible ? 1 : 0,
                        ArtisticBaseEligible =
                            source.ArtisticBaseEligible ? 1 : 0,
                        ArtisticFilterReason =
                            source.ArtisticFilterReason ?? string.Empty,
                        CandidateReason =
                            source.CandidateReason ?? string.Empty,
                        FinalReason = source.FinalReason ?? string.Empty,
                        Score = source.Score,
                        ArtisticMinimumLength = source.ArtisticMinimumLength,
                        ArtisticLengthScore = source.ArtisticLengthScore,
                        ArtisticAngleScore = source.ArtisticAngleScore,
                        ArtisticRandomScore = source.ArtisticRandomScore,
                        ArtisticBaseSuppression =
                            source.ArtisticBaseSuppression,
                        ArtisticUpwardEdgeBoost =
                            source.ArtisticUpwardEdgeBoost,
                        ArtisticCharacterBoost =
                            source.ArtisticCharacterBoost,
                        ArtisticEdgeAxisVertical01 =
                            source.ArtisticEdgeAxisVertical01,
                        ArtisticEdgeAxisAbsX = source.ArtisticEdgeAxisAbsX,
                        ArtisticEdgeAxisAbsY = source.ArtisticEdgeAxisAbsY,
                        ArtisticEdgeAxisAbsZ = source.ArtisticEdgeAxisAbsZ,
                        ArtisticSilhouettePotential =
                            source.ArtisticSilhouettePotential,
                        ArtisticFeasibleWidthFraction =
                            source.ArtisticFeasibleWidthFraction,
                        ArtisticSolvedWidthFraction =
                            source.ArtisticSolvedWidthFraction,
                        ArtisticLocalDensity01 =
                            source.ArtisticLocalDensity01,
                        ArtisticSharedVertexDegreeA =
                            source.ArtisticSharedVertexDegreeA,
                        ArtisticSharedVertexDegreeB =
                            source.ArtisticSharedVertexDegreeB,
                        ArtisticSelectionRank =
                            source.ArtisticSelectionRank,
                        ArtisticSelectionThreshold =
                            source.ArtisticSelectionThreshold,
                        ArtisticSelectionDelta =
                            source.ArtisticSelectionDelta,
                        ArtisticDeterministicVariation =
                            source.ArtisticDeterministicVariation,
                        ArtisticStrength = source.ArtisticStrength,
                        ArtisticDepthMultiplier =
                            source.ArtisticDepthMultiplier,
                        SolvedWidth = source.SolvedWidth,
                        MaterializedWidth = source.MaterializedWidth,
                        MaterializedWidthScale =
                            source.MaterializedWidthScale,
                        WidthReduced = source.WidthReduced ? 1 : 0,
                        Candidate = source.Candidate ? 1 : 0,
                        Selected = source.Selected ? 1 : 0,
                        WidthInactive = source.WidthInactive ? 1 : 0,
                        Active = source.Active ? 1 : 0,
                        AttemptedBuilt = source.AttemptedBuilt ? 1 : 0,
                        CertifiedBuilt = source.Built ? 1 : 0,
                        TrialRejected = source.TrialRejected ? 1 : 0,
                        Deferred = source.Deferred ? 1 : 0,
                        Rejected = source.Rejected ? 1 : 0
                    };
                if (viability != null)
                {
                    target.MacroBaseRequestedWidth =
                        viability.BaseRequestedWidth;
                    target.MacroIdentity01 = viability.MacroIdentity01;
                    target.MacroSampledMultiplier =
                        viability.MacroSampledMultiplier;
                    target.MacroEffectiveMultiplier =
                        viability.MacroEffectiveMultiplier;
                    target.MacroMinimumStyleClamped =
                        viability.MacroMinimumStyleClamped ? 1 : 0;
                    target.RequestedWidth = viability.RequestedWidth;
                    target.RequiredFootprintLength =
                        viability.RequiredFootprintLength;
                    target.LengthToWidthRatio =
                        viability.LengthToWidthRatio;
                    target.LocalityRetainPlaneFloor =
                        viability.LocalityRetainPlaneFloor;
                    target.LocalityRemovalPlaneCeiling =
                        viability.LocalityRemovalPlaneCeiling;
                    target.LocalityFeasibleMargin =
                        viability.LocalityFeasibleMargin;
                    target.LocalityGuardMargin =
                        viability.LocalityGuardMargin;
                    target.LocalityMinimumRemoval =
                        viability.LocalityMinimumRemoval;
                    target.LocalityLimitingVertex =
                        viability.LocalityLimitingVertex;
                    target.LocalityLimitingPosition =
                        viability.LocalityLimitingPosition;
                    target.MaximumLocallyFeasibleWidth =
                        viability.MaximumLocallyFeasibleWidth;
                    target.FeasibleWidthFraction =
                        viability.FeasibleWidthFraction;
                    target.IsolatedSucceeded =
                        viability.IsolatedSucceeded ? 1 : 0;
                    target.IsolatedWidthAttemptCount =
                        viability.IsolatedWidthAttemptCount;
                    target.IsolatedLastAttemptedWidth =
                        viability.IsolatedLastAttemptedWidth;
                    target.IsolatedMaximumCertifiedWidth =
                        viability.IsolatedMaximumCertifiedWidth;
                    target.IsolatedMaximumCertifiedWidthFraction =
                        viability.IsolatedMaximumCertifiedWidthFraction;
                    target.EndpointConsumptionA =
                        viability.EndpointConsumptionA;
                    target.EndpointConsumptionB =
                        viability.EndpointConsumptionB;
                    target.RemainingCentralSpan =
                        viability.RemainingCentralSpan;
                    target.MinimumCentralSpan =
                        viability.MinimumCentralSpan;
                    target.IsolatedOpenEdgeCount =
                        viability.IsolatedOpenEdgeCount;
                    target.IsolatedNonManifoldEdgeCount =
                        viability.IsolatedNonManifoldEdgeCount;
                    target.IsolatedTJunctionCount =
                        viability.IsolatedTJunctionCount;
                    target.IsolatedInvalidFaceCount =
                        viability.IsolatedInvalidFaceCount;
                    target.IsolatedDiagnostic =
                        viability.IsolatedDiagnostic ?? string.Empty;
                    target.ViabilityFailureReason =
                        viability.FailureReason ?? string.Empty;
                }
                records.Add(target);
            }

            records.Sort((left, right) =>
            {
                int leftIndex = left.SourceEdgeIndex;
                int rightIndex = right.SourceEdgeIndex;
                if (leftIndex != rightIndex)
                {
                    return leftIndex.CompareTo(rightIndex);
                }
                int start = left.Start.x.CompareTo(right.Start.x);
                if (start != 0)
                {
                    return start;
                }
                start = left.Start.y.CompareTo(right.Start.y);
                return start != 0
                    ? start
                    : left.Start.z.CompareTo(right.Start.z);
            });
            result.ArtisticEdges = records.ToArray();
        }

        private static void PopulateEdgeWearMacroAuditResult(
            EdgeWearBatchAuditCaseResult result,
            EdgeWearCoverageAudit coverage)
        {
            if (result == null || coverage == null ||
                coverage.Records == null)
            {
                return;
            }

            result.EdgeWearMacroVariationCoverage =
                coverage.MacroVariationCoverage;
            result.EdgeWearMacroVariation = coverage.MacroVariation;
            result.MacroBaseRequestedWidth =
                coverage.MacroBaseRequestedWidth;
            List<EdgeWearEdgeLifecycleRecord> ordered =
                new List<EdgeWearEdgeLifecycleRecord>(coverage.Records);
            ordered.Sort((left, right) =>
                ResolveEdgeWearDisplaySourceEdgeIndex(coverage, left).
                    CompareTo(
                        ResolveEdgeWearDisplaySourceEdgeIndex(
                            coverage,
                            right)));
            List<float> multipliers = new List<float>();
            List<float> widths = new List<float>();
            StringBuilder signature = new StringBuilder(1024);
            for (int recordIndex = 0;
                 recordIndex < ordered.Count;
                 recordIndex++)
            {
                EdgeWearEdgeLifecycleRecord lifecycle =
                    ordered[recordIndex];
                EdgeWearEdgeViabilityRecord viability =
                    lifecycle.Viability;
                if (viability == null ||
                    lifecycle.MicroTopologySuppressed ||
                    lifecycle.MicroTopologyGeneratedTransition ||
                    viability.BaseRequestedWidth <= PointMergeDistance)
                {
                    continue;
                }

                multipliers.Add(viability.MacroEffectiveMultiplier);
                widths.Add(viability.RequestedWidth);
                result.MacroEvaluatedEdgeCount++;
                if (viability.MacroVariationParticipates)
                {
                    result.MacroParticipantEdgeCount++;
                }
                if (viability.MacroEffectiveMultiplier < 0.999999f)
                {
                    result.MacroVariedEdgeCount++;
                }
                if (viability.MacroMinimumStyleClamped)
                {
                    result.MacroMinimumStyleClampedEdgeCount++;
                }
                if (viability.MaximumLocallyFeasibleWidth >
                        PointMergeDistance &&
                    viability.MaximumLocallyFeasibleWidth +
                        PointMergeDistance < viability.RequestedWidth)
                {
                    result.MacroFeasibilityReducedEdgeCount++;
                }

                signature.Append(
                    ResolveEdgeWearDisplaySourceEdgeIndex(
                        coverage,
                        lifecycle));
                signature.Append(':');
                signature.Append(
                    viability.MacroParticipationIdentity01.ToString(
                        "R",
                        CultureInfo.InvariantCulture));
                signature.Append(':');
                signature.Append(
                    viability.MacroVariationParticipates ? '1' : '0');
                signature.Append(':');
                signature.Append(viability.MacroIdentity01.ToString(
                    "R",
                    CultureInfo.InvariantCulture));
                signature.Append(':');
                signature.Append(
                    viability.MacroSampledMultiplier.ToString(
                        "R",
                        CultureInfo.InvariantCulture));
                signature.Append(':');
                signature.Append(
                    ResolveEdgeWearMacroAnglePermission(
                        lifecycle.DihedralDegrees).ToString(
                            "R",
                            CultureInfo.InvariantCulture));
                signature.Append(':');
                signature.Append(
                    viability.MacroEffectiveMultiplier.ToString(
                        "R",
                        CultureInfo.InvariantCulture));
                signature.Append(':');
                signature.Append(viability.RequestedWidth.ToString(
                    "R",
                    CultureInfo.InvariantCulture));
                signature.Append(';');
            }

            multipliers.Sort();
            widths.Sort();
            if (multipliers.Count > 0)
            {
                result.MacroMultiplierMinimum = multipliers[0];
                result.MacroMultiplierMedian = ResolveEdgeWearSortedMedian(
                    multipliers);
                result.MacroMultiplierMaximum =
                    multipliers[multipliers.Count - 1];
                result.MacroRequestedWidthMinimum = widths[0];
                result.MacroRequestedWidthMedian = ResolveEdgeWearSortedMedian(
                    widths);
                result.MacroRequestedWidthMaximum =
                    widths[widths.Count - 1];
            }
            result.MacroSignature = signature.ToString();
        }

        private static void PopulateEdgeWearArtisticAuditResult(
            EdgeWearBatchAuditCaseResult result,
            EdgeWearCoverageAudit coverage)
        {
            if (result == null)
            {
                return;
            }
            EdgeWearArtisticAuditSummary summary =
                BuildEdgeWearArtisticAuditSummary(coverage);
            result.ArtisticFilteredCount = summary.FilteredCount;
            result.ArtisticShortFilteredCount =
                summary.ShortFilteredCount;
            result.ArtisticShallowFilteredCount =
                summary.ShallowFilteredCount;
            result.ArtisticBaseFilteredCount = summary.BaseFilteredCount;
            result.ArtisticOtherFilteredCount = summary.OtherFilteredCount;
            result.ArtisticSelectionThreshold = summary.SelectionThreshold;
            result.ArtisticScoreMinimum = summary.ScoreMinimum;
            result.ArtisticScoreMedian = summary.ScoreMedian;
            result.ArtisticScoreMaximum = summary.ScoreMaximum;
            result.ArtisticSelectedScoreMinimum =
                summary.SelectedScoreMinimum;
            result.ArtisticSelectedScoreMedian =
                summary.SelectedScoreMedian;
            result.ArtisticSelectedScoreMaximum =
                summary.SelectedScoreMaximum;
            result.ArtisticFilteredScoreMinimum =
                summary.FilteredScoreMinimum;
            result.ArtisticFilteredScoreMedian =
                summary.FilteredScoreMedian;
            result.ArtisticFilteredScoreMaximum =
                summary.FilteredScoreMaximum;
            result.ArtisticLengthBins = summary.LengthBins;
            result.ArtisticDihedralBins = summary.DihedralBins;
            result.ArtisticOrientationBins = summary.OrientationBins;
            result.ArtisticSilhouetteBins = summary.SilhouetteBins;
            result.ArtisticLocalDensityBins = summary.LocalDensityBins;
            result.ArtisticCrowdingBins = summary.CrowdingBins;
        }
#endif

        private static EdgeWearArtisticAuditSummary
            BuildEdgeWearArtisticAuditSummary(
                EdgeWearCoverageAudit audit)
        {
            EdgeWearArtisticAuditSummary summary =
                new EdgeWearArtisticAuditSummary();
            if (audit == null || audit.Records == null)
            {
                return summary;
            }

            List<EdgeWearEdgeLifecycleRecord> geometric =
                new List<EdgeWearEdgeLifecycleRecord>();
            List<float> allScores = new List<float>();
            List<float> selectedScores = new List<float>();
            List<float> filteredScores = new List<float>();
            for (int recordIndex = 0;
                 recordIndex < audit.Records.Count;
                 recordIndex++)
            {
                EdgeWearEdgeLifecycleRecord record =
                    audit.Records[recordIndex];
                if (!record.GeometricEligible)
                {
                    continue;
                }
                geometric.Add(record);
                allScores.Add(record.Score);
                if (record.ArtisticEligible)
                {
                    summary.EligibleCount++;
                }
                else
                {
                    summary.FilteredCount++;
                    filteredScores.Add(record.Score);
                    switch (record.ArtisticFilterReason)
                    {
                        case "artistically-short-edge":
                            summary.ShortFilteredCount++;
                            break;
                        case "artistically-shallow-edge":
                            summary.ShallowFilteredCount++;
                            break;
                        case "artistically-base-suppressed":
                            summary.BaseFilteredCount++;
                            break;
                        default:
                            summary.OtherFilteredCount++;
                            break;
                    }
                }
                if (record.Selected)
                {
                    summary.SelectedCount++;
                    selectedScores.Add(record.Score);
                }
            }

            summary.GeometricCount = geometric.Count;
            summary.SelectionThreshold = audit.ArtisticSelectionThreshold;
            ResolveEdgeWearArtisticScoreRange(
                allScores,
                out summary.ScoreMinimum,
                out summary.ScoreMedian,
                out summary.ScoreMaximum);
            ResolveEdgeWearArtisticScoreRange(
                selectedScores,
                out summary.SelectedScoreMinimum,
                out summary.SelectedScoreMedian,
                out summary.SelectedScoreMaximum);
            ResolveEdgeWearArtisticScoreRange(
                filteredScores,
                out summary.FilteredScoreMinimum,
                out summary.FilteredScoreMedian,
                out summary.FilteredScoreMaximum);
            summary.LengthBins = BuildEdgeWearArtisticBinSummary(
                geometric,
                EdgeWearArtisticBinMetric.LengthScore);
            summary.DihedralBins = BuildEdgeWearArtisticBinSummary(
                geometric,
                EdgeWearArtisticBinMetric.DihedralDegrees);
            summary.OrientationBins = BuildEdgeWearArtisticBinSummary(
                geometric,
                EdgeWearArtisticBinMetric.EdgeAxisVertical01);
            summary.SilhouetteBins = BuildEdgeWearArtisticBinSummary(
                geometric,
                EdgeWearArtisticBinMetric.SilhouettePotential);
            summary.LocalDensityBins = BuildEdgeWearArtisticBinSummary(
                geometric,
                EdgeWearArtisticBinMetric.LocalDensity01);
            summary.CrowdingBins = BuildEdgeWearArtisticBinSummary(
                geometric,
                EdgeWearArtisticBinMetric.SharedVertexCrowding);
            return summary;
        }

        private static void ResolveEdgeWearArtisticScoreRange(
            List<float> values,
            out float minimum,
            out float median,
            out float maximum)
        {
            minimum = 0f;
            median = 0f;
            maximum = 0f;
            if (values == null || values.Count == 0)
            {
                return;
            }
            values.Sort();
            minimum = values[0];
            maximum = values[values.Count - 1];
            int middle = values.Count / 2;
            median = values.Count % 2 == 0
                ? (values[middle - 1] + values[middle]) * 0.5f
                : values[middle];
        }

        private static string BuildEdgeWearArtisticBinSummary(
            List<EdgeWearEdgeLifecycleRecord> records,
            EdgeWearArtisticBinMetric metric)
        {
            int[,] counts = new int[4, 3];
            if (records != null)
            {
                for (int recordIndex = 0;
                     recordIndex < records.Count;
                     recordIndex++)
                {
                    EdgeWearEdgeLifecycleRecord record = records[recordIndex];
                    int bin = ResolveEdgeWearArtisticBin(record, metric);
                    counts[bin, 0]++;
                    if (record.Selected)
                    {
                        counts[bin, 1]++;
                    }
                    if (!record.ArtisticEligible)
                    {
                        counts[bin, 2]++;
                    }
                }
            }

            string[] labels = metric switch
            {
                EdgeWearArtisticBinMetric.LengthScore =>
                    new[] { "0-.15", ".15-.35", ".35-.65", ".65-1" },
                EdgeWearArtisticBinMetric.DihedralDegrees =>
                    new[] { "15-25", "25-45", "45-70", "70+" },
                EdgeWearArtisticBinMetric.SharedVertexCrowding =>
                    new[] { "0", "1", "2", "3+" },
                _ => new[] { "0-.25", ".25-.5", ".5-.75", ".75-1" }
            };
            StringBuilder builder = new StringBuilder();
            for (int bin = 0; bin < 4; bin++)
            {
                if (bin > 0)
                {
                    builder.Append(';');
                }
                builder.Append(labels[bin]);
                builder.Append(':');
                builder.Append(counts[bin, 0]);
                builder.Append('/');
                builder.Append(counts[bin, 1]);
                builder.Append('/');
                builder.Append(counts[bin, 2]);
            }
            return builder.ToString();
        }

        private static int ResolveEdgeWearArtisticBin(
            EdgeWearEdgeLifecycleRecord record,
            EdgeWearArtisticBinMetric metric)
        {
            if (metric == EdgeWearArtisticBinMetric.SharedVertexCrowding)
            {
                int crowding = Mathf.Max(
                    record.ArtisticSharedVertexDegreeA,
                    record.ArtisticSharedVertexDegreeB);
                return Mathf.Clamp(crowding, 0, 3);
            }

            float value = metric switch
            {
                EdgeWearArtisticBinMetric.LengthScore =>
                    record.ArtisticLengthScore,
                EdgeWearArtisticBinMetric.DihedralDegrees =>
                    record.DihedralDegrees,
                EdgeWearArtisticBinMetric.EdgeAxisVertical01 =>
                    record.ArtisticEdgeAxisVertical01,
                EdgeWearArtisticBinMetric.SilhouettePotential =>
                    record.ArtisticSilhouettePotential,
                EdgeWearArtisticBinMetric.LocalDensity01 =>
                    record.ArtisticLocalDensity01,
                _ => 0f
            };
            if (metric == EdgeWearArtisticBinMetric.DihedralDegrees)
            {
                if (value < 25f) return 0;
                if (value < 45f) return 1;
                if (value < 70f) return 2;
                return 3;
            }
            if (metric == EdgeWearArtisticBinMetric.LengthScore)
            {
                if (value < 0.15f) return 0;
                if (value < 0.35f) return 1;
                if (value < 0.65f) return 2;
                return 3;
            }
            if (value < 0.25f) return 0;
            if (value < 0.5f) return 1;
            if (value < 0.75f) return 2;
            return 3;
        }

        private static void AppendEdgeWearArtisticSelectionAudit(
            StringBuilder builder,
            EdgeWearCoverageAudit audit)
        {
            if (builder == null)
            {
                return;
            }
            if (audit == null)
            {
                builder.AppendLine("notCaptured");
                return;
            }
            EdgeWearArtisticAuditSummary summary =
                BuildEdgeWearArtisticAuditSummary(audit);
            builder.Append("policy=");
            builder.AppendLine(audit.RequireAllGeometricCandidates
                ? "all-geometric"
                : "artistic-preview");
            builder.Append("captured=");
            builder.AppendLine(audit.ArtisticAuditCaptured ? "1" : "0");
            builder.Append("geometric/eligible/filtered/selected/target=");
            builder.Append(summary.GeometricCount);
            builder.Append('/');
            builder.Append(summary.EligibleCount);
            builder.Append('/');
            builder.Append(summary.FilteredCount);
            builder.Append('/');
            builder.Append(summary.SelectedCount);
            builder.Append('/');
            builder.AppendLine(audit.ArtisticSelectionTargetCount.ToString());
            builder.Append("filters=short/shallow/base/other:");
            builder.Append(summary.ShortFilteredCount);
            builder.Append('/');
            builder.Append(summary.ShallowFilteredCount);
            builder.Append('/');
            builder.Append(summary.BaseFilteredCount);
            builder.Append('/');
            builder.AppendLine(summary.OtherFilteredCount.ToString());
            builder.Append("selectionThreshold=");
            builder.AppendLine(summary.SelectionThreshold.ToString("G9"));
            builder.Append("scores=all:");
            AppendEdgeWearArtisticScoreRange(builder,
                summary.ScoreMinimum,
                summary.ScoreMedian,
                summary.ScoreMaximum);
            builder.Append(",selected:");
            AppendEdgeWearArtisticScoreRange(builder,
                summary.SelectedScoreMinimum,
                summary.SelectedScoreMedian,
                summary.SelectedScoreMaximum);
            builder.Append(",filtered:");
            AppendEdgeWearArtisticScoreRange(builder,
                summary.FilteredScoreMinimum,
                summary.FilteredScoreMedian,
                summary.FilteredScoreMaximum);
            builder.AppendLine();
            builder.AppendLine(
                "scoreFormula=(angle*0.60+length*0.35+random*0.05)*" +
                "basePriorityFactor*upwardPriorityFactor");
            builder.AppendLine(
                "diagnosticOnlyContextWeights=" +
                "silhouette:0,widthFraction:0,localDensity:0,crowding:0");
            builder.Append("lengthScoreBins(all/selected/filtered)=");
            builder.AppendLine(summary.LengthBins);
            builder.Append("dihedralBins(all/selected/filtered)=");
            builder.AppendLine(summary.DihedralBins);
            builder.Append("edgeAxisVerticalBins(all/selected/filtered)=");
            builder.AppendLine(summary.OrientationBins);
            builder.Append("silhouettePotentialBins(all/selected/filtered)=");
            builder.AppendLine(summary.SilhouetteBins);
            builder.Append("localDensityBins(all/selected/filtered)=");
            builder.AppendLine(summary.LocalDensityBins);
            builder.Append("sharedVertexCrowdingBins(all/selected/filtered)=");
            builder.AppendLine(summary.CrowdingBins);
        }

        private static void AppendEdgeWearArtisticScoreRange(
            StringBuilder builder,
            float minimum,
            float median,
            float maximum)
        {
            builder.Append(minimum.ToString("G9"));
            builder.Append('/');
            builder.Append(median.ToString("G9"));
            builder.Append('/');
            builder.Append(maximum.ToString("G9"));
        }

        private static int ResolveEdgeWearDisplayGraphEdgeIndex(
            EdgeWearCoverageAudit audit,
            int graphEdgeIndex)
        {
            if (graphEdgeIndex < 0 || audit == null ||
                audit.MicroTopologyNormalization == null)
            {
                return graphEdgeIndex;
            }
            EdgeWearMicroTopologyNormalizationResult normalization =
                audit.MicroTopologyNormalization;
            return normalization.
                OriginalSourceEdgeIndexByNormalizedGraphEdge.TryGetValue(
                    graphEdgeIndex,
                    out int originalIndex)
                ? originalIndex
                : normalization.OriginalEdgeCount + graphEdgeIndex;
        }

        private static int ResolveEdgeWearDisplaySourceEdgeIndex(
            EdgeWearCoverageAudit audit,
            EdgeWearEdgeLifecycleRecord record)
        {
            if (record == null)
            {
                return -1;
            }
            if (record.OriginalSourceEdgeIndex >= 0)
            {
                return record.OriginalSourceEdgeIndex;
            }
            if (record.MicroTopologyGeneratedTransition &&
                record.SourceEdgeIndex >= 0 &&
                audit != null &&
                audit.MicroTopologyNormalization != null)
            {
                return audit.MicroTopologyNormalization.OriginalEdgeCount +
                    record.SourceEdgeIndex;
            }
            return record.SourceEdgeIndex;
        }

        private static string FormatEdgeWearMicroTopologyCompact(
            EdgeWearMicroTopologyNormalizationResult normalization)
        {
            if (normalization == null)
            {
                return "notCaptured";
            }
            return (normalization.Applied ? "applied" : "unchanged") +
                ":" + normalization.AppliedComponentCount + "/" +
                normalization.EligibleComponentCount +
                ":suppressed{" +
                    FormatEdgeWearMicroTopologySuppressedIds(
                        normalization) + "}";
        }

        private static string FormatEdgeWearMicroTopologyNormalization(
            EdgeWearCoverageAudit audit)
        {
            EdgeWearMicroTopologyNormalizationResult normalization =
                audit == null ? null : audit.MicroTopologyNormalization;
            if (normalization == null)
            {
                return "notCaptured";
            }
            return "attempted=" + (normalization.Attempted ? "1" : "0") +
                ",applied=" + (normalization.Applied ? "1" : "0") +
                ",seedThreshold=" +
                    normalization.Threshold.ToString("G9") +
                ",componentThreshold=" +
                    normalization.ComponentThreshold.ToString("G9") +
                ",components=" + normalization.AppliedComponentCount + "/" +
                    normalization.EligibleComponentCount +
                ",candidates=" + normalization.CandidateCollapseCount +
                ",vertices=" + normalization.OriginalVertexCount + "/" +
                    normalization.NormalizedVertexCount +
                ",edges=" + normalization.OriginalEdgeCount + "/" +
                    normalization.NormalizedEdgeCount +
                ",faces=" + normalization.OriginalFaceCount + "/" +
                    normalization.NormalizedFaceCount +
                ",suppressed={" +
                    FormatEdgeWearMicroTopologySuppressedIds(
                        normalization) + "}" +
                ",generatedTransitions=" +
                    normalization.GeneratedTransitionKeys.Count +
                ",volume=" + normalization.OriginalVolume.ToString("G12") +
                    "/" + normalization.NormalizedVolume.ToString("G12") +
                ",loss=" + normalization.VolumeLoss.ToString("G12") +
                    "/" +
                    normalization.VolumeLossFraction.ToString("G12") +
                ",elapsedMs=" +
                    normalization.ElapsedMilliseconds.ToString("G9") +
                ",diagnostic={" + normalization.Diagnostic + "}" +
                ",componentEvidence=" +
                    FormatEdgeWearMicroTopologyComponentEvidence(
                        normalization);
        }

        private static string FormatEdgeWearMicroTopologyComponentEvidence(
            EdgeWearMicroTopologyNormalizationResult normalization)
        {
            if (normalization == null ||
                normalization.Components.Count == 0)
            {
                return "[none]";
            }

            StringBuilder builder = new StringBuilder();
            builder.Append('[');
            for (int componentIndex = 0;
                 componentIndex < normalization.Components.Count;
                 componentIndex++)
            {
                if (componentIndex > 0)
                {
                    builder.Append('|');
                }
                EdgeWearMicroTopologyComponentRecord component =
                    normalization.Components[componentIndex];
                builder.Append("component:");
                builder.Append(componentIndex);
                builder.Append(":edges{");
                builder.Append(FormatEdgeWearIndexList(
                    component.EdgeIndices));
                builder.Append("}:seeds{");
                builder.Append(FormatEdgeWearIndexList(
                    component.SeedEdgeIndices));
                builder.Append("}:vertices{");
                builder.Append(FormatEdgeWearIndexList(
                    component.VertexIndices));
                builder.Append("}:diameter:");
                builder.Append(component.Diameter.ToString("G9"));
                builder.Append(":candidateEligible:");
                builder.Append(component.CandidateEligible ? '1' : '0');
                builder.Append(":applied:");
                builder.Append(component.Applied ? '1' : '0');
                builder.Append(":selectedVertex:");
                builder.Append(component.SelectedCanonicalGraphVertexIndex);
                builder.Append(":selectedDisplacement:");
                builder.Append(
                    component.SelectedSquaredDisplacement.ToString("G12"));
                builder.Append(":selectedVolumeLoss:");
                builder.Append(component.SelectedVolumeLoss.ToString("G12"));
                builder.Append(":blocker{");
                builder.Append(string.IsNullOrEmpty(component.Blocker)
                    ? "none"
                    : component.Blocker);
                builder.Append("}:attempts[");
                for (int attemptIndex = 0;
                     attemptIndex < component.Attempts.Count;
                     attemptIndex++)
                {
                    if (attemptIndex > 0)
                    {
                        builder.Append(';');
                    }
                    EdgeWearMicroTopologyCollapseAttemptRecord attempt =
                        component.Attempts[attemptIndex];
                    builder.Append("vertex:");
                    builder.Append(attempt.CanonicalGraphVertexIndex);
                    builder.Append("@position:");
                    builder.Append(FormatPlaneCutVector(
                        attempt.CanonicalPosition));
                    builder.Append(":succeeded:");
                    builder.Append(attempt.Succeeded ? '1' : '0');
                    builder.Append(":displacement:");
                    builder.Append(
                        attempt.SquaredDisplacement.ToString("G12"));
                    builder.Append(":volume:");
                    builder.Append(attempt.NormalizedVolume.ToString("G12"));
                    builder.Append(":loss:");
                    builder.Append(attempt.VolumeLoss.ToString("G12"));
                    builder.Append(":blocker{");
                    builder.Append(string.IsNullOrEmpty(attempt.Blocker)
                        ? "none"
                        : attempt.Blocker);
                    builder.Append('}');
                }
                builder.Append(']');
            }
            builder.Append(']');
            return builder.ToString();
        }

        private static string FormatEdgeWearMicroTopologySuppressedIds(
            EdgeWearMicroTopologyNormalizationResult normalization)
        {
            if (normalization == null ||
                normalization.SuppressedEdges.Count == 0)
            {
                return "none";
            }
            List<int> ids = new List<int>(
                normalization.SuppressedEdges.Count);
            for (int i = 0;
                 i < normalization.SuppressedEdges.Count;
                 i++)
            {
                ids.Add(normalization.SuppressedEdges[i].
                    OriginalSourceEdgeIndex);
            }
            ids.Sort();
            return string.Join("/", ids);
        }

        private static float ResolveEdgeWearSortedMedian(
            List<float> sorted)
        {
            if (sorted == null || sorted.Count == 0)
            {
                return 0f;
            }
            int middle = sorted.Count / 2;
            return (sorted.Count & 1) == 0
                ? (sorted[middle - 1] + sorted[middle]) * 0.5f
                : sorted[middle];
        }

        private static string FormatEdgeWearMacroVariationSummary(
            EdgeWearCoverageAudit audit)
        {
            if (audit == null || audit.Records == null)
            {
                return "notCaptured";
            }

            List<float> multipliers = new List<float>();
            List<float> widths = new List<float>();
            List<float> anglePermissions = new List<float>();
            int participants = 0;
            int varied = 0;
            int minimumStyleClamped = 0;
            int feasibilityReduced = 0;
            for (int recordIndex = 0;
                 recordIndex < audit.Records.Count;
                 recordIndex++)
            {
                EdgeWearEdgeLifecycleRecord lifecycle =
                    audit.Records[recordIndex];
                EdgeWearEdgeViabilityRecord viability =
                    lifecycle.Viability;
                if (viability == null ||
                    lifecycle.MicroTopologySuppressed ||
                    lifecycle.MicroTopologyGeneratedTransition ||
                    viability.BaseRequestedWidth <= PointMergeDistance)
                {
                    continue;
                }

                multipliers.Add(viability.MacroEffectiveMultiplier);
                widths.Add(viability.RequestedWidth);
                anglePermissions.Add(
                    ResolveEdgeWearMacroAnglePermission(
                        lifecycle.DihedralDegrees));
                if (viability.MacroVariationParticipates)
                {
                    participants++;
                }
                if (viability.MacroEffectiveMultiplier < 0.999999f)
                {
                    varied++;
                }
                if (viability.MacroMinimumStyleClamped)
                {
                    minimumStyleClamped++;
                }
                if (viability.MaximumLocallyFeasibleWidth >
                        PointMergeDistance &&
                    viability.MaximumLocallyFeasibleWidth +
                        PointMergeDistance < viability.RequestedWidth)
                {
                    feasibilityReduced++;
                }
            }

            multipliers.Sort();
            widths.Sort();
            anglePermissions.Sort();
            float multiplierMinimum = multipliers.Count > 0
                ? multipliers[0]
                : 1f;
            float multiplierMedian = multipliers.Count > 0
                ? ResolveEdgeWearSortedMedian(multipliers)
                : 1f;
            float multiplierMaximum = multipliers.Count > 0
                ? multipliers[multipliers.Count - 1]
                : 1f;
            float widthMinimum = widths.Count > 0 ? widths[0] : 0f;
            float widthMedian = widths.Count > 0
                ? ResolveEdgeWearSortedMedian(widths)
                : 0f;
            float widthMaximum = widths.Count > 0
                ? widths[widths.Count - 1]
                : 0f;
            float anglePermissionMinimum = anglePermissions.Count > 0
                ? anglePermissions[0]
                : 1f;
            float anglePermissionMedian = anglePermissions.Count > 0
                ? ResolveEdgeWearSortedMedian(anglePermissions)
                : 1f;
            float anglePermissionMaximum = anglePermissions.Count > 0
                ? anglePermissions[anglePermissions.Count - 1]
                : 1f;
            return "policy=canonical-shape-seed-source-edge/dihedral-biased-downward-only" +
                ",shallowAngle=" +
                    EdgeWearMacroShallowAngleDegrees.ToString("G9") +
                ",sharpAngle=" +
                    EdgeWearMacroSharpAngleDegrees.ToString("G9") +
                ",sharpPermission=" +
                    EdgeWearMacroSharpReductionPermission.ToString("G9") +
                ",coverage=" +
                    audit.MacroVariationCoverage.ToString("G9") +
                ",controlStrength=" +
                    audit.MacroVariation.ToString("G9") +
                ",effectiveStrength=" +
                    (Mathf.Clamp01(audit.MacroVariation) *
                     EdgeWearMacroMaximumCertifiedStrength).ToString("G9") +
                ",baseRequestedWidth=" +
                    audit.MacroBaseRequestedWidth.ToString("G9") +
                ",evaluated=" + multipliers.Count +
                ",participants=" + participants +
                ",varied=" + varied +
                ",minimumStyleClamped=" + minimumStyleClamped +
                ",feasibilityReduced=" + feasibilityReduced +
                ",multiplier=" +
                    multiplierMinimum.ToString("G9") + "/" +
                    multiplierMedian.ToString("G9") + "/" +
                    multiplierMaximum.ToString("G9") +
                ",anglePermission=" +
                    anglePermissionMinimum.ToString("G9") + "/" +
                    anglePermissionMedian.ToString("G9") + "/" +
                    anglePermissionMaximum.ToString("G9") +
                ",requestedWidth=" +
                    widthMinimum.ToString("G9") + "/" +
                    widthMedian.ToString("G9") + "/" +
                    widthMaximum.ToString("G9");
        }

        private static string FormatEdgeWearCoverageSummary(
            EdgeWearCoverageAudit audit)
        {
            if (audit == null)
            {
                return "notCaptured";
            }

            RecalculateEdgeWearCoverageAudit(audit);
            return "max=" + (audit.MaximumCoverageMode ? "1" : "0") +
                ",requireAllGeometric=" +
                    (audit.RequireAllGeometricCandidates ? "1" : "0") +
                ",rawSource=" + audit.RawSourceEdgeCount +
                ",source=" + audit.SourceEdgeCount +
                ",microTopology=" +
                    FormatEdgeWearMicroTopologyCompact(
                        audit.MicroTopologyNormalization) +
                ",coincidentSeamPairs=" +
                    audit.CoincidentBoundarySeamPairCount +
                ",graphVertexAliases=" +
                    audit.CoincidentGraphVertexReconciliationCount +
                ",graphSeamPairs=" +
                    audit.CoincidentGraphBoundarySeamPairCount +
                ",collateral=" +
                    audit.BaselineGeometricEligibleCount + "/" +
                    audit.GeometricEligibleCount + "/" +
                    audit.RecoveredGeometricEdgeCount + "/" +
                    audit.CollateralLostEdgeCount + "/" +
                    audit.CollateralChangedEdgeCount + "/" +
                    (audit.CollateralPreservationValid ? "1" : "0") +
                ",structural=" + audit.StructuralEligibleCount +
                ",geometric=" + audit.GeometricEligibleCount +
                ",geometricIneligible=" +
                    audit.GeometricIneligibleCount +
                ",coexistence=" + audit.CoexistenceEligibleCount +
                ",coexistenceIneligible=" +
                    audit.CoexistenceIneligibleCount +
                ",artistic=" + audit.ArtisticEligibleCount +
                ",wouldBeArtisticallyFiltered=" +
                    audit.ArtisticFilteredCount +
                ",artisticAudit=" +
                    (audit.ArtisticAuditCaptured ? "1" : "0") + "/" +
                    audit.ArtisticSelectionTargetCount + "/" +
                    audit.ArtisticSelectionThreshold.ToString("G9") +
                ",candidates=" + audit.CandidateCount +
                ",selected=" + audit.SelectedCount +
                ",widthInactive=" + audit.WidthInactiveCount +
                ",unresolvedWidthInactive=" +
                    audit.UnresolvedWidthInactiveCount +
                ",cornerWidthMissingExclusions=" +
                    audit.CornerWidthMissingExclusionCount +
                ",cornerWidthInactiveExclusions=" +
                    audit.CornerWidthInactiveExclusionCount +
                ",widthReduced=" + audit.WidthReducedCount +
                ",active=" + audit.ActiveCount +
                ",attemptedBuilt=" + audit.AttemptedBuiltCount +
                ",certifiedBuilt=" + audit.BuiltCount +
                ",trialRejected=" + audit.TrialRejectedCount +
                ",deferred=" + audit.DeferredCount +
                ",rejected=" + audit.RejectedCount +
                ",unmapped=" + audit.UnmappedCount;
        }

        private static string FormatEdgeWearCoverageIdSummary(
            EdgeWearCoverageAudit audit)
        {
            if (audit == null)
            {
                return "notCaptured";
            }

            return "structuralIneligible={" +
                    FormatEdgeWearCoverageIds(
                        audit,
                        "structural-ineligible") + "}" +
                ",geometricIneligible={" +
                    FormatEdgeWearCoverageIds(
                        audit,
                        "geometric-ineligible") + "}" +
                ",coincidentSeams={" +
                    FormatEdgeWearCoverageIds(
                        audit,
                        "coincident-seam") + "}" +
                ",recoveredGeometric={" +
                    FormatEdgeWearIndexList(
                        audit.RecoveredGeometricEdgeIndices) + "}" +
                ",collateralLost={" +
                    FormatEdgeWearIndexList(
                        audit.CollateralLostEdgeIndices) + "}" +
                ",collateralChanged={" +
                    FormatEdgeWearIndexList(
                        audit.CollateralChangedEdgeIndices) + "}" +
                ",coexistenceIneligible={" +
                    FormatEdgeWearCoverageIds(
                        audit,
                        "coexistence-ineligible") + "}" +
                ",wouldBeArtisticallyFiltered={" +
                    FormatEdgeWearCoverageIds(
                        audit,
                        "artistic-filtered") + "}" +
                ",widthInactive={" +
                    FormatEdgeWearCoverageIds(
                        audit,
                        "width-inactive") + "}" +
                ",trialRejected={" +
                    FormatEdgeWearCoverageIds(
                        audit,
                        "trial-rejected") + "}" +
                ",deferred={" +
                    FormatEdgeWearCoverageIds(
                        audit,
                        "deferred") + "}" +
                ",rejected={" +
                    FormatEdgeWearCoverageIds(
                        audit,
                        "rejected") + "}";
        }

        private static string FormatEdgeWearIndexList(
            List<int> indices)
        {
            if (indices == null || indices.Count == 0)
            {
                return "none";
            }

            StringBuilder builder = new StringBuilder();
            for (int index = 0; index < indices.Count; index++)
            {
                if (index > 0)
                {
                    builder.Append('/');
                }
                builder.Append(indices[index]);
            }
            return builder.ToString();
        }

        private static string FormatEdgeWearCoverageIds(
            EdgeWearCoverageAudit audit,
            string category)
        {
            if (audit == null || audit.Records == null)
            {
                return "none";
            }

            List<int> indices = new List<int>();
            for (int recordIndex = 0;
                 recordIndex < audit.Records.Count;
                 recordIndex++)
            {
                EdgeWearEdgeLifecycleRecord record =
                    audit.Records[recordIndex];
                bool include = category switch
                {
                    "structural-ineligible" =>
                        !record.StructuralEligible,
                    "geometric-ineligible" =>
                        record.StructuralEligible &&
                        !record.GeometricEligible,
                    "coincident-seam" =>
                        record.CoincidentBoundarySeamReconciled,
                    "coexistence-ineligible" =>
                        record.GeometricEligible &&
                        !record.CoexistenceEligible,
                    "artistic-filtered" =>
                        record.GeometricEligible &&
                        !record.ArtisticEligible,
                    "width-inactive" => record.WidthInactive,
                    "trial-rejected" => record.TrialRejected,
                    "deferred" => record.Deferred,
                    "rejected" => record.Rejected,
                    _ => false
                };
                if (include)
                {
                    indices.Add(ResolveEdgeWearDisplaySourceEdgeIndex(
                        audit,
                        record));
                }
            }

            if (indices.Count == 0)
            {
                return "none";
            }

            indices.Sort();
            StringBuilder builder = new StringBuilder();
            for (int index = 0; index < indices.Count; index++)
            {
                if (index > 0)
                {
                    builder.Append('/');
                }
                builder.Append(indices[index]);
            }
            return builder.ToString();
        }

        private static string FormatEdgeWearViabilityExclusionSummary(
            EdgeWearCoverageAudit audit,
            bool includeEdgeIds)
        {
            string[] names =
            {
                "boundary",
                "dihedral",
                "footprint",
                "locality",
                "isolatedRail",
                "support",
                "widthFraction",
                "endpointSpan",
                "other"
            };
            int[] counts = new int[names.Length];
            List<int>[] edgeIds = new List<int>[names.Length];
            for (int categoryIndex = 0;
                 categoryIndex < edgeIds.Length;
                 categoryIndex++)
            {
                edgeIds[categoryIndex] = new List<int>();
            }

            if (audit != null && audit.Records != null)
            {
                for (int recordIndex = 0;
                     recordIndex < audit.Records.Count;
                     recordIndex++)
                {
                    EdgeWearEdgeLifecycleRecord record =
                        audit.Records[recordIndex];
                    if (record.ViabilityState !=
                            EdgeWearViabilityState.StructuralIneligible &&
                        record.ViabilityState !=
                            EdgeWearViabilityState.GeometricIneligible)
                    {
                        continue;
                    }

                    int category = ResolveEdgeWearViabilityExclusionCategory(
                        record.FinalReason);
                    counts[category]++;
                    int displayEdgeIndex =
                        ResolveEdgeWearDisplaySourceEdgeIndex(
                            audit,
                            record);
                    if (displayEdgeIndex >= 0)
                    {
                        edgeIds[category].Add(displayEdgeIndex);
                    }
                }
            }

            StringBuilder builder = new StringBuilder();
            for (int categoryIndex = 0;
                 categoryIndex < names.Length;
                 categoryIndex++)
            {
                if (categoryIndex > 0)
                {
                    builder.Append(',');
                }
                builder.Append(names[categoryIndex]);
                builder.Append('=');
                builder.Append(counts[categoryIndex]);
                if (!includeEdgeIds)
                {
                    continue;
                }
                edgeIds[categoryIndex].Sort();
                builder.Append("{");
                builder.Append(FormatPlaneCutEdgeIndexEvidence(
                    edgeIds[categoryIndex]));
                builder.Append('}');
            }
            return builder.ToString();
        }

        private static void AppendPlaneCutCoexistenceSearchStates(
            StringBuilder builder,
            PlaneCutBevelAuditResult audit)
        {
            builder.Append("statesEvaluated=");
            builder.Append(audit.CoexistenceSearchStatesEvaluated);
            builder.Append(",timeBudgetExceeded=");
            builder.Append(audit.CoexistenceSearchTimeBudgetExceeded);
            builder.Append(",cancelled=");
            builder.Append(audit.CoexistenceSearchCancelled);
            builder.Append(",elapsedMs=");
            builder.Append(audit.CoexistenceSearchElapsedMilliseconds
                .ToString("G9", CultureInfo.InvariantCulture));
            builder.Append(",statesDeduplicated=");
            builder.Append(audit.CoexistenceSearchStatesDeduplicated);
            builder.Append(",maximumDepth=");
            builder.Append(audit.CoexistenceSearchMaximumDepth);
            builder.Append(",frontierRemaining=");
            builder.Append(audit.CoexistenceSearchFrontierRemaining);
            builder.Append(",winningDepth=");
            builder.Append(audit.CoexistenceSearchWinningDepth);
            builder.Append(",searchStateCandidateConservationFailures=");
            builder.AppendLine(
                audit.CoexistenceCandidateConservationFailureCount.ToString());
            List<PlaneCutCoexistenceSearchStateRecord> states =
                audit.CoexistenceSearchStates;
            if (states == null || states.Count == 0)
            {
                builder.AppendLine("none");
                return;
            }
            for (int index = 0; index < states.Count; index++)
            {
                PlaneCutCoexistenceSearchStateRecord state = states[index];
                builder.Append("state=");
                builder.Append(state.StateIndex);
                builder.Append(",depth=");
                builder.Append(state.Depth);
                builder.Append(",exclusions={");
                builder.Append(string.IsNullOrEmpty(state.ExclusionEvidence)
                    ? "none"
                    : state.ExclusionEvidence);
                builder.Append("},failureCategory=");
                builder.Append(string.IsNullOrEmpty(state.FailureCategory)
                    ? "none"
                    : state.FailureCategory);
                builder.Append(",stage=");
                builder.Append(string.IsNullOrEmpty(state.FailureStage)
                    ? "none"
                    : state.FailureStage);
                builder.Append(",sourceVertex=");
                builder.Append(state.SourceVertex);
                builder.Append(",victim/foreign=");
                builder.Append(state.VictimEdge);
                builder.Append('/');
                builder.Append(state.ForeignEdge);
                builder.Append(",linked={");
                builder.Append(string.IsNullOrEmpty(
                        state.LinkedEdgeEvidence)
                    ? "none"
                    : state.LinkedEdgeEvidence);
                builder.Append("},star={");
                builder.Append(string.IsNullOrEmpty(
                        state.IncidentStarEvidence)
                    ? "none"
                    : state.IncidentStarEvidence);
                builder.Append("},implicated={");
                builder.Append(string.IsNullOrEmpty(
                        state.ImplicatedEdgeEvidence)
                    ? "none"
                    : state.ImplicatedEdgeEvidence);
                builder.Append("},expected/actual/certified=");
                builder.Append(state.ExpectedCandidateCount);
                builder.Append('/');
                builder.Append(state.ActualCandidateCount);
                builder.Append('/');
                builder.Append(state.CertifiedCandidateCount);
                builder.Append(",expectedEdges={");
                builder.Append(string.IsNullOrEmpty(
                        state.ExpectedCandidateEvidence)
                    ? "none"
                    : state.ExpectedCandidateEvidence);
                builder.Append("},actualEdges={");
                builder.Append(string.IsNullOrEmpty(
                        state.ActualCandidateEvidence)
                    ? "none"
                    : state.ActualCandidateEvidence);
                builder.Append("},missingEdges={");
                builder.Append(string.IsNullOrEmpty(
                        state.MissingCandidateEvidence)
                    ? "none"
                    : state.MissingCandidateEvidence);
                builder.Append("},unexpectedEdges={");
                builder.Append(string.IsNullOrEmpty(
                        state.UnexpectedCandidateEvidence)
                    ? "none"
                    : state.UnexpectedCandidateEvidence);
                builder.Append("},conservationValid=");
                builder.Append(state.CandidateConservationValid);
                builder.Append(",minimumWidthScale=");
                builder.Append(state.MinimumWidthScale.ToString("G9"));
                builder.Append(",fullyValid=");
                builder.Append(state.FullyValid);
                builder.Append(",signature=");
                builder.AppendLine(string.IsNullOrEmpty(
                        state.FailureSignature)
                    ? "none"
                    : state.FailureSignature);
            }
        }

        private static string FormatPlaneCutCoexistenceSearchTrace(
            PlaneCutBevelAuditResult audit)
        {
            if (audit.CoexistenceSearchStates == null ||
                audit.CoexistenceSearchStates.Count == 0)
            {
                return string.Empty;
            }
            StringBuilder builder = new StringBuilder();
            AppendPlaneCutCoexistenceSearchStates(builder, audit);
            return builder.ToString().TrimEnd();
        }

        private static int ResolveEdgeWearViabilityExclusionCategory(
            string reason)
        {
            if (string.Equals(reason, "boundary-edge",
                    StringComparison.Ordinal))
            {
                return 0;
            }
            if (string.Equals(reason,
                    "dihedral-below-bevel-viability",
                    StringComparison.Ordinal))
            {
                return 1;
            }
            if (string.Equals(reason,
                    "edge-too-short-for-bevel-footprint",
                    StringComparison.Ordinal))
            {
                return 2;
            }
            if (string.Equals(reason,
                    "independent-plane-locality-infeasible",
                    StringComparison.Ordinal))
            {
                return 3;
            }
            if (string.Equals(reason,
                    "isolated-rail-solve-failed",
                    StringComparison.Ordinal))
            {
                return 4;
            }
            if (string.Equals(reason,
                    "owner-face-support-insufficient",
                    StringComparison.Ordinal))
            {
                return 5;
            }
            if (string.Equals(reason,
                    "maximum-feasible-width-below-minimum-scale",
                    StringComparison.Ordinal) ||
                string.Equals(reason,
                    "maximum-certified-width-at-stable-width-floor",
                    StringComparison.Ordinal))
            {
                return 6;
            }
            if (string.Equals(reason,
                    "endpoint-star-consumes-edge-span",
                    StringComparison.Ordinal))
            {
                return 7;
            }
            return 8;
        }

        private static string FormatEdgeWearCoexistenceSummary(
            PlaneCutBevelAuditResult audit,
            bool includeEdgeIds)
        {
            EdgeWearCoverageAudit coverage = audit.CoverageAudit;
            int star = 0;
            int pair = 0;
            int band = 0;
            int widthFloor = 0;
            int conservation = 0;
            int cornerMissing = 0;
            int cornerInactive = 0;
            int other = 0;
            List<int> starEdges = new List<int>();
            List<int> pairEdges = new List<int>();
            List<int> bandEdges = new List<int>();
            List<int> widthEdges = new List<int>();
            List<int> conservationEdges = new List<int>();
            List<int> cornerMissingEdges = new List<int>();
            List<int> cornerInactiveEdges = new List<int>();
            List<int> otherEdges = new List<int>();
            if (coverage != null && coverage.Records != null)
            {
                for (int index = 0; index < coverage.Records.Count; index++)
                {
                    EdgeWearEdgeLifecycleRecord record = coverage.Records[index];
                    if (record.ViabilityState !=
                        EdgeWearViabilityState.CoexistenceIneligible)
                    {
                        continue;
                    }
                    int category = ResolveEdgeWearCoexistenceExclusionCategory(
                        record.FinalReason);
                    if (category == 0)
                    {
                        star++;
                        starEdges.Add(ResolveEdgeWearDisplaySourceEdgeIndex(
                            coverage,
                            record));
                    }
                    else if (category == 1)
                    {
                        pair++;
                        pairEdges.Add(ResolveEdgeWearDisplaySourceEdgeIndex(
                            coverage,
                            record));
                    }
                    else if (category == 2)
                    {
                        band++;
                        bandEdges.Add(ResolveEdgeWearDisplaySourceEdgeIndex(
                            coverage,
                            record));
                    }
                    else if (category == 3)
                    {
                        widthFloor++;
                        widthEdges.Add(ResolveEdgeWearDisplaySourceEdgeIndex(
                            coverage,
                            record));
                    }
                    else if (category == 4)
                    {
                        conservation++;
                        conservationEdges.Add(ResolveEdgeWearDisplaySourceEdgeIndex(
                            coverage,
                            record));
                    }
                    else if (category == 5)
                    {
                        cornerMissing++;
                        cornerMissingEdges.Add(ResolveEdgeWearDisplaySourceEdgeIndex(
                            coverage,
                            record));
                    }
                    else if (category == 6)
                    {
                        cornerInactive++;
                        cornerInactiveEdges.Add(ResolveEdgeWearDisplaySourceEdgeIndex(
                            coverage,
                            record));
                    }
                    else
                    {
                        other++;
                        otherEdges.Add(ResolveEdgeWearDisplaySourceEdgeIndex(
                            coverage,
                            record));
                    }
                }
            }
            string FormatCategory(string name, int count, List<int> ids)
            {
                if (!includeEdgeIds)
                {
                    return name + "=" + count;
                }
                ids.Sort();
                return name + "=" + count + "{" +
                    FormatPlaneCutEdgeIndexEvidence(ids) + "}";
            }
            return "eligible=" + (coverage == null
                    ? 0
                    : coverage.CoexistenceEligibleCount) +
                ",ineligible=" + (coverage == null
                    ? 0
                    : coverage.CoexistenceIneligibleCount) +
                "," + FormatCategory("sourceVertexStar", star, starEdges) +
                "," + FormatCategory("planePair", pair, pairEdges) +
                "," + FormatCategory("planeBand", band, bandEdges) +
                "," + FormatCategory("globalWidthFloor", widthFloor, widthEdges) +
                "," + FormatCategory("candidateConservation", conservation,
                    conservationEdges) +
                "," + FormatCategory("cornerWidthMissing", cornerMissing,
                    cornerMissingEdges) +
                "," + FormatCategory("cornerWidthInactive", cornerInactive,
                    cornerInactiveEdges) +
                "," + FormatCategory("other", other, otherEdges) +
                ",starEvaluations=" + audit.CoexistenceStarEvaluationCount +
                ",starCacheUses=" + audit.CoexistenceStarCacheUseCount +
                ",pairEvaluations=" + audit.CoexistencePairEvaluationCount +
                ",pairCacheUses=" + audit.CoexistencePairCacheUseCount +
                ",trials=" + audit.CoexistenceTrialCount +
                ",trialCacheUses=" + audit.CoexistenceTrialCacheUseCount +
                ",statesEvaluated=" +
                    audit.CoexistenceSearchStatesEvaluated +
                ",timeBudgetExceeded=" +
                    audit.CoexistenceSearchTimeBudgetExceeded +
                ",cancelled=" + audit.CoexistenceSearchCancelled +
                ",elapsedMs=" +
                    audit.CoexistenceSearchElapsedMilliseconds.ToString(
                        "G9", CultureInfo.InvariantCulture) +
                ",statesDeduplicated=" +
                    audit.CoexistenceSearchStatesDeduplicated +
                ",maximumDepth=" + audit.CoexistenceSearchMaximumDepth +
                ",frontierRemaining=" +
                    audit.CoexistenceSearchFrontierRemaining +
                ",winningDepth=" + audit.CoexistenceSearchWinningDepth +
                ",searchStateCandidateConservationFailures=" +
                    audit.CoexistenceCandidateConservationFailureCount +
                ",preShellExclusions=" +
                    (coverage == null
                        ? 0
                        : coverage.CoexistencePreShellExclusionCount) +
                ",searchExclusions=" +
                    (coverage == null
                        ? 0
                        : coverage.CoexistenceSearchExclusionCount) +
                ",exclusions=" + audit.CoexistenceExclusionCount +
                ",minimumCommittedWidthScale=" +
                    audit.CoexistenceMinimumCommittedWidthScale.ToString("G9") +
                ",candidateExpected={" +
                    (string.IsNullOrEmpty(
                        audit.CoexistenceCandidateExpectedEvidence)
                        ? "none"
                        : audit.CoexistenceCandidateExpectedEvidence) + "}" +
                ",candidateActual={" +
                    (string.IsNullOrEmpty(
                        audit.CoexistenceCandidateActualEvidence)
                        ? "none"
                        : audit.CoexistenceCandidateActualEvidence) + "}" +
                ",candidateMissing={" +
                    (string.IsNullOrEmpty(
                        audit.CoexistenceCandidateMissingEvidence)
                        ? "none"
                        : audit.CoexistenceCandidateMissingEvidence) + "}" +
                ",candidateUnexpected={" +
                    (string.IsNullOrEmpty(
                        audit.CoexistenceCandidateUnexpectedEvidence)
                        ? "none"
                        : audit.CoexistenceCandidateUnexpectedEvidence) + "}" +
                ",excludedEdges={" +
                    (string.IsNullOrEmpty(audit.CoexistenceExcludedEdgeEvidence)
                        ? "none"
                        : audit.CoexistenceExcludedEdgeEvidence) + "}" +
                ",reasons={" +
                    (string.IsNullOrEmpty(audit.CoexistenceExclusionReasonEvidence)
                        ? "none"
                        : audit.CoexistenceExclusionReasonEvidence) + "}";
        }

        private static int ResolveEdgeWearCoexistenceExclusionCategory(
            string reason)
        {
            if (string.Equals(reason,
                    "source-vertex-star-incompatible",
                    StringComparison.Ordinal))
            {
                return 0;
            }
            if (string.Equals(reason,
                    "plane-pair-incompatible",
                    StringComparison.Ordinal))
            {
                return 1;
            }
            if (string.Equals(reason,
                    "plane-band-incompatible",
                    StringComparison.Ordinal))
            {
                return 2;
            }
            if (string.Equals(reason,
                    "global-width-floor-conflict",
                    StringComparison.Ordinal))
            {
                return 3;
            }
            if (string.Equals(reason,
                    "candidate-conservation-incompatible",
                    StringComparison.Ordinal))
            {
                return 4;
            }
            if (string.Equals(reason,
                    "corner-width-missing",
                    StringComparison.Ordinal))
            {
                return 5;
            }
            if (string.Equals(reason,
                    "corner-width-inactive",
                    StringComparison.Ordinal))
            {
                return 6;
            }
            return 7;
        }

        private static string FormatEdgeWearLocalityCacheContract(
            EdgeWearCoverageAudit audit)
        {
            if (audit == null)
            {
                return "evaluations=0,constructionUses=0," +
                    "recomputationsDuringSolver=0,unusedEvaluatedRecords=0," +
                    "localityCacheMissesDuringConstruction=0";
            }
            int unused = Mathf.Max(
                0,
                audit.ViabilityLocalityEvaluationCount -
                    audit.ViabilityLocalityCacheUseCount);
            return "evaluations=" +
                    audit.ViabilityLocalityEvaluationCount +
                ",constructionUses=" +
                    audit.ViabilityLocalityCacheUseCount +
                ",recomputationsDuringSolver=" +
                    audit.ViabilityLocalityRecomputationCount +
                ",unusedEvaluatedRecords=" + unused +
                ",localityCacheMissesDuringConstruction=" +
                    audit.ViabilityLocalityCacheMissCount;
        }

        private static void AppendEdgeWearViabilityPreflight(
            StringBuilder builder,
            EdgeWearCoverageAudit audit)
        {
            if (builder == null)
            {
                return;
            }
            if (audit == null || audit.Records == null)
            {
                builder.AppendLine("notCaptured");
                return;
            }

            List<EdgeWearEdgeLifecycleRecord> ordered =
                new List<EdgeWearEdgeLifecycleRecord>(audit.Records);
            ordered.Sort((left, right) =>
                ResolveEdgeWearDisplaySourceEdgeIndex(audit, left).CompareTo(
                    ResolveEdgeWearDisplaySourceEdgeIndex(audit, right)));
            builder.Append("thresholds={minimumDihedral:");
            builder.Append(
                EdgeWearMinimumViableDihedralDegrees.ToString("G9"));
            builder.Append(",footprintMultiplier:");
            builder.Append(
                EdgeWearMinimumFootprintLengthMultiplier.ToString("G9"));
            builder.Append(",minimumWidthFraction:");
            builder.Append(
                EdgeWearMinimumFeasibleWidthFraction.ToString("G9"));
            builder.Append(",minimumCentralSpanMultiplier:");
            builder.Append(
                EdgeWearMinimumCentralSpanWidthMultiplier.ToString("G9"));
            builder.AppendLine("}");
            builder.Append("cache={localityEvaluations:");
            builder.Append(audit.ViabilityLocalityEvaluationCount);
            builder.Append(",isolatedEvaluations:");
            builder.Append(audit.ViabilityIsolatedEvaluationCount);
            builder.Append(",constructionUses:");
            builder.Append(audit.ViabilityLocalityCacheUseCount);
            builder.Append(",cacheMisses:");
            builder.Append(audit.ViabilityLocalityCacheMissCount);
            builder.Append(",solverRecomputations:");
            builder.Append(audit.ViabilityLocalityRecomputationCount);
            builder.Append(",unusedEvaluatedRecords:");
            builder.Append(Mathf.Max(
                0,
                audit.ViabilityLocalityEvaluationCount -
                    audit.ViabilityLocalityCacheUseCount));
            builder.Append(",milliseconds:");
            builder.Append(
                audit.ViabilityPreflightMilliseconds.ToString("G9"));
            builder.AppendLine("}");
            builder.Append("count=");
            builder.AppendLine(ordered.Count.ToString());
            for (int recordIndex = 0;
                 recordIndex < ordered.Count;
                 recordIndex++)
            {
                EdgeWearEdgeLifecycleRecord lifecycle =
                    ordered[recordIndex];
                EdgeWearEdgeViabilityRecord record =
                    lifecycle.Viability;
                builder.Append("edge=");
                builder.Append(
                    ResolveEdgeWearDisplaySourceEdgeIndex(audit, lifecycle));
                builder.Append(",state=");
                builder.Append(lifecycle.ViabilityState);
                builder.Append(",structural=");
                builder.Append(lifecycle.StructuralEligible ? '1' : '0');
                builder.Append(",geometric=");
                builder.Append(lifecycle.GeometricEligible ? '1' : '0');
                builder.Append(",length=");
                builder.Append(lifecycle.Length.ToString("G9"));
                builder.Append(",dihedral=");
                builder.Append(lifecycle.DihedralDegrees.ToString("G9"));
                if (record == null)
                {
                    builder.AppendLine(",preflight=notCaptured");
                    continue;
                }
                builder.Append(",macro={base:");
                builder.Append(record.BaseRequestedWidth.ToString("G9"));
                builder.Append(",coverage:");
                builder.Append(
                    record.MacroVariationCoverage.ToString("G9"));
                builder.Append(",strength:");
                builder.Append(record.MacroVariation.ToString("G9"));
                builder.Append(",participationIdentity:");
                builder.Append(
                    record.MacroParticipationIdentity01.ToString("G9"));
                builder.Append(",participates:");
                builder.Append(
                    record.MacroVariationParticipates ? '1' : '0');
                builder.Append(",identity:");
                builder.Append(record.MacroIdentity01.ToString("G9"));
                builder.Append(",sampled:");
                builder.Append(
                    record.MacroSampledMultiplier.ToString("G9"));
                builder.Append(",anglePermission:");
                builder.Append(
                    ResolveEdgeWearMacroAnglePermission(
                        lifecycle.DihedralDegrees).ToString("G9"));
                builder.Append(",effective:");
                builder.Append(
                    record.MacroEffectiveMultiplier.ToString("G9"));
                builder.Append(",minimumStyleClamped:");
                builder.Append(
                    record.MacroMinimumStyleClamped ? '1' : '0');
                builder.Append('}');
                builder.Append(",requestedWidth=");
                builder.Append(record.RequestedWidth.ToString("G9"));
                builder.Append(",requiredFootprintLength=");
                builder.Append(
                    record.RequiredFootprintLength.ToString("G9"));
                builder.Append(",lengthToWidthRatio=");
                builder.Append(record.LengthToWidthRatio.ToString("G9"));
                builder.Append(",minimumStyleWidth=");
                builder.Append(record.MinimumStyleWidth.ToString("G9"));
                builder.Append(",minimumRequiredCertifiedWidth=");
                builder.Append(
                    record.MinimumRequiredCertifiedWidth.ToString("G9"));
                builder.Append(",gates={dihedral:");
                builder.Append(record.DihedralValid ? '1' : '0');
                builder.Append(",footprint:");
                builder.Append(record.FootprintValid ? '1' : '0');
                builder.Append(",locality:");
                builder.Append(record.LocalityValid ? '1' : '0');
                builder.Append(",isolated:");
                builder.Append(
                    record.IsolatedConstructionValid ? '1' : '0');
                builder.Append(",widthFraction:");
                builder.Append(
                    record.FeasibleWidthFractionValid ? '1' : '0');
                builder.Append(",widthRecoveryProvisional:");
                builder.Append(
                    record.WidthRecoveryProvisional ? '1' : '0');
                builder.Append(",materialWidthRecoveryEligible:");
                builder.Append(
                    record.MaterialWidthRecoveryEligible ? '1' : '0');
                builder.Append(",materialWidthRecoveryRequiredLength:");
                builder.Append(
                    record.MaterialWidthRecoveryRequiredLength
                        .ToString("G9"));
                builder.Append(",multiSupportHullRecovery:");
                builder.Append(
                    record.MultiSupportHullRecovery ? '1' : '0');
                builder.Append(",endpointSpan:");
                builder.Append(record.EndpointSpanValid ? '1' : '0');
                builder.Append('}');
                builder.Append(",locality={retainFloor:");
                builder.Append(
                    record.LocalityRetainPlaneFloor.ToString("G9"));
                builder.Append(",removalCeiling:");
                builder.Append(
                    record.LocalityRemovalPlaneCeiling.ToString("G9"));
                builder.Append(",margin:");
                builder.Append(
                    record.LocalityFeasibleMargin.ToString("G9"));
                builder.Append(",guard:");
                builder.Append(
                    record.LocalityGuardMargin.ToString("G9"));
                builder.Append(",minimumRemoval:");
                builder.Append(
                    record.LocalityMinimumRemoval.ToString("G9"));
                builder.Append(",limitingVertex:");
                builder.Append(record.LocalityLimitingVertex);
                builder.Append(",limitingPosition:");
                builder.Append(FormatPlaneCutVector(
                    record.LocalityLimitingPosition));
                builder.Append('}');
                builder.Append(",isolated={succeeded:");
                builder.Append(record.IsolatedSucceeded ? '1' : '0');
                builder.Append(",attempts:");
                builder.Append(record.IsolatedWidthAttemptCount);
                builder.Append(",lastAttemptedWidth:");
                builder.Append(
                    record.IsolatedLastAttemptedWidth.ToString("G9"));
                builder.Append(",scheduleComplete:");
                builder.Append(
                    record.IsolatedAttemptScheduleComplete ? '1' : '0');
                builder.Append(",terminalAtMinimum:");
                builder.Append(
                    record.IsolatedTerminalConstructionAtMinimum
                        ? '1'
                        : '0');
                builder.Append(",scheduleResolution:");
                builder.Append(string.IsNullOrEmpty(
                        record.IsolatedAttemptScheduleResolution)
                    ? "none"
                    : record.IsolatedAttemptScheduleResolution);
                builder.Append(",attemptEvidence:{");
                builder.Append(string.IsNullOrEmpty(
                        record.IsolatedWidthAttemptEvidence)
                    ? "none"
                    : record.IsolatedWidthAttemptEvidence);
                builder.Append('}');
                builder.Append(",maximumCertifiedWidth:");
                builder.Append(
                    record.IsolatedMaximumCertifiedWidth.ToString("G9"));
                builder.Append(",maximumCertifiedFraction:");
                builder.Append(
                    record.IsolatedMaximumCertifiedWidthFraction
                        .ToString("G9"));
                builder.Append(",maxBoundarySnap:");
                builder.Append(
                    record.IsolatedMaximumBoundarySnapDistance
                        .ToString("G9"));
                builder.Append(",maxBoundaryPointTolerance:");
                builder.Append(
                    record.IsolatedMaximumBoundaryPointTolerance
                        .ToString("G9"));
                builder.Append(",alternateBoundaryRails:");
                builder.Append(
                    record.IsolatedAlternateBoundaryRailCount);
                builder.Append(",maxBoundaryCandidates:");
                builder.Append(
                    record.IsolatedMaximumBoundaryCandidateCount);
                builder.Append(",maxBoundaryDiagnosticRail:");
                builder.Append(
                    record.IsolatedMaximumBoundaryDiagnosticRailIndex);
                builder.Append(",originalAdjacentEdge:");
                builder.Append(ResolveEdgeWearDisplayGraphEdgeIndex(
                    audit,
                    record.IsolatedMaximumBoundaryOriginalAdjacentEdgeIndex));
                builder.Append(",resolvedBoundaryEdge:");
                builder.Append(ResolveEdgeWearDisplayGraphEdgeIndex(
                    audit,
                    record.IsolatedMaximumBoundaryResolvedEdgeIndex));
                builder.Append(",originalRawParameter:");
                builder.Append(
                    record.IsolatedMaximumBoundaryOriginalRawParameter
                        .ToString("G9"));
                builder.Append(",originalSegmentDistance:");
                builder.Append(
                    record.IsolatedMaximumBoundaryOriginalSegmentDistance
                        .ToString("G9"));
                builder.Append(",minEndpointDistance:");
                builder.Append(
                    record.IsolatedMinimumBoundaryEndpointDistance
                        .ToString("G9"));
                builder.Append(",endpointConsumption:");
                builder.Append(record.EndpointConsumptionA.ToString("G9"));
                builder.Append('/');
                builder.Append(record.EndpointConsumptionB.ToString("G9"));
                builder.Append(",remainingSpan:");
                builder.Append(record.RemainingCentralSpan.ToString("G9"));
                builder.Append(",minimumSpan:");
                builder.Append(record.MinimumCentralSpan.ToString("G9"));
                builder.Append(",topology:");
                builder.Append(record.IsolatedOpenEdgeCount);
                builder.Append('/');
                builder.Append(record.IsolatedNonManifoldEdgeCount);
                builder.Append('/');
                builder.Append(record.IsolatedTJunctionCount);
                builder.Append('/');
                builder.Append(record.IsolatedInvalidFaceCount);
                builder.Append(",diagnostic:");
                builder.Append(string.IsNullOrEmpty(
                        record.IsolatedDiagnostic)
                    ? "none"
                    : record.IsolatedDiagnostic);
                builder.Append('}');
                builder.Append(",widthRecovery={eligible:");
                builder.Append(
                    record.MaterialWidthRecoveryEligible ? '1' : '0');
                builder.Append(",target:");
                builder.Append(
                    lifecycle.MaterialWidthRecoveryTarget ? '1' : '0');
                builder.Append(",baselineDeferred:");
                builder.Append(
                    lifecycle.MaterialWidthRecoveryBaselineDeferred
                        ? '1'
                        : '0');
                builder.Append(",currentDeferred:");
                builder.Append(
                    lifecycle.RecoveryBaselineDeferred ? '1' : '0');
                builder.Append(",attempted:");
                builder.Append(
                    lifecycle.MaterialWidthRecoveryAttempted ? '1' : '0');
                builder.Append(",completed:");
                builder.Append(
                    lifecycle.MaterialWidthRecoveryTrialCompleted
                        ? '1'
                        : '0');
                builder.Append(",trialSucceeded:");
                builder.Append(
                    lifecycle.MaterialWidthRecoveryTrialSucceeded
                        ? '1'
                        : '0');
                builder.Append(",certified:");
                builder.Append(
                    lifecycle.MaterialWidthRecoveryCertified ? '1' : '0');
                builder.Append(",failure:{");
                builder.Append(string.IsNullOrEmpty(
                        lifecycle.MaterialWidthRecoveryFailure)
                    ? "none"
                    : SanitizeMaterialWidthRecoveryEvidence(
                        lifecycle.MaterialWidthRecoveryFailure));
                builder.Append("},resolution:{");
                builder.Append(string.IsNullOrEmpty(
                        lifecycle.WidthRecoveryResolution)
                    ? "none"
                    : lifecycle.WidthRecoveryResolution);
                builder.Append("},evidence:{");
                builder.Append(string.IsNullOrEmpty(
                        lifecycle.WidthRecoveryEvidence)
                    ? "none"
                    : lifecycle.WidthRecoveryEvidence);
                builder.Append("}}");
                builder.Append(",cornerRecovery={provisional:");
                builder.Append(
                    lifecycle.CornerRecoveryProvisional ? '1' : '0');
                builder.Append(",lastPositiveWidth:");
                builder.Append(
                    lifecycle.CornerRecoveryLastPositiveWidth
                        .ToString("G9"));
                builder.Append(",collapsedEdge:");
                builder.Append(
                    lifecycle.CornerRecoveryCollapsedSourceEdgeIndex);
                builder.Append(",uniformScale:");
                builder.Append(
                    lifecycle.CornerRecoveryUniformScale.ToString("G9"));
                builder.Append(",zeroingStage:");
                builder.Append(
                    lifecycle.CornerRecoveryZeroingStage);
                builder.Append(",participants:{");
                builder.Append(string.IsNullOrEmpty(
                        lifecycle.CornerRecoveryParticipants)
                    ? "none"
                    : lifecycle.CornerRecoveryParticipants);
                builder.Append("},zeroed:{");
                builder.Append(string.IsNullOrEmpty(
                        lifecycle.CornerRecoveryZeroedParticipants)
                    ? "none"
                    : lifecycle.CornerRecoveryZeroedParticipants);
                builder.Append("},resolution:{");
                builder.Append(string.IsNullOrEmpty(
                        lifecycle.CornerRecoveryResolution)
                    ? "none"
                    : lifecycle.CornerRecoveryResolution);
                builder.Append("}}");
                builder.Append(",failureReason=");
                builder.AppendLine(string.IsNullOrEmpty(record.FailureReason)
                    ? "none"
                    : record.FailureReason);
            }
        }

        private static void AppendEdgeWearCoverageLifecycle(
            StringBuilder builder,
            EdgeWearCoverageAudit audit)
        {
            if (builder == null)
            {
                return;
            }
            if (audit == null || audit.Records == null)
            {
                builder.AppendLine("notCaptured");
                return;
            }

            List<EdgeWearEdgeLifecycleRecord> ordered =
                new List<EdgeWearEdgeLifecycleRecord>(audit.Records);
            ordered.Sort((left, right) =>
                ResolveEdgeWearDisplaySourceEdgeIndex(audit, left).CompareTo(
                    ResolveEdgeWearDisplaySourceEdgeIndex(audit, right)));
            builder.Append("count=");
            builder.AppendLine(ordered.Count.ToString());
            for (int recordIndex = 0;
                 recordIndex < ordered.Count;
                 recordIndex++)
            {
                EdgeWearEdgeLifecycleRecord record = ordered[recordIndex];
                builder.Append("edge=");
                builder.Append(
                    ResolveEdgeWearDisplaySourceEdgeIndex(audit, record));
                builder.Append(",segment=");
                builder.Append(FormatPlaneCutVector(record.Start));
                builder.Append("->");
                builder.Append(FormatPlaneCutVector(record.End));
                builder.Append(",faces=");
                builder.Append(record.FaceA);
                builder.Append('/');
                builder.Append(record.FaceB);
                builder.Append('/');
                builder.Append(record.FaceCount);
                builder.Append(",length=");
                builder.Append(record.Length.ToString("G9"));
                builder.Append(",dihedral=");
                builder.Append(record.DihedralDegrees.ToString("G9"));
                builder.Append(",vertical01=");
                builder.Append(record.Vertical01.ToString("G9"));
                builder.Append(",classification=");
                builder.Append(record.Classification);
                builder.Append(",coincidentSeamReconciled=");
                builder.Append(
                    record.CoincidentBoundarySeamReconciled ? '1' : '0');
                builder.Append(",microSuppressed=");
                builder.Append(record.MicroTopologySuppressed ? '1' : '0');
                builder.Append(",microGeneratedTransition=");
                builder.Append(
                    record.MicroTopologyGeneratedTransition ? '1' : '0');
                builder.Append(",graphEdge=");
                builder.Append(record.SourceEdgeIndex);
                if (record.Viability != null)
                {
                    builder.Append(",macro={base:");
                    builder.Append(
                        record.Viability.BaseRequestedWidth.ToString("G9"));
                    builder.Append(",coverage:");
                    builder.Append(
                        record.Viability.MacroVariationCoverage.ToString(
                            "G9"));
                    builder.Append(",strength:");
                    builder.Append(
                        record.Viability.MacroVariation.ToString("G9"));
                    builder.Append(",participationIdentity:");
                    builder.Append(
                        record.Viability.MacroParticipationIdentity01
                            .ToString("G9"));
                    builder.Append(",participates:");
                    builder.Append(
                        record.Viability.MacroVariationParticipates
                            ? '1'
                            : '0');
                    builder.Append(",identity:");
                    builder.Append(
                        record.Viability.MacroIdentity01.ToString("G9"));
                    builder.Append(",sampled:");
                    builder.Append(
                        record.Viability.MacroSampledMultiplier.ToString(
                            "G9"));
                    builder.Append(",anglePermission:");
                    builder.Append(
                        ResolveEdgeWearMacroAnglePermission(
                            record.DihedralDegrees).ToString("G9"));
                    builder.Append(",effective:");
                    builder.Append(
                        record.Viability.MacroEffectiveMultiplier.ToString(
                            "G9"));
                    builder.Append(",requested:");
                    builder.Append(
                        record.Viability.RequestedWidth.ToString("G9"));
                    builder.Append('}');
                }
                builder.Append(",structural=");
                builder.Append(record.StructuralEligible ? '1' : '0');
                builder.Append(",geometric=");
                builder.Append(record.GeometricEligible ? '1' : '0');
                builder.Append(",coexistence=");
                builder.Append(record.CoexistenceEligible ? '1' : '0');
                builder.Append(",coexistenceReason=");
                builder.Append(string.IsNullOrEmpty(
                        record.CoexistenceFailureReason)
                    ? "none"
                    : record.CoexistenceFailureReason);
                builder.Append(",viabilityState=");
                builder.Append(record.ViabilityState);
                builder.Append(",artistic=");
                builder.Append(record.ArtisticEligible ? '1' : '0');
                builder.Append(",candidate=");
                builder.Append(record.Candidate ? '1' : '0');
                builder.Append(",candidateIndex=");
                builder.Append(record.CandidateIndex);
                builder.Append(",candidateReason=");
                builder.Append(string.IsNullOrEmpty(record.CandidateReason)
                    ? "none"
                    : record.CandidateReason);
                builder.Append(",score=");
                builder.Append(record.Score.ToString("G9"));
                builder.Append(",artisticInputs={minimumLength:");
                builder.Append(record.ArtisticMinimumLength.ToString("G9"));
                builder.Append(",lengthScore:");
                builder.Append(record.ArtisticLengthScore.ToString("G9"));
                builder.Append(",angleScore:");
                builder.Append(record.ArtisticAngleScore.ToString("G9"));
                builder.Append(",random:");
                builder.Append(record.ArtisticRandomScore.ToString("G9"));
                builder.Append(",baseSuppression:");
                builder.Append(
                    record.ArtisticBaseSuppression.ToString("G9"));
                builder.Append(",upwardBoost:");
                builder.Append(record.ArtisticUpwardEdgeBoost.ToString("G9"));
                builder.Append(",characterBoost:");
                builder.Append(
                    record.ArtisticCharacterBoost.ToString("G9"));
                builder.Append(",gates:");
                builder.Append(record.ArtisticLengthEligible ? '1' : '0');
                builder.Append('/');
                builder.Append(record.ArtisticAngleEligible ? '1' : '0');
                builder.Append('/');
                builder.Append(record.ArtisticBaseEligible ? '1' : '0');
                builder.Append('}');
                builder.Append(",artisticContext={edgeAxisVertical01:");
                builder.Append(
                    record.ArtisticEdgeAxisVertical01.ToString("G9"));
                builder.Append(",silhouettePotential:");
                builder.Append(
                    record.ArtisticSilhouettePotential.ToString("G9"));
                builder.Append(",feasibleWidthFraction:");
                builder.Append(
                    record.ArtisticFeasibleWidthFraction.ToString("G9"));
                builder.Append(",solvedWidthFraction:");
                builder.Append(
                    record.ArtisticSolvedWidthFraction.ToString("G9"));
                builder.Append(",localDensity01:");
                builder.Append(
                    record.ArtisticLocalDensity01.ToString("G9"));
                builder.Append(",sharedVertexDegree:");
                builder.Append(record.ArtisticSharedVertexDegreeA);
                builder.Append('/');
                builder.Append(record.ArtisticSharedVertexDegreeB);
                builder.Append(",scoreWeights:silhouette0/width0/density0/crowding0}");
                builder.Append(",artisticSelection={rank:");
                builder.Append(record.ArtisticSelectionRank);
                builder.Append(",threshold:");
                builder.Append(
                    record.ArtisticSelectionThreshold.ToString("G9"));
                builder.Append(",delta:");
                builder.Append(record.ArtisticSelectionDelta.ToString("G9"));
                builder.Append(",filterReason:");
                builder.Append(string.IsNullOrEmpty(
                        record.ArtisticFilterReason)
                    ? "none"
                    : record.ArtisticFilterReason);
                builder.Append('}');
                builder.Append(",selected=");
                builder.Append(record.Selected ? '1' : '0');
                builder.Append(",solvedWidth=");
                builder.Append(record.SolvedWidth.ToString("G9"));
                builder.Append(",materializedWidth=");
                builder.Append(record.MaterializedWidth.ToString("G9"));
                builder.Append(",materializedWidthScale=");
                builder.Append(
                    record.MaterializedWidthScale.ToString("G9"));
                builder.Append(",widthReduced=");
                builder.Append(record.WidthReduced ? '1' : '0');
                builder.Append(",widthInactive=");
                builder.Append(record.WidthInactive ? '1' : '0');
                builder.Append(",active=");
                builder.Append(record.Active ? '1' : '0');
                builder.Append(",attemptedBuilt=");
                builder.Append(record.AttemptedBuilt ? '1' : '0');
                builder.Append(",certifiedBuilt=");
                builder.Append(record.Built ? '1' : '0');
                builder.Append(",trialRejected=");
                builder.Append(record.TrialRejected ? '1' : '0');
                builder.Append(",deferred=");
                builder.Append(record.Deferred ? '1' : '0');
                builder.Append(",rejected=");
                builder.Append(record.Rejected ? '1' : '0');
                builder.Append(",finalReason=");
                builder.AppendLine(string.IsNullOrEmpty(record.FinalReason)
                    ? "none"
                    : record.FinalReason);
            }
        }

        private static void AppendStableEvaluationFingerprint(
            StringBuilder builder,
            MassPlacementFrame placementFrame)
        {
            builder.AppendLine();
            builder.AppendLine("[Stable Evaluation Fingerprint]");
            PendingEdgeWearStableFingerprint pending =
                pendingEdgeWearStableFingerprint;
            if (pending == null || !pending.Valid)
            {
                builder.AppendLine("status=notCaptured");
                pendingEdgeWearStableFingerprint = null;
                return;
            }

            GeneratedGeometryStableHashBuilder placement =
                GeneratedGeometryStableHashBuilder.Create(
                    "PS3D.GeneratedMass.EdgeWear.PlacementFrame.v1");
            placement.AddInt32(placementFrame.ReferenceVertexCount);
            placement.AddSingle(placementFrame.LeanMinimumY);
            placement.AddSingle(placementFrame.LeanHeight);
            placement.AddVector3(placementFrame.LeanDirection);
            placement.AddSingle(placementFrame.LeanDistance);
            placement.AddSingle(placementFrame.GroundingMinimumY);
            placement.AddSingle(placementFrame.GroundingHeight);
            placement.AddSingle(placementFrame.GroundingTop);
            placement.AddSingle(
                placementFrame.GroundingFlatteningStrength);
            placement.AddSingle(
                placementFrame.GroundingBroadeningStrength);
            placement.AddSingle(placementFrame.RecenterMinimumY);
            placement.AddSingle(placementFrame.ContactBand);
            placement.AddVector2(placementFrame.ContactCentre);
            placement.AddVector3(placementFrame.RecenterOffset);
            GeneratedGeometryStableFingerprint placementFingerprint =
                placement.Finish();

            GeneratedGeometryStableHashBuilder evaluation =
                GeneratedGeometryStableHashBuilder.Create(
                    "PS3D.GeneratedMass.EdgeWear.Evaluation.v2");
            evaluation.AddInt32(pending.SourceEdgeCount);
            evaluation.AddInt32(pending.StructuralEligibleCount);
            evaluation.AddInt32(pending.GeometricEligibleCount);
            evaluation.AddInt32(pending.CoexistenceEligibleCount);
            evaluation.AddInt32(pending.SelectedCount);
            evaluation.AddInt32(pending.CertifiedCount);
            evaluation.AddFingerprint(pending.ExclusionReasons);
            evaluation.AddFingerprint(pending.SelectedEdges);
            evaluation.AddFingerprint(pending.CertifiedEdges);
            evaluation.AddFingerprint(pending.GeometryTopology);
            evaluation.AddFingerprint(placementFingerprint);
            GeneratedGeometryStableFingerprint evaluationFingerprint =
                evaluation.Finish();

            builder.Append("sourceEdges=");
            builder.AppendLine(pending.SourceEdgeCount.ToString());
            builder.Append("structuralEligible=");
            builder.AppendLine(
                pending.StructuralEligibleCount.ToString());
            builder.Append("geometricEligible=");
            builder.AppendLine(
                pending.GeometricEligibleCount.ToString());
            builder.Append("coexistenceEligible=");
            builder.AppendLine(
                pending.CoexistenceEligibleCount.ToString());
            builder.Append("selected=");
            builder.AppendLine(pending.SelectedCount.ToString());
            builder.Append("certified=");
            builder.AppendLine(pending.CertifiedCount.ToString());
            builder.Append("exclusionReasonHash=");
            builder.AppendLine(pending.ExclusionReasons.ToString());
            builder.Append("selectedEdgeHash=");
            builder.AppendLine(pending.SelectedEdges.ToString());
            builder.Append("certifiedEdgeHash=");
            builder.AppendLine(pending.CertifiedEdges.ToString());
            builder.Append("geometryTopologyHash=");
            builder.AppendLine(pending.GeometryTopology.ToString());
            builder.Append("placementFrameHash=");
            builder.AppendLine(placementFingerprint.ToString());
            builder.Append("evaluationHash=");
            builder.AppendLine(evaluationFingerprint.ToString());

            pendingEdgeWearStableFingerprint = null;
        }

        private static void AppendMassPlacementFrameTelemetry(
            MassPlacementFrame canonicalFrame,
            MassPlacementFrame legacyPreviewFrame,
            bool hasLegacyPreviewFrame,
            bool usesImmutableSourcePlacementFrame,
            bool previewApplied,
            int outputVertexCount,
            int debugPositionCount)
        {
#if UNITY_EDITOR
            if (activeEdgeWearBatchAuditCapture != null)
            {
                activeEdgeWearBatchAuditCapture.PlacementCaptured = true;
                activeEdgeWearBatchAuditCapture.PlacementFrame =
                    canonicalFrame;
                activeEdgeWearBatchAuditCapture
                    .UsesImmutableSourcePlacementFrame =
                        usesImmutableSourcePlacementFrame;
                activeEdgeWearBatchAuditCapture.PreviewApplied =
                    previewApplied;
                return;
            }

            StringBuilder builder = new StringBuilder(1024);
            builder.AppendLine();
            builder.AppendLine("[Canonical Placement Frame]");
            builder.Append("placementFrameSource=");
            builder.AppendLine(usesImmutableSourcePlacementFrame
                ? "immutable-pre-bevel"
                : "output-soup");
            builder.AppendLine("placementFrameBuilds=1");
            int placementFrameReuses =
                (usesImmutableSourcePlacementFrame ? 1 : 0) +
                (debugPositionCount > 0 ? 1 : 0);
            builder.Append("placementFrameReuses=");
            builder.AppendLine(placementFrameReuses.ToString());
            builder.Append("previewApplied=");
            builder.AppendLine(previewApplied ? "1" : "0");
            builder.Append("previewDerivedPlacementParameters=");
            builder.AppendLine(
                previewApplied && !usesImmutableSourcePlacementFrame
                    ? "1"
                    : "0");
            builder.AppendLine("objectTransformChanged=0");
            builder.Append("sourceDebugUsesCanonicalFrame=");
            builder.AppendLine(debugPositionCount > 0 ? "1" : "0");
            builder.Append("previewUsesCanonicalFrame=");
            builder.AppendLine(
                previewApplied && usesImmutableSourcePlacementFrame
                    ? "1"
                    : "0");
            builder.Append("referenceVertexCount=");
            builder.AppendLine(
                canonicalFrame.ReferenceVertexCount.ToString());
            builder.Append("outputVertexCount=");
            builder.AppendLine(outputVertexCount.ToString());
            builder.Append("debugPositionCount=");
            builder.AppendLine(debugPositionCount.ToString());
            builder.Append("lean={minimumY:");
            builder.Append(canonicalFrame.LeanMinimumY.ToString("G9"));
            builder.Append(",height:");
            builder.Append(canonicalFrame.LeanHeight.ToString("G9"));
            builder.Append(",direction:");
            builder.Append(FormatPlaneCutVector(
                canonicalFrame.LeanDirection));
            builder.Append(",distance:");
            builder.Append(canonicalFrame.LeanDistance.ToString("G9"));
            builder.AppendLine("}");
            builder.Append("grounding={minimumY:");
            builder.Append(
                canonicalFrame.GroundingMinimumY.ToString("G9"));
            builder.Append(",height:");
            builder.Append(
                canonicalFrame.GroundingHeight.ToString("G9"));
            builder.Append(",top:");
            builder.Append(canonicalFrame.GroundingTop.ToString("G9"));
            builder.Append(",flattening:");
            builder.Append(
                canonicalFrame.GroundingFlatteningStrength.ToString("G9"));
            builder.Append(",broadening:");
            builder.Append(
                canonicalFrame.GroundingBroadeningStrength.ToString("G9"));
            builder.AppendLine("}");
            builder.Append("recenter={minimumY:");
            builder.Append(
                canonicalFrame.RecenterMinimumY.ToString("G9"));
            builder.Append(",contactBand:");
            builder.Append(canonicalFrame.ContactBand.ToString("G9"));
            builder.Append(",contactCentre:(");
            builder.Append(canonicalFrame.ContactCentre.x.ToString("G9"));
            builder.Append('/');
            builder.Append(canonicalFrame.ContactCentre.y.ToString("G9"));
            builder.Append("),offset:");
            builder.Append(FormatPlaneCutVector(
                canonicalFrame.RecenterOffset));
            builder.AppendLine("}");
            builder.Append("legacyPreviewFrameCaptured=");
            builder.AppendLine(hasLegacyPreviewFrame ? "1" : "0");
            if (hasLegacyPreviewFrame)
            {
                Vector3 recenterOffsetDelta =
                    legacyPreviewFrame.RecenterOffset -
                    canonicalFrame.RecenterOffset;
                Vector2 contactCentreDelta =
                    legacyPreviewFrame.ContactCentre -
                    canonicalFrame.ContactCentre;
                builder.Append("legacyPreviewFrameDelta={recenterOffset:");
                builder.Append(FormatPlaneCutVector(recenterOffsetDelta));
                builder.Append(",leanDistance:");
                builder.Append((
                    legacyPreviewFrame.LeanDistance -
                    canonicalFrame.LeanDistance).ToString("G9"));
                builder.Append(",leanMinimumY:");
                builder.Append((
                    legacyPreviewFrame.LeanMinimumY -
                    canonicalFrame.LeanMinimumY).ToString("G9"));
                builder.Append(",groundingMinimumY:");
                builder.Append((
                    legacyPreviewFrame.GroundingMinimumY -
                    canonicalFrame.GroundingMinimumY).ToString("G9"));
                builder.Append(",contactCentre:(");
                builder.Append(contactCentreDelta.x.ToString("G9"));
                builder.Append('/');
                builder.Append(contactCentreDelta.y.ToString("G9"));
                builder.AppendLine(")}");
            }
            else
            {
                builder.AppendLine("legacyPreviewFrameDelta={none}");
            }

            AppendStableEvaluationFingerprint(
                builder,
                canonicalFrame);

            try
            {
                string projectRoot = Path.GetFullPath(
                    Path.Combine(Application.dataPath, ".."));
                string fullPath = Path.Combine(
                    projectRoot,
                    "Library",
                    "GeneratedMassEdgeWearTelemetry.txt");
                string directory = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                File.AppendAllText(
                    fullPath,
                    builder.ToString(),
                    new UTF8Encoding(
                        encoderShouldEmitUTF8Identifier: false));
            }
            catch (Exception exception)
            {
                LogChamferNoStackTrace(
                    "GeneratedMass placement-frame telemetry write failed: " +
                    exception.GetType().Name + ":" + exception.Message,
                    true);
            }
#endif
        }

        private static void LogUnifiedAllEdgeBevelAudit(
            PlaneCutBevelAuditResult audit,
            bool cornerSolutionValid,
            string cornerBlocker)
        {
#if UNITY_EDITOR
            if (activeEdgeWearBatchAuditCapture != null)
            {
                activeEdgeWearBatchAuditCapture.AuditCaptured = true;
                activeEdgeWearBatchAuditCapture.Audit = audit;
                activeEdgeWearBatchAuditCapture.CornerSolutionValid =
                    cornerSolutionValid;
                activeEdgeWearBatchAuditCapture.CornerBlocker =
                    cornerBlocker ?? string.Empty;
                return;
            }

            const string relativePath =
                "Library/GeneratedMassEdgeWearTelemetry.txt";
            int writeSucceeded = 0;
            string writeFailure = string.Empty;
            CapturePendingEdgeWearStableFingerprint(audit);
            string detailed = BuildPlaneCutDetailedTelemetry(
                audit,
                cornerSolutionValid,
                cornerBlocker);
            try
            {
                string projectRoot = Path.GetFullPath(
                    Path.Combine(Application.dataPath, ".."));
                string fullPath = Path.Combine(
                    projectRoot,
                    "Library",
                    "GeneratedMassEdgeWearTelemetry.txt");
                string directory = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                File.WriteAllText(
                    fullPath,
                    detailed,
                    new UTF8Encoding(
                        encoderShouldEmitUTF8Identifier: false));
                writeSucceeded = 1;
            }
            catch (Exception exception)
            {
                writeFailure =
                    exception.GetType().Name + ":" + exception.Message;
            }

            string message =
                "GeneratedMass all-edge bevel rebuild audit. " +
                "mode=edgePlaneShell" +
                ",primaryFailure:{" +
                    FormatPlaneCutPrimaryFailure(audit) + "}" +
                ",cornerValid:" +
                    (cornerSolutionValid ? "1" : "0") +
                ",valid:" + audit.GeometryValid +
                ",geometryValid:" + audit.GeometryValid +
                ",coverageValid:" +
                    audit.MaterializedEdgeCoverageValid +
                ",selected:" + audit.SelectedEdgeCount +
                ",active:" + audit.ActiveEdgeCount +
                ",attemptedBuilt:" + audit.AttemptedPlanesBuilt +
                ",certifiedBuilt:" + audit.CertifiedPlanesBuilt +
                ",trialRejected:" + audit.TrialRejectedPlanes +
                ",built:" + audit.PlanesBuilt +
                ",deferred:" + audit.PlanesDeferred +
                ",rejected:" + audit.PlanesRejected +
                ",materializedCoverage:" +
                    audit.MaterializedEdgeCoverageValid +
                ",coverage:{" +
                    FormatEdgeWearCoverageSummary(
                        audit.CoverageAudit) + "}" +
                ",viabilityExclusions:{" +
                    FormatEdgeWearViabilityExclusionSummary(
                        audit.CoverageAudit,
                        false) + "}" +
                ",coexistence:{" +
                    FormatEdgeWearCoexistenceSummary(audit, false) + "}" +
                ",localityCache:{" +
                    FormatEdgeWearLocalityCacheContract(
                        audit.CoverageAudit) + "}" +
                ",coverageIds:{" +
                    FormatEdgeWearCoverageIdSummary(
                        audit.CoverageAudit) + "}" +
                ",conflictSolve:{" +
                    FormatPlaneCutEdgeConflictAudit(audit) + "}" +
                ",solverStates:{attempted=" +
                    (audit.LatestAttemptedState == null
                        ? -1
                        : audit.LatestAttemptedState.PassIndex) +
                    ",bandClean=" +
                    (audit.LatestBandCleanState == null
                        ? -1
                        : audit.LatestBandCleanState.PassIndex) +
                    ",topologyClean=" +
                    (audit.LatestTopologyCleanState == null
                        ? -1
                        : audit.LatestTopologyCleanState.PassIndex) +
                    ",certified=" +
                    (audit.LatestCertifiedState == null
                        ? -1
                        : audit.LatestCertifiedState.PassIndex) + "}" +
                ",retryFailures:{" +
                    FormatCappedPlaneCutRetryFailures(audit, 2) + "}" +
                ",polygonSurface:{" +
                    FormatPolygonSurfaceAudit(audit) + "}" +
                ",surfaceFaces:" + audit.BevelRegionFaceCount +
                FormatOneSurfaceTriangulationPolicy() +
                ",surfaceTriangles:" +
                    audit.BevelRegionTriangleCount +
                ",surfaceRenderValid:" +
                    audit.BevelRegionRenderValid +
                ",internalFanVertices:" +
                    audit.BevelRegionInternalFanVertexCount +
                ",topology:" +
                    audit.OpenEdgeCount + "/" +
                    audit.NonManifoldEdgeCount + "/" +
                    audit.TJunctionCount + "/" +
                    audit.InvalidFaceCount +
                ",faceQuality=count:" +
                    audit.FaceQualityNonPlanarCount +
                    ",planarityLimit:" +
                    audit.FaceQualityPlanarityTolerance.ToString("G9") +
                    ",spreadLimit:" +
                    audit.FaceQualityNormalSpreadToleranceDegrees
                        .ToString("G9") +
                    ",examples:{" +
                    FormatCappedPlaneCutFaceFailures(audit, 3) + "}" +
                ",numerics:{" +
                    FormatPlaneCutNumericalRepairs(
                        audit.NumericalRepairs) + "}" +
                ",openEdges=count:" + audit.OpenEdgeCount +
                    ",firstStage:" +
                    (string.IsNullOrEmpty(audit.FirstOpenEdgeStage)
                        ? "none"
                        : audit.FirstOpenEdgeStage) +
                    ",examples:{" +
                    FormatCappedPlaneCutOpenEdges(audit, 4) + "}" +
                ",tJunctions=count:" + audit.TJunctionCount +
                    ",firstStage:" +
                    (string.IsNullOrEmpty(audit.FirstTJunctionStage)
                        ? "none"
                        : audit.FirstTJunctionStage) +
                    ",examples:{" +
                    FormatCappedPlaneCutTJunctions(audit, 1) + "}" +
                ",localityDeferrals=count:" +
                    (audit.LocalityDeferrals == null
                        ? 0
                        : audit.LocalityDeferrals.Count) +
                    ",examples:{" +
                    FormatCappedPlaneCutLocalityDeferrals(audit, 2) + "}" +
                ",stageTimeline:{" +
                    FormatPlaneCutStageTimeline(audit) + "}" +
                ",meshTriangles:" + audit.PreviewTriangleCount +
                ",meshValid:" + audit.PreviewGeometryValid +
                ",edges=active:{" +
                    FormatCanonicalPlaneEdgeEvidence(
                        audit.ActiveEdgeEvidence,
                        audit.CoverageAudit) + "}" +
                    ",attempted:{" +
                    FormatCanonicalPlaneEdgeEvidence(
                        audit.AttemptedEdgeEvidence,
                        audit.CoverageAudit) + "}" +
                    ",certified:{" +
                    FormatCanonicalPlaneEdgeEvidence(
                        audit.BuiltEdgeEvidence,
                        audit.CoverageAudit) + "}" +
                    ",trialRejected:{" +
                    FormatCanonicalPlaneEdgeEvidence(
                        audit.TrialRejectedEdgeEvidence,
                        audit.CoverageAudit) + "}" +
                    ",deferred:{" +
                    FormatCanonicalPlaneEdgeEvidence(
                        audit.DeferredEdgeEvidence,
                        audit.CoverageAudit) + "}" +
                ",graphEdges=active:{" +
                    FormatRawPlaneEdgeEvidence(
                        audit.ActiveEdgeEvidence) + "}" +
                    ",attempted:{" +
                    FormatRawPlaneEdgeEvidence(
                        audit.AttemptedEdgeEvidence) + "}" +
                    ",certified:{" +
                    FormatRawPlaneEdgeEvidence(
                        audit.BuiltEdgeEvidence) + "}" +
                    ",trialRejected:{" +
                    FormatRawPlaneEdgeEvidence(
                        audit.TrialRejectedEdgeEvidence) + "}" +
                    ",deferred:{" +
                    FormatRawPlaneEdgeEvidence(
                        audit.DeferredEdgeEvidence) + "}" +
                ",telemetry=path:" + relativePath +
                    ",write:" + writeSucceeded +
                    (string.IsNullOrEmpty(writeFailure)
                        ? string.Empty
                        : ",writeFailure:" + writeFailure) +
                (string.IsNullOrEmpty(audit.Diagnostic)
                    ? string.Empty
                    : ",trace:" + audit.Diagnostic) +
                ",geometryCommit=disabled";
            LogChamferNoStackTrace(
                message,
                audit.GeometryValid != 1 ||
                (audit.CoverageAudit != null &&
                 audit.CoverageAudit.MaximumCoverageMode &&
                 audit.MaterializedEdgeCoverageValid != 1));
#endif
        }

        private static void LogBoundedSingleEdgeAudit(
            BoundedSingleEdgeAuditResult audit)
        {
#if UNITY_EDITOR
            string message =
                "GeneratedMass bounded edge compact audit. " +
                "boundedEdge=" +
                    "candidateCount:" + audit.CandidateCount +
                    ",selectedOrdinal:" + audit.SelectedOrdinal +
                    ",sourceEdge:" + audit.SourceEdgeIndex +
                    ",isolatedRailSolved:" +
                        audit.IsolatedRailSolved +
                    ",widthAttempts:" + audit.WidthAttemptCount +
                    ",scheduleComplete:" +
                        audit.IsolatedAttemptScheduleComplete +
                    ",terminalAtMinimum:" +
                        audit.IsolatedTerminalConstructionAtMinimum +
                    ",scheduleResolution:" +
                        (audit.IsolatedAttemptScheduleResolution ??
                            string.Empty) +
                    ",solvedWidth:" +
                        audit.SolvedWidth.ToString("G6") +
                    ",canonicalRails:" +
                        audit.CanonicalRailCount +
                    ",maxBoundarySnap:" +
                        audit.MaximumBoundarySnapDistance.ToString("G6") +
                    ",maxBoundaryPointTolerance:" +
                        audit.MaximumBoundaryPointTolerance.ToString("G6") +
                    ",alternateBoundaryRails:" +
                        audit.AlternateBoundaryRailCount +
                    ",maxBoundaryCandidates:" +
                        audit.MaximumBoundaryCandidateCount +
                    ",maxBoundaryDiagnosticRail:" +
                        audit.MaximumBoundaryDiagnosticRailIndex +
                    ",originalAdjacentEdge:" +
                        audit.MaximumBoundaryOriginalAdjacentEdgeIndex +
                    ",resolvedBoundaryEdge:" +
                        audit.MaximumBoundaryResolvedEdgeIndex +
                    ",originalRawParameter:" +
                        audit.MaximumBoundaryOriginalRawParameter.ToString("G6") +
                    ",originalSegmentDistance:" +
                        audit.MaximumBoundaryOriginalSegmentDistance.ToString("G6") +
                    ",minEndpointDistance:" +
                        audit.MinimumBoundaryEndpointDistance.ToString("G6") +
                    ",targetBoundaries:" +
                        audit.TargetBoundaryCount +
                    ",ownerClips:" + audit.OwnerClipCount +
                    ",boundarySubdivisions:" +
                        audit.BoundarySubdivisionCount +
                    ",bevelFaces:" + audit.BevelFaceCount +
                    ",endpointCaps:" + audit.EndpointCapCount +
                    ",modifiedSourceFaces:" +
                        audit.ModifiedSourceFaceCount +
                    ",ownerSourceFacesModified:" +
                        audit.OwnerSourceFaceModifiedCount +
                    ",endpointSupportFacesModified:" +
                        audit.EndpointSupportSourceFaceModifiedCount +
                    ",unexpectedSourceFacesModified:" +
                        audit.UnexpectedSourceFaceModifiedCount +
                    ",boundaryOnlyUnexpectedSourceFaces:" +
                        audit.BoundaryOnlyUnexpectedSourceFaceCount +
                    ",foreignSourceFacesModified:" +
                        audit.ForeignSourceFaceModifiedCount +
                    ",foreignBoundarySubdivided:" +
                        audit.ForeignBoundarySubdividedCount +
                    ",preparedSourceChangeComparisonAttempted:" +
                        audit.PreparedSourceChangeComparisonAttempted +
                    ",railDeviation:" +
                        audit.RailDeviation.ToString("G6") +
                    ",maxExtentBeyondRails:" +
                        audit.MaximumExtentBeyondRails.ToString("G6") +
                    ",valid:" + audit.GeometryValid +
                ", boundedEdgeClass=" +
                    "attempted:" + audit.EdgeClassificationAttempted +
                    ",classification:" + audit.EdgeClassification +
                    ",sourceFaceA:" + audit.EdgeSourceFaceA +
                    ",sourceFaceB:" + audit.EdgeSourceFaceB +
                    ",normalA:" + FormatBoundedAuditVector(
                        audit.EdgeNormalA) +
                    ",normalB:" + FormatBoundedAuditVector(
                        audit.EdgeNormalB) +
                    ",normalDot:" +
                        audit.EdgeNormalDot.ToString("G9") +
                    ",dihedralDegrees:" +
                        audit.EdgeDihedralDegrees.ToString("G9") +
                    ",faceAInteriorAgainstFaceB:" +
                        audit.EdgeFaceAInteriorAgainstFaceB.ToString("G9") +
                    ",faceBInteriorAgainstFaceA:" +
                        audit.EdgeFaceBInteriorAgainstFaceA.ToString("G9") +
                    ",solidCentreAgainstFaceA:" +
                        audit.EdgeSolidCentreAgainstFaceA.ToString("G9") +
                    ",solidCentreAgainstFaceB:" +
                        audit.EdgeSolidCentreAgainstFaceB.ToString("G9") +
                    ",tolerance:" +
                        audit.EdgeClassificationTolerance.ToString("G9") +
                    ",poolConvex:" + audit.ConvexCandidateCount +
                    ",poolConcave:" + audit.ConcaveCandidateCount +
                    ",poolCoplanar:" + audit.CoplanarCandidateCount +
                    ",poolAmbiguous:" + audit.AmbiguousCandidateCount +
                    ",poolInvalidOrientation:" +
                        audit.InvalidOrientationCandidateCount +
                ", boundedOwner=" +
                    "attempted:" + audit.OwnerClipAttemptedCount +
                    ",clipped:" + audit.OwnerClipCount +
                    ",intersectionFailure:" +
                        audit.OwnerIntersectionFailureCount +
                    ",degenerate:" + audit.OwnerDegenerateCount +
                    ",nonPlanar:" + audit.OwnerNonPlanarCount +
                    ",nonSimple:" + audit.OwnerNonSimpleCount +
                    ",nonConvex:" + audit.OwnerNonConvexCount +
                    ",windingFailure:" +
                        audit.OwnerWindingFailureCount +
                ", boundedEndpointSupport=" +
                    "attempted:" +
                        audit.EndpointSupportClipAttemptedCount +
                    ",clipped:" + audit.EndpointSupportClipCount +
                    ",faceA:" + audit.EndpointSupportFaceA +
                    ",additionalFaceA:" +
                        audit.EndpointSupportAdditionalFaceA +
                    ",faceB:" + audit.EndpointSupportFaceB +
                    ",additionalFaceB:" +
                        audit.EndpointSupportAdditionalFaceB +
                    ",multiFaceEndpoints:" +
                        audit.EndpointSupportMultiFaceEndpointCount +
                    ",expectedSupportFacesModified:" +
                        audit.EndpointSupportModifiedFaceExpectedCount +
                    ",multiSupportPlaneCut:" +
                        audit.MultiSupportPlaneCut +
                    ",isolatedWidthAttempts:{" +
                        FormatBoundedIsolatedWidthAttemptEvidence(audit) +
                        "}" +
                    ",singlePlaneAttempted/succeeded:" +
                        audit.MultiSupportSinglePlaneAttempted + "/" +
                        audit.MultiSupportSinglePlaneSucceeded +
                    ",singlePlaneFailure:{" +
                        (audit.MultiSupportSinglePlaneFailure ??
                            string.Empty) + "}" +
                    ",retainedHullAttempted/succeeded:" +
                        audit.MultiSupportRetainedHullAttempted + "/" +
                        audit.MultiSupportRetainedHullSucceeded +
                    ",retainedHullFailure:{" +
                        (audit.MultiSupportRetainedHullFailure ??
                            string.Empty) + "}" +
                    ",multiSupportPlaneCount:" +
                        audit.MultiSupportPlaneCount +
                    ",multiSupportCandidates:" +
                        audit.MultiSupportPlaneChainCandidateCount +
                    ",multiSupportForeignRejects:" +
                        audit.MultiSupportPlaneForeignVertexRejectCount +
                    ",multiSupportFirstForeignVertex:" +
                        audit.MultiSupportPlaneFirstForeignVertex +
                    ",multiSupportMaximumForeignDistance:" +
                        audit.MultiSupportPlaneMaximumForeignDistance
                            .ToString("G9") +
                    ",multiSupportSplit:" +
                        audit.MultiSupportPlaneSplitA.ToString("G9") +
                        "/" +
                        audit.MultiSupportPlaneSplitB.ToString("G9") +
                    ",multiSupportCapAdjacency:" +
                        audit.MultiSupportPlaneCapAdjacencyCount +
                    ",multiSupportHullPoints:" +
                        audit.MultiSupportHullPointCount +
                    ",multiSupportHullPlanes:" +
                        audit.MultiSupportHullPlaneCount +
                    ",multiSupportHullBevelFaces:" +
                        audit.MultiSupportHullBevelFaceCount +
                    ",multiSupportHullTriples:" +
                        audit.MultiSupportHullTriplesTested +
                    ",multiSupportHullSupportingTriples:" +
                        audit.MultiSupportHullSupportingTriples +
                    ",multiSupportHullEvidence:{" +
                        (audit.MultiSupportHullEvidence ?? string.Empty) +
                        "}" +
                    ",multiSupportPlaneNormal:" +
                        FormatBoundedAuditVector(
                            audit.MultiSupportPlaneNormal) +
                    ",multiSupportPlaneDistance:" +
                        audit.MultiSupportPlaneDistance.ToString("G9") +
                    ",multiSupportPlaneNormalB:" +
                        FormatBoundedAuditVector(
                            audit.MultiSupportPlaneNormalB) +
                    ",multiSupportPlaneDistanceB:" +
                        audit.MultiSupportPlaneDistanceB.ToString("G9") +
                    ",boundaryPathVertices:" +
                        audit.EndpointSupportBoundaryPathVertexCount +
                    ",graphFaceA:" +
                        audit.EndpointSupportGraphFaceA +
                    ",graphFaceB:" +
                        audit.EndpointSupportGraphFaceB +
                    ",vertexA:" + audit.EndpointSupportVertexA +
                    ",vertexB:" + audit.EndpointSupportVertexB +
                    ",previousEdgeA:" +
                        audit.EndpointSupportPreviousEdgeA +
                    ",previousEdgeB:" +
                        audit.EndpointSupportPreviousEdgeB +
                    ",nextEdgeA:" + audit.EndpointSupportNextEdgeA +
                    ",nextEdgeB:" + audit.EndpointSupportNextEdgeB +
                    ",previousRailA:" +
                        audit.EndpointSupportPreviousRailA +
                    ",previousRailB:" +
                        audit.EndpointSupportPreviousRailB +
                    ",nextRailA:" + audit.EndpointSupportNextRailA +
                    ",nextRailB:" + audit.EndpointSupportNextRailB +
                    ",sourcePositionA:" + FormatBoundedAuditVector(
                        audit.EndpointSupportSourcePositionA) +
                    ",sourcePositionB:" + FormatBoundedAuditVector(
                        audit.EndpointSupportSourcePositionB) +
                    ",previousRailPositionA:" +
                        FormatBoundedAuditVector(
                            audit.EndpointSupportPreviousRailPositionA) +
                    ",previousRailPositionB:" +
                        FormatBoundedAuditVector(
                            audit.EndpointSupportPreviousRailPositionB) +
                    ",nextRailPositionA:" + FormatBoundedAuditVector(
                        audit.EndpointSupportNextRailPositionA) +
                    ",nextRailPositionB:" + FormatBoundedAuditVector(
                        audit.EndpointSupportNextRailPositionB) +
                    ",normalA:" + FormatBoundedAuditVector(
                        audit.EndpointSupportNormalA) +
                    ",normalB:" + FormatBoundedAuditVector(
                        audit.EndpointSupportNormalB) +
                    ",previousParameterA:" +
                        audit.EndpointSupportPreviousParameterA
                            .ToString("G9") +
                    ",previousParameterB:" +
                        audit.EndpointSupportPreviousParameterB
                            .ToString("G9") +
                    ",nextParameterA:" +
                        audit.EndpointSupportNextParameterA.ToString("G9") +
                    ",nextParameterB:" +
                        audit.EndpointSupportNextParameterB.ToString("G9") +
                    ",previousEdgeResidualA:" +
                        audit.EndpointSupportPreviousEdgeResidualA
                            .ToString("G9") +
                    ",previousEdgeResidualB:" +
                        audit.EndpointSupportPreviousEdgeResidualB
                            .ToString("G9") +
                    ",nextEdgeResidualA:" +
                        audit.EndpointSupportNextEdgeResidualA
                            .ToString("G9") +
                    ",nextEdgeResidualB:" +
                        audit.EndpointSupportNextEdgeResidualB
                            .ToString("G9") +
                    ",previousPlaneResidualA:" +
                        audit.EndpointSupportPreviousPlaneResidualA
                            .ToString("G9") +
                    ",previousPlaneResidualB:" +
                        audit.EndpointSupportPreviousPlaneResidualB
                            .ToString("G9") +
                    ",nextPlaneResidualA:" +
                        audit.EndpointSupportNextPlaneResidualA
                            .ToString("G9") +
                    ",nextPlaneResidualB:" +
                        audit.EndpointSupportNextPlaneResidualB
                            .ToString("G9") +
                    ",sharedFaceFailure:" +
                        audit.EndpointSupportSharedFaceFailureCount +
                    ",incidenceFailure:" +
                        audit.EndpointSupportIncidenceFailureCount +
                    ",degenerate:" +
                        audit.EndpointSupportDegenerateCount +
                    ",nonPlanar:" +
                        audit.EndpointSupportNonPlanarCount +
                    ",nonSimple:" +
                        audit.EndpointSupportNonSimpleCount +
                    ",nonConvex:" +
                        audit.EndpointSupportNonConvexCount +
                    ",windingFailure:" +
                        audit.EndpointSupportWindingFailureCount +
                    ",removedVertices:" +
                        audit.EndpointSupportRemovedVertexCount +
                    ",railInsertions:" +
                        audit.EndpointSupportRailInsertionCount +
                ", boundedPrepare=" +
                    FormatBoundedPreparationAudit(
                        audit.ResultPreparation) +
                    ",failedCanonicalSubdivision:" +
                        audit.PrepareFailedCanonicalSubdivision +
                ", boundedSourcePrepare=" +
                    FormatBoundedPreparationAudit(
                        audit.SourcePreparation) +
                ", boundedSourceProvenance=" +
                    "certified:" +
                        audit.SourceProvenanceCertificationValid +
                    ",raw:{" +
                        FormatBoundedSourceProvenanceAudit(
                            audit.RawSourceProvenance) + "}" +
                    ",prepared:{" +
                        FormatBoundedSourceProvenanceAudit(
                            audit.PreparedSourceProvenance) + "}" +
                    ",result:{" +
                        FormatBoundedSourceProvenanceAudit(
                            audit.ResultSourceProvenance) + "}" +
                ", boundedSourceChanges=" +
                    "baseline:prepared" +
                    ",preparedAttempted:" +
                        audit.PreparedSourceChangeComparisonAttempted +
                    ",preparedModified:" +
                        audit.ModifiedSourceFaceCount +
                    ",preparedOwnerModified:" +
                        audit.OwnerSourceFaceModifiedCount +
                    ",preparedSupportModified:" +
                        audit.EndpointSupportSourceFaceModifiedCount +
                    ",preparedUnexpectedModified:" +
                        audit.UnexpectedSourceFaceModifiedCount +
                    ",preparedBoundaryOnlyUnexpected:" +
                        audit.BoundaryOnlyUnexpectedSourceFaceCount +
                    ",preparedForeignModified:" +
                        audit.ForeignSourceFaceModifiedCount +
                    ",preparedForeignBoundarySubdivided:" +
                        audit.ForeignBoundarySubdividedCount +
                    ",rawModified:" +
                        audit.RawModifiedSourceFaceCount +
                    ",rawOwnerModified:" +
                        audit.RawOwnerSourceFaceModifiedCount +
                    ",rawSupportModified:" +
                        audit.RawEndpointSupportSourceFaceModifiedCount +
                    ",rawUnexpectedModified:" +
                        audit.RawUnexpectedSourceFaceModifiedCount +
                    ",rawBoundaryOnlyUnexpected:" +
                        audit.RawBoundaryOnlyUnexpectedSourceFaceCount +
                    ",rawForeignModified:" +
                        audit.RawForeignSourceFaceModifiedCount +
                    ",rawForeignBoundarySubdivided:" +
                        audit.RawForeignBoundarySubdividedCount +
                ", boundedTopology=" +
                    "open:" + audit.OpenEdgeCount +
                    ",nonManifold:" + audit.NonManifoldEdgeCount +
                    ",tJunction:" + audit.TJunctionCount +
                    ",invalidFaces:" + audit.InvalidFaceCount +
                ", boundedBounds=" +
                    "attempted:" + audit.CertificationAttempted +
                    ",rawValid:" + audit.BoundsValid +
                    ",preparedValid:" +
                        audit.PreparedBoundsValid +
                    ",tolerance:" +
                        audit.BoundsTolerance.ToString("G9") +
                    ",rawMin:" + FormatBoundedAuditVector(
                        audit.RawSourceBoundsMinimum) +
                    ",rawMax:" + FormatBoundedAuditVector(
                        audit.RawSourceBoundsMaximum) +
                    ",preparedMin:" + FormatBoundedAuditVector(
                        audit.PreparedSourceBoundsMinimum) +
                    ",preparedMax:" + FormatBoundedAuditVector(
                        audit.PreparedSourceBoundsMaximum) +
                    ",resultMin:" + FormatBoundedAuditVector(
                        audit.ResultBoundsMinimum) +
                    ",resultMax:" + FormatBoundedAuditVector(
                        audit.ResultBoundsMaximum) +
                    ",rawMinMargin:" + FormatBoundedAuditVector(
                        audit.RawBoundsMinimumMargin) +
                    ",rawMaxMargin:" + FormatBoundedAuditVector(
                        audit.RawBoundsMaximumMargin) +
                    ",preparedMinMargin:" +
                        FormatBoundedAuditVector(
                            audit.PreparedBoundsMinimumMargin) +
                    ",preparedMaxMargin:" +
                        FormatBoundedAuditVector(
                            audit.PreparedBoundsMaximumMargin) +
                ", boundedSolid=" +
                    "sourceConvexityAttempted:" +
                        audit.SourceConvexityAttempted +
                    ",sourceConvexityViolations:" +
                        audit.SourceConvexityViolationCount +
                    ",sourceMaximumPlaneViolation:" +
                        audit.SourceMaximumPlaneViolation.ToString("G9") +
                    ",sourceViolatingPlaneFace:" +
                        audit.SourceViolatingPlaneFace +
                    ",sourceViolatingVertex:" +
                        audit.SourceViolatingVertexFace + ":" +
                        audit.SourceViolatingVertexIndex +
                    ",resultContainmentAttempted:" +
                        audit.ResultContainmentAttempted +
                    ",resultContainmentViolations:" +
                        audit.ResultContainmentViolationCount +
                    ",resultMaximumOutwardDistance:" +
                        audit.ResultMaximumOutwardDistance.ToString("G9") +
                    ",resultViolatingFace:" +
                        audit.ResultViolatingFace +
                    ",resultViolatingProvenance:" +
                        audit.ResultViolatingProvenanceKind + ":" +
                        audit.ResultViolatingProvenanceIndex +
                    ",resultViolatingVertex:" +
                        audit.ResultViolatingVertexIndex +
                    ",violatedSourcePlane:" +
                        audit.ResultViolatedSourcePlane +
                    ",tolerance:" +
                        audit.SolidContainmentTolerance.ToString("G9") +
                ", boundedResultConvexity=" +
                    "attempted:" + audit.ResultConvexityAttempted +
                    ",violations:" +
                        audit.ResultConvexityViolationCount +
                    ",maximumViolation:" +
                        audit.ResultMaximumConvexityViolation
                            .ToString("G9") +
                    ",planeFace:" + audit.ResultConvexityPlaneFace +
                    ",planeProvenance:" +
                        audit.ResultConvexityPlaneProvenanceKind + ":" +
                        audit.ResultConvexityPlaneProvenanceIndex +
                    ",vertexFace:" + audit.ResultConvexityVertexFace +
                    ",vertexProvenance:" +
                        audit.ResultConvexityVertexProvenanceKind + ":" +
                        audit.ResultConvexityVertexProvenanceIndex +
                    ",vertexIndex:" +
                        audit.ResultConvexityVertexIndex +
                ", boundedFaceIntersections=" +
                    "sourceAttempted:" +
                        audit.SourceFaceIntersectionAttempted +
                    ",sourcePairs:" +
                        audit.SourceFaceIntersectionPairCount +
                    ",sourceCoplanar:" +
                        audit.SourceCoplanarOverlapPairCount +
                    ",sourceNonCoplanar:" +
                        audit.SourceNonCoplanarIntersectionPairCount +
                    ",sourceBoundaryContacts:" +
                        audit.SourceBoundaryContactPairCount +
                    ",sourceImproperInterior:" +
                        audit.SourceImproperInteriorPairCount +
                    ",resultAttempted:" +
                        audit.FaceIntersectionAttempted +
                    ",resultPairs:" +
                        audit.FaceIntersectionPairCount +
                    ",resultCoplanar:" +
                        audit.CoplanarOverlapPairCount +
                    ",resultNonCoplanar:" +
                        audit.NonCoplanarIntersectionPairCount +
                    ",resultBoundaryContacts:" +
                        audit.ResultBoundaryContactPairCount +
                    ",resultImproperInterior:" +
                        audit.ResultImproperInteriorPairCount +
                    ",unchanged:" +
                        audit.UnchangedIntersectionPairCount +
                    ",changed:" +
                        audit.ChangedIntersectionPairCount +
                    ",new:" + audit.NewIntersectionPairCount +
                    ",newBoundaryContacts:" +
                        audit.NewBoundaryContactPairCount +
                    ",newInterior:" +
                        audit.NewImproperInteriorIntersectionPairCount +
                    ",changedInterior:" +
                        audit.ChangedImproperInteriorIntersectionPairCount +
                    ",introducedInterior:" +
                        audit.IntroducedImproperInteriorIntersectionPairCount +
                    ",resolved:" +
                        audit.ResolvedIntersectionPairCount +
                    ",firstResultA:" + audit.FirstIntersectionFaceA +
                    ",firstResultAProvenance:" +
                        audit.FirstIntersectionFaceAProvenanceKind + ":" +
                        audit.FirstIntersectionFaceAProvenanceIndex +
                    ",firstResultB:" + audit.FirstIntersectionFaceB +
                    ",firstResultBProvenance:" +
                        audit.FirstIntersectionFaceBProvenanceKind + ":" +
                        audit.FirstIntersectionFaceBProvenanceIndex +
                    ",sourceEvidence:{" +
                        FormatBoundedAuditEvidence(
                            audit.SourceIntersectionPairEvidence) + "}" +
                    ",resultEvidence:{" +
                        FormatBoundedAuditEvidence(
                            audit.ResultIntersectionPairEvidence) + "}" +
                    ",unchangedEvidence:{" +
                        FormatBoundedAuditEvidence(
                            audit.UnchangedIntersectionPairEvidence) + "}" +
                    ",changedEvidence:{" +
                        FormatBoundedAuditEvidence(
                            audit.ChangedIntersectionPairEvidence) + "}" +
                    ",newEvidence:{" +
                        FormatBoundedAuditEvidence(
                            audit.NewIntersectionPairEvidence) + "}" +
                    ",resolvedEvidence:{" +
                        FormatBoundedAuditEvidence(
                            audit.ResolvedIntersectionPairEvidence) + "}" +
                ", boundedVolume=" +
                    "rawSource:" +
                        audit.SourceVolume.ToString("G9") +
                    ",preparedSource:" +
                        audit.PreparedSourceVolume.ToString("G9") +
                    ",result:" +
                        audit.ResultVolume.ToString("G9") +
                    ",rawRatio:" +
                        audit.RawVolumeRatio.ToString("G9") +
                    ",preparedRatio:" +
                        audit.VolumeRatio.ToString("G9") +
                    ",sourcePreparationRatio:" +
                        audit.SourcePreparationVolumeRatio.ToString("G9") +
                    ",rawDelta:" +
                        audit.RawVolumeDelta.ToString("G9") +
                    ",preparedDelta:" +
                        audit.PreparedVolumeDelta.ToString("G9") +
                    ",minimumRatio:" +
                        audit.VolumeMinimumRatio.ToString("G9") +
                    ",maximumRatio:" +
                        audit.VolumeMaximumRatio.ToString("G9") +
                    ",lowerMargin:" +
                        audit.VolumeLowerMargin.ToString("G9") +
                    ",upperMargin:" +
                        audit.VolumeUpperMargin.ToString("G9") +
                    ",valid:" + audit.VolumeValid +
                ", boundedLocalVolume=" +
                    "attempted:" + audit.LocalVolumeAttempted +
                    ",sourceSigned:" +
                        audit.SourceSignedVolume.ToString("G12") +
                    ",preparedSourceSigned:" +
                        audit.PreparedSourceSignedVolume.ToString("G12") +
                    ",resultSigned:" +
                        audit.ResultSignedVolume.ToString("G12") +
                    ",resultAbsolute:" +
                        audit.ResultAbsoluteVolume.ToString("G12") +
                    ",originalOwnerA:" +
                        audit.OriginalOwnerAContribution.ToString("G12") +
                    ",originalOwnerB:" +
                        audit.OriginalOwnerBContribution.ToString("G12") +
                    ",originalOwnerTotal:" +
                        audit.OriginalOwnerContribution.ToString("G12") +
                    ",replacementOwnerA:" +
                        audit.ReplacementOwnerAContribution.ToString("G12") +
                    ",replacementOwnerB:" +
                        audit.ReplacementOwnerBContribution.ToString("G12") +
                    ",replacementOwnerTotal:" +
                        audit.ReplacementOwnerContribution.ToString("G12") +
                    ",originalSupportA:" +
                        audit.OriginalSupportAContribution.ToString("G12") +
                    ",originalSupportB:" +
                        audit.OriginalSupportBContribution.ToString("G12") +
                    ",originalSupportTotal:" +
                        audit.OriginalSupportContribution.ToString("G12") +
                    ",replacementSupportA:" +
                        audit.ReplacementSupportAContribution.ToString("G12") +
                    ",replacementSupportB:" +
                        audit.ReplacementSupportBContribution.ToString("G12") +
                    ",replacementSupportTotal:" +
                        audit.ReplacementSupportContribution.ToString("G12") +
                    ",bevel:" +
                        audit.BevelContribution.ToString("G12") +
                    ",capA:" +
                        audit.CapAContribution.ToString("G12") +
                    ",capB:" +
                        audit.CapBContribution.ToString("G12") +
                    ",originalForeign:" +
                        audit.OriginalForeignContribution.ToString("G12") +
                    ",resultForeign:" +
                        audit.ResultForeignContribution.ToString("G12") +
                    ",foreignDelta:" +
                        audit.ForeignContributionDelta.ToString("G12") +
                    ",localReplacementDelta:" +
                        audit.LocalReplacementDelta.ToString("G12") +
                    ",globalSignedDelta:" +
                        audit.GlobalSignedVolumeDelta.ToString("G12") +
                    ",localGlobalResidual:" +
                        audit.LocalGlobalResidual.ToString("G12") +
                ", boundedCertification=" +
                    "attempted:" + audit.CertificationAttempted +
                    ",facesReoriented:" +
                        audit.FacesReoriented +
                    ",outwardWindingFailures:" +
                        audit.OutwardWindingFailureCount +
                ", boundedBevelPlane=" +
                    "attempted:" + audit.BevelPlaneAttempted +
                    ",planeNormal:" + FormatBoundedAuditVector(
                        audit.BevelPlaneNormal) +
                    ",faceNormal:" + FormatBoundedAuditVector(
                        audit.BevelFaceNormal) +
                    ",normalAgreement:" +
                        audit.BevelPlaneNormalAgreement.ToString("G9") +
                    ",distance:" +
                        audit.BevelPlaneDistance.ToString("G9") +
                    ",solidCentreSide:" +
                        audit.BevelSolidCentreSide.ToString("G9") +
                    ",sourceEdgeASide:" +
                        audit.BevelSourceEdgeASide.ToString("G9") +
                    ",sourceEdgeBSide:" +
                        audit.BevelSourceEdgeBSide.ToString("G9") +
                    ",railMaxResidual:" +
                        audit.BevelRailMaximumPlaneResidual.ToString("G9") +
                ", boundedVolumeCrossCheck=" +
                    "triangulationAttempted:" +
                        audit.DiagnosticTriangulationAttempted +
                    ",triangleSoupValid:" +
                        audit.DiagnosticTriangleSoupValid +
                    ",triangleSigned:" +
                        audit.DiagnosticTriangleSignedVolume.ToString("G12") +
                    ",triangleAbsolute:" +
                        audit.DiagnosticTriangleVolume.ToString("G12") +
                    ",polygonTriangleDelta:" +
                        audit.PolygonTriangleVolumeDelta.ToString("G12") +
                    ",polygonTriangleSignedDelta:" +
                        audit.PolygonTriangleSignedVolumeDelta.ToString("G12") +
                ", boundedPolygonSurface={" +
                    FormatPolygonSurfaceAudit(audit) + "}" +
                ", boundedBevelRegion=" +
                    "polygonFaces:" + audit.BevelRegionFaceCount +
                    FormatOneSurfaceTriangulationPolicy() +
                    ",boundaryVertices:" +
                        audit.BevelRegionBoundaryVertexCount +
                    ",triangles:" + audit.BevelRegionTriangleCount +
                    ",authoredNormalTriangles:" +
                        audit.BevelRegionAuthoredNormalTriangleCount +
                    ",authoredSurfaceGroupTriangles:" +
                        audit.BevelRegionAuthoredSurfaceGroupTriangleCount +
                    ",internalFanVertices:" +
                        audit.BevelRegionInternalFanVertexCount +
                    ",maxPlaneResidual:" +
                        audit.BevelRegionMaximumPlaneResidual
                            .ToString("G9") +
                    ",maxGeometricNormalDeviationDegrees:" +
                        audit.BevelRegionMaximumNormalDeviationDegrees
                            .ToString("G9") +
                    ",renderValid:" +
                        audit.BevelRegionRenderValid +
                    ",failureFace:" +
                        audit.BevelRegionFailureFace +
                    ",failureProvenance:" +
                        audit.BevelRegionFailureProvenanceIndex +
                    ",failureReason:" +
                        (string.IsNullOrEmpty(
                            audit.BevelRegionFailureReason)
                            ? "none"
                            : audit.BevelRegionFailureReason) +
                ", boundedMesh=" +
                    "triangles:" + audit.PreviewTriangleCount +
                    ",triangulatedFaces:" +
                        audit.TriangulatedFaceCount +
                    ",degenerate:" +
                        audit.PreviewDegenerateTriangleCount +
                    ",open:" + audit.PreviewOpenEdgeCount +
                    ",nonManifold:" +
                        audit.PreviewNonManifoldEdgeCount +
                    ",winding:" +
                        audit.PreviewWindingFailureCount +
                    ",bounds:" + audit.PreviewBoundsFailureCount +
                    ",volume:" + audit.PreviewVolumeFailureCount +
                    ",failureFace:" +
                        audit.TriangulationFailureFace +
                    ",failureKind:" +
                        audit.TriangulationFailureKind +
                    ",failureProvenance:" +
                        audit.TriangulationFailureProvenanceKind + ":" +
                        audit.TriangulationFailureProvenanceIndex +
                    ",failureReason:" +
                        (string.IsNullOrEmpty(
                            audit.TriangulationFailureReason)
                            ? "none"
                            : audit.TriangulationFailureReason) +
                (string.IsNullOrEmpty(audit.Diagnostic)
                    ? string.Empty
                    : ", boundedTrace=" + audit.Diagnostic) +
                ", geometryCommit=disabled";
            LogChamferNoStackTrace(message, audit.GeometryValid != 1);
#endif
        }

        private static string FormatBoundedPreparationAudit(
            BoundedPreparationAudit audit)
        {
            return "attempted:" + audit.Attempted +
                ",succeeded:" + audit.Succeeded +
                ",inputFaces:" + audit.InputFaceCount +
                ",inputVertices:" + audit.InputVertexCount +
                ",inputUniqueVertices:" +
                    audit.InputUniqueVertexCount +
                ",outputFaces:" + audit.OutputFaceCount +
                ",outputVertices:" + audit.OutputVertexCount +
                ",outputUniqueVertices:" +
                    audit.OutputUniqueVertexCount +
                ",welded:" + audit.Welded +
                ",conformed:" + audit.ConformedCount +
                ",seamPairs:" + audit.SeamRepairCount +
                ",seamTouchedFaces:" +
                    audit.SeamTouchedFaceCount +
                ",inputOpen:" + audit.InputOpenEdgeCount +
                ",inputNonManifold:" +
                    audit.InputNonManifoldEdgeCount +
                ",inputTJunction:" + audit.InputTJunctionCount +
                ",inputInvalidFaces:" +
                    audit.InputInvalidFaceCount +
                ",outputOpen:" + audit.OutputOpenEdgeCount +
                ",outputNonManifold:" +
                    audit.OutputNonManifoldEdgeCount +
                ",outputTJunction:" + audit.OutputTJunctionCount +
                ",outputInvalidFaces:" +
                    audit.OutputInvalidFaceCount +
                ",inputVolume:" +
                    audit.InputVolume.ToString("G9") +
                ",outputVolume:" +
                    audit.OutputVolume.ToString("G9") +
                ",volumeDelta:" +
                    audit.VolumeDelta.ToString("G9") +
                ",volumeRatio:" +
                    audit.VolumeRatio.ToString("G9") +
                ",failedStage:" +
                    (string.IsNullOrEmpty(audit.FailedStage)
                        ? "none"
                        : audit.FailedStage) +
                ",failedFace:" + audit.FailedFace +
                ",failedKind:" + audit.FailedKind +
                ",failedProvenance:" +
                    audit.FailedProvenanceKind + ":" +
                    audit.FailedProvenanceIndex +
                ",degenerate:" + audit.DegenerateCount +
                ",nonPlanar:" + audit.NonPlanarCount +
                ",nonSimple:" + audit.NonSimpleCount +
                ",nonConvex:" + audit.NonConvexCount +
                ",windingFailure:" +
                    audit.WindingFailureCount;
        }

        private static string FormatOneSurfaceTriangulationPolicy()
        {
            return ",triangulationPolicy:" +
                "direct-preferred/general-complete/collinear-reinsert";
        }

        private static string FormatPolygonSurfaceAudit(
            PlaneCutBevelAuditResult audit)
        {
            return "faces:" + audit.PolygonSurfaceFaceCount +
                FormatOneSurfaceTriangulationPolicy() +
                ",boundaryVertices:" +
                    audit.PolygonSurfaceBoundaryVertexCount +
                ",expectedTriangles:" +
                    audit.PolygonSurfaceExpectedTriangleCount +
                ",triangles:" + audit.PolygonSurfaceTriangleCount +
                ",authoredNormalTriangles:" +
                    audit.PolygonSurfaceAuthoredNormalTriangleCount +
                ",authoredSurfaceGroupTriangles:" +
                    audit.PolygonSurfaceAuthoredSurfaceGroupTriangleCount +
                ",internalFanVertices:" +
                    audit.PolygonSurfaceInternalFanVertexCount +
                ",surfaceGroupCollisions:" +
                    audit.PolygonSurfaceGroupCollisionCount +
                ",collision:" +
                    audit.PolygonSurfaceGroupCollisionSurfaceGroup + ":" +
                    audit.PolygonSurfaceGroupCollisionFirstFace + ":" +
                    audit.PolygonSurfaceGroupCollisionSecondFace +
                ",maxPlaneResidual:" +
                    audit.PolygonSurfaceMaximumPlaneResidual.ToString("G9") +
                ",maxNormalDeviationDegrees:" +
                    audit.PolygonSurfaceMaximumNormalDeviationDegrees
                        .ToString("G9") +
                ",renderValid:" + audit.PolygonSurfaceRenderValid +
                ",failureFace:" + audit.PolygonSurfaceFailureFace +
                ",failureProvenance:" +
                    audit.PolygonSurfaceFailureProvenanceIndex +
                ",failureReason:" +
                    (string.IsNullOrEmpty(
                        audit.PolygonSurfaceFailureReason)
                        ? "none"
                        : audit.PolygonSurfaceFailureReason);
        }

        private static string FormatPolygonSurfaceAudit(
            BoundedSingleEdgeAuditResult audit)
        {
            return "faces:" + audit.PolygonSurfaceFaceCount +
                FormatOneSurfaceTriangulationPolicy() +
                ",boundaryVertices:" +
                    audit.PolygonSurfaceBoundaryVertexCount +
                ",expectedTriangles:" +
                    audit.PolygonSurfaceExpectedTriangleCount +
                ",triangles:" + audit.PolygonSurfaceTriangleCount +
                ",authoredNormalTriangles:" +
                    audit.PolygonSurfaceAuthoredNormalTriangleCount +
                ",authoredSurfaceGroupTriangles:" +
                    audit.PolygonSurfaceAuthoredSurfaceGroupTriangleCount +
                ",internalFanVertices:" +
                    audit.PolygonSurfaceInternalFanVertexCount +
                ",surfaceGroupCollisions:" +
                    audit.PolygonSurfaceGroupCollisionCount +
                ",collision:" +
                    audit.PolygonSurfaceGroupCollisionSurfaceGroup + ":" +
                    audit.PolygonSurfaceGroupCollisionFirstFace + ":" +
                    audit.PolygonSurfaceGroupCollisionSecondFace +
                ",maxPlaneResidual:" +
                    audit.PolygonSurfaceMaximumPlaneResidual.ToString("G9") +
                ",maxNormalDeviationDegrees:" +
                    audit.PolygonSurfaceMaximumNormalDeviationDegrees
                        .ToString("G9") +
                ",renderValid:" + audit.PolygonSurfaceRenderValid +
                ",failureFace:" + audit.PolygonSurfaceFailureFace +
                ",failureProvenance:" +
                    audit.PolygonSurfaceFailureProvenanceIndex +
                ",failureReason:" +
                    (string.IsNullOrEmpty(
                        audit.PolygonSurfaceFailureReason)
                        ? "none"
                        : audit.PolygonSurfaceFailureReason);
        }

        private static string FormatBoundedSourceProvenanceAudit(
            BoundedSourceProvenanceAudit audit)
        {
            return "attempted:" + audit.Attempted +
                ",valid:" + audit.Valid +
                ",expected:" + audit.ExpectedSourceFaceCount +
                ",totalFaces:" + audit.TotalFaceCount +
                ",sourceFaces:" +
                    audit.SourceProvenanceFaceCount +
                ",uniqueValid:" +
                    audit.ValidUniqueSourceFaceCount +
                ",missing:" + audit.MissingSourceFaceCount +
                ",duplicates:" +
                    audit.DuplicateSourceFaceCount +
                ",outOfRange:" +
                    audit.OutOfRangeSourceFaceCount +
                ",nonSource:" + audit.NonSourceFaceCount +
                ",nullFaces:" + audit.NullFaceCount +
                ",firstMissing:" + audit.FirstMissingSourceFace +
                ",firstDuplicate:" +
                    audit.FirstDuplicateSourceFace +
                ",firstOutOfRange:" +
                    audit.FirstOutOfRangeSourceFace;
        }

        private static string FormatBoundedAuditEvidence(
            string value)
        {
            return string.IsNullOrEmpty(value)
                ? "none"
                : value;
        }

        private static string FormatBoundedAuditVector(
            Vector3 value)
        {
            return "(" + value.x.ToString("G9") + "/" +
                value.y.ToString("G9") + "/" +
                value.z.ToString("G9") + ")";
        }

        private static void LogChamferEmissionAudit(
            ChamferEmissionStats stats,
            bool ready,
            string blocker,
            PlaneCutBevelAuditResult planeCutAudit)
        {
#if UNITY_EDITOR
            string message =
                "GeneratedMass edge wear compact audit. " +
                "selected=" + stats.ActiveSelectedEdgeCount + "/" +
                    stats.CandidateSelectedEdgeCount +
                ", replacement=" + stats.ReplacementFacesBuilt + "/" +
                    stats.ReplacementFacesAttempted +
                ", bevel=" + stats.BevelStripsBuilt + "/" +
                    stats.BevelStripsAttempted +
                ", livePatch=" + stats.PatchLoopsBuilt + "/" +
                    stats.PatchLoopsAttempted +
                ", correctedPatch=" + stats.PatchCorrectedLoopsBuilt +
                    "/" + stats.PatchCorrectedLoopsAttempted +
                ", baselineRejected=" +
                    stats.PatchCorrectedBaselineLoopsRejected +
                ", overlap=" + stats.PatchOverlapLoopsClassified + ":" +
                    stats.PatchOverlapPatchContainedInReplacement + "/" +
                    stats.PatchOverlapReplacementContainedInPatch + "/" +
                    stats.PatchOverlapPartialCoplanarArea + "/" +
                    stats.PatchOverlapNonCoplanarPenetration + "/" +
                    stats.PatchOverlapBevelStripPenetration + "/" +
                    stats.PatchOverlapUnclassified +
                ", overlapOwner=" + stats.PatchOverlapBoundaryOwner +
                    "/" + stats.PatchOverlapNonBoundaryOwner +
                ", overlapArea=" +
                    (stats.PatchOverlapProjectedAreaNanounits /
                        1000000000.0).ToString("G6") +
                ", contained=" +
                    stats.PatchContainedOwnershipCandidates + "/" +
                    stats.PatchContainedOwnershipResolved + "/" +
                    stats.PatchContainedOwnershipStillRequired + "/" +
                    stats.PatchContainedOwnershipOwnerAmbiguous + "/" +
                    stats.PatchContainedOwnershipBoundaryTransferFailures +
                    "/" +
                    stats.PatchContainedOwnershipTopologyFailures +
                ", containedRepartition=" +
                    stats.PatchContainedRepartitionCandidates + "/" +
                    stats.PatchContainedRepartitionResolved + "/" +
                    stats.PatchContainedRepartitionArrangementFailures + "/" +
                    stats.PatchContainedRepartitionTriangulationFailures +
                    "/" + stats.PatchContainedRepartitionAreaFailures +
                    "/" + stats.PatchContainedRepartitionBoundaryFailures +
                    "/" + stats.PatchContainedRepartitionTopologyFailures +
                    "/" + stats.PatchContainedRepartitionOverlapRemaining +
                ", containedRepair=" +
                    stats.PatchContainedRepairCandidates + "/" +
                    stats.PatchContainedRepairGuidedResiduals + "/" +
                    stats.PatchContainedRepairGenericFallbacks + "/" +
                    stats.PatchContainedRepairEndpointAligned + "/" +
                    stats.PatchContainedRepairResolved + "/" +
                    stats.PatchContainedRepairBuildFailures + "/" +
                    stats.PatchContainedRepairBoundaryFailures + "/" +
                    stats.PatchContainedRepairTopologyFailures + "/" +
                    stats.PatchContainedRepairOverlapRemaining +
                ", containedBoundary=" +
                    stats.PatchContainedBoundaryCandidates + "/" +
                    stats.PatchContainedBoundaryExactValid + "/" +
                    stats.PatchContainedBoundarySplitEquivalent + "/" +
                    stats.PatchContainedBoundaryResidualMissing + "/" +
                    stats.PatchContainedBoundaryExternalUnsplit + "/" +
                    stats.PatchContainedBoundaryUnderused + "/" +
                    stats.PatchContainedBoundaryOverused + "/" +
                    stats.PatchContainedBoundaryAmbiguous +
                ", containedBoundarySegments=" +
                    stats.PatchContainedBoundarySegments + "/" +
                    stats.PatchContainedBoundarySegmentExactValid + "/" +
                    stats.PatchContainedBoundarySegmentSplitEquivalent + "/" +
                    stats.PatchContainedBoundarySegmentResidualMissing + "/" +
                    stats.PatchContainedBoundarySegmentExternalUnsplit + "/" +
                    stats.PatchContainedBoundarySegmentUnderused + "/" +
                    stats.PatchContainedBoundarySegmentOverused + "/" +
                    stats.PatchContainedBoundarySegmentAmbiguous +
                ", containedShadow=" +
                    stats.PatchContainedShadowTested + "/" +
                    stats.PatchContainedShadowOverlapRemoved + "/" +
                    stats.PatchContainedShadowTopologyClean + "/" +
                    stats.PatchContainedShadowTJunctionIncrease + "/" +
                    stats.PatchContainedShadowUnexpectedOpenEdgeIncrease +
                    "/" + stats.PatchContainedShadowSourceBoundaryIncrease +
                    "/" + stats.PatchContainedShadowNonManifoldIncrease +
                ", containedCombined=" +
                    stats.PatchContainedCombinedAttempted + "/" +
                    stats.PatchContainedCombinedApplied + "/" +
                    stats.PatchContainedCombinedOwnerConflicts + "/" +
                    stats.PatchContainedCombinedTopologyFailures + "/" +
                    stats.PatchContainedCombinedRemainingOverlaps +
                ", planeBevel=" +
                    planeCutAudit.SelectedEdgeCount + "/" +
                    planeCutAudit.ActiveEdgeCount + "/" +
                    planeCutAudit.PlanesBuilt + "/" +
                    planeCutAudit.PlanesLocalized + "/" +
                    planeCutAudit.PlanesDeferred + "/" +
                    planeCutAudit.PlanesRejected + "/" +
                    planeCutAudit.CapsBuilt + "/" +
                    planeCutAudit.CapsMissing + "/" +
                    planeCutAudit.CapsRedundant + "/" +
                    planeCutAudit.ConformalSplitCount + "/" +
                    planeCutAudit.SeamPairCount + "/" +
                    planeCutAudit.OpenEdgeCount + "/" +
                    planeCutAudit.NonManifoldEdgeCount + "/" +
                    planeCutAudit.TJunctionCount + "/" +
                    planeCutAudit.InvalidFaceCount + "/" +
                    planeCutAudit.GeometryValid +
                ", planeVertexJunction=" +
                    planeCutAudit.VertexJunctionCandidateCount + "/" +
                    planeCutAudit.VertexJunctionDirectBuiltCount + "/" +
                    planeCutAudit.VertexJunctionAdaptiveBuiltCount + "/" +
                    planeCutAudit.VertexJunctionBacktrackBuiltCount + "/" +
                    planeCutAudit.VertexJunctionCleanSharpCount + "/" +
                    planeCutAudit.VertexJunctionUnresolvedCount + "/" +
                    planeCutAudit.VertexJunctionTriangleCapCount + "/" +
                    planeCutAudit.VertexJunctionQuadCapCount + "/" +
                    planeCutAudit.VertexJunctionLargerCapCount + "/" +
                    planeCutAudit.VertexJunctionEdgesDeferredCount + "/" +
                    planeCutAudit.VertexJunctionRebuildPassCount +
                ", planeSolve=" +
                    planeCutAudit.SolveStatesEvaluated + "/" +
                    planeCutAudit.SolveJunctionsVisited + "/" +
                    planeCutAudit.SolveCandidateTrials + "/" +
                    planeCutAudit.SolveSystemRebuilds + "/" +
                    planeCutAudit.SolvePolygonAudits + "/" +
                    planeCutAudit.SolveTriangleAudits + "/" +
                    planeCutAudit.SolveEdgesDeferred + "/" +
                    planeCutAudit.SolveElapsedMilliseconds + "/" +
                    planeCutAudit.SolveTimedOut +
                ", planeFaceQuality=" +
                    planeCutAudit.FaceQualityFaceCount + "/" +
                    planeCutAudit.FaceQualitySeamTouchedFaceCount + "/" +
                    planeCutAudit.FaceQualityNonPlanarCount + "/" +
                    planeCutAudit.FaceQualityElongatedJunctionCount + "/" +
                    planeCutAudit.FaceQualityMaxPlaneDeviation
                        .ToString("G6") + "/" +
                    planeCutAudit.FaceQualityMaxNormalSpreadDegrees
                        .ToString("G6") + "/" +
                    planeCutAudit.FaceQualityMinimumJunctionCompactness
                        .ToString("G6") + "/" +
                    planeCutAudit.FaceQualityMaximumJunctionAspectRatio
                        .ToString("G6") + "/" +
                    planeCutAudit.FaceQualityWorstVertexCount +
                ", planeBand=" +
                    FormatPlaneCutBandAudit(planeCutAudit) +
                ", edgeConflict=" +
                    FormatPlaneCutEdgeConflictAudit(planeCutAudit) +
                ", localJunction=" +
                    FormatPlaneCutLocalJunctionAudit(planeCutAudit) +
                ", planeMesh=" +
                    planeCutAudit.PreviewTriangleCount + "/" +
                    planeCutAudit.PreviewDegenerateTriangleCount + "/" +
                    planeCutAudit.PreviewOpenEdgeCount + "/" +
                    planeCutAudit.PreviewNonManifoldEdgeCount + "/" +
                    planeCutAudit.PreviewWindingFailureCount + "/" +
                    planeCutAudit.PreviewBoundsFailureCount + "/" +
                    planeCutAudit.PreviewVolumeFailureCount + "/" +
                    planeCutAudit.PreviewGeometryValid +
                (string.IsNullOrEmpty(planeCutAudit.Diagnostic)
                    ? string.Empty
                    : ", planeTrace=" + planeCutAudit.Diagnostic) +
                ", sector=" + stats.PatchSectorAuthoritativeLoops +
                    "/" + stats.PatchSectorExistingPlanLoops +
                ", sectorOwned=" +
                    stats.PatchSectorBoundaryHalfEdgesAssigned + "/" +
                    stats.PatchSectorBoundaryHalfEdges +
                ", sliver=" +
                    stats.PatchCorrectedReservedSliverTriangles +
                    "/" + stats.PatchCorrectedReservedSliverLoops +
                ", sliverDelta=" +
                    stats.PatchSliverDeltaPreCollapseComponents + "/" +
                    stats.PatchSliverDeltaPostCollapseComponents + "/" +
                    stats.PatchSliverDeltaReservedPreComponents + "/" +
                    stats.PatchSliverDeltaExactComponentMatches + "/" +
                    stats.PatchSliverDeltaDisappearedComponents + "/" +
                    stats.PatchSliverDeltaMergedPostComponents + "/" +
                    stats.PatchSliverDeltaSplitPreComponents + "/" +
                    stats.PatchSliverDeltaMissingLoopCount +
                (string.IsNullOrEmpty(stats.PatchSliverDeltaDiagnostic)
                    ? string.Empty
                    : ", sliverTrace=" +
                        stats.PatchSliverDeltaDiagnostic) +
                ", boundaryOccurrence=" +
                    stats.PatchCorrectedBoundaryMissingOpposite + "/" +
                    stats.PatchCorrectedBoundaryDuplicateOpposite + "/" +
                    stats.PatchCorrectedBoundaryDirectionMismatch + "/" +
                    stats.PatchCorrectedBoundaryExtraPatchEdge +
                (string.IsNullOrEmpty(
                        stats.PatchCorrectedBoundaryOccurrenceDiagnostic)
                    ? string.Empty
                    : ", boundaryTrace=" +
                        stats.PatchCorrectedBoundaryOccurrenceDiagnostic) +
                ", final=" +
                    stats.PatchCorrectedFinalUnexpectedOpenEdges + "/" +
                    stats.PatchCorrectedFinalNonManifoldEdges + "/" +
                    stats.PatchCorrectedFinalTJunctions +
                ", readyLive=" +
                    stats.ReadyForChamferPatchTopology +
                ", readyCorrected=" +
                    stats.ReadyForCorrectedChamferPatchTopology +
                ", geometryCommit=disabled";
            if (!string.IsNullOrEmpty(blocker))
            {
                message += ", blocker=" + blocker;
            }
            if (!ShouldSuppressChamferCompactSummary(
                    stats.DiagnosticGeometrySignature,
                    message))
            {
                LogChamferNoStackTrace(message, !ready);
            }
#endif
        }

        private static void LogBoundedAllEdgesAudit(
            BoundedAllEdgesAuditResult audit)
        {
#if UNITY_EDITOR
            string detailed =
                BuildBoundedAllEdgesDetailedTelemetry(audit);
            const string relativePath =
                "Library/GeneratedMassEdgeWearTelemetry.txt";
            audit.TelemetryRelativePath = relativePath;
            try
            {
                string projectRoot = Path.GetFullPath(
                    Path.Combine(Application.dataPath, ".."));
                string fullPath = Path.Combine(
                    projectRoot,
                    "Library",
                    "GeneratedMassEdgeWearTelemetry.txt");
                string directory = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                File.WriteAllText(
                    fullPath,
                    detailed,
                    new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
                audit.TelemetryWriteSucceeded = 1;
                audit.TelemetryWriteFailure = string.Empty;
            }
            catch (Exception exception)
            {
                audit.TelemetryWriteSucceeded = 0;
                audit.TelemetryWriteFailure =
                    exception.GetType().Name + ":" + exception.Message;
            }

            string message =
                "GeneratedMass unified bounded edge-wear audit. " +
                "stage=" + audit.Stage +
                ",failureStage:" + audit.FailureStage +
                ",valid:" + audit.GeometryValid +
                ",trace:" +
                    (string.IsNullOrEmpty(audit.Diagnostic)
                        ? "none"
                        : audit.Diagnostic) +
                ", allBounded=" +
                    "candidates:" + audit.CandidateCount +
                    ",convex:" + audit.ConvexCandidateCount +
                    ",railSolved:" + audit.RailSolvedEdgeCount +
                    ",railRejected:" + audit.RailRejectedEdgeCount +
                    ",hullSuppressed:" + audit.HullSuppressedEdgeCount +
                    ",active:" + audit.ActiveEdgeCount +
                ", pointCloud=" +
                    "points:" + audit.PointCount +
                    ",rank:" + audit.PointCloudRank +
                    ",min:" +
                        FormatBoundedAllEdgeVector(
                            audit.PointCloudBoundsMinimum) +
                    ",max:" +
                        FormatBoundedAllEdgeVector(
                            audit.PointCloudBoundsMaximum) +
                ", planeExtraction=" +
                    "triples:" + audit.HullTriplesTested +
                    ",degenerate:" + audit.HullDegenerateTriples +
                    ",nearDegenerate:" +
                        audit.HullNearDegenerateTriples +
                    ",normalizationRejected:" +
                        audit.HullNormalizationRejectedTriples +
                    ",postNormalizationInvalid:" +
                        audit.HullPostNormalizationInvalidTriples +
                    ",minimumCross:" +
                        audit.HullPlaneMinimumCrossMagnitude
                            .ToString("G9") +
                    ",rejectedCrossRange:" +
                        audit.HullMinimumRejectedCrossMagnitude
                            .ToString("G9") + "-" +
                        audit.HullMaximumRejectedCrossMagnitude
                            .ToString("G9") +
                    ",minimumAcceptedCross:" +
                        audit.HullMinimumAcceptedCrossMagnitude
                            .ToString("G9") +
                    ",supporting:" + audit.HullSupportingTriples +
                    ",straddling:" + audit.HullStraddlingTriples +
                    ",created:" + audit.HullPlanesCreated +
                    ",merged:" + audit.HullPlanesMerged +
                    ",beforePrune:" + audit.HullPlanesBeforePrune +
                    ",pruned:" +
                        audit.HullPlanesRemovedUnderThreePoints +
                    ",invalidRemoved:" +
                        audit.HullInvalidPlanesRemoved +
                    ",firstInvalid:" +
                        audit.HullFirstInvalidPlaneIndex +
                    ",firstInvalidSeed:" +
                        audit.HullFirstInvalidSeedA + "/" +
                        audit.HullFirstInvalidSeedB + "/" +
                        audit.HullFirstInvalidSeedC +
                    ",firstInvalidCross:" +
                        audit.HullFirstInvalidSeedCrossMagnitude
                            .ToString("G9") +
                    ",firstInvalidReason:" +
                        (string.IsNullOrEmpty(
                            audit.HullFirstInvalidPlaneReason)
                            ? "none"
                            : audit.HullFirstInvalidPlaneReason) +
                    ",final:" + audit.HullPlaneCount +
                ", facetBuild=" +
                    "attempted:" + audit.HullPlanesAttempted +
                    ",completed:" + audit.HullFacesCompleted +
                    ",failurePlane:" + audit.HullFailurePlaneIndex +
                    ",normal:" +
                        FormatBoundedAllEdgeVector(
                            audit.HullFailurePlaneNormal) +
                    ",distance:" +
                        audit.HullFailurePlaneDistance.ToString("G9") +
                    ",planePoints:" +
                        audit.HullFailurePlanePointCount +
                    ",ordered:" +
                        audit.HullFailureOrderedVertexCount +
                    ",sanitized:" +
                        audit.HullFailureSanitizedVertexCount +
                    ",area:" +
                        audit.HullFailureFacetArea.ToString("G9") +
                    ",convex:" +
                        audit.HullFailureConvexityValid +
                    ",reason:" +
                        (string.IsNullOrEmpty(audit.HullFailureReason)
                            ? "none"
                            : audit.HullFailureReason) +
                ", boundedHull=" +
                    "iterations:" + audit.HullIterationCount +
                    ",faces:" + audit.OutputFaceCount +
                    ",sourceFaces:" + audit.SourceFaceCount +
                    ",bevelFaces:" + audit.BevelFaceCount +
                    ",junctionFaces:" +
                        audit.VertexJunctionFaceCount +
                    ",missingBevelFaces:" +
                        audit.MissingBevelFaceCount +
                    ",duplicateBevelFaces:" +
                        audit.DuplicateBevelFaceCount +
                ", boundedPrepare=" +
                    FormatBoundedPreparationAudit(audit.Preparation) +
                ", boundedTopology=" +
                    "open:" + audit.OpenEdgeCount +
                    ",nonManifold:" + audit.NonManifoldEdgeCount +
                    ",tJunction:" + audit.TJunctionCount +
                    ",invalidFaces:" + audit.InvalidFaceCount +
                ", boundedVolume=" +
                    "source:" + audit.SourceVolume.ToString("G12") +
                    ",result:" + audit.ResultVolume.ToString("G12") +
                    ",ratio:" + audit.VolumeRatio.ToString("G12") +
                    ",delta:" + audit.VolumeDelta.ToString("G12") +
                    ",valid:" + audit.VolumeValid +
                ", boundedPolygonSurface={" +
                    FormatPolygonSurfaceAudit(
                        audit.CertificationAudit) + "}" +
                ", boundedBevelRegion=" +
                    "polygonFaces:" +
                        audit.CertificationAudit.BevelRegionFaceCount +
                    FormatOneSurfaceTriangulationPolicy() +
                    ",boundaryVertices:" +
                        audit.CertificationAudit
                            .BevelRegionBoundaryVertexCount +
                    ",triangles:" +
                        audit.CertificationAudit.BevelRegionTriangleCount +
                    ",authoredNormalTriangles:" +
                        audit.CertificationAudit
                            .BevelRegionAuthoredNormalTriangleCount +
                    ",authoredSurfaceGroupTriangles:" +
                        audit.CertificationAudit
                            .BevelRegionAuthoredSurfaceGroupTriangleCount +
                    ",internalFanVertices:" +
                        audit.CertificationAudit
                            .BevelRegionInternalFanVertexCount +
                    ",maxPlaneResidual:" +
                        audit.CertificationAudit
                            .BevelRegionMaximumPlaneResidual
                            .ToString("G9") +
                    ",maxGeometricNormalDeviationDegrees:" +
                        audit.CertificationAudit
                            .BevelRegionMaximumNormalDeviationDegrees
                            .ToString("G9") +
                    ",renderValid:" +
                        audit.CertificationAudit.BevelRegionRenderValid +
                    ",failureFace:" +
                        audit.CertificationAudit.BevelRegionFailureFace +
                    ",failureProvenance:" +
                        audit.CertificationAudit
                            .BevelRegionFailureProvenanceIndex +
                    ",failureReason:" +
                        (string.IsNullOrEmpty(
                            audit.CertificationAudit
                                .BevelRegionFailureReason)
                            ? "none"
                            : audit.CertificationAudit
                                .BevelRegionFailureReason) +
                ", boundedMesh=" +
                    "triangulationAttempted:" +
                        audit.TriangulationAttempted +
                    ",triangulatedFaces:" +
                        audit.TriangulatedFaceCount +
                    ",triangles:" + audit.TriangleCount +
                    ",triangleSoupValid:" + audit.TriangleSoupValid +
                ", diagnostics=" +
                    "corner:" + audit.CornerDiagnosticValid +
                    ",plane:" + audit.PlaneDiagnosticValid +
                    ",planeActive:" +
                        audit.PlaneDiagnosticActiveEdges +
                    ",planeBuilt:" +
                        audit.PlaneDiagnosticBuiltEdges +
                    ",planeDeferred:" +
                        audit.PlaneDiagnosticDeferredEdges +
                    ",planeRejected:" +
                        audit.PlaneDiagnosticRejectedEdges +
                ", telemetry=" +
                    "path:" + audit.TelemetryRelativePath +
                    ",write:" + audit.TelemetryWriteSucceeded +
                    (string.IsNullOrEmpty(audit.TelemetryWriteFailure)
                        ? string.Empty
                        : ",error:" + audit.TelemetryWriteFailure) +
                ", geometryCommit=disabled";
            LogChamferNoStackTrace(
                message,
                audit.GeometryValid != 1);
#endif
        }

#if UNITY_EDITOR
        private static string BuildBoundedAllEdgesDetailedTelemetry(
            BoundedAllEdgesAuditResult audit)
        {
            string detailed =
                "GeneratedMass unified bounded edge-wear detailed telemetry." +
                Environment.NewLine +
                "timestampUtc:" + DateTime.UtcNow.ToString("O") +
                Environment.NewLine +
                "stage:" + audit.Stage +
                ",failureStage:" + audit.FailureStage +
                ",valid:" + audit.GeometryValid +
                ",trace:" +
                    (string.IsNullOrEmpty(audit.Diagnostic)
                        ? "none"
                        : audit.Diagnostic) +
                Environment.NewLine +
                "allBounded=" +
                    "candidates:" + audit.CandidateCount +
                    ",convex:" + audit.ConvexCandidateCount +
                    ",railSolved:" + audit.RailSolvedEdgeCount +
                    ",railRejected:" + audit.RailRejectedEdgeCount +
                    ",hullSuppressed:" + audit.HullSuppressedEdgeCount +
                    ",active:" + audit.ActiveEdgeCount +
                    ",valid:" + audit.GeometryValid +
                Environment.NewLine + "pointCloud=" +
                    "points:" + audit.PointCount +
                    ",rank:" + audit.PointCloudRank +
                    ",min:" +
                        FormatBoundedAllEdgeVector(
                            audit.PointCloudBoundsMinimum) +
                    ",max:" +
                        FormatBoundedAllEdgeVector(
                            audit.PointCloudBoundsMaximum) +
                Environment.NewLine + "planeExtraction=" +
                    "triples:" + audit.HullTriplesTested +
                    ",degenerate:" + audit.HullDegenerateTriples +
                    ",nearDegenerate:" +
                        audit.HullNearDegenerateTriples +
                    ",normalizationRejected:" +
                        audit.HullNormalizationRejectedTriples +
                    ",postNormalizationInvalid:" +
                        audit.HullPostNormalizationInvalidTriples +
                    ",minimumCross:" +
                        audit.HullPlaneMinimumCrossMagnitude
                            .ToString("G9") +
                    ",rejectedCrossRange:" +
                        audit.HullMinimumRejectedCrossMagnitude
                            .ToString("G9") + "-" +
                        audit.HullMaximumRejectedCrossMagnitude
                            .ToString("G9") +
                    ",minimumAcceptedCross:" +
                        audit.HullMinimumAcceptedCrossMagnitude
                            .ToString("G9") +
                    ",supporting:" + audit.HullSupportingTriples +
                    ",straddling:" + audit.HullStraddlingTriples +
                    ",created:" + audit.HullPlanesCreated +
                    ",merged:" + audit.HullPlanesMerged +
                    ",beforePrune:" + audit.HullPlanesBeforePrune +
                    ",pruned:" +
                        audit.HullPlanesRemovedUnderThreePoints +
                    ",invalidRemoved:" +
                        audit.HullInvalidPlanesRemoved +
                    ",firstInvalid:" +
                        audit.HullFirstInvalidPlaneIndex +
                    ",firstInvalidSeed:" +
                        audit.HullFirstInvalidSeedA + "/" +
                        audit.HullFirstInvalidSeedB + "/" +
                        audit.HullFirstInvalidSeedC +
                    ",firstInvalidCross:" +
                        audit.HullFirstInvalidSeedCrossMagnitude
                            .ToString("G9") +
                    ",firstInvalidReason:" +
                        (string.IsNullOrEmpty(
                            audit.HullFirstInvalidPlaneReason)
                            ? "none"
                            : audit.HullFirstInvalidPlaneReason) +
                    ",final:" + audit.HullPlaneCount +
                Environment.NewLine + "facetBuild=" +
                    "attempted:" + audit.HullPlanesAttempted +
                    ",completed:" + audit.HullFacesCompleted +
                    ",failurePlane:" + audit.HullFailurePlaneIndex +
                    ",normal:" +
                        FormatBoundedAllEdgeVector(
                            audit.HullFailurePlaneNormal) +
                    ",distance:" +
                        audit.HullFailurePlaneDistance.ToString("G9") +
                    ",planePoints:" +
                        audit.HullFailurePlanePointCount +
                    ",ordered:" +
                        audit.HullFailureOrderedVertexCount +
                    ",sanitized:" +
                        audit.HullFailureSanitizedVertexCount +
                    ",area:" +
                        audit.HullFailureFacetArea.ToString("G9") +
                    ",convex:" +
                        audit.HullFailureConvexityValid +
                    ",reason:" +
                        (string.IsNullOrEmpty(audit.HullFailureReason)
                            ? "none"
                            : audit.HullFailureReason) +
                Environment.NewLine + "cornerDiagnostic=" +
                    "attempted:" + audit.CornerDiagnosticAttempted +
                    ",valid:" + audit.CornerDiagnosticValid +
                    (string.IsNullOrEmpty(audit.CornerDiagnostic)
                        ? string.Empty
                        : ",trace:" + audit.CornerDiagnostic) +
                Environment.NewLine + "planeDiagnostic=" +
                    "attempted:" + audit.PlaneDiagnosticAttempted +
                    ",valid:" + audit.PlaneDiagnosticValid +
                    ",active:" + audit.PlaneDiagnosticActiveEdges +
                    ",built:" + audit.PlaneDiagnosticBuiltEdges +
                    ",deferred:" + audit.PlaneDiagnosticDeferredEdges +
                    ",rejected:" + audit.PlaneDiagnosticRejectedEdges +
                    ",detail:{" + audit.PlaneDiagnosticEvidence + "}" +
                Environment.NewLine + "boundedHull=" +
                    "iterations:" + audit.HullIterationCount +
                    ",points:" + audit.PointCount +
                    ",planes:" + audit.HullPlaneCount +
                    ",faces:" + audit.OutputFaceCount +
                    ",sourceFaces:" + audit.SourceFaceCount +
                    ",bevelFaces:" + audit.BevelFaceCount +
                    ",vertexJunctionFaces:" +
                        audit.VertexJunctionFaceCount +
                    ",missingBevelFaces:" +
                        audit.MissingBevelFaceCount +
                    ",duplicateBevelFaces:" +
                        audit.DuplicateBevelFaceCount +
                Environment.NewLine + "boundedPrepare=" +
                    FormatBoundedPreparationAudit(audit.Preparation) +
                Environment.NewLine + "boundedTopology=" +
                    "open:" + audit.OpenEdgeCount +
                    ",nonManifold:" + audit.NonManifoldEdgeCount +
                    ",tJunction:" + audit.TJunctionCount +
                    ",invalidFaces:" + audit.InvalidFaceCount +
                Environment.NewLine + "boundedBounds=" +
                    "valid:" + audit.BoundsValid +
                    ",tolerance:" + audit.BoundsTolerance.ToString("G9") +
                    ",sourceMin:" +
                        FormatBoundedAllEdgeVector(
                            audit.SourceBoundsMinimum) +
                    ",sourceMax:" +
                        FormatBoundedAllEdgeVector(
                            audit.SourceBoundsMaximum) +
                    ",resultMin:" +
                        FormatBoundedAllEdgeVector(
                            audit.ResultBoundsMinimum) +
                    ",resultMax:" +
                        FormatBoundedAllEdgeVector(
                            audit.ResultBoundsMaximum) +
                    ",minMargin:" +
                        FormatBoundedAllEdgeVector(
                            audit.BoundsMinimumMargin) +
                    ",maxMargin:" +
                        FormatBoundedAllEdgeVector(
                            audit.BoundsMaximumMargin) +
                Environment.NewLine + "boundedContainment=" +
                    "sourceAttempted:" +
                        audit.CertificationAudit.SourceConvexityAttempted +
                    ",sourceViolations:" +
                        audit.CertificationAudit
                            .SourceConvexityViolationCount +
                    ",sourceMaximumViolation:" +
                        audit.CertificationAudit
                            .SourceMaximumPlaneViolation.ToString("G9") +
                    ",sourcePlaneFace:" +
                        audit.CertificationAudit.SourceViolatingPlaneFace +
                    ",sourceVertexFace:" +
                        audit.CertificationAudit.SourceViolatingVertexFace +
                    ",sourceVertexIndex:" +
                        audit.CertificationAudit.SourceViolatingVertexIndex +
                    ",resultAttempted:" +
                        audit.CertificationAudit
                            .ResultContainmentAttempted +
                    ",resultViolations:" +
                        audit.SourceContainmentViolations +
                    ",resultMaximumOutwardDistance:" +
                        audit.MaximumSourceContainmentViolation
                            .ToString("G9") +
                    ",resultFace:" +
                        audit.CertificationAudit.ResultViolatingFace +
                    ",resultProvenance:" +
                        audit.CertificationAudit
                            .ResultViolatingProvenanceKind + ":" +
                        audit.CertificationAudit
                            .ResultViolatingProvenanceIndex +
                    ",resultVertex:" +
                        audit.CertificationAudit
                            .ResultViolatingVertexIndex +
                    ",sourcePlane:" +
                        audit.CertificationAudit
                            .ResultViolatedSourcePlane +
                Environment.NewLine + "boundedConvexity=" +
                    "attempted:" +
                        audit.CertificationAudit
                            .ResultConvexityAttempted +
                    ",violations:" + audit.ResultConvexityViolations +
                    ",maximumViolation:" +
                        audit.MaximumResultConvexityViolation
                            .ToString("G9") +
                    ",planeFace:" +
                        audit.CertificationAudit
                            .ResultConvexityPlaneFace +
                    ",planeProvenance:" +
                        audit.CertificationAudit
                            .ResultConvexityPlaneProvenanceKind + ":" +
                        audit.CertificationAudit
                            .ResultConvexityPlaneProvenanceIndex +
                    ",vertexFace:" +
                        audit.CertificationAudit
                            .ResultConvexityVertexFace +
                    ",vertexProvenance:" +
                        audit.CertificationAudit
                            .ResultConvexityVertexProvenanceKind + ":" +
                        audit.CertificationAudit
                            .ResultConvexityVertexProvenanceIndex +
                    ",vertexIndex:" +
                        audit.CertificationAudit
                            .ResultConvexityVertexIndex +
                Environment.NewLine + "boundedIntersections=" +
                    "sourceAttempted:" +
                        audit.CertificationAudit
                            .SourceFaceIntersectionAttempted +
                    ",sourcePairs:" +
                        audit.CertificationAudit
                            .SourceFaceIntersectionPairCount +
                    ",sourceBoundary:" +
                        audit.CertificationAudit
                            .SourceBoundaryContactPairCount +
                    ",sourceInterior:" +
                        audit.CertificationAudit
                            .SourceImproperInteriorPairCount +
                    ",resultAttempted:" +
                        audit.CertificationAudit.FaceIntersectionAttempted +
                    ",resultPairs:" +
                        audit.CertificationAudit.FaceIntersectionPairCount +
                    ",resultBoundary:" +
                        audit.CertificationAudit
                            .ResultBoundaryContactPairCount +
                    ",resultInterior:" +
                        audit.CertificationAudit
                            .ResultImproperInteriorPairCount +
                    ",unchanged:" +
                        audit.CertificationAudit
                            .UnchangedIntersectionPairCount +
                    ",changed:" +
                        audit.CertificationAudit
                            .ChangedIntersectionPairCount +
                    ",new:" +
                        audit.CertificationAudit.NewIntersectionPairCount +
                    ",introducedInterior:" +
                        audit.IntroducedInteriorIntersections +
                    ",resolved:" +
                        audit.CertificationAudit
                            .ResolvedIntersectionPairCount +
                    ",sourceEvidence:{" +
                        FormatBoundedAuditEvidence(
                            audit.CertificationAudit
                                .SourceIntersectionPairEvidence) + "}" +
                    ",resultEvidence:{" +
                        FormatBoundedAuditEvidence(
                            audit.CertificationAudit
                                .ResultIntersectionPairEvidence) + "}" +
                    ",unchangedEvidence:{" +
                        FormatBoundedAuditEvidence(
                            audit.CertificationAudit
                                .UnchangedIntersectionPairEvidence) + "}" +
                    ",changedEvidence:{" +
                        FormatBoundedAuditEvidence(
                            audit.CertificationAudit
                                .ChangedIntersectionPairEvidence) + "}" +
                    ",newEvidence:{" +
                        FormatBoundedAuditEvidence(
                            audit.CertificationAudit
                                .NewIntersectionPairEvidence) + "}" +
                    ",resolvedEvidence:{" +
                        FormatBoundedAuditEvidence(
                            audit.CertificationAudit
                                .ResolvedIntersectionPairEvidence) + "}" +
                Environment.NewLine + "boundedVolume=" +
                    "source:" + audit.SourceVolume.ToString("G12") +
                    ",result:" + audit.ResultVolume.ToString("G12") +
                    ",ratio:" + audit.VolumeRatio.ToString("G12") +
                    ",delta:" + audit.VolumeDelta.ToString("G12") +
                    ",lowerMargin:" +
                        audit.VolumeLowerMargin.ToString("G12") +
                    ",upperMargin:" +
                        audit.VolumeUpperMargin.ToString("G12") +
                    ",valid:" + audit.VolumeValid +
                Environment.NewLine + "boundedPolygonSurface=" +
                    FormatPolygonSurfaceAudit(
                        audit.CertificationAudit) +
                Environment.NewLine + "boundedBevelRegion=" +
                    "polygonFaces:" +
                        audit.CertificationAudit.BevelRegionFaceCount +
                    FormatOneSurfaceTriangulationPolicy() +
                    ",boundaryVertices:" +
                        audit.CertificationAudit
                            .BevelRegionBoundaryVertexCount +
                    ",triangles:" +
                        audit.CertificationAudit.BevelRegionTriangleCount +
                    ",authoredNormalTriangles:" +
                        audit.CertificationAudit
                            .BevelRegionAuthoredNormalTriangleCount +
                    ",authoredSurfaceGroupTriangles:" +
                        audit.CertificationAudit
                            .BevelRegionAuthoredSurfaceGroupTriangleCount +
                    ",internalFanVertices:" +
                        audit.CertificationAudit
                            .BevelRegionInternalFanVertexCount +
                    ",maxPlaneResidual:" +
                        audit.CertificationAudit
                            .BevelRegionMaximumPlaneResidual
                            .ToString("G9") +
                    ",maxGeometricNormalDeviationDegrees:" +
                        audit.CertificationAudit
                            .BevelRegionMaximumNormalDeviationDegrees
                            .ToString("G9") +
                    ",renderValid:" +
                        audit.CertificationAudit.BevelRegionRenderValid +
                    ",failureFace:" +
                        audit.CertificationAudit.BevelRegionFailureFace +
                    ",failureProvenance:" +
                        audit.CertificationAudit
                            .BevelRegionFailureProvenanceIndex +
                    ",failureReason:" +
                        (string.IsNullOrEmpty(
                            audit.CertificationAudit
                                .BevelRegionFailureReason)
                            ? "none"
                            : audit.CertificationAudit
                                .BevelRegionFailureReason) +
                Environment.NewLine + "boundedMesh=" +
                    "triangulationAttempted:" +
                        audit.TriangulationAttempted +
                    ",triangulatedFaces:" +
                        audit.TriangulatedFaceCount +
                    ",triangulationFailureFace:" +
                        audit.CertificationAudit
                            .TriangulationFailureFace +
                    ",triangulationFailureKind:" +
                        audit.CertificationAudit
                            .TriangulationFailureKind +
                    ",triangulationFailureProvenance:" +
                        audit.CertificationAudit
                            .TriangulationFailureProvenanceKind + ":" +
                        audit.CertificationAudit
                            .TriangulationFailureProvenanceIndex +
                    ",triangulationFailureReason:" +
                        (string.IsNullOrEmpty(
                            audit.CertificationAudit
                                .TriangulationFailureReason)
                            ? "none"
                            : audit.CertificationAudit
                                .TriangulationFailureReason) +
                    ",triangles:" + audit.TriangleCount +
                    ",degenerate:" +
                        audit.TriangleAudit
                            .PreviewDegenerateTriangleCount +
                    ",open:" +
                        audit.TriangleAudit.PreviewOpenEdgeCount +
                    ",nonManifold:" +
                        audit.TriangleAudit
                            .PreviewNonManifoldEdgeCount +
                    ",winding:" +
                        audit.TriangleAudit
                            .PreviewWindingFailureCount +
                    ",bounds:" +
                        audit.TriangleAudit
                            .PreviewBoundsFailureCount +
                    ",volume:" +
                        audit.TriangleAudit
                            .PreviewVolumeFailureCount +
                    ",triangleSoupValid:" + audit.TriangleSoupValid +
                Environment.NewLine + "hullPoints=" + audit.HullPointEvidence +
                Environment.NewLine + "hullPlanes=" + audit.HullPlaneEvidence +
                Environment.NewLine + "hullFaces=" + audit.HullFaceEvidence +
                Environment.NewLine + "edgeResults=" + audit.EdgeEvidence +
                (string.IsNullOrEmpty(audit.Diagnostic)
                    ? string.Empty
                    : Environment.NewLine + "boundedTrace=" + audit.Diagnostic) +
                Environment.NewLine + "geometryCommit=disabled";

            return detailed;
        }
#endif

        private static void LogChamferCornerAudit(
            ChamferCornerStats stats,
            bool ready,
            string blocker)
        {
#if UNITY_EDITOR
            if (ready && !EnableVerboseChamferDiagnostics)
            {
                return;
            }
            string message =
                "GeneratedMass edge wear corner audit. " +
                "selected=" + stats.ActiveSelectedEdgeCount + "/" +
                    stats.SelectedEdgeCount +
                ", replacementFailures=" +
                    stats.ReplacementFaceAreaFailureCount + "/" +
                    stats.ReplacementFaceWindingFailureCount + "/" +
                    stats.ReplacementEdgeCollapseFailureCount +
                ", solveFailures=" + stats.WidthSolveFailures + "/" +
                    stats.CornerSolveFailures +
                ", conflictSearch=" +
                    stats.ConflictSearchStatesEvaluated + "/" +
                    stats.ConflictSearchCommittedExclusionCount +
                    "/" + stats.ConflictSearchTimeBudgetExceeded +
                    "/" + stats.ConflictSearchCancelled +
                    "/" + stats.ConflictSearchElapsedMilliseconds
                        .ToString("G9", CultureInfo.InvariantCulture) +
                ", ready=" + (ready ? 1 : 0);
            if (!string.IsNullOrEmpty(blocker))
            {
                message += ", blocker=" + blocker;
            }
            LogChamferNoStackTrace(message, !ready);
#endif
        }

        private static void LogChamferReadiness(
            ChamferReadinessStats stats,
            bool ready,
            string blocker)
        {
#if UNITY_EDITOR
            if (ready && !EnableVerboseChamferDiagnostics)
            {
                return;
            }
            string message =
                "GeneratedMass edge wear readiness audit. " +
                "selected=" + stats.SelectedGraphEdgeCount +
                ", affectedVertices=" + stats.AffectedVertexCount +
                ", sourceNonManifold=" +
                    stats.SourceNonManifoldEdgeCount +
                ", sourceTJunctions=" + stats.SourceTJunctionCount +
                ", ready=" + (ready ? 1 : 0);
            if (!string.IsNullOrEmpty(blocker))
            {
                message += ", blocker=" + blocker;
            }
            LogChamferNoStackTrace(message, !ready);
#endif
        }

        #endregion
    }
}
