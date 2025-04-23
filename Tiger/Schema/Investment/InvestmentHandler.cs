using System.Collections.Concurrent;
using System.Text;
using System.Text.RegularExpressions;
using ConcurrentCollections;
using Newtonsoft.Json;
using Tiger.Schema.Entity;
using Tiger.Schema.Strings;

namespace Tiger.Schema.Investment;

/// <summary>
/// Keeps track of the investment tags.
/// Finds them on launch from their tag class instead of hash.
/// </summary>
[InitializeAfter(typeof(Hash64Map))]
public class Investment : Strategy.LazyStrategistSingleton<Investment>
{
    private Tag<S80809685> _inventoryItemMap = null;
    private Tag<S808066D5> _entityAssignmentsMap = null;
    private Tag<S80806EF0> _inventoryItemStringContainerMap = null;
    private Tag<S8080B61C> _sandboxPatternAssignmentsTag = null;
    private Tag<S80806CAC> _sandboxPatternGlobalTagIdTag = null;
    private Tag<S808071C8> _localizedStringsIndexTag = null;

    // These still exist but are done differently I think?
    private Tag<SF2708080> _artArrangementMap = null;
    private Tag<SCE558080> _entityAssignmentTag = null;

    private ConcurrentDictionary<uint, InventoryItem> _inventoryItems = null;
    private Dictionary<uint, int> _inventoryItemIndexmap = null;

    public ConcurrentDictionary<int, Tag<S80806EF6>> InventoryItemStringContainers = null;
    private Dictionary<int, LocalizedStrings> _localizedStringsIndexMap = null;

    private Dictionary<uint, Tag<S8080890B>> _sortedArrangementHashmap = null;

    // Possibly obsolete things
    #region OBSOLETE?
    private Tag<S808071C0> _inventoryItemIconTag = null;
    private Tag<S2D548080> _sandboxPerkMap = null;
    private Tag<SAA768080> _sandboxPerkMap2 = null;
    private Tag<S6B588080> _statDefinitionMap = null;
    private Tag<SBE548080> _statGroupDefinitionMap = null;

    public ConcurrentDictionary<int, SD3508080> InventoryItemLoreStrings = null;
    public ConcurrentDictionary<int, S33548080> SandboxPerkStrings = null;
    public ConcurrentDictionary<int, S6F588080> StatStrings = null;
    public ConcurrentDictionary<int, SAE7680800> SandboxPerkMap2 = null;
    public ConcurrentDictionary<int, S50588080> ObjectiveStrings = null;

    public ConcurrentDictionary<int, S5D4F8080> SocketCategoryStringThings = null;
    public Tag<SD7788080> _presentationNodeDefinitionMap = null;
    public Tag<S03588080> _presentationNodeDefinitionStringMap = null;
    private Tag<S3C758080> _objectiveDefinitionMap = null;
    private Tag<S4C588080> _objectiveStringsMap = null;
    private Tag<SCF508080> _loreStringMap = null;
    private Tag<SCD778080> _randomizedPlugSetMap = null;
    private Tag<SB6768080> _socketTypeMap = null;
    private Tag<S594F8080> _socketCategoryMap = null;
    private Tag<S28788080> _collectableDefinitionMap = null;
    private Tag<SBF598080> _collectableStringsMap = null;
    public ConcurrentDictionary<int, SC3598080> CollectableStrings = null;
    private ConcurrentDictionary<uint, InventoryItem> _collectableItems = null;
    private Tag<SC2558080> _artDyeReferenceTag = null;
    private Tag<SDyeChannels> _dyeChannelTag = null;
    #endregion


    public Investment(TigerStrategy strategy) : base(strategy)
    {
    }

    protected override void Reset() => throw new NotImplementedException();

    protected override void Initialise()
    {
        GetAllInvestmentTags();
    }

