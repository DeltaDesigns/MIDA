using Tiger.Exporters;
using Tiger.Schema.Shaders;

namespace Tiger.Schema;

public class ShadowingLights : Tag<SMapShadowingLight>
{
    public TfxFeatureRenderer FeatureType = TfxFeatureRenderer.DeferredLights;
    public MapTransform Transfrom { get; set; }
    public ShadowingLights(FileHash hash) : base(hash)
    {
    }

    public void LoadIntoExporter()
    {
        var data = _tag.BufferData2 is null ? _tag.BufferData : _tag.BufferData2;
        if (data is null)
            return;

        List<Vec4> possibleColors = data.TagData.Buffer1.ToList();
        possibleColors.AddRange(data.TagData.Buffer2.ToList());

        Vector4 color = GetColor(data);
        Vector2 size = GetSize();
        Texture cookie = null;

        Material shading = FileResourcer.Get().GetFile<Material>(_tag.Shading);
        if (shading.Pixel.EnumerateTextures().Any())
        {
            cookie = shading.Pixel.EnumerateTextures().First().Texture;
        }

        Lights.LightData lightData = new()
        {
            Hash = data.Hash,
            Material = shading.Hash,
            LightType = Lights.LightType.Shadowing,
            Color = color,
            Size = new Vector3(_tag.HalfFOV * 2.0f, size.Y, 1f),
            Attenuation = 1, // Don't know
            Transform = new()
            {
                Position = Transfrom.Translation.ToVec3(),
                Quaternion = Transfrom.Rotation
            },
            Cookie = cookie != null ? cookie.Hash : null,
            Bytecode = data.TagData.Bytecode.Select(x => x.Value).ToArray(),
            BytecodeConstants = data.TagData.Buffer1.Select(x => x.Vec).ToArray()
        };

        Exporter.Get().GetGlobalScene().AddToGlobalScene(lightData);
    }

    public Vector4 GetColor(Tag<S80808691> data)
    {
        //if (Strategy.IsD1() && data.TagData.Buffer2.Count != 0 && !data.TagData.Buffer2[2].Vec.IsZero())
        //{
        //    return data.TagData.Buffer2[2].Vec; // Almost always color in D1?
        //}

        if (data.TagData.Bytecode.Count != 0)
        {
            return data.TagData.Buffer1.Find(x => x.Vec != Vector4.Zero).Vec;
        }
        else if (data.TagData.Buffer2.Count(x => x.Vec.Magnitude != 0) == 2)
        {
            var sorted = data.TagData.Buffer2.OrderByDescending(v => v.Vec.Magnitude).ToList();
            return sorted[0].Vec;// * sorted[1].Vec;
        }
        else
        {
            List<Vec4> possibleColors = data.TagData.Buffer1.ToList();
            possibleColors.AddRange(data.TagData.Buffer2.ToList());
            return possibleColors.Count == 0 ? Vector4.Zero : possibleColors.MaxBy(v => v.Vec.Magnitude).Vec;
        }
    }

    public Vector2 GetSize()
    {
        Matrix4x4 matrix = _tag.LightToWorld;

        // 2x2x2 Cube
        Vector3[] cubePoints = new Vector3[] {
            new Vector3(-1f, -1f, -1f),
            new Vector3(-1f, -1f, 1f),
            new Vector3(-1f, 1f, -1f),
            new Vector3(-1f, 1f, 1f),
            new Vector3(1f, -1f, -1f),
            new Vector3(1f, -1f, 1f),
            new Vector3(1f, 1f, -1f),
            new Vector3(1f, 1f, 1f)
        };

        for (int i = 0; i < cubePoints.Length; i++)
        {
            Vector4 r0;

            //r0.xyzw = cb0[19].xyzw * v0.yyyy;
            r0 = matrix.Y_Axis * new Vector4(cubePoints[i].Y);

            //r0.xyzw = cb0[18].xyzw * v0.xxxx + r0.xyzw;
            r0 = matrix.X_Axis * new Vector4(cubePoints[i].X) + r0;

            //r0.xyzw = cb0[20].xyzw * v0.zzzz + r0.xyzw;
            r0 = matrix.Z_Axis * new Vector4(cubePoints[i].Z) + r0;

            //o0.xyzw = cb0[21].xyzw + r0.xyzw;
            var b = (matrix.W_Axis + r0);

            cubePoints[i] = (b / new Vector4(b.W)).ToVec3();
        }

        float baseWH = cubePoints[1].Y * 2f; // Width of the base
        float coneHeight = cubePoints[1].X - cubePoints[0].X;
        float radianFOV = MathF.Atan((baseWH / 2) / coneHeight) * 2;
        return new(radianFOV, coneHeight);
    }
}

/// <summary>
/// Map Shadowing Light (Casts shadows)
/// </summary>
[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "44858080", 0x20)]
public struct SMapShadowingLightResource
{
    [SchemaField(0x10, TigerStrategy.MARATHON_ALPHA), NoLoad]
    public ShadowingLights ShadowingLight;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "80808557", 0x120)]
public struct SMapShadowingLight
{
    [SchemaField(0x60, TigerStrategy.MARATHON_ALPHA)]
    public Matrix4x4 LightToWorld;
    public Vector4 Distance; // Unsure but only W is used?

    [SchemaField(0xC0, TigerStrategy.MARATHON_ALPHA)]
    public float FarPlane;
    public float HalfFOV; // * 2, radians->degrees

    [SchemaField(0xD0, TigerStrategy.MARATHON_ALPHA)]
    public FileHash Shading;
    //public Material Shading_Shadowing;
    //public Material Volumetric;
    //public Material Volumetric_Shadowing;
    //public Material Lightprobe;
    //public Material Lightprobe_Shadowing;

    [SchemaField(0xE8, TigerStrategy.MARATHON_ALPHA)]
    public Tag<S80808691> BufferData;
    public Tag<S80808691> BufferData2;
}
