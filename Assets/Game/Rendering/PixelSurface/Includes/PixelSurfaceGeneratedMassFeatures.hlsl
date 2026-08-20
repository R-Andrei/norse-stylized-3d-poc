#ifndef PS3D_PIXELSURFACEGENERATEDMASSFEATURES_HLSL
#define PS3D_PIXELSURFACEGENERATEDMASSFEATURES_HLSL

            float ResolveGeneratedMassStructuralTypeMatch(
                float encodedType,
                float expectedType)
            {
                return step(
                    0.5,
                    1.0 - abs(encodedType - expectedType));
            }

            float ResolveGeneratedMassStructuralFeatureStrength(
                float4 structuralFeatures,
                float expectedType)
            {
                float primary =
                    ResolveGeneratedMassStructuralTypeMatch(
                        structuralFeatures.x,
                        expectedType) *
                    saturate(structuralFeatures.y);
                float secondary =
                    ResolveGeneratedMassStructuralTypeMatch(
                        structuralFeatures.z,
                        expectedType) *
                    saturate(structuralFeatures.w);
                return saturate(max(primary, secondary));
            }

            float ResolveGeneratedMassStructuralFeatureMembership(
                float4 structuralFeatures,
                float expectedType)
            {
                float primaryStrength = saturate(structuralFeatures.y);
                float secondaryStrength = saturate(structuralFeatures.w);
                float primary =
                    ResolveGeneratedMassStructuralTypeMatch(
                        structuralFeatures.x,
                        expectedType) *
                    (primaryStrength > 0.0001 ? 1.0 : 0.0);
                float secondary =
                    ResolveGeneratedMassStructuralTypeMatch(
                        structuralFeatures.z,
                        expectedType) *
                    (secondaryStrength > 0.0001 ? 1.0 : 0.0);
                return saturate(max(primary, secondary));
            }

            void ResolveGeneratedMassStructuralMaterialResponse(
                float4 structuralFeatures,
                out float convexResponse,
                out float chipResponse)
            {
                float generatedMassSurface =
                    1.0 - ResolveSurfaceContractIsGround();
                float convexMembership =
                    ResolveGeneratedMassStructuralFeatureMembership(
                        structuralFeatures,
                        1.0); // ConvexBoundary
                float chipMembership =
                    ResolveGeneratedMassStructuralFeatureMembership(
                        structuralFeatures,
                        3.0); // CornerChipCap

                // GM-SURFACE.6A.3: packed semantic strength proves transport
                // and remains available to raw diagnostics, but material
                // response treats ConvexBoundary/CornerChipCap as membership
                // classes. The object-level masters own artistic magnitude.
                convexResponse =
                    generatedMassSurface *
                    convexMembership *
                    saturate(_GeneratedMassEdgeWearResponseStrength);
                chipResponse =
                    generatedMassSurface *
                    chipMembership *
                    saturate(_GeneratedMassChipInteriorResponse);
            }

            half3 ResolveGeneratedMassStructuralDiagnosticColor(
                float4 structuralFeatures)
            {
                // GM-SURFACE.6A.2 temporary transport diagnostics. Values 29/30
                // are Generated-Mass-only Surface Debug modes. Ground and all
                // other debug/production modes bypass this helper exactly.
                int debugMode = (int)round(_MaskDebugMode);
                if ((debugMode != 29 && debugMode != 30) ||
                    ResolveSurfaceContractIsGround() > 0.5)
                {
                    return half3(-1.0h, -1.0h, -1.0h);
                }

                float convexStrength;
                float chipStrength;
                if (debugMode == 29)
                {
                    convexStrength =
                        ResolveGeneratedMassStructuralFeatureStrength(
                            structuralFeatures,
                            1.0); // ConvexBoundary
                    chipStrength =
                        ResolveGeneratedMassStructuralFeatureStrength(
                            structuralFeatures,
                            3.0); // CornerChipCap
                }
                else
                {
                    ResolveGeneratedMassStructuralMaterialResponse(
                        structuralFeatures,
                        convexStrength,
                        chipStrength);
                }

                half3 convexColor = half3(1.0h, 0.85h, 0.05h);
                half3 chipColor = half3(0.05h, 1.0h, 1.0h);
                return saturate(
                    (half)convexStrength * convexColor +
                    (half)chipStrength * chipColor);
            }

            float ResolveGeneratedMassStructuralVariationStrength(
                float4 structuralFeatures)
            {
                float convexResponse;
                float chipResponse;
                ResolveGeneratedMassStructuralMaterialResponse(
                    structuralFeatures,
                    convexResponse,
                    chipResponse);

                // GM-SURFACE.6A.4: variation is authored as an absolute
                // zero-mean tonal amplitude instead of multiplying whatever
                // base breakup survived the 5R convex restrictions. Reuse the
                // already-computed pixel-cell variation in the forward pass;
                // one authored strength unit equals 0.10 tonal amplitude.
                float convexVariationStrength =
                    clamp(
                        _GeneratedMassConvexVariationStrength,
                        0.0,
                        2.0) *
                    0.10;
                float chipVariationStrength =
                    clamp(
                        _GeneratedMassChipVariationStrength,
                        0.0,
                        3.0) *
                    0.10;
                return max(
                    convexResponse * convexVariationStrength,
                    chipResponse * chipVariationStrength);
            }

            half ResolveGeneratedMassStructuralSmoothnessOffset(
                float4 structuralFeatures)
            {
                float convexResponse;
                float chipResponse;
                ResolveGeneratedMassStructuralMaterialResponse(
                    structuralFeatures,
                    convexResponse,
                    chipResponse);

                float convexSmoothnessOffset =
                    clamp(
                        _GeneratedMassConvexSmoothnessOffset,
                        -0.40,
                        0.40);
                float chipSmoothnessOffset =
                    clamp(
                        _GeneratedMassChipSmoothnessOffset,
                        -0.40,
                        0.40);
                return (half)(
                    convexResponse * convexSmoothnessOffset +
                    chipResponse * chipSmoothnessOffset);
            }

            half3 ApplyGeneratedMassGeometryEdgeWearResponse(
                half3 albedo,
                Varyings input)
            {
                // Historical UV2.z bevel albedo-lift/tint response. GM-SURFACE.6A
                // does not call this function: structural material response is
                // driven by the packed ConvexBoundary semantic instead. The old
                // brightness/tint property-block strengths remain hard-zero.
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
