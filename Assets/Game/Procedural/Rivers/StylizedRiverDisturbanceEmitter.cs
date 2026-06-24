using System;
using System.Collections.Generic;
using ProgrammaticStylized3D.Geometry;
using UnityEngine;
using UnityEngine.Serialization;

namespace ProgrammaticStylized3D.Rivers
{
    public enum StylizedRiverDisturbanceSourceMobility
    {
        Static,
        Dynamic
    }

    public enum StylizedRiverDisturbanceFootprintMode
    {
        Automatic,
        Manual
    }

    [DisallowMultipleComponent]
    [AddComponentMenu("Programmatic Stylized 3D/Rivers/River Disturbance Emitter")]
    public sealed class StylizedRiverDisturbanceEmitter : MonoBehaviour
    {
        private const float MinimumFootprintHalfExtent = 0.05f;
        private const float WaterlineIntersectionEpsilon = 0.005f;

        private static readonly int[,] BoundsEdges =
        {
            { 0, 1 }, { 0, 2 }, { 0, 4 },
            { 1, 3 }, { 1, 5 }, { 2, 3 },
            { 2, 6 }, { 3, 7 }, { 4, 5 },
            { 4, 6 }, { 5, 7 }, { 6, 7 }
        };
        

        [Tooltip("Optional explicit river. Leave empty to locate the active river footprint automatically.")]
        [SerializeField] private StylizedRiver explicitRiver;

        [Tooltip("Automatically chooses the enabled river whose footprint contains this object.")]
        [SerializeField] private bool autoDetectRiver = true;

        [Tooltip("Maximum allowed vertical distance from the mean river surface when automatically detecting contact.")]
        [Min(0.05f)]
        [SerializeField] private float verticalContactTolerance = 1.25f;

        [Header("Source Footprint")]
        [Tooltip("Static sources are projected and cached once, then rebuilt only when the source, generated geometry, or river domain changes. Dynamic sources continue to submit swept movement samples.")]
        [SerializeField]
        private StylizedRiverDisturbanceSourceMobility sourceMobility =
            StylizedRiverDisturbanceSourceMobility.Static;

        [Tooltip("Automatic derives a static water-contact footprint from the final mesh at the river surface. Manual uses the authored dimensions below.")]
        [SerializeField]
        private StylizedRiverDisturbanceFootprintMode footprintMode =
            StylizedRiverDisturbanceFootprintMode.Automatic;

        [Tooltip("Optional mesh used for automatic footprint detection. Leave empty to use an IGeneratedGeometrySource, MeshFilter on this object, parent, or child.")]
        [SerializeField] private MeshFilter automaticMeshFilter;

        [Tooltip("Extra metres added around the detected waterline footprint so the pressure ridge remains visible outside the object silhouette.")]
        [Range(0f, 2f)]
        [SerializeField] private float automaticFootprintPadding = 0.12f;

        [Tooltip("Uses separate across-flow and along-flow dimensions. Disable this to use one linked radius for compact manual sources.")]
        [SerializeField] private bool useSeparateFootprintDimensions;

        [FormerlySerializedAs("radius")]
        [Tooltip("Linked manual water-contact half-size in metres. Used for both footprint dimensions when Separate Footprint Dimensions is disabled.")]
        [Range(0.05f, 8f)]
        [SerializeField] private float linkedFootprintRadius = 0.35f;

        [Tooltip("Manual half-width of the water-contact footprint measured across the river, in metres.")]
        [Range(0.05f, 12f)]
        [SerializeField] private float acrossFlowHalfWidth = 0.35f;

        [Tooltip("Manual half-length of the water-contact footprint measured along the river, in metres.")]
        [Range(0.05f, 12f)]
        [SerializeField] private float alongFlowHalfLength = 0.35f;

        [Header("Influence")]
        [Tooltip("Local source strength. Values above the normal production range are available for deliberate visual stress testing.")]
        [Range(0f, 8f)]
        [SerializeField] private float strength = 1f;

