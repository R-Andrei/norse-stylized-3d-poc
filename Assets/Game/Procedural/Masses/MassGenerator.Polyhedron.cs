using System;
using System.Collections.Generic;
using UnityEngine;
using ProgrammaticStylized3D.Geometry;

namespace ProgrammaticStylized3D.Geometry.Masses
{
    public static partial class MassGenerator
    {
        #region Plane-cut polyhedron utilities

        private static Bounds CalculateFaceBounds(List<PolygonFace> faces)
        {
            if (faces == null || faces.Count == 0)
            {
                return new Bounds(Vector3.zero, Vector3.one);
            }

            bool initialized = false;
            Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
            for (int faceIndex = 0; faceIndex < faces.Count; faceIndex++)
            {
                List<Vector3> vertices = faces[faceIndex].Vertices;
                for (int vertexIndex = 0; vertexIndex < vertices.Count; vertexIndex++)
                {
                    if (!initialized)
                    {
                        bounds = new Bounds(vertices[vertexIndex], Vector3.zero);
                        initialized = true;
                    }
                    else
                    {
                        bounds.Encapsulate(vertices[vertexIndex]);
                    }
                }
            }

            return initialized
                ? bounds
                : new Bounds(Vector3.zero, Vector3.one);
        }

        private static float GetCurrentSupport(
            List<PolygonFace> faces,
            Vector3 normal)
        {
            float support = float.NegativeInfinity;

            for (int faceIndex = 0; faceIndex < faces.Count; faceIndex++)
            {
                List<Vector3> vertices = faces[faceIndex].Vertices;

                for (int vertexIndex = 0; vertexIndex < vertices.Count; vertexIndex++)
                {
                    support = Mathf.Max(
                        support,
                        Vector3.Dot(normal, vertices[vertexIndex]));
                }
            }

            return support;
        }

        private static void ClipPolyhedron(
            List<PolygonFace> faces,
            CutPlane plane,
            PolygonFaceFeature capFeature = PolygonFaceFeature.Base,
            float capFeatureStrength = 0f,
            bool clampIntersectionsToSegment = false,
            float insideEpsilon = PlaneEpsilon,
            bool canonicalizeSharedIntersections = false,
            PolygonFaceProvenanceKind capProvenanceKind =
                PolygonFaceProvenanceKind.None,
            int capProvenanceIndex = -1,
            bool enforceExactPlaneIntersections = false,
            bool useDistanceWelding = false,
            PlaneCutNumericalRepairTelemetry numericalRepairs = null)
        {
            float effectiveInsideEpsilon = Mathf.Clamp(
                insideEpsilon,
                PointMergeDistance * 0.01f,
                PlaneEpsilon);
            Dictionary<EdgeKey, Vector3> intersectionCache =
                canonicalizeSharedIntersections
                    ? new Dictionary<EdgeKey, Vector3>()
                    : null;
            List<PolygonFace> clippedFaces = new List<PolygonFace>();
            List<Vector3> capPoints = new List<Vector3>();

            for (int i = 0; i < faces.Count; i++)
            {
                PolygonFace sourceFace = faces[i];
                float ownerPlaneDistance =
                    CalculateAuthoredFacePlaneDistance(sourceFace);
                List<Vector3> clipped = ClipPolygon(
                    sourceFace.Vertices,
                    sourceFace.Normal,
                    ownerPlaneDistance,
                    sourceFace.ProvenanceKind,
                    sourceFace.ProvenanceIndex,
                    plane,
                    capProvenanceKind,
                    capProvenanceIndex,
                    capPoints,
                    clampIntersectionsToSegment,
                    effectiveInsideEpsilon,
                    intersectionCache,
                    enforceExactPlaneIntersections,
                    numericalRepairs,
                    out bool clipSucceeded);

                if (!clipSucceeded)
                {
                    return;
                }

                clipped = SanitizePolygon(
                    clipped,
                    sourceFace.Normal);

                if (clipped.Count >= 3 &&
                    CalculatePolygonArea(clipped) > TinyFaceAreaEpsilon)
                {
                    clippedFaces.Add(
                        new PolygonFace(
                            clipped,
                            sourceFace.Normal,
                            sourceFace.Feature,
                            sourceFace.FeatureStrength,
                            sourceFace.ProvenanceKind,
                            sourceFace.ProvenanceIndex));
                }
            }

            if (enforceExactPlaneIntersections)
            {
                float strictResidualTolerance =
                    PointMergeDistance * 0.25f;
                for (int pointIndex = 0;
                     pointIndex < capPoints.Count;
                     pointIndex++)
                {
                    float residual = Mathf.Abs(
                        plane.SignedDistance(capPoints[pointIndex]));
                    if (numericalRepairs != null)
                    {
                        numericalRepairs.CapVertexValidationCount++;
                        numericalRepairs.MaximumCapResidualBeforeProjection =
                            Mathf.Max(
                                numericalRepairs
                                    .MaximumCapResidualBeforeProjection,
                                residual);
                        numericalRepairs.MaximumCapResidualAfterProjection =
                            Mathf.Max(
                                numericalRepairs
                                    .MaximumCapResidualAfterProjection,
                                residual);
                    }

                    if (residual > strictResidualTolerance)
                    {
                        if (numericalRepairs != null)
                        {
                            numericalRepairs.CapResidualRejectCount++;
                            RecordExactClipFailure(
                                numericalRepairs,
                                PolygonFaceProvenanceKind.None,
                                -1,
                                capProvenanceKind,
                                capProvenanceIndex,
                                0f,
                                0f,
                                "cap",
                                "cap",
                                0f,
                                residual,
                                0f,
                                residual,
                                "cap-point-residual");
                        }
                        return;
                    }
                }
            }

            List<Vector3> uniqueCapPoints =
                GetUniquePoints(capPoints);

            if (uniqueCapPoints.Count >= 3)
            {
                PolygonFace capFace = CreateOrientedFace(
                    plane.Normal,
                    capFeature,
                    capFeatureStrength,
                    uniqueCapPoints.ToArray());

                List<Vector3> sanitizedCap = SanitizePolygon(
                    capFace.Vertices,
                    capFace.Normal);

                bool capResidualValid = true;
                if (enforceExactPlaneIntersections)
                {
                    float maximumResidual = 0f;
                    for (int vertexIndex = 0;
                         vertexIndex < sanitizedCap.Count;
                         vertexIndex++)
                    {
                        maximumResidual = Mathf.Max(
                            maximumResidual,
                            Mathf.Abs(plane.SignedDistance(
                                sanitizedCap[vertexIndex])));
                    }
                    float strictResidualTolerance =
                        PointMergeDistance * 0.25f;
                    capResidualValid =
                        maximumResidual <= strictResidualTolerance;
                    if (!capResidualValid && numericalRepairs != null)
                    {
                        numericalRepairs.CapResidualRejectCount++;
                        RecordExactClipFailure(
                            numericalRepairs,
                            PolygonFaceProvenanceKind.None,
                            -1,
                            capProvenanceKind,
                            capProvenanceIndex,
                            0f,
                            0f,
                            "cap",
                            "cap",
                            0f,
                            maximumResidual,
                            0f,
                            maximumResidual,
                            "sanitized-cap-residual");
                    }
                }

                if (capResidualValid &&
                    sanitizedCap.Count >= 3 &&
                    CalculatePolygonArea(sanitizedCap) > TinyFaceAreaEpsilon)
                {
                    clippedFaces.Add(
                        new PolygonFace(
                            sanitizedCap,
                            capFace.Normal,
                            capFace.Feature,
                            capFace.FeatureStrength,
                            capProvenanceKind,
                            capProvenanceIndex));
                }
            }

            if (enforceExactPlaneIntersections &&
                numericalRepairs != null &&
                numericalRepairs.ExactConstructionFailureCount > 0)
            {
                return;
            }

            if (useDistanceWelding)
            {
                WeldSharedVerticesByDistance(
                    clippedFaces,
                    PointMergeDistance,
                    numericalRepairs);
            }
            else
            {
                WeldSharedVertices(clippedFaces);
            }
            SanitizeAllFaces(clippedFaces);

            if (clippedFaces.Count >= 4)
            {
                faces.Clear();
                faces.AddRange(clippedFaces);
            }
        }

