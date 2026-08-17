using Tiger.Schema.Audio;
using Tiger.Schema.Model;
using Tiger.Schema.Shaders;
using Tiger.Schema.Strings;

namespace Tiger.Schema.Entity;

[SchemaStruct(TigerStrategy.MARATHON, "8080BAAD", 0x98)]
public struct SEntity
{
    public long FileSize;

    [SchemaField(0x08, TigerStrategy.MARATHON)]
    public DynamicArrayUnloaded<S8080BAA2> EntityComponents;

    [SchemaField(0x50, TigerStrategy.MARATHON)]
    public DynamicArrayUnloaded<S8080BAC2> UnkResources; // Basically EntityComponents but contains the Resource's Unk10 ClassHash
}

[SchemaStruct(TigerStrategy.MARATHON, 0x8080BAA2, 0xC)] // A2BA8080
public struct S8080BAA2  // entity resource entry
{
    public FileHash Component;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x8080BAC2, 0x28)] // C2BA8080
public struct S8080BAC2
{
    [SchemaField(0xC)]
    public TigerHash Unk10ClassHash;
    public FileHash Resource;
}

[SchemaStruct(TigerStrategy.MARATHON, "8080BADB", 0xA0)]
public struct S8080BADB  // Entity resource
{
    public long FileSize;

    [SchemaField(0x10)]
    public ResourcePointer Unk10;
    public ResourcePointer Unk18;

    [SchemaField(0x80, TigerStrategy.MARATHON)]
    public Tag UnkHash80;
}


/*
 * The external material map provides the mapping of external material index -> material tag
 * could be these external materials are dynamic themselves - we'll extract them all but select the first
 */
[SchemaStruct(TigerStrategy.MARATHON, 0x80808678, 0x450)] // 78868080
public struct S80808678
{
    [SchemaField(0x28, TigerStrategy.MARATHON)]
    public DynamicArray<S8080BACC> Unk38;

    [SchemaField(0x234, TigerStrategy.MARATHON)]
    public EntityModel Model;

    [SchemaField(0x3D8, TigerStrategy.MARATHON)]
    public DynamicArrayUnloaded<SExternalMaterialMapEntry> ExternalMaterialsMap;

    [SchemaField(0x3F8, TigerStrategy.MARATHON)]
    public DynamicArrayUnloaded<SInt16> Unk408;

    [SchemaField(0x408, TigerStrategy.MARATHON)]
    public DynamicArrayUnloaded<S80808682> Unk418;

    [SchemaField(0x418, TigerStrategy.MARATHON)]
    public DynamicArrayUnloaded<SMaterialHash> ExternalMaterials;
}

// Physics model resource, same layout as normal model resource?
[SchemaStruct(TigerStrategy.MARATHON, 0x80808655, 0x4A0)] // 55868080
public struct S80808655
{
    [SchemaField(0x234, TigerStrategy.MARATHON)]
    public EntityModel PhysicsModel;

    [SchemaField(0x3D8, TigerStrategy.MARATHON)]
    public DynamicArrayUnloaded<SExternalMaterialMapEntry> ExternalMaterialsMap;

    [SchemaField(0x418, TigerStrategy.MARATHON)]
    public DynamicArrayUnloaded<SMaterialHash> ExternalMaterials;
}

// TODO Remove
#region Texture Plates

/// <summary>
/// Texture plate header that stores all the texture plates used for the EntityModel.
/// </summary>
[SchemaStruct(TigerStrategy.MARATHON, "1C6E8080", 0x38)]
public struct S1C6E8080
{
    public long FileSize;

    [SchemaField(0x28, TigerStrategy.MARATHON)]
    public TexturePlate AlbedoPlate;
    public TexturePlate NormalPlate;
    public TexturePlate GStackPlate;
    public TexturePlate DyemapPlate;
}

/// <summary>
/// Texture plate that stores the data for placing textures on a canvas.
/// </summary>
[SchemaStruct(TigerStrategy.MARATHON, "919E8080", 0x20)]
public struct S919E8080
{
    public long FileSize;
    [SchemaField(0x10)]
    public DynamicArrayUnloaded<S939E8080> PlateTransforms;
}

[SchemaStruct(TigerStrategy.MARATHON, "939E8080", 0x14)]
public struct S939E8080
{
    public Texture Texture;
    public IntVector2 Translation;
    public IntVector2 Scale;
}