    private void GetAllInvestmentTags()
    {
        ConcurrentHashSet<FileHash> allHashes = new();
        // Iterate over all investment pkgs until we find all the tags we need

        bool PackageFilterFunc(string packagePath) => packagePath.Contains("investment") || packagePath.Contains("client_startup");
        allHashes = PackageResourcer.Get().GetAllHashes(PackageFilterFunc);
        Parallel.ForEach(allHashes, (val, state, i) =>
        {
            switch (val.GetReferenceHash().Hash32)
            {
                case 0x80809685:
                    _inventoryItemMap = FileResourcer.Get().GetSchemaTag<S80809685>(val);
                    break;

                case 0x808070f2:
                    _artArrangementMap = FileResourcer.Get().GetSchemaTag<SF2708080>(val);
                    break;

                case 0x808055ce:
                    _entityAssignmentTag = FileResourcer.Get().GetSchemaTag<SCE558080>(val);
                    break;

                case 0x80806EF0:
                    _inventoryItemStringContainerMap = FileResourcer.Get().GetSchemaTag<S80806EF0>(val);
                    break;

                case 0x808071C8:
                    _localizedStringsIndexTag = FileResourcer.Get().GetSchemaTag<S808071C8>(val);
                    break;

                // named tag "investment_assets"
                case 0x80806590: // points to parent of the sandbox pattern ref list thing + entity assignment map
                    var parent = FileResourcer.Get().GetSchemaTag<S80806590>(val);
                    _sandboxPatternAssignmentsTag = parent.TagData.SandboxPatternAssignmentsTag; // also art dye refs
                    _entityAssignmentsMap = parent.TagData.EntityAssignmentsMap;
                    break;

                case 0x80806CAC: // inventory item -> pattern global tag id -> entity assignment
                    _sandboxPatternGlobalTagIdTag = FileResourcer.Get().GetSchemaTag<S80806CAC>(val);
                    for (int o = 0; o < _sandboxPatternGlobalTagIdTag.TagData.SandboxPatternGlobalTagId.Count; o++)
                    {
                        var ent = _sandboxPatternGlobalTagIdTag.TagData.SandboxPatternGlobalTagId.ElementAt(_sandboxPatternGlobalTagIdTag.GetReader(), o);
                        Console.WriteLine($"{o} : {ent.ItemHash} {ent.PatternGlobalTagIdHash}");
                    }
                    break;

                case 0x808071C0:
                    _inventoryItemIconTag = FileResourcer.Get().GetSchemaTag<S808071C0>(val);
                    break;

                case 0x808055c2:
                    _artDyeReferenceTag = FileResourcer.Get().GetSchemaTag<SC2558080>(val);
                    break;

                case 0x808051f2:  // shadowkeep is 0x80805bde
                    _dyeChannelTag = FileResourcer.Get().GetSchemaTag<SDyeChannels>(val);
                    break;
            }
        });


        GetLocalizedStringsIndexDict(); // must be before GetInventoryItemStringThings

        // must be after string index is built

        Parallel.ForEach(allHashes, (val, state, i) =>
        {
            //switch (val.GetReferenceHash().Hash32)
            //{
            //    case 0x808077CD:
            //        _randomizedPlugSetMap = FileResourcer.Get().GetSchemaTag<SCD778080>(val);
            //        break;
            //    case 0x808076B6:
            //        _socketTypeMap = FileResourcer.Get().GetSchemaTag<SB6768080>(val);
            //        break;
            //    case 0x80804F59:
            //        _socketCategoryMap = FileResourcer.Get().GetSchemaTag<S594F8080>(val);
            //        break;
            //    case 0x808050CF:
            //        _loreStringMap = FileResourcer.Get().GetSchemaTag<SCF508080>(val);
            //        break;
            //    case 0x8080542D:
            //        _sandboxPerkMap = FileResourcer.Get().GetSchemaTag<S2D548080>(val);
            //        break;
            //    case 0x808076AA:
            //        _sandboxPerkMap2 = FileResourcer.Get().GetSchemaTag<SAA768080>(val);
            //        break;
            //    case 0x8080586B:
            //        _statDefinitionMap = FileResourcer.Get().GetSchemaTag<S6B588080>(val);
            //        break;
            //    case 0x808054BE:
            //        _statGroupDefinitionMap = FileResourcer.Get().GetSchemaTag<SBE548080>(val);
            //        break;
            //    case 0x80807828:
            //        _collectableDefinitionMap = FileResourcer.Get().GetSchemaTag<S28788080>(val);
            //        break;
            //    case 0x808059BF:
            //        _collectableStringsMap = FileResourcer.Get().GetSchemaTag<SBF598080>(val);
            //        break;
            //    case 0x8080753C:
            //        _objectiveDefinitionMap = FileResourcer.Get().GetSchemaTag<S3C758080>(val);
            //        break;
            //    case 0x8080584C:
            //        _objectiveStringsMap = FileResourcer.Get().GetSchemaTag<S4C588080>(val);
            //        break;
            //    case 0x808078D7:
            //        _presentationNodeDefinitionMap = FileResourcer.Get().GetSchemaTag<SD7788080>(val);
            //        break;
            //    case 0x80805803:
            //        _presentationNodeDefinitionStringMap = FileResourcer.Get().GetSchemaTag<S03588080>(val);
            //        break;
            //}
        });


        Task.WaitAll(new[]
        {
            Task.Run(GetInventoryItemDict),
            Task.Run(GetEntityAssignmentDict),
            Task.Run(GetInventoryItemStringThings),
            //Task.Run(GetSocketCategoryStrings),
            //Task.Run(GetInventoryItemLoreStrings),
            //Task.Run(GetSandboxPerkStrings),
            //Task.Run(GetStatStrings),
            //Task.Run(GetCollectableStrings),
            //Task.Run(GetObjectiveStrings),
            //Task.Run(GetSandboxPerkMap2),
        });
    }


    public string GetItemName(InventoryItem item)
    {
        return GetItemName(item.TagData.InventoryItemHash);
    }

