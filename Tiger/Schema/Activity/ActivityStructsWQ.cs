using Tiger.Schema.Audio;
using Tiger.Schema.Entity;
using Tiger.Schema.Strings;

namespace Tiger.Schema.Activity.MARATHON_ALPHA;

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "8080B386", 0x78)]
public struct SActivity
{
    public long FileSize;

    [SchemaField(0x8, TigerStrategy.MARATHON_ALPHA)]
    public TigerHash LocationName;  // these all have actual string hashes but have no string container given directly
    public TigerHash Unk0C;
    public TigerHash Unk10;
    public TigerHash Unk14;
    public ResourcePointer Unk18;  // 6A988080 + 20978080
    public FileHash64 Destination;  // S8B8E8080

    [SchemaField(0x40, TigerStrategy.MARATHON_ALPHA)]
    public DynamicArray<S26898080> Unk40;
    public DynamicArray<S3EAB8080> Unk50;

    [SchemaField(0x60, TigerStrategy.MARATHON_ALPHA)]
    public TigerHash Unk60;
    public FileHash Unk64;  // an entity thing

    [SchemaField(0x68, TigerStrategy.MARATHON_ALPHA, Tag64 = true)]
    public Tag AmbientActivity;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "8B8E8080", 0x78)]
public struct S8B8E8080
{
    public long FileSize;
    public StringHash LocationName;

    [SchemaField(0x10, TigerStrategy.MARATHON_ALPHA, Tag64 = true)]
    public LocalizedStrings StringContainer;
    public FileHash Events;
    public FileHash Patrols;
    public uint Unk28;
    public FileHash Unk2C;
    public DynamicArray<SDE448080> TagBags;

    [SchemaField(0x48, TigerStrategy.MARATHON_ALPHA)]
    public DynamicArray<S2E898080> Activities;
    public StringPointer DestinationName;
}

[SchemaStruct("DE448080", 4)]
public struct SDE448080
{
    public Tag Unk00;
}

[SchemaStruct("2E898080", 0x18)]
public struct S2E898080
{
    public TigerHash ShortActivityName;
    [SchemaField(0x8)]
    public TigerHash Unk08;
    public TigerHash Unk10;
    public StringPointer ActivityName;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "26898080", 0x58)]
public struct S26898080
{
    public TigerHash LocationName;
    public TigerHash ActivityName;
    public TigerHash BubbleName;
    public TigerHash Unk0C;
    public TigerHash Unk10;

    [SchemaField(0x18, TigerStrategy.MARATHON_ALPHA)]
    public TigerHash BubbleName2;

    [SchemaField(0x20, TigerStrategy.MARATHON_ALPHA)]
    public TigerHash Unk20;
    public TigerHash Unk24;
    public TigerHash Unk28;

    [SchemaField(0x30, TigerStrategy.MARATHON_ALPHA)]
    public int Unk30;

    [SchemaField(0x38, TigerStrategy.MARATHON_ALPHA)]
    public DynamicArray<S48898080> Unk38;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "63B48080", 0x18)]
public struct S48898080
{
    public TigerHash LocationName;
    public TigerHash ActivityName;
    public StringHash BubbleName;
    public TigerHash ActivityPhaseName;
    public TigerHash ActivityPhaseName2;
    public Tag<S898E8080> UnkEntityReference;
}

[SchemaStruct("898E8080", 0x30)]
public struct S898E8080
{
    public long FileSize;
    public long Unk08;
    public ResourcePointer Unk10;  // 46938080 has dialogue table, 45938080 unk, 19978080 unk
    [SchemaField(0x18)]
    public Tag Unk18;  // S898E8080 entity script stuff
}

[SchemaStruct("46938080", 0x58)]
public struct S46938080
{
    [SchemaField(Tag64 = true)]
    public Tag DialogueTable;
    [SchemaField(0x3C)]
    public int Unk3C;
    public float Unk40;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "19978080", 0x20)]
public struct S19978080
{
    [SchemaField(TigerStrategy.MARATHON_ALPHA, Tag64 = true)]
    public Tag DialogueTable;
    public TigerHash Unk10;
}

[SchemaStruct("C39F8080", 0x18)]
public struct SC39F8080
{
    // TODO: are these actually obsolete in wq+?
    //[SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    public StringPointer DirectiveTableContentString;
    //[SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    public Tag<SC78E8080> DirectiveTable;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "18978080", 0x20)]
public struct S18978080
{
    [SchemaField(TigerStrategy.MARATHON_ALPHA, Tag64 = true)]
    public Tag DialogueTable;

    [SchemaField(TigerStrategy.MARATHON_ALPHA)]
    public TigerHash Unk10;

    [SchemaField(0x18, TigerStrategy.MARATHON_ALPHA)]
    public TigerHash Unk18;

    [SchemaField(0x1C, TigerStrategy.MARATHON_ALPHA)]
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

[SchemaStruct("45938080", 0x58)]
public struct S45938080
{
    [SchemaField(Tag64 = true)]
    public Tag DialogueTable;
    [SchemaField(0x18)]
    public DynamicArray<S28998080> Unk18;
    [SchemaField(0x3C)]
    public int Unk3C;
    public float Unk40;
}

[SchemaStruct("44938080", 0x58)]
public struct S44938080
{
    [SchemaField(Tag64 = true)]
    public Tag DialogueTable;
    [SchemaField(0x18)]
    public DynamicArray<S28998080> Unk18;
    [SchemaField(0x3C)]
    public int Unk3C;
    public float Unk40;
    public TigerHash Unk44;
    [SchemaField(0x50)]
    public int Unk50;
}

/// <summary>
/// Generally used in ambients to provide dialogue and music together.
/// </summary>
[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "D5908080", 0x50)]

public struct SD5908080
{
    [SchemaField(TigerStrategy.MARATHON_ALPHA, Tag64 = true)]
    public Tag DialogueTable;