#endregion

[SchemaStruct(TigerStrategy.MARATHON, 0x80808681, 0xC)] // 81868080
public struct SExternalMaterialMapEntry
{
    public int MaterialCount;
    public int MaterialStartIndex;
    public int Unk08;  // maybe some kind of LOD or dynamic marker
}

[SchemaStruct(TigerStrategy.MARATHON, 0x80808682, 0x8)] // 82868080
public struct S80808682
{
    public ushort Unk00;
    public short Unk02;
    public ushort Unk04;
    public short Unk06;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x80808673, 0x2E0)] // component 0x10 73868080
public struct S80808673
{
}

[SchemaStruct(TigerStrategy.MARATHON, 0x80809FB6, 0x140)] // component 0x10 B69F8080
public struct S80809FB6
{
    [SchemaField(0x30)]
    public DynamicArrayUnloaded<S80809FB5> Unk30;
    public DynamicArrayUnloaded<S8080AF40> Unk40;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x80809FB5, 0x40)] // B59F8080
public struct S80809FB5
{
    [SchemaField(0x20, TigerStrategy.MARATHON)]
    public DynamicArray<S8080BF47> Unk20;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x8080BF47, 0x20)] // 47BF8080
public struct S8080BF47
{
    public Tiger.Schema.Vector4 Rotation;
    public Tiger.Schema.Vector4 Translation;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x8080AF40, 8)] // 40AF8080
public struct S8080AF40
{
    public ushort Unk00;
    public ushort Unk02;
    public uint Unk04;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x80809FAE, 0xC0)] // component 0x10 AE9F8080
public struct S80809FAE
{
    [SchemaField(0x38, TigerStrategy.MARATHON)]
    public DynamicArrayUnloaded<SInt32> Unk38;
    public DynamicArrayUnloaded<SInt32> Unk48;
    public DynamicArrayUnloaded<S8080BF47> Unk58;
    public DynamicArrayUnloaded<S8080AF40> Unk68;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x80809FAF, 0xC0)] // component 0x18 AF9F8080
public struct S80809FAF
{
    [SchemaField(0x80)]
    public DynamicArrayUnloaded<S8080AF42> NodeHierarchy;
    public DynamicArrayUnloaded<S8080BF47> DefaultInverseObjectSpaceTransforms;
    //public DynamicArrayUnloaded<SInt16> RangeIndexMap;
    //public DynamicArrayUnloaded<SInt16> InnerIndexMap;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x80809FB7, 0x140)] // component 0x18 B79F8080
public struct S80809FB7
{
    [SchemaField(0xB0, TigerStrategy.MARATHON)]
    public DynamicArrayUnloaded<S8080AF42> NodeHierarchy;
    public DynamicArrayUnloaded<S8080BF47> DefaultObjectSpaceTransforms;
    public DynamicArrayUnloaded<S8080BF47> DefaultInverseObjectSpaceTransforms;
    public DynamicArrayUnloaded<SInt16> RangeIndexMap;
    public DynamicArrayUnloaded<SInt16> InnerIndexMap;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x8080AF42, 0x10)] // 42AF8080
public struct S8080AF42
{
    public TigerHash NodeHash;
    public int ParentNodeIndex;
    public int FirstChildNodeIndex;
    public int NextSiblingNodeIndex;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x8080881C, 0x110)]
public struct SEntityModel  // Entity model
{
    public long FileSize;
    [SchemaField(0x10)]
    public DynamicArrayUnloaded<SEntityModelMesh> Meshes;

    [SchemaField(0xA0, TigerStrategy.MARATHON)]
    public Vector4 ModelScale;
    public Vector4 ModelTranslation;
    public Vector2 TexcoordScale;
    public Vector2 TexcoordTranslation;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x808087CB, 0x88)] // CB878080
public struct SEntityModelMesh
{
    public VertexBuffer Vertices1;  // vert file 1 (positions)
    public VertexBuffer Vertices2;  // vert file 2 (texcoords/normals)
    public VertexBuffer OldWeights;  // old weights
    public TigerHash Unk0C;  // nothing ever
    public IndexBuffer Indices;  // indices
    public VertexBuffer VertexColour;  // vertex colour
    public VertexBuffer SinglePassSkinningBuffer;  // single pass skinning buffer
    public int Zeros1C;
    public DynamicArrayUnloaded<S808087D1> Parts;