        [Tooltip("Contribution to broad geometric wake and ripple height.")]
        [Range(0f, 1f)]
        [SerializeField] private float geometryContribution = 0.65f;

        [Tooltip("Contribution to fine lighting and refraction disturbance without intentionally adding bulk water height.")]
        [Range(0f, 1f)]
        [SerializeField] private float normalContribution = 1f;

        [Tooltip("When movement is slow, a dynamic source becomes a generic flow obstruction. Static sources always use the cached obstruction profile.")]
        [SerializeField] private bool stationaryObstruction = true;

        [Tooltip("Creates one impact after an observed outside-to-inside river transition for a dynamic source. Merely enabling a source already in water does not emit an impact.")]
        [SerializeField] private bool emitEntryImpact = true;

        [Tooltip("Creates one smaller impact when a dynamic source leaves the river.")]
        [SerializeField] private bool emitExitImpact;

        [Tooltip("CPU source-registration interval for dynamic sources and for retrying an unresolved static registration. Swept injection bridges movement between samples.")]
        [Range(0.025f, 0.2f)]
        [SerializeField] private float sourceUpdateInterval = 0.05f;

        private readonly List<Vector3> waterlinePoints = new();
        private readonly Vector3[] boundsCorners = new Vector3[8];

        private StylizedRiverDisturbanceRuntime currentRuntime;
        private StylizedRiver currentRiver;
        private IGeneratedGeometrySource generatedGeometrySource;
        private MonoBehaviour generatedGeometrySourceBehaviour;
        private Vector3 previousSamplePosition;
        private float updateAccumulator;
        private bool wasInside;
        private bool hasObservedContactState;
        private bool staticRegistrationDirty = true;
        private bool automaticFootprintDirty = true;
        private bool hasResolvedAutomaticFootprint;
        private Vector3 resolvedAutomaticWorldPosition;
        private Vector3 resolvedAutomaticLocalPosition;
        private float resolvedAutomaticAcrossHalfWidth;
        private float resolvedAutomaticAlongHalfLength;
        private string automaticFootprintStatus =
            "Automatic footprint has not been resolved yet.";

        private EntityId SourceId => GetEntityId();

        public StylizedRiverDisturbanceSourceMobility SourceMobility =>
            sourceMobility;
        public StylizedRiverDisturbanceFootprintMode FootprintMode =>
            footprintMode;
        public bool HasResolvedAutomaticFootprint =>
            hasResolvedAutomaticFootprint;
        public float ResolvedAutomaticAcrossHalfWidth =>
            resolvedAutomaticAcrossHalfWidth;
        public float ResolvedAutomaticAlongHalfLength =>
            resolvedAutomaticAlongHalfLength;
        public string AutomaticFootprintStatus => automaticFootprintStatus;
        public bool IsAutomaticallyManagedGeneratedSource =>
            sourceMobility ==
                StylizedRiverDisturbanceSourceMobility.Static &&
            generatedGeometrySource != null &&
            (!(generatedGeometrySource is UnityEngine.Object unityObject) ||
             unityObject != null) &&
            generatedGeometrySource.IsSolidGeometry &&
            generatedGeometrySource.IsStaticGeometry &&
            GeneratedGeometryRegistry.Contains(generatedGeometrySource);

        private bool UsesAutomaticFootprint =>
            sourceMobility == StylizedRiverDisturbanceSourceMobility.Static &&
            footprintMode == StylizedRiverDisturbanceFootprintMode.Automatic;

        private float ManualAcrossHalfWidth =>
            Mathf.Max(
                MinimumFootprintHalfExtent,
                useSeparateFootprintDimensions
                    ? acrossFlowHalfWidth
                    : linkedFootprintRadius);

        private float ManualAlongHalfLength =>
            Mathf.Max(
                MinimumFootprintHalfExtent,
                useSeparateFootprintDimensions
                    ? alongFlowHalfLength
                    : linkedFootprintRadius);