        private enum PlaneClipPointClassification
        {
            Inside,
            OnPlane,
            Outside
        }

        private static float CalculateAuthoredFacePlaneDistance(
            PolygonFace face)
        {
            if (face == null || face.Vertices.Count == 0)
            {
                return 0f;
            }

            float distance = 0f;
            for (int vertexIndex = 0;
                 vertexIndex < face.Vertices.Count;
                 vertexIndex++)
            {
                distance += Vector3.Dot(
                    face.Normal,
                    face.Vertices[vertexIndex]);
            }
            return distance / face.Vertices.Count;
        }

        private static List<Vector3> ClipPolygon(
            List<Vector3> vertices,
            CutPlane plane,
            List<Vector3> capPoints,
            bool clampIntersectionsToSegment,
            float insideEpsilon,
            Dictionary<EdgeKey, Vector3> intersectionCache)
        {
            return ClipPolygonLegacy(
                vertices,
                plane,
                capPoints,
                clampIntersectionsToSegment,
                insideEpsilon,
                intersectionCache);
        }

        private static List<Vector3> ClipPolygon(
            List<Vector3> vertices,
            Vector3 ownerPlaneNormal,
            float ownerPlaneDistance,
            PolygonFaceProvenanceKind ownerProvenanceKind,
            int ownerProvenanceIndex,
            CutPlane plane,
            PolygonFaceProvenanceKind cutProvenanceKind,
            int cutProvenanceIndex,
            List<Vector3> capPoints,
            bool clampIntersectionsToSegment,
            float insideEpsilon,
            Dictionary<EdgeKey, Vector3> intersectionCache,
            bool enforceExactPlaneIntersections,
            PlaneCutNumericalRepairTelemetry numericalRepairs,
            out bool succeeded)
        {
            if (!enforceExactPlaneIntersections)
            {
                succeeded = true;
                return ClipPolygonLegacy(
                    vertices,
                    plane,
                    capPoints,
                    clampIntersectionsToSegment,
                    insideEpsilon,
                    intersectionCache);
            }

            return ClipPolygonExact(
                vertices,
                ownerPlaneNormal,
                ownerPlaneDistance,
                ownerProvenanceKind,
                ownerProvenanceIndex,
                plane,
                cutProvenanceKind,
                cutProvenanceIndex,
                capPoints,
                clampIntersectionsToSegment,
                intersectionCache,
                numericalRepairs,
                out succeeded);
        }