    /// Range of parts to render per render stage
    /// Can be obtained as follows:
    ///
    ///     - Start = part_range_per_render_stage[stage]
    ///     - End = part_range_per_render_stage[stage + 1]

    [SchemaField(TigerStrategy.MARATHON, ArraySizeConst = 26)] // ArraySizeConst being the number of elements
    public short[] PartRangePerRenderStage;

    [SchemaField(TigerStrategy.MARATHON, ArraySizeConst = 25)]
    public byte[] InputLayoutPerRenderStage;

    public Range GetRangeForStage(int stage)
    {
        int start = PartRangePerRenderStage[stage];
        int end = PartRangePerRenderStage[stage + 1];
        return new Range(start, end);
    }

    public int GetInputLayoutForStage(int stage)
    {
        return InputLayoutPerRenderStage[stage];
    }
}

[SchemaStruct(TigerStrategy.MARATHON, 0x808087D1, 0x28)] // D1878080
public struct S808087D1
{
    public Material Material;
    public short VariantShaderIndex;
    public short PrimitiveType;
    public uint IndexOffset;
    public uint IndexCount;

    [SchemaField(0x14, TigerStrategy.MARATHON)]
    public short ExternalIdentifier;  // Unsure

    [SchemaField(0x1C, TigerStrategy.MARATHON)]
    public int Flags; // Unsure
    public byte GearDyeChangeColorIndex; // Unsure, if this even exists now
    public ELodCategory DetailLevel;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x80808644, 0x320)] // component 0x10 44868080
public struct S80808644
{
}

[SchemaStruct(TigerStrategy.MARATHON, 0x8080A317, 0x50)] // component 0x10 17A38080
public struct S8080A317
{
}

[SchemaStruct(TigerStrategy.MARATHON, 0x8080A313, 0xA0)] // component 0x18 13A38080
public struct S8080A313
{
    [SchemaField(0x98, TigerStrategy.MARATHON)]
    public DynamicArray<S8080A320> Unk88;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x8080A320, 0x18)] // 20A38080
public struct S8080A320
{
    [SchemaField(0x8, TigerStrategy.MARATHON)]
    public DynamicArray<S8080A322> Unk08;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x8080A322, 0x18)] // 22A38080
public struct S8080A322
{
    public TigerHash Unk00;
    public int Unk04;

    [SchemaField(TigerStrategy.MARATHON, Tag64 = true)]
    public Tag Entity;
}

// General, parents that reference Entity

[SchemaStruct(0x8080AB68, 0x28)]
public struct S8080AB68
{
    public long FileSize;
    [SchemaField(0x18)]
    public DynamicArray<S8080AB6B> Unk18;
}


[SchemaStruct(0x8080AB6B, 0x20)]
public struct S8080AB6B
{
    public StringPointer TagPath;
    [SchemaField(Tag64 = true)]
    public Tag Tag;  // if .pattern.tft, then Entity - if .budget_set.tft, then parent of itself
    public StringPointer TagNote;
}

// TODO remove? Dont think budget sets exist anymore
[SchemaStruct("ED9E8080", 0x58)]
public struct SED9E8080
{
    public long FileSize;
    [SchemaField(0x18)]
    public Tag Unk18;
    [SchemaField(0x28)]
    public DynamicArray<SF19E8080> Unk28;
}

[SchemaStruct("F19E8080", 0x18)]
public struct SF19E8080
{
    public StringPointer TagPath;
    [SchemaField(0x8, Tag64 = true)]
    public Tag Tag;  // if .pattern.tft, then Entity
}

[SchemaStruct("7E988080", 8)]
public struct S7E988080
{
    public Tag Unk00;
    public Tag Unk08;
}

#region Named Bags

[SchemaStruct(0x8080AB5F, 0x18)] // only one tag of this exists rn for lobby runners?
public struct S8080AB5F
{
    public long FileSize;
    public DynamicArray<S8080AB61> DestinationGlobalTagBags;
}

[SchemaStruct(0x8080AB61, 0x10)]
public struct S8080AB61
{
    public FileHash DestinationGlobalTagBag;
    [SchemaField(0x8)]
    public StringPointer DestinationGlobalTagBagName;
}

#endregion