    public string GetItemNameSanitized(InventoryItem item)
    {
        return Regex.Replace(GetItemName(item.TagData.InventoryItemHash), @"[^\u0000-\u007F]", "_");
    }

    public string GetItemName(TigerHash hash)
    {
        var entry = GetItemStrings(GetItemIndex(hash));
        return entry.TagData.ItemName.Value.ToString();
    }

    public Tag<S80806EF6>? GetItemStrings(TigerHash hash)
    {
        var entry = InventoryItemStringContainers[GetItemIndex(hash)];
        return entry;
    }

    public Tag<S80806EF6>? GetItemStrings(int index)
    {
        var entry = InventoryItemStringContainers[index];
        return entry;
    }

    public Tag<S80805335>? GetItemIconContainer(InventoryItem item)
    {
        return GetItemIconContainer(item.TagData.InventoryItemHash);
    }

    public Tag<S80805335>? GetItemIconContainer(TigerHash hash) // TODO
    {
        return null;
        //int iconIndex = GetItemStrings(GetItemIndex(hash)).TagData.IconIndex;
        //if (iconIndex == -1)
        //    return null;
        //return _inventoryItemIconTag.TagData.InventoryItemIconsMap.ElementAt(_inventoryItemIconTag.GetReader(), iconIndex).IconContainer;
    }

    public Tag<S80805335>? GetItemIconContainer(int index)
    {
        return _inventoryItemIconTag.TagData.InventoryItemIconsMap.ElementAt(_inventoryItemIconTag.GetReader(), index).IconContainer;
    }

    public Tag<S80805335>? GetFoundryItemIconContainer(InventoryItem item)
    {
        return GetFoundryItemIconContainer(item.TagData.InventoryItemHash);
    }

    public Tag<S80805335>? GetFoundryItemIconContainer(TigerHash hash) // TODO?
    {
        return null;
        //int iconIndex = GetItemStrings(GetItemIndex(hash)).TagData.FoundryIconIndex;
        //if (iconIndex == -1)
        //    return null;
        //return _inventoryItemIconTag.TagData.InventoryItemIconsMap.ElementAt(_inventoryItemIconTag.GetReader(), iconIndex).IconContainer;
    }

    public int GetItemIndex(TigerHash hash)
    {
        return _inventoryItemIndexmap[hash.Hash32];
    }

    public int GetItemIndex(uint hash32)
    {
        return _inventoryItemIndexmap[hash32];
    }

    private void GetLocalizedStringsIndexDict()
    {
        _localizedStringsIndexMap = new Dictionary<int, LocalizedStrings>(_localizedStringsIndexTag.TagData.StringIndexMap.Count);
        using TigerReader reader = _localizedStringsIndexTag.GetReader();
        for (int i = 0; i < _localizedStringsIndexTag.TagData.StringIndexMap.Count; i++)
        {
            _localizedStringsIndexMap.Add(i, _localizedStringsIndexTag.TagData.StringIndexMap[reader, i].LocalizedStrings);
        }
    }

    public LocalizedStrings GetLocalizedStringsFromIndex(int index)
    {
        // presume we want to read from it, so load it
        LocalizedStrings ls = _localizedStringsIndexMap[index];
        if (ls is not null)
        {
            ls.Load();
            return ls;
        }
        return null;
    }

    private void GetEntityAssignmentDict()
    {
        _sortedArrangementHashmap = new Dictionary<uint, Tag<S8080890B>>(_entityAssignmentsMap.TagData.EntityArrangementMap.Count);
        foreach (var e in _entityAssignmentsMap.TagData.EntityArrangementMap.Enumerate(_entityAssignmentsMap.GetReader()))
        {
            _sortedArrangementHashmap.Add(e.AssignmentHash, e.EntityParent);
        }
    }

    // Gets the actual entity now? Instead of using GetEntitiesFromHash/GetEntityFromAssignmentHash
    public Entity.Entity? GetPatternEntityFromHash(TigerHash hash)
    {
        Console.WriteLine("GetPatternEntityFromHash");
        var item = GetInventoryItem(hash);
        if (item.GetWeaponPatternIndex() == -1)
            return null;

        var patternGlobalId = GetPatternGlobalTagId(item);
        var patternData = _sandboxPatternAssignmentsTag.TagData.AssignmentBSL.BinarySearch(_sandboxPatternAssignmentsTag.GetReader(), patternGlobalId);

        Console.WriteLine($"GetPatternEntityFromHash {patternData.Value.ApiHash} : {patternData.Value.EntityRelationHash}");

        if (patternData.HasValue && patternData.Value.EntityRelationHash.IsValid() && patternData.Value.EntityRelationHash.GetReferenceHash() == 0x8080BAAD)
            return FileResourcer.Get().GetFile<Entity.Entity>(patternData.Value.EntityRelationHash);

        return null;
    }