        private static List<Vector3> ClipPolygonLegacy(
            List<Vector3> vertices,
            CutPlane plane,
            List<Vector3> capPoints,
            bool clampIntersectionsToSegment,
            float insideEpsilon,
            Dictionary<EdgeKey, Vector3> intersectionCache)
        {
            List<Vector3> result = new List<Vector3>();
            Vector3 previous = vertices[vertices.Count - 1];
            float previousDistance = plane.SignedDistance(previous);
            bool previousInside = previousDistance <= insideEpsilon;

            for (int vertexIndex = 0;
                 vertexIndex < vertices.Count;
                 vertexIndex++)
            {
                Vector3 current = vertices[vertexIndex];
                float currentDistance = plane.SignedDistance(current);
                bool currentInside = currentDistance <= insideEpsilon;

                if (previousInside && currentInside)
                {
                    AddPointIfDifferent(result, current);
                }
                else if (previousInside && !currentInside)
                {
                    Vector3 intersection = ResolveLegacyClipIntersection(
                        previous,
                        current,
                        previousDistance,
                        currentDistance,
                        clampIntersectionsToSegment,
                        insideEpsilon,
                        intersectionCache);
                    AddPointIfDifferent(result, intersection);
                    capPoints.Add(intersection);
                }
                else if (!previousInside && currentInside)
                {
                    Vector3 intersection = ResolveLegacyClipIntersection(
                        previous,
                        current,
                        previousDistance,
                        currentDistance,
                        clampIntersectionsToSegment,
                        insideEpsilon,
                        intersectionCache);
                    AddPointIfDifferent(result, intersection);
                    AddPointIfDifferent(result, current);
                    capPoints.Add(intersection);
                }

                previous = current;
                previousDistance = currentDistance;
                previousInside = currentInside;
            }

            RemoveClosingDuplicate(result);
            return result;
        }

        private static List<Vector3> ClipPolygonExact(
            List<Vector3> vertices,
            Vector3 ownerPlaneNormal,
            float ownerPlaneDistance,
            PolygonFaceProvenanceKind ownerProvenanceKind,
            int ownerProvenanceIndex,
            CutPlane plane,
            PolygonFaceProvenanceKind cutProvenanceKind,
            int cutProvenanceIndex,
            List<Vector3> capPoints,
            bool clampIntersectionsToSegment,
            Dictionary<EdgeKey, Vector3> intersectionCache,
            PlaneCutNumericalRepairTelemetry numericalRepairs,
            out bool succeeded)
        {
            List<Vector3> result = new List<Vector3>();
            float strictTolerance = PointMergeDistance * 0.25f;
            Vector3 previous = vertices[vertices.Count - 1];
            float previousDistance = plane.SignedDistance(previous);
            PlaneClipPointClassification previousClassification =
                ClassifyPlaneClipPoint(
                    previousDistance,
                    strictTolerance,
                    numericalRepairs,
                    false);

            succeeded = true;
            for (int vertexIndex = 0;
                 vertexIndex < vertices.Count;
                 vertexIndex++)
            {
                Vector3 current = vertices[vertexIndex];
                float currentDistance = plane.SignedDistance(current);
                PlaneClipPointClassification currentClassification =
                    ClassifyPlaneClipPoint(
                        currentDistance,
                        strictTolerance,
                        numericalRepairs,
                        true);
                bool previousRetained =
                    previousClassification !=
                    PlaneClipPointClassification.Outside;
                bool currentRetained =
                    currentClassification !=
                    PlaneClipPointClassification.Outside;

                if (previousRetained && currentRetained)
                {
                    Vector3 retainedCurrent = current;
                    if (currentClassification ==
                        PlaneClipPointClassification.OnPlane &&
                        !TrySnapOnPlanePoint(
                            current,
                            currentDistance,
                            ownerPlaneNormal,
                            ownerPlaneDistance,
                            ownerProvenanceKind,
                            ownerProvenanceIndex,
                            plane,
                            cutProvenanceKind,
                            cutProvenanceIndex,
                            strictTolerance,
                            numericalRepairs,
                            out retainedCurrent))
                    {
                        succeeded = false;
                        return result;
                    }
                    AddPointIfDifferent(result, retainedCurrent);
                    if (currentClassification ==
                        PlaneClipPointClassification.OnPlane)
                    {
                        capPoints.Add(retainedCurrent);
                    }
                }
                else if (previousRetained && !currentRetained)
                {
                    if (previousClassification ==
                        PlaneClipPointClassification.OnPlane)
                    {
                        if (!TrySnapOnPlanePoint(
                            previous,
                            previousDistance,
                            ownerPlaneNormal,
                            ownerPlaneDistance,
                            ownerProvenanceKind,
                            ownerProvenanceIndex,
                            plane,
                            cutProvenanceKind,
                            cutProvenanceIndex,
                            strictTolerance,
                            numericalRepairs,
                            out Vector3 snappedPrevious))
                        {
                            succeeded = false;
                            return result;
                        }
                        AddPointIfDifferent(result, snappedPrevious);
                        capPoints.Add(snappedPrevious);
                    }
                    else if (!TryResolveExactClipIntersection(
                        previous,
                        current,
                        previousDistance,
                        currentDistance,
                        previousClassification,
                        currentClassification,
                        ownerPlaneNormal,
                        ownerPlaneDistance,
                        ownerProvenanceKind,
                        ownerProvenanceIndex,
                        plane,
                        cutProvenanceKind,
                        cutProvenanceIndex,
                        clampIntersectionsToSegment,
                        strictTolerance,
                        intersectionCache,
                        numericalRepairs,
                        out Vector3 intersection))
                    {
                        succeeded = false;
                        return result;
                    }
                    else
                    {
                        AddPointIfDifferent(result, intersection);
                        capPoints.Add(intersection);
                    }
                }
                else if (!previousRetained && currentRetained)
                {
                    if (currentClassification ==
                        PlaneClipPointClassification.OnPlane)
                    {
                        if (!TrySnapOnPlanePoint(
                            current,
                            currentDistance,
                            ownerPlaneNormal,
                            ownerPlaneDistance,
                            ownerProvenanceKind,
                            ownerProvenanceIndex,
                            plane,
                            cutProvenanceKind,
                            cutProvenanceIndex,
                            strictTolerance,
                            numericalRepairs,
                            out Vector3 snappedCurrent))
                        {
                            succeeded = false;
                            return result;
                        }
                        AddPointIfDifferent(result, snappedCurrent);
                        capPoints.Add(snappedCurrent);
                    }
                    else if (!TryResolveExactClipIntersection(
                        previous,
                        current,
                        previousDistance,
                        currentDistance,
                        previousClassification,
                        currentClassification,
                        ownerPlaneNormal,
                        ownerPlaneDistance,
                        ownerProvenanceKind,
                        ownerProvenanceIndex,
                        plane,
                        cutProvenanceKind,
                        cutProvenanceIndex,
                        clampIntersectionsToSegment,
                        strictTolerance,
                        intersectionCache,
                        numericalRepairs,
                        out Vector3 intersection))
                    {
                        succeeded = false;
                        return result;
                    }
                    else
                    {
                        AddPointIfDifferent(result, intersection);
                        AddPointIfDifferent(result, current);
                        capPoints.Add(intersection);
                    }
                }

                previous = current;
                previousDistance = currentDistance;
                previousClassification = currentClassification;
            }

            RemoveClosingDuplicate(result);
            return result;
        }

