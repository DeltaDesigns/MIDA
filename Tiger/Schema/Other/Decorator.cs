using Tiger.Exporters;
using Tiger.Schema.Entity;
using Tiger.Schema.Model;

namespace Tiger.Schema;

public class Decorator : Tag<SDecorator>
{
    public TfxFeatureRenderer FeatureType = TfxFeatureRenderer.SpeedtreeTrees;
    public Decorator(FileHash hash) : base(hash)
    {

    }

    public void LoadIntoExporter(ExporterScene scene, string saveDirectory)
    {
        var models = _tag.DecoratorModels;
        // Model transform offsets
        List<Vector4> SpeedtreePlacements = new() { Vector4.Zero, Vector4.Zero.WithW(1) };

        TigerFile container = new(_tag.BufferData.TagData.Unk14.Hash);
        byte[] containerData = container.GetData();
        for (int i = 0; i < containerData.Length / 16; i++)
        {
            SpeedtreePlacements.Add(containerData.Skip(i * 16).Take(16).ToArray().ToType<Vector4>());
        }

        using TigerReader reader = _tag.BufferData.TagData.InstanceBuffer.GetReferenceReader();
        for (int i = 0; i < _tag.InstanceRanges.Count - 1; i++)
        {
            var start = _tag.InstanceRanges[i].Value;
            var end = _tag.InstanceRanges[i + 1].Value;
            var count = end - start;

            var dynID = models.Count == 1 ? i : 0;
            var model = models[models.Count == 1 ? 0 : i].DecoratorModel;

            if (model.TagData.SpeedTreeData != null)
                continue; // TODO: Trees, skip for now

            var parts = GenerateParts(model.TagData.Model); //.Load(ExportDetailLevel.MostDetailed, null);
            foreach (DynamicMeshPart part in parts)
            {
                if (part.Material == null) continue;
                scene.Materials.Add(new ExportMaterial(part.Material));
            }

            for (int j = 0; j < count; j++)
            {
                reader.BaseStream.Seek((start + j) * 0x10, SeekOrigin.Begin);
                var pos = new Vector4(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16());
                var rot = new Vector4(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());

                Transform transform = new Transform
                {
                    Position = (SpeedtreePlacements[2] * pos + SpeedtreePlacements[3]).ToVec3(),
                    Quaternion = (SpeedtreePlacements[4] * rot + SpeedtreePlacements[5]),
                    Rotation = Vector4.QuaternionToEulerAngles((SpeedtreePlacements[4] * rot + SpeedtreePlacements[5])),
                    Scale = new((SpeedtreePlacements[2] * pos + SpeedtreePlacements[3]).W)
                };

                scene.AddMapModelParts($"{model.Hash}_{dynID}", parts.Where(x => x.GroupIndex == dynID).ToList(), transform);
            }

            // Trees need(?) their vertex shader to transform correctly...
            //if (model.TagData.SpeedTreeData != null)
            //{
            //    var scale = model.TagData.SpeedTreeData.TagData.Unk08[0].Value;
            //    var offset = model.TagData.SpeedTreeData.TagData.Unk08[0].Unk10;
            //    foreach (var part in parts)
            //    {
            //        for (int k = 0; k < part.VertexPositions.Count; k++)
            //        {
            //            part.VertexPositions[k] = new Vector4(
            //                part.VertexPositions[k].X * scale.X + offset.X,
            //                part.VertexPositions[k].Y * scale.Y + offset.Y,
            //                part.VertexPositions[k].Z * scale.Z + offset.Z,
            //                part.VertexPositions[k].W
            //            );
            //        }
            //    }

            //    var uvTransform = model.TagData.SpeedTreeData.TagData.Unk08[0].Unk20;
            //    foreach (var part in parts)
            //    {
            //        for (int k = 0; k < part.VertexTexcoords0.Count; k++)
            //        {
            //            part.VertexTexcoords0[k] = new Vector2(
            //                part.VertexTexcoords0[k].X * uvTransform.X + uvTransform.Z,
            //                part.VertexTexcoords0[k].Y * -uvTransform.Y + 1 - uvTransform.W
            //            );
            //        }
            //    }
            //}
        }
    }