    public TigerHash GetPatternGlobalTagId(InventoryItem item)
    {
        return _sandboxPatternGlobalTagIdTag.TagData.SandboxPatternGlobalTagId[_sandboxPatternGlobalTagIdTag.GetReader(), item.GetWeaponPatternIndex()].PatternGlobalTagIdHash;
    }

    public TigerHash GetWeaponContentGroupHash(InventoryItem item)
    {
        return _sandboxPatternGlobalTagIdTag.TagData.SandboxPatternGlobalTagId[_sandboxPatternGlobalTagIdTag.GetReader(), item.GetWeaponPatternIndex()].WeaponContentGroupHash;
    }

    public TigerHash GetChannelHashFromIndex(short index)
    {
        return _dyeChannelTag.TagData.ChannelHashes[_dyeChannelTag.GetReader(), index].ChannelHash;
    }

    public Dye? GetDyeFromIndex(short index)
    {
        var artEntry = _artDyeReferenceTag.TagData.ArtDyeReferences.ElementAt(_artDyeReferenceTag.GetReader(), index);

        var dyeEntry = _sandboxPatternAssignmentsTag.TagData.AssignmentBSL.BinarySearch(_sandboxPatternAssignmentsTag.GetReader(), artEntry.DyeManifestHash);
        if (dyeEntry.HasValue && dyeEntry.Value.EntityRelationHash.GetReferenceHash() == 0x80806fa3)
        {
            return FileResourcer.Get().GetSchemaTag<SE36C8080>(FileResourcer.Get().GetSchemaTag<S8080890B>(dyeEntry.Value.EntityRelationHash).TagData.EntityData).TagData.Dye;
        }
        return null;
    }

    public InventoryItem? TryGetInventoryItem(TigerHash hash)
    {
        if (_inventoryItemIndexmap.ContainsKey(hash))
            return GetInventoryItem(_inventoryItemIndexmap[hash]);
        else
            return null;
    }

    public InventoryItem GetInventoryItem(TigerHash hash)
    {
        return GetInventoryItem(_inventoryItemIndexmap[hash]);
    }

    public InventoryItem GetInventoryItem(int index)
    {
        var item = _inventoryItemMap.TagData.InventoryItemDefinitionEntries.ElementAt(_inventoryItemMap.GetReader(), index).InventoryItem;
        if (!item.IsLoaded())
            item.Load();

        return _inventoryItemMap.TagData.InventoryItemDefinitionEntries.ElementAt(_inventoryItemMap.GetReader(), index).InventoryItem;
    }

    public void GetInventoryItemDict()
    {
        _inventoryItemIndexmap = new Dictionary<uint, int>();
        _inventoryItems = new ConcurrentDictionary<uint, InventoryItem>();

        using TigerReader reader = _inventoryItemMap.GetReader();
        for (int i = 0; i < _inventoryItemMap.TagData.InventoryItemDefinitionEntries.Count; i++)
        {
            S89968080 entry = _inventoryItemMap.TagData.InventoryItemDefinitionEntries[reader, i];
            _inventoryItemIndexmap.Add(entry.InventoryItemHash, i);
            _inventoryItems.TryAdd(entry.InventoryItemHash, entry.InventoryItem);
        }
    }

    // Getter so we can load them properly
    public async Task<IEnumerable<InventoryItem>> GetInventoryItems()
    {
        ParallelOptions parallelOptions = new() { MaxDegreeOfParallelism = 16, CancellationToken = CancellationToken.None };
        await Parallel.ForEachAsync(_inventoryItems.Values, parallelOptions, async (item, ct) =>
        {
            // todo needs a proper consumer queue
            item.Load();
        });
        return _inventoryItems.Values;
    }

    public TigerHash GetArtArrangementHash(InventoryItem item)
    {
        return _artArrangementMap.TagData.ArtArrangementHashes.ElementAt(_artArrangementMap.GetReader(), item.GetArtArrangementIndex()).ArtArrangementHash;
    }

    public List<Entity.Entity> GetEntitiesFromHash(TigerHash hash)
    {
        var item = GetInventoryItem(hash);
        var index = item.GetArtArrangementIndex();
        List<Entity.Entity> entities = GetEntitiesFromArrangementIndex(index);
        return entities;
    }

