using Tiger.Schema.Audio;
using Tiger.Schema.Entity;
using Tiger.Schema.Strings;

namespace Tiger.Schema.Activity.MARATHON;

[SchemaStruct(TigerStrategy.MARATHON, 0x8080B386, 0x78)] // 86B38080
public struct SActivity
{
    public long FileSize;

    [SchemaField(0x8, TigerStrategy.MARATHON)]
    public TigerHash LocationName;  // these all have actual string hashes but have no string container given directly
    public TigerHash Unk0C;
    public TigerHash Unk10;
    public TigerHash Unk14;
    public ResourcePointer Unk18;  // 6A988080 + 20978080

    [SchemaField(0x20, Tag64 = true)]
    public Tag<S8080B383> Destination;  // S8080B383

    [SchemaField(0x40, TigerStrategy.MARATHON)]
    public DynamicArray<S80808926> Unk40; // unsure if exists in marathon atm
    public DynamicArray<S8080AB3E> Unk50;

    [SchemaField(0x60, TigerStrategy.MARATHON)]
    public TigerHash Unk60;
    public FileHash Unk64;  // an entity thing

    [SchemaField(0x68, TigerStrategy.MARATHON, Tag64 = true)]
    public Tag AmbientActivity;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x8080B383, 0x78)] // 83B38080
public struct S8080B383
{
    public long FileSize;
    public StringHash LocationName;

    [SchemaField(0x10, TigerStrategy.MARATHON, Tag64 = true)]
    public LocalizedStrings StringContainer;
    public FileHash Events;
    public FileHash Patrols;
    public uint Unk28;
    public FileHash Unk2C;
    public DynamicArray<SDE448080> TagBags; // unsure if exists in marathon atm

    [SchemaField(0x48, TigerStrategy.MARATHON)]
    public DynamicArray<S8080AB62> Activities;
    public StringPointer DestinationName;
}

[SchemaStruct("DE448080", 4)] // DE448080
public struct SDE448080
{
    public Tag Unk00;
}

[SchemaStruct(0x8080AB62, 0x18)] // 62AB8080
public struct S8080AB62
{
    public TigerHash ShortActivityName;
    [SchemaField(0x8)]
    public TigerHash Unk08;
    public TigerHash Unk10;
    public StringPointer ActivityName;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x80808926, 0x58)] // 26898080
public struct S80808926
{
    public TigerHash LocationName;
    public TigerHash ActivityName;
    public TigerHash BubbleName;
    public TigerHash Unk0C;
    public TigerHash Unk10;

    [SchemaField(0x18, TigerStrategy.MARATHON)]
    public TigerHash BubbleName2;

    [SchemaField(0x20, TigerStrategy.MARATHON)]
    public TigerHash Unk20;
    public TigerHash Unk24;
    public TigerHash Unk28;

    [SchemaField(0x30, TigerStrategy.MARATHON)]
    public int Unk30;

    [SchemaField(0x38, TigerStrategy.MARATHON)]
    public DynamicArray<S8080B463> Unk38;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x8080B463, 0x18)] // 63B48080
public struct S8080B463
{
    public TigerHash LocationName;
    public TigerHash ActivityName;
    public StringHash BubbleName;
    public TigerHash ActivityPhaseName;
    public TigerHash ActivityPhaseName2;
    public Tag<S8080B381> UnkEntityReference;
}

[SchemaStruct(0x8080B381, 0x30)] // 81B38080
public struct S8080B381
{
    public long FileSize;
    public long Unk08;
    public ResourcePointer Unk10;  // 34AB8080, 
    [SchemaField(0x18)]
    public Tag Unk18;  // S8080B381 entity script stuff
}

[SchemaStruct(0x8080AB34, 0x58)] // 34AB8080
public struct S8080AB34
{
    [SchemaField(Tag64 = true)]
    public Tag DialogueTable;
}

[SchemaStruct(0x8080AB38, 0x58)] // 38AB8080
public struct S8080AB38
{
    [SchemaField(Tag64 = true)]
    public Tag DialogueTable;
}

[SchemaStruct(TigerStrategy.MARATHON, "18978080", 0x20)]
public struct S18978080
{
    [SchemaField(TigerStrategy.MARATHON, Tag64 = true)]
    public Tag DialogueTable;

    [SchemaField(TigerStrategy.MARATHON)]
    public TigerHash Unk10;

    [SchemaField(0x18, TigerStrategy.MARATHON)]
    public TigerHash Unk18;

    [SchemaField(0x1C, TigerStrategy.MARATHON)]
    public Tag<SMusicTemplate> Unk1C;
}

[SchemaStruct("17978080", 0x20)]
public struct S17978080
{
    [SchemaField(Tag64 = true)]
    public Tag DialogueTable;
    public TigerHash Unk10;
    [SchemaField(0x18)]
    public TigerHash Unk18;
    public int Unk1C;
}

