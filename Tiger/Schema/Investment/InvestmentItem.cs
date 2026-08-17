using System.Collections.Concurrent;
using System.Diagnostics;

namespace Tiger.Schema.Investment;

public class InventoryItem : Tag<SInventoryItem>
{
    public InventoryItem(FileHash hash, bool shouldParse) : base(hash, shouldParse)
    {
    }

    public uint ApiHash => _tag.InventoryItemHash.Hash32;

    private IReadOnlyCollection<MarathonTraitID> _traits;
    public IReadOnlyCollection<MarathonTraitID> ItemTraits => _traits ??= GetItemTraits(); // cache traits on first use

    private string _name = null;
    public string Name
    {
        get
        {
            if (_name is not null)
                return _name;

            _name = GetItemName();
            return _name;
        }
        set
        {
            _name = value;
        }
    }

    private MarathonTierType? _rarity = null;
    public MarathonTierType Rarity => _rarity ??= GetItemRarity();

    private string _type = null;
    public string Type => _type ??= GetItemType();

    private string _description = null;
    public string Description => _description ??= GetItemDescription();

    private string _typeGlyph = null;
    public string TypeGlyph => _typeGlyph ??= GetItemTypeGlyph();

    private string _traitGlyph = null;
    public string TraitGlyph => _traitGlyph ??= GetItemTraitGlyph();

    public int? _sellValue = null;
    public int SellValue
    {
        get
        {
            if (_sellValue is not null)
                return _sellValue.Value;

            _sellValue = GetItemSellValue();
            return _sellValue.Value;
        }
        set
        {
            _sellValue = value;
        }
    }

    private bool? _isWeapon = null;
    public bool IsWeapon
    {
        get
        {
            if (_isWeapon is not null)
                return _isWeapon.Value;

            _isWeapon = ItemTraits.Any(x => x.GetTraitType() == MarathonItemType.Weapon);
            return _isWeapon.Value;
        }
    }

    private bool? _isRunner = null;
    public bool IsRunner
    {
        get
        {
            if (_isRunner is not null)
                return _isRunner.Value;

            _isRunner = ItemTraits.Any(x => x.GetTraitType() == MarathonItemType.Runner);
            return _isRunner.Value;
        }
    }

    private ConcurrentBag<InventoryItem> _skins = null;
    public ConcurrentBag<InventoryItem> Skins => _skins ??= GetItemSkins();

    public InventoryItem Parent = null;

    //public string FlavorText => GetItemFlavorText();

    public override void Load(bool force = false)
    {
        base.Load(force);

        // this is needed to make sure its skins are loaded (if it has any)
        // which in turn will set the skins parent item
        _ = Skins;
    }

    public string GetItemName()
    {
        var name = Investment.Get().GetItemName(this);
        if (Parent is not null)
            name = $"{Parent.Name} - {name}";

        return name ?? "";
    }

    public string GetItemType()
    {
        if (ItemTraits.Any(x => x.GetTraitType() == MarathonItemType.Weapon))
            return ItemTraits.First(x => x.GetTraitType() == MarathonItemType.Weapon).GetTraitName();

        return Investment.Get().GetItemType(this) ?? "";
    }

    public string GetItemTypeGlyph()
    {
        string glyph = Investment.Get().GetItemTypeGlyph(this) ?? "";
        if (Parent != null && glyph == string.Empty)
            glyph = Parent.TypeGlyph;

        return glyph;
    }

    public string GetItemTraitGlyph()
    {
        if (ItemTraits.Any(x => x.GetTraitType() != MarathonItemType.Default))
            return ItemTraits.First(x => x.GetTraitType() != MarathonItemType.Default).GetTraitGlyph();

        return "";
    }

    public string GetItemDescription()
    {
        return GetItemStrings().TagData.ItemDescription?.Value.ToString() ?? "";
    }

    public int GetItemSellValue()
    {
        if (_tag.UnkA0.GetValue(Reader) is S80809295 value)
            return (int)Math.Max(0, MathF.Floor(value.BuyValue * value.SellValueMultiplier));

        return -1;
    }

    public int GetItemBuyValue()
    {
        if (_tag.UnkA0.GetValue(Reader) is S80809295 value)
            return (int)MathF.Floor(value.BuyValue);

        return -1;
    }

