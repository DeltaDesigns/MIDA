using Tiger.Schema.Strings;

namespace Tiger.Schema.Investment;

/// <summary>
/// Stores all the inventory item definitions in a huge hashmap.
/// </summary>
[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "80809685", 0x18)]
public struct S80809685
{
    public long FileSize;
    public DynamicArrayUnloaded<S89968080> InventoryItemDefinitionEntries;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "89968080", 0x20)]
public struct S89968080
{
    public TigerHash InventoryItemHash;
    [SchemaField(0x10), NoLoad]
    public InventoryItem InventoryItem;
}

#region InventoryItemDefinition

/// <summary>
/// Inventory item definition.
/// </summary>
[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "8080968B", 0x120)]
public struct S8080968B
{
    public long FileSize;

    [SchemaField(0x08, TigerStrategy.MARATHON_ALPHA)]
    public ResourcePointer Unk08;  // 6D938080
    [SchemaField(0x70, TigerStrategy.MARATHON_ALPHA)]
    public ResourcePointer Unk70;  // 45928080
    [SchemaField(0x80, TigerStrategy.MARATHON_ALPHA)]
    public ResourcePointer Unk80;  // D3918080
    [SchemaField(0xA0, TigerStrategy.MARATHON_ALPHA)]
    public ResourcePointer UnkA0;  // 95928080
    [SchemaField(0xA8, TigerStrategy.MARATHON_ALPHA)]
    public ResourcePointer UnkA8;  // 93928080
    [SchemaField(0xB0, TigerStrategy.MARATHON_ALPHA)]
    public ResourcePointer UnkB0;  // 7F928080
    [SchemaField(0xD0, TigerStrategy.MARATHON_ALPHA)]
    public ResourcePointer UnkD0;  // AD928080
    [SchemaField(0xE0, TigerStrategy.MARATHON_ALPHA)]
    public ResourcePointer UnkE0;  // A2928080 rarity?

    [SchemaField(0xE8, TigerStrategy.MARATHON_ALPHA)]
    public TigerHash InventoryItemHash;
    public TigerHash UnkAC;

    [SchemaField(0xFC, TigerStrategy.MARATHON_ALPHA)]
    public byte UnkFC;

    [SchemaField(0x160, TigerStrategy.MARATHON_ALPHA)]
    public DynamicArray<S89B18080> TraitIndices; // Seems right, no idea is that still its name though
}

[SchemaStruct("A2928080", 0x4)]
public struct SA2928080
{
    public int TierType;
}

/// <summary>
/// D2 "equippingBlock"
/// </summary>
[SchemaStruct("E7778080", 0x20)]
public struct SE7778080
{
    public DynamicArray<S387A8080> Unk00;
    [SchemaField(0x14)]
    public StringHash UniqueLabel;
    public TigerHash UniqueLabelHash;
    public byte EquipmentSlotTypeIndex; // 'equipmentSlotTypeHash'
    public byte Attributes; // EquippingItemBlockAttributes (just 0 or 1)
}

[SchemaStruct("387A8080", 0x10)]
public struct S387A8080
{
    public DynamicArray<S3A7A8080> Unk00;
}

[SchemaStruct("3A7A8080", 8)]
public struct S3A7A8080
{
    public int Unk00;
    public int Unk04;
}

// 'quality'
[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "DC778080", 0x70)]
public struct SDC778080
{
    [SchemaField(0x08)]
    public short ProgressionLevelRequirementIndex; // 'progressionLevelRequirementHash'
    //[SchemaField(0x10)]
    //public DynamicArray<SStringHash> InfusionCategoryHashes;

    [SchemaField(0x28)]
    public DynamicArray<S2D788080> DisplayVersionWatermarkIcons; // Unsure

    [SchemaField(0x60, TigerStrategy.MARATHON_ALPHA)]
    public DynamicArray<SDE778080> Versions;
}

[SchemaStruct("2D788080", 2)]
public struct S2D788080
{
    public short IconIndex;
}

