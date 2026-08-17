namespace Tiger.Schema.Audio;

[SchemaStruct(0x8080B4D1, 0x28)] // D1B48080
public struct SDialogueTable
{
    public long FileSize;
    public DynamicArray<S8080AA33> Unk08;
    public DynamicArray<S8080AA34> Unk18;
}

[SchemaStruct(0x8080AA33, 8)] // 33AA8080
public struct S8080AA33
{
    public TigerHash Unk00;
}

[SchemaStruct(0x8080AA34, 0x10)] // 34AA8080
public struct S8080AA34
{
    public TigerHash Unk00;
    [SchemaField(0x8)]
    public ResourcePointer Unk08;
}

/// <summary>
/// Group of S3FAA8080, used for accessing random sounds to play out of a bundle.
/// </summary>
[SchemaStruct(0x8080AA3B, 0x28)] // 3BAA8080
public struct S8080AA3B
{
    [SchemaField(Tag64 = true)]
    public Tag Unk00;

    [SchemaField(0x20)]
    public ResourcePointer Unk20; // 3FAA8080
}

[SchemaStruct(TigerStrategy.MARATHON, 0x8080AA38, 0x38)] // 38AA8080
public struct S8080AA38
{
    [SchemaField(0x28, TigerStrategy.MARATHON)]
    public DynamicArray<S8080AA3B> Unk28;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x8080AA3F, 0xA0)] // 3FAA8080
public struct S8080AA3F
{
    // Male
    [SchemaField(0x20, TigerStrategy.MARATHON, Tag64 = true)]
    public WwiseSound SoundM;

    [SchemaField(0x30, TigerStrategy.MARATHON)]
    public StringReference64 VoicelineM;

    // Female, Unsure if there are any differences in Marathon
    //[SchemaField(0x50, TigerStrategy.MARATHON, Tag64 = true)]
    //public WwiseSound SoundF;

    //[SchemaField(0x68, TigerStrategy.MARATHON)]
    //public StringReference64 VoicelineF;

    [SchemaField(0x94, TigerStrategy.MARATHON)]
    public StringHash NarratorString;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x8080AA35, 0x30)] // 35AA8080
public struct S8080AA35
{
    [SchemaField(0x30, TigerStrategy.MARATHON)]
    public DynamicArray<S8080AA3A> Unk30;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x8080AA3A, 0x48)] // 3AAA8080
public struct S8080AA3A
{
    [SchemaField(0x10, TigerStrategy.MARATHON, Tag64 = true)]
    public Tag Unk00;

    [SchemaField(0x40, TigerStrategy.MARATHON)]
    public ResourcePointer Unk40; //33978080 or 38AA8080
}

[SchemaStruct(TigerStrategy.MARATHON, 0x8080B128, 0x38)]
public struct SWwiseSound
{
    public long FileSize;
    public StringHash SoundbankName;

    [SchemaField(0x18, TigerStrategy.MARATHON)]
    public FileHash Soundbank; //Tag<S8080B12D>, Can be an 84 tag sometimes

    [SchemaField(0x20, TigerStrategy.MARATHON)]
    public DynamicArray<Wem> Wems;

    public Tag<S8080B12D> GetSoundbank()
    {
        byte last = (byte)(Soundbank & 0xFF);
        bool is84Tag = last == 0x84;
        if (is84Tag)
            return null;

        return FileResourcer.Get().GetSchemaTag<S8080B12D>(Soundbank);
    }
}

[SchemaStruct(0x8080B12D, 4)]
public struct S8080B12D
{
    public BKHD SoundBank;
}





