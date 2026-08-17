using Arithmic;

namespace Tiger;

public enum TfxExtern : byte
{
    None,
    Frame,
    View,
    Deferred,
    DeferredLight,
    DeferredUberLight,
    DeferredShadow,
    Atmosphere,
    RigidModel,
    EditorMesh,
    EditorMeshMaterial,
    EditorDecal,
    EditorTerrain,
    EditorTerrainPatch,
    EditorTerrainDebug,
    SimpleGeometry,
    UiFont,
    CuiView,
    CuiObject,
    CuiBitmap,
    CuiVideo,
    CuiStandard,
    CuiHud,
    CuiScreenspaceBoxes,
    CuiDrawingShader,
    TextureVisualizer,
    Generic,
    Particle,
    ParticleDebug,
    GearDyeVisualizationMode,
    ScreenArea,
    Mlaa,
    Msaa,
    Hdao,
    DownsampleTextureGeneric,
    DownsampleDepth,
    Ssao,
    VolumetricObscurance,
    Postprocess,
    TextureSet,
    Transparent,
    Vignette,
    GlobalLighting,
    ShadowMask,
    ObjectEffect,
    Decal,
    DecalSetTransform,
    DynamicDecal,
    DecoratorWind,
    TextureCameraLighting,
    VolumeFog,
    Fxaa,
    Smaa,
    Cmaa,
    Letterbox,
    DepthOfField,
    PostprocessInitialDownsample,
    CopyDepth,
    DisplacementMotionBlur,
    DebugShader,
    MinmaxDepth,
    SdsmBiasAndScale,
    SdsmBiasAndScaleTextures,
    ComputeShadowMapData,
    ComputeLocalLightShadowMapData,
    BilateralUpsample,
    HealthOverlay,
    LightProbeDominantLight,
    LightProbeLightInstance,
    Water,
    LensFlare,
    ScreenShader,
    Scaler,
    GammaControl,
    SpeedtreePlacements,
    Reticle,
    Distortion,
    WaterDebug,
    ScreenAreaInput,
    WaterDepthPrepass,
    OverheadVisibilityMapMain,
    OverheadVisibilityMapInterior,
    OverheadVisibilityMapOcclusion,
    ParticleCompute,
    CubemapFiltering,
    CubemapDiffuseCapture,
    ParticleFastpath,
    VolumetricsPass,
    TemporalReprojection,
    FxaaCompute,
    UberDepth,
    GearDye,
    Cubemaps,
    ShadowBlendWithPrevious,
    DebugShadingOutput,
    Ssao3D,
    WaterDisplacement,
    PatternBlending,
    UiHdrTransform,
    PlayerCenteredCascadedGrid,
    SoftDeform,
    RaymarchedAtmosphereVolume,
    Gtao,
    Taa,
    FirstPersonShadows,
    SkinningCompute,
    Ssr,
    StylizedDropShadow,
    SsrTrace,
    TextureCameraDownsample,
    WaterReflection,
    OcclusionCullingReprojectDepth,
    OcclusionCullingVisibilityTesting,
    VariableRateShading,
}

public static class Externs
{

    /// <summary>
    /// Remaps the given byte value to the correct Tfx extern depending on the current version.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="InvalidCastException"></exception>
    public static TfxExtern GetExtern(byte value)
    {
        return (TfxExtern)value;

        string name = ((TfxExtern)value).ToString();
        if (Enum.TryParse(name, out TfxExtern result))
            return result;

        throw new InvalidCastException($"Couldn't cast extern value {value} ({(((TfxExtern)value).ToString())}) for {Strategy.CurrentStrategy}");
    }

