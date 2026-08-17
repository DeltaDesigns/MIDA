using System.Collections.Concurrent;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;
using Arithmic;
using ConcurrentCollections;
using Newtonsoft.Json;
using Tiger.Exporters;
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
    private Tag<S80803081> _investmentCosmeticMap = null;
    private Tag<S808071C0> _inventoryItemIconTag = null;
    public Tag<S8080706A> _traitDefinitionStringMap = null;
    private Tag<S80808C6B> _socketTypeMap = null;
    private Tag<S80803419> _socketCategoryMap = null;

    // These still exist but are done differently I think?
    private Tag<SF2708080> _artArrangementMap = null;
    private Tag<SCE558080> _entityAssignmentTag = null;

    private Tag<S80806E50> _statDefinitionMap = null;
    private Tag<S808033FD> _statGroupDefinitionMap = null; // unsure

    private ConcurrentDictionary<uint, int> _inventoryItemHashIndexMap = null;
    private ConcurrentDictionary<int, InventoryItem> _inventoryItemIndexMap = null;
    private ConcurrentDictionary<uint, InventoryItem> _inventoryItems = null;

    private Dictionary<int, LocalizedStrings> _localizedStringsIndexMap = null;

    private Dictionary<uint, Tag<S8080890B>> _sortedArrangementHashmap = null;

    public ConcurrentDictionary<int, Tag<SInventoryItemStrings>> InventoryItemStringContainers = null;
    public ConcurrentDictionary<int, S6E708080> TraitIndexMap = null;
    public ConcurrentDictionary<MarathonTraitID, S6E708080> TraitMap = null;
    public ConcurrentDictionary<int, S8080341D> SocketCategoryStringThings = null;

    // Possibly obsolete things
    #region OBSOLETE?
    private Tag<S2D548080> _sandboxPerkMap = null;
    private Tag<SAA768080> _sandboxPerkMap2 = null;


    public ConcurrentDictionary<int, SD3508080> InventoryItemLoreStrings = null;
    public ConcurrentDictionary<int, S33548080> SandboxPerkStrings = null;
    public ConcurrentDictionary<int, S546E8080> StatStrings = null;
    public ConcurrentDictionary<int, SAE7680800> SandboxPerkMap2 = null;
    public ConcurrentDictionary<int, S50588080> ObjectiveStrings = null;

    public Tag<SD7788080> _presentationNodeDefinitionMap = null;
    public Tag<S03588080> _presentationNodeDefinitionStringMap = null;
    private Tag<S3C758080> _objectiveDefinitionMap = null;
    private Tag<S4C588080> _objectiveStringsMap = null;
    private Tag<SCF508080> _loreStringMap = null;
    private Tag<S28788080> _collectableDefinitionMap = null;
    private Tag<SBF598080> _collectableStringsMap = null;
    public ConcurrentDictionary<int, SC3598080> CollectableStrings = null;
    private ConcurrentDictionary<uint, InventoryItem> _collectableItems = null;
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
                case 0x808071C8:
                    _localizedStringsIndexTag = FileResourcer.Get().GetSchemaTag<S808071C8>(val);
                    break;
            }
        });


        GetLocalizedStringsIndexDict(); // must be before GetInventoryItemStringThings

        // must be after string index is built

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

                // named tag "investment_assets"
                case 0x80806590: // points to parent of the sandbox pattern ref list thing + entity assignment map
                    var parent = FileResourcer.Get().GetSchemaTag<S80806590>(val);
                    _sandboxPatternAssignmentsTag = parent.TagData.SandboxPatternAssignmentsTag; // also art dye refs
                    _entityAssignmentsMap = parent.TagData.EntityAssignmentsMap;
                    break;

                case 0x80806CAC: // inventory item -> pattern global tag id -> entity assignment
                    _sandboxPatternGlobalTagIdTag = FileResourcer.Get().GetSchemaTag<S80806CAC>(val);
                    //for (int o = 0; o < _sandboxPatternGlobalTagIdTag.TagData.SandboxPatternGlobalTagId.Count; o++)
                    //{
                    //    var ent = _sandboxPatternGlobalTagIdTag.TagData.SandboxPatternGlobalTagId.ElementAt(_sandboxPatternGlobalTagIdTag.GetReader(), o);
                    //    Console.WriteLine($"{o} : {ent.ItemHash} {ent.PatternGlobalTagIdHash}");
                    //}
                    break;

                case 0x808071C0:
                    _inventoryItemIconTag = FileResourcer.Get().GetSchemaTag<S808071C0>(val);
                    break;

                case 0x80803081:
                    _investmentCosmeticMap = FileResourcer.Get().GetSchemaTag<S80803081>(val);
                    break;

                case 0x8080706A:
                    _traitDefinitionStringMap = FileResourcer.Get().GetSchemaTag<S8080706A>(val);
                    break;

                //case 0x808077CD:
                //    _randomizedPlugSetMap = FileResourcer.Get().GetSchemaTag<SCD778080>(val);
                //    break;
                case 0x80808C6B:
                    _socketTypeMap = FileResourcer.Get().GetSchemaTag<S80808C6B>(val);
                    break;
                case 0x80803419:
                    _socketCategoryMap = FileResourcer.Get().GetSchemaTag<S80803419>(val);
                    break;
                //case 0x808050CF:
                //    _loreStringMap = FileResourcer.Get().GetSchemaTag<SCF508080>(val);
                //    break;
                //case 0x8080542D:
                //    _sandboxPerkMap = FileResourcer.Get().GetSchemaTag<S2D548080>(val);
                //    break;
                //case 0x808076AA:
                //    _sandboxPerkMap2 = FileResourcer.Get().GetSchemaTag<SAA768080>(val);
                //    break;
                case 0x80806E50:
                    _statDefinitionMap = FileResourcer.Get().GetSchemaTag<S80806E50>(val);
                    break;
                case 0x808033FD:
                    _statGroupDefinitionMap = FileResourcer.Get().GetSchemaTag<S808033FD>(val);
                    break;
                    //case 0x80807828:
                    //    _collectableDefinitionMap = FileResourcer.Get().GetSchemaTag<S28788080>(val);
                    //    break;
                    //case 0x808059BF:
                    //    _collectableStringsMap = FileResourcer.Get().GetSchemaTag<SBF598080>(val);
                    //    break;
                    //case 0x8080753C:
                    //    _objectiveDefinitionMap = FileResourcer.Get().GetSchemaTag<S3C758080>(val);
                    //    break;
                    //case 0x8080584C:
                    //    _objectiveStringsMap = FileResourcer.Get().GetSchemaTag<S4C588080>(val);
                    //    break;
                    //case 0x808078D7:
                    //    _presentationNodeDefinitionMap = FileResourcer.Get().GetSchemaTag<SD7788080>(val);
                    //    break;
                    //case 0x80805803:
                    //    _presentationNodeDefinitionStringMap = FileResourcer.Get().GetSchemaTag<S03588080>(val);
                    //    break;
            }
        });


        Task.WaitAll(new[]
        {
            Task.Run(DebugPrintTags),
            Task.Run(GetInventoryItemDict),
            Task.Run(GetEntityAssignmentDict),
            Task.Run(GetInventoryItemStringThings),
            Task.Run(GetStatStrings),
            Task.Run(GetTraitMap),
            Task.Run(GetSocketCategoryStrings),
        });

        //RunWithLogging(DebugPrintTags);
        //RunWithLogging(GetInventoryItemDict);
        //RunWithLogging(GetEntityAssignmentDict);
        //RunWithLogging(GetInventoryItemStringThings);
        //RunWithLogging(GetStatStrings);
        //RunWithLogging(GetTraitMap);

    }

    // Getter so we can load them properly
    public void GetInventoryItemDict()
    {
        _inventoryItemHashIndexMap = new ConcurrentDictionary<uint, int>();
        _inventoryItemIndexMap = new ConcurrentDictionary<int, InventoryItem>();
        _inventoryItems = new ConcurrentDictionary<uint, InventoryItem>();

        using TigerReader reader = _inventoryItemMap.GetReader();
        for (int i = 0; i < _inventoryItemMap.TagData.InventoryItemDefinitionEntries.Count; i++)
        {
            var entry = _inventoryItemMap.TagData.InventoryItemDefinitionEntries[reader, i];
            _inventoryItemHashIndexMap.TryAdd(entry.InventoryItemHash, i); // Hash -> Index
            _inventoryItemIndexMap.TryAdd(i, entry.InventoryItem); // Index -> InventoryItem
            _inventoryItems.TryAdd(entry.InventoryItemHash, entry.InventoryItem); // Hash -> InventoryItem
        }
    }

    public async Task<IEnumerable<InventoryItem>> GetInventoryItems()
    {
        ParallelOptions parallelOptions = new() { MaxDegreeOfParallelism = Environment.ProcessorCount, CancellationToken = CancellationToken.None };
        await Parallel.ForEachAsync(_inventoryItems.Values, parallelOptions, async (item, ct) =>
        {
            // todo needs a proper consumer queue
            item.Load();
        });
        return _inventoryItems.Values;
    }

    #region Strings
    private void GetInventoryItemStringThings()
    {
        InventoryItemStringContainers = new ConcurrentDictionary<int, Tag<SInventoryItemStrings>>();
        using TigerReader reader = _inventoryItemStringContainerMap.GetReader();
        for (int i = 0; i < _inventoryItemStringContainerMap.TagData.Containers.Count; i++)
        {
            InventoryItemStringContainers.TryAdd(i, _inventoryItemStringContainerMap.TagData.Containers[reader, i].StringContainer);
        }
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

    public S80808ACC GetSocketType(int index)
    {
        return _socketTypeMap.TagData.SocketTypeEntries.ElementAt(_socketTypeMap.GetReader(), index);
    }
    private void GetTraitMap()
    {
        TraitMap = new();
        TraitIndexMap = new();
        for (int i = 0; i < _traitDefinitionStringMap.TagData.TraitStrings.Count; i++)
        {
            var trait = _traitDefinitionStringMap.TagData.TraitStrings[i];
            TraitMap.TryAdd(trait.TraitHash, trait);
            TraitIndexMap.TryAdd(i, trait);

#if DEBUG
            // checks if MarathonTraitID enum contains the trait, if not prints it out
            if (!Enum.IsDefined(typeof(MarathonTraitID), trait.TraitHash))
                Console.WriteLine($"New Trait: _{GlobalStrings.Get().GetString(new((uint)trait.TraitHash))} = {(uint)trait.TraitHash},");
#endif
        }
    }

    public S6E708080? GetTrait(int index)
    {
        if (!TraitIndexMap.ContainsKey(index))
        {
            //Console.WriteLine($"TraitIndexMap {TraitIndexMap.Count} : {index}");
            return null;
        }

        return TraitIndexMap[index];
    }

    public S6E708080? GetTrait(MarathonTraitID traitID)
    {
        if (!TraitMap.ContainsKey(traitID))
            return null;

        return TraitMap[traitID];
    }

    public string GetItemName(InventoryItem item)
    {
        return GetItemName(item.TagData.InventoryItemHash);
    }

    public string GetItemName(TigerHash hash)
    {
        var entry = GetItemStrings(GetItemIndex(hash));
        var name = entry.TagData.ItemName.Value.ToString();
        if (name.StartsWith("NotFound-")) // TODO, probably temp
            return GlobalStrings.Get().GetString(new(hash.Hash32));
        else
            return entry.TagData.ItemName.Value.ToString();
    }

    public string GetItemNameSanitized(InventoryItem item)
    {
        return Regex.Replace(GetItemName(item.TagData.InventoryItemHash), @"[^\u0000-\u007F]", "_");
    }

    public string GetItemType(InventoryItem item)
    {
        var entry = GetItemStrings(GetItemIndex(item.TagData.InventoryItemHash));
        return entry.TagData.ItemType.Value.ToString();
    }

    public string GetItemTypeGlyph(InventoryItem item)
    {
        var entry = GetItemStrings(GetItemIndex(item.TagData.InventoryItemHash));
        return entry.TagData.ItemTypeGlyph.Value.ToString();
    }

    public Tag<SInventoryItemStrings>? GetItemStrings(TigerHash hash)
    {
        var entry = InventoryItemStringContainers[GetItemIndex(hash)];
        return entry;
    }

    public Tag<SInventoryItemStrings>? GetItemStrings(int index)
    {
        var entry = InventoryItemStringContainers[index];
        return entry;
    }
    #endregion

    #region Icons
    public int GetItemIconContainerIndex(InventoryItem item)
    {
        return GetItemStrings(GetItemIndex(item.TagData.InventoryItemHash)).TagData.IconIndex;
    }

    public Tag<S80805335>? GetItemIconContainer(InventoryItem item)
    {
        return GetItemIconContainer(item.TagData.InventoryItemHash);
    }

    public Tag<S80805335>? GetItemIconContainer(TigerHash hash) // TODO
    {
        int iconIndex = GetItemStrings(GetItemIndex(hash)).TagData.IconIndex;
        if (iconIndex == -1)
            return null;
        return _inventoryItemIconTag.TagData.InventoryItemIconsMap.ElementAt(_inventoryItemIconTag.GetReader(), iconIndex).IconContainer;
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

    public Texture? GetTextureFromContainer(Tag<S80805350> iconContainer, int index = 0, int listIndex = 0)
    {
        using TigerReader reader = iconContainer.GetReader();
        dynamic? prim = iconContainer.TagData.Unk10.GetValue(reader);
        if (prim is S4C538080 structCD3E8080)
        {
            // TextureList[0] is default, others are for colourblind modes
            if (index >= structCD3E8080.Unk00[reader, listIndex].TextureList.Count)
                return null;

            return structCD3E8080.Unk00[reader, listIndex].TextureList[reader, index].IconTexture;
        }
        if (prim is S48538080 structCB3E8080)
        {
            if (index >= structCB3E8080.Unk00[reader, listIndex].TextureList.Count)
                return null;

            return structCB3E8080.Unk00[reader, listIndex].TextureList[reader, index].IconTexture;
        }
        return null;
    }

    public Texture? GetTextureFromContainer(FileHash containerHash, int index, int listIndex = 0)
    {
        return GetTextureFromContainer(FileResourcer.Get().GetSchemaTag<S80805350>(containerHash), index, listIndex);
    }
    #endregion

    #region Item Specific
    public InventoryItem? TryGetInventoryItem(TigerHash hash)
    {
        if (_inventoryItemHashIndexMap.ContainsKey(hash))
            return GetInventoryItem(_inventoryItemHashIndexMap[hash]);
        else
            return null;
    }

    public InventoryItem GetInventoryItem(TigerHash hash)
    {
        return GetInventoryItem(_inventoryItemHashIndexMap[hash]);
    }

    public InventoryItem GetInventoryItem(int index)
    {
        InventoryItem item = _inventoryItemIndexMap[index];
        if (!item.IsLoaded())
            item.Load();

        return item;
    }

    public int GetItemIndex(TigerHash hash)
    {
        return _inventoryItemHashIndexMap[hash.Hash32];
    }

    public int GetItemIndex(uint hash32)
    {
        return _inventoryItemHashIndexMap[hash32];
    }

    private void GetEntityAssignmentDict()
    {
        _sortedArrangementHashmap = new Dictionary<uint, Tag<S8080890B>>(_entityAssignmentsMap.TagData.EntityArrangementMap.Count);
        foreach (var e in _entityAssignmentsMap.TagData.EntityArrangementMap.Enumerate(_entityAssignmentsMap.GetReader()))
        {
            _sortedArrangementHashmap.Add(e.AssignmentHash, e.EntityParent);
        }
    }

    public Entity.Entity? GetPatternEntity(InventoryItem item)
    {
        return GetPatternEntityFromHash(item.TagData.InventoryItemHash);
    }

    // Gets the actual entity now? Instead of using GetEntitiesFromHash/GetEntityFromAssignmentHash
    public Entity.Entity? GetPatternEntityFromHash(TigerHash hash)
    {
        var item = GetInventoryItem(hash);
        var index = item.GetPatternIndex();
        Log.Debug($"{index} : CosmeticID {GetPatternGlobalCosmeticID(index)}");
        if (index == -1)
            return null;

        var patternGlobalId = GetPatternGlobalTagId(item);
        var patternData = _sandboxPatternAssignmentsTag.TagData.AssignmentBSL.BinarySearch(_sandboxPatternAssignmentsTag.GetReader(), patternGlobalId);

        Log.Debug($"PatternGlobalId {patternGlobalId.Reverse()}");
        if (patternData.HasValue && patternData.Value.EntityRelationHash.IsValid() && patternData.Value.EntityRelationHash.GetReferenceHash() == 0x8080BAAD)
        {
            var ent = FileResourcer.Get().GetFile<Entity.Entity>(patternData.Value.EntityRelationHash);
            Log.Debug($"Entity {ent.Hash}");
            return ent;
        }

        return null;
    }

    // TODO: Combine these 2 methods? ugly rn

    // some items like the Patch Kit have 2 valid indexes
    // the first index is the "world entity" that is shown when the item is used (the injector in this case)
    // the second index is the "ui entity" that is shown in the players inventory
    public Entity.Entity? GetPatternEntity2(InventoryItem item)
    {
        var index = item.GetPatternIndex2();
        Log.Debug($"{index} : CosmeticID {GetPatternGlobalCosmeticID(index)}");
        if (index == -1)
            return null;

        var patternGlobalId = GetPatternGlobalTagId2(item);
        var patternData = _sandboxPatternAssignmentsTag.TagData.AssignmentBSL.BinarySearch(_sandboxPatternAssignmentsTag.GetReader(), patternGlobalId);

        if (patternData.HasValue && patternData.Value.EntityRelationHash.IsValid() && patternData.Value.EntityRelationHash.GetReferenceHash() == 0x8080BAAD)
        {
            var ent = FileResourcer.Get().GetFile<Entity.Entity>(patternData.Value.EntityRelationHash);
            Log.Debug($"Entity {ent.Hash}");
            return ent;
        }

        return null;
    }

    // used for some things Sponsor Kits, seen it also used for ammo rounds?
    public Entity.Entity? GetPatternEntity3(InventoryItem item)
    {
        var index = item.GetPatternIndex3();
        Log.Debug($"{index} : CosmeticID {GetPatternGlobalCosmeticID(index)}");
        if (index == -1)
            return null;

        var patternGlobalId = GetPatternGlobalTagId3(item);
        var patternData = _sandboxPatternAssignmentsTag.TagData.AssignmentBSL.BinarySearch(_sandboxPatternAssignmentsTag.GetReader(), patternGlobalId);

        if (patternData.HasValue && patternData.Value.EntityRelationHash.IsValid() && patternData.Value.EntityRelationHash.GetReferenceHash() == 0x8080BAAD)
        {
            var ent = FileResourcer.Get().GetFile<Entity.Entity>(patternData.Value.EntityRelationHash);
            Log.Debug($"Entity {ent.Hash}");
            return ent;
        }

        return null;
    }


    public SB06C8080 GetPatternGlobalTagStruct(InventoryItem item)
    {
        return _sandboxPatternGlobalTagIdTag.TagData.SandboxPatternGlobalTagId[_sandboxPatternGlobalTagIdTag.GetReader(), item.GetPatternIndex()];
    }

    public SB06C8080 GetPatternGlobalTagStruct(int index)
    {
        return _sandboxPatternGlobalTagIdTag.TagData.SandboxPatternGlobalTagId[_sandboxPatternGlobalTagIdTag.GetReader(), index];
    }
    public TigerHash GetPatternGlobalTagId(InventoryItem item)
    {
        return _sandboxPatternGlobalTagIdTag.TagData.SandboxPatternGlobalTagId[_sandboxPatternGlobalTagIdTag.GetReader(), item.GetPatternIndex()].PatternGlobalTagIdHash;
    }

    public TigerHash GetPatternGlobalTagId2(InventoryItem item)
    {
        return _sandboxPatternGlobalTagIdTag.TagData.SandboxPatternGlobalTagId[_sandboxPatternGlobalTagIdTag.GetReader(), item.GetPatternIndex2()].PatternGlobalTagIdHash;
    }

    public TigerHash GetPatternGlobalTagId3(InventoryItem item)
    {
        return _sandboxPatternGlobalTagIdTag.TagData.SandboxPatternGlobalTagId[_sandboxPatternGlobalTagIdTag.GetReader(), item.GetPatternIndex3()].PatternGlobalTagIdHash;
    }

    public int GetPatternGlobalCosmeticID(InventoryItem item)
    {
        return GetPatternGlobalCosmeticID(item.GetPatternIndex());
    }

    public int GetPatternGlobalCosmeticID(int index)
    {
        if (index < 0 || index >= _sandboxPatternGlobalTagIdTag.TagData.SandboxPatternGlobalTagId.Count)
            return -1;

        return _sandboxPatternGlobalTagIdTag.TagData.SandboxPatternGlobalTagId[_sandboxPatternGlobalTagIdTag.GetReader(), index].SkinID;
    }

    // TODO? Unsure if used in marathon
    public TigerHash GetWeaponContentGroupHash(InventoryItem item)
    {
        return _sandboxPatternGlobalTagIdTag.TagData.SandboxPatternGlobalTagId[_sandboxPatternGlobalTagIdTag.GetReader(), item.GetPatternIndex()].WeaponContentGroupHash;
    }

    public TigerHash GetArtArrangementHash(InventoryItem item)
    {
        return _artArrangementMap.TagData.ArtArrangementHashes.ElementAt(_artArrangementMap.GetReader(), item.GetArtArrangementIndex()).ArtArrangementHash;
    }

    // Obsolete?
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

    public List<Entity.Entity> GetEntitiesFromPattern(Entity.Entity pattern)
    {
        List<Entity.Entity> entities = new();
        if (pattern is null)
            return entities;

        //if (pattern.AttachmentInfo is not null)
        //{
        //    Console.WriteLine($"{pattern.AttachmentInfo.Hash.Reverse()}");
        //    for (int i = 0; i < pattern.AttachmentInfo.Transforms.Count; i++)
        //    {
        //        var attachment = pattern.AttachmentInfo.Transforms.ElementAt(i);
        //        Console.WriteLine($"{i} BoneIndex {attachment.Value.BoneIndex}: {attachment.Key.Reverse()}:\n{attachment.Value.Transform.ToString()}\n");
        //        //Console.WriteLine($"{i}: {new StringHash(attachment.Key).GetString()}:\n{attachment.Value.ToString()}\n");
        //    }
        //}

        foreach (var resourceHash in pattern.Components)
        {
            EntityComponent resource = FileResourcer.Get().GetFile<EntityComponent>(resourceHash);
            switch (resource.TagData.Unk10.GetValue(resource.GetReader()))
            {
                case S808032B1: // TODO? Seems to just be a basic version of the main weapon model
                    //foreach (var entry in ((SB3328080)resource.TagData.Unk18.GetValue(resource.GetReader())).Unk128)
                    //{
                    //    if (entry.Entity is not null)
                    //        entities.Add(entry.Entity);
                    //}
                    break;

                case S80804603: // Attachments
                    foreach (var entry in ((S80804A14)resource.TagData.Unk18.GetValue(resource.GetReader())).Unk3E8)
                    {
                        if (entry.Entity is not null)
                            entities.Add(entry.Entity);
                    }
                    break;

                case S8080B69A:
                    foreach (var entry in ((S80809F51)resource.TagData.Unk18.GetValue(resource.GetReader())).Array2)
                    {
                        if (entry.Unk10.GetValue(resource.GetReader()) is S8080A398 entry2 && entry2.Entity is not null)
                            entities.Add(entry2.Entity);
                    }
                    break;

                case S80803287: // World model, used for things like the Patch Kit injector
                    var S88328080 = ((S80803288)resource.TagData.Unk18.GetValue(resource.GetReader()));
                    if (S88328080.Entity is not null)
                        entities.Add(S88328080.Entity);
                    break;
            }
        }


        return entities;
    }

    public Entity.Entity GetEntityFromCosmeticMap(InventoryItem item)
    {
        var index = item.GetArtArrangementIndex();
        Log.Debug($"ArtArrangementIndex {index}");
        if (index == -1)
        {
            index = item.GetPatternIndex();
            if (index == -1)
                return null;
        }

        Log.Debug($"Cosmetic Ent PatternGlobal EntryHash {GetPatternGlobalTagStruct(index).EntryHash.Reverse()}");
        var cosmeticID = GetPatternGlobalCosmeticID(index);
        if (cosmeticID == -1)
            return null;

        var ent = _investmentCosmeticMap.TagData.InvestmentCosmetics.ElementAt(_investmentCosmeticMap.GetReader(), cosmeticID).Pattern.TagData.Pattern;
        Log.Debug($"Cosmetic ID {cosmeticID} : Pattern {ent.Hash}");
        return ent;
    }
    #endregion

    #region OBSOLETE?
    public SD3508080? GetItemLore(TigerHash hash) // TODO?
    {
        //var item = GetInventoryItem(hash);
        //if (item.TagData.Unk30.GetValue(item.GetReader()) is SB6738080)
        //    return InventoryItemLoreStrings[((SB6738080)item.TagData.Unk30.GetValue(item.GetReader())).LoreEntryIndex];
        //else
        return null;
    }

    //public int GetSocketCategoryIndex(int index)
    //{
    //    return _socketTypeMap.TagData.SocketTypeEntries.ElementAt(_socketTypeMap.GetReader(), index).SocketCategoryIndex;
    //}

    private int GetStatGroupIndex(InventoryItem item) // TODO?
    {
        if (item.TagData.Unk70.GetValue(item.GetReader()) is S80809245 details)
            return details.StatGroupIndex;

        return -1;
    }

    public S01348080? GetStatGroup(InventoryItem item)
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
        SocketCategoryStringThings = new ConcurrentDictionary<int, S8080341D>();
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


    public static void ExportInventoryItem(InventoryItem item,
        string savePath,
        bool aggregateOutput = false,
        Dictionary<MarathonAttachmentType, InventoryItem> overrideAttachments = null)
    {
        Log.Debug($"Begin export on {item.Name} ({item.ApiHash})");
        ConfigSubsystem config = ConfigSubsystem.Get();
        string name = Helpers.SanitizeString(item.Name);
        if (!aggregateOutput)
            savePath = config.GetExportSavePath() + $"/{name}";

        Directory.CreateDirectory(savePath);
        Directory.CreateDirectory($"{savePath}/Textures");

        // custom skeleton assignment, i currently have no idea how/where Runner skeletons are assigned
        var skeleHash = MarathonRunnerSkeletons.Fallback;
        bool isRunner = false;
        if (item.ItemTraits.Any(x => x.GetTraitType() == MarathonItemType.Runner))
        {
            isRunner = true;
            uint trait = (uint)item.ItemTraits.First(x => x.GetTraitType() == MarathonItemType.Runner);
            switch (trait)
            {
                case 2886882161: // Vandal
                    skeleHash = MarathonRunnerSkeletons.Vandal;
                    break;
                case 3200411899: // Rook
                    skeleHash = MarathonRunnerSkeletons.Rook;
                    break;
                case 138305627: // Triage
                    skeleHash = MarathonRunnerSkeletons.Triage;
                    break;
                case 223585903: // Destroyer
                    skeleHash = MarathonRunnerSkeletons.Destroyer;
                    break;
                case 2824782972: // Assassin
                    skeleHash = MarathonRunnerSkeletons.Assassin;
                    break;
                case 940625209: // Thief
                    skeleHash = MarathonRunnerSkeletons.Thief;
                    break;
                case 3032611596: // Recon
                    skeleHash = MarathonRunnerSkeletons.Recon;
                    break;
                case 4158276887: // Sentinel
                    skeleHash = MarathonRunnerSkeletons.Sentinel;
                    break;
            }
        }

        List<Entity.Entity> entities = new();
        Entity.Entity skele = FileResourcer.Get().GetFile<Entity.Entity>(new FileHash(Hash64Map.Get().GetHash32Checked((ulong)skeleHash))); // 64 bit more permanent
        EntitySkeleton overrideSkeleton = new EntitySkeleton(skele.Skeleton.Hash);

        //var val = Investment.Get().GetPatternEntityFromHash(item.Parent != null ? item.Parent.TagData.InventoryItemHash : item.Item.TagData.InventoryItemHash);
        var patternEnt = Investment.Get().GetPatternEntity(item.Parent != null ? item.Parent : item);
        if (patternEnt != null)
        {
            Log.Debug($"Pattern Ent {patternEnt.Hash} : {patternEnt.HasGeometry()}");
            if (patternEnt.Model != null)
                entities.Add(patternEnt);

            if (patternEnt.Skeleton != null)
                overrideSkeleton = patternEnt.Skeleton;
        }

        entities.AddRange(Investment.Get().GetEntitiesFromPattern(patternEnt));
        entities.Add(Investment.Get().GetPatternEntity2(item));
        entities.Add(Investment.Get().GetPatternEntity3(item));
        if (isRunner)
            entities.Add(skele);

        //var entities2 = Investment.Get().GetEntitiesFromHash(item.TagData.InventoryItemHash);
        var cosmeticEnt = Investment.Get().GetEntityFromCosmeticMap(item);

        Log.Info($"Exporting investment item: {name}");
        if (item.ItemTraits.Any(x => x.GetTraitType() == MarathonItemType.Weapon))
        {
            var scene = Tiger.Exporters.Exporter.Get().CreateScene(name, ExportType.Weapon);
            ExportInvestmentWeapon(scene, savePath, cosmeticEnt, patternEnt, entities, overrideAttachments);
        }
        else if (item.ItemTraits.Any(x => x.GetTraitType() == MarathonItemType.Sticker))
        {
            ExportSticker(cosmeticEnt, savePath);
        }
        else
        {
            if (cosmeticEnt != null)
            {
                entities.Add(cosmeticEnt);
                entities.AddRange(cosmeticEnt.GetEntityChildren()); // Mainly for runner heads
            }

            foreach (Entity.Entity entity in entities)
            {
                if (entity is null)
                    continue;

                var scene = Tiger.Exporters.Exporter.Get().CreateScene(entity.Hash, ExportType.Entities);

                Log.Debug($"Entity {entity?.Hash}: HasGeometry {entity?.HasGeometry()}: HasSkeleton {entity?.Skeleton != null}");

                List<DynamicMeshPart> dynamicParts = entity.Load(ExportDetailLevel.MostDetailed);

                List<BoneNode> boneNodes = new List<BoneNode>();
                // Only assigns the override skele if the entity DOES have weights but no actual skeleton tied to it
                if (entity.Skeleton == null && overrideSkeleton != null && dynamicParts.Any(x => x.VertexWeights.Any()))
                    entity.Skeleton = overrideSkeleton;

                if (entity.Skeleton != null)
                    boneNodes = entity.Skeleton.GetBoneNodes();

                scene.AddEntity(entity, dynamicParts, boneNodes);
                entity.SaveMaterialsFromParts(scene, dynamicParts);
                entity.SaveTexturePlates(savePath);

                Tiger.Exporters.Exporter.Get().Export(savePath);
            }
        }

        //if (!aggregateOutput)
        //    Tiger.Exporters.Exporter.Get().Export();
        //else
        //    Tiger.Exporters.Exporter.Get().Export(savePath);

        Log.Info($"Exported investment item {name} to {savePath.Replace('\\', '/')}/");
    }

    private static void ExportSticker(Entity.Entity cosmeticEnt, string savePath)
    {
        foreach (var resourceHash in cosmeticEnt.Components)
        {
            EntityComponent resource = FileResourcer.Get().GetFile<EntityComponent>(resourceHash);
            switch (resource.TagData.Unk10.GetValue(resource.GetReader()))
            {
                case S8080B69A:
                    foreach (var entry in ((S80809F51)resource.TagData.Unk18.GetValue(resource.GetReader())).Array2)
                    {
                        if (entry.Unk10.GetValue(resource.GetReader()) is S80807F69 entry2 && entry2.Unk24 is not null)
                        {
                            if (entry2.Unk24.TagData.Materials.Count == 0)
                                continue;

                            using TigerReader reader = entry2.Unk24.Reader;
                            var stickerMat = entry2.Unk24.TagData.Materials[reader, 0].Material;
                            if (stickerMat is not null)
                                stickerMat.Export(savePath);
                        }
                    }
                    break;
            }
        }
    }

    private static void ExportInvestmentWeapon(ExporterScene scene,
        string savePath,
        Entity.Entity mainWep,
        Entity.Entity patternEnt, // mainWep skeleton
        List<Entity.Entity> attachments,
        Dictionary<MarathonAttachmentType, InventoryItem> overrideAttachments = null)
    {
        mainWep.ItemType = MarathonItemType.Weapon;
        var mainWepParts = mainWep.Load(ExportDetailLevel.MostDetailed);
        scene.AddEntity(mainWep, mainWepParts, patternEnt.Skeleton.GetBoneNodes());
        mainWep.SaveMaterialsFromParts(scene, mainWepParts);

        ExportShader(mainWep, scene.Name, savePath);

        if (overrideAttachments is not null)
        {
            var overrideTypes = new HashSet<MarathonAttachmentType>(overrideAttachments.Keys);
            attachments.RemoveAll(x => x is not null && x.AttachmentType.HasValue && overrideTypes.Contains(x.AttachmentType.Value));
            foreach (var att in overrideAttachments)
            {
                var attEntity = Investment.Get().GetPatternEntity(att.Value);
                attachments.Add(attEntity);
            }
        }

        foreach (Entity.Entity curEntity in attachments)
        {
            Entity.Entity entity = curEntity;
            if (entity is null)
                continue;

            //Console.WriteLine($"{entity.AttachmentType} : {entity.Hash}");

            List<BoneNode> boneNodes = new List<BoneNode>();
            List<DynamicMeshPart> dynamicParts = entity.Load(ExportDetailLevel.MostDetailed);

            // Only assigns the override skele if the entity DOES have weights but no actual skeleton tied to it
            if (entity.Skeleton == null && patternEnt.Skeleton != null && dynamicParts.Any(x => x.VertexWeights.Any()))
                boneNodes = patternEnt.Skeleton.GetBoneNodes();

            // some attachments like the Overrun ARs sight have a skeleton
            if (entity.Skeleton != null)
                boneNodes = entity.Skeleton.GetBoneNodes();

            if (entity.Model != null && entity.AttachmentID != null
               && patternEnt != null && patternEnt.AttachmentInfo != null)
            {
                if (patternEnt.AttachmentInfo.Transforms.TryGetValue(entity.AttachmentID, out var transform))
                {
                    int parentBone = transform.BoneIndex;
                    entity.Model.AttachmentBoneIndex = parentBone;
                    entity.ItemType = MarathonItemType.WeaponAttachment;

                    var offset = transform.Transform;
                    if (parentBone == 0) // root
                    {
                        Quaternion newRot = offset.Rotation.ToQuat();

                        if (entity.Skeleton == null) // idk why but i gotta do this for blender
                        {
                            float angle = MathF.PI / 2; // 90 degrees
                            Quaternion rotZ = Quaternion.CreateFromAxisAngle(System.Numerics.Vector3.UnitZ, angle);
                            newRot = Quaternion.Normalize(Quaternion.Multiply(newRot, rotZ));
                        }

                        entity.Model.TranslationOffset = offset.Translation;
                        entity.Model.RotationOffset = new Tiger.Schema.Vector4(newRot.X, newRot.Y, newRot.Z, newRot.W);
                    }
                    else if (patternEnt.Skeleton is not null)
                    {
                        var nodes = patternEnt.Skeleton.GetBoneNodes();
                        if (nodes.Count >= parentBone)
                        {
                            Tiger.Schema.Vector4 offsetTrans = new(nodes[parentBone].DefaultObjectSpaceTransform.Translation);
                            Tiger.Schema.Vector4 offsetRot = nodes[parentBone].DefaultObjectSpaceTransform.QuaternionRotation;

                            Quaternion newRot = offsetRot.ToQuat() * offset.Rotation.ToQuat();
                            float angle = MathF.PI / 2; // 90 degrees
                            Quaternion rotZ = Quaternion.CreateFromAxisAngle(System.Numerics.Vector3.UnitZ, angle);
                            newRot = Quaternion.Normalize(Quaternion.Multiply(newRot, rotZ));

                            Quaternion boneRot = offsetRot.ToQuat();
                            System.Numerics.Vector3 localOffset = new System.Numerics.Vector3(
                                offset.Translation.X,
                                offset.Translation.Y,
                                offset.Translation.Z
                            );

                            // rotate offset into bone space
                            System.Numerics.Vector3 rotatedOffset = System.Numerics.Vector3.Transform(localOffset, boneRot);

                            offsetTrans += new Tiger.Schema.Vector4(rotatedOffset);
                            offsetRot = new Tiger.Schema.Vector4(newRot.X, newRot.Y, newRot.Z, newRot.W);

                            entity.Model.TranslationOffset = offsetTrans;
                            entity.Model.RotationOffset = offsetRot;
                        }
                    }
                }
            }

            scene.AddEntity(entity, dynamicParts, boneNodes);
            entity.SaveMaterialsFromParts(scene, dynamicParts);
        }

        Tiger.Exporters.Exporter.Get().Export(savePath);
    }

    public static void ExportShader(Entity.Entity mainWep, string name, string savePath)
    {
        if (mainWep.ObjectChannels is null)
            return;

        Log.Debug($"Channels {mainWep.ObjectChannels.Hash}");

        var dyes = new Dictionary<int, Vector4> { [0] = new(0.2158605f, 0.2158605f, 0.2158605f, 1f) };
        var metal = new Dictionary<int, Vector4> { [0] = new(1f, 0f, 0f, 1f) };
        var rough = new Dictionary<int, Vector4> { [0] = new(1f, 0f, 0f, 1f) };

        var dyeMap = new Dictionary<uint, int>
        {
            // R                G                   B
            { 0x1B3D64F3, 1 }, { 0x1B3D64F6, 2 }, { 0x1B3D64F0, 3 },
            // Y                C                   M
            { 0x1B3D64F1, 4 }, { 0x1B3D64F7, 5 }, { 0x1B3D64F4, 6 },
        };

        var metalMap = new Dictionary<uint, int> // I *think* these are right now...? maybe?
        {
            { 0xD5754C52, 1 }, { 0xD5754C57, 2 }, { 0xD5754C51, 3 },
            { 0xD5754C50, 4 }, { 0xD5754C56, 5 }, { 0xD5754C55, 6 },
        };

        var roughMap = new Dictionary<uint, int>
        {
            { 0xBF1554A8, 1 }, { 0xBF1554AD, 2 }, { 0xBF1554AB, 3 },
            { 0xBF1554AA, 4 }, { 0xBF1554AC, 5 }, { 0xBF1554AF, 6 },
        };

        var channels = ((S8080AF75)mainWep.ObjectChannels.GetUnk18()).Unk130;
        foreach (var channel in channels)
        {
            var vec = channel.UnkConstants.Count != 0
                ? channel.UnkConstants[0].Vec.WithW(1)
                : Vector4.Zero;

            uint hash = channel.ChannelHash.Hash32;

            if (dyeMap.TryGetValue(hash, out int d))
                dyes.TryAdd(d, vec);

            if (metalMap.TryGetValue(hash, out int m))
                metal.TryAdd(m, vec);

            if (roughMap.TryGetValue(hash, out int r))
                rough.TryAdd(r, vec);
        }

        var result = new Dictionary<string, Dictionary<int, JsonWeaponDye>>
        {
            ["DyeColors"] = dyes
            .OrderBy(x => x.Key)
            .ToDictionary(
                x => x.Key,
                x => new JsonWeaponDye
                {
                    Vector = x.Value.ToFloatArray(),
                    String = $"[{x.Value.X}, {x.Value.Y}, {x.Value.Z}, 1]"
                }),

            ["MetalRemaps"] = metal
            .OrderBy(x => x.Key)
            .ToDictionary(
                x => x.Key,
                x => new JsonWeaponDye
                {
                    Vector = x.Value.ToFloatArray(),
                    String = $"[{x.Value.X}, {x.Value.Y}, {x.Value.Z}, 1]"
                }),

            ["RoughRemaps"] = rough
            .OrderBy(x => x.Key)
            .ToDictionary(
                x => x.Key,
                x => new JsonWeaponDye
                {
                    Vector = x.Value.ToFloatArray(),
                    String = $"[{x.Value.X}, {x.Value.Y}, {x.Value.Z}, 1]"
                })
        };

        File.WriteAllText(Path.Combine(savePath, $"{name}_DyeColors.json"), JsonConvert.SerializeObject(result, Formatting.Indented));
    }

    private void RunWithLogging(Action method)
    {
        string methodName = method.Method.Name;
        try
        {
            Stopwatch sw = Stopwatch.StartNew();
            Log.Debug($"Starting {methodName}");
            method();
            sw.Stop();
            Log.Debug($"Completed {methodName} in {sw.Elapsed.Milliseconds}ms");
        }
        catch (Exception ex)
        {
            Log.Error($"Error in {methodName}: {ex.Message}");
            throw;
        }
    }

    public void DebugPrintTags()
    {
#if DEBUG
        var fields = typeof(Investment).GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        foreach (var field in fields)
        {
            if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(Tag<>))
            {
                var tagInstance = field.GetValue(this);
                if (tagInstance != null)
                {
                    var hashProperty = field.FieldType.GetField("Hash");
                    var hashValue = hashProperty?.GetValue(tagInstance) ?? null;
                    Console.WriteLine($"{field.Name}: {(hashValue ?? $"NULL")}");
                }
            }
        }
#endif
    }

    private struct JsonWeaponDye
    {
        public float[] Vector;
        public string String;
    }
}