/// <summary>
/// Generally used in ambients to provide dialogue and music together.
/// </summary>
[SchemaStruct(TigerStrategy.MARATHON, "D5908080", 0x50)]
public struct SD5908080
{
    [SchemaField(TigerStrategy.MARATHON, Tag64 = true)]
    public Tag DialogueTable;

    [SchemaField(0x38, TigerStrategy.MARATHON)]
    public Tag<SMusicTemplate> Music;
}

/// <summary>
/// Stores static map data for activities
/// </summary>
[SchemaStruct(TigerStrategy.MARATHON, 0x8080AB3E, 0x38)] // 3EAB8080
public struct S8080AB3E
{
    public TigerHash LocationName;
    public TigerHash ActivityName;
    public StringHash BubbleName;

    [SchemaField(0x18, TigerStrategy.MARATHON)]
    public DynamicArray<S8080B463> Unk18;
    public DynamicArray<S8080AB26> MapReferences;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x8080AB26, 0x10)] // 26AB8080
public struct S8080AB26
{
    [SchemaField(Tag64 = true)]
    public Tag<SBubbleParent> MapReference;
}

[SchemaStruct("53418080", 0x20)]
public struct S53418080
{
    public TigerHash Unk00;
    public TigerHash Unk04;
    [SchemaField(0xC)]
    public int Unk0C;
}

[SchemaStruct("54418080", 0x40)]
public struct S54418080
{
    public TigerHash Unk00;
    public TigerHash Unk04;
    [SchemaField(0xC)]
    public int Unk0C;
}

[SchemaStruct("0F978080", 0x40)]
public struct S0F978080
{
    public StringPointer BubbleName;
    public TigerHash Unk08;
    public TigerHash Unk0C;
    public TigerHash Unk10;
    [SchemaField(0x28)]
    public long Unk28;
    public DynamicArray<SDD978080> Unk30;
}

[SchemaStruct("DD978080", 0x10)]
public struct SDD978080
{
    public TigerHash Unk00;
    public TigerHash Unk04;
    public TigerHash Unk08;
}

/// <summary>
/// Directive table + audio links for activity directives.
/// </summary>
/// 
[SchemaStruct(TigerStrategy.MARATHON, "6A988080", 0x84)]
public struct S6A988080
{
    public DynamicArray<S28898080> DirectiveTables;
    public DynamicArray<SB7978080> DialogueTables;
    public TigerHash StartingBubbleName;
    public TigerHash Unk24;

    [SchemaField(0x2C, TigerStrategy.MARATHON)]
    public Tag<SMusicTemplate> Music;

    [SchemaField(0x60, TigerStrategy.MARATHON)]
    public StringPointer DescentMusicPath;

    [SchemaField(Tag64 = true)]
    public Entity.Entity DescentMusic;

    [SchemaField(0x7C, TigerStrategy.MARATHON)]
    public Tag DescentMisc; // C7978080, contains anim clips and models used when loading into destination
}

[SchemaStruct("A4BC8080", 0x18)]
public struct SA4BC8080
{
    [SchemaField(0x8)]
    public DynamicArray<SA6BC8080> Unk08;
}

[SchemaStruct("A6BC8080", 0x18)]
public struct SA6BC8080
{
    [SchemaField(Tag64 = true)]
    public WwiseSound Sound;
}

/// <summary>
/// Directive table for public events so no audio linked.
/// </summary>
[SchemaStruct("20978080", 0x38)]
public struct S20978080
{
    public DynamicArray<S28898080> PEDirectiveTables;
    [SchemaField(0x20)]
    public TigerHash StartingBubbleName;
    [SchemaField(0x2C)]
    public Tag<SMusicTemplate> Music;
}

[SchemaStruct("28898080", 4)]
public struct S28898080
{
    public Tag<SC78E8080> DirectiveTable;
}

[SchemaStruct(TigerStrategy.MARATHON, "B7978080", 0x14)]
public struct SB7978080
{
    [SchemaField(Tag64 = true)]
    public Tag<SDialogueTable> DialogueTable;
}

[SchemaStruct("C78E8080", 0x18)]
public struct SC78E8080
{
    public long FileSize;
    public DynamicArray<SC98E8080> DirectiveTable;
}

[SchemaStruct(TigerStrategy.MARATHON, "C98E8080", 0x80)]
public struct SC98E8080
{
    public TigerHash Hash;

    [SchemaField(0x10, TigerStrategy.MARATHON)]
    public StringReference64 NameString;

    [SchemaField(0x28, TigerStrategy.MARATHON)]
    public StringReference64 DescriptionString;

    [SchemaField(0x40, TigerStrategy.MARATHON)]
    public StringReference64 ObjectiveString;

