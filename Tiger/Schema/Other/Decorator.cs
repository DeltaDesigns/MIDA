using System.Numerics;
using Arithmic;
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

    public void LoadIntoExporter(ExporterScene decorsScene, ExporterScene treesScene, string saveDirectory)
    {
        if (_tag.BufferData is null)
            return;

        Log.Debug($"Loading Decorators {Hash} into Exporter.");

        var models = _tag.DecoratorModels;
        // Model transform offsets
        List<Vector4> SpeedtreePlacements = new(); //{ Vector4.Zero, Vector4.Zero.WithW(1) };

        TigerFile container = new(_tag.BufferData.TagData.Unk14.Hash);
        byte[] containerData = container.GetData();
        for (int i = 0; i < containerData.Length / 16; i++)
        {
            SpeedtreePlacements.Add(containerData.Skip(i * 16).Take(16).ToArray().ToType<Vector4>());
        }
        SpeedtreePlacements.Add(Vector4.Zero);
        SpeedtreePlacements.Add(Vector4.Zero.WithW(1));

        using TigerReader reader = _tag.BufferData.TagData.InstanceBuffer.GetReferenceReader();

        string baseSceneName = decorsScene.Name;
        for (int i = 0; i < _tag.InstanceRanges.Count - 1; i++)
        {
            // testing
            //decorsScene = Exporter.Get().CreateScene($"{baseSceneName}_{i}", ExportType.Decorators, DataExportType.Map);

            int start = _tag.InstanceRanges[i].Value;
            int end = _tag.InstanceRanges[i + 1].Value;
            int count = end - start;

            int dynID = models.Count == 1 ? i : 0;
            var model = models[models.Count == 1 ? 0 : i].DecoratorModel;
            var isSpeedTree = model.TagData.SpeedTreeData != null;

            List<DynamicMeshPart> parts = model.TagData.Model.Load(ExportDetailLevel.MostDetailed, null);
            if (isSpeedTree)
            {
                parts = parts.Where(x => x.IndexOffset == 0).ToList();
                if (parts.Count > 1)
                    parts = parts.SkipLast(1).ToList();
            }
            else
            {
                parts = parts.Where(x => x.MeshIndex == 0 && x.GroupIndex == dynID).ToList();
            }

            foreach (DynamicMeshPart part in parts)
            {
                if (part.Material == null) continue;

                if (isSpeedTree)
                {
                    treesScene.Materials.Add(new ExportMaterial(part.Material));

                    var vecs = model.TagData.SpeedTreeData.TagData.Unk08[part.MeshIndex];
                    var scale = vecs.Unk00;
                    var offset = vecs.Unk10;
                    var uvTransform = vecs.Unk20;

                    for (int k = 0; k < part.VertexPositions.Count; k++)
                    {
                        part.VertexPositions[k] = new Vector4(
                            part.VertexPositions[k].X * scale.X + offset.X,
                            part.VertexPositions[k].Y * scale.Y + offset.Y,
                            part.VertexPositions[k].Z * scale.Z + offset.Z,
                            part.VertexPositions[k].W
                        );
                    }

                    for (int k = 0; k < part.VertexTexcoords0.Count; k++)
                    {
                        part.VertexTexcoords0[k] = new Vector2(
                            part.VertexTexcoords0[k].X * uvTransform.X + uvTransform.Z,
                            part.VertexTexcoords0[k].Y * -uvTransform.Y + 1 - uvTransform.W
                        );
                    }
                }
                else
                    decorsScene.Materials.Add(new ExportMaterial(part.Material));
            }

            for (int j = 0; j < count; j++)
            {
                reader.BaseStream.Seek((start + j) * 0x10, SeekOrigin.Begin);
                var pos = new Vector4(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16()); // v5?
                var rot = new Vector4(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte()); // v6?
                var v7 = new Vector4(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte()); // v7?

                Vector4 inst = SpeedtreePlacements[0] * pos + SpeedtreePlacements[1];
                Vector4 q = SpeedtreePlacements[2] * rot + SpeedtreePlacements[3];
                Vector4 unk = SpeedtreePlacements[4] * v7 + SpeedtreePlacements[5];

                if (isSpeedTree)
                {
                    System.Numerics.Vector3 r2 = q.ToVec3();
                    System.Numerics.Vector3 r1 = unk.ToVec3();
                    System.Numerics.Vector3 r3 = System.Numerics.Vector3.Cross(r1, r2);

                    System.Numerics.Matrix4x4 rotationMatrix = new System.Numerics.Matrix4x4(
                        r2.X, r3.X, r1.X, 0,
                        r2.Y, r3.Y, r1.Y, 0,
                        r2.Z, r3.Z, r1.Z, 0,
                        0, 0, 0, 1
                    );

                    var quat = Quaternion.CreateFromRotationMatrix(rotationMatrix);
                    q = new(quat.X, quat.Y, quat.Z, -quat.W);
                }

                Transform transform = new()
                {
                    Position = inst.ToVec3(),
                    Quaternion = q,
                    Scale = new(inst.W)
                };

                if (isSpeedTree)
                    treesScene.AddMapModelParts($"{model.Hash}_{i}", parts, transform);
                else
                    decorsScene.AddMapModelParts($"{model.Hash}_{dynID}", parts, transform);
            }
        }
    }
}