    public MarathonTierType GetItemRarity()
    {
        if (_tag.UnkE8.GetValue(GetReader()) is S808092A2 rarity && rarity.TierType != 6) // 6 key
            return (MarathonTierType)rarity.TierType;
        else
        {
            foreach (var trait in ItemTraits)
            {
                switch (trait)
                {
                    case MarathonTraitID.rarity_tier_grey:
                        return MarathonTierType.Standard;
                    case MarathonTraitID.rarity_tier_green:
                        return MarathonTierType.Enhanced;
                    case MarathonTraitID.rarity_tier_blue:
                        return MarathonTierType.Deluxe;
                    case MarathonTraitID.rarity_tier_purple:
                        return MarathonTierType.Superior;
                    case MarathonTraitID.rarity_tier_gold:
                        return MarathonTierType.Prestige;
                    case MarathonTraitID.rarity_tier_contraband:
                        return MarathonTierType.Contraband;
                    case MarathonTraitID.rarity_tier_dynamic:
                        return MarathonTierType.Dynamic;
                    case MarathonTraitID.rarity_tier_unique:
                        return MarathonTierType.Unique;
                }
            }
        }

        return MarathonTierType.None;
    }

    public Tag<SInventoryItemStrings> GetItemStrings()
    {
        return Investment.Get().GetItemStrings(Investment.Get().GetItemIndex(_tag.InventoryItemHash));
    }

    public List<MarathonTraitID> GetItemTraits()
    {
        List<MarathonTraitID> traits = new();

        foreach (var index in _tag.TraitIndices.Select(x => x.Index))
        {
            traits.Add(Investment.Get().GetTrait(index).Value.TraitHash);
            //traits.Add(MarathonTraitID._item_other);
        }

        if (Parent is not null)
            traits.AddRange(Parent.ItemTraits.Where(x => x.GetTraitType() == MarathonItemType.Weapon));

        return traits;
    }

    public int GetItemIndex()
    {
        return Investment.Get().GetItemIndex(_tag.InventoryItemHash);
    }

    public int GetArtArrangementIndex() // seems to be only used by weapons
    {
        if (_tag.Unk80 is null) return -1;
        if (_tag.Unk80.GetValue(GetReader()) is S808091D3 entry && entry.Arrangements.Count > 0)
        {
            Debug.Assert(entry.Arrangements.Count == 1);
            return entry.Arrangements[GetReader(), 0].Index;
        }
        return -1;
    }

    /// <summary>
    /// Gets the first available pattern index.
    /// Prioritising PatternIndex, then PatternIndex2, then the one in the item strings which seems to be used for items that dont exist in the world like Sponsor Kits.
    /// </summary>
    /// <returns></returns>
    public int GetPatternIndex()
    {
        if (_tag.Unk80.GetValue(GetReader()) is S808091D3 entry)
        {
            if (entry.PatternIndex != -1)
                return entry.PatternIndex;
            else if (entry.PatternIndex2 != -1) // used for world items like ammo and health kits?
                return entry.PatternIndex2;
        }

        // Used for some items like Sponsor Kits which have a UI model but dont exist in the world?
        var strings = GetItemStrings();
        if (strings.TagData.Unk80.GetValue(strings.GetReader()) is S808030B9 entry2 && entry2.Index != -1)
            return entry2.Index;

        return -1;
    }

    /// <summary>
    /// Gets the second pattern index, which seems to be used for world/"use" models like the Patch Kit injector.
    /// </summary>
    /// <returns></returns>
    public int GetPatternIndex2()
    {
        if (_tag.Unk80.GetValue(GetReader()) is S808091D3 entry && entry.PatternIndex2 != -1)
            return entry.PatternIndex2;

        return -1;
    }

    /// <summary>
    /// Gets the third pattern index, which seems to be used for some items that dont exist in the world at all (like Sponsor Kits)?
    /// This is found in the item strings rather than the inventory item tag like the other 2 for some reason.
    /// </summary>
    /// <returns></returns>
    public int GetPatternIndex3()
    {
        var strings = GetItemStrings();
        if (strings.TagData.Unk80.GetValue(strings.GetReader()) is S808030B9 entry2 && entry2.Index != -1)
            return entry2.Index;

        return -1;
    }

    public ConcurrentBag<InventoryItem> GetItemSkins()
    {
        ConcurrentBag<InventoryItem> skins = new();
        Debug.Assert(this.IsLoaded()); // something weird going on here, seems rare tho?
        if (!this.IsLoaded())
            this.Load();

        if (_tag.UnkE0?.GetValue(Reader) is S808092A5 sockets)
        {
            // needed because some weapons like the Bully SMG have a dummy version that is just item_type_weapon
            if (!ItemTraits.Any(x => x.GetTraitType() == MarathonItemType.Weapon) || ItemTraits.Any(x => x == MarathonTraitID.rarity_tier_unique))
                return skins;

            foreach (var socket in sockets.SocketEntries)
            {
                var entry = Investment.Get().GetSocketType(socket.SocketTypeIndex);
                if (entry.SocketCategoryHash.Hash32 != 1371130879)
                    continue;

                foreach (var item in entry.PlugWhitelists)
                {
                    var socketItem = Investment.Get().GetInventoryItem(item.ItemIndex);
                    socketItem.Parent ??= this;

                    skins.Add(socketItem);
                }

                //foreach (var item in socket.Unk08)
                //{
                //    var socketItem = Investment.Get().GetInventoryItem(item.ItemIndex);
                //    Console.WriteLine($"\t-Unk08 {socketItem.Name} : {socketItem.Rarity}");
                //}
            }
        }

        return skins;
    }

