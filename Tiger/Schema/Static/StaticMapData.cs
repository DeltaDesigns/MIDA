using Tiger.Exporters;
using Tiger.Schema.Model;

namespace Tiger.Schema;

public class Map : Tag<SMapContainer>
{
    public Map(FileHash fileHash) : base(fileHash)
    {
    }
}

public class StaticMapData : Tag<SStaticMapData>
{
    public StaticMapData(FileHash hash) : base(hash)
    {
    }

    public void LoadIntoExporterScene(ExporterScene scene)
    {
        var chunks = PreChunkStaticsAndInstances(650, scene.Name);
        //Parallel.ForEach(chunks, chunk => // some things get skipped when doing Parallel, for some reason..
        chunks.ForEach(chunk =>
        {
            var newScene = Exporter.Get().CreateScene(chunk.SceneName, ExportType.Statics, DataExportType.Map);

            for (int i = 0; i < chunk.Statics.Count; i++)
            {
                var model = chunk.Statics[i];
                var instances = chunk.Instances[i];
                newScene.AddStaticInstancesToMesh(model, instances);
            }
        });
    }

    public List<PreChunkedStaticGroup> PreChunkStaticsAndInstances(int maxStaticsPerChunk, string baseSceneName)
    {
        var result = new List<PreChunkedStaticGroup>();

        var currentGroup = new PreChunkedStaticGroup
        {
            SceneName = $"{baseSceneName}_0"
        };

        int staticCounter = 0;
        int sceneCounter = 0;

        foreach (var c in _tag.InstanceCounts)
        {
            if (staticCounter >= maxStaticsPerChunk)
            {
                result.Add(currentGroup);
                sceneCounter++;
                currentGroup = new PreChunkedStaticGroup
                {
                    SceneName = $"{baseSceneName}_{sceneCounter}"
                };
                staticCounter = 0;
            }

            // Get the model and corresponding instances
            var model = _tag.Statics[c.StaticIndex].Static;
            var instances = _tag.Instances.Skip(c.InstanceOffset).Take(c.InstanceCount).ToList();

            // Add model and its instances together in the chunk
            currentGroup.Statics.Add(model);
            currentGroup.Instances.Add(instances);

            staticCounter++;
        }

        // Add the last group if there are any statics left
        if (currentGroup.Statics.Count > 0)
        {
            result.Add(currentGroup);
        }

        return result;
    }

    public class PreChunkedStaticGroup
    {
        public string SceneName { get; set; }
        public List<StaticMesh> Statics { get; } = new();
        public List<List<SStaticMeshInstanceTransform>> Instances { get; } = new();
    }
}

[SchemaStruct(TigerStrategy.MARATHON, "8080A7F1", 0xC0)]
public struct SStaticMapData
{
    public long FileSize;

    [SchemaField(0x18, TigerStrategy.MARATHON)]
    public Tag<SOcclusionBounds> ModelOcclusionBounds;

    [SchemaField(0x40, TigerStrategy.MARATHON)]
    public DynamicArray<SStaticMeshInstanceTransform> Instances;

    [SchemaField(TigerStrategy.MARATHON)]
    public DynamicArray<SUInt32> Unk50;

    [SchemaField(0x78, TigerStrategy.MARATHON)]
    public DynamicArray<SStaticMeshHash> Statics;

    [SchemaField(0x88, TigerStrategy.MARATHON)]
    public DynamicArray<SStaticMeshInstanceMap> InstanceCounts;
}


[SchemaStruct(TigerStrategy.MARATHON, "8080A7F5", 0x18)]
public struct SOcclusionBounds
{
    public long FileSize;
    public DynamicArrayUnloaded<SBounds> Bounds;
}

[SchemaStruct(TigerStrategy.MARATHON, "F7A78080", 0x30)]
public struct SBounds
{
    public Vector4 Corner1;
    public Vector4 Corner2;
    public TigerHash Unk20;
    public TigerHash Unk24;
}

[SchemaStruct(TigerStrategy.MARATHON, "2F868080", 0x60)]
public struct SStaticMeshInstanceTransform
{
    public Vector4 Rotation;
    public Vector3 Position;
    public Vector3 Scale;  // Only X is used as a global scale
}