#region Audio
[SchemaStruct(TigerStrategy.MARATHON, 0x8080AE2E, 0x18)]
public struct S8080AE2E
{
    public long FileSize;
    public DynamicArray<S8080AE30> Audio;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x8080AE30, 0x18)]
public struct S8080AE30
{
    public TigerHash WwiseEventHash;
    [SchemaField(0x8)]
    public DynamicArray<S8080AE34> Sounds;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x8080AE34, 0x38)]
public struct S8080AE34
{
    [SchemaField(0x8)]
    public TigerHash Unk08;

    [SchemaField(0x10)]
    public StringPointer WwiseEventName;

    [SchemaField(Tag64 = true)]
    public FileHash Data; // Can be WwiseSound or pattern entity
}


// The Sequencer
[SchemaStruct(TigerStrategy.MARATHON, 0x80809F51, 0x390)] // 519F8080
public struct S80809F51
{
    [SchemaField(0x200)]
    public DynamicArray<S8080B66E> Array1;
    public DynamicArray<S8080B66E> Array2;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x8080B66E, 0x18)] // 6EB68080
public struct S8080B66E
{
    [SchemaField(0x10, TigerStrategy.MARATHON)]
    public ResourcePointer Unk10; // B9678080, 40668080
}

[SchemaStruct(TigerStrategy.MARATHON, 0x80807ECA, 0x68)] // CA7E8080
public struct S80807ECA
{
    [SchemaField(0x28, Tag64 = true)]
    public WwiseSound Audio;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x8080B69A, 0x370)] // 9AB68080
public struct S8080B69A
{
}

[SchemaStruct(TigerStrategy.MARATHON, 0x808040D6, 0x10)] // D6408080
public struct S808040D6
{
}

[SchemaStruct(TigerStrategy.MARATHON, 0x808040D4, 0x498)] // D4408080
public struct S808040D4 // what the fuck.
{
    [SchemaField(0xC0, Tag64 = true)]
    public Entity Entity1;
    [SchemaField(0xD8, Tag64 = true)]
    public Entity Entity2;
    [SchemaField(0xF0, Tag64 = true)]
    public Entity Entity3;
    [SchemaField(0x108, Tag64 = true)]
    public Entity Entity4;
    [SchemaField(0x120, Tag64 = true)]
    public Entity Entity5;
    [SchemaField(0x138, Tag64 = true)]
    public Entity Entity6;
    [SchemaField(0x150, Tag64 = true)]
    public Entity Entity7;
    [SchemaField(0x168, Tag64 = true)]
    public Entity Entity8;
    [SchemaField(0x180, Tag64 = true)]
    public Entity Entity9;
    [SchemaField(0x198, Tag64 = true)]
    public Entity Entity10;
    [SchemaField(0x1B0, Tag64 = true)]
    public Entity Entity11;
    [SchemaField(0x1C8, Tag64 = true)]
    public Entity Entity12;
    [SchemaField(0x1E0, Tag64 = true)]
    public Entity Entity13;
}

#endregion

[SchemaStruct(TigerStrategy.MARATHON, 0x80807F3C, 0x1F0)] // 3C7F8080
public struct SMapCubemapResource // Dataresource for cubemaps
{
    [SchemaField(0x20)]
    public Vector4 CubemapSize; //XYZ, no W
    public Vector4 CubemapPosition; // Not actually right afaik

    [SchemaField(0xC0)]
    public long WorldID; // Same as the ID in the datatable entry

    [SchemaField(0x110, TigerStrategy.MARATHON)]
    public Vector4 CubemapRotation;

    [SchemaField(0x1C8, TigerStrategy.MARATHON)]
    public Texture CubemapTexture;

    [SchemaField(0x1D0, TigerStrategy.MARATHON)]
    public Texture CubemapIBLTexture; //Sometype of reflection tint texture idk
    public Tag Unk1D4; // 80807F5B, seems to hold colors?
}

[SchemaStruct(TigerStrategy.MARATHON, 0x8080B1A5, 0x190)] // A5B18080
public struct S8080B1A5
{
    [SchemaField(0x84)]
    public Tag<SMapDataTable> Unk84;
    [SchemaField(0x90)]
    public Vector4 Rotation;
    public Vector4 Translation;
}

[SchemaStruct(TigerStrategy.MARATHON, "EF8C8080", 0x60)]
public struct SEF8C8080 // Todo, unsure if exists are class id may be different
{
    [SchemaField(0x58)]
    public Tag<SMapDataTable> Unk58;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x8080B199, 0x80)] // 99B18080
