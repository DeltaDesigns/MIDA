using Tiger.Exporters;
using Tiger.Schema.Entity;

namespace Tiger.Schema;

public class SkyObjects : Tag<SMapSkyObjects>
{
    public TfxFeatureRenderer FeatureType = TfxFeatureRenderer.SkyTransparent;
    public SkyObjects(FileHash hash) : base(hash)
    {

    }

    public void LoadIntoExporter(ExporterScene scene)
    {
        var _config = ConfigSubsystem.Get();

        if (_tag.Entries is null)
            return;

        foreach ((int i, var element) in _tag.Entries.Select((value, index) => (index, value)))
        {
            if (element.Model.TagData.Model is null || element.Unk70 == 5)
                continue;

            Matrix4x4 matrix = element.Transform;

            Vector3 scale = new();
            Vector4 trans = new();
            Vector4 quat = new();
            matrix.Decompose(out trans, out quat, out scale);

            scene.AddMapModel(element.Model.TagData.Model, new Transform
            {
                Position = trans.ToVec3(),
                Rotation = Vector4.QuaternionToEulerAngles(quat),
                Quaternion = quat,
                Scale = scale,
                Order = element.Unk68
            });

            foreach (DynamicMeshPart part in element.Model.TagData.Model.Load(ExportDetailLevel.MostDetailed, null))
            {
                if (part.Material == null) continue;
                part.Material.RenderStage = TfxRenderStage.Transparents;
                scene.Materials.Add(new ExportMaterial(part.Material));
            }
        }
    }
}

/// </summary>
/// Background entities/skybox resource
/// </summary>
[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "78838080", 0x18)]
public struct SMapSkyObjectsResource
{
    [SchemaField(0x10, TigerStrategy.MARATHON_ALPHA), NoLoad]
    public SkyObjects SkyObjects;
}

/// <summary>
/// Background entities/skybox
/// </summary>
[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "8080837C", 0x60)]
public struct SMapSkyObjects
{
    public long FileSize;
    public DynamicArray<S7E838080> Entries;
    //public DynamicArray<SB3938080> Unk18;
    //public DynamicArray<SInt32> Unk28;
    [SchemaField(0x40)]
    public Vector4 Unk40;
    public Vector4 Unk50;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "7E838080", 0x90)]
public struct S7E838080
{
    public Matrix4x4 Transform;
    public AABB Bounds;
    public Tag<S80808383> Model;
    public float Unk68; // Ordering?

    [SchemaField(0x70, TigerStrategy.MARATHON_ALPHA)]
    public int Unk70; // if 5, skip the model??

    [SchemaField(0x7C, TigerStrategy.MARATHON_ALPHA)]
    public Tag<S8080ACD0> Complex;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "8080ACD0", 0x1C)]
public struct S8080ACD0
{
    public long FileSize;
    public int Unk08;
    public float Unk0C;
    public ResourcePointer Pointer; // 3FB18080
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "3FB18080", 0x70)]
public struct S3FB18080
{
    [SchemaField(0x10)]
    public DynamicArray<SInt16> Unk00;
    public DynamicArray<SInt16> Unk10;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "80808383", 0x10)]
public struct S80808383
{
    public long FileSize;
    public EntityModel Model;
}
