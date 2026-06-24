using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ProgrammaticStylized3D.Rivers
{
    public enum StylizedRiverReflectionUpdateMode
    {
        EveryFrame,
        EveryNthFrame,
        OnDemand
    }

    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(StylizedRiver))]
    public sealed class StylizedRiverPlanarReflection : MonoBehaviour
    {
        [Header("Reflection")]
        [SerializeField] private bool reflectionsEnabled = true;
        [SerializeField] private Camera sourceCameraOverride;

        [Range(0.1f, 1f)]
        [SerializeField] private float resolutionScale = 0.5f;

        [SerializeField] private LayerMask reflectionMask = ~0;
        [SerializeField] private bool renderSkybox = true;
        [SerializeField] private bool renderShadows = true;
        [SerializeField] private bool includeSceneView = true;

        [Tooltip("Offsets the oblique clipping plane upward to avoid reflection seams.")]
        [Range(0.001f, 0.5f)]
        [SerializeField] private float clipPlaneOffset = 0.07f;

        [Min(0f)]
        [SerializeField] private float maxRenderDistance = 150f;

        [Header("Appearance")]
        [Range(0f, 1f)]
        [SerializeField] private float reflectionStrength = 0.45f;


        [Range(0f, 0.1f)]
        [SerializeField] private float reflectionDistortion = 0.015f;

        [Header("Update Cost")]
        [SerializeField]
        private StylizedRiverReflectionUpdateMode updateMode =
            StylizedRiverReflectionUpdateMode.EveryNthFrame;

        [Range(1, 30)]
        [SerializeField] private int updateEveryNFrames = 2;

        private StylizedRiver river;
        private Camera reflectionCamera;
        private UniversalAdditionalCameraData reflectionCameraData;
        private RenderTexture reflectionTexture;
        private int allocatedWidth;
        private int allocatedHeight;
        private int updateCounter;
        private bool renderRequested = true;
        private bool unsupportedRequestWarningReported;
        private bool missingSourceWarningReported;
        private static bool isSubmittingReflection;

        public bool ReflectionsEnabled => reflectionsEnabled;
        public RenderTexture ReflectionTexture => reflectionTexture;
        public Camera ReflectionCamera => reflectionCamera;
        public bool HasRenderedTexture => reflectionTexture != null && reflectionTexture.IsCreated();

        private void Reset()
        {
            river = GetComponent<StylizedRiver>();
            RequestRender();
        }

        private void OnEnable()
        {
            river = GetComponent<StylizedRiver>();
            renderRequested = true;
            PushDisabledStateIfNeeded();
        }

        private void OnDisable()
        {
            ClearRiverBinding();
            ReleaseResources();
        }

        private void OnValidate()
        {
            resolutionScale = Mathf.Clamp(resolutionScale, 0.1f, 1f);
            clipPlaneOffset = Mathf.Clamp(clipPlaneOffset, 0.001f, 0.5f);
            maxRenderDistance = Mathf.Max(0f, maxRenderDistance);
            reflectionStrength = Mathf.Clamp01(reflectionStrength);
            reflectionDistortion = Mathf.Clamp(reflectionDistortion, 0f, 0.1f);
            updateEveryNFrames = Mathf.Clamp(updateEveryNFrames, 1, 30);
            renderRequested = true;
            PushDisabledStateIfNeeded();
        }

        private void Update()
        {
            if (!isActiveAndEnabled || !reflectionsEnabled || isSubmittingReflection)
            {
                PushDisabledStateIfNeeded();
                return;
            }

            Camera sourceCamera = ResolveSourceCamera();

            if (sourceCamera == null)
            {
                if (!missingSourceWarningReported && Application.isPlaying)
                {
                    Debug.LogWarning(
                        $"StylizedRiverPlanarReflection on '{name}' has no source camera. Assign an override or tag the gameplay camera MainCamera.",
                        this);
                    missingSourceWarningReported = true;
                }

                return;
            }

            missingSourceWarningReported = false;

            if (!ShouldRenderThisUpdate())
            {
                return;
            }

            if (!IsWithinRenderDistance(sourceCamera))
            {
                return;
            }

            RenderReflection(sourceCamera);
        }

        [ContextMenu("Render Reflection Now")]
        public void RequestRender()
        {
            renderRequested = true;
        }

        [ContextMenu("Release Reflection Texture")]
        public void ReleaseReflectionTexture()
        {
            ClearRiverBinding();
            ReleaseResources();
            renderRequested = true;
        }

        private bool ShouldRenderThisUpdate()
        {
            updateCounter++;

            switch (updateMode)
            {
                case StylizedRiverReflectionUpdateMode.EveryFrame:
                    return true;

                case StylizedRiverReflectionUpdateMode.EveryNthFrame:
                    return renderRequested ||
                           updateCounter % Mathf.Max(1, updateEveryNFrames) == 0;

                case StylizedRiverReflectionUpdateMode.OnDemand:
                    return renderRequested;

                default:
                    return false;
            }
        }

        private Camera ResolveSourceCamera()
        {
            if (sourceCameraOverride != null)
            {
                return sourceCameraOverride;
            }

#if UNITY_EDITOR
            if (!Application.isPlaying && includeSceneView)
            {
                SceneView sceneView = SceneView.lastActiveSceneView;

                if (sceneView != null && sceneView.camera != null)
                {
                    return sceneView.camera;
                }
            }
#endif

            return Camera.main;
        }

        private bool IsWithinRenderDistance(Camera sourceCamera)
        {
            if (maxRenderDistance <= 0f || river == null)
            {
                return true;
            }

            if (!river.TryGetSurfaceBounds(out Bounds surfaceBounds))
            {
                return true;
            }

            float distance =
                Vector3.Distance(
                    sourceCamera.transform.position,
                    surfaceBounds.ClosestPoint(sourceCamera.transform.position));

            return distance <= maxRenderDistance;
        }

        private void RenderReflection(Camera sourceCamera)
        {
            if (river == null)
            {
                river = GetComponent<StylizedRiver>();
            }

            if (river == null || sourceCamera == null)
            {
                return;
            }

            EnsureReflectionCamera();
            EnsureReflectionTexture(sourceCamera);

            if (reflectionCamera == null || reflectionTexture == null)
            {
                return;
            }

            ConfigureReflectionCamera(sourceCamera);

            UniversalRenderPipeline.SingleCameraRequest request =
                new UniversalRenderPipeline.SingleCameraRequest
                {
                    destination = reflectionTexture
                };

            if (!RenderPipeline.SupportsRenderRequest(reflectionCamera, request))
            {
                if (!unsupportedRequestWarningReported)
                {
                    Debug.LogWarning(
                        $"The active render pipeline does not support URP SingleCameraRequest for river reflection '{name}'.",
                        this);
                    unsupportedRequestWarningReported = true;
                }

                ClearRiverBinding();
                return;
            }

            unsupportedRequestWarningReported = false;
            bool previousInvertCulling = GL.invertCulling;

            try
            {
                isSubmittingReflection = true;
                GL.invertCulling = !previousInvertCulling;
                RenderPipeline.SubmitRenderRequest(reflectionCamera, request);
            }
            finally
            {
                GL.invertCulling = previousInvertCulling;
                isSubmittingReflection = false;
            }

            Matrix4x4 gpuProjection =
                GL.GetGPUProjectionMatrix(
                    reflectionCamera.projectionMatrix,
                    true);

            Matrix4x4 viewProjection =
                gpuProjection * reflectionCamera.worldToCameraMatrix;

            river.SetPlanarReflectionData(
                reflectionTexture,
                viewProjection,
                reflectionStrength,
                reflectionDistortion,
                true);

            renderRequested = false;
        }

        private void EnsureReflectionCamera()
        {
            if (reflectionCamera != null)
            {
                return;
            }

            GameObject cameraObject =
                new GameObject("__PS3D_RiverPlanarReflectionCamera")
                {
                    hideFlags = HideFlags.HideAndDontSave
                };

            reflectionCamera = cameraObject.AddComponent<Camera>();
            reflectionCamera.enabled = false;
            reflectionCameraData =
                cameraObject.AddComponent<UniversalAdditionalCameraData>();
            reflectionCameraData.renderType = CameraRenderType.Base;
            reflectionCameraData.renderPostProcessing = false;
        }

        private void EnsureReflectionTexture(Camera sourceCamera)
        {
            int width =
                Mathf.Max(
                    64,
                    Mathf.RoundToInt(sourceCamera.pixelWidth * resolutionScale));

            int height =
                Mathf.Max(
                    64,
                    Mathf.RoundToInt(sourceCamera.pixelHeight * resolutionScale));

            if (reflectionTexture != null &&
                allocatedWidth == width &&
                allocatedHeight == height &&
                reflectionTexture.IsCreated())
            {
                return;
            }

            ReleaseReflectionTextureOnly();

            RenderTextureFormat format =
                sourceCamera.allowHDR
                    ? RenderTextureFormat.DefaultHDR
                    : RenderTextureFormat.Default;

            reflectionTexture =
                new RenderTexture(
                    width,
                    height,
                    24,
                    format,
                    RenderTextureReadWrite.Default)
                {
                    name = $"RT_PS3D_RiverReflection_{GetEntityId()}",
                    hideFlags = HideFlags.HideAndDontSave,
                    useMipMap = false,
                    autoGenerateMips = false,
                    antiAliasing = 1,
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp
                };

            reflectionTexture.Create();
            allocatedWidth = width;
            allocatedHeight = height;
        }

        private void ConfigureReflectionCamera(Camera sourceCamera)
        {
            reflectionCamera.CopyFrom(sourceCamera);
            reflectionCamera.enabled = false;
            reflectionCamera.targetTexture = null;
            reflectionCamera.allowMSAA = false;
            reflectionCamera.useOcclusionCulling = sourceCamera.useOcclusionCulling;
            reflectionCamera.cullingMask = ResolveReflectionMask();

            if (!renderSkybox)
            {
                reflectionCamera.clearFlags = CameraClearFlags.SolidColor;
                reflectionCamera.backgroundColor = Color.clear;
            }

            if (reflectionCameraData == null)
            {
                reflectionCameraData =
                    reflectionCamera.GetUniversalAdditionalCameraData();
            }

            reflectionCameraData.renderType = CameraRenderType.Base;
            reflectionCameraData.renderPostProcessing = false;
            reflectionCameraData.renderShadows = renderShadows;

            float planeHeight = river.AverageSurfaceHeight;
            Vector3 planePosition = new Vector3(0f, planeHeight, 0f);
            Vector3 planeNormal = Vector3.up;
            Vector4 reflectionPlane =
                new Vector4(
                    planeNormal.x,
                    planeNormal.y,
                    planeNormal.z,
                    -Vector3.Dot(planeNormal, planePosition));

            Matrix4x4 reflectionMatrix =
                CalculateReflectionMatrix(reflectionPlane);

            Vector3 reflectedPosition =
                reflectionMatrix.MultiplyPoint(sourceCamera.transform.position);

            Vector3 reflectedForward =
                reflectionMatrix.MultiplyVector(sourceCamera.transform.forward);

            Vector3 reflectedUp =
                reflectionMatrix.MultiplyVector(sourceCamera.transform.up);

            reflectionCamera.transform.SetPositionAndRotation(
                reflectedPosition,
                Quaternion.LookRotation(reflectedForward, reflectedUp));

            reflectionCamera.worldToCameraMatrix =
                sourceCamera.worldToCameraMatrix * reflectionMatrix;

            Vector4 clipPlane =
                CameraSpacePlane(
                    reflectionCamera,
                    planePosition,
                    planeNormal,
                    1f);

            reflectionCamera.projectionMatrix = sourceCamera.projectionMatrix;
            reflectionCamera.projectionMatrix =
                reflectionCamera.CalculateObliqueMatrix(clipPlane);
        }

        private int ResolveReflectionMask()
        {
            int mask = reflectionMask.value;
            int waterLayer = LayerMask.NameToLayer("Water");

            if (waterLayer >= 0)
            {
                mask &= ~(1 << waterLayer);
            }

            return mask;
        }

        private Vector4 CameraSpacePlane(
            Camera camera,
            Vector3 position,
            Vector3 normal,
            float sideSign)
        {
            Vector3 offsetPosition = position + normal * clipPlaneOffset;
            Matrix4x4 worldToCamera = camera.worldToCameraMatrix;
            Vector3 cameraPosition = worldToCamera.MultiplyPoint(offsetPosition);
            Vector3 cameraNormal =
                worldToCamera.MultiplyVector(normal).normalized * sideSign;

            return new Vector4(
                cameraNormal.x,
                cameraNormal.y,
                cameraNormal.z,
                -Vector3.Dot(cameraPosition, cameraNormal));
        }

        private static Matrix4x4 CalculateReflectionMatrix(Vector4 plane)
        {
            Matrix4x4 matrix = Matrix4x4.identity;

            matrix.m00 = 1f - 2f * plane.x * plane.x;
            matrix.m01 = -2f * plane.x * plane.y;
            matrix.m02 = -2f * plane.x * plane.z;
            matrix.m03 = -2f * plane.w * plane.x;

            matrix.m10 = -2f * plane.y * plane.x;
            matrix.m11 = 1f - 2f * plane.y * plane.y;
            matrix.m12 = -2f * plane.y * plane.z;
            matrix.m13 = -2f * plane.w * plane.y;

            matrix.m20 = -2f * plane.z * plane.x;
            matrix.m21 = -2f * plane.z * plane.y;
            matrix.m22 = 1f - 2f * plane.z * plane.z;
            matrix.m23 = -2f * plane.w * plane.z;

            return matrix;
        }

        private void PushDisabledStateIfNeeded()
        {
            if (reflectionsEnabled)
            {
                return;
            }

            ClearRiverBinding();
        }

        private void ClearRiverBinding()
        {
            if (river == null)
            {
                river = GetComponent<StylizedRiver>();
            }

            if (river != null)
            {
                river.ClearPlanarReflectionData();
            }
        }

        private void ReleaseResources()
        {
            ReleaseReflectionTextureOnly();

            if (reflectionCamera != null)
            {
                GameObject cameraObject = reflectionCamera.gameObject;
                reflectionCamera = null;
                reflectionCameraData = null;

                if (Application.isPlaying)
                {
                    Destroy(cameraObject);
                }
                else
                {
                    DestroyImmediate(cameraObject);
                }
            }
        }

        private void ReleaseReflectionTextureOnly()
        {
            if (reflectionTexture == null)
            {
                return;
            }

            if (reflectionTexture.IsCreated())
            {
                reflectionTexture.Release();
            }

            if (Application.isPlaying)
            {
                Destroy(reflectionTexture);
            }
            else
            {
                DestroyImmediate(reflectionTexture);
            }

            reflectionTexture = null;
            allocatedWidth = 0;
            allocatedHeight = 0;
        }

        private void OnDestroy()
        {
            ClearRiverBinding();
            ReleaseResources();
        }
    }
}