        private float ResolvedAcrossHalfWidth =>
            UsesAutomaticFootprint && hasResolvedAutomaticFootprint
                ? resolvedAutomaticAcrossHalfWidth
                : ManualAcrossHalfWidth;

        private float ResolvedAlongHalfLength =>
            UsesAutomaticFootprint && hasResolvedAutomaticFootprint
                ? resolvedAutomaticAlongHalfLength
                : ManualAlongHalfLength;

        private float ResolvedImpactRadius =>
            Mathf.Max(ResolvedAcrossHalfWidth, ResolvedAlongHalfLength);

        private Vector3 ResolvedSourcePosition =>
            UsesAutomaticFootprint && hasResolvedAutomaticFootprint
                ? resolvedAutomaticWorldPosition
                : transform.position;

        private void OnEnable()
        {
            RefreshGeneratedGeometrySubscription();
            previousSamplePosition = transform.position;
            updateAccumulator = sourceUpdateInterval;
            wasInside = false;
            hasObservedContactState = false;
            staticRegistrationDirty = true;
            automaticFootprintDirty = true;
            transform.hasChanged = false;
        }

        private void OnValidate()
        {
            verticalContactTolerance = Mathf.Max(
                0.05f,
                verticalContactTolerance);
            automaticFootprintPadding = Mathf.Clamp(
                automaticFootprintPadding,
                0f,
                2f);
            linkedFootprintRadius = Mathf.Clamp(
                linkedFootprintRadius,
                0.05f,
                8f);
            acrossFlowHalfWidth = Mathf.Clamp(
                acrossFlowHalfWidth,
                0.05f,
                12f);
            alongFlowHalfLength = Mathf.Clamp(
                alongFlowHalfLength,
                0.05f,
                12f);
            strength = Mathf.Clamp(strength, 0f, 8f);
            geometryContribution = Mathf.Clamp01(geometryContribution);
            normalContribution = Mathf.Clamp01(normalContribution);
            sourceUpdateInterval = Mathf.Clamp(
                sourceUpdateInterval,
                0.025f,
                0.2f);

            if (isActiveAndEnabled)
            {
                RefreshGeneratedGeometrySubscription();
            }

            staticRegistrationDirty = true;
            automaticFootprintDirty = true;
        }

        private void Update()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (sourceMobility ==
                StylizedRiverDisturbanceSourceMobility.Static)
            {
                UpdateStaticSource();
                return;
            }

            UpdateDynamicSource();
        }

        private void UpdateStaticSource()
        {
            if (IsAutomaticallyManagedGeneratedSource)
            {
                ChangeRuntime(null);
                staticRegistrationDirty = false;
                transform.hasChanged = false;
                return;
            }

            if (transform.hasChanged)
            {
                staticRegistrationDirty = true;
                automaticFootprintDirty = true;
            }

            if (!staticRegistrationDirty && currentRuntime != null)
            {
                updateAccumulator += Time.deltaTime;
                if (updateAccumulator < 1f)
                {
                    return;
                }

                updateAccumulator = 0f;
                if (currentRuntime.ContainsContinuousSource(SourceId) &&
                    (!UsesAutomaticFootprint ||
                     hasResolvedAutomaticFootprint))
                {
                    return;
                }

                staticRegistrationDirty = true;
                automaticFootprintDirty = true;
            }

            updateAccumulator += Time.deltaTime;
            if (updateAccumulator < sourceUpdateInterval)
            {
                return;
            }

            updateAccumulator = 0f;
            StylizedRiverDisturbanceRuntime resolvedRuntime =
                ResolveStaticRuntime();
            ChangeRuntime(resolvedRuntime);

            if (currentRuntime != null && UsesAutomaticFootprint &&
                (automaticFootprintDirty ||
                 !hasResolvedAutomaticFootprint))
            {
                TryResolveAutomaticFootprint(currentRuntime);
            }

            Vector3 sourcePosition = ResolvedSourcePosition;
            bool registered =
                currentRuntime != null &&
                currentRuntime.RegisterStaticSource(
                    SourceId,
                    sourcePosition,
                    ResolvedAcrossHalfWidth,
                    ResolvedAlongHalfLength,
                    strength,
                    geometryContribution,
                    normalContribution);

            staticRegistrationDirty = !registered;
            transform.hasChanged = false;
            previousSamplePosition = sourcePosition;
            wasInside = registered;
            hasObservedContactState = true;
        }