    private List<Entity.Entity> GetEntitiesFromArrangementIndex(int index)
    {
        List<Entity.Entity> entities = new();
        var entry = _entityAssignmentTag.TagData.ArtArrangementEntityAssignments.ElementAt(_entityAssignmentTag.GetReader(), index);
        if (entry.MultipleEntityAssignments.Count == 0)  // single
        {
            if (entry.FeminineSingleEntityAssignment.IsValid())
            {
                var entity = GetEntityFromAssignmentHash(entry.FeminineSingleEntityAssignment);
                entity.Gender = DestinyGenderDefinition.Feminine;
                entities.Add(entity);
            }
            if (entry.MasculineSingleEntityAssignment.IsValid())
            {
                var entity = GetEntityFromAssignmentHash(entry.MasculineSingleEntityAssignment);
                entity.Gender = DestinyGenderDefinition.Masculine;
                entities.Add(entity);
            }
        }
        else
        {
            foreach (var entryMultipleEntityAssignment in entry.MultipleEntityAssignments)
            {
                foreach (var assignment in entryMultipleEntityAssignment.EntityAssignmentResource.Value.Value.EntityAssignments)
                {
                    if (assignment.EntityAssignmentHash.IsValid())
                    {
                        var assignmentEntity = GetEntityFromAssignmentHash(assignment.EntityAssignmentHash);
                        if (assignmentEntity != null)
                            entities.Add(assignmentEntity);
                    }
                }
            }
        }

        return entities;
    }

    private Entity.Entity GetEntityFromAssignmentHash(TigerHash assignmentHash)
    {
        if (!_sortedArrangementHashmap.ContainsKey(assignmentHash))
            return null;

        Tag<S8080890B> tag = _sortedArrangementHashmap[assignmentHash];
        tag.Load();

        if (tag.TagData.EntityData.IsInvalid() || tag.TagData.EntityData is null)
            return null;

        // if entity
        if (tag.TagData.EntityData.GetReferenceHash() == 0x8080BAAD)
            return FileResourcer.Get().GetFile<Entity.Entity>(tag.TagData.EntityData);

        return null;
    }

    #region OBSOLETE?

    public DynamicArray<SD5778080> GetRandomizedPlugSet(int index)
    {
        return _randomizedPlugSetMap.TagData.PlugSetDefinitionEntries.ElementAt(_randomizedPlugSetMap.GetReader(), index).ReusablePlugItems;
    }

    public SD3508080? GetItemLore(TigerHash hash) // TODO?
    {
        //var item = GetInventoryItem(hash);
        //if (item.TagData.Unk30.GetValue(item.GetReader()) is SB6738080)
        //    return InventoryItemLoreStrings[((SB6738080)item.TagData.Unk30.GetValue(item.GetReader())).LoreEntryIndex];
        //else
        return null;
    }

    public SBA768080 GetSocketType(int index)
    {
        return _socketTypeMap.TagData.SocketTypeEntries.ElementAt(_socketTypeMap.GetReader(), index);
    }

    public int GetSocketCategoryIndex(int index)
    {
        return _socketTypeMap.TagData.SocketTypeEntries.ElementAt(_socketTypeMap.GetReader(), index).SocketCategoryIndex;
    }

    private int GetStatGroupIndex(InventoryItem item) // TODO?
    {
        //var stringThing = GetItemStrings(item.TagData.InventoryItemHash);

        //if (stringThing.TagData.Unk78.GetValue(stringThing.GetReader()) is SB4548080 details)
        //    return details.StatGroupIndex;

        return -1;
    }

    public SC4548080? GetStatGroup(InventoryItem item)
    {
        var index = GetStatGroupIndex(item);
        if (index == -1 || index > _statGroupDefinitionMap.TagData.StatGroupDefinitions.Count)
            return null;

        return _statGroupDefinitionMap.TagData.StatGroupDefinitions.ElementAt(_statGroupDefinitionMap.GetReader(), index);
    }

    public S2C788080? GetCollectible(int index)
    {
        if (index == -1 || index > _collectableDefinitionMap.TagData.CollectibleDefinitionEntries.Count)
            return null;

        var reader = _collectableDefinitionMap.GetReader();
        var entry = _collectableDefinitionMap.TagData.CollectibleDefinitionEntries.ElementAt(reader, index);

        return entry;
    }

    public SC3598080? GetCollectibleStrings(int index)
    {
        if (index == -1 || index > _collectableDefinitionMap.TagData.CollectibleDefinitionEntries.Count)
            return null;

        return CollectableStrings[index];
    }

    public SC3598080? GetCollectibleStringsFromItemIndex(int index)
    {
        int stringIndex = -1;
        var reader = _collectableDefinitionMap.GetReader();
        for (int i = 0; i < _collectableDefinitionMap.TagData.CollectibleDefinitionEntries.Count; i++)
        {
            var entry = _collectableDefinitionMap.TagData.CollectibleDefinitionEntries.ElementAt(reader, i);
            if (entry.InventoryItemIndex == index)
            {
                stringIndex = i;
                break;
            }
        }

        if (stringIndex == -1 || stringIndex > CollectableStrings.Count)
            return null;

        return CollectableStrings[stringIndex];
    }

    public int GetObjectiveValue(int index)
    {
        if (index == -1 || index > _objectiveDefinitionMap.TagData.ObjectiveDefinitionEntries.Count)
            return 0;

        var reader = _objectiveDefinitionMap.GetReader();
        return _objectiveDefinitionMap.TagData.ObjectiveDefinitionEntries.ElementAt(reader, index).CompletionValue;
    }