[SchemaStruct("DE778080", 2)]
public struct SDE778080
{
    public short PowerCapIndex; // 'powerCapHash' DestinyPowerCapDefinition
}

[SchemaStruct("89B18080", 2)]
public struct S89B18080
{
    public short Unk00;
}

[SchemaStruct("81738080", 0x30)]
public struct S81738080
{
    public DynamicArray<S86738080> InvestmentStats;  // "investmentStats" from API
    public DynamicArray<S87738080> Perks;  // 'perks'
}

/// <summary>
/// "investmentStat" from API
/// </summary>
[SchemaStruct("86738080", 0x28)]
public struct S86738080
{
    public int StatTypeIndex;  // "statTypeHash" from API
    public int Value;  // "value" from API
}

[SchemaStruct("86738080", 0x18)]
public struct S87738080
{
    public int PerkIndex;  // "perkHash" from API
}

[SchemaStruct("7F738080", 2)]
public struct S7F738080
{
    public short Unk00;
}

[SchemaStruct("B6738080", 0x4)]
public struct SB6738080
{
    public short LoreEntryIndex;
}

// 'gearset'
[SchemaStruct("C5738080", 0x38)]
public struct SC5738080
{
    public DynamicArray<S26908080> ItemList;
}

[SchemaStruct("26908080", 0x2)]
public struct S26908080
{
    public short ItemIndex;
}

/// <summary>
/// "translationBlock" from API
/// </summary>
[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "D3918080", 0x70)]
public struct SD3918080
{
    [SchemaField(0x28, TigerStrategy.MARATHON_ALPHA)]
    public DynamicArrayUnloaded<S30688080> Arrangements;

    //[SchemaField(0x28, TigerStrategy.MARATHON_ALPHA)]
    //public DynamicArray<S7B738080> CustomDyes;  // "customDyes" from API

    //[SchemaField(0x38, TigerStrategy.MARATHON_ALPHA)]
    //public DynamicArray<S7B738080> DefaultDyes;  // "defaultDyes" from API

    //[SchemaField(0x48, TigerStrategy.MARATHON_ALPHA)]
    //public DynamicArray<S7B738080> LockedDyes;  // "lockedDyes" from API

    [SchemaField(0x68, TigerStrategy.MARATHON_ALPHA)]
    public short WeaponPatternIndex;
}

/// <summary>
/// "arrangement" from API
/// </summary>
[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "30688080", 4)]
public struct S30688080
{
    public int Index; // SandboxPatternGlobalTagId Index, short?
    public int UnkHash;
}

/// <summary>
/// "lockedDyes" from API
/// </summary>
[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "7B738080", 4)]
public struct S7B738080
{
    public short ChannelIndex;  // "channelHash" from API
    public short DyeIndex;  // "dyeHash" from API
}

#endregion

#region Stats
[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "BE548080", 0x18)]
public struct SBE548080
{
    public ulong FileSize;
    public DynamicArrayUnloaded<SC4548080> StatGroupDefinitions;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "C4548080", 0x38)]
public struct SC4548080
{
    public TigerHash StatGroupHash;
    public short Unk04;
    [SchemaField(0x8)]
    public TigerHash Unk08;
    [SchemaField(0x10)]
    public DynamicArray<SC8548080> ScaledStats;
    [SchemaField(0x30)]
    public int MaximumValue;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "C8548080", 0x18)]
public struct SC8548080
{
    public byte StatIndex; // 'statHash'
    public byte DisplayAsNumeric;
    public byte Unk02;
    public byte IsLinear; // not in api, means the value "isnt" interpolated? WYSIWYG
    [SchemaField(0x8)]
    public DynamicArray<S257A8080> DisplayInterpolation;

}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "257A8080", 0x8)]
public struct S257A8080
{
    public int Value;
    public int Weight;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "6B588080", 0x18)]
public struct S6B588080
{
    public ulong FileSize;
    public DynamicArrayUnloaded<S6F588080> StatDefinitions;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "6F588080", 0x24)]
public struct S6F588080
{
    public TigerHash StatHash;
    public StringIndexReference StatName;
    public StringIndexReference StatDescription;
    public short StatIconIndex;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "C9798080", 0x18)]
