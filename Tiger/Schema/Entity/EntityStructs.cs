using Tiger.Schema.Audio;
using Tiger.Schema.Investment;
using Tiger.Schema.Model;
using Tiger.Schema.Shaders;

namespace Tiger.Schema.Entity;

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "8080BAAD", 0x98)]
public struct SEntity
{
    public long FileSize;

    [SchemaField(0x08, TigerStrategy.MARATHON_ALPHA)]
    public DynamicArrayUnloaded<SCD9A8080> EntityResources;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "8080BAA2", 0xC)]
public struct SCD9A8080  // entity resource entry
{
    public FileHash Resource; // Can sometimes be a non-entity resource in D1, for whatever reason
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "8080BADB", 0xA0)]
public struct S8080BADB  // Entity resource
{
    public long FileSize;

    [SchemaField(0x10)]
    public ResourcePointer Unk10; // this isnt any of the ones in Entity.Load in beyond light
    public ResourcePointer Unk18;

    [SchemaField(0x80, TigerStrategy.MARATHON_ALPHA)]
    public Tag UnkHash80;
}


/*
 * The external material map provides the mapping of external material index -> material tag
 * could be these external materials are dynamic themselves - we'll extract them all but select the first
 */
[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "78868080", 0x450)]
public struct S78868080
{
    [SchemaField(0x244, TigerStrategy.MARATHON_ALPHA)]
    public EntityModel Model;

    //[SchemaField(0x310, TigerStrategy.MARATHON_ALPHA)] // Not used currently?
    //public Tag<S1C6E8080> TexturePlates;

    [SchemaField(0x3E0, TigerStrategy.MARATHON_ALPHA)]
    public DynamicArrayUnloaded<SExternalMaterialMapEntry> ExternalMaterialsMap;

    [SchemaField(0x410, TigerStrategy.MARATHON_ALPHA)]
    public DynamicArrayUnloaded<S82868080> Unk410;

    [SchemaField(0x420, TigerStrategy.MARATHON_ALPHA)]
    public DynamicArrayUnloaded<S14008080> ExternalMaterials;
}

// Physics model resource, same layout as normal model resource?
[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "55868080", 0x4A0)]
public struct S55868080
{
    [SchemaField(0x244, TigerStrategy.MARATHON_ALPHA)]
    public EntityModel PhysicsModel;

    [SchemaField(0x3E0, TigerStrategy.MARATHON_ALPHA)]
    public DynamicArrayUnloaded<SExternalMaterialMapEntry> ExternalMaterialsMap;

    [SchemaField(0x420, TigerStrategy.MARATHON_ALPHA)]
    public DynamicArrayUnloaded<S14008080> ExternalMaterials;
}

#region Texture Plates

/// <summary>
/// Texture plate header that stores all the texture plates used for the EntityModel.
/// </summary>
[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "1C6E8080", 0x38)]
public struct S1C6E8080
{
    public long FileSize;

    [SchemaField(0x28, TigerStrategy.MARATHON_ALPHA)]
    public TexturePlate AlbedoPlate;
    public TexturePlate NormalPlate;
    public TexturePlate GStackPlate;
    public TexturePlate DyemapPlate;
}

/// <summary>
/// Texture plate that stores the data for placing textures on a canvas.
/// </summary>
[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "919E8080", 0x20)]
public struct S919E8080
{
    public long FileSize;
    [SchemaField(0x10)]
    public DynamicArrayUnloaded<S939E8080> PlateTransforms;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "939E8080", 0x14)]
public struct S939E8080
{
    public Texture Texture;
    public IntVector2 Translation;
    public IntVector2 Scale;
}

#endregion

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "82868080", 0x8)]
public struct S82868080
{
    public ushort Unk00;
    public ushort Unk02;
    public ushort Unk04;
    public ushort Unk06;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "81868080", 0xC)]
public struct SExternalMaterialMapEntry
{
    public int MaterialCount;
    public int MaterialStartIndex;
    public int Unk08;  // maybe some kind of LOD or dynamic marker
}

[SchemaStruct("14008080", 0x4)]
public struct S14008080
{
    public Material Material;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "73868080", 0x2E0)]
