using Tiger.Schema.Strings;

namespace Tiger.Schema.Investment;

/// <summary>
/// Stores all the inventory item definitions in a huge hashmap.
/// </summary>
[SchemaStruct(TigerStrategy.MARATHON, 0x80809685, 0x18)]
public struct S80809685
{
    public long FileSize;
    public DynamicArrayUnloaded<S80809689> InventoryItemDefinitionEntries;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x80809689, 0x20)]
public struct S80809689
{
    public TigerHash InventoryItemHash;
    [SchemaField(0x10), NoLoad]
    public InventoryItem InventoryItem;
}

#region InventoryItemDefinition

/// <summary>
/// Inventory item definition.
/// </summary>
[SchemaStruct(TigerStrategy.MARATHON, 0x8080968B, 0x178)]
public struct SInventoryItem
{
    public long FileSize;

    [SchemaField(0x08, TigerStrategy.MARATHON)]
    public ResourcePointer Unk08;  // 6D938080
    [SchemaField(0x10, TigerStrategy.MARATHON)]
    public ResourcePointer Unk10;  // 89928080
    [SchemaField(0x28, TigerStrategy.MARATHON)]
    public ResourcePointer Unk28;  // 31688080
    [SchemaField(0x70, TigerStrategy.MARATHON)]
    public ResourcePointer Unk70;  // 45928080
    [SchemaField(0x80, TigerStrategy.MARATHON)]
    public ResourcePointer Unk80;  // 0x808091D3
    [SchemaField(0x88, TigerStrategy.MARATHON)]
    public ResourcePointer Unk88;  // CF918080
    [SchemaField(0xA0, TigerStrategy.MARATHON)]
    public ResourcePointer UnkA0;  // 95928080
    [SchemaField(0xA8, TigerStrategy.MARATHON)]
    public ResourcePointer UnkA8;  // 93928080
    [SchemaField(0xB0, TigerStrategy.MARATHON)]
    public ResourcePointer UnkB0;  // 7F928080
    [SchemaField(0xD0, TigerStrategy.MARATHON)]
    public ResourcePointer UnkD0;  // AD928080
    [SchemaField(0xE0, TigerStrategy.MARATHON)]
    public ResourcePointer UnkE0;  // A5928080
    [SchemaField(0xE8, TigerStrategy.MARATHON)]
    public ResourcePointer UnkE8;  // A2928080 rarity?
    [SchemaField(0xF0, TigerStrategy.MARATHON)]
    public ResourcePointer UnkF0;  // 4C278080

    [SchemaField(0xF8, TigerStrategy.MARATHON)]
    public TigerHash InventoryItemHash;
    public TigerHash UnkAC;

    [SchemaField(0x170, TigerStrategy.MARATHON)]
    public DynamicArray<S8080B189> TraitIndices; // Seems right, no idea is that still its name though
}

[SchemaStruct(0x80809295, 0x48)] // 95928080
public struct S80809295
{
    public short SellForItemIndex; // Usually Credits
    [SchemaField(0x4)]
    public float BuyValue;

    [SchemaField(0xC)]
    public float MaxValue; // Always 1000000?
    public float SellValueMultiplier;
}

[SchemaStruct(0x808092A2, 0x4)] // A2928080
public struct S808092A2
{
    public short TierType;
    public short Unk02;
}

[SchemaStruct(0x8080B189, 2)] // 89B18080
public struct S8080B189
{
    public short Index;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x80809245, 0x40)] // 45928080
public struct S80809245
{
    public int StatGroupIndex; // unsure

    [SchemaField(0x8)]
    public DynamicArray<S8080924B> InvestmentStats;  // "investmentStats" from API
    //public DynamicArray<S87738080> Perks;  // 'perks'
}

/// <summary>
/// "investmentStat" from API
/// </summary>
[SchemaStruct(TigerStrategy.MARATHON, 0x8080924B, 0x28)] // 4B928080
public struct S8080924B
{
    public int StatTypeIndex;  // "statTypeHash"
    public int Value;  // "value"
}