public struct SC9798080
{
    [SchemaField(0x8)]
    public DynamicArray<SCF798080> PowerCapDefinitions;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "CF798080", 0x8)]
public struct SCF798080
{
    public TigerHash PowerCapHash;
    public float PowerCap; // needs multiplied by 10 for some reason?
}
#endregion

#region String Stuff

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "80806EF0", 0x18)]
public struct S80806EF0
{
    public long FileSize;
    public DynamicArrayUnloaded<SF46E8080> Containers;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "F46E8080", 0x20)]
public struct SF46E8080
{
    public TigerHash ApiHash;

    [SchemaField(0x10, TigerStrategy.MARATHON_ALPHA, Tag64 = true)]
    public Tag<S80806EF6> StringContainer;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "80806EF6", 0x130)]
public struct S80806EF6
{
    public long FileSize;

    [SchemaField(0x8, TigerStrategy.MARATHON_ALPHA)]
    public ResourcePointer Unk08;  // 516F8080
    [SchemaField(0x10, TigerStrategy.MARATHON_ALPHA)]
    public ResourcePointer Unk10;  // 506F8080
    [SchemaField(0x20, TigerStrategy.MARATHON_ALPHA)]
    public ResourcePointer Unk20;  // 4D6F8080
    [SchemaField(0x28, TigerStrategy.MARATHON_ALPHA)]
    public ResourcePointer Unk28;  // 4C6F8080
    [SchemaField(0x68, TigerStrategy.MARATHON_ALPHA)]
    public ResourcePointer Unk68;  // 306F8080
    [SchemaField(0x80, TigerStrategy.MARATHON_ALPHA)]
    public ResourcePointer Unk80;  // B9308080
    [SchemaField(0x90, TigerStrategy.MARATHON_ALPHA)]
    public ResourcePointer Unk90;  // 742C8080
    [SchemaField(0x98, TigerStrategy.MARATHON_ALPHA)]
    public ResourcePointer Unk98;  // 546F8080
    [SchemaField(0xA0, TigerStrategy.MARATHON_ALPHA)]
    public ResourcePointer UnkA0;  // 1D328080
    [SchemaField(0xA8, TigerStrategy.MARATHON_ALPHA)]
    public ResourcePointer UnkA8;  // 752C8080


    [SchemaField(0xB4, TigerStrategy.MARATHON_ALPHA)]
    public StringIndexReference ItemName;
    [SchemaField(0xC0, TigerStrategy.MARATHON_ALPHA)]
    public StringIndexReference ItemType;
    [SchemaField(0xC8, TigerStrategy.MARATHON_ALPHA)]
    public StringIndexReference ItemDescription;
    [SchemaField(0xE0, TigerStrategy.MARATHON_ALPHA)]
    public DynamicArrayUnloaded<SB0718080> UnkE0;

    public TigerHash UnkF0;
    public TigerHash UnkF4;
    public TigerHash UnkF8;
    public TigerHash UnkFC;

    public DestinyTooltipStyle TooltipStyle; // Unsure, still same layout as d2 but idk if it will be used
    public DestinyUIDisplayStyle DisplayStyle; // Unsure

    //[SchemaField(0xE0, TigerStrategy.MARATHON_ALPHA)]
    //public DynamicArray<SB2548080> TooltipNotifications;
}

[SchemaStruct("D8548080", 0x88)]
public struct SD8548080
{
    [SchemaField(0x10)]
    public DynamicArray<SDC548080> InsertionRules;
}

[SchemaStruct("DC548080", 0x8)]
public struct SDC548080
{
    public StringIndexReference FailureMessage;
}

[SchemaStruct("D7548080", 0x20)]
public struct SD7548080 // 'preview'
{
    public DestinyScreenStyle ScreenStyle; // screenStyle
    public int PreviewVendorIndex; // previewVendorHash
    public StringIndexReference PreviewActionString; // previewActionString
}