    public S50588080? GetObjective(int index)
    {
        if (index == -1 || index > _objectiveStringsMap.TagData.ObjectiveDefinitionStringEntries.Count)
            return null;

        return ObjectiveStrings[index];
    }

    private void GetSandboxPerkMap2()
    {
        SandboxPerkMap2 = new();
        using TigerReader reader = _sandboxPerkMap2.GetReader();
        for (int i = 0; i < _sandboxPerkMap2.TagData.SandboxPerkDefinitionEntries.Count; i++)
        {
            SandboxPerkMap2.TryAdd(i, _sandboxPerkMap2.TagData.SandboxPerkDefinitionEntries[reader, i]);
        }
    }

    private void GetInventoryItemStringThings()
    {
        InventoryItemStringContainers = new ConcurrentDictionary<int, Tag<S80806EF6>>();
        using TigerReader reader = _inventoryItemStringContainerMap.GetReader();
        for (int i = 0; i < _inventoryItemStringContainerMap.TagData.Containers.Count; i++)
        {
            InventoryItemStringContainers.TryAdd(i, _inventoryItemStringContainerMap.TagData.Containers[reader, i].StringContainer);
        }
    }

    private void GetObjectiveStrings()
    {
        ObjectiveStrings = new();
        using TigerReader reader = _objectiveStringsMap.GetReader();
        for (int i = 0; i < _objectiveStringsMap.TagData.ObjectiveDefinitionStringEntries.Count; i++)
        {
            ObjectiveStrings.TryAdd(i, _objectiveStringsMap.TagData.ObjectiveDefinitionStringEntries[reader, i]);
        }
    }

    private void GetInventoryItemLoreStrings()
    {
        InventoryItemLoreStrings = new();
        using TigerReader reader = _loreStringMap.GetReader();
        for (int i = 0; i < _loreStringMap.TagData.LoreStringMap.Count; i++)
        {
            InventoryItemLoreStrings.TryAdd(i, _loreStringMap.TagData.LoreStringMap[reader, i]);
        }
    }

    private void GetSocketCategoryStrings()
    {
        SocketCategoryStringThings = new ConcurrentDictionary<int, S5D4F8080>();
        using TigerReader reader = _socketCategoryMap.GetReader();
        for (int i = 0; i < _socketCategoryMap.TagData.SocketCategoryEntries.Count; i++)
        {
            SocketCategoryStringThings.TryAdd(i, _socketCategoryMap.TagData.SocketCategoryEntries[reader, i]);
        }
    }

    private void GetSandboxPerkStrings()
    {
        SandboxPerkStrings = new();
        using TigerReader reader = _sandboxPerkMap.GetReader();
        for (int i = 0; i < _sandboxPerkMap.TagData.SandboxPerkDefinitionEntries.Count; i++)
        {
            SandboxPerkStrings.TryAdd(i, _sandboxPerkMap.TagData.SandboxPerkDefinitionEntries[reader, i]);
        }
    }

    private void GetStatStrings()
    {
        StatStrings = new();
        using TigerReader reader = _statDefinitionMap.GetReader();
        for (int i = 0; i < _statDefinitionMap.TagData.StatDefinitions.Count; i++)
        {
            StatStrings.TryAdd(i, _statDefinitionMap.TagData.StatDefinitions[reader, i]);
        }
    }
    #endregion

#if DEBUG
    public void DebugAllInvestmentEntities()
    {
        Dictionary<string, Dictionary<dynamic, TigerHash>> data = new();
        for (int i = (int)_entityAssignmentTag.TagData.ArtArrangementEntityAssignments.Count - 1; i >= 0; i--)
        {
            List<Entity.Entity> entities = GetEntitiesFromArrangementIndex(i);
            foreach (var entity in entities)
            {
                bool bAllValid = true;
                if (entity is null || entity.Model is null)
                    continue;
                foreach (var entry in entity.Model.TagData.Meshes[entity.Model.GetReader(), 0].Parts)
                {
                    foreach (var field in typeof(SD1878080).GetFields())
                    {
                        if (!data.ContainsKey(field.Name))
                        {
                            data.Add(field.Name, new Dictionary<dynamic, TigerHash>());
                        }
                        dynamic fieldValue = field.GetValue(entry);
                        if (fieldValue is not null && !data[field.Name].ContainsKey(fieldValue) && data[field.Name].Count < 10)
                        {
                            data[field.Name].Add(fieldValue, _entityAssignmentTag.TagData.ArtArrangementEntityAssignments.ElementAt(_entityAssignmentTag.GetReader(), i).ArtArrangementHash);
                        }

                        bAllValid &= data[field.Name].Count > 1;
                    }
                }
                if (bAllValid)
                {
                    var a = 0;
                }
            }
        }
    }

    private static Random rng = new Random();