public struct S73868080
{
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "B69F8080", 0x140)]
public struct SB69F8080
{
    [SchemaField(0x30)]
    public DynamicArray<SB59F8080> Unk30;
    public DynamicArray<S40AF8080> Unk40;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "B59F8080", 0x40)]
public struct SB59F8080
{
    [SchemaField(0x20, TigerStrategy.MARATHON_ALPHA)]
    public DynamicArray<S47BF8080> Unk20;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "47BF8080", 0x20)]
public struct S47BF8080
{
    public Tiger.Schema.Vector4 Rotation;
    public Tiger.Schema.Vector4 Translation;
}

[SchemaStruct("40AF8080", 8)]
public struct S40AF8080
{
    public ushort Unk00;
    public ushort Unk02;
    public uint Unk04;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "AE9F8080", 0xC0)]
public struct SAE9F8080
{
    [SchemaField(0x38, TigerStrategy.MARATHON_ALPHA)]
    public DynamicArrayUnloaded<SInt32> Unk38;
    public DynamicArrayUnloaded<SInt32> Unk48;
    public DynamicArrayUnloaded<S47BF8080> Unk58;
    public DynamicArrayUnloaded<S40AF8080> Unk68;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "AF9F8080", 0xC0)]
public struct SAF9F8080
{
    [SchemaField(0x90)]
    public DynamicArrayUnloaded<S42AF8080> NodeHierarchy;
    public DynamicArrayUnloaded<S47BF8080> DefaultInverseObjectSpaceTransforms;
    //public DynamicArrayUnloaded<SInt16> RangeIndexMap;
    //public DynamicArrayUnloaded<SInt16> InnerIndexMap;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "B79F8080", 0x110)]
public struct SB79F8080
{
    [SchemaField(0x90, TigerStrategy.MARATHON_ALPHA)]
    public DynamicArrayUnloaded<S42AF8080> NodeHierarchy;
    public DynamicArrayUnloaded<S47BF8080> DefaultObjectSpaceTransforms;
    public DynamicArrayUnloaded<S47BF8080> DefaultInverseObjectSpaceTransforms;
    public DynamicArrayUnloaded<SInt16> RangeIndexMap;
    public DynamicArrayUnloaded<SInt16> InnerIndexMap;

    [SchemaField(TigerStrategy.MARATHON_ALPHA)]
    public Vector2 UnkE0;
    public Vector2 UnkE8;
    public Vector2 UnkF0;
    //public DynamicArrayUnloaded<SE1818080> UnkF8; // lod distance?
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "42AF8080", 0x10)]
public struct S42AF8080
{
    public TigerHash NodeHash;
    public int ParentNodeIndex;
    public int FirstChildNodeIndex;
    public int NextSiblingNodeIndex;
}

[SchemaStruct("E1818080", 0x18)]
public struct SE1818080
{
    public ResourceInTagPointer Unk00;
    public long Unk10;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "8080881C", 0x110)]
public struct SEntityModel  // Entity model
{
    public long FileSize;
    [SchemaField(0x10)]
    public DynamicArrayUnloaded<SEntityModelMesh> Meshes;

    //[SchemaField(0x20)]
    //public Vector4 Unk20;
    //public long Unk30;

    //[SchemaField(0x38)]
    //public long UnkFlags38;

    [SchemaField(0xA0, TigerStrategy.MARATHON_ALPHA)]
    public Vector4 ModelScale;
    public Vector4 ModelTranslation;
    public Vector2 TexcoordScale;
    public Vector2 TexcoordTranslation;
    public Vector4 Unk80;

    public TigerHash Unk90;
    public TigerHash Unk94;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "CB878080", 0x80)]
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
    public DynamicArrayUnloaded<SD1878080> Parts;

    /// Range of parts to render per render stage
    /// Can be obtained as follows:
    ///
    ///     - Start = part_range_per_render_stage[stage]
    ///     - End = part_range_per_render_stage[stage + 1]

    [SchemaField(TigerStrategy.MARATHON_ALPHA, ArraySizeConst = 25)] // ArraySizeConst being the number of elements
    public short[] PartRangePerRenderStage;

    [SchemaField(TigerStrategy.MARATHON_ALPHA, ArraySizeConst = 24)]
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

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "D1878080", 0x28)]
public struct SD1878080
{
    public Material Material;
    public short VariantShaderIndex;
    public short PrimitiveType;
    public uint IndexOffset;
    public uint IndexCount;

