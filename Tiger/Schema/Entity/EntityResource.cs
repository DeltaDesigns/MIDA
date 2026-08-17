namespace Tiger.Schema.Entity;

public class EntityComponent : Tag<S8080BADB>
{
    public EntityComponent(FileHash hash) : base(hash)
    {
    }

    public TigerReader Reader => GetReader();
    public dynamic GetUnk10(bool deserialize = true)
    {
        return _tag.Unk10.GetValue(Reader, deserialize);
    }
    public dynamic GetUnk10(TigerReader reader, bool deserialize = true)
    {
        return _tag.Unk10.GetValue(reader, deserialize);
    }

    public dynamic GetUnk18(bool deserialize = true)
    {
        return _tag.Unk18.GetValue(Reader, deserialize);
    }
    public dynamic GetUnk18(TigerReader reader, bool deserialize = true)
    {
        return _tag.Unk18.GetValue(reader, deserialize);
    }
}