[SchemaStruct("86738080", 0x18)]
public struct S87738080
{
    public int PerkIndex;  // "perkHash" from API
}

[SchemaStruct("B6738080", 0x4)]
public struct SB6738080
{
    public short LoreEntryIndex;
}

/// <summary>
/// "translationBlock" from API
/// </summary>
[SchemaStruct(TigerStrategy.MARATHON, 0x808091D3, 0x70)] // D3918080
public struct S808091D3
{
    [SchemaField(0x28, TigerStrategy.MARATHON)]
    public DynamicArrayUnloaded<S30688080> Arrangements;

    //[SchemaField(0x28, TigerStrategy.MARATHON)]
    //public DynamicArray<S7B738080> CustomDyes;  // "customDyes" from API

    //[SchemaField(0x38, TigerStrategy.MARATHON)]
    //public DynamicArray<S7B738080> DefaultDyes;  // "defaultDyes" from API

    //[SchemaField(0x48, TigerStrategy.MARATHON)]
    //public DynamicArray<S7B738080> LockedDyes;  // "lockedDyes" from API

    [SchemaField(0x68, TigerStrategy.MARATHON)]
    public short PatternIndex;
    public short PatternIndex2;
}

/// <summary>
/// "arrangement" from API
/// </summary>
[SchemaStruct(TigerStrategy.MARATHON, "30688080", 4)]
public struct S30688080
{
    public int Index; // SandboxPatternGlobalTagId Index, short?
    public int UnkHash;
}

/// <summary>
/// "lockedDyes" from API
/// </summary>
[SchemaStruct(TigerStrategy.MARATHON, "7B738080", 4)]
public struct S7B738080
{
    public short ChannelIndex;  // "channelHash" from API
    public short DyeIndex;  // "dyeHash" from API
}

#endregion

#region Stats
[SchemaStruct(TigerStrategy.MARATHON, "808033FD", 0x18)]
public struct S808033FD
{
    public ulong FileSize;
    public DynamicArrayUnloaded<S01348080> StatGroupDefinitions;
}

[SchemaStruct(TigerStrategy.MARATHON, "01348080", 0x18)]
public struct S01348080
{
    public TigerHash StatGroupHash;

    [SchemaField(0x8, TigerStrategy.MARATHON)]
    public DynamicArray<S03348080> Stats;
}

[SchemaStruct(TigerStrategy.MARATHON, "03348080", 0x48)]
public struct S03348080
{
    public TigerHash StatHash;
    public StringIndexReference StatDisplayName;
    public StringIndexReference StatDisplayDescription;
    public StringIndexReference StatValueSuffix; // '%', 'm', 's'

    //public byte DisplayAsNumeric;
    //public byte Unk02;
    //public byte IsLinear; // not in api, means the value "isnt" interpolated? WYSIWYG

    [SchemaField(0x28, TigerStrategy.MARATHON)]
    public DynamicArray<SF4288080> DisplayInterpolation;

}

[SchemaStruct(TigerStrategy.MARATHON, "F4288080", 0x8)]
public struct SF4288080
{
    public int Value;
    public int Weight;
}

[SchemaStruct(TigerStrategy.MARATHON, "80806E50", 0x18)]
public struct S80806E50
{
    public ulong FileSize;
    public DynamicArrayUnloaded<S546E8080> StatDefinitions;
}

[SchemaStruct(TigerStrategy.MARATHON, "546E8080", 0x30)]
public struct S546E8080
{
    public TigerHash StatHash;
    public StringIndexReference StatName;
    public StringIndexReference StatDescription;
    public short StatIconIndex;
}

[SchemaStruct(TigerStrategy.MARATHON, "C9798080", 0x18)]
public struct SC9798080
{
    [SchemaField(0x8)]
    public DynamicArray<SCF798080> PowerCapDefinitions;
}

[SchemaStruct(TigerStrategy.MARATHON, "CF798080", 0x8)]
public struct SCF798080
{
    public TigerHash PowerCapHash;
    public float PowerCap; // needs multiplied by 10 for some reason?
}
#endregion

#region String Stuff

