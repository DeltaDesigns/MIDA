using Tiger.Exporters;

namespace Tiger.Schema.Entity;

public class Entity : Tag<SEntity>
{
    public TfxFeatureRenderer FeatureType = TfxFeatureRenderer.DynamicObjects;
    // Entity features, todo clean this up
    public EntitySkeleton? Skeleton { get; set; }
    public EntityModel? Model { get; private set; }
    public EntityResource? ModelParentResource { get; private set; }
    public EntityModel? PhysicsModel { get; private set; }
    public EntityPhysicsModelParent? PhysicsModelParentResource { get; private set; }
    public EntityResource? PatternAudio { get; private set; }
    public EntityResource? PatternAudioUnnamed { get; private set; }
    public EntityControlRig? ControlRig { get; private set; }
    public EntityResource? EntityChildren { get; private set; }
    public string? EntityName { get; set; } // Usually just the generic name (Ogre, Vandal, etc)
    public DestinyGenderDefinition Gender { get; set; } = DestinyGenderDefinition.None; // Only used for player armor

    public List<EntityResource>? EntityChildren2 { get; private set; } // Some weird collection of entities thats only in Pre-BL?

    private bool _loaded = false;

    public Entity(FileHash hash) : base(hash)
    {
        Load();
    }

    public Entity(FileHash hash, bool shouldParse = true) : base(hash, shouldParse)
    {
        if (shouldParse)
            Load();
    }

    // TODO: Figure out a way to make Dynamics view not take 10gb of ram and almost a minute to load.
    // Tried setting most things to NoLoad but it made little difference. I'm not entirely sure what's being the
    // resource hog. Thought it was EntityModel but I guess not.
    public void Load()
    {
        Deserialize();
        _loaded = true;
        foreach (var resourceHash in _tag.EntityResources.Select(GetReader(), r => r.Resource))
        {
            EntityResource resource = FileResourcer.Get().GetFile<EntityResource>(resourceHash);
            switch (resource.TagData.Unk10.GetValue(resource.GetReader()))
            {
                case S73868080: // Entity model
                    Model = ((S78868080)resource.TagData.Unk18.GetValue(resource.GetReader())).Model;
                    ModelParentResource = resource;
                    break;

                case S44868080: // Entity physics model
                    PhysicsModel = ((S55868080)resource.TagData.Unk18.GetValue(resource.GetReader())).PhysicsModel;
                    PhysicsModelParentResource = FileResourcer.Get().GetFile<EntityPhysicsModelParent>(resource.Hash);
                    break;

                case SAE9F8080: // Some weird skeleton
                case SB69F8080: // Entity skeleton FK
                    Skeleton = FileResourcer.Get().GetFile<EntitySkeleton>(resource.Hash);
                    break;

                //case S668B8080:  // Entity skeleton IK
                //    ControlRig = FileResourcer.Get().GetFile<EntityControlRig>(resource.Hash);
                //    break;

                //case S97318080: // todo
                //    PatternAudio = resource;
                //    break;

                //case SF62C8080: // todo
                //    PatternAudioUnnamed = resource;
                //    break;

                //case S357C8080: // Generic name
                //    // we care more about the specific name so if the entity name is already assigned, dont assign this one
                //    if (EntityName == null)
                //    {
                //        var genericName = ((S18808080)resource.TagData.Unk18.GetValue(resource.GetReader())).Unk3C0.TagData.EntityName;
                //        if (GlobalStrings.Get().GetString(genericName) != genericName)
                //            EntityName = GlobalStrings.Get().GetString(genericName);
                //    }
                //    break;

                //case SDA5E8080: // Specific name
                //    var specificName = ((SDB5E8080)resource.TagData.Unk18.GetValue(resource.GetReader())).Unk108.TagData.EntityName;

                //    // Don't assign a name if the name hash doesnt return an actual string (returns the name hash instead)
                //    if (GlobalStrings.Get().GetString(specificName) != specificName)
                //        EntityName = GlobalStrings.Get().GetString(specificName);
                //    break;

                //case S9AB68080 when Strategy.IsPreBL(): // No idea if this is SK only
                //    if (EntityChildren2 is null)
                //        EntityChildren2 = new();

                //    EntityChildren2.Add(resource);
                //    break;

                //case S12848080:
                //    EntityChildren = resource;
                //    break;

                default:
                    //Console.WriteLine($"Unk10 {resource.TagData.Unk18.GetValue(resource.GetReader())}");
                    //Console.WriteLine($"Unk18 {resource.TagData.Unk18.GetValue(resource.GetReader())}");
                    // throw new NotImplementedException($"Implement parsing for {resource.Resource._tag.Unk08}");
                    break;
            }
        }
    }

