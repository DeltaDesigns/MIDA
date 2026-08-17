namespace Tiger.Schema.Entity;

public class EntityPhysicsModelParent : EntityModelParent
{
    public EntityPhysicsModelParent(FileHash resource) : base(resource)
    {
    }

    public S80808655 Reader => ((S80808655)TagData.Unk18.GetValue(GetReader()));
    public EntityModel GetModel()
    {
        return Reader.PhysicsModel;
    }
}

