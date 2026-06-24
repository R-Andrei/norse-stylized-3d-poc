Shader "PS3D/Stylized River Water"
{
    Properties
    {
        [Header(Water Body)]
        _ShallowColor("Shallow Colour", Color) = (0.458, 0.802, 0.798, 1)
        _DeepColor("Deep Colour", Color) = (0.0, 0.310, 0.594, 1)
        _Clarity("Clarity", Range(0, 1)) = 0.62
        _BodyDepthRange("Depth Range", Range(0.1, 8)) = 1.4
        _BodyDepthContrast("Depth Contrast", Range(0, 1)) = 0.5
        _WaterTintStrength("Water Tint Strength", Range(0, 1)) = 0.72
        _SurfacePresence("Surface Presence", Range(0, 1)) = 0.46

        [Header(Surface State)]
        _FreezeAmount("Freeze Amount", Range(0, 1)) = 0

        [Header(Frozen Body)]
        _IceColor("Ice Colour", Color) = (0.56, 0.78, 0.90, 1)
        _IceTransmission("Ice Transmission", Range(0, 1)) = 0.16
        _IceThickness("Ice Thickness", Range(0, 1)) = 0.72
        _IceCloudiness("Ice Cloudiness", Range(0, 1)) = 0.58
        _IceSurfacePresence("Ice Surface Presence", Range(0, 1)) = 0.86
        _IceScattering("Ice Scattering", Range(0, 1)) = 0.68


        [Header(Surface Motion)]
        [NoScaleOffset] _MotionDetailTexture("Motion Detail Texture", 2D) = "bump" {}
        _MotionFlowSpeed("Flow Speed", Range(0, 12)) = 0
        _MotionWaveHeight("Wave Height", Range(0, 1.25)) = 0
        _MotionWaveLength("Wave Length", Range(0.5, 30)) = 5
        _MotionWaveSteepness("Wave Steepness", Range(0, 1)) = 0.35
        _MotionDetailStrength("Detail Strength", Range(0, 2)) = 0
        _MotionDetailScale("Detail Scale", Range(0.15, 12)) = 1.4
        _MotionTurbulence("Turbulence", Range(0, 1)) = 0.25
        _CurrentAccentStrength("Current Accent Strength", Range(0, 1)) = 0
        _CurrentAccentScale("Current Accent Scale", Range(0.5, 30)) = 5
        _ShoreMotion("Shore Motion", Range(0, 1)) = 0.35
        _ShoreMotionWidth("Shore Motion Width", Range(0.05, 5)) = 0.75
        [HideInInspector] _MotionTime("Motion Time", Float) = 0
        [HideInInspector] _MotionSeed("Motion Seed", Float) = 1731
        _MotionDebugView("Motion Debug View", Range(0, 5)) = 0

        [Header(Refraction and Optical Distortion)]
        _LiquidRefractionStrength("Liquid Refraction Strength", Range(0, 0.02)) = 0
        _RefractionDepthInfluence("Refraction Depth Influence", Range(0, 1)) = 0.55
        _RefractionNormalInfluence("Refraction Normal Influence", Range(0, 1)) = 0.65
        _ShoreRefraction("Shore Refraction", Range(0, 1)) = 0.22
        _RefractionEdgeProtection("Refraction Edge Protection", Range(0, 1)) = 0.88
        [Toggle] _PreserveObjectSilhouettes("Preserve Object Silhouettes", Float) = 1
        _IceDistortionStrength("Ice Distortion Strength", Range(0, 0.012)) = 0.0015
        _IceDiffusion("Ice Diffusion", Range(0, 1)) = 0.28
        [HideInInspector] _RefractionQuality("Refraction Quality", Float) = 1
        _RefractionDebugView("Refraction Debug View", Range(0, 6)) = 0

        
        [Header(Runtime Disturbance Field)]
        [HideInInspector] _DisturbanceEnabled("Disturbance Enabled", Float) = 0
        [HideInInspector] _DisturbanceInterpolation("Disturbance Interpolation", Range(0, 1)) = 1
        [HideInInspector] _DisturbanceGlobalStart("Disturbance Global Start", Float) = 0
        [HideInInspector] _DisturbanceFieldLength("Disturbance Field Length", Float) = 1
        [HideInInspector] _DisturbanceGeometryStrength("Disturbance Geometry Strength", Float) = 1
        [HideInInspector] _DisturbanceNormalStrength("Disturbance Normal Strength", Float) = 1
        [HideInInspector] _DisturbanceShoreInteraction("Disturbance Shore Interaction", Float) = 0.5
        [HideInInspector] _DisturbanceMaximumHeight("Disturbance Maximum Height", Float) = 0.1
        [HideInInspector] _DisturbanceStaticMaximumHeight("Static Pressure Maximum Height", Float) = 1.25
        [HideInInspector] _DisturbanceDebugView("Disturbance Debug View", Float) = 0
        [HideInInspector] _DisturbanceFragmentDetail("Disturbance Fragment Detail", Float) = 0
        [HideInInspector] _DisturbanceStaticTarget("Disturbance Static Pressure", 2D) = "black" {}
        [HideInInspector] _DisturbanceStaticWakeSource("Disturbance Static Wake Source", 2D) = "black" {}
        [HideInInspector] _DisturbanceWakePrevious("Disturbance Wake Previous", 2D) = "black" {}
        [HideInInspector] _DisturbanceWakeCurrent("Disturbance Wake Current", 2D) = "black" {}
        [HideInInspector] _DisturbanceWakeInterpolation("Disturbance Wake Interpolation", Range(0, 1)) = 1

        [Header(Lighting Response)]
        _LightDependence("Light Dependence", Range(0, 1)) = 1
        _AmbientResponse("Ambient Response", Range(0, 2)) = 1
        _SunResponse("Sun Response", Range(0, 2)) = 1
        _LocalLightResponse("Local Light Response", Range(0, 3)) = 1
        _LightColorInfluence("Light Colour Influence", Range(0, 1)) = 0.8
        _MinimumNightVisibility("Minimum Night Visibility", Range(0, 0.5)) = 0.025
        _ShadowResponse("Shadow Response Master", Range(0, 1)) = 1
        _LiquidSurfaceShadowResponse("Liquid Surface Shadow", Range(0, 1)) = 0.08
        _IceSurfaceShadowResponse("Ice Surface Shadow", Range(0, 1)) = 0.65
        _DiffuseWrap("Diffuse Wrap", Range(0, 1)) = 0.22

        [HideInInspector]
        _DomainFallbackDepth("Domain Fallback Depth", Float) = 1.1

        [Header(Body Validation)]
        _BodyDebugView("Body Debug View", Range(0, 12)) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "Queue" = "Transparent-10"
        }

        Pass
        {
            Name "ForwardWaterBody"
            Tags { "LightMode" = "UniversalForward" }

            Cull Off
            ZWrite On
            ZTest LEqual
            Blend One Zero

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _FORWARD_PLUS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
            #include "Includes/RiverWaterCommon.hlsl"
            #include "Includes/RiverWaterDepth.hlsl"
            #include "Includes/RiverWaterLighting.hlsl"
            #include "Includes/RiverWaterMotion.hlsl"
            #include "Includes/RiverWaterDisturbance.hlsl"
            #include "Includes/RiverWaterRefraction.hlsl"
            #include "Includes/RiverWaterBody.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _ShallowColor;
                half4 _DeepColor;
                float _Clarity;
                float _BodyDepthRange;
                float _BodyDepthContrast;
                float _WaterTintStrength;
                float _SurfacePresence;

                float _FreezeAmount;
                half4 _IceColor;
                float _IceTransmission;
                float _IceThickness;
                float _IceCloudiness;
                float _IceSurfacePresence;
                float _IceScattering;

                float _LightDependence;
                float _AmbientResponse;
                float _SunResponse;
                float _LocalLightResponse;
                float _LightColorInfluence;
                float _MinimumNightVisibility;
                float _ShadowResponse;
                float _LiquidSurfaceShadowResponse;
                float _IceSurfaceShadowResponse;
                float _DiffuseWrap;

                float _MotionFlowSpeed;
                float _MotionWaveHeight;
                float _MotionWaveLength;
                float _MotionWaveSteepness;
                float _MotionDetailStrength;
                float _MotionDetailScale;
                float _MotionTurbulence;
                float _CurrentAccentStrength;
                float _CurrentAccentScale;
                float _ShoreMotion;
                float _ShoreMotionWidth;
                float _MotionTime;
                float _MotionSeed;
                float _MotionDebugView;

                float _LiquidRefractionStrength;
                float _RefractionDepthInfluence;
                float _RefractionNormalInfluence;
                float _ShoreRefraction;
                float _RefractionEdgeProtection;
                float _PreserveObjectSilhouettes;
                float _IceDistortionStrength;
                float _IceDiffusion;
                float _RefractionQuality;
                float _RefractionDebugView;

                float _DisturbanceEnabled;
                float _DisturbanceInterpolation;
                float _DisturbanceWakeInterpolation;
                float _DisturbanceGlobalStart;
                float _DisturbanceFieldLength;
                float _DisturbanceGeometryStrength;
                float _DisturbanceNormalStrength;
                float _DisturbanceShoreInteraction;
                float _DisturbanceMaximumHeight;
                float _DisturbanceStaticMaximumHeight;
                float _DisturbanceDebugView;
                float _DisturbanceFragmentDetail;

                float _DomainFallbackDepth;
                float _BodyDebugView;
            CBUFFER_END

            TEXTURE2D(_MotionDetailTexture);
            SAMPLER(sampler_MotionDetailTexture);
            TEXTURE2D(_DisturbanceFieldPrevious);
            SAMPLER(sampler_DisturbanceFieldPrevious);
            TEXTURE2D(_DisturbanceFieldCurrent);
            SAMPLER(sampler_DisturbanceFieldCurrent);
            TEXTURE2D(_DisturbanceStaticTarget);
            SAMPLER(sampler_DisturbanceStaticTarget);
            TEXTURE2D(_DisturbanceStaticWakeSource);
            SAMPLER(sampler_DisturbanceStaticWakeSource);
            TEXTURE2D(_DisturbanceWakePrevious);
            SAMPLER(sampler_DisturbanceWakePrevious);
            TEXTURE2D(_DisturbanceWakeCurrent);
            SAMPLER(sampler_DisturbanceWakeCurrent);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv0 : TEXCOORD0;
                float4 uv1 : TEXCOORD1;
                float4 uv2 : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half3 baseNormalWS : TEXCOORD1;
                float4 domainData : TEXCOORD2;
                half3 tangentWS : TEXCOORD3;
                half3 sideWS : TEXCOORD4;
                float4 motionData : TEXCOORD5;
                float4 disturbanceData : TEXCOORD6;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings Vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                float3 basePositionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 baseNormalWS = normalize(
                    TransformObjectToWorldNormal(input.normalOS));
                float3 tangentWS = normalize(
                    TransformObjectToWorldDir(input.tangentOS.xyz));
                float3 sideWS = normalize(cross(baseNormalWS, tangentWS));

                RiverWaterMotionInputs motionInputs;
                motionInputs.positionWS = basePositionWS;
                motionInputs.baseNormalWS = baseNormalWS;
                motionInputs.tangentWS = tangentWS;
                motionInputs.sideWS = sideWS;
                motionInputs.globalDistance = input.uv1.x;
                motionInputs.lateralMetres = input.uv1.y;
                motionInputs.visibleHalfWidth = input.uv2.x;
                motionInputs.surfaceHalfWidth = input.uv2.y;
                motionInputs.time = _MotionTime;
                motionInputs.freezeAmount = _FreezeAmount;

                RiverWaterMotionResult motion = RiverWaterEvaluateMotionVertex(
                    motionInputs,
                    _MotionFlowSpeed,
                    _MotionWaveHeight,
                    _MotionWaveLength,
                    _MotionWaveSteepness,
                    _MotionTurbulence,
                    _ShoreMotion,
                    _ShoreMotionWidth,
                    _MotionSeed);

                RiverWaterDisturbanceResult disturbance =
                    RiverWaterEvaluateDisturbance(
                        TEXTURE2D_ARGS(
                            _DisturbanceFieldPrevious,
                            sampler_DisturbanceFieldPrevious),
                        TEXTURE2D_ARGS(
                            _DisturbanceFieldCurrent,
                            sampler_DisturbanceFieldCurrent),
                        TEXTURE2D_ARGS(
                            _DisturbanceStaticTarget,
                            sampler_DisturbanceStaticTarget),
                        _DisturbanceEnabled,
                        input.uv1.x,
                        input.uv1.y,
                        input.uv2.x,
                        input.uv2.y,
                        _DisturbanceGlobalStart,
                        _DisturbanceFieldLength,
                        _DisturbanceInterpolation,
                        _DisturbanceGeometryStrength,
                        _DisturbanceShoreInteraction,
                        _DisturbanceMaximumHeight,
                        _DisturbanceStaticMaximumHeight,
                        _FreezeAmount,
                        _MotionTime,
                        motion.macroHeight,
                        _MotionWaveHeight,
                        _MotionFlowSpeed,
                        _MotionWaveLength,
                        _MotionWaveSteepness,
                        _MotionTurbulence,
                        _MotionSeed);

                output.positionWS =
                    basePositionWS +
                    motion.displacementWS +
                    baseNormalWS *
                    (motion.disturbanceHeight + disturbance.height);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.baseNormalWS = baseNormalWS;
                output.tangentWS = tangentWS;
                output.sideWS = sideWS;
                output.domainData = float4(
                    input.uv1.x,
                    input.uv1.y,
                    input.uv2.x,
                    input.uv2.y);
                output.motionData = float4(
                    input.uv0.y,
                    motion.macroHeight,
                    motion.bankMask,
                    ComputeFogFactor(output.positionCS.z));
                output.disturbanceData = float4(
                    disturbance.downstreamGradient,
                    disturbance.lateralGradient,
                    disturbance.height,
                    disturbance.velocity);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 screenUV = GetNormalizedScreenSpaceUV(
                    input.positionCS);

                RiverWaterMotionInputs motionInputs;
                motionInputs.positionWS = input.positionWS;
                motionInputs.baseNormalWS = normalize(input.baseNormalWS);
                motionInputs.tangentWS = normalize(input.tangentWS);
                motionInputs.sideWS = normalize(input.sideWS);
                motionInputs.globalDistance = input.domainData.x;
                motionInputs.lateralMetres = input.domainData.y;
                motionInputs.visibleHalfWidth = input.domainData.z;
                motionInputs.surfaceHalfWidth = input.domainData.w;
                motionInputs.time = _MotionTime;
                motionInputs.freezeAmount = _FreezeAmount;

                RiverWaterMotionResult motion = RiverWaterEvaluateMotionFragment(
                    TEXTURE2D_ARGS(
                        _MotionDetailTexture,
                        sampler_MotionDetailTexture),
                    motionInputs,
                    _MotionFlowSpeed,
                    _MotionWaveHeight,
                    _MotionWaveLength,
                    _MotionWaveSteepness,
                    _MotionDetailStrength,
                    _MotionDetailScale,
                    _MotionTurbulence,
                    _CurrentAccentStrength,
                    _CurrentAccentScale,
                    _ShoreMotion,
                    _ShoreMotionWidth,
                    _MotionSeed);

                float4 resolvedDisturbanceData =
                    input.disturbanceData;

                // Medium and High quality re-sample the low-resolution
                // pressure/ripple field per fragment for crisp contact normals.
                // Low quality retains interpolated vertex pressure gradients;
                // the downstream wake field is still sampled once per fragment.
                if (_DisturbanceFragmentDetail > 0.5 &&
                    _DisturbanceEnabled > 0.5)
                {
                    RiverWaterDisturbanceResult fragmentDisturbance =
                        RiverWaterEvaluateDisturbance(
                            TEXTURE2D_ARGS(
                                _DisturbanceFieldPrevious,
                                sampler_DisturbanceFieldPrevious),
                            TEXTURE2D_ARGS(
                                _DisturbanceFieldCurrent,
                                sampler_DisturbanceFieldCurrent),
                            TEXTURE2D_ARGS(
                                _DisturbanceStaticTarget,
                                sampler_DisturbanceStaticTarget),
                            _DisturbanceEnabled,
                            input.domainData.x,
                            input.domainData.y,
                            input.domainData.z,
                            input.domainData.w,
                            _DisturbanceGlobalStart,
                            _DisturbanceFieldLength,
                            _DisturbanceInterpolation,
                            _DisturbanceGeometryStrength,
                            _DisturbanceShoreInteraction,
                            _DisturbanceMaximumHeight,
                            _DisturbanceStaticMaximumHeight,
                            _FreezeAmount,
                            _MotionTime,
                            motion.macroHeight,
                            _MotionWaveHeight,
                            _MotionFlowSpeed,
                            _MotionWaveLength,
                            _MotionWaveSteepness,
                            _MotionTurbulence,
                            _MotionSeed);

                    resolvedDisturbanceData.x =
                        fragmentDisturbance.downstreamGradient;
                    resolvedDisturbanceData.y =
                        fragmentDisturbance.lateralGradient;
                    resolvedDisturbanceData.w =
                        fragmentDisturbance.velocity;
                }

                RiverWaterWakeResult wake = RiverWaterEvaluateWake(
                    TEXTURE2D_ARGS(
                        _DisturbanceWakePrevious,
                        sampler_DisturbanceWakePrevious),
                    TEXTURE2D_ARGS(
                        _DisturbanceWakeCurrent,
                        sampler_DisturbanceWakeCurrent),
                    _DisturbanceEnabled,
                    input.domainData.x,
                    input.domainData.y,
                    input.domainData.z,
                    input.domainData.w,
                    _DisturbanceGlobalStart,
                    _DisturbanceFieldLength,
                    _DisturbanceWakeInterpolation,
                    _DisturbanceShoreInteraction,
                    _FreezeAmount);

                resolvedDisturbanceData.x += wake.downstreamGradient;
                resolvedDisturbanceData.y += wake.lateralGradient;

                float3 disturbanceNormalWS =
                    RiverWaterApplyDisturbanceNormal(
                        motion.surfaceNormalWS,
                        normalize(input.tangentWS),
                        normalize(input.sideWS),
                        resolvedDisturbanceData.x,
                        resolvedDisturbanceData.y,
                        _DisturbanceNormalStrength);

                motion.surfaceNormalWS = disturbanceNormalWS;
                motion.disturbanceHeight =
                    resolvedDisturbanceData.z;
                motion.disturbanceNormalWS =
                    disturbanceNormalWS - motionInputs.baseNormalWS;

                RiverWaterSurfaceInputs surfaceInputs;
                surfaceInputs.positionWS = input.positionWS;
                surfaceInputs.baseNormalWS = motion.surfaceNormalWS;
                surfaceInputs.localDistance = input.motionData.x;
                surfaceInputs.globalDistance = input.domainData.x;
                surfaceInputs.lateralMetres = input.domainData.y;

                RiverWaterIntegrationInputs integration =
                    RiverWaterCreateEmptyIntegration(motion.surfaceNormalWS);
                integration.disturbanceHeight = motion.disturbanceHeight;
                integration.disturbanceNormalWS = motion.disturbanceNormalWS;

                RiverWaterDepthData depthData = RiverWaterEvaluateDepth(
                    screenUV,
                    surfaceInputs.positionWS,
                    _DomainFallbackDepth,
                    _BodyDepthRange,
                    _BodyDepthContrast,
                    _Clarity);

                RiverWaterRefractionInputs refractionInputs;
                refractionInputs.screenUV = screenUV;
                refractionInputs.positionWS = surfaceInputs.positionWS;
                refractionInputs.baseNormalWS = normalize(input.baseNormalWS);
                refractionInputs.surfaceNormalWS = motion.surfaceNormalWS;
                refractionInputs.tangentWS = normalize(input.tangentWS);
                refractionInputs.sideWS = normalize(input.sideWS);
                refractionInputs.globalDistance = input.domainData.x;
                refractionInputs.lateralMetres = input.domainData.y;
                refractionInputs.visibleHalfWidth = input.domainData.z;
                refractionInputs.surfaceHalfWidth = input.domainData.w;
                refractionInputs.freezeAmount = _FreezeAmount;
                refractionInputs.iceCloudiness = _IceCloudiness;

                RiverWaterRefractionResult refraction =
                    RiverWaterEvaluateRefraction(
                        TEXTURE2D_ARGS(
                            _MotionDetailTexture,
                            sampler_MotionDetailTexture),
                        refractionInputs,
                        depthData,
                        _LiquidRefractionStrength,
                        _RefractionDepthInfluence,
                        _RefractionNormalInfluence,
                        _ShoreRefraction,
                        _RefractionEdgeProtection,
                        _PreserveObjectSilhouettes,
                        _IceDistortionStrength,
                        _IceDiffusion,
                        _RefractionQuality,
                        _DomainFallbackDepth,
                        _BodyDepthRange,
                        _BodyDepthContrast,
                        _Clarity,
                        _MotionSeed,
                        _MotionTime,
                        _MotionFlowSpeed,
                        _MotionWaveHeight,
                        _MotionWaveLength,
                        _MotionWaveSteepness,
                        _MotionDetailStrength,
                        _MotionDetailScale,
                        _MotionTurbulence,
                        _ShoreMotion,
                        _ShoreMotionWidth);

                integration.refractionOffset = refraction.offset;
                float3 sceneColour = refraction.sceneColour;

                float3 viewDirectionWS = GetWorldSpaceNormalizeViewDir(
                    surfaceInputs.positionWS);
                float viewFacing = saturate(dot(
                    motion.surfaceNormalWS,
                    viewDirectionWS));

                InputData lightingInput = (InputData)0;
                lightingInput.positionWS = surfaceInputs.positionWS;
                lightingInput.normalWS = integration.surfaceNormalWS;
                lightingInput.viewDirectionWS = viewDirectionWS;
                lightingInput.shadowCoord = TransformWorldToShadowCoord(
                    surfaceInputs.positionWS);
                lightingInput.normalizedScreenSpaceUV = screenUV;

                RiverWaterLightingResult lighting =
                    RiverWaterEvaluateLighting(
                        lightingInput,
                        _AmbientResponse,
                        _SunResponse,
                        _LocalLightResponse,
                        _LightColorInfluence,
                        _ShadowResponse,
                        _DiffuseWrap);

                float3 liquidBodyLighting =
                    RiverWaterResolveBodyLightingWithMainShadowPolicy(
                        lighting,
                        _LightDependence,
                        _MinimumNightVisibility,
                        _ShadowResponse *
                        _LiquidSurfaceShadowResponse);

                float3 frozenBodyLighting =
                    RiverWaterResolveBodyLightingWithMainShadowPolicy(
                        lighting,
                        _LightDependence,
                        _MinimumNightVisibility,
                        _ShadowResponse *
                        _IceSurfaceShadowResponse);

                RiverWaterBodyResult body = RiverWaterComposeBody(
                    sceneColour,
                    _ShallowColor.rgb,
                    _DeepColor.rgb,
                    depthData,
                    _WaterTintStrength,
                    _SurfacePresence,
                    viewFacing,
                    _FreezeAmount,
                    _IceColor.rgb,
                    _IceTransmission,
                    _IceThickness,
                    _IceCloudiness,
                    _IceSurfacePresence,
                    _IceScattering,
                    liquidBodyLighting,
                    frozenBodyLighting);

                float3 finalColour = RiverWaterApplyReservedIntegration(
                    body.colour,
                    integration);
                finalColour *= 1.0 + motion.currentAccent * 0.22;
                finalColour = MixFog(finalColour, input.motionData.w);

                int disturbanceDebug =
                    (int)round(_DisturbanceDebugView);

                if (disturbanceDebug == 1)
                {
                    float encodedHeight = saturate(
                        resolvedDisturbanceData.z /
                        max(
                            0.001,
                            max(
                                _DisturbanceMaximumHeight,
                                _DisturbanceStaticMaximumHeight)) *
                        0.5 + 0.5);
                    return half4(encodedHeight.xxx, 1.0);
                }

                if (disturbanceDebug == 2)
                {
                    float encodedVelocity = saturate(
                        resolvedDisturbanceData.w * 0.25 + 0.5);
                    return half4(encodedVelocity.xxx, 1.0);
                }

                if (disturbanceDebug == 3)
                {
                    return half4(
                        motion.surfaceNormalWS * 0.5 + 0.5,
                        1.0);
                }

                if (disturbanceDebug == 4)
                {
                    float intensity = saturate(
                        abs(resolvedDisturbanceData.z) /
                        max(0.001, _DisturbanceMaximumHeight) * 0.55 +
                        abs(resolvedDisturbanceData.w) * 0.12 +
                        length(resolvedDisturbanceData.xy) * 0.30 +
                        wake.intensity * 0.35);
                    return half4(intensity.xxx, 1.0);
                }

                if (disturbanceDebug == 5)
                {
                    float2 fieldUV = float2(
                        saturate(
                            (input.domainData.x - _DisturbanceGlobalStart) /
                            max(0.001, _DisturbanceFieldLength)),
                        saturate(
                            input.domainData.y /
                            max(0.001, input.domainData.w) *
                            0.5 + 0.5));
                    return half4(fieldUV.x, fieldUV.y, 0.0, 1.0);
                }

                if (disturbanceDebug == 6 ||
                    disturbanceDebug == 7 ||
                    disturbanceDebug == 8)
                {
                    float2 fieldUV = float2(
                        saturate(
                            (input.domainData.x - _DisturbanceGlobalStart) /
                            max(0.001, _DisturbanceFieldLength)),
                        saturate(
                            input.domainData.y /
                            max(0.001, input.domainData.w) *
                            0.5 + 0.5));

                    if (disturbanceDebug == 6)
                    {
                        float4 staticPressure = SAMPLE_TEXTURE2D(
                            _DisturbanceStaticTarget,
                            sampler_DisturbanceStaticTarget,
                            fieldUV);
                        float pressure = saturate(
                            staticPressure.r /
                            max(
                                0.001,
                                _DisturbanceStaticMaximumHeight));
                        return half4(pressure.xxx, 1.0);
                    }

                    if (disturbanceDebug == 7)
                    {
                        float4 wakeSource = SAMPLE_TEXTURE2D(
                            _DisturbanceStaticWakeSource,
                            sampler_DisturbanceStaticWakeSource,
                            fieldUV);
                        return half4(
                            saturate(wakeSource.r * 0.35),
                            saturate(wakeSource.b),
                            0.0,
                            1.0);
                    }

                    float4 wakePrevious = SAMPLE_TEXTURE2D(
                        _DisturbanceWakePrevious,
                        sampler_DisturbanceWakePrevious,
                        fieldUV);
                    float4 wakeCurrent = SAMPLE_TEXTURE2D(
                        _DisturbanceWakeCurrent,
                        sampler_DisturbanceWakeCurrent,
                        fieldUV);
                    float4 wakeState = lerp(
                        wakePrevious,
                        wakeCurrent,
                        saturate(_DisturbanceWakeInterpolation));
                    float energy = saturate(wakeState.r * 0.30);
                    float lateral = saturate(abs(wakeState.a) * 0.35);
                    return half4(energy, lateral, 0.0, 1.0);
                }

                int refractionDebug =
                    (int)round(_RefractionDebugView);

                if (refractionDebug == 1)
                {
                    return half4(refraction.sceneColour, 1.0);
                }

                if (refractionDebug == 2)
                {
                    float2 encodedOffset =
                        saturate(refraction.offset * 60.0 + 0.5);
                    return half4(
                        encodedOffset.x,
                        encodedOffset.y,
                        0.5,
                        1.0);
                }

                if (refractionDebug == 3)
                {
                    return half4(
                        refraction.depthInfluence.xxx,
                        1.0);
                }

                if (refractionDebug == 4)
                {
                    return half4(
                        refraction.shoreMask.xxx,
                        1.0);
                }

                if (refractionDebug == 5)
                {
                    return half4(
                        refraction.sampleValidity.xxx,
                        1.0);
                }

                if (refractionDebug == 6)
                {
                    return half4(
                        refraction.iceDiffusion.xxx,
                        1.0);
                }

                int motionDebug = (int)round(_MotionDebugView);

                if (motionDebug == 1)
                {
                    return half4(motion.bankMask.xxx, 1.0);
                }

                if (motionDebug == 2)
                {
                    float heightView = saturate(
                        motion.macroHeight /
                        max(0.001, _MotionWaveHeight) *
                        0.5 + 0.5);
                    return half4(heightView.xxx, 1.0);
                }

                if (motionDebug == 3)
                {
                    return half4(motion.surfaceNormalWS * 0.5 + 0.5, 1.0);
                }

                if (motionDebug == 4)
                {
                    return half4(motion.currentAccent.xxx, 1.0);
                }

                if (motionDebug == 5)
                {
                    return half4(motion.liquidFactor.xxx, 1.0);
                }

                int debugMode = (int)round(_BodyDebugView);

                if (debugMode == 1)
                {
                    return half4(depthData.normalizedDepth.xxx, 1.0);
                }

                if (debugMode == 2)
                {
                    return half4(depthData.depthBlend.xxx, 1.0);
                }

                if (debugMode == 3)
                {
                    return half4(depthData.transmission.xxx, 1.0);
                }

                if (debugMode == 4)
                {
                    return half4(body.coverage.xxx, 1.0);
                }

                if (debugMode == 5)
                {
                    return half4(sceneColour, 1.0);
                }

                if (debugMode == 6)
                {
                    return half4(depthData.validSceneDepth.xxx, 1.0);
                }

                if (debugMode == 7)
                {
                    return half4(body.surfaceCoverage.xxx, 1.0);
                }

                if (debugMode == 8)
                {
                    return half4(lighting.combined, 1.0);
                }

                if (debugMode == 9)
                {
                    return half4(lighting.ambient, 1.0);
                }

                if (debugMode == 10)
                {
                    return half4(lighting.sun, 1.0);
                }

                if (debugMode == 11)
                {
                    return half4(lighting.localLights, 1.0);
                }

                if (debugMode == 12)
                {
                    return half4(body.freezeAmount.xxx, 1.0);
                }

                return half4(max(finalColour, 0.0), 1.0);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
