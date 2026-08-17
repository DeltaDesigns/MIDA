using Tiger.Schema.Audio;

namespace Tiger.Schema.Entity;

public class EntitySequencer : EntityComponent
{
    public EntitySequencer(FileHash resource) : base(resource)
    {
    }

    // todo, figure out where/how else this is used
    //public List<Entity> GetSequencerEntities()
    //{
    //    List<Entity> entities = new();
    //    if (GetUnk18() is S79818080 sequencer)
    //    {
    //        foreach (SF1918080 entry in sequencer.Array2)
    //        {
    //            if (entry.Unk10.GetValue(Reader) is S81888080 entry2)
    //            {
    //                if (entry2.Entity is null)
    //                    continue;

    //                Entity entity = FileResourcer.Get().GetFile<Entity>(entry2.Entity.Hash);
    //                if (!entities.Contains(entity) && entity.HasGeometry())
    //                {
    //                    entities.Add(entity);
    //                    //Just in case
    //                    foreach (Entity child in entity.GetEntityChildren())
    //                        entities.Add(child);
    //                }
    //            }
    //        }
    //    }

    //    return entities;
    //}
}

[NonSchemaStruct(TigerStrategy.MARATHON, 0x24)]
public struct SSequenceNodeBase
{
    [SchemaField(0x0, TigerStrategy.MARATHON)]
    public TigerHash Name;
    public short Unk04;
    public short ParentIndex;

    [SchemaField(0x10)]
    public float StartTime;

    [SchemaField(0x18)]
    public float Duration;
}

[SchemaStruct(TigerStrategy.MARATHON, "40668080", 0x6C)]
public struct SSequenceAudioEvent
{
    public DynamicStruct<SSequenceNodeBase> Base;

    [SchemaField(0x28, TigerStrategy.MARATHON, Tag64 = true)]
    public WwiseSound Sound;
}