    private Texture? GetTexture(Tag<S80805350> iconSecondaryContainer, int index = 0)
    {
        using TigerReader reader = iconSecondaryContainer.GetReader();
        dynamic? prim = iconSecondaryContainer.TagData.Unk10.GetValue(reader);
        if (prim is S4C538080 struct1)
        {
            // TextureList[0] is default, others are for colourblind modes
            if (index >= struct1.Unk00[reader, 0].TextureList.Count)
                return null;
            return struct1.Unk00[reader, 0].TextureList[reader, index].IconTexture;
        }
        if (prim is S48538080 struct2)
        {
            if (index >= struct2.Unk00[reader, 0].TextureList.Count)
                return null;
            return struct2.Unk00[reader, 0].TextureList[reader, index].IconTexture;
        }
        return null;
    }

    public UnmanagedMemoryStream? GetIconBackgroundStream()
    {
        Tag<S80805335>? iconContainer = Investment.Get().GetItemIconContainer(this);
        if (iconContainer == null || iconContainer.TagData.IconBackgroundContainer == null)
            return null;
        var backgroundIcon = GetTexture(iconContainer.TagData.IconBackgroundContainer);
        return backgroundIcon.GetTexture();
    }

    public UnmanagedMemoryStream? GetIconBackgroundOverlayStream()
    {
        Tag<S80805335>? iconContainer = Investment.Get().GetItemIconContainer(this);
        if (iconContainer == null || iconContainer.TagData.IconBGOverlayContainer == null)
            return null;
        var backgroundIcon = GetTexture(iconContainer.TagData.IconBGOverlayContainer);
        return backgroundIcon.GetTexture();
    }

    public UnmanagedMemoryStream? GetIconPrimaryStream()
    {
        Tag<S80805335>? iconContainer = Investment.Get().GetItemIconContainer(this);
        if (iconContainer == null || iconContainer.TagData.IconPrimaryContainer == null)
            return null;
        var primaryIcon = GetTexture(iconContainer.TagData.IconPrimaryContainer);
        return primaryIcon.GetTexture();
    }

    public UnmanagedMemoryStream? GetIconPrimaryStream(int index)
    {
        Tag<S80805335>? iconContainer = Investment.Get().GetItemIconContainer(index);
        if (iconContainer == null || iconContainer.TagData.IconPrimaryContainer == null)
            return null;
        var primaryIcon = GetTexture(iconContainer.TagData.IconPrimaryContainer);
        return primaryIcon.GetTexture();
    }

    public Texture? GetIconPrimaryTexture()
    {
        Tag<S80805335>? iconContainer = Investment.Get().GetItemIconContainer(this);
        if (iconContainer == null || iconContainer.TagData.IconPrimaryContainer == null)
            return null;
        var primaryIcon = GetTexture(iconContainer.TagData.IconPrimaryContainer);
        return primaryIcon;
    }

    public Texture? GetIconPrimaryTexture(int index, int listIndex = 0)
    {
        Tag<S80805335>? iconContainer = Investment.Get().GetItemIconContainer(this);
        if (iconContainer == null || iconContainer.TagData.IconPrimaryContainer == null)
            return null;
        Texture? primaryIcon = Investment.Get().GetTextureFromContainer(iconContainer.TagData.IconPrimaryContainer, index, listIndex);
        return primaryIcon;
    }

    public UnmanagedMemoryStream? GetIconOverlayStream(int index = 0)
    {
        Tag<S80805335>? iconContainer = Investment.Get().GetItemIconContainer(this);
        if (iconContainer == null || iconContainer.TagData.IconOverlayContainer == null)
            return null;
        var overlayIcon = GetTexture(iconContainer.TagData.IconOverlayContainer, index);
        if (overlayIcon is null)
            return null;
        return overlayIcon.GetTexture();
    }

    public UnmanagedMemoryStream? GetFoundryIconStream()
    {
        Tag<S80805335>? iconContainer = Investment.Get().GetFoundryItemIconContainer(this);
        if (iconContainer == null || iconContainer.TagData.IconPrimaryContainer == null)
            return null;
        var foundryIcon = GetTexture(iconContainer.TagData.IconPrimaryContainer);
        return foundryIcon.GetTexture();
    }

    public UnmanagedMemoryStream? GetTextureFromHash(FileHash hash)
    {
        Texture texture = FileResourcer.Get().GetFile<Texture>(hash);

        return texture.GetTexture();
    }
}