[SchemaStruct(TigerStrategy.MARATHON, "02A88080", 0x4)]
public struct SStaticMeshHash
{
    public StaticMesh Static;
}

[SchemaStruct(TigerStrategy.MARATHON, "18868080", 0x10)]
public struct SStaticMeshInstanceMap
{
    public int InstanceOffset;
    public int InstanceCount;
    public int StaticIndex;
    public int Unk06;
}

#region Parent/other structures for maps


/// <summary>
/// The very top reference for all map-related things.
/// </summary>
[SchemaStruct(TigerStrategy.MARATHON, "8080AB27", 0x6C)]
public struct SBubbleParent
{
    public long FileSize;
    public Tag<SBubbleDefinition> ChildMapReference;

    [SchemaField(0x18, TigerStrategy.MARATHON)]
    public StringHash MapName;
    public int Unk1C;

    [SchemaField(0x40, TigerStrategy.MARATHON)]
    public DynamicArray<SB2B08080> Unk40;

    [SchemaField(TigerStrategy.MARATHON, Tag64 = true)]
    public Tag Unk50;  // some kind of parent thing, very strange weird idk

}

/// <summary>
/// Basically same table as in the child tag, but in a weird format. Never understood what its for.
/// </summary>
[SchemaStruct(TigerStrategy.MARATHON, "B2B08080", 0x10)]
public struct SB2B08080
{
    [SchemaField(Tag64 = true)]
    public Tag Unk00;
}

/// <summary>
/// The one below the top reference, actually contains useful information.
/// First of MapResources is what I call "ambient entities", second is always the static map.
/// </summary>
[SchemaStruct(TigerStrategy.MARATHON, "8080A7BB", 0x18)]
public struct SBubbleDefinition
{
    public long FileSize;
    public DynamicArray<SMapContainerEntry> MapResources;
}

[SchemaStruct(TigerStrategy.MARATHON, "BDA78080", 0x10)]
public struct SMapContainerEntry
{
    [SchemaField(TigerStrategy.MARATHON, Tag64 = true)]
    public Tag<SMapContainer> MapContainer;
}

/// <summary>
/// A map resource, contains data used to make a map.
/// This is quite similar to EntityComponent, but with more children.
/// </summary>
[SchemaStruct(TigerStrategy.MARATHON, "8080A7C1", 0x38)]
public struct SMapContainer
{
    public long FileSize;
    public long Unk08;

    [SchemaField(0x28, TigerStrategy.MARATHON)]
    public DynamicArray<SMapDataTableEntry> MapDataTables;
}

[SchemaStruct(TigerStrategy.MARATHON, "8080A7C3", 4)]
public struct SMapDataTableEntry
{
    public Tag<SMapDataTable> MapDataTable;
}

/// <summary>
/// A map data table, containing data entries.
/// </summary>
[SchemaStruct(TigerStrategy.MARATHON, "8080B1A7", 0x18)]
public struct SMapDataTable
{
    public long FileSize;
    public DynamicArray<SMapDataEntry> DataEntries;
}


/// <summary>
/// A data entry. Can be static maps, entities, etc. with a defined world transform.
/// </summary>
[SchemaStruct(TigerStrategy.MARATHON, "8080B3F5", 0x90)]
public struct SMapDataEntry
{
    [SchemaField(0, TigerStrategy.MARATHON)]
    public MapTransform Transfrom;

    [SchemaField(0x28, TigerStrategy.MARATHON, Tag64 = true), NoLoad]
    public Entity.Entity Entity;

    [SchemaField(0x68)]
    public uint Unk68;

    [SchemaField(0x70, TigerStrategy.MARATHON)]
    public ulong WorldID;

    [SchemaField(0x78, TigerStrategy.MARATHON)]
    public ResourcePointer DataResource;
}

/// <summary>
/// Data resource containing a static map.
/// </summary>
[SchemaStruct(TigerStrategy.MARATHON, "808085B0", 0x18)]
public struct SMapDataResource
{
    [SchemaField(0x8, TigerStrategy.MARATHON)]
    public TigerHash Unk08;

    [SchemaField(0x10, TigerStrategy.MARATHON), NoLoad]
    public Tag<SStaticMapParent> StaticMapParent;
}