        private static PlaneClipPointClassification ClassifyPlaneClipPoint(
            float signedDistance,
            float tolerance,
            PlaneCutNumericalRepairTelemetry numericalRepairs,
            bool record)
        {
            PlaneClipPointClassification classification;
            if (signedDistance < -tolerance)
            {
                classification = PlaneClipPointClassification.Inside;
            }
            else if (signedDistance > tolerance)
            {
                classification = PlaneClipPointClassification.Outside;
            }
            else
            {
                classification = PlaneClipPointClassification.OnPlane;
            }

            if (record && numericalRepairs != null)
            {
                switch (classification)
                {
                    case PlaneClipPointClassification.Inside:
                        numericalRepairs.StrictInsideClassificationCount++;
                        break;
                    case PlaneClipPointClassification.OnPlane:
                        numericalRepairs.StrictOnPlaneClassificationCount++;
                        break;
                    case PlaneClipPointClassification.Outside:
                        numericalRepairs.StrictOutsideClassificationCount++;
                        break;
                }
            }

            return classification;
        }

        private static bool TrySnapOnPlanePoint(
            Vector3 point,
            float signedDistance,
            Vector3 ownerPlaneNormal,
            float ownerPlaneDistance,
            PolygonFaceProvenanceKind ownerProvenanceKind,
            int ownerProvenanceIndex,
            CutPlane plane,
            PolygonFaceProvenanceKind cutProvenanceKind,
            int cutProvenanceIndex,
            float strictTolerance,
            PlaneCutNumericalRepairTelemetry numericalRepairs,
            out Vector3 snapped)
        {
            float cutResidualBefore = Mathf.Abs(plane.SignedDistance(point));
            float ownerResidualBefore = Mathf.Abs(
                Vector3.Dot(ownerPlaneNormal, point) - ownerPlaneDistance);
            snapped = ProjectPointOntoCutPlane(point, plane);
            float movement = (snapped - point).magnitude;
            float cutResidualAfter = Mathf.Abs(plane.SignedDistance(snapped));
            float ownerResidualAfter = Mathf.Abs(
                Vector3.Dot(ownerPlaneNormal, snapped) -
                ownerPlaneDistance);

            bool valid = IsFinite(snapped) &&
                cutResidualAfter <= strictTolerance &&
                ownerResidualAfter <= strictTolerance;
            if (!valid && TryConstrainPointToOwnerAndCutPlanes(
                point,
                ownerPlaneNormal,
                ownerPlaneDistance,
                plane,
                out Vector3 corrected))
            {
                snapped = corrected;
                movement = (snapped - point).magnitude;
                cutResidualAfter = Mathf.Abs(
                    plane.SignedDistance(snapped));
                ownerResidualAfter = Mathf.Abs(
                    Vector3.Dot(ownerPlaneNormal, snapped) -
                    ownerPlaneDistance);
                valid = IsFinite(snapped) &&
                    cutResidualAfter <= strictTolerance &&
                    ownerResidualAfter <= strictTolerance;
                if (numericalRepairs != null)
                {
                    numericalRepairs.IntersectionProjectionCount++;
                    numericalRepairs.MaximumIntersectionProjectionDistance =
                        Mathf.Max(
                            numericalRepairs
                                .MaximumIntersectionProjectionDistance,
                            movement);
                }
            }

            if (numericalRepairs != null)
            {
                numericalRepairs.OnPlaneSnapCount++;
                numericalRepairs.MaximumOnPlaneSnapDistance = Mathf.Max(
                    numericalRepairs.MaximumOnPlaneSnapDistance,
                    movement);
                RecordIntersectionResidualExtrema(
                    numericalRepairs,
                    cutResidualBefore,
                    cutResidualAfter,
                    ownerResidualBefore,
                    ownerResidualAfter);
            }

            if (valid)
            {
                return true;
            }

            RecordExactClipFailure(
                numericalRepairs,
                ownerProvenanceKind,
                ownerProvenanceIndex,
                cutProvenanceKind,
                cutProvenanceIndex,
                signedDistance,
                signedDistance,
                PlaneClipPointClassification.OnPlane.ToString(),
                PlaneClipPointClassification.OnPlane.ToString(),
                cutResidualBefore,
                cutResidualAfter,
                ownerResidualBefore,
                ownerResidualAfter,
                "on-plane-snap-residual-exceeds-strict-tolerance");
            return false;
        }