    [SchemaField(0x14, TigerStrategy.MARATHON_ALPHA)]
    public short ExternalIdentifier;  // Unsure

    [SchemaField(0x1C, TigerStrategy.MARATHON_ALPHA)]
    public int Flags; // Unsure
    public byte GearDyeChangeColorIndex;
    public ELodCategory LodCategory;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "44868080", 0x320)]
public struct S44868080
{
}

[SchemaStruct("668B8080", 0x70)]
public struct S668B8080
{
    [SchemaField(0x30)]
    public DynamicArrayUnloaded<S628B8080> Unk30;
}

[SchemaStruct("628B8080", 0x30)]
public struct S628B8080
{
    public Vector4 Unk00;
}

[SchemaStruct("90008080", 0x10)]
public struct S90008080
{
    public Vector4 Unk00;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "DA5E8080", 0x150)]
public struct SDA5E8080
{
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "DB5E8080", 0x240)]
public struct SDB5E8080
{
    [SchemaField(0x108, TigerStrategy.MARATHON_ALPHA)]
    public Tag<S23978080> Unk108;
}

[SchemaStruct("23978080", 0x48)]
public struct S23978080
{
    public long FileSize;
    public StringHash EntityName;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "12848080", 0x50)]
public struct S12848080
{
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "0E848080", 0xA0)]
public struct S0E848080
{
    [SchemaField(0x88, TigerStrategy.MARATHON_ALPHA)]
    public DynamicArray<S1B848080> Unk88;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "1B848080", 0x18)]
public struct S1B848080
{
    [SchemaField(0x8, TigerStrategy.MARATHON_ALPHA)]
    public DynamicArray<S1D848080> Unk08;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "1D848080", 0x18)]
public struct S1D848080
{
    public int Unk00;
    public int Unk04;

    [SchemaField(TigerStrategy.MARATHON_ALPHA, Tag64 = true)]
    public Tag Entity;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "81888080", 0xEC)]
public struct S81888080
{
    [SchemaField(0x74)]
    public Tag Entity;
}

// General, parents that reference Entity

[SchemaStruct("30898080", 0x28)]
public struct S30898080
{
    public long FileSize;
    public DynamicArray<S34898080> Unk08;
    public DynamicArray<S33898080> Unk18;
}

[SchemaStruct("34898080", 0x20)]
public struct S34898080
{
}

[SchemaStruct("33898080", 0x20)]
public struct S33898080
{
    public StringPointer TagPath;
    [SchemaField(Tag64 = true)]
    public Tag Tag;  // if .pattern.tft, then Entity - if .budget_set.tft, then parent of itself
    public StringPointer TagNote;
}

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

[SchemaStruct("44318080", 8)]
public struct S44318080
{
    public long FileSize;
    [SchemaField(0x8, Tag64 = true)]
    public Entity? Entity;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "3B9A8080", 0x50)]
public struct S3B9A8080
{
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "8F948080", 0xC8)]
public struct S8F948080
{
    [SchemaField(0xA8)]
    public DynamicArray<S56838080> UnkA8;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "56838080", 0x68)]
public struct S56838080
{
    [SchemaField(0x8)]
    public DynamicArray<S58838080> Table1; // Why...Are these all the same...?
    public DynamicArray<S58838080> Table2;
    public DynamicArray<S58838080> Table3;
    public DynamicArray<S58838080> Table4;
    public DynamicArray<S58838080> Table5;
    public DynamicArray<S58838080> Table6;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "58838080", 0x18)]
public struct S58838080
{
    public ResourceInTablePointer<SMapDataEntry>? Datatable;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "B67E8080", 0x34)]
public struct SB67E8080
{
    [SchemaField(0x20)]
    public StringHash EntityName;
}

#region Named entities

//I think this is the old struct for named bags, it seems like it changed to 1D478080?