[SchemaStruct(TigerStrategy.MARATHON, "80806EF0", 0x18)]
public struct S80806EF0
{
    public long FileSize;
    public DynamicArrayUnloaded<SF46E8080> Containers;
}

[SchemaStruct(TigerStrategy.MARATHON, "F46E8080", 0x20)]
public struct SF46E8080
{
    public TigerHash ApiHash;

    [SchemaField(0x10, TigerStrategy.MARATHON, Tag64 = true)]
    public Tag<SInventoryItemStrings> StringContainer;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x80806EF6, 0x130)]
public struct SInventoryItemStrings
{
    public long FileSize;

    [SchemaField(0x8, TigerStrategy.MARATHON)]
    public ResourcePointer Unk08;  // 516F8080
    [SchemaField(0x10, TigerStrategy.MARATHON)]
    public ResourcePointer Unk10;  // 506F8080
    [SchemaField(0x20, TigerStrategy.MARATHON)]
    public ResourcePointer Unk20;  // 4D6F8080
    [SchemaField(0x28, TigerStrategy.MARATHON)]
    public ResourcePointer Unk28;  // 4C6F8080
    [SchemaField(0x68, TigerStrategy.MARATHON)]
    public ResourcePointer Unk68;  // 306F8080
    [SchemaField(0x80, TigerStrategy.MARATHON)]
    public ResourcePointer Unk80;  // B9308080
    [SchemaField(0x90, TigerStrategy.MARATHON)]
    public ResourcePointer Unk90;  // 742C8080
    [SchemaField(0x98, TigerStrategy.MARATHON)]
    public ResourcePointer Unk98;  // 546F8080
    [SchemaField(0xA0, TigerStrategy.MARATHON)]
    public ResourcePointer UnkA0;  // 1D328080
    [SchemaField(0xB0, TigerStrategy.MARATHON)]
    public ResourcePointer UnkB0;  // 752C8080

    [SchemaField(0xB8, TigerStrategy.MARATHON)]
    public short IconIndex;
    public short UnkB2;

    [SchemaField(0xBC, TigerStrategy.MARATHON)]
    public StringIndexReference ItemName;

    [SchemaField(0xC4, TigerStrategy.MARATHON)]
    public StringIndexReference ItemTypeGlyph;

    [SchemaField(0xD0, TigerStrategy.MARATHON)]
    public StringIndexReference ItemType;

    [SchemaField(0xD8, TigerStrategy.MARATHON)]
    public StringIndexReference ItemDescription;

    [SchemaField(0xE0, TigerStrategy.MARATHON)]
    public StringIndexReference UnkE0;

    [SchemaField(0xE8, TigerStrategy.MARATHON)]
    public StringIndexReference UnkE8; // Same as description?

    [SchemaField(0xF0, TigerStrategy.MARATHON)]
    public DynamicArrayUnloaded<SB0718080> UnkF0;

    public TigerHash Unk100;
    public TigerHash Unk104;
    public TigerHash Unk108;
    public TigerHash Unk10C;

    public DestinyTooltipStyle TooltipStyle; // Unsure, still same layout as d2 but idk if it will be used
    public DestinyUIDisplayStyle DisplayStyle; // Unsure

    //[SchemaField(0xE0, TigerStrategy.MARATHON)]
    //public DynamicArray<SB2548080> TooltipNotifications;
}

[SchemaStruct("B0718080", 2)]
public struct SB0718080
{
    public short Unk00;
}

[SchemaStruct(0x808030B9, 4)] // B9308080
public struct S808030B9
{
    public short Index;
    public short Unk02;
}


[SchemaStruct(0xFFFFFFFF, 0x0)] // TODO FIX HASH AND SIZE, CURRENT CONFLICT WITH OLD CLASS HASH
public struct S2D548080
{
    public long FileSize;
    public DynamicArrayUnloaded<S33548080> SandboxPerkDefinitionEntries;
}

[SchemaStruct("33548080", 0x28)]
public struct S33548080
{
    public TigerHash SandboxPerkHash;
    public TigerHash Unk04;
    public StringIndexReference SandboxPerkName;
    public StringIndexReference SandboxPerkDescription;
    public short IconIndex;
}