[SchemaStruct("CF548080", 0x8)]
public struct SCF548080 // 'details'
{
    public StringIndexReference DetailsActionString;
}

[SchemaStruct("B2548080", 0x20)]
public struct SB2548080
{
    [SchemaField(0x10)]
    public StringIndexReference DisplayString;
    public StringHash DisplayStyle; // No actual strings, fnv (B4437851 = ui_display_style_item_add_on)
}

[SchemaStruct("B0718080", 2)]
public struct SB0718080
{
    public short Unk00;
}

[SchemaStruct("59238080", 0x18)]
public struct S59238080
{
    [SchemaField(0x10)]
    public short Unk10;
    [SchemaField(0x14)]
    public TigerHash Unk14;
}


/// <summary>
/// Item destruction, includes the term "Dismantle".
/// </summary>
[SchemaStruct("EF548080", 0x1C)]
public struct SEF548080
{
    public StringIndexReference DestructionTerm;
    // some other terms, integers
}

[SchemaStruct("E7548080", 8)]
public struct SE7548080
{
    public short Unk00;
}

[SchemaStruct("E5548080", 0x28)]
public struct SE5548080
{
    public short Unk00;
    public short Unk02;
    public short Unk04;
    [SchemaField(0x8)]
    public DynamicArray<SF2598080> Unk08;
    public DynamicArray<SAE578080> Unk18;
}

[SchemaStruct("F2598080", 8)]
public struct SF2598080
{
    public short Unk00;
    [SchemaField(0x4)]
    public TigerHash Unk04;
}

[SchemaStruct("AE578080", 2)]
public struct SAE578080
{
    public short Unk00;
}

[SchemaStruct("E4548080", 8)]
public struct SE4548080
{
    public short Unk00;
    [SchemaField(0x4)]
    public TigerHash Unk04;
}

[SchemaStruct("CA548080", 0x18)]
public struct SCA548080
{
    [SchemaField(0x1)]
    public byte StatGroupIndex; // TFS Episode 2
}

/// <summary>
/// Item inspection, includes the term "Details".
/// </summary>
[SchemaStruct("B4548080", 0x18)]
public struct SB4548080
{
    public TigerHash Unk00;
    public TigerHash Unk04;
    [SchemaField(0xC)]
    public StringIndexReference InspectionTerm;
    public int StatGroupIndex;
}

[SchemaStruct("FFFFFFFF", 0x0)] // TODO FIX HASH AND SIZE, CURRENT CONFLICT WITH OLD CLASS HASH
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
[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "CE558080", 0x28)]
public struct SCE558080
{
    public long FileSize;
    public DynamicArrayUnloaded<SD4558080> ArtArrangementEntityAssignments;
    // [DestinyField(FieldType.TablePointer)]
    // public DynamicArray<SD8558080> FinalAssignment;  // this is not needed as the above table has resource pointers
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "D4558080", 0x20)]
public struct SD4558080
{
    [SchemaField(TigerStrategy.MARATHON_ALPHA)]
    public TigerHash ArtArrangementHash;

    [SchemaField(0x8, TigerStrategy.MARATHON_ALPHA)]
    public TigerHash MasculineSingleEntityAssignment; // things like armour only have 1 entity, so can skip the jumps
    public TigerHash FeminineSingleEntityAssignment;

    [SchemaField(0x10, TigerStrategy.MARATHON_ALPHA)]
    public DynamicArray<SD7558080> MultipleEntityAssignments;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "D7558080", 8)]
public struct SD7558080
{
    public ResourceInTablePointer<SD8558080> EntityAssignmentResource;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "D8558080", 0x18)]
public struct SD8558080
{
    public long Unk00;
    public DynamicArray<SDA558080> EntityAssignments;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "DA558080", 4)]
public struct SDA558080
{
    public TigerHash EntityAssignmentHash;
}

