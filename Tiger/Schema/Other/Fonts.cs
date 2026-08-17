namespace Tiger.Schema.Other;

[SchemaStruct(TigerStrategy.MARATHON, "808050B7", 0x18)]
public struct S808050B7
{
    public long FileSize;
    public DynamicArray<SB9508080> FontParents;
}

[SchemaStruct(TigerStrategy.MARATHON, "B9508080", 0x04)]
public struct SB9508080
{
    public Tag<S808050BA> FontParent;
}

[SchemaStruct(TigerStrategy.MARATHON, "808050BA", 0x20)]
public struct S808050BA
{
    public long FileSize;
    public TigerFile FontFile;
    [SchemaField(0x10)]
    public StringPointer FontName;
    public long FontFileSize;
}