//[SchemaStruct("C96C8080", 0x50)]
//public struct S75988080
//{
//    public long FileSize;
//    // [DestinyField(FieldType.RelativePointer)]
//    // public string DestinationGlobalTagBagName;
//    public FileHash DestinationGlobalTagBag;
//    // [SchemaField(0x20)]
//    // public FileHash PatrolTable1;
//    // [SchemaField(0x28), DestinyField(FieldType.RelativePointer)]
//    // public string PatrolTableName;
//    // public FileHash PatrolTable2;
//}

[SchemaStruct("1D478080", 0x18)]
public struct S1D478080
{
    public long FileSize;
    public DynamicArray<SD3598080> DestinationGlobalTagBags;
}

[SchemaStruct("D3598080", 0x10)]
public struct SD3598080
{
    public FileHash DestinationGlobalTagBag;
    [SchemaField(0x8)]
    public StringPointer DestinationGlobalTagBagName;
}

#endregion

#region Audio

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "6E358080", 0x6b8)]
public struct S6E358080
{
    [SchemaField(0x648, TigerStrategy.MARATHON_ALPHA)]
    public DynamicArray<S9B318080> PatternAudioGroups;

    //[SchemaField(0x4E8, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    //[SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    //[SchemaField(0x610, TigerStrategy.DESTINY2_LATEST)] // unsure if actually tag64
    //public Tag<S8080890B> FallbackAudioGroup;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "9B318080", 0x128)]
public struct S9B318080
{
    public TigerHash WeaponContentGroup1Hash;
    [SchemaField(0x8)]
    public TigerHash Unk08;
    //[SchemaField(0x18, Tag64 = true)]
    //public FileHash StringContainer;  // idk why but i presume debug strings, not important

    [SchemaField(0x28, TigerStrategy.MARATHON_ALPHA)]
    public TigerHash WeaponContentGroup2Hash;  // "weaponContentGroupHash" from API
    // theres other stringcontainer stuff but skipping it

    [SchemaField(0xA0, TigerStrategy.MARATHON_ALPHA, Tag64 = true)]
    public Entity? WeaponSkeletonEntity;

    [SchemaField(0xD0, TigerStrategy.MARATHON_ALPHA, Tag64 = true)]
    public Tag<S8080890B> AudioGroup;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "0D8C8080", 0x18)]
public struct S0D8C8080
{
    public long FileSize;
    public DynamicArray<S0F8C8080> Audio;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "0F8C8080", 0x18)]
public struct S0F8C8080
{
    public TigerHash WwiseEventHash;
    [SchemaField(0x8)]
    public DynamicArray<S138C8080> Sounds;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "138C8080", 0x28)]
public struct S138C8080
{
    public short Unk00;
    public short Unk02;
    [SchemaField(0x8)]
    public TigerHash Unk08;
    [SchemaField(0x10)]
    public StringPointer WwiseEventName;

    [SchemaField(TigerStrategy.MARATHON_ALPHA, Tag64 = true)]
    public FileHash Data; // Can be WwiseSound or pattern entity
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "97318080", 0x540)]
public struct S97318080
{
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "F62C8080", 0xB0)]
public struct SF62C8080
{
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "FFFFFFFF", 0x0)] // TODO FIX HASH AND SIZE, CURRENT CONFLICT WITH OLD CLASS HASH
public struct SF42C8080
{
    //[SchemaField(0x2C8, TigerStrategy.MARATHON_ALPHA)]
    //public DynamicArray<SFA2C8080> PatternAudioGroups;

    //[SchemaField(0xD0, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    //[SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    //[SchemaField(0xD0, TigerStrategy.DESTINY2_LATEST, Tag64 = true)]
    //public Entity? FallbackAudio1;

    //[SchemaField(0xF0, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    //[SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    //[SchemaField(0x100, TigerStrategy.DESTINY2_LATEST, Tag64 = true)]
    //public Entity? FallbackAudio2;

    //[SchemaField(0x118, TigerStrategy.DESTINY2_LATEST, Tag64 = true)]
    //public Entity? FallbackAudio3;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "FA2C8080", 0x258)]
public struct SFA2C8080
{
    [SchemaField(0x10, TigerStrategy.MARATHON_ALPHA)]
    public TigerHash WeaponContentGroupHash; // "weaponContentGroupHash" from API
    public TigerHash Unk14;
    public TigerHash Unk18;

