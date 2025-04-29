namespace Tiger.Schema.Audio;

[SchemaStruct("8080B4D1", 0x28)]
public struct SDialogueTable
{
    public long FileSize;
    public DynamicArray<S33AA8080> Unk08;
    public DynamicArray<S34AA8080> Unk18;
}

[SchemaStruct("33AA8080", 8)]
public struct S33AA8080
{
    public TigerHash Unk00;
}

[SchemaStruct("34AA8080", 0x10)]
public struct S34AA8080
{
    public TigerHash Unk00;
    [SchemaField(0x8)]
    public ResourcePointer Unk08;
}

/// <summary>
/// Group of S3FAA8080, used for accessing random sounds to play out of a bundle.
/// </summary>
[SchemaStruct("3BAA8080", 0x28)]
public struct S3BAA8080
{
    [SchemaField(Tag64 = true)]
    public Tag Unk00;

    [SchemaField(0x20)]
    public ResourcePointer Unk20; // 3FAA8080
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "38AA8080", 0x38)]
public struct S38AA8080
{
    [SchemaField(0x28, TigerStrategy.MARATHON_ALPHA)]
    public DynamicArray<S3BAA8080> Unk28;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "3FAA8080", 0x90)]
public struct S3FAA8080
{
    // Male
    [SchemaField(0x20, TigerStrategy.MARATHON_ALPHA, Tag64 = true)]
    public WwiseSound SoundM;

    [SchemaField(0x30, TigerStrategy.MARATHON_ALPHA)]
    public StringReference64 VoicelineM;

    // Female
    [SchemaField(0x50, TigerStrategy.MARATHON_ALPHA, Tag64 = true)]
    public WwiseSound SoundF;

    [SchemaField(0x60, TigerStrategy.MARATHON_ALPHA)]
    public StringReference64 VoicelineF;

    [SchemaField(0x70, TigerStrategy.MARATHON_ALPHA)]
    public StringHash NarratorString;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "35AA8080", 0x30)]
public struct S35AA8080
{
    [SchemaField(0x30, TigerStrategy.MARATHON_ALPHA)]
    public DynamicArray<S3AAA8080> Unk30;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "3AAA8080", 0x48)]
public struct S3AAA8080
{
    [SchemaField(0x10, TigerStrategy.MARATHON_ALPHA, Tag64 = true)]
    public Tag Unk00;

    [SchemaField(0x40, TigerStrategy.MARATHON_ALPHA)]
    public ResourcePointer Unk40; //33978080 or 38AA8080
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "38978080", 0x38)]
public struct S38978080
{
    public long FileSize;
    public StringHash SoundbankName;

    [SchemaField(0x18, TigerStrategy.MARATHON_ALPHA)]
    public Tag<S63838080> Soundbank;

    [SchemaField(0x20, TigerStrategy.MARATHON_ALPHA)]
    public DynamicArray<Wem> Wems;
}

[SchemaStruct("418A8080", 0x38)]
public struct S418A8080
{
    public long Unk00;
    public float Unk08;
}

[SchemaStruct("63838080", 4)]
public struct S63838080
{
    public BKHD SoundBank;
}

[SchemaStruct("438A8080", 0x28)]
public struct S438A8080
{
    public long FileSize;
}