[SchemaStruct(TigerStrategy.MARATHON, "808082D5", 0x30)]
public struct SStaticMapParent
{
    [SchemaField(0x8)]
    public StaticMapData StaticMap;
}

/// <summary>
/// Unk data resource.
/// </summary>
[SchemaStruct(TigerStrategy.MARATHON, "80808691", 0x80)]
public struct S80808691
{
    public ulong FileSize;
    [SchemaField(0x30)]
    public DynamicArray<SUInt8> Bytecode;
    public DynamicArray<Vec4> Buffer1; // bytecode constants?
    [SchemaField(0x60)]
    public DynamicArray<Vec4> Buffer2;
}

[SchemaStruct(TigerStrategy.MARATHON, "B3938080", 0x30)]
public struct SB3938080
{
    //Bounds
    public Vector4 Unk00;
    public Vector4 Unk10;
}

// /// <summary>
// /// entity data resource?
// /// </summary>
[SchemaStruct(TigerStrategy.MARATHON, "839D8080", 0x50)]
public struct S839D8080
{
    [SchemaField(0x24)]
    public StringHash EntityName;
}

[SchemaStruct(TigerStrategy.MARATHON, "A0848080", 0x130)]
public struct SMapAtmosphere
{
    // 0 and 1 used in...
    // sky_lookup_generate_near/far, result used in 'Sky' and set to T11 and T13 (transparent scope)
    // full_hemisphere_sky_color_generate,
    // hemisphere_sky_color_generate,
    // water_sky_color_generate,
    [SchemaField(0x90, TigerStrategy.MARATHON, Tag64 = true)]
    public Texture Lookup0;

    [SchemaField(TigerStrategy.MARATHON, Tag64 = true)]
    public Texture Lookup1;

    [SchemaField(TigerStrategy.MARATHON, Tag64 = true)]
    public Texture Lookup2;

    [SchemaField(TigerStrategy.MARATHON, Tag64 = true)]
    public Texture Lookup3;

    [SchemaField(0xD0, TigerStrategy.MARATHON)]
    public Texture Lookup4; // used in atmo_depth_angle_density_lookup_generate, result set to T15 (transparent scope)

    public FileHash UnkD4; // Some weird RGBA byte texture that looks like when Lookup4 is sampled in atmo_depth_angle_density_lookup_generate

    public Vector4 UnkD8;
    public Vector4 UnkE8;
    public Vector4 UnkF8;
    public Vector4 Unk108;
}

[SchemaStruct(TigerStrategy.MARATHON, "716A8080", 0x28)]
public struct S716A8080
{
    [SchemaField(0x10, TigerStrategy.MARATHON)]
    public Tag<S746A8080> Unk10;
    public float Unk14; // always 3600? (one hour as seconds)
    public float Unk18; // some kind of multiplier maybe?
    public FileHash Unk1C; // Lens dirt or something
    public FileHash Unk20; // Lens dirt or something
}

[SchemaStruct(TigerStrategy.MARATHON, "746A8080", 0x20)]
public struct S746A8080
{
    [SchemaField(0x0, TigerStrategy.MARATHON)]
    public Vector4 Unk00;

    [SchemaField(0x10, TigerStrategy.MARATHON)]
    public Tag<SC88A8080> Unk10;
    public Tag<SC88A8080> Unk14;
    public Tag<SC88A8080> Unk18;
    public Tag<SC88A8080> Unk1C;
}

[SchemaStruct(TigerStrategy.MARATHON, "C88A8080", 0x48)]
public struct SC88A8080
{
    [SchemaField(0x8)]
    public int Unk08; // always 1800? (1/2 hour as seconds)
    public float Unk0C; // always 108000?

    // Theres actually a relative pointer here but its always(?) 498B8080 so it doesnt matter

    [SchemaField(0x30)]
    public DynamicArrayUnloaded<Vec4> Unk30; // Global Channel 102? Some type of sun/light rotation
}

[SchemaStruct(TigerStrategy.MARATHON, "406A8080", 0x18)]
public struct SStaticAOResource
{
    [SchemaField(0x10)]
    public FileHash MapAO;
}