        private void UpdateDynamicSource()
        {
            updateAccumulator += Time.deltaTime;
            if (updateAccumulator < sourceUpdateInterval)
            {
                return;
            }

            float sampleDelta = Mathf.Max(0.001f, updateAccumulator);
            updateAccumulator = 0f;
            Vector3 currentPosition = transform.position;

            StylizedRiverDisturbanceRuntime resolvedRuntime =
                ResolveRuntime(currentPosition);
            StylizedRiverDisturbanceRuntime previousRuntime =
                currentRuntime;
            bool isInside = resolvedRuntime != null;

            if (resolvedRuntime != currentRuntime)
            {
                ChangeRuntime(resolvedRuntime);
                previousSamplePosition = currentPosition;
            }

            bool observedEntry =
                hasObservedContactState &&
                !wasInside &&
                isInside;

            bool observedExit =
                hasObservedContactState &&
                wasInside &&
                !isInside;

            if (isInside)
            {
                if (observedEntry && emitEntryImpact)
                {
                    currentRuntime.EmitImpact(
                        currentPosition,
                        ResolvedImpactRadius * 1.15f,
                        strength,
                        geometryContribution,
                        normalContribution);
                }

                currentRuntime.UpdateContinuousSource(
                    SourceId,
                    previousSamplePosition,
                    currentPosition,
                    sampleDelta,
                    ManualAcrossHalfWidth,
                    ManualAlongHalfLength,
                    strength,
                    geometryContribution,
                    normalContribution,
                    stationaryObstruction);
            }
            else if (observedExit && emitExitImpact && previousRuntime != null)
            {
                previousRuntime.EmitImpact(
                    previousSamplePosition,
                    ResolvedImpactRadius,
                    strength * 0.55f,
                    geometryContribution,
                    normalContribution);
            }

            hasObservedContactState = true;
            wasInside = isInside;
            previousSamplePosition = currentPosition;
        }

        private void OnDisable()
        {
            ChangeRuntime(null);
            ClearGeneratedGeometrySubscription();
            wasInside = false;
            hasObservedContactState = false;
            staticRegistrationDirty = true;
            automaticFootprintDirty = true;
        }

        [ContextMenu("Emit Impact Now")]
        public void EmitImpactNow()
        {
            StylizedRiverDisturbanceRuntime runtime =
                ResolveRuntime(transform.position);

            runtime?.EmitImpact(
                transform.position,
                ResolvedImpactRadius,
                strength,
                geometryContribution,
                normalContribution);
        }

        private StylizedRiverDisturbanceRuntime ResolveStaticRuntime()
        {
            StylizedRiverDisturbanceRuntime runtime =
                ResolveRuntime(transform.position);

            if (runtime != null || !UsesAutomaticFootprint)
            {
                return runtime;
            }

            MeshFilter meshFilter = ResolveAutomaticMeshFilter();
            if (!TryGetWorldBounds(meshFilter, out Bounds worldBounds))
            {
                return null;
            }

            Vector3 candidate = worldBounds.center;
            for (int index = 0; index <= 12; index++)
            {
                candidate.y = Mathf.Lerp(
                    worldBounds.min.y,
                    worldBounds.max.y,
                    index / 12f);
                runtime = ResolveRuntime(candidate);

                if (runtime != null)
                {
                    return runtime;
                }
            }

            return null;
        }