    public void DebugAPIRequestAllInfo()
    {
        // get all inventory item hashes

        // var itemHash = 138282166;
        var l = _inventoryItemIndexmap.Keys.ToList();
        var shuffled = l.OrderBy(a => rng.Next()).ToList();
        foreach (var itemHash in shuffled)
        {
            if (itemHash != 731561450)
                continue;
            ManifestData? itemDef;
            byte[] tgxm;
            try
            {
                itemDef = MakeGetRequestManifestData($"https://www.light.gg/db/items/{itemHash}/?raw=2");
                // ManifestData? itemDef = MakeGetRequestManifestData($"https://lowlidev.com.au/destiny/api/gearasset/{itemHash.Hash}?destiny2");
                if (itemDef is null || itemDef.gearAsset is null || itemDef.gearAsset.content.Length == 0 || itemDef.gearAsset.content[0].geometry is null || itemDef.gearAsset.content[0].geometry.Length == 0)
                    continue;
                tgxm = MakeGetRequest(
                    $"https://www.bungie.net/common/destiny2_content/geometry/platform/mobile/geometry/{itemDef.gearAsset.content[0].geometry[0]}");
            }
            catch (Exception e)
            {
                continue;
            }

            // Read TGXM
            // File.WriteAllBytes("C:/T/geometry.tgxm", tgxm);
            var br = new BinaryReader(new MemoryStream(tgxm));
            // br.BaseStream.Seek(8, SeekOrigin.Begin);
            var magic = br.ReadBytes(4);
            if (magic.Equals(new byte[] { 0x54, 0x47, 0x58, 0x4d }))
            {
                continue;
            }
            var version = br.ReadUInt32();
            var fileOffset = br.ReadInt32();
            var fileCount = br.ReadInt32();
            for (int i = 0; i < fileCount; i++)
            {
                br.BaseStream.Seek(fileOffset + 0x110 * i, SeekOrigin.Begin);
                var fileName = Encoding.ASCII.GetString(br.ReadBytes(0x100)).TrimEnd('\0');
                var offset = br.ReadInt32();
                var type = br.ReadInt32();
                var size = br.ReadInt32();
                if (fileName.Contains(".js"))
                {
                    byte[] fileData;
                    Array.Copy(tgxm, offset, fileData = new byte[size], 0, size);
                    File.WriteAllBytes($"C:/T/geom/{itemHash}_{fileName}", fileData);
                }
            }
        }
    }

    public void DebugAPIRenderMetadata()
    {

        var files = Directory.GetFiles("C:/T/geom");
        foreach (var file in files)
        {
            dynamic json = JsonConvert.DeserializeObject(File.ReadAllText(file));
            var data = json["render_model"]["render_meshes"];
            var a = 0;
        }
    }

    private ManifestData MakeGetRequestManifestData(string url)
    {
        using (var client = new HttpClient())
        {
            client.Timeout = TimeSpan.FromSeconds(2);
            var response = client.GetAsync(url).Result;
            var content = response.Content.ReadAsStringAsync().Result;
            if (content.Contains("\"gearAsset\": false"))
            {
                return null;
            }
            ManifestData item = System.Text.Json.JsonSerializer.Deserialize<ManifestData>(content);
            return item;
        }
    }

    private byte[] MakeGetRequest(string url)
    {
        using (var client = new HttpClient())
        {
            client.Timeout = TimeSpan.FromSeconds(2);
            var response = client.GetAsync(url).Result;
            var content = response.Content.ReadAsByteArrayAsync().Result;
            return content;
        }
    }
#pragma warning disable S1144 // Unused private types or members should be removed
    private class ManifestData
    {
        public dynamic requestedId { get; set; }
        public DestinyGearAssetsDefinition gearAsset { get; set; }
        public dynamic definition { get; set; }
    }

    private class DestinyGearAssetsDefinition
    {
        public string[] gear { get; set; }
        public ContentDefinition[] content { get; set; }
    }

    private class ContentDefinition
    {
        public string platform { get; set; }
        public string[] geometry { get; set; }
        public string[] textures { get; set; }
        public IndexSet male_index_set { get; set; }
        public IndexSet female_index_set { get; set; }
        public IndexSet dye_index_set { get; set; }
        public Dictionary<string, IndexSet[]> region_index_sets { get; set; }
    }

    private class IndexSet
    {
        public int[] textures { get; set; }
        public int[] geometry { get; set; }
    }
#endif