[SchemaStruct("AA768080", 0x18)]
public struct SAA768080
{
    public long FileSize;
    public DynamicArrayUnloaded<SAE7680800> SandboxPerkDefinitionEntries;
}

[SchemaStruct("AE768080", 0xC)]
public struct SAE7680800
{
    public TigerHash SandboxPerkHash;
    public int UnkIndex;
    public int Unk08;
}

#endregion

#region ArtArrangement

/// <summary>
/// Stores all the art arrangement hashes in an index-accessed DynamicArray.
/// </summary>
[SchemaStruct("F2708080", 0x18)]
public struct SF2708080
{
    public long FileSize;
    public DynamicArrayUnloaded<SED6F8080> ArtArrangementHashes;
}

[SchemaStruct("ED6F8080", 4)]
public struct SED6F8080
{
    public TigerHash ArtArrangementHash;
}

#endregion

#region ApiEntities

/// <summary>
/// Entity assignment tag header. The assignment can be accessed via the art arrangement index.
/// The file is massive so I don't auto-parse it.
/// </summary>
[SchemaStruct(TigerStrategy.MARATHON, "CE558080", 0x28)]
public struct SCE558080
{
    public long FileSize;
    public DynamicArrayUnloaded<SD4558080> ArtArrangementEntityAssignments;
    // [DestinyField(FieldType.TablePointer)]
    // public DynamicArray<SD8558080> FinalAssignment;  // this is not needed as the above table has resource pointers
}

[SchemaStruct(TigerStrategy.MARATHON, "D4558080", 0x20)]
public struct SD4558080
{
    [SchemaField(TigerStrategy.MARATHON)]
    public TigerHash ArtArrangementHash;

    [SchemaField(0x8, TigerStrategy.MARATHON)]
    public TigerHash MasculineSingleEntityAssignment; // things like armour only have 1 entity, so can skip the jumps
    public TigerHash FeminineSingleEntityAssignment;

    [SchemaField(0x10, TigerStrategy.MARATHON)]
    public DynamicArray<SD7558080> MultipleEntityAssignments;
}

[SchemaStruct(TigerStrategy.MARATHON, "D7558080", 8)]
public struct SD7558080
{
    public ResourceInTablePointer<SD8558080> EntityAssignmentResource;
}

[SchemaStruct(TigerStrategy.MARATHON, "D8558080", 0x18)]
public struct SD8558080
{
    public long Unk00;
    public DynamicArray<SDA558080> EntityAssignments;
}

[SchemaStruct(TigerStrategy.MARATHON, "DA558080", 4)]
public struct SDA558080
{
    public TigerHash EntityAssignmentHash;
}

/// <summary>
/// The "final" assignment map of assignment hash : entity hash
/// </summary>
[SchemaStruct(TigerStrategy.MARATHON, "808066D5", 0x18)] // Unused??
public struct S808066D5
{
    public long FileSize;
    public DynamicArrayUnloaded<SD7668080> EntityArrangementMap; // Only 1 entry in the alpha
}

[SchemaStruct(TigerStrategy.MARATHON, "D7668080", 8)]
public struct SD7668080 : IComparer<SD7668080>
{
    public TigerHash AssignmentHash;
    [NoLoad]
    public Tag<S8080890B> EntityParent;

    public int Compare(SD7668080 x, SD7668080 y)
    {
        if (x.AssignmentHash.Equals(y.AssignmentHash)) return 0;
        return x.AssignmentHash.CompareTo(y.AssignmentHash);
    }
}

[SchemaStruct(TigerStrategy.MARATHON, "80806590", 0x38)]
public struct S80806590
{
    public long FileSize;
    [SchemaField(0x10, Tag64 = true)]
    public Tag<S8080B61C> SandboxPatternAssignmentsTag;
    [SchemaField(0x28, Tag64 = true)]
    public Tag<S808066D5> EntityAssignmentsMap;
}