    [SchemaField(0x30, TigerStrategy.MARATHON_ALPHA)]
    public TigerHash WeaponTypeHash1; // "weaponTypeHash" from API

    [SchemaField(0x60, TigerStrategy.MARATHON_ALPHA, Tag64 = true)]
    public Tag Unk60;

    [SchemaField(0x78, TigerStrategy.MARATHON_ALPHA, Tag64 = true)]
    public Tag Unk78;

    [SchemaField(0x90, TigerStrategy.MARATHON_ALPHA, Tag64 = true)]
    public Tag Unk90;

    [SchemaField(0xA8, TigerStrategy.MARATHON_ALPHA, Tag64 = true)]
    public Tag UnkA8;

    [SchemaField(0xC0, TigerStrategy.MARATHON_ALPHA, Tag64 = true)]
    public Tag UnkC0;

    [SchemaField(0xD8, TigerStrategy.MARATHON_ALPHA, Tag64 = true)]
    public Tag UnkD8;

    [SchemaField(0xF0, TigerStrategy.MARATHON_ALPHA, Tag64 = true)]
    public Tag<S8080890B> AudioEntityParent;

    [SchemaField(0x120, TigerStrategy.MARATHON_ALPHA)]
    public TigerHash WeaponTypeHash2; // "weaponTypeHash" from API

    [SchemaField(0x130, TigerStrategy.MARATHON_ALPHA, Tag64 = true)]
    public Tag Unk130;

    [SchemaField(0x148, TigerStrategy.MARATHON_ALPHA, Tag64 = true)]
    public Tag Unk148;

    [SchemaField(0x118, TigerStrategy.MARATHON_ALPHA)]
    public ResourcePointer Unk118;

    [SchemaField(0x1C0, TigerStrategy.MARATHON_ALPHA, Tag64 = true)]
    public Tag Unk1C0;

    [SchemaField(0x1D8, TigerStrategy.MARATHON_ALPHA, Tag64 = true)]
    public Tag Unk1D8;

    // public DynamicArray<S87978080> Unk1E8;
    // public DynamicArray<S84978080> Unk1F8;
    // public DynamicArray<S062D8080> Unk208;

    [SchemaField(0x248, TigerStrategy.MARATHON_ALPHA, Tag64 = true)]
    public Tag Unk248;
}

[SchemaStruct("092D8080", 0xA0)]
public struct S092D8080
{
    public long FileSize;
    public TigerHash Unk08;
    [SchemaField(0x18, Tag64 = true)]
    public Entity? Unk18;
    [SchemaField(0x30, Tag64 = true)]
    public Entity? Unk30;
    [SchemaField(0x48, Tag64 = true)]
    public Entity? Unk48;
    [SchemaField(0x60, Tag64 = true)]
    public Entity? Unk60;
    [SchemaField(0x78, Tag64 = true)]
    public Entity? Unk78;
    [SchemaField(0x90, Tag64 = true)]
    public Entity? Unk90;
}


// Turns out this can be used for more than just sounds, recent findings have seen it used for map global channels?
[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "519F8080", 0x390)]
public struct S519F8080
{
    [SchemaField(0x210, TigerStrategy.MARATHON_ALPHA)]
    public DynamicArray<S6EB68080> Array1;

    [SchemaField(0x220, TigerStrategy.MARATHON_ALPHA)]
    public DynamicArray<S6EB68080> Array2;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "6EB68080", 0x18)]
public struct S6EB68080
{
    [SchemaField(0x10, TigerStrategy.MARATHON_ALPHA)]
    public ResourcePointer Unk10; // B9678080, 40668080
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "40668080", 0x68)]
public struct S40668080
{
    [SchemaField(0x28, TigerStrategy.MARATHON_ALPHA, Tag64 = true)]
    public WwiseSound Sound;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "B9678080", 0x110)]
public struct SB9678080
{
    [SchemaField(0x28)]
    public DynamicArray<SBB678080> Unk28;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "BB678080", 0x18)]
public struct SBB678080
{
    [SchemaField(0x10)]
    public Tag<S20698080> FXContainer;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "20698080", 0x40)]
