#ifndef PS3D_PIXELSURFACEGENERATEDMASSFEATURES_HLSL
#define PS3D_PIXELSURFACEGENERATEDMASSFEATURES_HLSL

            half3 ApplyGeneratedMassGeometryEdgeWearResponse(
                half3 albedo,
                Varyings input)
            {
                // EW-4 edge wear is carried by actual generated bevel/chamfer
                // faces through UV2.z. It intentionally does not sample the
                // temporary FeatureAtlas0/1 boundary diagnostics. Use a
                // dedicated GeneratedMass enable flag instead of SurfaceContract
                // so the final response follows the same UV2.z mask validated
                // by Surface Mask Debug = ConvexEdgeWear.
                float faceMask =
                    saturate(input.materialMasks.z) *
                    saturate(_GeneratedMassGeometryEdgeWearEnabled);
                float softness = saturate(_GeneratedMassEdgeWearSoftness);
                float responseSoftening = lerp(1.0, 0.72, softness);
                float edgeWearMask =
                    faceMask *
                    saturate(_GeneratedMassEdgeWearResponseStrength) *
                    responseSoftening;
                if (edgeWearMask <= 0.0001)
                {
                    return albedo;
                }

                half lift =
                    (half)(_GeneratedMassEdgeWearBrightnessLift * edgeWearMask * lerp(1.0, 0.82, softness));
                half3 lifted = saturate(albedo + lift * 0.58h);
                half3 tinted = PS3D_ApplyValuePreservingTint(
                    lifted,
                    _GeneratedMassEdgeWearTint.rgb,
                    _GeneratedMassEdgeWearTintStrength * responseSoftening);
                return lerp(albedo, tinted, (half)edgeWearMask);
            }

            float4 ResolveGeneratedMassFeatureAtlas0(Varyings input)
            {
                if (_GeneratedMassFeatureAtlas0Enabled < 0.5)
                {
                    return float4(0.0, 0.0, 0.0, 0.0);
                }

                float2 atlasUV = saturate(input.featureAtlasUV);
                return saturate(
                    SAMPLE_TEXTURE2D(
                        _GeneratedMassFeatureAtlas0,
                        sampler_GeneratedMassFeatureAtlas0,
                        atlasUV));
            }

            float4 ResolveGeneratedMassFeatureAtlas1(Varyings input)
            {
                if (_GeneratedMassFeatureAtlas1Enabled < 0.5)
                {
                    return float4(0.0, 0.0, 0.0, 0.0);
                }

                float2 atlasUV = saturate(input.featureAtlasUV);
                return saturate(
                    SAMPLE_TEXTURE2D(
                        _GeneratedMassFeatureAtlas1,
                        sampler_GeneratedMassFeatureAtlas1,
                        atlasUV));
            }

            float ResolveGeneratedMassAtlasEdgeWearMask(Varyings input)
            {
                float4 atlas0 = ResolveGeneratedMassFeatureAtlas0(input);
                return saturate(atlas0.r);
            }

            float ResolveGeneratedMassAtlasCreaseMask(Varyings input)
            {
                float4 atlas0 = ResolveGeneratedMassFeatureAtlas0(input);
                return saturate(atlas0.g);
            }
#endif // PS3D_PIXELSURFACEGENERATEDMASSFEATURES_HLSL
