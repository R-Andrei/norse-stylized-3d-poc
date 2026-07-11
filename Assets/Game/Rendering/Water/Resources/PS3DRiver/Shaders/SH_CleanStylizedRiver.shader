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
        _ShoreWaveHeightScale("Shore Wave Height Scale", Range(0, 2.5)) = 1
        _ShoreWaveLengthScale("Shore Wave Length Scale", Range(0.25, 4)) = 1
        _ShoreWaveReach("Shore Wave Reach", Range(0, 1)) = 1
        _ShoreWaveTransitionLength("Shore Wave Transition Length", Range(0.25, 3)) = 1
        _ShoreWaveSizeVariation("Shore Wave Size Variation", Range(0, 1)) = 0
        _ShoreWaveSideAsymmetry("Shore Side Asymmetry", Range(0, 1)) = 0
        _ShoreWaveProfileVariation("Shore Wave Profile Variation", Range(0, 1)) = 0
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
        [HideInInspector] _DisturbanceWakeGeometryHeight("Wake Geometry Height", Float) = 0.08
        [HideInInspector] _DisturbanceWakeGeometryCompactness("Wake Geometry Compactness", Float) = 1.50
        [HideInInspector] _DisturbanceDebugView("Disturbance Debug View", Float) = 0
        [HideInInspector] _DisturbanceFragmentDetail("Disturbance Fragment Detail", Float) = 0
        [HideInInspector] _DisturbanceStaticTarget("Disturbance Static Pressure", 2D) = "black" {}
        [HideInInspector] _DisturbanceRippleBoundary("Disturbance Ripple Boundary", 2D) = "white" {}
        [HideInInspector] _DisturbanceStaticWakeSource("Disturbance Static Wake Source", 2D) = "black" {}
        [HideInInspector] _DisturbanceWakePrevious("Disturbance Wake Previous", 2D) = "black" {}
        [HideInInspector] _DisturbanceWakeCurrent("Disturbance Wake Current", 2D) = "black" {}
        [HideInInspector] _DisturbanceWakeInterpolation("Disturbance Wake Interpolation", Range(0, 1)) = 1

        [Header(Stage 6 Foam Field)]
        [HideInInspector] _FoamEnabled("Foam Enabled", Float) = 0
        [HideInInspector] _FoamPrevious("Foam Previous", 2D) = "black" {}
        [HideInInspector] _FoamCurrent("Foam Current", 2D) = "black" {}
        [HideInInspector] _FoamShapeMask("Foam Shape Mask", 2D) = "black" {}
        [HideInInspector] _FoamFilmSource("Foam Film Source", 2D) = "black" {}
        [HideInInspector] _FoamFilmSupport("Foam Film Support", 2D) = "black" {}
        [HideInInspector] _FoamVisualOccupancy("Foam Visual Occupancy", 2D) = "black" {}
        [HideInInspector] _FoamBirthDebug("Foam Progressive Birth Debug", 2D) = "black" {}
        [HideInInspector] _FoamTopology("Foam Topology", 2D) = "black" {}
        [HideInInspector] _FoamTopologySources("Foam Topology Sources", 2D) = "black" {}
        [HideInInspector] _FoamObstacleExclusion("Foam Obstacle Footprint", 2D) = "black" {}
        [HideInInspector] _FoamMotionLane("Foam Motion Lane", 2D) = "black" {}
        [HideInInspector] _FoamObstacleRouting("Foam Obstacle Routing", 2D) = "black" {}
        [HideInInspector] _FoamMotionLaneScrollCells("Foam Motion Lane Scroll Cells", Float) = 0
        [HideInInspector] _FoamBaseDownstreamSpeed("Foam Base Downstream Speed", Float) = 0
        [HideInInspector] _FoamMaximumLateralSpeedRatio("Foam Maximum Lateral Speed Ratio", Float) = 0.22
        [HideInInspector] _FoamObstacleSlowdownStrength("Foam Obstacle Slowdown Strength", Float) = 0.85
        [HideInInspector] _FoamObstacleMinimumDownstreamFactor("Foam Obstacle Minimum Downstream Factor", Float) = 0.12
        [HideInInspector] _FoamInterpolation("Foam Interpolation", Range(0, 1)) = 1
        [HideInInspector] _FoamRenderAdvectionSeconds("Foam Render Advection Seconds", Float) = 0
        [HideInInspector] _FoamFlowDirection("Foam Flow Direction", Float) = 1
        [HideInInspector] _FoamGlobalStart("Foam Global Start", Float) = 0
        [HideInInspector] _FoamFieldLength("Foam Field Length", Float) = 1
        [HideInInspector] _FoamColour("Foam Colour", Color) = (0.94, 0.97, 0.94, 1)
        [HideInInspector] _FoamSharpness("Foam Sharpness", Range(0, 1)) = 0.8
        [HideInInspector] _FoamDebugView("Foam Debug View", Float) = 0

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
            #include "Includes/RiverWaterFoam.hlsl"
            #include "Includes/RiverWaterFoamVelocity.hlsl"
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
                float _ShoreWaveHeightScale;
                float _ShoreWaveLengthScale;
                float _ShoreWaveReach;
                float _ShoreWaveTransitionLength;
                float _ShoreWaveSizeVariation;
                float _ShoreWaveSideAsymmetry;
                float _ShoreWaveProfileVariation;
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
                float _DisturbanceWakeGeometryHeight;
                float _DisturbanceWakeGeometryCompactness;
                float _DisturbanceDebugView;
                float _DisturbanceFragmentDetail;
                float4 _DisturbanceStaticWakeTexelSize;

                float _FoamEnabled;
                float _FoamInterpolation;
                float _FoamRenderAdvectionSeconds;
                float _FoamFlowDirection;
                float _FoamGlobalStart;
                float _FoamFieldLength;
                half4 _FoamColour;
                float _FoamSharpness;
                float _FoamDebugView;
                float _FoamMotionLaneScrollCells;
                float _FoamBaseDownstreamSpeed;
                float _FoamMaximumLateralSpeedRatio;
                float _FoamObstacleSlowdownStrength;
                float _FoamObstacleMinimumDownstreamFactor;
                float4 _FoamCurrent_TexelSize;
                float4 _FoamBirthDebug_TexelSize;
                float4 _FoamObstacleExclusion_TexelSize;
                float4 _FoamMotionLane_TexelSize;
                float4 _FoamObstacleRouting_TexelSize;

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
            TEXTURE2D(_DisturbanceRippleBoundary);
            SAMPLER(sampler_DisturbanceRippleBoundary);
            TEXTURE2D(_DisturbanceStaticWakeSource);
            SAMPLER(sampler_DisturbanceStaticWakeSource);
            TEXTURE2D(_DisturbanceWakePrevious);
            SAMPLER(sampler_DisturbanceWakePrevious);
            TEXTURE2D(_DisturbanceWakeCurrent);
            SAMPLER(sampler_DisturbanceWakeCurrent);
            TEXTURE2D(_FoamPrevious);
            SAMPLER(sampler_FoamPrevious);
            TEXTURE2D(_FoamCurrent);
            SAMPLER(sampler_FoamCurrent);
            TEXTURE2D(_FoamShapeMask);
            TEXTURE2D(_FoamFilmSource);
            TEXTURE2D(_FoamFilmSupport);
            TEXTURE2D(_FoamVisualOccupancy);
            TEXTURE2D(_FoamBirthDebug);
            // Topology diagnostics reuse sampler_FoamCurrent, which is already
            // allocated by the normal Foam path, so no extra fragment sampler
            // is required.
            TEXTURE2D(_FoamTopology);
            TEXTURE2D(_FoamTopologySources);
            TEXTURE2D(_FoamObstacleExclusion);
            TEXTURE2D(_FoamMotionLane);
            TEXTURE2D(_FoamObstacleRouting);

            float4 SampleCommittedFoamState(float2 fieldUV)
            {
                return SAMPLE_TEXTURE2D_LOD(
                    _FoamCurrent,
                    sampler_FoamCurrent,
                    saturate(fieldUV),
                    0.0);
            }

            float ResolveCommittedFoamPresenceVisibility(
                float committedPresence)
            {
                // Comparative diagnostics share one meaningful-material gate.
                // Raw Material Presence remains amplitude-faithful, while
                // overlays and normalized moment views suppress tiny donor-cell
                // tails that would otherwise look like full-strength coverage.
                return smoothstep(
                    0.02,
                    0.16,
                    saturate(committedPresence));
            }

            RiverWaterFoamResolvedVelocity ResolveRenderedFoamVelocity(
                float2 fieldUV)
            {
                int2 laneDimensions = int2(
                    max(1.0, _FoamMotionLane_TexelSize.z),
                    max(1.0, _FoamMotionLane_TexelSize.w));
                int2 routingDimensions = int2(
                    max(1.0, _FoamObstacleRouting_TexelSize.z),
                    max(1.0, _FoamObstacleRouting_TexelSize.w));
                float laneX = fieldUV.x * (float)laneDimensions.x -
                    _FoamMotionLaneScrollCells;
                laneX = fmod(laneX, (float)laneDimensions.x);
                if (laneX < 0.0)
                {
                    laneX += (float)laneDimensions.x;
                }

                int laneX0 = clamp(
                    (int)floor(laneX),
                    0,
                    laneDimensions.x - 1);
                int laneX1 = laneX0 + 1;
                if (laneX1 >= laneDimensions.x)
                {
                    laneX1 = 0;
                }

                int laneY = clamp(
                    (int)floor(fieldUV.y * (float)laneDimensions.y),
                    0,
                    laneDimensions.y - 1);
                float laneBlend = frac(laneX);
                float laneA = _FoamMotionLane.Load(
                    int3(laneX0, laneY, 0)).r;
                float laneB = _FoamMotionLane.Load(
                    int3(laneX1, laneY, 0)).r;
                float lane = clamp(
                    lerp(laneA, laneB, laneBlend),
                    -1.0,
                    1.0);

                int2 routingCoordinate = clamp(
                    (int2)floor(fieldUV * (float2)routingDimensions),
                    int2(0, 0),
                    routingDimensions - 1);
                float2 obstacleRouting = _FoamObstacleRouting.Load(
                    int3(routingCoordinate, 0)).rg;
                obstacleRouting.x = clamp(obstacleRouting.x, -1.0, 1.0);
                obstacleRouting.y = saturate(obstacleRouting.y);

                int2 obstacleDimensions = int2(
                    max(1.0, _FoamObstacleExclusion_TexelSize.z),
                    max(1.0, _FoamObstacleExclusion_TexelSize.w));
                int2 obstacleCoordinate = clamp(
                    (int2)floor(fieldUV * (float2)obstacleDimensions),
                    int2(0, 0),
                    obstacleDimensions - 1);
                float obstacleFootprint = saturate(
                    _FoamObstacleExclusion.Load(
                        int3(obstacleCoordinate, 0)).r);

                return RiverWaterResolveFoamVelocityContract(
                    lane,
                    obstacleRouting.x,
                    obstacleRouting.y,
                    _FoamBaseDownstreamSpeed,
                    _FoamMaximumLateralSpeedRatio,
                    _FoamObstacleSlowdownStrength,
                    _FoamObstacleMinimumDownstreamFactor,
                    1.0 - obstacleFootprint);
            }

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
                    _ShoreWaveHeightScale,
                    _ShoreWaveLengthScale,
                    _ShoreWaveReach,
                    _ShoreWaveTransitionLength,
                    _ShoreWaveSizeVariation,
                    _ShoreWaveSideAsymmetry,
                    _ShoreWaveProfileVariation,
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

                RiverWaterStaticWakeLeeResult staticWakeLee =
                    RiverWaterEvaluateStaticWakeLee(
                        TEXTURE2D_ARGS(
                            _DisturbanceStaticWakeSource,
                            sampler_DisturbanceStaticWakeSource),
                        _DisturbanceEnabled,
                        input.uv1.x,
                        input.uv1.y,
                        input.uv2.x,
                        input.uv2.y,
                        _DisturbanceGlobalStart,
                        _DisturbanceFieldLength,
                        _DisturbanceShoreInteraction,
                        _FreezeAmount,
                        _DisturbanceStaticWakeTexelSize.xy,
                        0.0);
                RiverWaterWakeResult vertexWake = RiverWaterEvaluateWake(
                    TEXTURE2D_ARGS(
                        _DisturbanceWakePrevious,
                        sampler_DisturbanceWakePrevious),
                    TEXTURE2D_ARGS(
                        _DisturbanceWakeCurrent,
                        sampler_DisturbanceWakeCurrent),
                    _DisturbanceEnabled,
                    input.uv1.x,
                    input.uv1.y,
                    input.uv2.x,
                    input.uv2.y,
                    _DisturbanceGlobalStart,
                    _DisturbanceFieldLength,
                    _DisturbanceWakeInterpolation,
                    _DisturbanceShoreInteraction,
                    _FreezeAmount,
                    _DisturbanceWakeGeometryCompactness);
                float transportedWakeHeight =
                    RiverWaterResolveWakeGeometryHeight(
                        vertexWake.geometryCore,
                        _DisturbanceWakeGeometryHeight,
                        staticWakeLee.depth);
                float staticWakeHeight =
                    transportedWakeHeight - staticWakeLee.depth;
                float resolvedGeometryHeight =
                    disturbance.height + staticWakeHeight;

                output.positionWS =
                    basePositionWS +
                    motion.displacementWS +
                    baseNormalWS *
                    (motion.disturbanceHeight + resolvedGeometryHeight);
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
                    resolvedGeometryHeight,
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
                    _ShoreWaveHeightScale,
                    _ShoreWaveLengthScale,
                    _ShoreWaveReach,
                    _ShoreWaveTransitionLength,
                    _ShoreWaveSizeVariation,
                    _ShoreWaveSideAsymmetry,
                    _ShoreWaveProfileVariation,
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

                    RiverWaterStaticWakeLeeResult fragmentStaticWakeLee =
                        RiverWaterEvaluateStaticWakeLee(
                            TEXTURE2D_ARGS(
                                _DisturbanceStaticWakeSource,
                                sampler_DisturbanceStaticWakeSource),
                            _DisturbanceEnabled,
                            input.domainData.x,
                            input.domainData.y,
                            input.domainData.z,
                            input.domainData.w,
                            _DisturbanceGlobalStart,
                            _DisturbanceFieldLength,
                            _DisturbanceShoreInteraction,
                            _FreezeAmount,
                            _DisturbanceStaticWakeTexelSize.xy,
                            1.0);
                    resolvedDisturbanceData.x +=
                        fragmentStaticWakeLee.downstreamGradient;
                    resolvedDisturbanceData.y +=
                        fragmentStaticWakeLee.lateralGradient;
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
                    _FreezeAmount,
                    _DisturbanceWakeGeometryCompactness);

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
                        _ShoreMotionWidth,
                        _ShoreWaveHeightScale,
                        _ShoreWaveLengthScale,
                        _ShoreWaveReach,
                        _ShoreWaveTransitionLength,
                        _ShoreWaveSizeVariation,
                        _ShoreWaveSideAsymmetry,
                        _ShoreWaveProfileVariation);

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

                RiverWaterFoamSurfaceInfluence foamSurface =
                    RiverWaterCreateFoamSurfaceInfluence();
                foamSurface.macroHeight = motion.macroHeight;
                foamSurface.currentAccent = motion.currentAccent;
                foamSurface.disturbanceHeight = motion.disturbanceHeight;
                foamSurface.downstreamGradient = resolvedDisturbanceData.x;
                foamSurface.lateralGradient = resolvedDisturbanceData.y;
                foamSurface.disturbanceVelocity = resolvedDisturbanceData.w;
                foamSurface.wakeEnergy = wake.energy;
                foamSurface.wakeIntensity = wake.intensity;
                foamSurface.wakeDownstreamGradient = wake.downstreamGradient;
                foamSurface.wakeLateralGradient = wake.lateralGradient;

                float2 foamVelocityFieldUV = float2(
                    saturate((input.domainData.x - _FoamGlobalStart) /
                        max(0.001, _FoamFieldLength)),
                    saturate(input.domainData.y /
                        max(0.001, input.domainData.w) * 0.5 + 0.5));
                RiverWaterFoamResolvedVelocity renderedFoamVelocity =
                    ResolveRenderedFoamVelocity(foamVelocityFieldUV);

                RiverWaterFoamResult foam = RiverWaterEvaluateFoam(
                    TEXTURE2D_ARGS(
                        _FoamPrevious,
                        sampler_FoamPrevious),
                    TEXTURE2D_ARGS(
                        _FoamCurrent,
                        sampler_FoamCurrent),
                    _FoamEnabled,
                    input.domainData.x,
                    input.domainData.y,
                    input.domainData.w,
                    _FoamGlobalStart,
                    _FoamFieldLength,
                    _FoamInterpolation,
                    _FoamRenderAdvectionSeconds,
                    renderedFoamVelocity.velocityMetresPerSecond,
                    renderedFoamVelocity.obstacleInfluence,
                    _FoamFlowDirection,
                    _FoamSharpness,
                    _FreezeAmount,
                    foamSurface);


                float3 finalColour = RiverWaterApplyReservedIntegration(
                    body.colour,
                    integration);
                finalColour *= 1.0 + motion.currentAccent * 0.22;

                float3 foamColour = RiverWaterResolveFoamColourFiltered(
                    _FoamColour.rgb,
                    lighting.combined,
                    foam.mask,
                    foam.surfaceEnergy,
                    _MinimumNightVisibility);
                // Foam lifetime no longer fades the whole patch. RiverWaterEvaluateFoam
                // returns an erosion-shaped mask: surviving fragments should stay
                // foam-coloured, with only a narrow fringe blending into water.
                float foamBlend = saturate(
                    smoothstep(0.08, 0.46, foam.mask) *
                    _FoamColour.a);
                finalColour = lerp(finalColour, foamColour, foamBlend);
                finalColour = MixFog(finalColour, input.motionData.w);

                int foamDebug = (int)round(_FoamDebugView);

                if (foamDebug == 5 || foamDebug == 6)
                {
                    int2 laneDimensions = int2(
                        max(1.0, _FoamMotionLane_TexelSize.z),
                        max(1.0, _FoamMotionLane_TexelSize.w));
                    int2 routingDimensions = int2(
                        max(1.0, _FoamObstacleRouting_TexelSize.z),
                        max(1.0, _FoamObstacleRouting_TexelSize.w));
                    float laneX = foam.fieldUV.x * (float)laneDimensions.x -
                        _FoamMotionLaneScrollCells;
                    laneX = fmod(laneX, (float)laneDimensions.x);
                    if (laneX < 0.0)
                    {
                        laneX += (float)laneDimensions.x;
                    }

                    int laneX0 = clamp(
                        (int)floor(laneX),
                        0,
                        laneDimensions.x - 1);
                    int laneX1 = laneX0 + 1;
                    if (laneX1 >= laneDimensions.x)
                    {
                        laneX1 = 0;
                    }

                    int laneY = clamp(
                        (int)floor(foam.fieldUV.y * (float)laneDimensions.y),
                        0,
                        laneDimensions.y - 1);
                    float laneBlend = frac(laneX);
                    float laneA = _FoamMotionLane.Load(
                        int3(laneX0, laneY, 0)).r;
                    float laneB = _FoamMotionLane.Load(
                        int3(laneX1, laneY, 0)).r;
                    float lane = clamp(lerp(laneA, laneB, laneBlend), -1.0, 1.0);

                    int2 routingCoordinate = clamp(
                        (int2)floor(foam.fieldUV * (float2)routingDimensions),
                        int2(0, 0),
                        routingDimensions - 1);
                    float2 obstacleRouting = _FoamObstacleRouting.Load(
                        int3(routingCoordinate, 0)).rg;
                    obstacleRouting.x = clamp(obstacleRouting.x, -1.0, 1.0);
                    obstacleRouting.y = saturate(obstacleRouting.y);
                    int2 obstacleDimensions = int2(
                        max(1.0, _FoamObstacleExclusion_TexelSize.z),
                        max(1.0, _FoamObstacleExclusion_TexelSize.w));
                    int2 obstacleCoordinate = clamp(
                        (int2)floor(
                            foam.fieldUV * (float2)obstacleDimensions),
                        int2(0, 0),
                        obstacleDimensions - 1);
                    float obstacleFootprint = saturate(
                        _FoamObstacleExclusion.Load(
                            int3(obstacleCoordinate, 0)).r);
                    RiverWaterFoamResolvedVelocity resolvedVelocity =
                        RiverWaterResolveFoamVelocityContract(
                            lane,
                            obstacleRouting.x,
                            obstacleRouting.y,
                            _FoamBaseDownstreamSpeed,
                            _FoamMaximumLateralSpeedRatio,
                            _FoamObstacleSlowdownStrength,
                            _FoamObstacleMinimumDownstreamFactor,
                            1.0 - obstacleFootprint);

                    float maximumLateralSpeed =
                        max(0.0, _FoamBaseDownstreamSpeed) *
                        max(0.0, _FoamMaximumLateralSpeedRatio);
                    float resolvedLateral = maximumLateralSpeed > 0.0001
                        ? clamp(
                            resolvedVelocity.velocityMetresPerSecond.y /
                            maximumLateralSpeed,
                            -1.0,
                            1.0)
                        : 0.0;
                    float lateralMagnitude = abs(resolvedLateral);
                    float3 neutralColour = float3(0.72, 0.72, 0.72);
                    float3 lateralColour = resolvedLateral >= 0.0
                        ? float3(0.96, 0.20, 0.07)
                        : float3(0.08, 0.30, 0.96);
                    float3 fieldHue = lerp(
                        neutralColour,
                        lateralColour,
                        lateralMagnitude);
                    fieldHue = lerp(
                        fieldHue,
                        float3(1.0, 0.82, 0.10),
                        resolvedVelocity.obstacleInfluence * 0.48);
                    float downstreamBrightness = lerp(
                        0.06,
                        1.0,
                        resolvedVelocity.downstreamSpeedFactor);
                    float3 fieldColour = fieldHue * downstreamBrightness;

                    // Motion Field diagnostics show the committed Layer C state
                    // at fieldUV. Render-only residual prediction must not move
                    // the white ownership overlay or its simulation-cell grid.
                    float committedPresence = saturate(
                        SampleCommittedFoamState(foam.fieldUV).r);
                    float committedPresenceVisibility =
                        ResolveCommittedFoamPresenceVisibility(
                            committedPresence);
                    float foamOverlay = saturate(
                        committedPresenceVisibility * 0.58);
                    fieldColour = lerp(
                        saturate(fieldColour),
                        float3(1.0, 1.0, 0.95),
                        foamOverlay);

                    if (foamDebug == 6)
                    {
                        int2 foamDimensions = int2(
                            max(1.0, _FoamCurrent_TexelSize.z),
                            max(1.0, _FoamCurrent_TexelSize.w));
                        if (foamDimensions.x <= 1 || foamDimensions.y <= 1)
                        {
                            foamDimensions = laneDimensions;
                        }

                        // Draw persistent material cells in committed field space,
                        // never in the residual-predicted presentation coordinate.
                        float2 cellCoordinate =
                            saturate(foam.fieldUV) * (float2)foamDimensions;
                        float2 cellFraction = frac(cellCoordinate);
                        float2 cellEdgeDistance = min(
                            cellFraction,
                            1.0 - cellFraction);
                        float2 fineWidth = max(
                            fwidth(cellCoordinate) * 1.35,
                            float2(0.0001, 0.0001));
                        float fineGrid = max(
                            1.0 - smoothstep(0.0, fineWidth.x, cellEdgeDistance.x),
                            1.0 - smoothstep(0.0, fineWidth.y, cellEdgeDistance.y));

                        float2 majorCoordinate = cellCoordinate * 0.125;
                        float2 majorFraction = frac(majorCoordinate);
                        float2 majorEdgeDistance = min(
                            majorFraction,
                            1.0 - majorFraction);
                        float2 majorWidth = max(
                            fwidth(majorCoordinate) * 1.65,
                            float2(0.0001, 0.0001));
                        float majorGrid = max(
                            1.0 - smoothstep(0.0, majorWidth.x, majorEdgeDistance.x),
                            1.0 - smoothstep(0.0, majorWidth.y, majorEdgeDistance.y));

                        fieldColour = lerp(
                            fieldColour,
                            fieldColour * 0.52,
                            saturate(fineGrid * 0.32));
                        fieldColour = lerp(
                            fieldColour,
                            float3(1.0, 0.96, 0.72),
                            saturate(majorGrid * 0.50));
                    }

                    return half4(saturate(fieldColour), 1.0);
                }

                if (foamDebug == 7 || foamDebug == 8 ||
                    foamDebug == 9 || foamDebug == 10 ||
                    foamDebug == 11 || foamDebug == 12 ||
                    foamDebug == 13 || foamDebug == 14 ||
                    foamDebug == 15)
                {
                    float evaluatedShape = saturate(
                        SAMPLE_TEXTURE2D(
                            _FoamShapeMask,
                            sampler_FoamCurrent,
                            foam.fieldUV).r);

                    if (foamDebug == 11)
                    {
                        float filmSource = saturate(
                            SAMPLE_TEXTURE2D(
                                _FoamFilmSource,
                                sampler_FoamCurrent,
                                foam.fieldUV).r);
                        return half4(filmSource.xxx, 1.0);
                    }

                    if (foamDebug == 12)
                    {
                        float filmSupport = saturate(
                            SAMPLE_TEXTURE2D(
                                _FoamFilmSupport,
                                sampler_FoamCurrent,
                                foam.fieldUV).r);
                        return half4(filmSupport.xxx, 1.0);
                    }

                    float filmSource = saturate(
                        SAMPLE_TEXTURE2D(
                            _FoamFilmSource,
                            sampler_FoamCurrent,
                            foam.fieldUV).r);
                    float filmSupport = saturate(
                        SAMPLE_TEXTURE2D(
                            _FoamFilmSupport,
                            sampler_FoamCurrent,
                            foam.fieldUV).r);
                    float instantaneousFilmTarget = saturate(max(
                        smoothstep(0.10, 0.46, filmSource) * 0.68,
                        smoothstep(0.28, 0.74, filmSupport) * 0.60));
                    float temporalOccupancy = saturate(
                        SAMPLE_TEXTURE2D(
                            _FoamVisualOccupancy,
                            sampler_FoamCurrent,
                            foam.fieldUV).r);

                    if (foamDebug == 13)
                    {
                        return half4(instantaneousFilmTarget.xxx, 1.0);
                    }

                    if (foamDebug == 14)
                    {
                        return half4(temporalOccupancy.xxx, 1.0);
                    }

                    if (foamDebug == 15)
                    {
                        float retainedOccupancy = saturate(
                            (temporalOccupancy - instantaneousFilmTarget) * 4.0);
                        float pendingAcquisition = saturate(
                            (instantaneousFilmTarget - temporalOccupancy) * 4.0);
                        float3 temporalDifferenceColour = float3(
                            pendingAcquisition,
                            retainedOccupancy,
                            pendingAcquisition * 0.85);
                        return half4(
                            saturate(temporalDifferenceColour),
                            1.0);
                    }

                    if (foamDebug == 8)
                    {
                        float materialPresence = saturate(
                            SampleCommittedFoamState(foam.fieldUV).r);
                        float addedByShape = saturate(
                            (evaluatedShape - materialPresence) * 4.0);
                        float removedByShape = saturate(
                            (materialPresence - evaluatedShape) * 4.0);
                        float3 differenceColour = float3(
                            removedByShape + addedByShape * 0.10,
                            addedByShape,
                            removedByShape * 0.85);
                        return half4(saturate(differenceColour), 1.0);
                    }

                    if (foamDebug == 9 || foamDebug == 10)
                    {
                        float detailedShape =
                            RiverWaterFoamEvaluateShaderLocalDetailProbe(
                                evaluatedShape,
                                foam.presence,
                                foam.remainingLife,
                                foam.materialPattern,
                                foam.fieldUV,
                                input.domainData.x,
                                input.domainData.y,
                                _FoamRenderAdvectionSeconds,
                                _FoamSharpness,
                                foam.surfaceEnergy);

                        if (foamDebug == 10)
                        {
                            float addedByDetail = saturate(
                                (detailedShape - evaluatedShape) * 5.0);
                            float removedByDetail = saturate(
                                (evaluatedShape - detailedShape) * 5.0);
                            float3 detailDifferenceColour = float3(
                                removedByDetail + addedByDetail * 0.10,
                                addedByDetail,
                                removedByDetail * 0.85);
                            return half4(
                                saturate(detailDifferenceColour),
                                1.0);
                        }

                        return half4(detailedShape.xxx, 1.0);
                    }

                    return half4(evaluatedShape.xxx, 1.0);
                }

                if (foamDebug == 4)
                {
                    float4 committedState =
                        SampleCommittedFoamState(foam.fieldUV);
                    float committedPresence = saturate(committedState.x);
                    float committedRemainingLife =
                        committedPresence > 0.0001
                            ? saturate(
                                committedState.y /
                                committedPresence)
                            : 0.0;
                    float committedPresenceVisibility =
                        ResolveCommittedFoamPresenceVisibility(
                            committedPresence);
                    float lifeBrightness = lerp(
                        0.12,
                        1.0,
                        saturate(committedRemainingLife));
                    float life =
                        lifeBrightness * committedPresenceVisibility;
                    return half4(life.xxx, 1.0);
                }

                if (foamDebug == 3)
                {
                    float presence = saturate(
                        SampleCommittedFoamState(foam.fieldUV).r);
                    return half4(presence.xxx, 1.0);
                }

                if (foamDebug == 2)
                {
                    int2 birthDebugDimensions = int2(
                        max(1.0, _FoamBirthDebug_TexelSize.z),
                        max(1.0, _FoamBirthDebug_TexelSize.w));
                    int2 birthDebugCoordinate = clamp(
                        (int2)floor(
                            foam.fieldUV *
                            (float2)birthDebugDimensions),
                        int2(0, 0),
                        birthDebugDimensions - 1);
                    float4 birthDebug = saturate(
                        _FoamBirthDebug.Load(
                            int3(birthDebugCoordinate, 0)));
                    float3 debugColour =
                        birthDebug.b * float3(0.08, 0.20, 0.75) +
                        birthDebug.g * float3(0.05, 0.85, 0.15) +
                        birthDebug.r * float3(1.00, 0.08, 0.03);
                    debugColour = lerp(
                        debugColour,
                        float3(1.00, 0.95, 0.08),
                        birthDebug.a);
                    return half4(saturate(debugColour), 1.0);
                }

                if (foamDebug == 1)
                {
                    int2 structuralDimensions = int2(
                        max(1.0, _FoamObstacleExclusion_TexelSize.z),
                        max(1.0, _FoamObstacleExclusion_TexelSize.w));
                    int2 structuralCoordinate = clamp(
                        (int2)floor(
                            foam.fieldUV *
                            (float2)structuralDimensions),
                        int2(0, 0),
                        structuralDimensions - 1);
                    float4 topologyDebug = saturate(
                        _FoamTopology.Load(
                            int3(structuralCoordinate, 0)));
                    float4 anchoredSources = saturate(
                        _FoamTopologySources.Load(
                            int3(structuralCoordinate, 0)));
                    float obstacleFootprint = saturate(
                        _FoamObstacleExclusion.Load(
                            int3(structuralCoordinate, 0)).r);

                    float positiveSupport = max(
                        max(topologyDebug.r, topologyDebug.g),
                        max(
                            max(anchoredSources.r, anchoredSources.g),
                            anchoredSources.b));
                    float negativePressure = topologyDebug.b;

                    float3 topologyColour = float3(0.012, 0.016, 0.020);
                    topologyColour +=
                        positiveSupport * float3(0.04, 0.68, 0.08);
                    topologyColour +=
                        negativePressure * float3(0.88, 0.05, 0.02);
                    topologyColour = lerp(
                        saturate(topologyColour),
                        float3(0.05, 0.22, 0.95),
                        obstacleFootprint);

                    // Do not colour-code ordinary Foam by Remaining Life here.
                    // That made aging look like a teal/blue hue shift. Life-specific
                    // proof belongs in the later local-aging diagnostic.
                    float3 foamOverlay = float3(0.96, 0.98, 0.92);
                    float3 composite = lerp(
                        topologyColour,
                        foamOverlay,
                        saturate(smoothstep(0.08, 0.46, foam.mask) * 0.96));
                    return half4(saturate(composite), 1.0);
                }

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

                if (disturbanceDebug >= 6 && disturbanceDebug <= 8)
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

                    if (disturbanceDebug == 8)
                    {
                        float energy = saturate(wake.energy * 0.30);
                        return half4(energy, 0.0, 0.0, 1.0);
                    }

                    float4 wakeSource = SAMPLE_TEXTURE2D(
                        _DisturbanceStaticWakeSource,
                        sampler_DisturbanceStaticWakeSource,
                        fieldUV);
                    float release = saturate(wakeSource.r * 0.35);
                    float leeDepth =
                        RiverWaterResolveStaticWakeLeeDepth(wakeSource.g);
                    float lee = saturate(leeDepth / 0.200);
                    float reach = saturate(wakeSource.a);

                    if (disturbanceDebug == 7)
                    {
                        return half4(release, lee, reach, 1.0);
                    }
                }

                if (disturbanceDebug == 21)
                {
                    float2 fieldUV = float2(
                        saturate(
                            (input.domainData.x - _DisturbanceGlobalStart) /
                            max(0.001, _DisturbanceFieldLength)),
                        saturate(
                            input.domainData.y /
                            max(0.001, input.domainData.w) *
                            0.5 + 0.5));
                    float2 boundary = SAMPLE_TEXTURE2D(
                        _DisturbanceRippleBoundary,
                        sampler_DisturbanceRippleBoundary,
                        fieldUV).rg;
                    return half4(
                        saturate(boundary.y),
                        saturate(boundary.x),
                        0.0,
                        1.0);
                }

                if (disturbanceDebug == 19)
                {
                    RiverWaterStaticWakeLeeResult debugStaticWakeLee =
                        RiverWaterEvaluateStaticWakeLee(
                            TEXTURE2D_ARGS(
                                _DisturbanceStaticWakeSource,
                                sampler_DisturbanceStaticWakeSource),
                            _DisturbanceEnabled,
                            input.domainData.x,
                            input.domainData.y,
                            input.domainData.z,
                            input.domainData.w,
                            _DisturbanceGlobalStart,
                            _DisturbanceFieldLength,
                            _DisturbanceShoreInteraction,
                            _FreezeAmount,
                            _DisturbanceStaticWakeTexelSize.xy,
                            0.0);
                    float protectedTrailHeight =
                        RiverWaterResolveWakeGeometryHeight(
                            wake.geometryCore,
                            _DisturbanceWakeGeometryHeight,
                            debugStaticWakeLee.depth);
                    float finalWakeGeometryHeight =
                        protectedTrailHeight - debugStaticWakeLee.depth;
                    const float signedHeightScale = 0.400;
                    float encodedHeight = saturate(
                        finalWakeGeometryHeight /
                        (2.0 * signedHeightScale) + 0.5);
                    return half4(encodedHeight.xxx, 1.0);
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