/// <summary>
/// The assignment map for api entity sandbox patterns, for things like skeletons and audio || OR art dye references
/// </summary>
[SchemaStruct(TigerStrategy.MARATHON, "8080B61C", 0x28)]
public struct S8080B61C
{
    public long FileSize;
    public DynamicArrayUnloaded<SC9A78080> AssignmentBSL;
    public DynamicArray<SUInt32> Unk18;
}

[SchemaStruct(TigerStrategy.MARATHON, "C9A78080", 0x18)]
public struct SC9A78080 : IComparer<SC9A78080>
{
    public TigerHash ApiHash;

    [SchemaField(0x8, TigerStrategy.MARATHON, Tag64 = true)]
    public FileHash EntityRelationHash;  // can be entity or smth else, if SandboxPattern is entity if ArtDyeReference idk

    public int Compare(SC9A78080 x, SC9A78080 y)
    {
        if (x.ApiHash.Equals(y.ApiHash)) return 0;
        return x.ApiHash.CompareTo(y.ApiHash);
    }
}

[SchemaStruct(TigerStrategy.MARATHON, "80806CAC", 0x18)]
public struct S80806CAC
{
    public long FileSize;
    public DynamicArrayUnloaded<SB06C8080> SandboxPatternGlobalTagId;
}

[SchemaStruct(TigerStrategy.MARATHON, "B06C8080", 0x48)]
public struct SB06C8080
{
    public TigerHash EntryHash;

    [SchemaField(0x8, TigerStrategy.MARATHON)]
    public TigerHash PatternGlobalTagIdHash;
    public short SkinID; // index into cosmetic map

    [SchemaField(0x18, TigerStrategy.MARATHON)]
    public TigerHash WeaponContentGroupHash;
    public TigerHash WeaponTypeHash;
}

[SchemaStruct(TigerStrategy.MARATHON, "8080890B", 0x18)]
public struct S8080890B
{
    public long FileSize;

    [SchemaField(8, TigerStrategy.MARATHON, Tag64 = true)]
    public FileHash EntityData;  // can be entity, can be audio group for entity
}

// This seems to contain the *real* base models of weapons + their cosmetic base versions
// plus other things like runner skins and charms (CHARM REFERENCE??!!)
[SchemaStruct(TigerStrategy.MARATHON, 0x80803081, 0x18)]
public struct S80803081
{
    public long FileSize;
    public DynamicArrayUnloaded<S87308080> InvestmentCosmetics;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x80803087, 0x8)]
public struct S87308080
{
    public TigerHash APIHash;
    public Tag<S80803089> Pattern;
}

[SchemaStruct(TigerStrategy.MARATHON, 0x80803089, 0x8)]
public struct S80803089
{
    [SchemaField(0x8, Tag64 = true)]
    public Entity.Entity Pattern;
}

#endregion

#region InventoryItem Icons

[SchemaStruct(TigerStrategy.MARATHON, "808071C0", 0x18)]
public struct S808071C0
{
    public long FileSize;
    public DynamicArrayUnloaded<SC6718080> InventoryItemIconsMap;
}

[SchemaStruct(TigerStrategy.MARATHON, "C6718080", 0x20)]
public struct SC6718080
{
    public TigerHash InventoryItemHash;
    [SchemaField(0x10, Tag64 = true)]
    public Tag<S80805335> IconContainer;
}

[SchemaStruct(TigerStrategy.MARATHON, "80805335", 0x80)]
public struct S80805335
{
    public long FileSize;
    [SchemaField(0x10)]
    public TigerHash Unk10;

    [SchemaField(0x18)]
    public Tag<S80805350> IconPrimaryContainer;
    public Tag<S80805350> IconAdContainer; // Rest unknown atm
    public Tag<S80805350> IconBGOverlayContainer;
    public Tag<S80805350> IconBackgroundContainer;
    public Tag<S80805350> IconOverlayContainer;
    public Tag<S80805350> IconSpecialContainer;
}


[SchemaStruct(TigerStrategy.MARATHON, "80805350", 0x18)]
public struct S80805350
{
    public long FileSize;
    [SchemaField(0x10)]
    public ResourcePointer Unk10;  // 4C538080, 48538080
}