public struct S20698080
{
    public FileHash Unk00;
    [SchemaField(0x18)] // idfk why not having the above FileHash makes this read at 0x0??
    public Material UnkMat;
    [SchemaField(0x20, Tag64 = true)]
    public Tag<S29698080> ModelContainer;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "29698080", 0x18)]
public struct S29698080
{
    [SchemaField(0x10)]
    public DynamicArray<S066F8080> Models;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "066F8080", 4)]
public struct S066F8080
{
    public EntityModel Model;
}

[SchemaStruct("72818080", 0x18)]
public struct S72818080
{
}

[SchemaStruct("00488080", 0x20)]
public struct S00488080
{
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "9AB68080", 0x370)]
public struct S9AB68080
{
}

[SchemaStruct("E3918080", 0x40)]
public struct SE3918080
{
}

[SchemaStruct("0A2D8080", 0x4C)]
public struct S0A2D8080
{
    [SchemaField(0x8, Tag64 = true)]
    public Entity? Unk08;
    [SchemaField(0x20, Tag64 = true)]
    public Entity? Unk20;
    [SchemaField(0x38, Tag64 = true)]
    public Entity? Unk38;
}

#endregion

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "95668080", 0x1E0)]
public struct SMapCubemapResource //Dataresource for cubemaps
{
    [SchemaField(0x20)]
    public Vector4 CubemapSize; //XYZ, no W
    public Vector4 CubemapPosition; // Not actually right afaik

    [SchemaField(0xB0)]
    public long WorldID; // Same as the ID in the datatable entry

    [SchemaField(0x100, TigerStrategy.MARATHON_ALPHA)]
    public Vector4 CubemapRotation;

    [SchemaField(0x1B0, TigerStrategy.MARATHON_ALPHA)]
    public StringPointer CubemapName;

    [SchemaField(0x1B8, TigerStrategy.MARATHON_ALPHA)]
    public Texture CubemapTexture;

    [SchemaField(0x1C0, TigerStrategy.MARATHON_ALPHA)]
    public Texture CubemapIBLTexture; //Sometype of reflection tint texture idk
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "D8928080", 0x190)]
public struct SD8928080
{
    [SchemaField(0x84)]
    public Tag<SMapDataTable> Unk84;
    [SchemaField(0x90)]
    public Vector4 Rotation;
    public Vector4 Translation;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "EF8C8080", 0x60)]
public struct SEF8C8080
{
    [SchemaField(0x58)]
    public Tag<SMapDataTable> Unk58;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "FA988080", 0x80)]
public struct SFA988080
{
    [SchemaField(0x28)]
    public TigerHash FNVHash;
    [SchemaField(0x30)]
    public ulong WorldID;
    [SchemaField(0x58)]
    public DynamicArray<S05998080> Unk58;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "F88C8080", 0x80)]
public struct SF88C8080
{
    [SchemaField(0x28)]
    public TigerHash FNVHash;
    [SchemaField(0x30)]
    public ulong WorldID;
    [SchemaField(0x58)]
    public DynamicArray<S05998080> Unk58;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "EF988080", 0x80)]
public struct SEF988080
{
    [SchemaField(0x28)]
    public TigerHash FNVHash;
    [SchemaField(0x30)]
    public ulong WorldID;
    [SchemaField(0x58)]
    public DynamicArray<S05998080> Unk58;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "6F418080", 0xE0)]
public struct S6F418080
{
    [SchemaField(0x28)]
    public TigerHash FNVHash;
    [SchemaField(0x30)]
    public ulong WorldID;
    [SchemaField(0x58)]
    public DynamicArray<S05998080> Unk58;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "26988080", 0x98)]
public struct S26988080
{
    [SchemaField(0x28)]
    public TigerHash FNVHash;
    [SchemaField(0x30)]
    public ulong WorldID;
    [SchemaField(0x58)]
    public DynamicArray<S05998080> Unk58;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "95468080", 0x90)]
public struct S95468080
{
    [SchemaField(0x28)]
    public TigerHash FNVHash;
    [SchemaField(0x30)]
    public ulong WorldID;
    [SchemaField(0x58)]
    public DynamicArray<S05998080> Unk58;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "05998080", 0x10)]
public struct S05998080
{
    public TigerHash FNVHash;
    [SchemaField(0x8)]
    public ulong WorldID;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "6B908080", 0x28)]