/// <summary>
/// The "final" assignment map of assignment hash : entity hash
/// </summary>
[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "808066D5", 0x18)] // Unused??
public struct S808066D5
{
    public long FileSize;
    public DynamicArrayUnloaded<SD7668080> EntityArrangementMap; // Only 1 entry in the alpha
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "D7668080", 8)]
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

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "80806590", 0x38)]
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
[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "8080B61C", 0x28)]
public struct S8080B61C
{
    public long FileSize;
    public DynamicArrayUnloaded<SC9A78080> AssignmentBSL;
    public DynamicArray<SUint32> Unk18;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "C9A78080", 0x18)]
public struct SC9A78080 : IComparer<SC9A78080>
{
    public TigerHash ApiHash;

    [SchemaField(0x8, TigerStrategy.MARATHON_ALPHA, Tag64 = true)]
    public FileHash EntityRelationHash;  // can be entity or smth else, if SandboxPattern is entity if ArtDyeReference idk

    public int Compare(SC9A78080 x, SC9A78080 y)
    {
        if (x.ApiHash.Equals(y.ApiHash)) return 0;
        return x.ApiHash.CompareTo(y.ApiHash);
    }
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "80806CAC", 0x18)]
public struct S80806CAC
{
    public long FileSize;
    public DynamicArrayUnloaded<SB06C8080> SandboxPatternGlobalTagId;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "B06C8080", 0x38)]
public struct SB06C8080
{
    public TigerHash ItemHash; // API item hash
    public TigerHash PatternGlobalTagIdHash;
    public short SkinID; // index in 80A3A06B

    [SchemaField(0x10, TigerStrategy.MARATHON_ALPHA)]
    public TigerHash WeaponContentGroupHash;
    public TigerHash WeaponTypeHash;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "8080890B", 0x18)]
public struct S8080890B
{
    public long FileSize;

    [SchemaField(8, TigerStrategy.MARATHON_ALPHA, Tag64 = true)]
    public FileHash EntityData;  // can be entity, can be audio group for entity
}

// This seems to contain the *real* base models of weapons + their cosmetic base versions
// plus other things like runner skins and charms (CHARM REFERENCE??!!)
[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "80803081", 0x18)]
public struct S80803081
{
    public long FileSize;
    public DynamicArrayUnloaded<S87308080> InvestmentCosmetics;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "87308080", 0x20)]
public struct S87308080
{
    public TigerHash APIHash;
    [SchemaField(0x8, Tag64 = true), NoLoad]
    public Entity.Entity Pattern;
    // theres a TagGlobal after that just points right back the pattern, no idea why it exists
}

#endregion

#region InventoryItem Icons

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "808071C0", 0x18)]
public struct S808071C0
{
    public long FileSize;
    public DynamicArrayUnloaded<SC6718080> InventoryItemIconsMap;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "C6718080", 0x20)]
public struct SC6718080
{
    public TigerHash InventoryItemHash;
    [SchemaField(0x10, Tag64 = true)]
    public Tag<S80805335> IconContainer;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "80805335", 0x80)]
public struct S80805335
{
    public long FileSize;
    [SchemaField(0x10)]
    public TigerHash Unk10;
    public Tag<S80805350> IconPrimaryContainer;
    public Tag<S80805350> IconAdContainer; // Rest unknown atm
    public Tag<S80805350> IconBGOverlayContainer;
    public Tag<S80805350> IconBackgroundContainer;
    public Tag<S80805350> IconOverlayContainer;
    public Tag<S80805350> IconSpecialContainer;
}


[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "80805350", 0x18)]
public struct S80805350
{
    public long FileSize;
    [SchemaField(0x10)]
    public ResourcePointer Unk10;  // 4C538080, 48538080
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "4C538080", 0x20)]
public struct S4C538080
{
    public DynamicArrayUnloaded<S53538080> Unk00;
}
[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "53538080", 0x10)]
public struct S53538080
{
    public DynamicArrayUnloaded<S56538080> TextureList;
}
[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "56538080", 4)]
public struct S56538080
{
    public Texture IconTexture;
}

//-----

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "48538080", 0x20)]
public struct S48538080
{
    public DynamicArrayUnloaded<S51538080> Unk00;
}
[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "51538080", 0x10)]
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