[SchemaStruct(TigerStrategy.MARATHON, "4C538080", 0x20)]
public struct S4C538080
{
    public DynamicArrayUnloaded<S53538080> Unk00;
}

[SchemaStruct(TigerStrategy.MARATHON, "53538080", 0x10)]
public struct S53538080
{
    public DynamicArrayUnloaded<S56538080> TextureList;
}

[SchemaStruct(TigerStrategy.MARATHON, "56538080", 4)]
public struct S56538080
{
    public Texture IconTexture;
}

//-----

[SchemaStruct(TigerStrategy.MARATHON, "48538080", 0x20)]
public struct S48538080
{
    public DynamicArrayUnloaded<S51538080> Unk00;
}

[SchemaStruct(TigerStrategy.MARATHON, "51538080", 0x10)]
public struct S51538080
{
    public DynamicArrayUnloaded<S55538080> TextureList;
}

[SchemaStruct("55538080", 4)]
public struct S55538080
{
    public Texture IconTexture;
}


#endregion

#region String container hash + indexmap

[SchemaStruct(TigerStrategy.MARATHON, "808071C8", 0x18)]
public struct S808071C8
{
    public long FileSize;
    public DynamicArrayUnloaded<SCD718080> StringIndexMap;
}

[SchemaStruct(TigerStrategy.MARATHON, "CD718080", 0x18)]
public struct SCD718080
{
    public TigerHash BankFnvHash;  // some kind of name for the bank

    [SchemaField(0x8, TigerStrategy.MARATHON, Tag64 = true), NoLoad]
    public LocalizedStrings LocalizedStrings;
}

[SchemaStruct(TigerStrategy.MARATHON, "CF508080", 0x18)]
public struct SCF508080
{
    public long FileSize;
    public DynamicArrayUnloaded<SD3508080> LoreStringMap;
}

[SchemaStruct(TigerStrategy.MARATHON, "D3508080", 0x28)]
public struct SD3508080
{
    public long Unk00;
    public TigerHash LoreHash;
    public StringIndexReference LoreName;
    public StringIndexReference LoreSubtitle;
    public StringIndexReference LoreDescription;
}

#endregion

#region Socket+Plug Entries
[SchemaStruct(0x808092A5, 0x20)] // A5928080
public struct S808092A5
{
    public DynamicArray<S80806919> SocketEntries;
    //public DynamicArray<SC8778080> IntrinsicSockets;
}


[SchemaStruct(0x80806919, 0x50)] // 19698080
public struct S80806919
{
    public short SocketTypeIndex; // 'socketTypeHash' 

    [SchemaField(0x8)]
    public DynamicArray<S8080B1DD> Unk08; // Uniques?

    [SchemaField(0x28)]
    public short Unk28;

    [SchemaField(0x30)]
    public short Unk30;

    [SchemaField(0x48)]
    public short Unk48;
}

#endregion

#region Socket Category
[SchemaStruct(0x80808C6B, 0x18)]
public struct S80808C6B
{
    public long FileSize;
    public DynamicArrayUnloaded<S80808ACC> SocketTypeEntries;
}

[SchemaStruct(0x80808ACC, 0x48)]
public struct S80808ACC // CC8A8080
{
    public TigerHash SocketTypeHash;
    //public short Unk04;
    //public short SocketCategoryIndex; // 'socketCategoryHash'
    //public int SocketVisiblity; // 'visibility'

    [SchemaField(0x18)]
    public DynamicArray<S8080B1DD> PlugWhitelists;
    public DynamicArray<S8080B1DD> Unk28; // Items this socket can be on?

    [SchemaField(0x3C)]
    public TigerHash SocketCategoryHash;

    [SchemaField(0x44)]
    public TigerHash SocketSubcategoryHash; // sight, magazine, barrel, etc
}

[SchemaStruct(0x8080B1DD, 0x2)]
public struct S8080B1DD // DDB18080
{
    public short ItemIndex;
}

[SchemaStruct(0x80803419, 0x18)] // unsure but looks similar to D2
public struct S80803419
{
    public long FileSize;
    public DynamicArrayUnloaded<S8080341D> SocketCategoryEntries;
}