public struct S8080B199
{
    [SchemaField(0x28)]
    public TigerHash FNVHash;
    [SchemaField(0x30)]
    public ulong WorldID;
    [SchemaField(0x58)]
    public DynamicArray<S8080B8AB> Unk58;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x8080B8AB, 0x10)] // ABB88080
public struct S8080B8AB
{
    public TigerHash FNVHash;
    [SchemaField(0x8)]
    public ulong WorldID;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x8080B1E2, 0x28)]
public struct S8080B1E2
{
    [SchemaField(0x8)]
    public DynamicArray<S8080BFDC> Unk08;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x8080BFDC, 0x10)] // DCBF8080
public struct S8080BFDC
{
    public ResourceInTablePointer<S8080AB89> Unk00;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x8080AB89, 0xC)] // 89AB8080
public struct S8080AB89
{
    public StringPointer Name;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x80805B83, 0x1B0)] // 835B8080
public struct S80805B83
{
    [SchemaField(0x80)]
    public DynamicArray<S80805B62> Unk80;

    [SchemaField(0xF0)]
    public Vector4 Rotation;
    public Vector4 Translation;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x80805B62, 0x80)] // 625B8080
public struct S80805B62
{
    [SchemaField(0x28, Tag64 = true)]
    public Tag<SMapDataTable> DataTable;
    public StringHash Name;
}

// Loot container names
[SchemaStruct(TigerStrategy.MARATHON, 0x808035B4, 0x10)] // B4358080, component 0x10
public struct S808035B4
{

}

[SchemaStruct(TigerStrategy.MARATHON, 0x808035B7, 0x590)] // B7358080, component 0x18
public struct S808035B7
{
    [SchemaField(0x340)]
    public StringHash Name;
    [SchemaField(0x350, Tag64 = true)]
    public LocalizedStrings Container;
}

#region Gear + Gear attachments

// Relating to base weapon model?
[SchemaStruct(TigerStrategy.MARATHON, 0x808032B1, 0x170)] // component 0x10 B1328080
public struct S808032B1
{
}

[SchemaStruct(TigerStrategy.MARATHON, 0x808032B3, 0x170)] // component 0x18 B3328080
public struct S808032B3
{
    [SchemaField(0x128)]
    public DynamicArray<S808032BA> Unk128;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x808032BA, 0x40)] // BA328080
public struct S808032BA
{
    public TigerHash Unk04;
    [SchemaField(0x10, Tag64 = true), NoLoad]
    public Entity Entity;
}
//-------------------------------------------

// Relating to weapon attachments?
[SchemaStruct(TigerStrategy.MARATHON, 0x80804603, 0x2D0)] // Ent resource 0x10
public struct S80804603
{
}

[SchemaStruct(TigerStrategy.MARATHON, 0x80804A14, 0x468)] // 144A8080 Ent resource 0x18
public struct S80804A14
{
    [SchemaField(0x28)]
    public DynamicArray<S8080BACC> Unk38;

    [SchemaField(0x218)]
    public DynamicArray<S8080BA9E> Unk228;

    [SchemaField(0x290)]
    public DynamicArray<S8080BA9E> Unk2A0;

    //[SchemaField(0x310, Tag64 = true)] // Contains the skeleton for attachments?
    //public Entity UnkEntity310;

    //[SchemaField(0x358)]
    //public DynamicArray<S412F8080> Unk358;

    [SchemaField(0x368, Tag64 = true)]
    public Tag<S8080AE2E> Audio;

    [SchemaField(0x3D8)]
    public DynamicArray<S8080460A> Unk3E8;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x8080BA9E, 0x30)] // 9EBA8080
public struct S8080BA9E
{
    public MapTransform Transform;

    [SchemaField(0x28)]
    public TigerHash Unk28; // type (attachment) 
    public TigerHash Unk2C;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x80802F41, 0x20)] // 412F8080
public struct S80802F41
{
    public TigerHash Unk00; // Runner name/type
    [SchemaField(0x10, Tag64 = true)]
    public Entity UnkEntity;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x8080460A, 0x20)] // 0A468080
public struct S8080460A
{
    public TigerHash Unk00; // Attachment name
    [SchemaField(0x10, Tag64 = true)]
    public Entity Entity;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x8080A398, 0x100)] // 98A38080