        private static Vector3 ResolveLegacyClipIntersection(
            Vector3 start,
            Vector3 end,
            float startDistance,
            float endDistance,
            bool clampToSegment,
            float denominatorEpsilon,
            Dictionary<EdgeKey, Vector3> intersectionCache)
        {
            if (intersectionCache != null)
            {
                EdgeKey edgeKey = new EdgeKey(start, end);
                if (intersectionCache.TryGetValue(
                    edgeKey,
                    out Vector3 cached))
                {
                    return cached;
                }

                Vector3 cachedIntersection = IntersectEdgeLegacy(
                    start,
                    end,
                    startDistance,
                    endDistance,
                    clampToSegment,
                    denominatorEpsilon);
                intersectionCache.Add(edgeKey, cachedIntersection);
                return cachedIntersection;
            }

            return IntersectEdgeLegacy(
                start,
                end,
                startDistance,
                endDistance,
                clampToSegment,
                denominatorEpsilon);
        }

        private static Vector3 IntersectEdgeLegacy(
            Vector3 start,
            Vector3 end,
            float startDistance,
            float endDistance,
            bool clampToSegment,
            float denominatorEpsilon)
        {
            float denominator = startDistance - endDistance;
            if (Mathf.Abs(denominator) <= denominatorEpsilon)
            {
                return start;
            }

            float t = startDistance / denominator;
            if (clampToSegment)
            {
                t = Mathf.Clamp01(t);
            }
            return Vector3.LerpUnclamped(start, end, t);
        }