        private StylizedRiverDisturbanceRuntime ResolveRuntime(
            Vector3 worldPosition)
        {
            if (explicitRiver != null)
            {
                StylizedRiverDisturbanceRuntime runtime =
                    explicitRiver.GetOrCreateDisturbanceRuntime();

                if (runtime != null &&
                    explicitRiver.RuntimeDisturbancesEnabled &&
                    explicitRiver.TryProjectWorldPoint(
                        worldPosition,
                        out StylizedRiverProjection projection) &&
                    projection.IsInside &&
                    Mathf.Abs(
                        worldPosition.y -
                        projection.SurfacePoint.y) <=
                    verticalContactTolerance)
                {
                    return runtime;
                }

                return null;
            }

            if (!autoDetectRiver)
            {
                return null;
            }

            return StylizedRiverDisturbanceRuntime.TryFindContainingRiver(
                worldPosition,
                verticalContactTolerance,
                out StylizedRiverDisturbanceRuntime runtimeFound,
                out _)
                ? runtimeFound
                : null;
        }

        private void ChangeRuntime(
            StylizedRiverDisturbanceRuntime nextRuntime)
        {
            if (nextRuntime == currentRuntime)
            {
                return;
            }

            currentRuntime?.RemoveContinuousSource(SourceId);

            if (currentRiver != null)
            {
                currentRiver.DomainChanged -= HandleRiverDomainChanged;
            }

            currentRuntime = nextRuntime;
            currentRiver =
                currentRuntime != null
                    ? currentRuntime.GetComponent<StylizedRiver>()
                    : null;

            if (currentRiver != null)
            {
                currentRiver.DomainChanged += HandleRiverDomainChanged;
            }

            staticRegistrationDirty = true;
            automaticFootprintDirty = true;
        }

        private void HandleRiverDomainChanged(RiverDomainSnapshot snapshot)
        {
            staticRegistrationDirty = true;
            automaticFootprintDirty = true;
        }