#region Dyes

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "C2558080", 0x18)]
public struct SC2558080
{
    public long FileSize;
    public DynamicArrayUnloaded<SC6558080> ArtDyeReferences;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "C6558080", 8)]
public struct SC6558080
{
    [SchemaField(0, TigerStrategy.MARATHON_ALPHA)]
    public TigerHash ArtDyeHash;
    [SchemaField(4, TigerStrategy.MARATHON_ALPHA)]
    public TigerHash DyeManifestHash;
}

[SchemaStruct("E36C8080", 8)]
public struct SE36C8080
{
    public long FileSize;
    [SchemaField(0x0C)]
    public Dye Dye;
    // same thing + some unknown flags and info
}


[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "F2518080", 0x18)]
public struct SDyeChannels
{
    public long FileSize;
    public DynamicArrayUnloaded<SDyeChannelHash> ChannelHashes;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "2C4F8080", 4)]
public struct SDyeChannelHash
{
    public TigerHash ChannelHash;
}


#endregion

#region String container hash + indexmap

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "808071C8", 0x18)]
public struct S808071C8
{
    public long FileSize;
    public DynamicArrayUnloaded<SCD718080> StringIndexMap;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "CD718080", 0x18)]
public struct SCD718080
{
    public TigerHash BankFnvHash;  // some kind of name for the bank

    [SchemaField(0x8, TigerStrategy.MARATHON_ALPHA, Tag64 = true), NoLoad]
    public LocalizedStrings LocalizedStrings;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "CF508080", 0x18)]
public struct SCF508080
{
    public long FileSize;
    public DynamicArrayUnloaded<SD3508080> LoreStringMap;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "D3508080", 0x28)]
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
[SchemaStruct("C0778080", 0x20)]
public struct SC0778080
{
    public DynamicArray<SC3778080> SocketEntries;
    public DynamicArray<SC8778080> IntrinsicSockets;
}

/// <summary>
/// "socketEntries" from API
/// </summary>
[SchemaStruct("C3778080", 0x58)]
public struct SC3778080
{
    public short SocketTypeIndex; // 'socketTypeHash' 
    public short Unk02;
    public short Unk04;
    public short SingleInitialItemIndex; // 'singleInitialItemHash'
    [SchemaField(0x10)]
    public short ReusablePlugSetIndex1; // randomizedPlugSetHash -> reusablePlugItems
    //[SchemaField(0x18)]
    //public DynamicArray<S3A7A8080> Unk18;
    [SchemaField(0x28)]
    public short ReusablePlugSetIndex2; // randomizedPlugSetHash -> reusablePlugItems
    [SchemaField(0x48)]
    public DynamicArray<SD5778080> PlugItems; // reusablePlugSetHash -> reusablePlugItems
}

[SchemaStruct("CD778080", 0x18)]
public struct SCD778080
{
    public long FileSize;
    public DynamicArrayUnloaded<SD3778080> PlugSetDefinitionEntries;
}

[SchemaStruct("D3778080", 0x18)]
public struct SD3778080
{
    public TigerHash PlugSetHash;
    [SchemaField(0x8)]
    public DynamicArray<SD5778080> ReusablePlugItems;
}

[SchemaStruct("D5778080", 0x40)]
public struct SD5778080
{
    [SchemaField(0x20)]
    public int PlugInventoryItemIndex;
    [SchemaField(0x28)]
    public DynamicArray<S3A7A8080>? UnkUnlocks;
}

[SchemaStruct("C8778080", 0x4)]
public struct SC8778080
{
    public short SocketTypeIndex; // socketTypeHash
    public short PlugItemIndex; // plugItemHash
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "A1738080", 0x128)]
public struct SA1738080
{
    public TigerHash PlugCategoryHash;
    [SchemaField(0xF8, TigerStrategy.MARATHON_ALPHA)]
    public StringHash PlugStyle; // 'uiPlugLabel', theres only none (invalid) and masterwork (6048A01E)
}

#endregion

#region Socket Category
[SchemaStruct("B6768080", 0x18)]
public struct SB6768080
{
    public long FileSize;
    public DynamicArrayUnloaded<SBA768080> SocketTypeEntries;
}

