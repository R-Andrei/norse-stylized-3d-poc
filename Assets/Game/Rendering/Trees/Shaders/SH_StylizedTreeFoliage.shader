Shader "PS3D/Trees/Stylized Tree Foliage"
{
    Properties
    {
        [MainTexture] _BaseMap("Foliage Texture", 2D) = "white" {}
        [MainColor] _BaseColor("Foliage Tint", Color) = (1, 1, 1, 1)
        _Cutoff("Alpha Cutoff", Range(0, 1)) = 0.5
        _NormalUpBias("Normal Up Bias", Range(0, 1)) = 0.28
        _Smoothness("Smoothness", Range(0, 1)) = 0.08
        _SpecularColor("Specular", Color) = (0.10, 0.10, 0.10, 1)

        [Header(Foliage Readability)]
        _CanopyDepthStrength("Canopy Depth Strength", Range(0, 0.5)) = 0.16
        _CanopyDepthPower("Canopy Depth Power", Range(0.05, 4)) = 1.0
        _OrientationContrast("Orientation Contrast", Range(0, 1)) = 0.55
        _OrientationReadability("Orientation Readability", Range(0, 1)) = 0.35
        _UndersideDarkening("Underside Darkening", Range(0, 0.6)) = 0.14
        _ClusterVariationStrength("Cluster Variation Strength", Range(0, 0.3)) = 0.06
        _ClusterVariationScale("Cluster Variation Scale", Range(0.25, 4)) = 1.35
        _DiffuseWrap("Diffuse Wrap", Range(0, 1)) = 0.45

        [Header(Foliage Shadow Reception)]
        _ShadowReceiveStrength("Realtime Shadow Receive Strength", Range(0, 1)) = 0.65
        _ShadowFloor("Realtime Shadow Floor", Range(0, 1)) = 0.38

        [Header(Foliage Diagnostics)]
        [Enum(ProgrammaticStylized3D.Trees.TreeFoliageDebugMode)]
        _FoliageDebugMode("Foliage Debug Mode", Float) = 0

        [HideInInspector] _TreeWindEnabled("Tree Wind Enabled", Float) = 1
        [HideInInspector] _TreeWindMaskMode("Tree Wind Mask Mode", Float) = 0
        [HideInInspector] _TreeBoundsMinY("Tree Bounds Minimum Y", Float) = 0
        [HideInInspector] _TreeBoundsHeight("Tree Bounds Height", Float) = 1
        [HideInInspector] _TreeRootPositionOS("Tree Root Position OS", Vector) = (0, 0, 0, 0)
        [HideInInspector] _TreeStiffness("Tree Stiffness", Range(0, 1)) = 0.5
        [HideInInspector] _TreeMacroWindStrength("Tree Macro Wind Strength", Range(0, 2)) = 0.5
        [HideInInspector] _TreeFoliageFlutterStrength("Tree Foliage Flutter Strength", Range(0, 0.2)) = 0.03
        [HideInInspector] _TreePhase("Tree Phase", Float) = 0
        [HideInInspector] _TreeDebugMode("Tree Debug Mode", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "TransparentCutout"
            "Queue" = "AlphaTest"
        }

        HLSLINCLUDE
        #define _SPECULAR_SETUP 1

        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "../Includes/TreeCommon.hlsl"
        #include "../Includes/TreeWindResponse.hlsl"
        #include "../Includes/TreeLighting.hlsl"
        #include "../Includes/TreeFoliageLighting.hlsl"

        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            half4 _BaseColor;
            half4 _SpecularColor;
            float _Cutoff;
            float _NormalUpBias;
            float _Smoothness;
            float _CanopyDepthStrength;
            float _CanopyDepthPower;
            float _OrientationContrast;
            float _OrientationReadability;
            float _UndersideDarkening;
            float _ClusterVariationStrength;
            float _ClusterVariationScale;
            float _DiffuseWrap;
            float _ShadowReceiveStrength;
            float _ShadowFloor;
            float _FoliageDebugMode;
            float _TreeWindEnabled;
            float _TreeWindMaskMode;
            float _TreeBoundsMinY;
            float _TreeBoundsHeight;
            float4 _TreeRootPositionOS;
            float _TreeStiffness;
            float _TreeMacroWindStrength;
            float _TreeFoliageFlutterStrength;
            float _TreePhase;
            float _TreeDebugMode;
        CBUFFER_END

        float3 _LightDirection;
        float3 _LightPosition;

        struct TreeFoliageAttributes
        {
            float4 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float4 tangentOS : TANGENT;
            float2 uv : TEXCOORD0;
            float4 colour : COLOR;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct TreeFoliageVertexData
        {
            float3 positionWS;
            float3 normalWS;
            float3 tangentWS;
            float heightMask;
            float windMask;
            float flutterPhase;
            float clusterVariation;
        };

        TreeFoliageVertexData EvaluateTreeFoliageVertex(
            TreeFoliageAttributes input)
        {
            TreeFoliageVertexData output;
            float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
            float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
            float3 tangentWS = TransformObjectToWorldDir(
                input.tangentOS.xyz);

            TreeWindVertexResult wind = ApplyTreeWindResponse(
                input.positionOS.xyz,
                positionWS,
                normalWS,
                tangentWS,
                input.colour,
                TransformObjectToWorld(_TreeRootPositionOS.xyz),
                _TreeBoundsMinY,
                _TreeBoundsHeight,
                _TreeWindEnabled,
                _TreeWindMaskMode,
                _TreeStiffness,
                _TreeMacroWindStrength,
                _TreeFoliageFlutterStrength,
                _TreePhase,
                1.0);

            output.positionWS = wind.positionWS;
            output.normalWS = wind.normalWS;
            output.tangentWS = wind.tangentWS;
            output.heightMask = wind.heightMask;
            output.windMask = wind.windMask;
            output.flutterPhase = wind.flutterPhase;
            float3 clusterCell = floor(
                input.positionOS.xyz /
                max(0.25, _ClusterVariationScale));
            output.clusterVariation = TreeHash31(
                clusterCell + _TreePhase * 0.173);
            return output;
        }
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Cull Off
            ZWrite On
            ZTest LEqual
            Blend One Zero

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _FORWARD_PLUS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _LIGHT_COOKIES

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
                float4 colour : TEXCOORD3;
                float heightMask : TEXCOORD4;
                float windMask : TEXCOORD5;
                float flutterPhase : TEXCOORD6;
                float fogFactor : TEXCOORD7;
                float clusterVariation : TEXCOORD8;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings Vert(TreeFoliageAttributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                TreeFoliageVertexData tree = EvaluateTreeFoliageVertex(input);
                output.positionCS = TransformWorldToHClip(tree.positionWS);
                output.positionWS = tree.positionWS;
                output.normalWS = tree.normalWS;
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.colour = input.colour;
                output.heightMask = tree.heightMask;
                output.windMask = tree.windMask;
                output.flutterPhase = tree.flutterPhase;
                output.clusterVariation = tree.clusterVariation;
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half4 Frag(
                Varyings input,
                FRONT_FACE_TYPE face : FRONT_FACE_SEMANTIC) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                half4 baseSample = SAMPLE_TEXTURE2D(
                    _BaseMap,
                    sampler_BaseMap,
                    input.uv) * _BaseColor;
                clip(baseSample.a - _Cutoff);

                float faceSign = IS_FRONT_VFACE(face, 1.0, -1.0);
                float3 rawNormalWS = normalize(input.normalWS * faceSign);
                float3 normalWS = normalize(lerp(
                    rawNormalWS,
                    float3(0.0, 1.0, 0.0),
                    saturate(_NormalUpBias)));

                float3 treeDebugColour = TreeResolveDebugColour(
                    _TreeDebugMode,
                    input.colour,
                    input.heightMask,
                    input.windMask,
                    input.flutterPhase,
                    normalWS);
                if (_TreeDebugMode >= 0.5)
                {
                    return half4(treeDebugColour, 1.0);
                }

                InputData inputData = BuildTreeInputData(
                    input.positionCS,
                    input.positionWS,
                    normalWS,
                    input.fogFactor);
                TreeFoliageLightingResult lighting =
                    TreeEvaluateFoliageLighting(
                        inputData,
                        normalWS,
                        _DiffuseWrap,
                        _OrientationContrast,
                        _ShadowReceiveStrength,
                        _ShadowFloor,
                        _SpecularColor.rgb,
                        _Smoothness);

                float canopyFactor = TreeResolveFoliageCanopyFactor(
                    input.heightMask,
                    _CanopyDepthStrength,
                    _CanopyDepthPower);
                float orientationFactor =
                    TreeResolveFoliageOrientationFactor(
                        rawNormalWS,
                        _OrientationReadability);
                float undersideFactor =
                    TreeResolveFoliageUndersideFactor(
                        faceSign,
                        _UndersideDarkening);
                float clusterFactor = TreeResolveFoliageClusterFactor(
                    input.clusterVariation,
                    _ClusterVariationStrength);
                float readabilityFactor =
                    canopyFactor *
                    orientationFactor *
                    undersideFactor *
                    clusterFactor;

                float3 foliageDebugColour =
                    TreeResolveFoliageDebugColour(
                        _FoliageDebugMode,
                        baseSample.rgb,
                        baseSample.a,
                        faceSign,
                        input.heightMask,
                        input.clusterVariation,
                        orientationFactor,
                        lighting);
                if (_FoliageDebugMode >= 0.5)
                {
                    return half4(foliageDebugColour, 1.0);
                }

                half4 colour = half4(
                    baseSample.rgb *
                        readabilityFactor *
                        lighting.combined,
                    1.0h);
                colour.rgb = MixFog(colour.rgb, input.fogFactor);
                return colour;
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            Cull Off
            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #pragma multi_compile_instancing
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            struct ShadowVaryings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            ShadowVaryings ShadowPassVertex(
                TreeFoliageAttributes input)
            {
                ShadowVaryings output = (ShadowVaryings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                TreeFoliageVertexData tree =
                    EvaluateTreeFoliageVertex(input);
                #if defined(_CASTING_PUNCTUAL_LIGHT_SHADOW)
                    float3 lightDirectionWS = SafeNormalize(
                        _LightPosition - tree.positionWS);
                #else
                    float3 lightDirectionWS = _LightDirection;
                #endif

                output.positionCS = TransformWorldToHClip(
                    ApplyShadowBias(
                        tree.positionWS,
                        tree.normalWS,
                        lightDirectionWS));
                #if UNITY_REVERSED_Z
                    output.positionCS.z = min(
                        output.positionCS.z,
                        UNITY_NEAR_CLIP_VALUE);
                #else
                    output.positionCS.z = max(
                        output.positionCS.z,
                        UNITY_NEAR_CLIP_VALUE);
                #endif
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                return output;
            }

            half4 ShadowPassFragment(ShadowVaryings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                half alpha = SAMPLE_TEXTURE2D(
                    _BaseMap,
                    sampler_BaseMap,
                    input.uv).a * _BaseColor.a;
                clip(alpha - _Cutoff);
                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            Cull Off
            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment
            #pragma multi_compile_instancing

            struct DepthVaryings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            DepthVaryings DepthOnlyVertex(
                TreeFoliageAttributes input)
            {
                DepthVaryings output = (DepthVaryings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                TreeFoliageVertexData tree =
                    EvaluateTreeFoliageVertex(input);
                output.positionCS = TransformWorldToHClip(tree.positionWS);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                return output;
            }

            half4 DepthOnlyFragment(DepthVaryings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                half alpha = SAMPLE_TEXTURE2D(
                    _BaseMap,
                    sampler_BaseMap,
                    input.uv).a * _BaseColor.a;
                clip(alpha - _Cutoff);
                return 0;
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Lit"
}
