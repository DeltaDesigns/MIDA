using Tiger.Schema.Model;
using Tiger.Schema.Shaders;

namespace Tiger.Schema.Static;

[SchemaStruct(TigerStrategy.MARATHON, 0x80808635, 0x70)]
public struct SStaticMesh
{
    public long FileSize;
    public IStaticMeshData StaticData;

    [SchemaField(0x10)]
    public DynamicArray<SMaterialHash> Materials;
    public DynamicArray<SStaticMeshDecal> Decals;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x8080861F, 0x20)] // 1F868080
public struct SStaticMeshDecal
{
    public byte RenderStage;
    public byte VertexLayoutIndex;
    public ELodCategory DetailLevel;
    public byte PrimitiveType;
    public IndexBuffer Indices;
    public VertexBuffer Vertices0;
    public VertexBuffer Vertices1;
    public VertexBuffer? VertexColor;
    public uint IndexOffset;
    public uint IndexCount;
    public Material Material;

    public int GetVertexLayoutIndex()
    {
        return VertexLayoutIndex;
    }

    public int GetRenderStage()
    {
        return RenderStage;
    }
}

[SchemaStruct(TigerStrategy.MARATHON, 0x80808620, 0x60)]
public struct SStaticMeshData
{
    public long FileSize;
    public DynamicArray<SStaticMeshMaterialAssignment> MaterialAssignments;
    public DynamicArray<SStaticMeshPart> Parts;
    public DynamicArray<SStaticMeshBuffers> Meshes;

    [SchemaField(0x40, TigerStrategy.MARATHON)]
    public Vector4 ModelTransform;
    public float TexcoordScale;
    public Vector2 TexcoordTranslation;
    public uint MaxVertexColorIndex;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x80808628, 0x6)] // 28868080
public struct SStaticMeshMaterialAssignment
{
    public ushort PartIndex;
    public byte RenderStage;  // TFX render stage
    public byte VertexLayoutIndex;
    public ushort Unk04;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x80808627, 0xC)] // 27868080
public struct SStaticMeshPart
{
    public uint IndexOffset;
    public uint IndexCount;
    public ushort BufferIndex;
    public ELodCategory DetailLevel;
    public sbyte PrimitiveType;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x80808626, 0x10)] // 26868080
public struct SStaticMeshBuffers
{
    public IndexBuffer Indices;
    public VertexBuffer Vertices0;
    public VertexBuffer? Vertices1;
    public VertexBuffer VertexColor;
}