    [SchemaField(0x38, TigerStrategy.MARATHON_ALPHA)]
    public Tag<SMusicTemplate> Music;

    [SchemaField(0x18, TigerStrategy.MARATHON_ALPHA)]
    public DynamicArray<S28998080> Unk20;
}

[SchemaStruct("28998080", 0x10)]
public struct S28998080
{
    public TigerHash Unk00;
    public TigerHash Unk04;
    public TigerHash Unk08;
}

[SchemaStruct("1A978080", 0x18)]
public struct S1A978080
{
    [SchemaField(Tag64 = true)]
    public Tag Unk00;
}

[SchemaStruct("478F8080", 0x18)]
public struct S478F8080
{
    [SchemaField(Tag64 = true)]
    public Tag Unk00;
}

/// <summary>
/// Stores static map data for activities
/// </summary>
[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "3EAB8080", 0x38)]
public struct S3EAB8080
{
    public TigerHash LocationName;
    public TigerHash ActivityName;
    public StringHash BubbleName;

    [SchemaField(0x10, TigerStrategy.MARATHON_ALPHA)]
    public ResourcePointer Unk10;  // 0F978080, 53418080
    public DynamicArray<S48898080> Unk18;
    public DynamicArray<S26AB8080> MapReferences;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "26AB8080", 0x10)]
public struct S26AB8080
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
[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "6A988080", 0x84)]
public struct S6A988080
{
    public DynamicArray<S28898080> DirectiveTables;
    public DynamicArray<SB7978080> DialogueTables;
    public TigerHash StartingBubbleName;
    public TigerHash Unk24;

    [SchemaField(0x2C, TigerStrategy.MARATHON_ALPHA)]
    public Tag<SMusicTemplate> Music;

    [SchemaField(0x60, TigerStrategy.MARATHON_ALPHA)]
    public StringPointer DescentMusicPath;

    [SchemaField(Tag64 = true)]
    public Entity.Entity DescentMusic;

    [SchemaField(0x7C, TigerStrategy.MARATHON_ALPHA)]
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

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "B7978080", 0x14)]
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

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "C98E8080", 0x80)]
public struct SC98E8080
{
    public TigerHash Hash;

    [SchemaField(0x10, TigerStrategy.MARATHON_ALPHA)]
    public StringReference64 NameString;

    [SchemaField(0x28, TigerStrategy.MARATHON_ALPHA)]
    public StringReference64 DescriptionString;

    [SchemaField(0x40, TigerStrategy.MARATHON_ALPHA)]
    public StringReference64 ObjectiveString;

    [SchemaField(0x58, TigerStrategy.MARATHON_ALPHA)]
    public StringReference64 Unk58;

    [SchemaField(0x70, TigerStrategy.MARATHON_ALPHA)]
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
    public DynamicArray<SUint64> Unk40;
}

#region Audio

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "EB458080", 0x38)]
public struct SMusicTemplate
{
    public long FileSize;

    [SchemaField(TigerStrategy.MARATHON_ALPHA)]
    public StringPointer MusicTemplateName;
    public Tag MusicTemplateTag; // F0458080

    [SchemaField(0x28, TigerStrategy.MARATHON_ALPHA)]
    public DynamicArray<SED458080> Unk28;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "ED458080", 8)]
public struct SED458080
{
    public ResourcePointer Unk00;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "FFFFFFFF", 0x0)] // TODO FIX HASH AND SIZE, CURRENT CONFLICT WITH OLD CLASS HASH
public struct SF5458080
{
    [SchemaField(TigerStrategy.MARATHON_ALPHA)]
    public StringPointer WwiseMusicLoopName;
    public WwiseSound MusicLoopSound;

    [SchemaField(0x18, TigerStrategy.MARATHON_ALPHA)]
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

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "FA458080", 0x20)]
public struct SFA458080
{
    public TigerHash Unk00;
    [SchemaField(8, TigerStrategy.MARATHON_ALPHA)]
    public StringPointer EventName;

    [SchemaField(0x10, TigerStrategy.MARATHON_ALPHA)]
    public TigerHash EventHash;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "FB458080", 0x20)]
public struct SFB458080
{
    public TigerHash Unk00;

    [SchemaField(0x8, TigerStrategy.MARATHON_ALPHA)]
    public StringPointer EventName;

    [SchemaField(0x18, TigerStrategy.MARATHON_ALPHA)]
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

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "E6BF8080", 0x38)]
public struct SUnkMusicE6BF8080
{
    [SchemaField(0x18, TigerStrategy.MARATHON_ALPHA, Tag64 = true)]
    public Tag Unk18;

    [SchemaField(0x28, TigerStrategy.MARATHON_ALPHA)]
    public DynamicArray<SUnkMusicE8BF8080> Unk28;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "E8BF8080", 0x30)]
public struct SUnkMusicE8BF8080
{
    [SchemaField(0, TigerStrategy.MARATHON_ALPHA)]
    public TigerHash EventHash;

    [SchemaField(0x08, TigerStrategy.MARATHON_ALPHA)]
    public StringPointer EventDescription;
}

[SchemaStruct("BE8E8080", 0x20)]
public struct SBE8E8080
{
    public long FileSize;
    public DynamicArray<S42898080> EntityResources;
}

[SchemaStruct("42898080", 0x4)]
public struct S42898080
{
    public Tag<S43898080> EntityResourceParent;
}

[SchemaStruct("43898080", 0x28)]
public struct S43898080
{
    public long FileSize;
    public TigerHash Unk08;
    [SchemaField(0x20)]
    public EntityResource EntityResource;
}

#endregion