[SchemaStruct(0x8080341D, 0x28)] // 1D348080
public struct S8080341D
{
    public TigerHash SocketCategoryHash;
    [SchemaField(0x18)]
    public StringIndexReference SocketName;
    public int IconIndex;
}
#endregion

#region Collectables

[SchemaStruct("28788080", 0x18)]
public struct S28788080
{
    public long FileSize;
    public DynamicArrayUnloaded<S2C788080> CollectibleDefinitionEntries;
}

[SchemaStruct("2C788080", 0xB0)]
public struct S2C788080
{
    [SchemaField(0x18)]
    public DynamicArray<SF7788080> ParentNodeHashes;
    public TigerHash CollectibleHash;
    public short InventoryItemIndex;
}

[SchemaStruct("F7788080", 2)]
public struct SF7788080
{
    public short ParentNodeHashIndex;
}


[SchemaStruct("BF598080", 0x18)]
public struct SBF598080
{
    public long FileSize;
    public DynamicArrayUnloaded<SC3598080> CollectibleDefinitionStringEntries;
}

[SchemaStruct("C3598080", 0x60)]
public struct SC3598080
{
    public TigerHash CollectibleHash;
    public int Unk04;
    public StringIndexReference CollectibleName;
    [SchemaField(0x18)]
    public StringIndexReference SourceName;
    public StringIndexReference RequirementDescription;
}

#endregion

#region Objectives
// objective definition
[SchemaStruct(TigerStrategy.MARATHON, "3C758080", 0x18)]
public struct S3C758080
{
    [SchemaField(0x8)]
    public DynamicArrayUnloaded<S40758080> ObjectiveDefinitionEntries;
}

[SchemaStruct(TigerStrategy.MARATHON, "40758080", 0xB0)]
public struct S40758080
{
    public TigerHash ObjectiveHash;
    [SchemaField(0x10, TigerStrategy.MARATHON)]
    public int CompletionValue;
}

// objective definition strings
[SchemaStruct(TigerStrategy.MARATHON, "4C588080", 0x18)]
public struct S4C588080
{
    [SchemaField(0x8)]
    public DynamicArrayUnloaded<S50588080> ObjectiveDefinitionStringEntries;
}

[SchemaStruct(TigerStrategy.MARATHON, "50588080", 0x58)]
public struct S50588080
{
    public TigerHash ObjectiveHash;
    public short IconIndex;
    [SchemaField(0x18)]
    public StringIndexReference ProgressDescription;
    public byte InProgressValueStyle; // enum DestinyUnlockValueUIStyle ?
    public byte CompletedValueStyle;
    public short LocationIndex; // 'locationHash' DestinyLocationDefinition
}

[SchemaStruct(TigerStrategy.MARATHON, "B0738080", 0x28)]
public struct SB0738080
{
    public DynamicArray<S15908080> Objectives;
}

[SchemaStruct(TigerStrategy.MARATHON, "15908080", 0x2)]
public struct S15908080
{
    public short ObjectiveIndex;
}
#endregion

#region DestinyPresentationNodeDefinitions
[SchemaStruct(TigerStrategy.MARATHON, "D7788080", 0x18)]
public struct SD7788080
{
    [SchemaField(0x8)]
    public DynamicArray<SDB788080> PresentationNodeDefinitions;
}

[SchemaStruct(TigerStrategy.MARATHON, "DB788080", 0xC8)]
public struct SDB788080
{
    [SchemaField(0x18)]
    public DynamicArray<SF7788080> ParentNodes;
    [SchemaField(0x2C)]
    public int MaxCategoryRecordScore;
    [SchemaField(0x30)]
    public TigerHash Hash;
    public byte NodeType;
    public byte Scope;
    [SchemaField(0x58)]
    public short ObjectiveIndex;
    public short CompletionRecordIndex; // completionRecordHash
    [SchemaField(0x70)]
    public DynamicArray<SED788080> PresentationNodes; // children -> presentationNodes
    public DynamicArray<SEA788080> Collectables; // children -> collectibles
    public DynamicArray<SE7788080> Records; // children -> records
}

