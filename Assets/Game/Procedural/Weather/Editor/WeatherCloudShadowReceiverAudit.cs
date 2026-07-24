using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ProgrammaticStylized3D.Weather.Editor
{
    internal static class WeatherCloudShadowReceiverAudit
    {
        private const string CookieKeywordName = "_LIGHT_COOKIES";
        private const string MenuPath =
            "Tools/PS3D/Weather/Run & Copy Cloud-Shadow Receiver Audit";

        private static readonly Regex IncludeRegex = new Regex(
            "#include(?:_with_pragmas)?\\s+\"([^\"]+)\"",
            RegexOptions.Compiled);

        private static readonly Regex CookieVariantPragmaRegex = new Regex(
            @"#pragma\s+(?:multi_compile(?:_fragment|_vertex)?|shader_feature(?:_local)?(?:_fragment|_vertex)?)\b[^\r\n]*\b_LIGHT_COOKIES\b",
            RegexOptions.Compiled);

        private static readonly HashSet<string> ExplicitlyExemptShaderNames =
            new HashSet<string>(StringComparer.Ordinal)
            {
                "PS3D/Weather/Weather Wind Trails"
            };

        private static readonly string[] MandatoryReceiverShaderNames =
        {
            "PS3D/Pixel Ground Surface Lit",
            "PS3D/Pixel Surface Lit",
            "PS3D/Vegetation/Stylized Vegetation Benchmark",
            "PS3D/Stylized River Water",
            "PS3D/Trees/Stylized Tree Bark",
            "PS3D/Trees/Stylized Tree Foliage"
        };

        private enum ReceiverStatus
        {
            Supported,
            ExplicitlyExempt,
            Unsupported
        }

        private sealed class ShaderAssessment
        {
            public ReceiverStatus Status;
            public string ShaderName = string.Empty;
            public string AssetPath = string.Empty;
            public bool DeclaresCookieKeyword;
            public bool UsesCookieAwareLighting;
            public string Reason = string.Empty;
        }

        private sealed class ReceiverRecord
        {
            public string HierarchyPath = string.Empty;
            public string RendererType = string.Empty;
            public string MaterialName = string.Empty;
            public ShaderAssessment Assessment = new ShaderAssessment();
        }

        [MenuItem(MenuPath, priority = 2400)]
        private static void RunAndCopyAudit()
        {
            string report = BuildReport();
            EditorGUIUtility.systemCopyBuffer = report;
            Debug.Log(report);
            Debug.Log(
                "Weather cloud-shadow receiver audit copied to the clipboard.");
        }

        public static string BuildReport()
        {
            var builder = new StringBuilder(16384);
            var assessmentCache = new Dictionary<Shader, ShaderAssessment>();
            var records = new List<ReceiverRecord>(256);

            AppendHeader(builder);
            bool pipelineReady = AppendPipelineAssets(builder);
            bool sunReady = AppendSunState(builder);
            bool controllerReady = AppendControllerState(builder);
            CollectReceiverRecords(records, assessmentCache);
            bool mandatoryReceiversReady =
                AppendMandatoryReceiverAssessments(
                    builder,
                    assessmentCache);
            bool receiversReady = AppendReceiverSummary(
                builder,
                records,
                assessmentCache,
                pipelineReady &&
                sunReady &&
                controllerReady &&
                mandatoryReceiversReady);
            AppendReceiverDetails(builder, records);
            AppendRequiredNextAction(
                builder,
                pipelineReady,
                sunReady,
                controllerReady,
                mandatoryReceiversReady,
                receiversReady);

            return builder.ToString();
        }

        private static void AppendHeader(StringBuilder builder)
        {
            builder.AppendLine(
                "[Weather Cloud-Shadow Directional-Cookie Receiver Audit]");
            builder.Append("Generated: ")
                .AppendLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            builder.Append("Loaded scene: ")
                .AppendLine(UnityEngine.SceneManagement.SceneManager
                    .GetActiveScene().path);
            builder.AppendLine(
                "Scope: active loaded-scene Renderer components, mandatory authored receivers, the active cloud controller, and all discovered URP assets");
            builder.AppendLine(
                "Mutation: none; shared materials and shader source are read only");
            builder.AppendLine();
        }

        private static bool AppendPipelineAssets(StringBuilder builder)
        {
            builder.AppendLine("[URP Pipeline Assets]");

            string[] guids = AssetDatabase.FindAssets(
                "t:UniversalRenderPipelineAsset");
            if (guids.Length == 0)
            {
                builder.AppendLine(
                    "UNSUPPORTED: no UniversalRenderPipelineAsset was discovered.");
                builder.AppendLine();
                return false;
            }

            bool allSupported = true;
            Array.Sort(guids, StringComparer.Ordinal);
            RenderPipelineAsset activeAsset =
                GraphicsSettings.currentRenderPipeline;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<
                    UniversalRenderPipelineAsset>(path);
                if (asset == null)
                {
                    allSupported = false;
                    builder.Append("UNSUPPORTED | ")
                        .Append(path)
                        .AppendLine(" | failed to load");
                    continue;
                }

                allSupported &= asset.supportsLightCookies;
                bool isActive = asset == activeAsset;
                builder.Append(asset.supportsLightCookies
                        ? "SUPPORTED"
                        : "UNSUPPORTED")
                    .Append(" | ")
                    .Append(path)
                    .Append(" | Light Cookies: ")
                    .Append(asset.supportsLightCookies ? "Enabled" : "Disabled")
                    .Append(" | Active: ")
                    .AppendLine(isActive ? "Yes" : "No");
            }

            builder.AppendLine();
            return allSupported;
        }

        private static bool AppendSunState(StringBuilder builder)
        {
            builder.AppendLine("[Authoritative Sun]");
            Light sun = RenderSettings.sun;
            if (sun == null)
            {
                builder.AppendLine(
                    "UNSUPPORTED: RenderSettings.sun is not assigned.");
                builder.AppendLine();
                return false;
            }

            builder.Append("Object: ")
                .AppendLine(GetHierarchyPath(sun.transform));
            builder.Append("Type: ")
                .AppendLine(sun.type.ToString());
            builder.Append("Enabled / active: ")
                .Append(sun.enabled ? "Yes" : "No")
                .Append(" / ")
                .AppendLine(sun.gameObject.activeInHierarchy ? "Yes" : "No");
            builder.Append("Intensity: ")
                .AppendLine(sun.intensity.ToString("0.###"));
            builder.Append("Cookie: ")
                .AppendLine(sun.cookie != null ? sun.cookie.name : "None");
            builder.Append("Directional cookie size: ")
                .AppendLine(sun.cookieSize2D.ToString("F3"));

            if (sun.TryGetComponent(out UniversalAdditionalLightData data))
            {
                builder.Append("URP cookie size: ")
                    .AppendLine(data.lightCookieSize.ToString("F3"));
                builder.Append("URP cookie offset: ")
                    .AppendLine(data.lightCookieOffset.ToString("F3"));
            }
            else
            {
                builder.AppendLine(
                    "URP additional light data: not present on the current sun");
            }

            bool isDirectional = sun.type == LightType.Directional;
            if (!isDirectional)
            {
                builder.AppendLine(
                    "UNSUPPORTED: the authoritative sun is not directional.");
            }

            builder.AppendLine();
            return isDirectional;
        }

        private static bool AppendControllerState(StringBuilder builder)
        {
            builder.AppendLine("[Weather Cloud-Shadow Controller]");
            WeatherCloudShadowController[] controllers =
                UnityEngine.Object.FindObjectsByType<
                    WeatherCloudShadowController>(
                        FindObjectsInactive.Include);
            int activeCount = 0;
            foreach (WeatherCloudShadowController controller in controllers)
            {
                if (controller != null && controller.isActiveAndEnabled)
                {
                    activeCount++;
                }
            }

            WeatherCloudShadowController published =
                WeatherCloudShadowController.PublishedController;
            builder.Append("Discovered / active controllers: ")
                .Append(controllers.Length)
                .Append(" / ")
                .AppendLine(activeCount.ToString());
            builder.Append("Published controller: ")
                .AppendLine(published != null ?
                    GetHierarchyPath(published.transform) :
                    "None");

            if (published == null || !published.isActiveAndEnabled)
            {
                builder.AppendLine(
                    "UNSUPPORTED: no active published Weather cloud-shadow controller exists.");
                builder.AppendLine();
                return false;
            }

            Light resolvedSun = published.ResolvedSun;
            bool cookieReady = published.CookieReady;
            bool assignmentExpected = published.SunGateActive;
            bool assignmentReady =
                !assignmentExpected ||
                (resolvedSun != null &&
                 resolvedSun.cookie == published.GeneratedCookie);

            builder.Append("Cloud shadows enabled: ")
                .AppendLine(published.CloudShadowsEnabled ? "Yes" : "No");
            builder.Append("Cookie ready: ")
                .AppendLine(cookieReady ? "Yes" : "No");
            builder.Append("Sun gate active: ")
                .AppendLine(assignmentExpected ? "Yes" : "No");
            builder.Append("Cookie assignment: ")
                .AppendLine(assignmentReady ?
                    (assignmentExpected ? "Assigned" : "Not required while gated") :
                    "Missing");
            builder.Append("Controller error: ")
                .AppendLine(string.IsNullOrEmpty(published.LastError) ?
                    "None" :
                    published.LastError);

            bool ready =
                activeCount == 1 &&
                published.CloudShadowsEnabled &&
                cookieReady &&
                resolvedSun != null &&
                resolvedSun.type == LightType.Directional &&
                assignmentReady &&
                string.IsNullOrEmpty(published.LastError);
            builder.Append("Controller gate: ")
                .AppendLine(ready ? "PASS" : "BLOCKED");
            builder.AppendLine();
            return ready;
        }

        private static bool AppendMandatoryReceiverAssessments(
            StringBuilder builder,
            Dictionary<Shader, ShaderAssessment> assessmentCache)
        {
            builder.AppendLine("[Mandatory Authored Receiver Shaders]");
            bool allSupported = true;
            foreach (string shaderName in MandatoryReceiverShaderNames)
            {
                Shader shader = Shader.Find(shaderName);
                if (shader == null)
                {
                    allSupported = false;
                    builder.Append("UNSUPPORTED | ")
                        .Append(shaderName)
                        .AppendLine(" | Shader.Find returned null.");
                    continue;
                }

                if (!assessmentCache.TryGetValue(
                        shader,
                        out ShaderAssessment assessment))
                {
                    assessment = AssessShader(shader);
                    assessmentCache.Add(shader, assessment);
                }

                allSupported &=
                    assessment.Status == ReceiverStatus.Supported;
                builder.Append(assessment.Status
                        .ToString()
                        .ToUpperInvariant())
                    .Append(" | ")
                    .Append(shaderName)
                    .Append(" | ")
                    .AppendLine(assessment.Reason);
            }

            builder.AppendLine();
            return allSupported;
        }

        private static void CollectReceiverRecords(
            List<ReceiverRecord> records,
            Dictionary<Shader, ShaderAssessment> assessmentCache)
        {
            Renderer[] renderers = UnityEngine.Object.FindObjectsByType<Renderer>(
                FindObjectsInactive.Include);

            foreach (Renderer renderer in renderers)
            {
                if (renderer == null ||
                    !renderer.gameObject.scene.IsValid() ||
                    !renderer.enabled ||
                    !renderer.gameObject.activeInHierarchy)
                {
                    continue;
                }

                Material[] materials = renderer.sharedMaterials;
                if (materials == null || materials.Length == 0)
                {
                    records.Add(new ReceiverRecord
                    {
                        HierarchyPath = GetHierarchyPath(renderer.transform),
                        RendererType = renderer.GetType().Name,
                        MaterialName = "<No shared material>",
                        Assessment = new ShaderAssessment
                        {
                            Status = ReceiverStatus.Unsupported,
                            ShaderName = "<None>",
                            Reason = "Renderer has no shared material."
                        }
                    });
                    continue;
                }

                foreach (Material material in materials)
                {
                    if (material == null)
                    {
                        records.Add(new ReceiverRecord
                        {
                            HierarchyPath = GetHierarchyPath(renderer.transform),
                            RendererType = renderer.GetType().Name,
                            MaterialName = "<Missing material>",
                            Assessment = new ShaderAssessment
                            {
                                Status = ReceiverStatus.Unsupported,
                                ShaderName = "<None>",
                                Reason = "A renderer material slot is empty."
                            }
                        });
                        continue;
                    }

                    Shader shader = material.shader;
                    if (shader == null)
                    {
                        records.Add(new ReceiverRecord
                        {
                            HierarchyPath = GetHierarchyPath(renderer.transform),
                            RendererType = renderer.GetType().Name,
                            MaterialName = material.name,
                            Assessment = new ShaderAssessment
                            {
                                Status = ReceiverStatus.Unsupported,
                                ShaderName = "<Missing shader>",
                                Reason = "Material shader reference is missing."
                            }
                        });
                        continue;
                    }

                    if (!assessmentCache.TryGetValue(shader, out ShaderAssessment assessment))
                    {
                        assessment = AssessShader(shader);
                        assessmentCache.Add(shader, assessment);
                    }

                    records.Add(new ReceiverRecord
                    {
                        HierarchyPath = GetHierarchyPath(renderer.transform),
                        RendererType = renderer.GetType().Name,
                        MaterialName = material.name,
                        Assessment = assessment
                    });
                }
            }

            records.Sort((left, right) =>
            {
                int status = left.Assessment.Status.CompareTo(
                    right.Assessment.Status);
                if (status != 0)
                {
                    return status;
                }

                int shader = string.Compare(
                    left.Assessment.ShaderName,
                    right.Assessment.ShaderName,
                    StringComparison.Ordinal);
                if (shader != 0)
                {
                    return shader;
                }

                return string.Compare(
                    left.HierarchyPath,
                    right.HierarchyPath,
                    StringComparison.Ordinal);
            });
        }

        private static ShaderAssessment AssessShader(Shader shader)
        {
            string shaderName = shader.name ?? string.Empty;
            string assetPath = AssetDatabase.GetAssetPath(shader) ?? string.Empty;
            bool keywordSpaceContainsCookie = HasCookieKeyword(shader);

            if (ExplicitlyExemptShaderNames.Contains(shaderName) ||
                shaderName.StartsWith("UI/", StringComparison.Ordinal))
            {
                return new ShaderAssessment
                {
                    Status = ReceiverStatus.ExplicitlyExempt,
                    ShaderName = shaderName,
                    AssetPath = assetPath,
                    DeclaresCookieKeyword = keywordSpaceContainsCookie,
                    Reason = "Shader matches an explicit UI or Weather-trail exemption."
                };
            }

            if (assetPath.EndsWith(
                    ".shadergraph",
                    StringComparison.OrdinalIgnoreCase))
            {
                return new ShaderAssessment
                {
                    Status = keywordSpaceContainsCookie
                        ? ReceiverStatus.Supported
                        : ReceiverStatus.Unsupported,
                    ShaderName = shaderName,
                    AssetPath = assetPath,
                    DeclaresCookieKeyword = keywordSpaceContainsCookie,
                    UsesCookieAwareLighting = keywordSpaceContainsCookie,
                    Reason = keywordSpaceContainsCookie
                        ? "Compiled Shader Graph declares the URP cookie keyword; visual verification remains required."
                        : "Compiled Shader Graph does not declare the URP cookie keyword."
                };
            }

            bool isUrpShader = shaderName.StartsWith(
                "Universal Render Pipeline/",
                StringComparison.Ordinal);
            bool isPackageShader = assetPath.StartsWith(
                "Packages/",
                StringComparison.Ordinal);
            if (isUrpShader && isPackageShader && keywordSpaceContainsCookie)
            {
                return new ShaderAssessment
                {
                    Status = ReceiverStatus.Supported,
                    ShaderName = shaderName,
                    AssetPath = assetPath,
                    DeclaresCookieKeyword = true,
                    UsesCookieAwareLighting = true,
                    Reason =
                        "Package-owned URP shader declares _LIGHT_COOKIES; " +
                        "its package lighting implementation is authoritative."
                };
            }

            if (assetPath.EndsWith(
                    ".shader",
                    StringComparison.OrdinalIgnoreCase))
            {
                string source = LoadShaderSourceClosure(assetPath);
                bool declaresCookieVariant =
                    CookieVariantPragmaRegex.IsMatch(source);
                bool usesPbr = source.Contains(
                    "UniversalFragmentPBR",
                    StringComparison.Ordinal);
                bool samplesCookie = source.Contains(
                    "SampleMainLightCookie",
                    StringComparison.Ordinal);
                bool usesCookieAwareMainLight =
                    ContainsFunctionCallWithArgumentCount(
                        source,
                        "GetMainLight",
                        3);
                bool usesCookieAwareLighting =
                    usesPbr || samplesCookie || usesCookieAwareMainLight;

                return new ShaderAssessment
                {
                    Status = declaresCookieVariant && usesCookieAwareLighting
                        ? ReceiverStatus.Supported
                        : ReceiverStatus.Unsupported,
                    ShaderName = shaderName,
                    AssetPath = assetPath,
                    DeclaresCookieKeyword = declaresCookieVariant,
                    UsesCookieAwareLighting = usesCookieAwareLighting,
                    Reason = BuildShaderReason(
                        declaresCookieVariant,
                        usesPbr,
                        samplesCookie,
                        usesCookieAwareMainLight)
                };
            }

            if (isUrpShader && keywordSpaceContainsCookie)
            {
                return new ShaderAssessment
                {
                    Status = ReceiverStatus.Supported,
                    ShaderName = shaderName,
                    AssetPath = assetPath,
                    DeclaresCookieKeyword = true,
                    UsesCookieAwareLighting = true,
                    Reason = "URP shader declares the main-light cookie keyword."
                };
            }

            return new ShaderAssessment
            {
                Status = ReceiverStatus.Unsupported,
                ShaderName = shaderName,
                AssetPath = assetPath,
                DeclaresCookieKeyword = keywordSpaceContainsCookie,
                Reason = keywordSpaceContainsCookie
                    ? "Shader declares the cookie keyword, but its cookie-aware lighting path could not be proven from source."
                    : "Shader does not declare the URP main-light cookie keyword."
            };
        }

        private static bool HasCookieKeyword(Shader shader)
        {
            LocalKeyword keyword = shader.keywordSpace.FindKeyword(
                CookieKeywordName);
            return keyword.isValid;
        }

        private static bool ContainsFunctionCallWithArgumentCount(
            string source,
            string functionName,
            int expectedArgumentCount)
        {
            int searchIndex = 0;
            while (searchIndex < source.Length)
            {
                int functionIndex = source.IndexOf(
                    functionName,
                    searchIndex,
                    StringComparison.Ordinal);
                if (functionIndex < 0)
                {
                    return false;
                }

                int functionEnd = functionIndex + functionName.Length;
                bool hasIdentifierBefore =
                    functionIndex > 0 &&
                    IsIdentifierCharacter(source[functionIndex - 1]);
                bool hasIdentifierAfter =
                    functionEnd < source.Length &&
                    IsIdentifierCharacter(source[functionEnd]);
                if (hasIdentifierBefore || hasIdentifierAfter)
                {
                    searchIndex = functionEnd;
                    continue;
                }

                int openParenthesisIndex = functionEnd;
                while (
                    openParenthesisIndex < source.Length &&
                    char.IsWhiteSpace(source[openParenthesisIndex]))
                {
                    openParenthesisIndex++;
                }

                if (
                    openParenthesisIndex >= source.Length ||
                    source[openParenthesisIndex] != '(')
                {
                    searchIndex = functionEnd;
                    continue;
                }

                if (
                    TryCountCallArguments(
                        source,
                        openParenthesisIndex,
                        out int argumentCount) &&
                    argumentCount == expectedArgumentCount)
                {
                    return true;
                }

                searchIndex = openParenthesisIndex + 1;
            }

            return false;
        }

        private static bool TryCountCallArguments(
            string source,
            int openParenthesisIndex,
            out int argumentCount)
        {
            argumentCount = 0;
            int parenthesisDepth = 0;
            int topLevelCommaCount = 0;
            bool hasTopLevelContent = false;

            for (int index = openParenthesisIndex; index < source.Length; index++)
            {
                char character = source[index];
                if (character == '(')
                {
                    parenthesisDepth++;
                    if (parenthesisDepth > 1)
                    {
                        hasTopLevelContent = true;
                    }

                    continue;
                }

                if (character == ')')
                {
                    parenthesisDepth--;
                    if (parenthesisDepth < 0)
                    {
                        return false;
                    }

                    if (parenthesisDepth == 0)
                    {
                        argumentCount = hasTopLevelContent
                            ? topLevelCommaCount + 1
                            : 0;
                        return true;
                    }

                    continue;
                }

                if (parenthesisDepth != 1)
                {
                    continue;
                }

                if (character == ',')
                {
                    topLevelCommaCount++;
                    continue;
                }

                if (!char.IsWhiteSpace(character))
                {
                    hasTopLevelContent = true;
                }
            }

            return false;
        }

        private static bool IsIdentifierCharacter(char character)
        {
            return char.IsLetterOrDigit(character) || character == '_';
        }

        private static string BuildShaderReason(
            bool hasKeyword,
            bool usesPbr,
            bool samplesCookie,
            bool usesCookieAwareMainLight)
        {
            if (!hasKeyword)
            {
                return "Missing _LIGHT_COOKIES shader variant. " +
                    BuildLightingEvidence(
                        usesPbr,
                        samplesCookie,
                        usesCookieAwareMainLight);
            }

            if (!usesPbr && !samplesCookie && !usesCookieAwareMainLight)
            {
                return "Declares _LIGHT_COOKIES, but no cookie-aware main-light path was found in the shader include closure.";
            }

            return "Declares _LIGHT_COOKIES. " +
                BuildLightingEvidence(
                    usesPbr,
                    samplesCookie,
                    usesCookieAwareMainLight);
        }

        private static string BuildLightingEvidence(
            bool usesPbr,
            bool samplesCookie,
            bool usesCookieAwareMainLight)
        {
            var evidence = new List<string>(3);
            if (usesPbr)
            {
                evidence.Add("UniversalFragmentPBR");
            }

            if (samplesCookie)
            {
                evidence.Add("SampleMainLightCookie");
            }

            if (usesCookieAwareMainLight)
            {
                evidence.Add("three-argument GetMainLight");
            }

            return evidence.Count > 0
                ? "Cookie-aware lighting evidence: " +
                    string.Join(", ", evidence) + "."
                : "No cookie-aware lighting evidence was found.";
        }

        private static string LoadShaderSourceClosure(string rootAssetPath)
        {
            var builder = new StringBuilder(32768);
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            AppendShaderSource(rootAssetPath, builder, visited, 0);
            return builder.ToString();
        }

        private static void AppendShaderSource(
            string assetPath,
            StringBuilder builder,
            HashSet<string> visited,
            int depth)
        {
            if (depth > 12)
            {
                return;
            }

            string normalizedPath = assetPath.Replace('\\', '/');
            if (!visited.Add(normalizedPath))
            {
                return;
            }

            string fullPath = Path.GetFullPath(normalizedPath);
            if (!File.Exists(fullPath))
            {
                return;
            }

            string source;
            try
            {
                source = File.ReadAllText(fullPath);
            }
            catch (IOException)
            {
                return;
            }
            catch (UnauthorizedAccessException)
            {
                return;
            }

            builder.AppendLine(source);
            string directory = Path.GetDirectoryName(normalizedPath) ?? string.Empty;

            MatchCollection matches = IncludeRegex.Matches(source);
            foreach (Match match in matches)
            {
                string include = match.Groups[1].Value.Replace('\\', '/');
                string includePath =
                    include.StartsWith("Assets/", StringComparison.Ordinal) ||
                    include.StartsWith("Packages/", StringComparison.Ordinal)
                        ? include
                        : Path.Combine(directory, include).Replace('\\', '/');

                // Package source defines generic URP helpers such as
                // UniversalFragmentPBR and cookie-aware GetMainLight overloads.
                // Including those definitions would falsely prove that a custom
                // shader actually calls them. Audit authored Assets source only.
                if (includePath.StartsWith(
                        "Packages/",
                        StringComparison.Ordinal))
                {
                    continue;
                }

                AppendShaderSource(
                    includePath,
                    builder,
                    visited,
                    depth + 1);
            }
        }

        private static bool AppendReceiverSummary(
            StringBuilder builder,
            List<ReceiverRecord> records,
            Dictionary<Shader, ShaderAssessment> assessmentCache,
            bool infrastructureReady)
        {
            int supported = 0;
            int exempt = 0;
            int unsupported = 0;

            foreach (ReceiverRecord record in records)
            {
                switch (record.Assessment.Status)
                {
                    case ReceiverStatus.Supported:
                        supported++;
                        break;
                    case ReceiverStatus.ExplicitlyExempt:
                        exempt++;
                        break;
                    default:
                        unsupported++;
                        break;
                }
            }

            builder.AppendLine("[Receiver Summary]");
            builder.Append("Renderer/material records: ")
                .AppendLine(records.Count.ToString());
            builder.Append("Unique shaders: ")
                .AppendLine(assessmentCache.Count.ToString());
            builder.Append("Supported records: ")
                .AppendLine(supported.ToString());
            builder.Append("Explicitly exempt records: ")
                .AppendLine(exempt.ToString());
            builder.Append("Unsupported records: ")
                .AppendLine(unsupported.ToString());
            bool receiversReady = unsupported == 0;
            builder.Append("V0 audit gate: ")
                .AppendLine(
                    infrastructureReady && receiversReady
                        ? "PASS"
                        : "BLOCKED");
            builder.AppendLine();

            builder.AppendLine("[Unique Shader Assessments]");
            var assessments = new List<ShaderAssessment>(
                assessmentCache.Values);
            assessments.Sort((left, right) =>
                string.Compare(
                    left.ShaderName,
                    right.ShaderName,
                    StringComparison.Ordinal));

            foreach (ShaderAssessment assessment in assessments)
            {
                builder.Append(assessment.Status.ToString().ToUpperInvariant())
                    .Append(" | ")
                    .Append(assessment.ShaderName)
                    .Append(" | Keyword: ")
                    .Append(assessment.DeclaresCookieKeyword ? "Yes" : "No")
                    .Append(" | Lighting path: ")
                    .Append(assessment.UsesCookieAwareLighting ? "Yes" : "No")
                    .Append(" | Path: ")
                    .Append(string.IsNullOrEmpty(assessment.AssetPath)
                        ? "<Built-in or generated>"
                        : assessment.AssetPath)
                    .Append(" | ")
                    .AppendLine(assessment.Reason);
            }

            builder.AppendLine();
            return receiversReady;
        }

        private static void AppendReceiverDetails(
            StringBuilder builder,
            List<ReceiverRecord> records)
        {
            builder.AppendLine("[Loaded-Scene Receiver Details]");
            if (records.Count == 0)
            {
                builder.AppendLine("No loaded-scene Renderer records found.");
                builder.AppendLine();
                return;
            }

            foreach (ReceiverRecord record in records)
            {
                builder.Append(record.Assessment.Status
                        .ToString()
                        .ToUpperInvariant())
                    .Append(" | ")
                    .Append(record.HierarchyPath)
                    .Append(" | ")
                    .Append(record.RendererType)
                    .Append(" | Material: ")
                    .Append(record.MaterialName)
                    .Append(" | Shader: ")
                    .Append(record.Assessment.ShaderName)
                    .Append(" | ")
                    .AppendLine(record.Assessment.Reason);
            }

            builder.AppendLine();
        }

        private static void AppendRequiredNextAction(
            StringBuilder builder,
            bool pipelineReady,
            bool sunReady,
            bool controllerReady,
            bool mandatoryReceiversReady,
            bool receiversReady)
        {
            builder.AppendLine("[Required Next Action]");
            if (!pipelineReady ||
                !sunReady ||
                !controllerReady ||
                !mandatoryReceiversReady ||
                !receiversReady)
            {
                builder.AppendLine(
                    "Resolve every BLOCKED section before cloud-shadow visual acceptance. Do not exempt an ordinary sun-responsive world shader merely because it is absent from the current scene.");
                return;
            }

            builder.AppendLine(
                "All loaded-scene and mandatory authored receivers are supported or explicitly exempt, all discovered URP assets support cookies, the directional sun is valid, and the active Weather controller owns a generated cookie. Proceed to visual continuity and performance validation.");
        }

        private static string GetHierarchyPath(Transform transform)
        {
            if (transform == null)
            {
                return "<Missing transform>";
            }

            var names = new Stack<string>();
            Transform current = transform;
            while (current != null)
            {
                names.Push(current.name);
                current = current.parent;
            }

            return string.Join("/", names);
        }
    }
}