[SchemaStruct(TigerStrategy.MARATHON, "196D8080", 0x78)]
public struct SStaticAmbientOcclusion
{
    [SchemaField(0x8)]
    public DynamicStruct<SAmbientOcclusionBuffer> AO_1;
    public DynamicStruct<SAmbientOcclusionBuffer> AO_2;
    public DynamicStruct<SAmbientOcclusionBuffer> AO_3;
}

[NonSchemaStruct(TigerStrategy.MARATHON, 0x18)]
public struct SAmbientOcclusionBuffer
{
    public VertexBuffer Buffer;
    [SchemaField(0x8)]
    public DynamicArray<SStaticAmbientOcclusionMappings> Mappings;
}

[SchemaStruct(TigerStrategy.MARATHON, "216D8080", 0x20)]
public struct SStaticAmbientOcclusionMappings
{
    public ulong Identifier;
    public uint Offset;
}

// /// <summary>
// /// Audio data resource.
// /// </summary>
// [SchemaStruct("6F668080", 0x30)]
// public struct S6F668080
// {
//     [SchemaField(0x10), DestinyField(FieldType.TagHash64)]
//     public Tag AudioContainer;  // 38978080 audio container
// }
//
// /// <summary>
// /// Spatial audio data resource, contains a list of positions to play an audio container.
// /// </summary>
// [SchemaStruct("6D668080", 0x48)]
// public struct S6D668080
// {
//     [SchemaField(0x10), DestinyField(FieldType.TagHash64)]
//     public Tag AudioContainer;  // 38978080 audio container
//     [SchemaField(0x30), DestinyField(FieldType.TablePointer)]
//     public List<S94008080> AudioPositions;
//     public float Unk40;
// }
//
// [SchemaStruct("94008080", 0x10)]
// public struct S94008080
// {
//     public Vector4 Translation;
// }
//
// /// <summary>
// /// Unk data resource.
// /// </summary>
// [SchemaStruct("B58C8080", 0x18)]
// public struct SB58C8080
// {
//     [SchemaField(0x10), DestinyField(FieldType.FileHash)]
//     public Tag Unk10;  // B78C8080
// }
//
// /// <summary>
// /// Unk data resource.
// /// </summary>
// [SchemaStruct("55698080", 0x18)]
// public struct S55698080
// {
//     [SchemaField(0x10), DestinyField(FieldType.FileHash)]
//     public Tag Unk10;  // 5B698080, lights/volumes/smth maybe cubemaps idk
// }
//
// /// <summary>
// /// Unk data resource.
// /// </summary>
// [SchemaStruct("7B918080", 0x18)]
// public struct S7B918080
// {
//     [DestinyField(FieldType.RelativePointer)]
//     public dynamic? Unk00;
// }
//
// /// <summary>
// /// Havok volume data resource.
// /// </summary>
// [SchemaStruct("21918080", 0x20)]
// public struct S21918080
// {
//     [SchemaField(0x10), DestinyField(FieldType.FileHash)]
//     public Tag HavokVolume;  // type 27 subtype 0
//     public TigerHash Unk14;
// }
//
// /// <summary>
// /// Unk data resource.
// /// </summary>
// [SchemaStruct("C0858080", 0x18)]
// public struct SC0858080
// {
//     [SchemaField(0x10), DestinyField(FieldType.FileHash)]
//     public Tag Unk10;  // C2858080
// }
//
// /// <summary>
// /// Unk data resource.
// /// </summary>
// [SchemaStruct("C26A8080", 0x18)]
// public struct SC26A8080
// {
//     [SchemaField(0x10), DestinyField(FieldType.FileHash)]
//     public Tag Unk10;  // C46A8080
// }
//
//
// /// <summary>
// /// Unk data resource.
// /// </summary>
// [SchemaStruct("222B8080", 0x18)]
// public struct S222B8080
// {
//     [SchemaField(0x10)]
//     public TigerHash Unk10;
// }
//
// /// <summary>
// /// Unk data resource.
// /// </summary>
// [SchemaStruct("04868080", 0x18)]
// public struct S04868080
// {
//     [SchemaField(0x10), DestinyField(FieldType.FileHash)]
//     public Tag Unk10;  // 24878080, smth related to havok volumes
// }


#endregion
