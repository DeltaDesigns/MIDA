
namespace Tiger.Schema.Strings;

[SchemaStruct(TigerStrategy.MARATHON, 0x8080B9B8, 0x50)] // B8B98080
public struct SLocalizedStrings
{
    public ulong ThisSize;
    public SortedDynamicArray<SStringHash> StringHashes;
    // only working with english rn for speed
    public LocalizedStringsData EnglishStringsData;
}

[SchemaStruct(0x80800070, 0x4)] // 70008080
public struct SStringHash
{
    public StringHash StringHash;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x808099F1, 0x48)] // F1998080
public struct SLocalizedStringsData
{
    public long ThisSize;
    public DynamicArrayUnloaded<SStringPart> StringParts;
    // might be a colour table here

    [SchemaField(0x28, TigerStrategy.MARATHON)]
    public DynamicArrayUnloaded<SInt8> StringCharacters;
    public DynamicArrayUnloaded<SStringPartDefinition> StringCombinations;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x808099F7, 0x20)] // F7998080
public struct SStringPart
{
    [SchemaField(0x8)]
    public RelativePointer StringPartDefinitionPointer;    // this doesn't get accessed so no need to make this easy to access
    // public TigerHash Unk10;
    [SchemaField(0x14)]
    public ushort ByteLength;    // these can differ if multibyte unicode
    public ushort StringLength;
    public ushort CipherShift;    // now always zero
}

[SchemaStruct(TigerStrategy.MARATHON, 0x808099F5, 0x10)] // F5998080
public struct SStringPartDefinition
{
    public RelativePointer StartStringPartPointer;
    public long PartCount;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x808050A1, 0x68)] // A1508080
public struct S808050A1
{
    [SchemaField(0x28, TigerStrategy.MARATHON)]
    public DynamicArray<S808050B6> Unk28;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x808050B6, 0x28)] // B6508080
public struct S808050B6
{
    //[SchemaField(Tag64 = true)]
    //public Tag Unk00; // Always FFFFFFFF?

    [SchemaField(0x10, TigerStrategy.MARATHON, Tag64 = true)]
    public Tag Unk10; // Can be string container or something else
}