        private static bool TryResolveExactClipIntersection(
            Vector3 start,
            Vector3 end,
            float startDistance,
            float endDistance,
            PlaneClipPointClassification startClassification,
            PlaneClipPointClassification endClassification,
            Vector3 ownerPlaneNormal,
            float ownerPlaneDistance,
            PolygonFaceProvenanceKind ownerProvenanceKind,
            int ownerProvenanceIndex,
            CutPlane plane,
            PolygonFaceProvenanceKind cutProvenanceKind,
            int cutProvenanceIndex,
            bool clampToSegment,
            float strictTolerance,
            Dictionary<EdgeKey, Vector3> intersectionCache,
            PlaneCutNumericalRepairTelemetry numericalRepairs,
            out Vector3 intersection)
        {
            if (numericalRepairs != null)
            {
                numericalRepairs.IntersectionRequestCount++;
            }

            bool strictCrossing =
                startClassification == PlaneClipPointClassification.Inside &&
                endClassification == PlaneClipPointClassification.Outside ||
                startClassification == PlaneClipPointClassification.Outside &&
                endClassification == PlaneClipPointClassification.Inside;
            if (!strictCrossing)
            {
                if (numericalRepairs != null)
                {
                    numericalRepairs.SameSideFallbackAttemptCount++;
                    RecordExactClipFailure(
                        numericalRepairs,
                        ownerProvenanceKind,
                        ownerProvenanceIndex,
                        cutProvenanceKind,
                        cutProvenanceIndex,
                        startDistance,
                        endDistance,
                        startClassification.ToString(),
                        endClassification.ToString(),
                        0f,
                        0f,
                        0f,
                        0f,
                        "same-side-intersection-request");
                }
                intersection = Vector3.zero;
                return false;
            }

            EdgeKey edgeKey = new EdgeKey(start, end);
            bool recomputingInvalidCachedIntersection = false;
            if (intersectionCache != null &&
                intersectionCache.TryGetValue(
                    edgeKey,
                    out Vector3 cached))
            {
                if (numericalRepairs != null)
                {
                    numericalRepairs.CachedIntersectionReuseCount++;
                }
                bool cachedValid = TryValidateExactIntersection(
                    cached,
                    ownerPlaneNormal,
                    ownerPlaneDistance,
                    plane,
                    strictTolerance,
                    out float cachedCutResidual,
                    out float cachedOwnerResidual,
                    out _,
                    out _);
                if (numericalRepairs != null)
                {
                    RecordIntersectionResidualExtrema(
                        numericalRepairs,
                        cachedCutResidual,
                        cachedCutResidual,
                        cachedOwnerResidual,
                        cachedOwnerResidual);
                }
                if (cachedValid)
                {
                    intersection = cached;
                    return true;
                }

                recomputingInvalidCachedIntersection = true;
                intersectionCache.Remove(edgeKey);
                if (numericalRepairs != null)
                {
                    numericalRepairs.CachedIntersectionInvalidationCount++;
                }
            }

            float denominator = startDistance - endDistance;
            if (float.IsNaN(denominator) ||
                float.IsInfinity(denominator) ||
                Mathf.Abs(denominator) <= MinimumEdgeLengthSqr)
            {
                RecordExactClipFailure(
                    numericalRepairs,
                    ownerProvenanceKind,
                    ownerProvenanceIndex,
                    cutProvenanceKind,
                    cutProvenanceIndex,
                    startDistance,
                    endDistance,
                    startClassification.ToString(),
                    endClassification.ToString(),
                    0f,
                    0f,
                    0f,
                    0f,
                    "invalid-crossing-denominator");
                intersection = Vector3.zero;
                return false;
            }

            float t = startDistance / denominator;
            if (float.IsNaN(t) || float.IsInfinity(t))
            {
                RecordExactClipFailure(
                    numericalRepairs,
                    ownerProvenanceKind,
                    ownerProvenanceIndex,
                    cutProvenanceKind,
                    cutProvenanceIndex,
                    startDistance,
                    endDistance,
                    startClassification.ToString(),
                    endClassification.ToString(),
                    0f,
                    0f,
                    0f,
                    0f,
                    "non-finite-crossing-parameter");
                intersection = Vector3.zero;
                return false;
            }
            if (clampToSegment)
            {
                t = Mathf.Clamp01(t);
            }

            Vector3 rawIntersection = Vector3.LerpUnclamped(
                start,
                end,
                t);
            float cutResidualBefore;
            float ownerResidualBefore;
            float cutResidualAfter;
            float ownerResidualAfter;
            Vector3 resolved = rawIntersection;
            bool valid = TryValidateExactIntersection(
                resolved,
                ownerPlaneNormal,
                ownerPlaneDistance,
                plane,
                strictTolerance,
                out cutResidualBefore,
                out ownerResidualBefore,
                out cutResidualAfter,
                out ownerResidualAfter);

            if (!valid && TryConstrainPointToOwnerAndCutPlanes(
                rawIntersection,
                ownerPlaneNormal,
                ownerPlaneDistance,
                plane,
                out Vector3 corrected))
            {
                float movement = (corrected - rawIntersection).magnitude;
                if (numericalRepairs != null)
                {
                    numericalRepairs.IntersectionProjectionCount++;
                    numericalRepairs.MaximumIntersectionProjectionDistance =
                        Mathf.Max(
                            numericalRepairs
                                .MaximumIntersectionProjectionDistance,
                            movement);
                }
                resolved = corrected;
                valid = TryValidateExactIntersection(
                    resolved,
                    ownerPlaneNormal,
                    ownerPlaneDistance,
                    plane,
                    strictTolerance,
                    out _,
                    out _,
                    out cutResidualAfter,
                    out ownerResidualAfter);
            }

            if (numericalRepairs != null)
            {
                RecordIntersectionResidualExtrema(
                    numericalRepairs,
                    cutResidualBefore,
                    cutResidualAfter,
                    ownerResidualBefore,
                    ownerResidualAfter);
            }

            if (!valid)
            {
                RecordExactClipFailure(
                    numericalRepairs,
                    ownerProvenanceKind,
                    ownerProvenanceIndex,
                    cutProvenanceKind,
                    cutProvenanceIndex,
                    startDistance,
                    endDistance,
                    startClassification.ToString(),
                    endClassification.ToString(),
                    cutResidualBefore,
                    cutResidualAfter,
                    ownerResidualBefore,
                    ownerResidualAfter,
                    "intersection-residual-exceeds-strict-tolerance");
                intersection = Vector3.zero;
                return false;
            }

            if (numericalRepairs != null)
            {
                numericalRepairs.StrictCrossingIntersectionCount++;
                if (recomputingInvalidCachedIntersection)
                {
                    numericalRepairs
                        .CachedIntersectionRecomputeSuccessCount++;
                }
            }
            if (intersectionCache != null)
            {
                intersectionCache.Add(edgeKey, resolved);
            }
            intersection = resolved;
            return true;
        }

