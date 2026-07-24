Shader "PS3D/Trees/Stylized Tree Bark"
{
    Properties
    {
        [MainTexture] _BaseMap("Bark Albedo", 2D) = "white" {}
        [MainColor] _BaseColor("Bark Tint", Color) = (1, 1, 1, 1)
        [Normal] _BumpMap("Bark Normal", 2D) = "bump" {}
        _BumpScale("Normal Strength", Range(0, 2)) = 1
        _Smoothness("Smoothness", Range(0, 1)) = 0.18
        _SpecularColor("Specular", Color) = (0.16, 0.16, 0.16, 1)

        [HideInInspector] _TreeWindEnabled("Tree Wind Enabled", Float) = 1
        [HideInInspector] _TreeWindMaskMode("Tree Wind Mask Mode", Float) = 0
        [HideInInspector] _TreeBoundsMinY("Tree Bounds Minimum Y", Float) = 0
        [HideInInspector] _TreeBoundsHeight("Tree Bounds Height", Float) = 1
        [HideInInspector] _TreeRootPositionOS("Tree Root Position OS", Vector) = (0, 0, 0, 0)
        [HideInInspector] _TreeStiffness("Tree Stiffness", Range(0, 1)) = 0.5
        [HideInInspector] _TreeMacroWindStrength("Tree Macro Wind Strength", Range(0, 2)) = 0.5
        [HideInInspector] _TreeFoliageFlutterStrength("Tree Foliage Flutter Strength", Range(0, 0.2)) = 0
        [HideInInspector] _TreePhase("Tree Phase", Float) = 0
        [HideInInspector] _TreeDebugMode("Tree Debug Mode", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
        }

        HLSLINCLUDE
        #define _SPECULAR_SETUP 1

        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "../Includes/TreeCommon.hlsl"
        #include "../Includes/TreeWindResponse.hlsl"
        #include "../Includes/TreeLighting.hlsl"

        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);
        TEXTURE2D(_BumpMap);
        SAMPLER(sampler_BumpMap);

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            half4 _BaseColor;
            float4 _BumpMap_ST;
            half4 _SpecularColor;
            float _BumpScale;
            float _Smoothness;
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

        struct TreeBarkAttributes
        {
            float4 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float4 tangentOS : TANGENT;
            float2 uv : TEXCOORD0;
            float4 colour : COLOR;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct TreeBarkVertexData
        {
            float3 positionWS;
            float3 normalWS;
            float3 tangentWS;
            float tangentSign;
            float heightMask;
            float windMask;
            float flutterPhase;
        };

        TreeBarkVertexData EvaluateTreeBarkVertex(
            TreeBarkAttributes input)
        {
            TreeBarkVertexData output;
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
                0.0,
                _TreePhase,
                0.0);

            output.positionWS = wind.positionWS;
            output.normalWS = wind.normalWS;
            output.tangentWS = wind.tangentWS;
            output.tangentSign =
                input.tangentOS.w * GetOddNegativeScale();
            output.heightMask = wind.heightMask;
            output.windMask = wind.windMask;
            output.flutterPhase = wind.flutterPhase;
            return output;
        }
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Cull Back
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
                float4 tangentWS : TEXCOORD2;
                float2 uv : TEXCOORD3;
                float4 colour : TEXCOORD4;
                float heightMask : TEXCOORD5;
                float windMask : TEXCOORD6;
                float flutterPhase : TEXCOORD7;
                float fogFactor : TEXCOORD8;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings Vert(TreeBarkAttributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                TreeBarkVertexData tree = EvaluateTreeBarkVertex(input);
                output.positionCS = TransformWorldToHClip(tree.positionWS);
                output.positionWS = tree.positionWS;
                output.normalWS = tree.normalWS;
                output.tangentWS = float4(
                    tree.tangentWS,
                    tree.tangentSign);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.colour = input.colour;
                output.heightMask = tree.heightMask;
                output.windMask = tree.windMask;
                output.flutterPhase = tree.flutterPhase;
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float3 tangentWS = normalize(input.tangentWS.xyz);
                float3 baseNormalWS = normalize(input.normalWS);
                float3 bitangentWS = normalize(
                    cross(baseNormalWS, tangentWS) * input.tangentWS.w);
                half3 normalTS = UnpackNormalScale(
                    SAMPLE_TEXTURE2D(
                        _BumpMap,
                        sampler_BumpMap,
                        input.uv),
                    _BumpScale);
                float3 normalWS = normalize(
                    tangentWS * normalTS.x +
                    bitangentWS * normalTS.y +
                    baseNormalWS * normalTS.z);

                float3 debugColour = TreeResolveDebugColour(
                    _TreeDebugMode,
                    input.colour,
                    input.heightMask,
                    input.windMask,
                    input.flutterPhase,
                    normalWS);
                if (_TreeDebugMode >= 0.5)
                {
                    return half4(debugColour, 1.0);
                }

                half4 baseSample = SAMPLE_TEXTURE2D(
                    _BaseMap,
                    sampler_BaseMap,
                    input.uv) * _BaseColor;
                InputData inputData = BuildTreeInputData(
                    input.positionCS,
                    input.positionWS,
                    normalWS,
                    input.fogFactor);
                half4 colour = ShadeTreeSurface(
                    inputData,
                    baseSample.rgb,
                    _SpecularColor.rgb,
                    _Smoothness,
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

            Cull Back
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
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            ShadowVaryings ShadowPassVertex(TreeBarkAttributes input)
            {
                ShadowVaryings output = (ShadowVaryings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                TreeBarkVertexData tree = EvaluateTreeBarkVertex(input);
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
                return output;
            }

            half4 ShadowPassFragment(ShadowVaryings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            Cull Back
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
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            DepthVaryings DepthOnlyVertex(TreeBarkAttributes input)
            {
                DepthVaryings output = (DepthVaryings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                TreeBarkVertexData tree = EvaluateTreeBarkVertex(input);
                output.positionCS = TransformWorldToHClip(tree.positionWS);
                return output;
            }

            half4 DepthOnlyFragment(DepthVaryings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                return 0;
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Lit"
}