    [SchemaField(0x58, TigerStrategy.MARATHON)]
    public StringReference64 Unk58;

    [SchemaField(0x70, TigerStrategy.MARATHON)]
    public int ObjectiveTargetCount;
}

[SchemaStruct("0B978080", 0x38)]
public struct S0B978080
{
    public StringPointer BubbleName;
    public TigerHash Unk08;
    public TigerHash Unk0C;
    public TigerHash Unk10;
    [SchemaField(0x40)]
    public DynamicArray<SUInt64> Unk40;
}

#region Audio

[SchemaStruct(TigerStrategy.MARATHON, "EB458080", 0x38)]
public struct SMusicTemplate
{
    public long FileSize;

    [SchemaField(TigerStrategy.MARATHON)]
    public StringPointer MusicTemplateName;
    public Tag MusicTemplateTag; // F0458080

    [SchemaField(0x28, TigerStrategy.MARATHON)]
    public DynamicArray<SED458080> Unk28;
}

[SchemaStruct(TigerStrategy.MARATHON, "ED458080", 8)]
public struct SED458080
{
    public ResourcePointer Unk00;
}

[SchemaStruct(TigerStrategy.MARATHON, "FFFFFFFF", 0x0)] // TODO FIX HASH AND SIZE, CURRENT CONFLICT WITH OLD CLASS HASH
public struct SF5458080
{
    [SchemaField(TigerStrategy.MARATHON)]
    public StringPointer WwiseMusicLoopName;
    public WwiseSound MusicLoopSound;

    [SchemaField(0x18, TigerStrategy.MARATHON)]
    public DynamicArray<SFB458080> Unk18;
}

[SchemaStruct("F7458080", 0x28)]
public struct SF7458080
{
    public StringPointer AmbientMusicSetName;
    [SchemaField(0x8, Tag64 = true)]
    public Tag<S50968080> AmbientMusicSet;
    public DynamicArray<SFA458080> Unk18;
}

[SchemaStruct("50968080", 0x20)]
public struct S50968080
{
    public long FileSize;
    public DynamicArray<S318A8080> Unk08;
    public TigerHash Unk18;
}

[SchemaStruct("318A8080", 0x30)]
public struct S318A8080
{
    [SchemaField(Tag64 = true)]
    public WwiseSound MusicLoopSound;
    public float Unk10;
    public TigerHash Unk14;
    public float Unk18;
    public TigerHash Unk1C;
    public float Unk20;
    public TigerHash Unk24;
    public int Unk28;
}

[SchemaStruct(TigerStrategy.MARATHON, "FA458080", 0x20)]
public struct SFA458080
{
    public TigerHash Unk00;
    [SchemaField(8, TigerStrategy.MARATHON)]
    public StringPointer EventName;

    [SchemaField(0x10, TigerStrategy.MARATHON)]
    public TigerHash EventHash;
}

[SchemaStruct(TigerStrategy.MARATHON, "FB458080", 0x20)]
public struct SFB458080
{
    public TigerHash Unk00;

    [SchemaField(0x8, TigerStrategy.MARATHON)]
    public StringPointer EventName;

    [SchemaField(0x18, TigerStrategy.MARATHON)]
    public TigerHash EventHash;
}

[SchemaStruct("F0458080", 0x28)]
public struct SF0458080
{
    public long FileSize;
    public int Unk08;
    public int Unk0C;
    public int WwiseSwitchKey;
}

[SchemaStruct(TigerStrategy.MARATHON, "E6BF8080", 0x38)]
public struct SUnkMusicE6BF8080
{
    [SchemaField(0x18, TigerStrategy.MARATHON, Tag64 = true)]
    public Tag Unk18;

    [SchemaField(0x28, TigerStrategy.MARATHON)]
    public DynamicArray<SUnkMusicE8BF8080> Unk28;
}

[SchemaStruct(TigerStrategy.MARATHON, "E8BF8080", 0x30)]
public struct SUnkMusicE8BF8080
{
    [SchemaField(0, TigerStrategy.MARATHON)]
    public TigerHash EventHash;

    [SchemaField(0x08, TigerStrategy.MARATHON)]
    public StringPointer EventDescription;
}

[SchemaStruct("8080B3EA", 0x20)]
public struct S8080B3EA
{
    public long FileSize;
    public DynamicArray<S81AB8080> EntityComponents;
}

[SchemaStruct("81AB8080", 0x4)]
public struct S81AB8080
{
    public Tag<S8080AB82> EntityComponentParent;
}

[SchemaStruct("8080AB82", 0x28)]
public struct S8080AB82
{
    public long FileSize;
    public TigerHash Unk08;
    [SchemaField(0x20)]
    public EntityComponent EntityComponent;
}

#endregion