    // Should just use EntityModel.Load but we need to get just the first mesh entry in Meshes since the rest are LODs
    private List<DynamicMeshPart> GenerateParts(EntityModel model)
    {
        var dynamicParts = GetPartsOfDetailLevel(model);
        List<DynamicMeshPart> parts = new();
        List<int> exportPartRange = new();
        if (model.TagData.Meshes.Count == 0) return parts;

        var mesh = model.TagData.Meshes[model.GetReader(), 0];
        exportPartRange = EntityModel.GetExportRanges(mesh);

        foreach ((int i, SD1878080 part) in dynamicParts[0])
        {
            if (!exportPartRange.Contains(i))
                continue;

            DynamicMeshPart dynamicMeshPart = new(part, null)
            {
                Index = i,
                GroupIndex = part.ExternalIdentifier,
                DetailLevel = part.DetailLevel,
                bAlphaClip = (part.Flags & 0x8) != 0,
                VertexLayoutIndex = mesh.GetInputLayoutForStage(0)
            };

            if (dynamicMeshPart.Material is null ||
            dynamicMeshPart.Material.Vertex.Shader is null ||
            dynamicMeshPart.Material.Pixel.Shader is null)
                continue;

            dynamicMeshPart.GetAllData(mesh, model.TagData);
            parts.Add(dynamicMeshPart);
        }

        return parts;
    }

    private Dictionary<int, Dictionary<int, SD1878080>> GetPartsOfDetailLevel(EntityModel model)
    {
        Dictionary<int, Dictionary<int, SD1878080>> parts = new();
        using TigerReader reader = model.GetReader();

        int meshIndex = 0;
        int partIndex = 0;
        var mesh = model.TagData.Meshes[reader, 0];

        parts.Add(meshIndex, new Dictionary<int, SD1878080>());
        for (int i = 0; i < mesh.Parts.Count; i++)
        {
            SD1878080 part = mesh.Parts[reader, i];
            if (part.DetailLevel.IsHighestLevel())
                parts[meshIndex].Add(partIndex, part);

            partIndex++;
        }

        return parts;
    }

}

#region Decorator structs
[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "AA858080", 0x18)]
public struct SDecoratorMapResource
{
    [SchemaField(0x10, TigerStrategy.MARATHON_ALPHA), NoLoad]
    public Decorator Decorator;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "8080857D", 0xA8)]
public struct SDecorator
{
    public ulong Size;
    public DynamicArray<S98858080> DecoratorModels;
    public DynamicArray<SInt32> InstanceRanges;
    public DynamicArray<SInt32> Unk28;
    public DynamicArray<SInt32> Unk38;
    public Tag<S8080858B> BufferData;
    public Tag<SOcclusionBounds> OcculusionBounds;
    public DynamicArray<SInt32> Unk50;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "98858080", 0x4)]
public struct S98858080
{
    public Tag<S80808599> DecoratorModel;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "80808599", 0x100)]
public struct S80808599
{
    public long FileSize;
    public EntityModel Model;
    public int UnkC;
    //public AABB BoundingBox;

    [SchemaField(0x30, TigerStrategy.MARATHON_ALPHA)]
    public Tag Unk30;  // 8080859B

    [SchemaField(0x34, TigerStrategy.MARATHON_ALPHA)]
    public Tag<SB86C8080> SpeedTreeData; // Used for actual trees, TODO marathon (currently none in the alpha?)
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "B86C8080", 0x18)]
public struct SB86C8080
{
    [SchemaField(0x8)]
    public DynamicArray<SBA6C8080> Unk08;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "BA6C8080", 0x50)]
public struct SBA6C8080
{
    // part of Speedtree cbuffer (cb10)
    public Vector4 Unk00;
    public Vector4 Unk10;
    public Vector4 Unk20;
    public Vector4 Unk30;
    public Vector4 Unk40;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "8080858B", 0x20)]
public struct S8080858B
{
    public ulong Size;
    public TigerHash Unk08;
    public TigerHash UnkC;
    public int Unk10;
    public Tag<S80808586> Unk14;
    public VertexBuffer InstanceBuffer;
    [NoLoad]
    public Tag<SDecoratorInstanceData> InstanceData;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "8080858E", 0x18)]
public struct SDecoratorInstanceData
{
    [SchemaField(0x8)]
    public DynamicArrayUnloaded<S90858080> InstanceElement;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "90858080", 0x10)]
public struct S90858080
{
    // Normalized position
    [SchemaField(TigerStrategy.MARATHON_ALPHA, ArraySizeConst = 4)]
    public ushort[] Position;
    // Rotation represented as an 8-bit quaternion
    [SchemaField(TigerStrategy.MARATHON_ALPHA, ArraySizeConst = 4)]
    public byte[] Rotation;
    // RGBA color
    [SchemaField(TigerStrategy.MARATHON_ALPHA, ArraySizeConst = 4)]
    public byte[] Color;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "80808586", 0x60)]
public struct S80808586
{
    // SpeedtreePlacements[2-7]
    public Vector4 Unk00;
    public Vector4 Unk10;
    public Vector4 Unk20;
    public Vector4 Unk30;
    public Vector4 Unk40;
    public Vector4 Unk50;
}
#endregion