        private bool TryResolveAutomaticFootprint(
            StylizedRiverDisturbanceRuntime runtime)
        {
            automaticFootprintDirty = false;
            hasResolvedAutomaticFootprint = false;

            StylizedRiver river =
                runtime != null
                    ? runtime.GetComponent<StylizedRiver>()
                    : null;
            MeshFilter meshFilter = ResolveAutomaticMeshFilter();
            Mesh mesh = meshFilter != null ? meshFilter.sharedMesh : null;

            if (river == null)
            {
                automaticFootprintStatus =
                    "Automatic footprint could not resolve a river.";
                return false;
            }

            if (meshFilter == null || mesh == null)
            {
                automaticFootprintStatus =
                    "No generated geometry source or MeshFilter was found. Manual dimensions are being used.";
                return false;
            }

            if (!TryGetWorldBounds(meshFilter, out Bounds worldBounds) ||
                !river.TryProjectWorldPoint(
                    worldBounds.center,
                    out StylizedRiverProjection projection))
            {
                automaticFootprintStatus =
                    "The mesh could not be projected into the river domain. Manual dimensions are being used.";
                return false;
            }

            Vector3 up = projection.Up.sqrMagnitude > 0.0001f
                ? projection.Up.normalized
                : Vector3.up;
            Vector3 downstream =
                projection.Tangent * river.FlowDirection;
            downstream.y = 0f;
            downstream = downstream.sqrMagnitude > 0.0001f
                ? downstream.normalized
                : Vector3.forward;
            Vector3 across = projection.Side.sqrMagnitude > 0.0001f
                ? projection.Side.normalized
                : Vector3.Cross(up, downstream).normalized;
            Vector3 planePoint = projection.SurfacePoint;

            waterlinePoints.Clear();
            bool usedBoundsFallback = false;

            try
            {
                Vector3[] vertices = mesh.vertices;
                int[] triangles = mesh.triangles;

                for (int index = 0;
                     index + 2 < triangles.Length;
                     index += 3)
                {
                    Vector3 a = meshFilter.transform.TransformPoint(
                        vertices[triangles[index]]);
                    Vector3 b = meshFilter.transform.TransformPoint(
                        vertices[triangles[index + 1]]);
                    Vector3 c = meshFilter.transform.TransformPoint(
                        vertices[triangles[index + 2]]);

                    AddTrianglePlaneIntersections(
                        a,
                        b,
                        c,
                        planePoint,
                        up,
                        waterlinePoints);
                }
            }
            catch (UnityException)
            {
                waterlinePoints.Clear();
            }

            if (waterlinePoints.Count < 2)
            {
                AddBoundsPlaneIntersections(
                    meshFilter,
                    mesh.bounds,
                    planePoint,
                    up,
                    waterlinePoints);
                usedBoundsFallback = true;
            }

            if (waterlinePoints.Count < 2)
            {
                automaticFootprintStatus =
                    "The mesh does not intersect the river surface. Manual dimensions are being used.";
                return false;
            }

            float minimumAlong = float.PositiveInfinity;
            float maximumAlong = float.NegativeInfinity;
            float minimumAcross = float.PositiveInfinity;
            float maximumAcross = float.NegativeInfinity;

            for (int index = 0; index < waterlinePoints.Count; index++)
            {
                Vector3 offset = waterlinePoints[index] - planePoint;
                float along = Vector3.Dot(offset, downstream);
                float lateral = Vector3.Dot(offset, across);
                minimumAlong = Mathf.Min(minimumAlong, along);
                maximumAlong = Mathf.Max(maximumAlong, along);
                minimumAcross = Mathf.Min(minimumAcross, lateral);
                maximumAcross = Mathf.Max(maximumAcross, lateral);
            }

            if (float.IsInfinity(minimumAlong) ||
                float.IsInfinity(minimumAcross))
            {
                automaticFootprintStatus =
                    "The automatic waterline footprint was invalid. Manual dimensions are being used.";
                return false;
            }

            float centreAlong = (minimumAlong + maximumAlong) * 0.5f;
            float centreAcross = (minimumAcross + maximumAcross) * 0.5f;
            resolvedAutomaticAlongHalfLength = Mathf.Max(
                MinimumFootprintHalfExtent,
                (maximumAlong - minimumAlong) * 0.5f +
                automaticFootprintPadding);
            resolvedAutomaticAcrossHalfWidth = Mathf.Max(
                MinimumFootprintHalfExtent,
                (maximumAcross - minimumAcross) * 0.5f +
                automaticFootprintPadding);
            resolvedAutomaticWorldPosition =
                planePoint +
                downstream * centreAlong +
                across * centreAcross;
            resolvedAutomaticLocalPosition =
                transform.InverseTransformPoint(
                    resolvedAutomaticWorldPosition);
            hasResolvedAutomaticFootprint = true;

            string sourceName =
                generatedGeometrySourceBehaviour != null
                    ? generatedGeometrySourceBehaviour.GetType().Name
                    : meshFilter.name;
            automaticFootprintStatus = usedBoundsFallback
                ? $"Resolved from {sourceName} bounds because readable waterline triangles were unavailable."
                : $"Resolved automatically from the {sourceName} mesh at the river surface.";
            return true;
        }

        private MeshFilter ResolveAutomaticMeshFilter()
        {
            if (automaticMeshFilter != null)
            {
                return automaticMeshFilter;
            }

            if (generatedGeometrySource != null &&
                generatedGeometrySourceBehaviour != null)
            {
                MeshFilter generatedFilter =
                    generatedGeometrySource.GeometryMeshFilter;

                if (generatedFilter != null)
                {
                    return generatedFilter;
                }
            }

            MeshFilter filter = GetComponent<MeshFilter>();
            if (filter != null)
            {
                return filter;
            }

            filter = GetComponentInParent<MeshFilter>();
            return filter != null
                ? filter
                : GetComponentInChildren<MeshFilter>();
        }