[SchemaStruct(TigerStrategy.MARATHON, "ED788080", 0x18)]
public struct SED788080
{
    public short Unk00; // nodeDisplayPriority? Always 0 in api though
    public short PresentationNodeIndex; // presentationNodeHash
}

[SchemaStruct(TigerStrategy.MARATHON, "EA788080", 0x4)]
public struct SEA788080
{
    public short Unk00;
    public short CollectableIndex; // Collectable index
}

[SchemaStruct(TigerStrategy.MARATHON, "E7788080", 0x6)]
public struct SE7788080
{
    public short Unk00;
    public short RecordDefinitionIndex; // RecordDefinition index
}

[SchemaStruct(TigerStrategy.MARATHON, "03588080", 0x18)]
public struct S03588080
{
    [SchemaField(0x8)]
    public DynamicArray<S07588080> PresentationNodeDefinitionStrings;
}

[SchemaStruct(TigerStrategy.MARATHON, "07588080", 0x2C)]
public struct S07588080
{
    public TigerHash NodeHash;
    public int IconIndex;
    public StringIndexReference Name;
    public StringIndexReference Description;
}
#endregion

#region DestinyRecordDefinition
[SchemaStruct(TigerStrategy.MARATHON, "1F718080", 0x18)]
public struct S1F718080
{
    [SchemaField(0x8)]
    public DynamicArray<SC16F8080> RecordDefinitions;
}

[SchemaStruct(TigerStrategy.MARATHON, "C16F8080", 0xE8)]
public struct SC16F8080
{
    [SchemaField(0x18)]
    public DynamicArray<SF7788080> ParentNodeHashes;

    [SchemaField(0x30)]
    public TigerHash Hash;
    public short LoreIndex;

    [SchemaField(0x38)]
    public DynamicArray<SC96F8080> ObjectiveHashes;

    [SchemaField(0x64)]
    public int ScoreValue;
}

[SchemaStruct(TigerStrategy.MARATHON, "C96F8080", 0x2)]
public struct SC96F8080
{
    public short ObjectiveIndex;
}

[SchemaStruct(TigerStrategy.MARATHON, "87588080", 0x18)]
public struct S87588080
{
    [SchemaField(0x8)]
    public DynamicArray<S8B588080> RecordDefinitionStrings;
}

[SchemaStruct(TigerStrategy.MARATHON, "8B588080", 0x90)]
public struct S8B588080
{
    public TigerHash Hash;
    public int IconIndex;
    public StringIndexReference Name;
    public StringIndexReference Description;
    public StringIndexReference RecordTypeName;
    public StringIndexReference ObscuredName;
    public StringIndexReference ObscuredDescription;

    [SchemaField(0x50)]
    public DynamicArray<S93588080> RewardItems;
}

[SchemaStruct(TigerStrategy.MARATHON, "93588080", 0x18)]
public struct S93588080
{
    public int ItemIndex; // InventoryItem index
    public int Quantity;
}
#endregion

#region Trait Definition
//[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "80807900", 0x28)]
//public struct S80807900
//{
//    [SchemaField(0x8)]
//    public DynamicArray<S09798080> Traits;
//    // Another table here but its the same as above but unordered with its index where Unk04 would be?
//}

//[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "09798080", 0x8)]
//public struct S09798080
//{
//    public DestinyTraitID TraitHash;
//    public int Unk04; // Sometimes its index, sometimes not
//}

[SchemaStruct(TigerStrategy.MARATHON, "8080706A", 0x18)]
public struct S8080706A
{
    [SchemaField(0x8)]
    public DynamicArray<S6E708080> TraitStrings;
}

[SchemaStruct(TigerStrategy.MARATHON, "54258080", 0x1C)]
public struct S6E708080
{
    public MarathonTraitID TraitHash;
    public int IconIndex;
    public StringIndexReference TraitName;
    public StringIndexReference TraitDescription;
    public TigerHash Unk18; // always 'keyword'?
}
#endregion