public struct S6B908080
{
    [SchemaField(0x8)]
    public DynamicArray<S029D8080> Unk08;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "029D8080", 0x10)]
public struct S029D8080
{
    public ResourceInTablePointer<S4D898080> Unk00;
    public RelativePointer Unk08;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "4D898080", 0xC)]
public struct S4D898080
{
    public StringPointer Name;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "357C8080", 0x1BD0)]
public struct S357C8080
{
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "18808080", 0x478)]
public struct S18808080
{
    [SchemaField(0x3C0, TigerStrategy.MARATHON_ALPHA)]
    public Tag<S4D7E8080> Unk3C0;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "4D7E8080", 0x30)]
public struct S4D7E8080
{
    [SchemaField(0x2C, TigerStrategy.MARATHON_ALPHA)]
    public StringHash EntityName;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "E05B8080", 0x120)]
public struct SE05B8080
{
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "835B8080", 0x1B0)]
public struct S835B8080
{
    [SchemaField(0x80)]
    public DynamicArray<S625B8080> Unk80;

    [SchemaField(0xF0)]
    public Vector4 Rotation;
    public Vector4 Translation;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "625B8080", 0x80)]
public struct S625B8080
{
    [SchemaField(0x28, Tag64 = true)]
    public Tag<SMapDataTable> DataTable;
    public StringHash Name;
}

#region Gear + Gear attachments

// Relating to base weapon model?
[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "B1328080", 0x170)] // Ent resource 0x10
public struct SB1328080
{
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "B3328080", 0x170)] // Ent resource 0x18
public struct SB3328080
{
    [SchemaField(0x108)]
    public DynamicArray<SBA328080> Unk108;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "BA328080", 0x20)]
public struct SBA328080
{
    public int Unk00;
    public TigerHash Unk04;
    [SchemaField(0x10, Tag64 = true), NoLoad]
    public Entity Entity;
}
//-------------------------------------------

// Relating to weapon attachments?
[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "03468080", 0x2D0)] // Ent resource 0x10
public struct S03468080
{
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "144A8080", 0x2D0)] // Ent resource 0x18
public struct S144A8080
{
    [SchemaField(0x38)]
    public DynamicArray<SCCBA8080> Unk38;

    [SchemaField(0x208)]
    public DynamicArray<S9EBA8080> Unk208;

    [SchemaField(0x280)]
    public DynamicArray<S9EBA8080> Unk280;

    [SchemaField(0x310, Tag64 = true)] // Contains the skeleton for attachments?
    public Entity UnkEntity310;

    [SchemaField(0x320)]
    public DynamicArray<S412F8080> Unk320;

    [SchemaField(0x3A8)]
    public DynamicArray<S0A468080> Unk3A8;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "CCBA8080", 0x4)] // Size unsure
public struct SCCBA8080
{
    public int Unk00;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "9EBA8080", 0x30)]
public struct S9EBA8080
{
    public MapTransform Transform;

    [SchemaField(0x28)]
    public TigerHash Unk28;
    public TigerHash Unk2C;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "412F8080", 0x20)]
public struct S412F8080
{
    public TigerHash Unk00; // Runner name/type
    [SchemaField(0x10, Tag64 = true), NoLoad]
    public Entity UnkEntity;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "0A468080", 0x20)]
public struct S0A468080
{
    public TigerHash Unk00; // Attachment name
    [SchemaField(0x10, Tag64 = true), NoLoad]
    public Entity UnkEntity;
}

//[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "9AB68080", 0x2D0)] // Ent resource 0x10
//public struct S9AB68080 // already exists
//{
//}

//[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "9AB68080", 0x2D0)] // Ent resource 0x18
//public struct S519F8080 // already exists
//{
//}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "98A38080", 0x100)]
public struct S98A38080
{
    public TigerHash Unk00;

    [SchemaField(0x70, Tag64 = true), NoLoad]
    public Entity Entity;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "87328080", 0x9F0)] // Ent resource 0x10
public struct S87328080
{
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "88328080", 0x750)] // Ent resource 0x18
public struct S88328080
{
    [SchemaField(0x6B0, Tag64 = true), NoLoad]
    public Entity Entity;
}

#endregion