public struct S8080A398
{
    public TigerHash Unk00;

    [SchemaField(0x88, Tag64 = true)]
    public Entity Entity;
}


[SchemaStruct(TigerStrategy.MARATHON, 0x80803287, 0x9F0)] // 87328080 Ent resource 0x10
public struct S80803287
{
}

[SchemaStruct(TigerStrategy.MARATHON, 0x80803288, 0x750)] // 88328080 Ent resource 0x18
public struct S80803288
{
    [SchemaField(0x6E8, Tag64 = true), NoLoad]
    public Entity Entity;
}


[SchemaStruct(TigerStrategy.MARATHON, 0x8080AD85, 0x70)] // Ent resource 0x10 85AD8080
public struct S8080AD85
{
}

[SchemaStruct(TigerStrategy.MARATHON, 0x8080AD7E, 0x140)] // Ent resource 0x18 7EAD8080
public struct S8080AD7E
{
    [SchemaField(0xC8, TigerStrategy.MARATHON)]
    public DynamicArray<S8080BF47> UnkC8;

    [SchemaField(0x128, TigerStrategy.MARATHON)]
    public DynamicArray<S8080ADFB> Unk128;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x8080ADFB, 0x8)] // FBAD8080
public struct S8080ADFB
{
    public StringHash UnkName;
    public int UnkIndex;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x80809F75, 0x70)] // 759F8080 Ent resource 0x10
public struct S80809F75
{
}

[SchemaStruct(TigerStrategy.MARATHON, 0x80809F76, 0x108)] // 769F8080 Ent resource 0x18
public struct S80809F76
{
    [SchemaField(0xA8, TigerStrategy.MARATHON)]
    public DynamicArray<S80809F82> AttachmentTransforms;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x80809F82, 0x30)] // 829F8080
public struct S80809F82
{
    public MapTransform Transform;
    [SchemaField(0x24)]
    public int BoneIndex;
    public StringHash AttachmentName;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x808032C2, 0x10)] // C2328080 Ent resource 0x10
public struct S808032C2
{
}

[SchemaStruct(TigerStrategy.MARATHON, 0x808032C4, 0x138)] // C4328080 Ent resource 0x18
public struct S808032C4
{
    [SchemaField(0x48, TigerStrategy.MARATHON)]
    public DynamicArray<S8080BACC> Unk38;

    [SchemaField(0xC0, TigerStrategy.MARATHON)]
    public TigerHash AttachmentType;

    [SchemaField(0x148, TigerStrategy.MARATHON)]
    public ResourcePointer Unk120;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x8080BACC, 0x18)] // CCBA8080
public struct S8080BACC
{
    public int Unk00;
    [SchemaField(0x8)]
    public DynamicArray<S8080BAD0> Unk8;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x8080BAD0, 0x8)] // D0BA8080
public struct S8080BAD0
{
    public TigerHash SwitchKey; // weapon_type "switch_key"
    public TigerHash Value; // weapon name "value"
}

[SchemaStruct(TigerStrategy.MARATHON, 0x808032C9, 0x20)] // C9328080
public struct S808032C9
{
    public TigerHash Unk00;
    public TigerHash Unk04;
    public TigerHash Unk08;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x80807F69, 0x30)] // 697F8080
public struct S80807F69 // larger than 0x30 but atm dont care about everything after
{
    [SchemaField(0x24)]
    public Tag<S808085DA> Unk24;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x808085DA, 0x90)] // DA858080
public struct S808085DA
{
    [SchemaField(0x78)]
    public DynamicArrayUnloaded<SMaterialHash> Materials;
}

#endregion

#region Object Channel related
[SchemaStruct(TigerStrategy.MARATHON, 0x8080AF8E, 0x10)] // 8EAF8080 component 0x10
public struct S8080AF8E
{
}

[SchemaStruct(TigerStrategy.MARATHON, 0x8080AF75, 0x1A8)] // 75AF8080 component 0x18
public struct S8080AF75
{
    [SchemaField(0x120)]
    public DynamicArray<S8080AF86> Unk130;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x8080AF86, 0x70)] // 86AF8080
public struct S8080AF86
{
    public TigerHash ChannelHash;
    [SchemaField(0x8)]
    public DynamicArray<SUInt8> UnkBytecode;
    public DynamicArray<Vec4> UnkConstants;
}
#endregion