    public List<DynamicMeshPart> Load(ExportDetailLevel detailLevel)
    {
        if (!_loaded)
        {
            Load();
        }
        var dynamicParts = new List<DynamicMeshPart>();
        if (Model != null)
        {
            dynamicParts = dynamicParts.Concat(Model.Load(detailLevel, ModelParentResource, hasSkeleton: Skeleton != null)).ToList();
        }
        if (PhysicsModel != null)
        {
            dynamicParts = dynamicParts.Concat(PhysicsModel.Load(detailLevel, PhysicsModelParentResource, hasSkeleton: Skeleton != null)).ToList();
        }
        return dynamicParts;
    }

    public void SaveMaterialsFromParts(ExporterScene scene, List<DynamicMeshPart> dynamicParts)
    {
        foreach (var dynamicPart in dynamicParts)
        {
            if (dynamicPart.Material == null) continue;
            scene.Materials.Add(new ExportMaterial(dynamicPart.Material));
        }
    }

    public void SaveTexturePlates(string saveDirectory)
    {
        //if (ModelParentResource is null)
        //    return;

        //Directory.CreateDirectory($"{saveDirectory}/Textures/");
        //var parentResource = (S78868080)ModelParentResource.TagData.Unk18.GetValue(ModelParentResource.GetReader());

        //if (parentResource.TexturePlates is null)
        //    return;

        //var rsrc = parentResource.TexturePlates.TagData;
        //rsrc.AlbedoPlate?.SavePlatedTexture($"{saveDirectory}/Textures/{Hash}_albedo");
        //rsrc.NormalPlate?.SavePlatedTexture($"{saveDirectory}/Textures/{Hash}_normal");
        //rsrc.GStackPlate?.SavePlatedTexture($"{saveDirectory}/Textures/{Hash}_gstack");
        //rsrc.DyemapPlate?.SavePlatedTexture($"{saveDirectory}/Textures/{Hash}_dyemap");
    }

    private readonly object _lock = new();
    public bool HasGeometry()
    {
        lock (_lock)
        {
            if (!_loaded)
            {
                Load();
            }
        }
        return Model != null;

        //// saves about 10 seconds when loading dynamics list, but its not really worth it since
        //// the entity will need to be fully loaded after anyways, which adds time
        //foreach (var resourceHash in _tag.EntityResources.Select(GetReader(), r => r.Resource))
        //{
        //    if (resourceHash.ContainsHash(0x80806D8A)) // 8A6D8080
        //        return true;
        //}
        //return false;
    }

    public List<Entity> GetEntityChildren()
    {
        lock (_lock)
        {
            if (!_loaded)
            {
                Load();
            }
        }

        List<Entity> entities = new List<Entity>();
        //if (Strategy.IsPreBL() && EntityChildren2 is not null)
        //    entities.AddRange(GetEntityChildren2());

        if (EntityChildren is null)
            return entities;

        if (EntityChildren.TagData.Unk18.GetValue(EntityChildren.GetReader()) is S0E848080)
        {
            foreach (var entry in ((S0E848080)EntityChildren.TagData.Unk18.GetValue(EntityChildren.GetReader())).Unk88)
            {
                foreach (var entry2 in entry.Unk08)
                {
                    if (entry2.Entity is null)
                        continue;

                    Entity entity = FileResourcer.Get().GetFile<Entity>(entry2.Entity.Hash);
                    if (entity.HasGeometry())
                    {
                        entities.Add(entity);
                        //Just in case
                        foreach (var child in entity.GetEntityChildren())
                            entities.Add(child);
                    }
                }
            }
        }


        return entities;
    }

    public List<Entity> GetEntityChildren2() // THIS SUCKS WHYYY BUNGIEEE
    {
        List<Entity> entities = new List<Entity>();
        if (EntityChildren2 is null)
            return entities;

        foreach (var resource in EntityChildren2)
        {
            if (resource.TagData.Unk18.GetValue(resource.GetReader()) is S519F8080)
            {
                foreach (var entry in ((S519F8080)resource.TagData.Unk18.GetValue(resource.GetReader())).Array2)
                {
                    if (entry.Unk10.GetValue(resource.GetReader()) is S81888080 entry2)
                    {
                        if (entry2.Entity is null)
                            continue;
#if DEBUG
                        Console.WriteLine($"GetEntityChildren2: {resource.Hash} : {entry2.Entity.Hash}");
#endif
                        Entity entity = FileResourcer.Get().GetFile<Entity>(entry2.Entity.Hash);
                        if (!entities.Contains(entity) && entity.HasGeometry())
                        {
                            entities.Add(entity);
                            //Just in case
                            foreach (var child in entity.GetEntityChildren())
                                entities.Add(child);
                        }
                    }
                }
            }
        }
        return entities;
    }
}