        private static bool TryValidateExactIntersection(
            Vector3 point,
            Vector3 ownerPlaneNormal,
            float ownerPlaneDistance,
            CutPlane plane,
            float strictTolerance,
            out float cutResidualBefore,
            out float ownerResidualBefore,
            out float cutResidualAfter,
            out float ownerResidualAfter)
        {
            cutResidualBefore = Mathf.Abs(plane.SignedDistance(point));
            ownerResidualBefore = Mathf.Abs(
                Vector3.Dot(ownerPlaneNormal, point) - ownerPlaneDistance);
            cutResidualAfter = cutResidualBefore;
            ownerResidualAfter = ownerResidualBefore;
            return IsFinite(point) &&
                cutResidualAfter <= strictTolerance &&
                ownerResidualAfter <= strictTolerance;
        }

        private static bool TryConstrainPointToOwnerAndCutPlanes(
            Vector3 point,
            Vector3 ownerPlaneNormal,
            float ownerPlaneDistance,
            CutPlane cutPlane,
            out Vector3 corrected)
        {
            float dot = Vector3.Dot(ownerPlaneNormal, cutPlane.Normal);
            float determinant = 1f - dot * dot;
            if (determinant <= 0.000001f)
            {
                corrected = point;
                return false;
            }

            float ownerError = ownerPlaneDistance -
                Vector3.Dot(ownerPlaneNormal, point);
            float cutError = cutPlane.Distance -
                Vector3.Dot(cutPlane.Normal, point);
            float ownerScale = (ownerError - dot * cutError) /
                determinant;
            float cutScale = (cutError - dot * ownerError) /
                determinant;
            corrected = point +
                ownerPlaneNormal * ownerScale +
                cutPlane.Normal * cutScale;
            return IsFinite(corrected);
        }

        private static void RecordIntersectionResidualExtrema(
            PlaneCutNumericalRepairTelemetry numericalRepairs,
            float cutResidualBefore,
            float cutResidualAfter,
            float ownerResidualBefore,
            float ownerResidualAfter)
        {
            numericalRepairs.MaximumCutPlaneResidualBeforeCorrection =
                Mathf.Max(
                    numericalRepairs
                        .MaximumCutPlaneResidualBeforeCorrection,
                    cutResidualBefore);
            numericalRepairs.MaximumCutPlaneResidualAfterCorrection =
                Mathf.Max(
                    numericalRepairs
                        .MaximumCutPlaneResidualAfterCorrection,
                    cutResidualAfter);
            numericalRepairs.MaximumOwnerPlaneResidualBeforeCorrection =
                Mathf.Max(
                    numericalRepairs
                        .MaximumOwnerPlaneResidualBeforeCorrection,
                    ownerResidualBefore);
            numericalRepairs.MaximumOwnerPlaneResidualAfterCorrection =
                Mathf.Max(
                    numericalRepairs
                        .MaximumOwnerPlaneResidualAfterCorrection,
                    ownerResidualAfter);
        }

        private static void RecordExactClipFailure(
            PlaneCutNumericalRepairTelemetry numericalRepairs,
            PolygonFaceProvenanceKind ownerProvenanceKind,
            int ownerProvenanceIndex,
            PolygonFaceProvenanceKind cutProvenanceKind,
            int cutProvenanceIndex,
            float startDistance,
            float endDistance,
            string startClassification,
            string endClassification,
            float cutResidualBefore,
            float cutResidualAfter,
            float ownerResidualBefore,
            float ownerResidualAfter,
            string reason)
        {
            if (numericalRepairs == null)
            {
                return;
            }

            numericalRepairs.ExactConstructionFailureCount++;
            if (numericalRepairs.FirstExactFailureRecorded != 0)
            {
                return;
            }

            numericalRepairs.FirstExactFailureRecorded = 1;
            numericalRepairs.FirstExactFailureOwnerProvenanceKind =
                ownerProvenanceKind;
            numericalRepairs.FirstExactFailureOwnerProvenanceIndex =
                ownerProvenanceIndex;
            numericalRepairs.FirstExactFailureCutProvenanceKind =
                cutProvenanceKind;
            numericalRepairs.FirstExactFailureCutProvenanceIndex =
                cutProvenanceIndex;
            numericalRepairs.FirstExactFailureStartDistance = startDistance;
            numericalRepairs.FirstExactFailureEndDistance = endDistance;
            numericalRepairs.FirstExactFailureStartClassification =
                startClassification;
            numericalRepairs.FirstExactFailureEndClassification =
                endClassification;
            numericalRepairs.FirstExactFailureCutResidualBefore =
                cutResidualBefore;
            numericalRepairs.FirstExactFailureCutResidualAfter =
                cutResidualAfter;
            numericalRepairs.FirstExactFailureOwnerResidualBefore =
                ownerResidualBefore;
            numericalRepairs.FirstExactFailureOwnerResidualAfter =
                ownerResidualAfter;
            numericalRepairs.FirstExactFailureReason = reason;
        }

        private static Vector3 ProjectPointOntoCutPlane(
            Vector3 point,
            CutPlane plane)
        {
            return point - plane.Normal * plane.SignedDistance(point);
        }

        private static PolygonFace CreateOrientedFace(
            Vector3 outwardNormal,
            params Vector3[] points)
        {
            return CreateOrientedFace(
                outwardNormal,
                PolygonFaceFeature.Base,
                0f,
                points);
        }