        private void RefreshGeneratedGeometrySubscription()
        {
            IGeneratedGeometrySource nextSource =
                FindGeneratedGeometrySource();
            MonoBehaviour nextBehaviour = nextSource as MonoBehaviour;

            if (ReferenceEquals(nextSource, generatedGeometrySource) &&
                nextBehaviour == generatedGeometrySourceBehaviour)
            {
                return;
            }

            ClearGeneratedGeometrySubscription();
            generatedGeometrySource = nextSource;
            generatedGeometrySourceBehaviour = nextBehaviour;

            if (generatedGeometrySource != null)
            {
                generatedGeometrySource.GeometryChanged +=
                    HandleGeneratedGeometryChanged;
            }

            automaticFootprintDirty = true;
            staticRegistrationDirty = true;
        }

        private IGeneratedGeometrySource FindGeneratedGeometrySource()
        {
            GameObject searchObject =
                automaticMeshFilter != null
                    ? automaticMeshFilter.gameObject
                    : gameObject;

            IGeneratedGeometrySource source =
                FindGeneratedGeometrySource(
                    searchObject.GetComponents<MonoBehaviour>());
            if (source != null)
            {
                return source;
            }

            source = FindGeneratedGeometrySource(
                searchObject.GetComponentsInParent<MonoBehaviour>(true));
            if (source != null)
            {
                return source;
            }

            return FindGeneratedGeometrySource(
                searchObject.GetComponentsInChildren<MonoBehaviour>(true));
        }

        private static IGeneratedGeometrySource FindGeneratedGeometrySource(
            MonoBehaviour[] behaviours)
        {
            for (int index = 0; index < behaviours.Length; index++)
            {
                if (behaviours[index] is IGeneratedGeometrySource source)
                {
                    return source;
                }
            }

            return null;
        }

        private void ClearGeneratedGeometrySubscription()
        {
            if (generatedGeometrySource != null)
            {
                generatedGeometrySource.GeometryChanged -=
                    HandleGeneratedGeometryChanged;
            }

            generatedGeometrySource = null;
            generatedGeometrySourceBehaviour = null;
        }

        private void HandleGeneratedGeometryChanged()
        {
            automaticFootprintDirty = true;
            staticRegistrationDirty = true;
        }

        private static void AddTrianglePlaneIntersections(
            Vector3 a,
            Vector3 b,
            Vector3 c,
            Vector3 planePoint,
            Vector3 planeNormal,
            List<Vector3> target)
        {
            float distanceA = Vector3.Dot(a - planePoint, planeNormal);
            float distanceB = Vector3.Dot(b - planePoint, planeNormal);
            float distanceC = Vector3.Dot(c - planePoint, planeNormal);

            AddEdgePlaneIntersection(
                a, b, distanceA, distanceB, target);
            AddEdgePlaneIntersection(
                b, c, distanceB, distanceC, target);
            AddEdgePlaneIntersection(
                c, a, distanceC, distanceA, target);
        }

        private static void AddEdgePlaneIntersection(
            Vector3 start,
            Vector3 end,
            float startDistance,
            float endDistance,
            List<Vector3> target)
        {
            bool startOnPlane =
                Mathf.Abs(startDistance) <=
                WaterlineIntersectionEpsilon;
            bool endOnPlane =
                Mathf.Abs(endDistance) <=
                WaterlineIntersectionEpsilon;

            if (startOnPlane)
            {
                target.Add(start);
            }

            if (endOnPlane)
            {
                target.Add(end);
            }

            bool crossesPlane =
                (startDistance < -WaterlineIntersectionEpsilon &&
                 endDistance > WaterlineIntersectionEpsilon) ||
                (startDistance > WaterlineIntersectionEpsilon &&
                 endDistance < -WaterlineIntersectionEpsilon);

            if (!crossesPlane)
            {
                return;
            }

            float denominator = startDistance - endDistance;
            if (Mathf.Abs(denominator) <= 0.000001f)
            {
                return;
            }

            float interpolation = startDistance / denominator;
            target.Add(Vector3.LerpUnclamped(
                start,
                end,
                interpolation));
        }