#region Decorator structs
[SchemaStruct(TigerStrategy.MARATHON, "AA858080", 0x18)]
public struct SDecoratorMapResource
{
    [SchemaField(0x10, TigerStrategy.MARATHON), NoLoad]
    public Decorator Decorator;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x8080857D, 0xA8)]
public struct SDecorator
{
    public ulong Size;
    public DynamicArray<S80808598> DecoratorModels;
    public DynamicArray<SInt32> InstanceRanges;
    public DynamicArray<SInt32> Unk28;
    public DynamicArray<SInt32> Unk38;
    public Tag<S8080858B> BufferData;
    public Tag<SOcclusionBounds> OcculusionBounds;
    public DynamicArray<SInt32> Unk50;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x80808598, 0x4)] // 98858080
public struct S80808598
{
    public Tag<S80808599> DecoratorModel;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x80808599, 0x100)]
public struct S80808599
{
    public long FileSize;
    public EntityModel Model;
    public int UnkC;
    //public AABB BoundingBox;

    [SchemaField(0x30, TigerStrategy.MARATHON)]
    public Tag Unk30;  // 8080859B

    [SchemaField(0x34, TigerStrategy.MARATHON)]
    public Tag<SB86C8080> SpeedTreeData; // Used for actual trees, TODO marathon (currently none in the alpha?)
}

[SchemaStruct(TigerStrategy.MARATHON, "B86C8080", 0x18)]
public struct SB86C8080
{
    [SchemaField(0x8)]
    public DynamicArray<SBA6C8080> Unk08;
}

[SchemaStruct(TigerStrategy.MARATHON, "BA6C8080", 0x50)]
public struct SBA6C8080
{
    // part of Speedtree cbuffer (cb10)
    public Vector4 Unk00;
    public Vector4 Unk10;
    public Vector4 Unk20;
    public Vector4 Unk30;
    public Vector4 Unk40;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x8080858B, 0x20)]
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

[SchemaStruct(TigerStrategy.MARATHON, "8080858E", 0x18)]
public struct SDecoratorInstanceData
{
    [SchemaField(0x8)]
    public DynamicArrayUnloaded<S90858080> InstanceElement;
}

[SchemaStruct(TigerStrategy.MARATHON, "90858080", 0x10)]
public struct S90858080
{
    // Normalized position
    [SchemaField(TigerStrategy.MARATHON, ArraySizeConst = 4)]
    public ushort[] Position;
    // Rotation represented as an 8-bit quaternion
    [SchemaField(TigerStrategy.MARATHON, ArraySizeConst = 4)]
    public byte[] Rotation;
    // RGBA color
    [SchemaField(TigerStrategy.MARATHON, ArraySizeConst = 4)]
    public byte[] Color;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x80808586, 0x60)]
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
