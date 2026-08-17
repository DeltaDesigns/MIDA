namespace Tiger.Schema.Entity;

public class EntityAttachmentInfo : EntityComponent
{
    public Dictionary<TigerHash, AttachmentTransform> Transforms { get; set; } = new();

    public EntityAttachmentInfo(FileHash resource) : base(resource)
    {
        FillTransforms();
    }

    //public void FillTransforms()
    //{
    //    if (_tag.Unk18.GetValue(GetReader()) is not S7EAD8080 info)
    //        return;

    //    foreach (var entry in info.AttachmentInfo)
    //    {
    //        Transforms.TryAdd(entry.AttachmentName, new MapTransform()
    //        {
    //            Translation = info.AttachmentTransforms[entry.TransformIndex].Translation,
    //            Rotation = info.AttachmentTransforms[entry.TransformIndex].Rotation
    //        });
    //    }
    //}

    public void FillTransforms()
    {
        if (_tag.Unk18.GetValue(GetReader()) is not S80809F76 info)
            return;

        foreach (var entry in info.AttachmentTransforms)
        {
            Transforms.TryAdd(entry.AttachmentName, new AttachmentTransform()
            {
                BoneIndex = entry.BoneIndex,
                Transform = new()
                {
                    Translation = entry.Transform.Translation,
                    Rotation = entry.Transform.Rotation
                }
            });
        }
    }
}

public struct AttachmentTransform
{
    public AttachmentTransform() { }

    public int BoneIndex = -1;
    public MapTransform Transform;
}
