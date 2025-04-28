namespace Tiger;

public enum PrimitiveType  // name comes from bungie
{
    Triangles = 3,
    TriangleStrip = 5,
}

public enum ELodCategory : byte
{
    //MainGeom0 = 0, // main geometry lod0
    //GripStock0 = 1,  // grip/stock lod0
    //Stickers0 = 2,  // stickers lod0
    //InternalGeom0 = 3,  // internal geom lod0
    //LowPolyGeom1 = 4,  // low poly geom lod1
    //LowPolyGeom2 = 7,  // low poly geom lod2
    //GripStockScope2 = 8,  // grip/stock/scope lod2
    //LowPolyGeom3 = 9,  // low poly geom lod3
    //Detail0 = 10 // detail lod0

    // Statics
    Unk0 = 0,     // 00000000 - ?
    Unk1 = 1,     // 00000001 - ?
    Unk2 = 2,     // 00000010 - ?
    Unk12 = 12,   // 00001100 - ?
    Unk16 = 16,   // 00010000 - ?
    Unk31 = 31,   // 00011111 - ?
    Unk32 = 32,   // 00100000 - ?
    Unk48 = 48,   // 00110000 - ?
    Unk64 = 64,   // 01000000 - ?
    Unk96 = 96,   // 01100000 - ?
    Unk112 = 112, // 01110000 - ?
    Unk128 = 128, // 10000000 - ?
    Unk240 = 240,
    Unk252 = 252, // 11111100 - ?
    Unk255 = 255, // 11111111 - ?

    // Player/gear
    Unk3 = 3,   // 00000011 - highest res player/gear
    Unk4 = 4,   // 00000100 - medium res player/gear
    Unk7 = 7,   // 00000111 - ?
    Unk8 = 8,   // 00001000 - lowest res player/gear
    Unk15 = 15, // 00001111 - ?
}

public enum TigerLanguage
{
    English = 1,
    French = 2,
    Italian = 3,
    German = 4,
    Spanish = 5,
    Japanese = 6,
    Portuguese = 7,
    Russian = 8,
    Polish = 9,
    Simplified_Chinese = 10,
    Traditional_Chinese = 11,
    Latin_American_Spanish = 12,
    Korean = 13,
}

public enum DestinyGenderDefinition
{
    Masculine = 0,
    Feminine = 1,
    None = 2
}

// https://bungie-net.github.io/multi/schema_Destiny-Definitions-Sockets-DestinySocketCategoryDefinition.html#schema_Destiny-Definitions-Sockets-DestinySocketCategoryDefinition
// https://bungie-net.github.io/multi/schema_Destiny-DestinySocketCategoryStyle.html#schema_Destiny-DestinySocketCategoryStyle
public enum DestinySocketCategoryStyle : uint
{
    Unknown = 0, // 0
    Reusable = 2656457638, // 1
    Consumable = 1469714392, // 2
                             // where Intrinsic? Replaced with LargePerk? // 4
    Unlockable = 1762428417, // 3
    EnergyMeter = 750616615, // 5
    LargePerk = 2251952357, // 6
    Abilities = 1901312945, // 7
    Supers = 497024337, // 8
}

public enum DestinyTooltipStyle : uint
{
    None = StringHash.InvalidHash32, // C59D1C81
    Build = 3284755031, // 'build'
    Record = 3918064370, // 'record'
    VendorAction = 4278229900, // 'vendor_action'
    Package = 1905831191, // 'package'
    Bounty = 1345459588, // 'bounty'
    Quest = 1801258597, // 'quest'
    Emblem = 4274335291, // 'emblem'
}

public enum DestinyUIDisplayStyle : uint
{
    None = StringHash.InvalidHash32, // C59D1C81
    Info = 3556713801, // 'ui_display_style_info'
    PerkInfo = 900809780, // 'ui_display_style_perk_info'
    ItemAddon = 1366836148, // 'ui_display_style_item_add_on'
    EnergyMod = 3201739904, // 'ui_display_style_energy_mod'
    Crafting = 2902805631, // 'ui_display_style_crafting'
    Warning = 3475342179, // 'ui_display_style_warning'
    Box = 2744312160, // 'ui_display_style_box'
    SetContainer = 262729839, // 'ui_display_style_set_container'
    IntrinsicPlug = 2065752925, // 'ui_display_style_intrinsic_plug'
    Engram = 2688883665, // 'ui_display_style_engram'
    Token = 4060663772, // 'ui_display_style_token'
    Infuse = 1494624843, // 'ui_display_style_infuse'
    Memory = 1497864296, // 'ui_display_style_memory'
}

public enum DestinyScreenStyle : uint
{
    None = StringHash.InvalidHash32, // C59D1C81
    Emblem = 3797307284, // 'screen_style_emblem'
    Sockets = 1726057944, // 'screen_style_sockets'
    Vendor = 1509794344, // 'screen_style_vendor'
    Pursuit = 347107188, // 'screen_style_pursuit'
    SeasonalArtifact = 3129355947, // 'screen_style_seasonal_artifact'
    SeasonalArtifactMemorialized = 1186261878, // 'screen_style_seasonal_artifact_memorialized'
    Builds = 2050070793, // 'screen_style_builds'
    DestinationMods = 2085300474, // 'screen_style_destination_mods'
    LoreOnly = 788037671, // 'screen_style_lore_only'
    NewLightSkip = 2869100985, // 'screen_style_new_light_skip'
    Potions = 4095008086, // 'screen_style_potions'
    Emote = 1177935158, // 'screen_style_emote'
}

