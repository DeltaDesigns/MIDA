using System.Diagnostics;
using Tiger.Exporters;

namespace Tiger.Schema.Entity;

public class Entity : Tag<SEntity>
{
    public TfxFeatureRenderer FeatureType = TfxFeatureRenderer.DynamicObjects;
    // Entity features, todo clean this up
    public EntitySkeleton? Skeleton { get; set; }
    public EntityControlRig? ControlRig { get; private set; }
    public EntityModelParent? ModelParent { get; private set; }
    public EntityPhysicsModelParent? PhysicsModelParent { get; private set; }
    public EntityComponent? PatternAudio { get; private set; }
    public EntityComponent? PatternAudioUnnamed { get; private set; }
    public EntityAttachmentInfo? AttachmentInfo { get; private set; }
    public EntityComponent? EntityChildren { get; private set; }
    public EntityComponent? ObjectChannels { get; private set; }
    public List<EntitySequencer>? Sequences { get; private set; } // The Sequencer (tm)

    public EntityModel? Model => ModelParent?.GetModel();
    public EntityModel? PhysicsModel => PhysicsModelParent?.GetModel();

    public string? EntityName { get; set; }
    public DestinyGenderDefinition Gender { get; set; } = DestinyGenderDefinition.None; // obsolete

    public MarathonItemType ItemType = MarathonItemType.Default; // Currently used for investment purposes
    public MarathonAttachmentType? AttachmentType;
    public TigerHash AttachmentID = null;

    public IEnumerable<FileHash> Components => _tag.EntityComponents.Select(GetReader(), r => r.Component);

    private bool _loaded = false;
    public Entity(FileHash hash, bool shouldParse = true) : base(hash, shouldParse)
    {
        if (shouldParse)
            Load();
    }

    public void Load()
    {
        Deserialize();
        _loaded = true;
        foreach (var resourceHash in Components)
        {
            EntityComponent resource = FileResourcer.Get().GetFile<EntityComponent>(resourceHash);
            switch (resource.TagData.Unk10.GetValue(resource.GetReader()))
            {
                case S80808673: // Entity model
                    ModelParent = FileResourcer.Get().GetFile<EntityModelParent>(resource.Hash);
                    break;

                case S80808644: // Entity physics model
                    PhysicsModelParent = FileResourcer.Get().GetFile<EntityPhysicsModelParent>(resource.Hash);
                    break;

                case S80809FAE: // Some weird skeleton
                case S80809FB6: // Entity skeleton FK
                    Skeleton = FileResourcer.Get().GetFile<EntitySkeleton>(resource.Hash);
                    break;

                //case S85AD8080:
                case S80809F75:
                    AttachmentInfo = FileResourcer.Get().GetFile<EntityAttachmentInfo>(resource.Hash);
                    break;

                case S808032C2:
                    var unk18 = (S808032C4)resource.GetUnk18();
                    var attachmentType = (MarathonAttachmentType)unk18.AttachmentType.Hash32;

                    if (Enum.IsDefined(attachmentType))
                        AttachmentType = attachmentType;
                    else if (unk18.AttachmentType != StringHash.InvalidHash32)
                        Debug.Assert(false, $"Unknown attachment type {attachmentType} : Resource {resourceHash}");

                    if (unk18.Unk120.GetValue(resource.Reader) is S808032C9 unk)
                        AttachmentID = unk.Unk00;
                    break;

                //case S668B8080:  // Entity skeleton IK
                //    ControlRig = FileResourcer.Get().GetFile<EntityControlRig>(resource.Hash);
                //    break;

                case S80804603:
                    PatternAudio = resource;
                    break;

                case S808040D6:
                    PatternAudioUnnamed = resource;
                    break;

                case S8080B69A: // sequencer
                    if (Sequences is null)
                        Sequences = new();

                    Sequences.Add(new(resource.Hash));
                    break;

                case S8080A317:
                    EntityChildren = resource;
                    break;

                case S8080AF8E:
                    ObjectChannels = resource;
                    break;

                case S808035B4: // Loot container name
                    var S808035B7 = (S808035B7)resource.GetUnk18();
                    if (S808035B7.Container is not null && S808035B7.Name.IsValid())
                        EntityName = S808035B7.Container.GetStringFromHash(S808035B7.Name);
                    break;

                default:
                    //Console.WriteLine($"Unk10 {resource.TagData.Unk18.GetValue(resource.GetReader())}");
                    //Console.WriteLine($"Unk18 {resource.TagData.Unk18.GetValue(resource.GetReader())}");
                    // throw new NotImplementedException($"Implement parsing for {resource.Resource._tag.Unk08}");
                    break;
            }
        }
    }

    /// <summary>
    /// Loads both the normal model and physics model into dynamic mesh parts
    /// </summary>
    /// <param name="detailLevel"></param>
    /// <param name="loadLevel"></param>
    /// <returns></returns>
    public List<DynamicMeshPart> Load(ExportDetailLevel detailLevel)
    {
        if (!_loaded)
            Load();

        var dynamicParts = new List<DynamicMeshPart>();
        if (Model != null)
            dynamicParts = dynamicParts.Concat(Model.Load(detailLevel, ModelParent, hasSkeleton: Skeleton != null)).ToList();

        if (PhysicsModel != null)
            dynamicParts = dynamicParts.Concat(PhysicsModel.Load(detailLevel, PhysicsModelParent, hasSkeleton: Skeleton != null)).ToList();

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
                Load();
        }

        return ModelParent != null;
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

        if (EntityChildren is null)
            return entities;

        if (EntityChildren.TagData.Unk18.GetValue(EntityChildren.GetReader()) is S8080A313 a)
        {
            foreach (var entry in a.Unk88)
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
}
