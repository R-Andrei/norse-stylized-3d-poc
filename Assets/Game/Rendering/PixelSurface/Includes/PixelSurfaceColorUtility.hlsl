#ifndef PS3D_PIXELSURFACECOLORUTILITY_HLSL
#define PS3D_PIXELSURFACECOLORUTILITY_HLSL

            half PS3D_MaskTintLuminance(half3 color)
            {
                return dot(color, half3(0.2126h, 0.7152h, 0.0722h));
            }

            half3 PS3D_ApplyValuePreservingTint(
                half3 neutralTarget,
                half3 tintColor,
                float tintStrength)
            {
                half strength = saturate((half)tintStrength);
                half targetLum = max(0.001h, PS3D_MaskTintLuminance(neutralTarget));
                half tintLum = max(0.001h, PS3D_MaskTintLuminance(tintColor));
                half3 hueTarget = tintColor * (targetLum / tintLum);
                return lerp(neutralTarget, hueTarget, strength);
            }
#endif // PS3D_PIXELSURFACECOLORUTILITY_HLSL