        private void AddBoundsPlaneIntersections(
            MeshFilter meshFilter,
            Bounds localBounds,
            Vector3 planePoint,
            Vector3 planeNormal,
            List<Vector3> target)
        {
            Vector3 minimum = localBounds.min;
            Vector3 maximum = localBounds.max;

            boundsCorners[0] = new Vector3(minimum.x, minimum.y, minimum.z);
            boundsCorners[1] = new Vector3(maximum.x, minimum.y, minimum.z);
            boundsCorners[2] = new Vector3(minimum.x, maximum.y, minimum.z);
            boundsCorners[3] = new Vector3(maximum.x, maximum.y, minimum.z);
            boundsCorners[4] = new Vector3(minimum.x, minimum.y, maximum.z);
            boundsCorners[5] = new Vector3(maximum.x, minimum.y, maximum.z);
            boundsCorners[6] = new Vector3(minimum.x, maximum.y, maximum.z);
            boundsCorners[7] = new Vector3(maximum.x, maximum.y, maximum.z);

            for (int index = 0; index < boundsCorners.Length; index++)
            {
                boundsCorners[index] =
                    meshFilter.transform.TransformPoint(
                        boundsCorners[index]);
            }

            for (int edge = 0;
                 edge < BoundsEdges.GetLength(0);
                 edge++)
            {
                Vector3 start = boundsCorners[BoundsEdges[edge, 0]];
                Vector3 end = boundsCorners[BoundsEdges[edge, 1]];
                float startDistance =
                    Vector3.Dot(start - planePoint, planeNormal);
                float endDistance =
                    Vector3.Dot(end - planePoint, planeNormal);
                AddEdgePlaneIntersection(
                    start,
                    end,
                    startDistance,
                    endDistance,
                    target);
            }
        }

        private static bool TryGetWorldBounds(
            MeshFilter meshFilter,
            out Bounds worldBounds)
        {
            worldBounds = default;
            if (meshFilter == null || meshFilter.sharedMesh == null)
            {
                return false;
            }

            MeshRenderer renderer =
                meshFilter.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                worldBounds = renderer.bounds;
                return true;
            }

            Bounds localBounds = meshFilter.sharedMesh.bounds;
            Vector3 centre =
                meshFilter.transform.TransformPoint(localBounds.center);
            Vector3 extents = localBounds.extents;
            Vector3 axisX =
                meshFilter.transform.TransformVector(
                    new Vector3(extents.x, 0f, 0f));
            Vector3 axisY =
                meshFilter.transform.TransformVector(
                    new Vector3(0f, extents.y, 0f));
            Vector3 axisZ =
                meshFilter.transform.TransformVector(
                    new Vector3(0f, 0f, extents.z));
            extents = new Vector3(
                Mathf.Abs(axisX.x) +
                Mathf.Abs(axisY.x) +
                Mathf.Abs(axisZ.x),
                Mathf.Abs(axisX.y) +
                Mathf.Abs(axisY.y) +
                Mathf.Abs(axisZ.y),
                Mathf.Abs(axisX.z) +
                Mathf.Abs(axisY.z) +
                Mathf.Abs(axisZ.z));
            worldBounds = new Bounds(centre, extents * 2f);
            return true;
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 centre =
                UsesAutomaticFootprint && hasResolvedAutomaticFootprint
                    ? transform.TransformPoint(
                        resolvedAutomaticLocalPosition)
                    : transform.position;
            Gizmos.DrawWireSphere(
                centre,
                Mathf.Max(
                    ResolvedAcrossHalfWidth,
                    ResolvedAlongHalfLength));
        }
    }
}