    public static string GetExternFloat(TfxExtern extern_, int element, bool bInline = false)
    {
        string InlineOrDefault(string name, string defaultValue) =>
                    bInline ? $"float4({name}.xxxx)" : $"(exists({name}) ? ({name}) : ({defaultValue}))";

        static string HandleUnknownElement(int element, TfxExtern extern_)
        {
            Log.Warning($"Unimplemented element {element} (0x{element:X}) for extern {extern_}");
            return "float4(1,1,1,1)";
        }

        switch (extern_)
        {
            case TfxExtern.Frame:
                return element switch
                {
                    // Not using inline method for time since its an engine provided value
                    0 => bInline ? $"float4(CurrentTime.xxxx)" : "(Time)", // game_time
                    0x04 => bInline ? $"float4(CurrentTime.xxxx)" : "(Time)", // render_time
                    0x10 => InlineOrDefault("FrameTimeOfDay", "0.5"),
                    0x14 => "float4(0.05.xxxx)", // delta_game_time
                    0x18 => "float4(0.016.xxxx)", // exposure_time
                    0x1C => InlineOrDefault("ExposureScale", "0.65"), // exposure_scale
                    0x28 => InlineOrDefault("ExposureIllumRelative", "1"), // exposure_illum_relative
                    _ => HandleUnknownElement(element, extern_)
                };

            case TfxExtern.Atmosphere:
                return element switch
                {
                    0x70 => InlineOrDefault("AtmosTimeOfDay", "0.5"),
                    0x1b4 => InlineOrDefault("AtmosRotation", "0"),
                    0x1b8 => InlineOrDefault("AtmosIntensity", "1"),
                    0x1e4 => InlineOrDefault("AtmosSunIntensity", "0.05923"),
                    0x198 or 0x170 => "float4(0.0001,0.0001,0.0001,0.0001)",
                    0x1bc => "float4(0.5,0.5,0.5,0.5)",
                    0x1e8 => "float4(0,0,0,0)",
                    _ => HandleUnknownElement(element, extern_)
                };

            default:
                Log.Warning($"Unimplemented extern {extern_}[{element} (0x{(element):X})]");
                return $"float4(1,1,1,1)";
        }
    }

    public static string GetExternVec4(TfxExtern extern_, int element, bool bInline = false)
    {
        static string HandleUnknownElement(int element, TfxExtern extern_)
        {
            Log.Warning($"Unimplemented element {element} (0x{element:X}) for extern {extern_}");
            return "float4(1,1,1,1)";
        }

        string InlineOrDefault(string name, string defaultValue) =>
            bInline ? $"{name}" : $"(exists({name}) ? ({name}) : ({defaultValue}))";

        switch (extern_)
        {
            case TfxExtern.Deferred:
                return element switch
                {
                    0 => "float4(0.0, 100, 0.0, 0.0)",
                    _ => HandleUnknownElement(element, extern_)
                };

            case TfxExtern.Frame:
                return element switch
                {
                    0x1A0 => "float4(0, 0, 0, 0)",
                    0x1C0 => "float4(1, 1, 0, 1)",
                    _ => HandleUnknownElement(element, extern_)
                };

            case TfxExtern.Atmosphere:
                return element switch
                {
                    0xD0 => "float4(512.0, 512.0, 1.0 / 512.0, 1.0 / 512.0)",
                    0x90 => InlineOrDefault("AtmosRTDimensions", "float4(480.0, 270.0, 0.00208, 0.0037)"),
                    0x110 => InlineOrDefault("AtmosSunDir", "float4(-0.30372, -0.59835, 0.74144, 0.0)"),
                    0x140 => InlineOrDefault("AtmosSunColor", "float4(1.0, 0.95, 0.85, 1.0)"),
                    0x1D0 => "float4(0,0,0,0)",
                    _ => HandleUnknownElement(element, extern_)
                };

            case TfxExtern.Decal:
                return element switch
                {
                    0x10 => "float4(0, 100, 0, 0)",
                    0x20 => "float4(0.03, 0, 0, 0)",
                    _ => HandleUnknownElement(element, extern_)
                };
            case TfxExtern.DecalSetTransform:
                return element switch
                {
                    0x0 => "float4(0, 0, 0, 1)",
                    0x10 => "float4(0, 0, 0, 1)",
                    _ => HandleUnknownElement(element, extern_)
                };

            default:
                Log.Warning($"Unimplemented extern {extern_}[{element} (0x{(element):X})]");
                return $"float4(1, 1, 1, 1)";
        }
    }

    // TODO, make this return just the texture attribute name maybe?
    public static string GetExternTexture(TfxExtern extern_, int element, bool bInline = false)
    {
        static string HandleUnknownElement(int element, TfxExtern extern_)
        {
            Log.Warning($"Unimplemented element {element} (0x{element:X}) for extern {extern_}");
            return "float4(1,1,1,1)";
        }

        switch (extern_)
        {
            case TfxExtern.Deferred:
                return element switch
                {

                    _ => HandleUnknownElement(element, extern_)
                };

            case TfxExtern.Frame:
                return element switch
                {

                    _ => HandleUnknownElement(element, extern_)
                };

            case TfxExtern.Atmosphere:
                return element switch
                {

                    _ => HandleUnknownElement(element, extern_)
                };

            case TfxExtern.Decal:
                return element switch
                {

                    _ => HandleUnknownElement(element, extern_)
                };

            default:
                Log.Warning($"Unimplemented extern {extern_}[{element} (0x{(element):X})]");
                return $"float4(1, 1, 1, 1)";
        }
    }
}
