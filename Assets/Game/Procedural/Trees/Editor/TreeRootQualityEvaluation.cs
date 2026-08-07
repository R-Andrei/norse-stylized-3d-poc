using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace ProgrammaticStylized3D.Trees.Editor
{
    internal static class TreeRootQualityEvaluation
    {
        private const string OutputDirectory =
            "Library/PS3D/Trees/RootContactRadialEvaluation";
        private const string CaptureDirectoryName = "Captures";
        private const string ReportFileName =
            "TreeRootContactRadialEvaluationReport.md";
        private const string CsvFileName =
            "TreeRootContactRadialEvaluation.csv";
        private const string BoardFileName =
            "TreeRootContactRadialEvaluationBoard.html";
        private const int CloseCaptureWidth = 640;
        private const int CloseCaptureHeight = 640;
        private const int GameCaptureHeight = 432;
        private const int MaximumGameCaptureWidth = 1024;

        private enum EvaluationStage
        {
            Build = 0,
            BeginCloseCapture = 1,
            WaitCloseCapture = 2,
            BeginGameCapture = 3,
            WaitGameCapture = 4,
            CompleteCase = 5
        }

        private enum CaptureKind
        {
            CloseRoot = 0,
            GameContext = 1
        }

        private sealed class Representative
        {
            internal string Name;
            internal TreeFamily Family;
            internal int Seed;
            internal string SourceIdentity;
            internal TreeResolvedControls Controls;
            internal Vector3 Position;
            internal Quaternion Rotation;
            internal Vector3 Scale;
        }

        private sealed class EvaluationCase
        {
            internal int Index;
            internal Representative Representative;
            internal string Label;
            internal string Slug;
            internal float? AxialTwist;
            internal float? RootThickness;
            internal float? RootReach;
            internal float? RootHeight;
            internal float? ButtressPersistence;
        }

        private sealed class CaseResult
        {
            internal EvaluationCase Case;
            internal bool GenerationPassed;
            internal bool BarkPassed;
            internal bool TopologyPassed;
            internal string Failure = string.Empty;
            internal TreeDefinition Definition;
            internal TreeBarkMeshBuildResult Bark;
            internal Mesh Mesh;
            internal string CloseCapturePath = string.Empty;
            internal string CloseCaptureFailure = string.Empty;
            internal string GameCapturePath = string.Empty;
            internal string GameCaptureFailure = string.Empty;
            internal double BuildMilliseconds;

            internal bool Passed =>
                GenerationPassed &&
                BarkPassed &&
                TopologyPassed &&
                !string.IsNullOrEmpty(CloseCapturePath) &&
                !string.IsNullOrEmpty(GameCapturePath);
        }

        private sealed class PendingCapture
        {
            internal CaseResult Result;
            internal CaptureKind Kind;
            internal RenderTexture RenderTexture;
            internal AsyncGPUReadbackRequest Request;
            internal AsyncGPUReadbackRequest CompletedRequest;
            internal bool CallbackCompleted;
            internal bool Abandoned;
            internal bool Released;
            internal string Path;
            internal int Width;
            internal int Height;
        }

        private sealed class Job
        {
            internal List<Representative> Representatives;
            internal List<EvaluationCase> Cases;
            internal readonly List<CaseResult> Results =
                new List<CaseResult>();
            internal int CaseIndex;
            internal EvaluationStage Stage;
            internal CaseResult CurrentResult;
            internal PendingCapture PendingCapture;
            internal DateTime StartedUtc;
            internal bool CancelRequested;
            internal string ReportPath;
            internal string CsvPath;
            internal string BoardPath;
            internal string CaptureDirectory;
            internal StreamWriter CsvWriter;
            internal Quaternion GameCameraRotation;
            internal Matrix4x4 GameProjectionMatrix;
            internal bool GameCameraOrthographic;
            internal float GameNearClipPlane;
            internal float GameFarClipPlane;
            internal float GameReferenceDistance;
            internal int GameCaptureWidth;
            internal string GameCameraDescription;
        }

        private static Job activeJob;
        private static string lastReportPath = string.Empty;
        private static string lastBoardPath = string.Empty;
        private static string currentDetail = "Not running";
        private static string currentEta = string.Empty;
        private static float currentProgress;

        internal static bool IsRunning => activeJob != null;
        internal static string LastReportPath => lastReportPath;
        internal static string LastBoardPath => lastBoardPath;
        internal static string CurrentDetail => currentDetail;
        internal static string CurrentEta => currentEta;
        internal static float CurrentProgress => currentProgress;

        internal static string ProgressLabel
        {
            get
            {
                Job job = activeJob;
                if (job == null)
                {
                    return "Not running";
                }

                return Mathf.Min(job.CaseIndex, job.Cases.Count) +
                    " / " + job.Cases.Count;
            }
        }

        internal static bool Start(ProceduralTreeInstance selected)
        {
            if (activeJob != null)
            {
                return false;
            }

            List<Representative> representatives =
                CollectRepresentatives();
            if (representatives.Count != 8)
            {
                Debug.LogError(
                    "[TREE-ROOTS.4C] The ground-contact radial evaluation requires Twisted 1-5 plus Common 1, Pine 1, and Dead 1 exact-control gallery representatives. Found " +
                    representatives.Count + ". Rebuild the curated recipe gallery first.");
                return false;
            }

            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError(
                    "[TREE-ROOTS.4C] No enabled MainCamera-tagged Camera was found. The evaluation requires the current game-camera projection for its context captures.");
                return false;
            }

            Directory.CreateDirectory(OutputDirectory);
            string captureDirectory = Path.Combine(
                OutputDirectory,
                CaptureDirectoryName);
            Directory.CreateDirectory(captureDirectory);
            DeleteExistingCaptures(captureDirectory);

            string reportPath = Path.Combine(
                OutputDirectory,
                ReportFileName);
            string csvPath = Path.Combine(OutputDirectory, CsvFileName);
            string boardPath = Path.Combine(OutputDirectory, BoardFileName);
            var csvWriter = new StreamWriter(
                csvPath,
                false,
                Encoding.UTF8);
            csvWriter.WriteLine(
                "CaseIndex,Representative,Family,Case,Status,Mode," +
                "AxialTwist,RequestedRootThickness,EvaluatedRootThickness,RootReach,RootHeight,ButtressPersistence," +
                "RequestedTwist,MeasuredTwist,TwistError,FirstTwistDistance," +
                "TwistAtGroundPlateau,TwistAtRootCollapse," +
                "TwistAtEarliestTransition,TwistAtEffectiveTransition," +
                "MaximumTwistStep,AllowedTwistStep," +
                "MaximumTwistStepStart,MaximumTwistStepEnd," +
                "Vertices,Triangles,RootIntervals,RootCrestMultiplier," +
                "RequestedSupportDegrees,EmittedSupportDegrees,SupportClamped," +
                "GroundBaseMergeFactor,FootShapePlateauEnd," +
                "RootHalfWidthDegrees,RootHalfChordWidth," +
                "EffectiveTransitionHeight,TrunkRadialMin,TrunkRadialMax," +
                "GroundContactRadialSegments,GroundContactBoostedRings,GroundContactBoostReleaseDistance," +
                "BuildMilliseconds,CloseCapture,GameCapture,Failure");
            csvWriter.Flush();

            float gameAspect = Mathf.Max(0.1f, mainCamera.aspect);
            int gameCaptureWidth = Mathf.Clamp(
                Mathf.RoundToInt(GameCaptureHeight * gameAspect),
                1,
                MaximumGameCaptureWidth);
            float referenceDistance = Mathf.Abs(Vector3.Dot(
                representatives[0].Position - mainCamera.transform.position,
                mainCamera.transform.forward));
            referenceDistance = Mathf.Max(0.1f, referenceDistance);

            activeJob = new Job
            {
                Representatives = representatives,
                Cases = BuildCases(representatives),
                Stage = EvaluationStage.Build,
                StartedUtc = DateTime.UtcNow,
                ReportPath = reportPath,
                CsvPath = csvPath,
                BoardPath = boardPath,
                CaptureDirectory = captureDirectory,
                CsvWriter = csvWriter,
                GameCameraRotation = mainCamera.transform.rotation,
                GameProjectionMatrix = mainCamera.projectionMatrix,
                GameCameraOrthographic = mainCamera.orthographic,
                GameNearClipPlane = mainCamera.nearClipPlane,
                GameFarClipPlane = mainCamera.farClipPlane,
                GameReferenceDistance = referenceDistance,
                GameCaptureWidth = gameCaptureWidth,
                GameCameraDescription = mainCamera.name +
                    " exact projection; " + gameCaptureWidth + "×" +
                    GameCaptureHeight + "; reference distance=" +
                    referenceDistance.ToString(
                        "F3",
                        CultureInfo.InvariantCulture)
            };
            lastReportPath = reportPath;
            lastBoardPath = boardPath;
            currentProgress = 0f;
            currentDetail = "Preparing first ground-contact radial evaluation case";
            currentEta = "ETA calculating";
            WriteCheckpoint(activeJob, "RUNNING", null);
            EditorApplication.update += Tick;
            AssemblyReloadEvents.beforeAssemblyReload += AbortForReload;
            EditorApplication.quitting += AbortForQuit;
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            Debug.Log(
                "[TREE-ROOTS.4C] Incremental 8-case ground-contact radial evaluation started. Output: " +
                boardPath);
            return true;
        }

        internal static void RequestCancel()
        {
            if (activeJob != null)
            {
                activeJob.CancelRequested = true;
            }
        }

        internal static void OpenOutputFolder()
        {
            Directory.CreateDirectory(OutputDirectory);
            EditorUtility.RevealInFinder(Path.GetFullPath(OutputDirectory));
        }

        internal static void OpenBoard()
        {
            if (!string.IsNullOrEmpty(lastBoardPath) &&
                File.Exists(lastBoardPath))
            {
                Application.OpenURL(
                    new Uri(Path.GetFullPath(lastBoardPath)).AbsoluteUri);
            }
        }

        internal static void CopyLastReport()
        {
            if (!string.IsNullOrEmpty(lastReportPath) &&
                File.Exists(lastReportPath))
            {
                EditorGUIUtility.systemCopyBuffer =
                    File.ReadAllText(lastReportPath);
            }
        }

        private static void AbortForReload()
        {
            if (activeJob != null)
            {
                Finish(
                    activeJob,
                    "CANCELLED",
                    "Assembly reload interrupted the evaluation after partial captures and reports were preserved.");
            }
        }

        private static void AbortForQuit()
        {
            if (activeJob != null)
            {
                Finish(
                    activeJob,
                    "CANCELLED",
                    "Editor shutdown interrupted the evaluation after partial captures and reports were preserved.");
            }
        }

        private static void Tick()
        {
            Job job = activeJob;
            if (job == null)
            {
                return;
            }

            try
            {
                if (job.PendingCapture != null)
                {
                    UpdateProgress(job);
                    if (!IsCaptureComplete(job.PendingCapture))
                    {
                        return;
                    }

                    CompletePendingCapture(job);
                    return;
                }

                if (job.CancelRequested)
                {
                    Finish(job, "CANCELLED", null);
                    return;
                }

                if (job.CaseIndex >= job.Cases.Count)
                {
                    Finish(job, "COMPLETE", null);
                    return;
                }

                UpdateProgress(job);
                switch (job.Stage)
                {
                    case EvaluationStage.Build:
                        BuildCurrentCase(job);
                        break;
                    case EvaluationStage.BeginCloseCapture:
                        BeginCurrentCapture(job, CaptureKind.CloseRoot);
                        break;
                    case EvaluationStage.BeginGameCapture:
                        BeginCurrentCapture(job, CaptureKind.GameContext);
                        break;
                    case EvaluationStage.CompleteCase:
                        CompleteCurrentCase(job);
                        break;
                }
            }
            catch (Exception exception)
            {
                Finish(activeJob, "FAILED", exception.ToString());
            }
        }

        private static void UpdateProgress(Job job)
        {
            float stageFraction = GetStageFraction(job.Stage);
            currentProgress = Mathf.Clamp01(
                (job.CaseIndex + stageFraction) /
                Mathf.Max(1f, job.Cases.Count));
            EvaluationCase evaluationCase =
                job.Cases[Mathf.Min(job.CaseIndex, job.Cases.Count - 1)];
            currentDetail = evaluationCase.Representative.Name + " — " +
                evaluationCase.Label + " — " + StageLabel(job.Stage);
            TimeSpan elapsed = DateTime.UtcNow - job.StartedUtc;
            double secondsPerCase = job.CaseIndex > 0
                ? elapsed.TotalSeconds / job.CaseIndex
                : 0.0;
            double eta = secondsPerCase *
                Mathf.Max(0, job.Cases.Count - job.CaseIndex);
            currentEta = "Elapsed " + FormatDuration(elapsed.TotalSeconds) +
                " | ETA " + FormatDuration(eta);
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }

        private static float GetStageFraction(EvaluationStage stage)
        {
            switch (stage)
            {
                case EvaluationStage.Build:
                    return 0f;
                case EvaluationStage.BeginCloseCapture:
                case EvaluationStage.WaitCloseCapture:
                    return 0.34f;
                case EvaluationStage.BeginGameCapture:
                case EvaluationStage.WaitGameCapture:
                    return 0.67f;
                default:
                    return 0.95f;
            }
        }

        private static string StageLabel(EvaluationStage stage)
        {
            switch (stage)
            {
                case EvaluationStage.Build:
                    return "building temporary production bark";
                case EvaluationStage.BeginCloseCapture:
                case EvaluationStage.WaitCloseCapture:
                    return "capturing close root view";
                case EvaluationStage.BeginGameCapture:
                case EvaluationStage.WaitGameCapture:
                    return "capturing game-camera context";
                default:
                    return "checkpointing result";
            }
        }

        private static void BuildCurrentCase(Job job)
        {
            EvaluationCase evaluationCase = job.Cases[job.CaseIndex];
            var result = new CaseResult
            {
                Case = evaluationCase
            };
            job.CurrentResult = result;

            TreeResolvedControls controls = CloneControls(
                evaluationCase.Representative.Controls);
            ApplyOverrides(controls, evaluationCase);
            controls.ValidateAndClamp();

            TreeGenerationResult generation =
                TreeGenerator.GenerateExactForValidation(
                    controls,
                    evaluationCase.Representative.Seed,
                    evaluationCase.Representative.SourceIdentity,
                    evaluationCase.Representative.Family);
            result.GenerationPassed = generation != null &&
                generation.Passed &&
                generation.Definition != null &&
                generation.Definition.IsValid;
            if (!result.GenerationPassed)
            {
                result.Failure = generation != null
                    ? FirstFailureLine(generation.Report)
                    : "Tree generation returned null.";
                job.Stage = EvaluationStage.CompleteCase;
                return;
            }

            result.Definition = generation.Definition;
            result.Mesh = new Mesh
            {
                name = "TREE-ROOTS.4C " +
                    evaluationCase.Representative.Name + " " +
                    evaluationCase.Label,
                hideFlags = HideFlags.HideAndDontSave
            };
            TreeBarkMeshSettings settings =
                TreeBarkMeshSettings.CreateRecipeOnlyDefaults();
            result.Bark = TreeBarkMeshGenerator.Build(
                result.Definition,
                settings,
                result.Mesh);
            result.BarkPassed = result.Bark != null && result.Bark.Passed;
            result.TopologyPassed = result.BarkPassed &&
                result.Bark.TopologyAudit != null &&
                result.Bark.TopologyAudit.Passed;
            result.BuildMilliseconds = result.Bark != null
                ? result.Bark.TotalBuildMilliseconds
                : 0.0;
            if (!result.BarkPassed || !result.TopologyPassed)
            {
                if (result.Bark == null)
                {
                    result.Failure = "Bark mesh generation returned null.";
                }
                else if (!result.BarkPassed)
                {
                    result.Failure = FirstFailureLine(result.Bark.Failure);
                }
                else if (result.Bark.TopologyAudit == null)
                {
                    result.Failure = "Bark topology audit returned null.";
                }
                else
                {
                    result.Failure = FirstFailureLine(
                        result.Bark.TopologyAudit.Report);
                }
                job.Stage = EvaluationStage.CompleteCase;
                return;
            }

            job.Stage = EvaluationStage.BeginCloseCapture;
        }

        private static void BeginCurrentCapture(Job job, CaptureKind kind)
        {
            CaseResult result = job.CurrentResult;
            bool started = BeginCapture(job, result, kind);
            if (started)
            {
                job.Stage = kind == CaptureKind.CloseRoot
                    ? EvaluationStage.WaitCloseCapture
                    : EvaluationStage.WaitGameCapture;
                return;
            }

            job.Stage = kind == CaptureKind.CloseRoot
                ? EvaluationStage.BeginGameCapture
                : EvaluationStage.CompleteCase;
        }

        private static bool BeginCapture(
            Job job,
            CaseResult result,
            CaptureKind kind)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find(
                    "Universal Render Pipeline/Simple Lit");
            }
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }
            if (shader == null)
            {
                shader = Shader.Find("Unlit/Color");
            }
            if (shader == null)
            {
                SetCaptureFailure(
                    result,
                    kind,
                    "No compatible Lit or Unlit capture shader was available.");
                return false;
            }

            Scene previewScene = default;
            bool previewSceneCreated = false;
            Material barkMaterial = null;
            Material groundMaterial = null;
            RenderTexture renderTexture = null;
            Camera camera = null;
            bool ownershipTransferred = false;
            try
            {
                previewScene = EditorSceneManager.NewPreviewScene();
                previewSceneCreated = true;

                var treeObject = new GameObject(
                    "TREE-ROOTS.4C Capture Tree")
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                SceneManager.MoveGameObjectToScene(treeObject, previewScene);
                MeshFilter filter = treeObject.AddComponent<MeshFilter>();
                MeshRenderer renderer = treeObject.AddComponent<MeshRenderer>();
                filter.sharedMesh = result.Mesh;
                barkMaterial = CreateCaptureMaterial(
                    shader,
                    new Color(0.48f, 0.31f, 0.17f, 1f),
                    0.16f);
                renderer.sharedMaterial = barkMaterial;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = true;
                treeObject.transform.SetPositionAndRotation(
                    Vector3.zero,
                    result.Case.Representative.Rotation);
                treeObject.transform.localScale =
                    result.Case.Representative.Scale;

                Bounds worldBounds = TransformBounds(
                    result.Mesh.bounds,
                    treeObject.transform.localToWorldMatrix);
                CreateGround(
                    previewScene,
                    shader,
                    worldBounds,
                    treeObject.transform.TransformPoint(Vector3.zero).y,
                    out groundMaterial);
                CreateCaptureLights(previewScene);

                var cameraObject = new GameObject(
                    "TREE-ROOTS.4C Capture Camera")
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                SceneManager.MoveGameObjectToScene(
                    cameraObject,
                    previewScene);
                camera = cameraObject.AddComponent<Camera>();
                camera.scene = previewScene;
                camera.enabled = false;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(
                    0.075f,
                    0.075f,
                    0.075f,
                    1f);
                camera.cullingMask = ~0;
                camera.allowHDR = false;
                camera.allowMSAA = false;

                int width = kind == CaptureKind.CloseRoot
                    ? CloseCaptureWidth
                    : job.GameCaptureWidth;
                int height = kind == CaptureKind.CloseRoot
                    ? CloseCaptureHeight
                    : GameCaptureHeight;
                camera.aspect = width / (float)Mathf.Max(1, height);
                if (kind == CaptureKind.CloseRoot)
                {
                    PositionCloseRootCamera(
                        camera,
                        result,
                        treeObject.transform.localToWorldMatrix);
                }
                else
                {
                    PositionGameCamera(camera, worldBounds, job);
                }
                renderTexture = new RenderTexture(
                    width,
                    height,
                    24,
                    RenderTextureFormat.ARGB32,
                    RenderTextureReadWrite.sRGB)
                {
                    name = "TREE-ROOTS.4C Async Capture",
                    hideFlags = HideFlags.HideAndDontSave,
                    antiAliasing = 1,
                    useMipMap = false,
                    autoGenerateMips = false
                };
                if (!renderTexture.Create())
                {
                    SetCaptureFailure(
                        result,
                        kind,
                        "The asynchronous capture RenderTexture could not be created.");
                    return false;
                }

                camera.targetTexture = renderTexture;
                camera.Render();
                camera.targetTexture = null;

                string suffix = kind == CaptureKind.CloseRoot
                    ? "CloseRoot"
                    : "GameContext";
                string fileName = result.Case.Index.ToString("D2") + "_" +
                    SanitizeFileName(result.Case.Representative.Name) + "_" +
                    result.Case.Slug + "_" + suffix + ".png";
                var pending = new PendingCapture
                {
                    Result = result,
                    Kind = kind,
                    RenderTexture = renderTexture,
                    Path = Path.Combine(job.CaptureDirectory, fileName),
                    Width = width,
                    Height = height
                };
                pending.Request = AsyncGPUReadback.Request(
                    renderTexture,
                    0,
                    TextureFormat.RGBA32,
                    completedRequest =>
                    {
                        pending.CompletedRequest = completedRequest;
                        pending.CallbackCompleted = true;
                        if (pending.Abandoned)
                        {
                            ReleasePendingCapture(pending);
                        }
                    });
                job.PendingCapture = pending;
                ownershipTransferred = true;
                return true;
            }
            catch (Exception exception)
            {
                SetCaptureFailure(
                    result,
                    kind,
                    exception.GetType().Name + ": " +
                    FirstFailureLine(exception.Message));
                return false;
            }
            finally
            {
                if (camera != null)
                {
                    camera.targetTexture = null;
                }
                if (!ownershipTransferred && renderTexture != null)
                {
                    renderTexture.Release();
                    UnityEngine.Object.DestroyImmediate(renderTexture);
                }
                if (barkMaterial != null)
                {
                    UnityEngine.Object.DestroyImmediate(barkMaterial);
                }
                if (groundMaterial != null)
                {
                    UnityEngine.Object.DestroyImmediate(groundMaterial);
                }
                if (previewSceneCreated && previewScene.IsValid())
                {
                    EditorSceneManager.ClosePreviewScene(previewScene);
                }
            }
        }

        private static Material CreateCaptureMaterial(
            Shader shader,
            Color color,
            float smoothness)
        {
            var material = new Material(shader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }
            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }
            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", smoothness);
            }
            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat("_Metallic", 0f);
            }
            return material;
        }

        private static void CreateGround(
            Scene previewScene,
            Shader shader,
            Bounds treeBounds,
            float groundHeight,
            out Material groundMaterial)
        {
            GameObject ground = GameObject.CreatePrimitive(
                PrimitiveType.Plane);
            ground.name = "TREE-ROOTS.4C Capture Ground";
            ground.hideFlags = HideFlags.HideAndDontSave;
            SceneManager.MoveGameObjectToScene(ground, previewScene);
            Collider collider = ground.GetComponent<Collider>();
            if (collider != null)
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }

            float span = Mathf.Max(
                2f,
                Mathf.Max(treeBounds.size.x, treeBounds.size.z) * 3f);
            ground.transform.position = new Vector3(
                treeBounds.center.x,
                groundHeight -
                    Mathf.Max(0.003f, treeBounds.size.y * 0.0005f),
                treeBounds.center.z);
            ground.transform.localScale = new Vector3(
                span / 10f,
                1f,
                span / 10f);
            groundMaterial = CreateCaptureMaterial(
                shader,
                new Color(0.18f, 0.20f, 0.18f, 1f),
                0.05f);
            MeshRenderer renderer = ground.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = groundMaterial;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = true;
        }

        private static void CreateCaptureLights(Scene previewScene)
        {
            CreateDirectionalLight(
                previewScene,
                "TREE-ROOTS.4C Key Light",
                Quaternion.Euler(48f, -35f, 0f),
                1.25f);
            CreateDirectionalLight(
                previewScene,
                "TREE-ROOTS.4C Fill Light",
                Quaternion.Euler(25f, 145f, 0f),
                0.48f);
        }

        private static void CreateDirectionalLight(
            Scene previewScene,
            string name,
            Quaternion rotation,
            float intensity)
        {
            var lightObject = new GameObject(name)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            SceneManager.MoveGameObjectToScene(lightObject, previewScene);
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = intensity;
            light.shadows = LightShadows.None;
            lightObject.transform.rotation = rotation;
        }

        private static void PositionCloseRootCamera(
            Camera camera,
            CaseResult result,
            Matrix4x4 localToWorld)
        {
            Bounds localRootBounds = CalculateRootLocalBounds(
                result.Mesh,
                result.Definition,
                result.Bark);
            Bounds rootBounds = TransformBounds(localRootBounds, localToWorld);
            Vector3 target = rootBounds.center +
                Vector3.up * rootBounds.extents.y * 0.05f;
            Vector3 cameraDirection = new Vector3(
                1.15f,
                0.62f,
                -1.15f).normalized;
            camera.orthographic = false;
            camera.fieldOfView = 32f;
            camera.nearClipPlane = 0.01f;
            camera.farClipPlane = 1000f;
            float radius = Mathf.Max(0.05f, rootBounds.extents.magnitude);
            float distance = radius /
                Mathf.Sin(camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            distance *= 1.18f;
            camera.transform.position = target + cameraDirection * distance;
            camera.transform.rotation = Quaternion.LookRotation(
                target - camera.transform.position,
                Vector3.up);
            camera.ResetProjectionMatrix();
        }

        private static Bounds CalculateRootLocalBounds(
            Mesh mesh,
            TreeDefinition definition,
            TreeBarkMeshBuildResult bark)
        {
            Bounds fallback = mesh.bounds;
            Vector3[] vertices = mesh.vertices;
            if (vertices == null || vertices.Length == 0)
            {
                return fallback;
            }

            float treeHeight = definition != null &&
                definition.ResolvedParameters != null
                    ? Mathf.Max(
                        0.01f,
                        definition.ResolvedParameters.Height)
                    : Mathf.Max(0.01f, fallback.size.y);
            float authoredRootHeight = bark != null
                ? bark.AuthoredRootHeightNormalized
                : 0.1f;
            float rootSpan = treeHeight * Mathf.Clamp(
                Mathf.Max(0.16f, authoredRootHeight * 1.35f),
                0.12f,
                0.38f);
            float maximumY = fallback.min.y + rootSpan;
            bool initialized = false;
            Bounds bounds = default;
            for (int index = 0; index < vertices.Length; index++)
            {
                Vector3 vertex = vertices[index];
                if (vertex.y > maximumY)
                {
                    continue;
                }

                if (!initialized)
                {
                    bounds = new Bounds(vertex, Vector3.zero);
                    initialized = true;
                }
                else
                {
                    bounds.Encapsulate(vertex);
                }
            }

            if (!initialized)
            {
                return fallback;
            }

            Vector3 size = bounds.size;
            size.x = Mathf.Max(0.05f, size.x * 1.28f);
            size.z = Mathf.Max(0.05f, size.z * 1.28f);
            size.y = Mathf.Max(0.05f, size.y * 1.20f);
            bounds.size = size;
            return bounds;
        }

        private static void PositionGameCamera(
            Camera camera,
            Bounds worldBounds,
            Job job)
        {
            camera.orthographic = job.GameCameraOrthographic;
            camera.nearClipPlane = job.GameNearClipPlane;
            camera.farClipPlane = job.GameFarClipPlane;
            camera.transform.rotation = job.GameCameraRotation;
            camera.projectionMatrix = job.GameProjectionMatrix;
            float boundsRadius = Mathf.Max(
                0.01f,
                worldBounds.extents.magnitude);
            float distance = job.GameCameraOrthographic
                ? Mathf.Max(10f, boundsRadius * 4f)
                : job.GameReferenceDistance;
            distance = Mathf.Clamp(
                distance,
                job.GameNearClipPlane + boundsRadius + 0.01f,
                Mathf.Max(
                    job.GameNearClipPlane + boundsRadius + 0.02f,
                    job.GameFarClipPlane - boundsRadius - 0.01f));
            camera.transform.position = worldBounds.center -
                camera.transform.forward * distance;
        }

        private static bool IsCaptureComplete(PendingCapture pending)
        {
            return pending != null &&
                (pending.CallbackCompleted || pending.Request.done);
        }

        private static void CompletePendingCapture(Job job)
        {
            PendingCapture pending = job.PendingCapture;
            job.PendingCapture = null;
            Texture2D texture = null;
            try
            {
                AsyncGPUReadbackRequest request = pending.CallbackCompleted
                    ? pending.CompletedRequest
                    : pending.Request;
                if (request.hasError)
                {
                    SetCaptureFailure(
                        pending.Result,
                        pending.Kind,
                        "AsyncGPUReadback reported a GPU capture error.");
                }
                else
                {
                    var data = request.GetData<Color32>();
                    int expectedPixels = pending.Width * pending.Height;
                    if (data.Length != expectedPixels)
                    {
                        SetCaptureFailure(
                            pending.Result,
                            pending.Kind,
                            "AsyncGPUReadback returned " + data.Length +
                            " pixels; expected " + expectedPixels + ".");
                    }
                    else
                    {
                        var pixels = new Color32[expectedPixels];
                        for (int index = 0; index < pixels.Length; index++)
                        {
                            pixels[index] = data[index];
                        }

                        texture = new Texture2D(
                            pending.Width,
                            pending.Height,
                            TextureFormat.RGBA32,
                            false,
                            false)
                        {
                            hideFlags = HideFlags.HideAndDontSave
                        };
                        texture.SetPixels32(pixels);
                        texture.Apply(false, false);
                        File.WriteAllBytes(
                            pending.Path,
                            texture.EncodeToPNG());
                        SetCapturePath(
                            pending.Result,
                            pending.Kind,
                            pending.Path);
                    }
                }
            }
            catch (Exception exception)
            {
                SetCaptureFailure(
                    pending.Result,
                    pending.Kind,
                    exception.GetType().Name + ": " +
                    FirstFailureLine(exception.Message));
            }
            finally
            {
                if (texture != null)
                {
                    UnityEngine.Object.DestroyImmediate(texture);
                }
                ReleasePendingCapture(pending);
            }

            job.Stage = pending.Kind == CaptureKind.CloseRoot
                ? EvaluationStage.BeginGameCapture
                : EvaluationStage.CompleteCase;
        }

        private static void SetCapturePath(
            CaseResult result,
            CaptureKind kind,
            string path)
        {
            if (kind == CaptureKind.CloseRoot)
            {
                result.CloseCapturePath = path;
                result.CloseCaptureFailure = string.Empty;
            }
            else
            {
                result.GameCapturePath = path;
                result.GameCaptureFailure = string.Empty;
            }
        }

        private static void SetCaptureFailure(
            CaseResult result,
            CaptureKind kind,
            string failure)
        {
            if (kind == CaptureKind.CloseRoot)
            {
                result.CloseCaptureFailure = failure ?? string.Empty;
                result.CloseCapturePath = string.Empty;
            }
            else
            {
                result.GameCaptureFailure = failure ?? string.Empty;
                result.GameCapturePath = string.Empty;
            }
        }

        private static void ReleasePendingCapture(PendingCapture pending)
        {
            if (pending == null || pending.Released)
            {
                return;
            }

            pending.Released = true;
            if (pending.RenderTexture != null)
            {
                pending.RenderTexture.Release();
                UnityEngine.Object.DestroyImmediate(pending.RenderTexture);
                pending.RenderTexture = null;
            }
        }

        private static void CompleteCurrentCase(Job job)
        {
            CaseResult result = job.CurrentResult;
            job.CurrentResult = null;
            if (result == null)
            {
                throw new InvalidOperationException(
                    "The current root evaluation result was missing.");
            }

            if (!result.Passed && string.IsNullOrEmpty(result.Failure))
            {
                var failures = new List<string>();
                if (!string.IsNullOrEmpty(result.CloseCaptureFailure))
                {
                    failures.Add("Close capture: " +
                        result.CloseCaptureFailure);
                }
                if (!string.IsNullOrEmpty(result.GameCaptureFailure))
                {
                    failures.Add("Game capture: " +
                        result.GameCaptureFailure);
                }
                result.Failure = string.Join(" | ", failures);
            }

            job.Results.Add(result);
            WriteCsvResult(job.CsvWriter, result);
            job.CsvWriter.Flush();
            if (result.Mesh != null)
            {
                UnityEngine.Object.DestroyImmediate(result.Mesh);
                result.Mesh = null;
            }

            job.CaseIndex++;
            job.Stage = EvaluationStage.Build;
            WriteCheckpoint(job, "RUNNING", null);
        }

        private static void WriteCsvResult(
            StreamWriter writer,
            CaseResult result)
        {
            TreeBarkMeshBuildResult bark = result.Bark;
            TreeBarkMeshBranchGeometryAccounting trunk =
                FindTrunkAccounting(bark);
            EvaluationCase evaluationCase = result.Case;
            TreeResolvedControls baseline =
                evaluationCase.Representative.Controls;
            writer.WriteLine(string.Join(",", new[]
            {
                evaluationCase.Index.ToString(CultureInfo.InvariantCulture),
                Csv(evaluationCase.Representative.Name),
                Csv(evaluationCase.Representative.Family.ToString()),
                Csv(evaluationCase.Label),
                result.Passed ? "PASS" : "FAIL",
                Csv(ModeLabel(evaluationCase)),
                F(evaluationCase.AxialTwist ?? baseline.AxialTwist),
                F(evaluationCase.RootThickness ?? baseline.RootThickness),
                bark != null
                    ? F(bark.EvaluatedRootThickness)
                    : F(Mathf.Min(
                        evaluationCase.RootThickness ?? baseline.RootThickness,
                        2f)),
                F(evaluationCase.RootReach ?? baseline.RootReach),
                F(evaluationCase.RootHeight ?? baseline.RootHeight),
                F(evaluationCase.ButtressPersistence ??
                    baseline.ButtressTransition),
                bark != null ? F(bark.RequestedAxialTwistDegrees) : "",
                bark != null ? F(bark.MeasuredAxialTwistDegrees) : "",
                bark != null ? F(bark.AxialTwistErrorDegrees) : "",
                bark != null
                    ? F(bark.FirstAuthoredAxialTwistNormalizedDistance)
                    : "",
                bark != null
                    ? F(bark.AxialTwistAtGroundPlateauEndDegrees)
                    : "",
                bark != null
                    ? F(bark.AxialTwistAtRootCollapseEndDegrees)
                    : "",
                bark != null
                    ? F(bark.AxialTwistAtEarliestRootTransitionDegrees)
                    : "",
                bark != null
                    ? F(bark.AxialTwistAtEffectiveRootTransitionDegrees)
                    : "",
                bark != null
                    ? F(bark.MaximumAuthoredAxialTwistStepDegrees)
                    : "",
                bark != null
                    ? F(bark.MaximumAllowedAxialTwistStepDegrees)
                    : "",
                bark != null
                    ? F(bark.MaximumAuthoredAxialTwistStepStartNormalizedDistance)
                    : "",
                bark != null
                    ? F(bark.MaximumAuthoredAxialTwistStepEndNormalizedDistance)
                    : "",
                bark != null
                    ? bark.VertexCount.ToString(CultureInfo.InvariantCulture)
                    : "0",
                bark != null
                    ? bark.TriangleCount.ToString(CultureInfo.InvariantCulture)
                    : "0",
                bark != null
                    ? bark.RootZoneLongitudinalIntervals.ToString(
                        CultureInfo.InvariantCulture)
                    : "0",
                bark != null ? F(bark.GroundButtressCrestMultiplier) : "",
                bark != null
                    ? F(bark.RequestedRootSupportAngularWidthDegrees)
                    : "",
                bark != null
                    ? F(bark.EmittedRootSupportAngularWidthDegrees)
                    : "",
                bark != null
                    ? (bark.RootSupportWidthClampedByCount ? "1" : "0")
                    : "",
                bark != null
                    ? F(bark.GroundRootBaseMergeFactor)
                    : "",
                bark != null
                    ? F(bark.RootFootShapePlateauEndNormalized)
                    : "",
                bark != null
                    ? F(bark.GroundRootHalfExtensionAngularWidthDegrees)
                    : "",
                bark != null
                    ? F(bark.GroundRootHalfExtensionChordWidth)
                    : "",
                bark != null
                    ? F(bark.EffectiveRootTransitionHeightNormalized)
                    : "",
                bark != null
                    ? bark.MinimumEffectiveTrunkRadialSegments.ToString(
                        CultureInfo.InvariantCulture)
                    : "0",
                bark != null
                    ? bark.MaximumEffectiveTrunkRadialSegments.ToString(
                        CultureInfo.InvariantCulture)
                    : "0",
                trunk != null
                    ? trunk.GroundContactRadialSegments.ToString(
                        CultureInfo.InvariantCulture)
                    : "0",
                trunk != null
                    ? trunk.GroundContactBoostedRingCount.ToString(
                        CultureInfo.InvariantCulture)
                    : "0",
                trunk != null
                    ? F(trunk.GroundContactBoostReleaseNormalizedDistance)
                    : "",
                result.BuildMilliseconds.ToString(
                    "F3",
                    CultureInfo.InvariantCulture),
                Csv(result.CloseCapturePath),
                Csv(result.GameCapturePath),
                Csv(result.Failure)
            }));
        }

        private static void Finish(
            Job job,
            string outcome,
            string failure)
        {
            if (job == null)
            {
                return;
            }

            EditorApplication.update -= Tick;
            AssemblyReloadEvents.beforeAssemblyReload -= AbortForReload;
            EditorApplication.quitting -= AbortForQuit;

            if (job.PendingCapture != null)
            {
                PendingCapture pending = job.PendingCapture;
                job.PendingCapture = null;
                if (IsCaptureComplete(pending))
                {
                    ReleasePendingCapture(pending);
                }
                else
                {
                    pending.Abandoned = true;
                }
            }

            if (job.CurrentResult != null &&
                job.CurrentResult.Mesh != null)
            {
                UnityEngine.Object.DestroyImmediate(
                    job.CurrentResult.Mesh);
                job.CurrentResult.Mesh = null;
            }

            if (job.CsvWriter != null)
            {
                job.CsvWriter.Flush();
                job.CsvWriter.Dispose();
                job.CsvWriter = null;
            }

            WriteCheckpoint(job, outcome, failure);
            activeJob = null;
            currentProgress = outcome == "COMPLETE" ? 1f : currentProgress;
            currentDetail = outcome;
            currentEta = string.Empty;
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();

            if (outcome == "COMPLETE")
            {
                Debug.Log(
                    "[TREE-ROOTS.4C] Ground-contact radial evaluation complete. Board: " +
                    job.BoardPath);
            }
            else if (outcome == "FAILED")
            {
                Debug.LogError(
                    "[TREE-ROOTS.4C] Ground-contact radial evaluation failed. Partial output: " +
                    job.ReportPath + "\n" + failure);
            }
            else
            {
                Debug.LogWarning(
                    "[TREE-ROOTS.4C] Ground-contact radial evaluation cancelled. Partial output: " +
                    job.ReportPath);
            }
        }

        private static void WriteCheckpoint(
            Job job,
            string outcome,
            string failure)
        {
            WriteReport(job, outcome, failure);
            WriteBoard(job, outcome);
        }

        private static void WriteReport(
            Job job,
            string outcome,
            string failure)
        {
            var report = new StringBuilder();
            report.AppendLine("# TREE-ROOTS.4C — Local Ground-Contact Radial Boost Evaluation");
            report.AppendLine();
            report.Append("- Outcome: **").Append(outcome).AppendLine("**");
            report.Append("- Generated UTC: ")
                .AppendLine(DateTime.UtcNow.ToString("O"));
            report.Append("- Completed cases: ")
                .Append(job.Results.Count).Append(" / ")
                .AppendLine(job.Cases.Count.ToString());
            report.AppendLine("- Every case uses ordinary Production Current bark algorithm 28.");
            report.AppendLine("- Production root-shape equations remain TREE-ROOTS.4B; only the lowest lobe-owned ring resolution is locally increased from actual root/contact demand.");
            report.AppendLine("- Ground-foot directional anchoring, foot-shape amplitude, continuous twist, Root Reach, Root Thickness, Root Height, and Buttress Persistence are unchanged.");
            report.AppendLine("- Captures per successful case: neutral close-root three-quarter view; exact game-camera context view.");
            report.AppendLine("- Contract: temporary validation definitions and meshes only; no scene objects, recipes, exact-control snapshots, generated gallery meshes, or serialized assets are modified.");
            report.Append("- Game camera: ")
                .AppendLine(job.GameCameraDescription);
            report.AppendLine("- Capture pipeline: isolated preview Scenes plus polled AsyncGPUReadback; no synchronous GPU readback or wait.");
            if (!string.IsNullOrEmpty(failure))
            {
                report.Append("- Failure: ").AppendLine(FirstFailureLine(failure));
            }
            report.AppendLine();
            report.AppendLine("## Cases");
            report.AppendLine();
            report.AppendLine("| # | Tree | Status | Root Count | Reach / Thickness / Height | Ground radial | Boosted rings | Boost release t | Trunk radial min-max | V/T | Close | Game | Finding |");
            report.AppendLine("|---:|---|---|---:|---:|---:|---:|---:|---:|---:|---|---|---|");
            for (int index = 0; index < job.Results.Count; index++)
            {
                CaseResult result = job.Results[index];
                EvaluationCase evaluationCase = result.Case;
                TreeResolvedControls baseline = evaluationCase.Representative.Controls;
                TreeBarkMeshBuildResult bark = result.Bark;
                TreeBarkMeshBranchGeometryAccounting trunk =
                    FindTrunkAccounting(bark);
                report.Append("| ").Append(evaluationCase.Index)
                    .Append(" | ").Append(evaluationCase.Representative.Name)
                    .Append(" | ").Append(result.Passed ? "PASS" : "FAIL")
                    .Append(" | ").Append(bark != null && bark.BranchGeometryAccounting != null
                        ? result.Definition.ResolvedParameters.RootButtressCount.ToString(CultureInfo.InvariantCulture)
                        : "n/a")
                    .Append(" | ").Append(F(baseline.RootReach))
                    .Append(" / ").Append(F(baseline.RootThickness))
                    .Append(" / ").Append(F(baseline.RootHeight))
                    .Append(" | ").Append(trunk != null
                        ? trunk.GroundContactRadialSegments.ToString(CultureInfo.InvariantCulture)
                        : "n/a")
                    .Append(" | ").Append(trunk != null
                        ? trunk.GroundContactBoostedRingCount.ToString(CultureInfo.InvariantCulture)
                        : "n/a")
                    .Append(" | ").Append(trunk != null
                        ? F(trunk.GroundContactBoostReleaseNormalizedDistance)
                        : "n/a")
                    .Append(" | ").Append(bark != null
                        ? bark.MinimumEffectiveTrunkRadialSegments.ToString(CultureInfo.InvariantCulture) + "-" +
                            bark.MaximumEffectiveTrunkRadialSegments.ToString(CultureInfo.InvariantCulture)
                        : "n/a")
                    .Append(" | ").Append(bark != null
                        ? bark.VertexCount.ToString(CultureInfo.InvariantCulture) + "/" +
                            bark.TriangleCount.ToString(CultureInfo.InvariantCulture)
                        : "n/a")
                    .Append(" | ").Append(RelativeCaptureLink(job, result.CloseCapturePath, "close"))
                    .Append(" | ").Append(RelativeCaptureLink(job, result.GameCapturePath, "game"))
                    .Append(" | ").Append(FirstFailureLine(result.Failure))
                    .AppendLine(" |");
            }

            report.AppendLine();
            report.AppendLine("## Decision use");
            report.AppendLine();
            report.AppendLine("- Twisted 1-5 are the primary silhouette acceptance set; each must show materially smoother ground-contact curvature without a root-shape change.");
            report.AppendLine("- Common 1, Pine 1, and Dead 1 are cost/regression controls; localized demand gating should prevent disproportionate densification.");
            report.AppendLine("- Any topology failure, unexpected upper-trunk densification, or unexplained geometry growth blocks acceptance.");

            File.WriteAllText(job.ReportPath, report.ToString(), Encoding.UTF8);
        }

        private static void WriteBoard(Job job, string outcome)
        {
            var html = new StringBuilder();
            html.AppendLine("<!doctype html>");
            html.AppendLine("<html><head><meta charset=\"utf-8\">");
            html.AppendLine("<title>TREE-ROOTS.4C Local Ground-Contact Radial Boost</title>");
            html.AppendLine("<style>");
            html.AppendLine("body{font-family:Segoe UI,Arial,sans-serif;background:#171717;color:#eee;margin:24px}h1,h2{margin:0 0 12px}h2{margin-top:32px;border-bottom:1px solid #555;padding-bottom:6px}.meta{color:#bbb;margin-bottom:24px}.grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(340px,1fr));gap:16px}.card{background:#242424;border:1px solid #444;border-radius:8px;padding:12px}.card.fail{border-color:#a44}.title{font-weight:700;margin-bottom:8px}.values{font-size:12px;color:#bbb;margin-bottom:8px}.views{display:grid;grid-template-columns:1fr 1fr;gap:8px}.views img{width:100%;height:auto;background:#111;border:1px solid #444}.caption{font-size:11px;color:#aaa;text-align:center;margin-top:3px}.missing{aspect-ratio:1/1;background:#111;display:flex;align-items:center;justify-content:center;color:#a88;border:1px solid #633}.finding{font-size:12px;color:#d9b0b0;margin-top:8px}</style>");
            html.AppendLine("</head><body>");
            html.AppendLine("<h1>TREE-ROOTS.4C — Local Ground-Contact Radial Boost</h1>");
            html.Append("<div class=\"meta\">Outcome: ")
                .Append(Html(outcome)).Append(" · Completed ")
                .Append(job.Results.Count).Append(" / ")
                .Append(job.Cases.Count)
                .AppendLine(" cases · Production Current Twisted 1-5 ground-contact smoothing with Common/Pine/Dead controls.</div>");

            for (int representativeIndex = 0;
                representativeIndex < job.Representatives.Count;
                representativeIndex++)
            {
                Representative representative = job.Representatives[representativeIndex];
                html.Append("<h2>").Append(Html(representative.Name))
                    .AppendLine("</h2><div class=\"grid\">");
                for (int resultIndex = 0;
                    resultIndex < job.Results.Count;
                    resultIndex++)
                {
                    CaseResult result = job.Results[resultIndex];
                    if (result.Case.Representative != representative)
                    {
                        continue;
                    }

                    TreeResolvedControls baseline = representative.Controls;
                    TreeBarkMeshBuildResult bark = result.Bark;
                    html.Append("<div class=\"card")
                        .Append(result.Passed ? string.Empty : " fail")
                        .AppendLine("\">");
                    html.Append("<div class=\"title\">")
                        .Append(result.Case.Index.ToString("D2"))
                        .Append(" · ").Append(Html(result.Case.Label))
                        .AppendLine("</div>");
                    html.Append("<div class=\"values\">")
                        .Append(Html(ModeLabel(result.Case)))
                        .Append(" · Twist ").Append(F(result.Case.AxialTwist ?? baseline.AxialTwist)).Append("°")
                        .Append(" · Thickness ").Append(F(result.Case.RootThickness ?? baseline.RootThickness));
                    if (bark != null)
                    {
                        html.Append("→").Append(F(bark.EvaluatedRootThickness));
                    }
                    html.Append(" · Reach ").Append(F(result.Case.RootReach ?? baseline.RootReach))
                        .Append(" · Height ").Append(F(result.Case.RootHeight ?? baseline.RootHeight))
                        .Append(" · Persistence ").Append(F(result.Case.ButtressPersistence ?? baseline.ButtressTransition));
                    if (bark != null)
                    {
                        TreeBarkMeshBranchGeometryAccounting trunk =
                            FindTrunkAccounting(bark);
                        html.Append(" · Ground radial ")
                            .Append(trunk != null
                                ? trunk.GroundContactRadialSegments.ToString(CultureInfo.InvariantCulture)
                                : "n/a")
                            .Append(" · Boosted rings ")
                            .Append(trunk != null
                                ? trunk.GroundContactBoostedRingCount.ToString(CultureInfo.InvariantCulture)
                                : "n/a")
                            .Append(" · Release t ")
                            .Append(trunk != null
                                ? F(trunk.GroundContactBoostReleaseNormalizedDistance)
                                : "n/a");
                    }
                    html.AppendLine("</div>");
                    html.AppendLine("<div class=\"views\">");
                    AppendBoardImage(html, job, result.CloseCapturePath, "close root");
                    AppendBoardImage(html, job, result.GameCapturePath, "game camera");
                    html.AppendLine("</div>");
                    if (!string.IsNullOrEmpty(result.Failure))
                    {
                        html.Append("<div class=\"finding\">")
                            .Append(Html(FirstFailureLine(result.Failure)))
                            .AppendLine("</div>");
                    }
                    html.AppendLine("</div>");
                }
                html.AppendLine("</div>");
            }

            html.AppendLine("</body></html>");
            File.WriteAllText(job.BoardPath, html.ToString());
        }

        private static void AppendBoardImage(
            StringBuilder html,
            Job job,
            string path,
            string caption)
        {
            html.AppendLine("<div>");
            if (!string.IsNullOrEmpty(path))
            {
                string relative = MakeRelativePath(GetOutputRoot(job), path);
                html.Append("<img loading=\"lazy\" src=\"")
                    .Append(Html(relative.Replace('\\', '/')))
                    .Append("\" alt=\"").Append(Html(caption))
                    .AppendLine("\">");
            }
            else
            {
                html.AppendLine("<div class=\"missing\">Capture unavailable</div>");
            }
            html.Append("<div class=\"caption\">")
                .Append(Html(caption)).AppendLine("</div></div>");
        }

        private static string GetOutputRoot(Job job)
        {
            return Path.GetDirectoryName(job.BoardPath) ?? OutputDirectory;
        }

        private static string RelativeCaptureLink(
            Job job,
            string path,
            string label)
        {
            if (string.IsNullOrEmpty(path))
            {
                return "n/a";
            }
            string relative = MakeRelativePath(GetOutputRoot(job), path)
                .Replace('\\', '/');
            return "[" + label + "](" + relative + ")";
        }

        private static string MakeRelativePath(
            string root,
            string path)
        {
            Uri rootUri = new Uri(
                Path.GetFullPath(root).TrimEnd(
                    Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar) +
                Path.DirectorySeparatorChar);
            Uri pathUri = new Uri(Path.GetFullPath(path));
            return Uri.UnescapeDataString(
                rootUri.MakeRelativeUri(pathUri).ToString());
        }

        private static List<Representative> CollectRepresentatives()
        {
            ProceduralTreeInstance[] instances =
                UnityEngine.Object.FindObjectsByType<ProceduralTreeInstance>(
                    FindObjectsInactive.Include);
            var representatives = new List<Representative>(8);
            AddRepresentativeBySlot(
                representatives, instances, TreeFamily.Twisted, 1, "Twisted 1");
            AddRepresentativeBySlot(
                representatives, instances, TreeFamily.Twisted, 2, "Twisted 2");
            AddRepresentativeBySlot(
                representatives, instances, TreeFamily.Twisted, 3, "Twisted 3");
            AddRepresentativeBySlot(
                representatives, instances, TreeFamily.Twisted, 4, "Twisted 4");
            AddRepresentativeBySlot(
                representatives, instances, TreeFamily.Twisted, 5, "Twisted 5");
            AddRepresentativeBySlot(
                representatives, instances, TreeFamily.Common, 1, "Common 1");
            AddRepresentativeBySlot(
                representatives, instances, TreeFamily.Pine, 1, "Pine 1");
            AddRepresentativeBySlot(
                representatives, instances, TreeFamily.Dead, 1, "Dead 1");
            return representatives;
        }

        private static void AddRepresentativeBySlot(
            List<Representative> representatives,
            IReadOnlyList<ProceduralTreeInstance> instances,
            TreeFamily family,
            int variant,
            string name)
        {
            string stableSlot = TreeGenerationLibraryVariant.BuildStableKey(
                family,
                variant);
            for (int index = 0; index < instances.Count; index++)
            {
                ProceduralTreeInstance candidate = instances[index];
                if (candidate == null ||
                    !candidate.HasExactControls ||
                    candidate.Family != family ||
                    candidate.StableSlotIdentity != stableSlot)
                {
                    continue;
                }

                string sourceIdentity = !string.IsNullOrEmpty(
                    candidate.ExactControlsSourceRecipeIdentity)
                        ? candidate.ExactControlsSourceRecipeIdentity
                        : candidate.Recipe != null
                            ? candidate.Recipe.StableIdentity
                            : stableSlot;
                representatives.Add(new Representative
                {
                    Name = name,
                    Family = candidate.Family,
                    Seed = candidate.MasterSeed,
                    SourceIdentity = sourceIdentity,
                    Controls = CloneControls(candidate.ExactControls),
                    Position = candidate.transform.position,
                    Rotation = candidate.transform.rotation,
                    Scale = candidate.transform.lossyScale
                });
                return;
            }
        }

        private static List<EvaluationCase> BuildCases(
            List<Representative> representatives)
        {
            if (representatives == null || representatives.Count != 8)
            {
                throw new InvalidOperationException(
                    "TREE-ROOTS.4C expected eight gallery representatives.");
            }

            var cases = new List<EvaluationCase>(8);
            for (int index = 0; index < representatives.Count; index++)
            {
                AddCase(
                    cases,
                    representatives[index],
                    "Production Current",
                    "ProductionCurrent");
            }

            if (cases.Count != 8)
            {
                throw new InvalidOperationException(
                    "TREE-ROOTS.4C expected eight cases but built " +
                    cases.Count + ".");
            }

            return cases;
        }

        private static void AddCase(
            List<EvaluationCase> cases,
            Representative representative,
            string label,
            string slug,
            float? axialTwist = null,
            float? rootThickness = null,
            float? rootReach = null,
            float? rootHeight = null,
            float? buttressPersistence = null)
        {
            cases.Add(new EvaluationCase
            {
                Index = cases.Count + 1,
                Representative = representative,
                Label = label,
                Slug = slug,
                AxialTwist = axialTwist,
                RootThickness = rootThickness,
                RootReach = rootReach,
                RootHeight = rootHeight,
                ButtressPersistence = buttressPersistence
            });
        }

        private static TreeBarkMeshBranchGeometryAccounting
            FindTrunkAccounting(TreeBarkMeshBuildResult bark)
        {
            if (bark?.BranchGeometryAccounting == null)
            {
                return null;
            }

            IReadOnlyList<TreeBarkMeshBranchGeometryAccounting> records =
                bark.BranchGeometryAccounting;
            for (int index = 0; index < records.Count; index++)
            {
                TreeBarkMeshBranchGeometryAccounting record = records[index];
                if (record != null && record.BranchOrder == 0)
                {
                    return record;
                }
            }

            return null;
        }

        private static string ModeLabel(EvaluationCase evaluationCase)
        {
            return "Production Current";
        }

        private static void ApplyOverrides(
            TreeResolvedControls controls,
            EvaluationCase evaluationCase)
        {
            if (evaluationCase.AxialTwist.HasValue)
            {
                SetFloat(
                    controls,
                    "axialTwist",
                    evaluationCase.AxialTwist.Value);
            }
            if (evaluationCase.RootThickness.HasValue)
            {
                SetFloat(
                    controls,
                    "rootThickness",
                    evaluationCase.RootThickness.Value);
            }
            if (evaluationCase.RootReach.HasValue)
            {
                SetFloat(
                    controls,
                    "rootReach",
                    evaluationCase.RootReach.Value);
            }
            if (evaluationCase.RootHeight.HasValue)
            {
                SetFloat(
                    controls,
                    "rootHeight",
                    evaluationCase.RootHeight.Value);
            }
            if (evaluationCase.ButtressPersistence.HasValue)
            {
                SetFloat(
                    controls,
                    "buttressTransition",
                    evaluationCase.ButtressPersistence.Value);
            }
        }

        private static TreeResolvedControls CloneControls(
            TreeResolvedControls source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            return JsonUtility.FromJson<TreeResolvedControls>(
                JsonUtility.ToJson(source));
        }

        private static void SetFloat(
            TreeResolvedControls controls,
            string fieldName,
            float value)
        {
            FieldInfo field = typeof(TreeResolvedControls).GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null || field.FieldType != typeof(float))
            {
                throw new MissingFieldException(
                    typeof(TreeResolvedControls).FullName,
                    fieldName);
            }
            field.SetValue(controls, value);
        }

        private static Bounds TransformBounds(
            Bounds localBounds,
            Matrix4x4 localToWorld)
        {
            Vector3 center = localToWorld.MultiplyPoint3x4(localBounds.center);
            Vector3 extents = localBounds.extents;
            Vector3 axisX = localToWorld.MultiplyVector(
                new Vector3(extents.x, 0f, 0f));
            Vector3 axisY = localToWorld.MultiplyVector(
                new Vector3(0f, extents.y, 0f));
            Vector3 axisZ = localToWorld.MultiplyVector(
                new Vector3(0f, 0f, extents.z));
            extents = new Vector3(
                Mathf.Abs(axisX.x) + Mathf.Abs(axisY.x) +
                    Mathf.Abs(axisZ.x),
                Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) +
                    Mathf.Abs(axisZ.y),
                Mathf.Abs(axisX.z) + Mathf.Abs(axisY.z) +
                    Mathf.Abs(axisZ.z));
            return new Bounds(center, extents * 2f);
        }

        private static void DeleteExistingCaptures(string directory)
        {
            string[] files = Directory.GetFiles(directory, "*.png");
            for (int index = 0; index < files.Length; index++)
            {
                File.Delete(files[index]);
            }
        }

        private static string SanitizeFileName(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "Unnamed";
            }

            char[] invalid = Path.GetInvalidFileNameChars();
            var builder = new StringBuilder(value.Length);
            for (int index = 0; index < value.Length; index++)
            {
                char character = value[index];
                builder.Append(Array.IndexOf(invalid, character) >= 0 ||
                    char.IsWhiteSpace(character)
                        ? '_'
                        : character);
            }
            return builder.ToString();
        }

        private static string Csv(string value)
        {
            string safe = value ?? string.Empty;
            return "\"" + safe.Replace("\"", "\"\"") + "\"";
        }

        private static string F(float value)
        {
            return value.ToString("F3", CultureInfo.InvariantCulture);
        }

        private static string Html(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }
            return value.Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;");
        }

        private static string FirstFailureLine(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }
            int newline = value.IndexOfAny(new[] { '\r', '\n' });
            return newline >= 0 ? value.Substring(0, newline) : value;
        }

        private static string FormatDuration(double totalSeconds)
        {
            if (double.IsNaN(totalSeconds) ||
                double.IsInfinity(totalSeconds) ||
                totalSeconds <= 0.0)
            {
                return "calculating";
            }
            TimeSpan duration = TimeSpan.FromSeconds(totalSeconds);
            if (duration.TotalHours >= 1.0)
            {
                return duration.ToString(@"h\:mm\:ss");
            }
            return duration.ToString(@"m\:ss");
        }
    }
}