    public void ExportShader(InventoryItem item, string savePath, string name, TextureExportFormat outputTextureFormat) // TODO
    {
        return;
        //        Dictionary<string, Dye> dyes = new();
        //        // export all the customDyes
        //        if (item.TagData.Unk90.GetValue(item.GetReader()) is S77738080 translationBlock)
        //        {
        //            foreach (var dyeEntry in translationBlock.CustomDyes)
        //            {
        //                Dye dye = GetDyeFromIndex(dyeEntry.DyeIndex);
        //                dye.ExportTextures(savePath + "/Textures", outputTextureFormat);
        //                dyes.Add(Dye.GetChannelName(GetChannelHashFromIndex(dyeEntry.ChannelIndex)), dye);
        //#if DEBUG
        //                System.Console.WriteLine($"{item.GetItemName()}: DefaultDye {dye.Hash}");
        //#endif
        //            }
        //        }
        //        // armor
        //        AutomatedExporter.SaveBlenderApiFile(savePath, name, outputTextureFormat, new List<Dye> { dyes["ArmorPlate"], dyes["ArmorSuit"], dyes["ArmorCloth"] }, "_armour");
        //        // ghost
        //        AutomatedExporter.SaveBlenderApiFile(savePath, name, outputTextureFormat, new List<Dye> { dyes["GhostMain"], dyes["GhostHighlights"], dyes["GhostDecals"] }, "_ghost");
        //        // ship
        //        AutomatedExporter.SaveBlenderApiFile(savePath, name, outputTextureFormat, new List<Dye> { dyes["ShipUpper"], dyes["ShipDecals"], dyes["ShipLower"] }, "_ship");
        //        // sparrow
        //        AutomatedExporter.SaveBlenderApiFile(savePath, name, outputTextureFormat, new List<Dye> { dyes["SparrowUpper"], dyes["SparrowEngine"], dyes["SparrowLower"] }, "_sparrow");
        //        // weapon
        //        AutomatedExporter.SaveBlenderApiFile(savePath, name, outputTextureFormat, new List<Dye> { dyes["Weapon1"], dyes["Weapon2"], dyes["Weapon3"] }, "_weapon");

        //        var iridesceneLookup = Globals.Get().RenderGlobals.TagData.Textures.TagData.IridescenceLookup;
        //        TextureExtractor.SaveTextureToFile($"{savePath}/Textures/Iridescence_Lookup", iridesceneLookup.GetScratchImage());

    }
}


public class InventoryItem : Tag<S8080968B>
{
    public InventoryItem(FileHash hash, bool shouldParse) : base(hash, shouldParse)
    {
    }

    public Tag<S80806EF6> GetItemStrings()
    {
        return Investment.Get().GetItemStrings(Investment.Get().GetItemIndex(_tag.InventoryItemHash));
    }

    public string GetItemName()
    {
        return Investment.Get().GetItemName(this);
    }

    public int GetItemDamageTypeIndex() // TODO?
    {
        //if (_tag.Unk78.GetValue(GetReader()) is S81738080 perks)
        //{
        //    foreach (var perk in perks.Perks)
        //    {
        //        if (Investment.Get().SandboxPerkMap2[perk.PerkIndex].UnkIndex != -1)
        //            return Investment.Get().SandboxPerkMap2[perk.PerkIndex].UnkIndex;
        //    }
        //}
        return -1;
    }

    public int GetArtArrangementIndex() // TODO? Unused now? Maybe??
    {
        if (_tag.Unk80 is null) return -1;
        if (_tag.Unk80.GetValue(GetReader()) is SD3918080 entry)
        {
            if (entry.Arrangements.Count > 0)
                return entry.Arrangements[GetReader(), 0].ArtArrangementHash;
        }
        return -1;
    }

    public int GetWeaponPatternIndex() // TODO? Not sure if right
    {
        if (_tag.Unk80.GetValue(GetReader()) is SD3918080 entry)
        {
            if (entry.WeaponPatternIndex != -1)
                return entry.WeaponPatternIndex;
        }
        return -1;
    }

    public List<InventoryItem> GetItemOrnaments() // TODO?
    {
        List<InventoryItem> ornaments = new();
        //if (_tag.Unk70.GetValue(GetReader()) is SC0778080 sockets)
        //{
        //    foreach (var socket in sockets.SocketEntries)
        //    {
        //        if (socket.SocketTypeIndex == -1 || Investment.Get().SocketCategoryStringThings[Investment.Get().GetSocketCategoryIndex(socket.SocketTypeIndex)].SocketName.Value != "WEAPON COSMETICS")
        //            continue;

        //        if (socket.PlugItems.Count == 0 && socket.ReusablePlugSetIndex1 != -1) // huh?
        //        {
        //            foreach (var randomPlugs in Investment.Get().GetRandomizedPlugSet(socket.ReusablePlugSetIndex1))
        //            {
        //                if (randomPlugs.PlugInventoryItemIndex == -1)
        //                    continue;

        //                ornaments.Add(Investment.Get().GetInventoryItem(randomPlugs.PlugInventoryItemIndex));
        //            }
        //        }

        //        foreach (var plug in socket.PlugItems)
        //        {
        //            if (plug.PlugInventoryItemIndex == -1)
        //                continue;

        //            ornaments.Add(Investment.Get().GetInventoryItem(plug.PlugInventoryItemIndex));
        //        }
        //    }
        //}
        return ornaments;
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