        private static PolygonFace CreateOrientedFace(
            Vector3 outwardNormal,
            PolygonFaceFeature feature,
            float featureStrength,
            params Vector3[] points)
        {
            List<Vector3> ordered = new List<Vector3>(points);
            Vector3 centre = CalculateAverage(ordered);

            Vector3 tangent = Mathf.Abs(outwardNormal.y) < 0.9f
                ? Vector3.Cross(outwardNormal, Vector3.up).normalized
                : Vector3.Cross(outwardNormal, Vector3.right).normalized;

            Vector3 bitangent =
                Vector3.Cross(outwardNormal, tangent).normalized;

            ordered.Sort((left, right) =>
            {
                Vector3 leftOffset = left - centre;
                Vector3 rightOffset = right - centre;

                float leftAngle = Mathf.Atan2(
                    Vector3.Dot(leftOffset, bitangent),
                    Vector3.Dot(leftOffset, tangent));

                float rightAngle = Mathf.Atan2(
                    Vector3.Dot(rightOffset, bitangent),
                    Vector3.Dot(rightOffset, tangent));

                return leftAngle.CompareTo(rightAngle);
            });

            Vector3 calculatedNormal = CalculatePolygonNormal(ordered);

            if (Vector3.Dot(calculatedNormal, outwardNormal) < 0f)
            {
                ordered.Reverse();
            }

            return new PolygonFace(
                ordered,
                outwardNormal.normalized,
                feature,
                featureStrength);
        }

        private static Vector3 CalculatePolygonNormal(
            List<Vector3> vertices)
        {
            Vector3 normal = Vector3.zero;

            for (int i = 0; i < vertices.Count; i++)
            {
                Vector3 current = vertices[i];
                Vector3 next = vertices[(i + 1) % vertices.Count];

                normal.x += (current.y - next.y) * (current.z + next.z);
                normal.y += (current.z - next.z) * (current.x + next.x);
                normal.z += (current.x - next.x) * (current.y + next.y);
            }

            float normalSqrMagnitude = normal.sqrMagnitude;

            return normalSqrMagnitude > MinimumEdgeLengthSqr
                ? normal / Mathf.Sqrt(normalSqrMagnitude)
                : Vector3.up;
        }

        private static TriangleSoup TriangulatePolyhedron(
            List<PolygonFace> faces,
            SurfaceFacetDensity density,
            EdgeCharacter edgeCharacter,
            int surfaceSeed)
        {
            TriangleSoup soup = new TriangleSoup();
            int edgeSegments = GetBoundarySegments(density);

            for (int faceIndex = 0; faceIndex < faces.Count; faceIndex++)
            {
                PolygonFace sourceFace = faces[faceIndex];

                List<Vector3> cleanFaceVertices = SanitizePolygon(
                    sourceFace.Vertices,
                    sourceFace.Normal);

                if (cleanFaceVertices.Count < 3 ||
                    CalculatePolygonArea(cleanFaceVertices) <= TinyFaceAreaEpsilon)
                {
                    continue;
                }

                PolygonFace face = new PolygonFace(
                    cleanFaceVertices,
                    sourceFace.Normal,
                    sourceFace.Feature,
                    sourceFace.FeatureStrength);

                if (density == SurfaceFacetDensity.Sparse ||
                    face.Feature == PolygonFaceFeature.ConvexEdgeWear)
                {
                    for (int i = 1; i < face.Vertices.Count - 1; i++)
                    {
                        AddOrientedTriangle(
                            soup,
                            face.Vertices[0],
                            face.Vertices[i],
                            face.Vertices[i + 1],
                            face.Normal,
                            face.Feature,
                            face.FeatureStrength);
                    }

                    continue;
                }

                List<Vector3> boundary = BuildSegmentedBoundary(
                    face.Vertices,
                    edgeSegments);

                Vector3 centre = CalculateAverage(boundary);
                float faceRadius = CalculateAverageRadius(boundary, centre);

                float relief = GetSurfaceRelief(density) * faceRadius;
                relief *= GetReliefMultiplier(edgeCharacter);

                float signedVariation =
                    HashSigned(surfaceSeed, faceIndex);

                centre += face.Normal * relief * signedVariation;

                for (int i = 0; i < boundary.Count; i++)
                {
                    Vector3 current = boundary[i];
                    Vector3 next = boundary[(i + 1) % boundary.Count];

                    AddOrientedTriangle(
                        soup,
                        centre,
                        current,
                        next,
                        face.Normal,
                        face.Feature,
                        face.FeatureStrength);
                }
            }

            return soup;
        }

private static List<Vector3> BuildSegmentedBoundary(
            List<Vector3> vertices,
            int segmentsPerEdge)
        {
            List<Vector3> result = new List<Vector3>();

            for (int i = 0; i < vertices.Count; i++)
            {
                Vector3 start = vertices[i];
                Vector3 end = vertices[(i + 1) % vertices.Count];

                for (int segment = 0; segment < segmentsPerEdge; segment++)
                {
                    float t = segment / (float)segmentsPerEdge;
                    result.Add(Vector3.Lerp(start, end, t));
                }
            }

            return result;
        }

        #endregion
    }
}