[SchemaStruct("BA768080", 0x68)]
public struct SBA768080
{
    public TigerHash SocketTypeHash;
    public short Unk04;
    public short SocketCategoryIndex; // 'socketCategoryHash'
    public int SocketVisiblity; // 'visibility'

    [SchemaField(0x30)]
    public DynamicArray<SC5768080> PlugWhitelists;
}

[SchemaStruct("C5768080", 0x8)]
public struct SC5768080
{
    public TigerHash PlugCategoryHash;
    public short Unk04;
}

[SchemaStruct("594F8080", 0x18)]
public struct S594F8080
{
    public long FileSize;
    public DynamicArrayUnloaded<S5D4F8080> SocketCategoryEntries;
}

[SchemaStruct("5D4F8080", 0x18)]
public struct S5D4F8080
{
    public TigerHash SocketCategoryHash;
    public StringIndexReference SocketName;
    public StringIndexReference SocketDescription;
    public uint CategoryStyle; // 'uiCategoryStyle'
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
    [SchemaField(0x30)]
    public DynamicArray<S3A7A8080> UnkUnlock30;
    [SchemaField(0x60)]
    public DynamicArray<S3A7A8080> UnkUnlockClass;
    public DynamicArray<S3A7A8080> Unk70;
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
[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "3C758080", 0x18)]
public struct S3C758080
{
    [SchemaField(0x8)]
    public DynamicArrayUnloaded<S40758080> ObjectiveDefinitionEntries;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "40758080", 0xB0)]
public struct S40758080
{
    public TigerHash ObjectiveHash;
    [SchemaField(0x10, TigerStrategy.MARATHON_ALPHA)]
    public int CompletionValue;
}

// objective definition strings
[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "4C588080", 0x18)]
public struct S4C588080
{
    [SchemaField(0x8)]
    public DynamicArrayUnloaded<S50588080> ObjectiveDefinitionStringEntries;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "50588080", 0x58)]
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

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "B0738080", 0x28)]
public struct SB0738080
{
    public DynamicArray<S15908080> Objectives;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "15908080", 0x2)]
public struct S15908080
{
    public short ObjectiveIndex;
}
#endregion

#region DestinyPresentationNodeDefinitions
[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "D7788080", 0x18)]
public struct SD7788080
{
    [SchemaField(0x8)]
    public DynamicArray<SDB788080> PresentationNodeDefinitions;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "DB788080", 0xC8)]
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

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "ED788080", 0x18)]
public struct SED788080
{
    public short Unk00; // nodeDisplayPriority? Always 0 in api though
    public short PresentationNodeIndex; // presentationNodeHash
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "EA788080", 0x4)]
public struct SEA788080
{
    public short Unk00;
    public short CollectableIndex; // Collectable index
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "E7788080", 0x6)]
public struct SE7788080
{
    public short Unk00;
    public short RecordDefinitionIndex; // RecordDefinition index
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "03588080", 0x18)]
public struct S03588080
{
    [SchemaField(0x8)]
    public DynamicArray<S07588080> PresentationNodeDefinitionStrings;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "07588080", 0x2C)]
public struct S07588080
{
    public TigerHash NodeHash;
    public int IconIndex;
    public StringIndexReference Name;
    public StringIndexReference Description;
}
#endregion

#region DestinyRecordDefinition
[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "1F718080", 0x18)]
public struct S1F718080
{
    [SchemaField(0x8)]
    public DynamicArray<SC16F8080> RecordDefinitions;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "C16F8080", 0xE8)]
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

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "C96F8080", 0x2)]
public struct SC96F8080
{
    public short ObjectiveIndex;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "87588080", 0x18)]
public struct S87588080
{
    [SchemaField(0x8)]
    public DynamicArray<S8B588080> RecordDefinitionStrings;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "8B588080", 0x90)]
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

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "93588080", 0x18)]
public struct S93588080
{
    public int ItemIndex; // InventoryItem index
    public int Quantity;
}
#endregion

