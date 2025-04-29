using Tiger.Exporters;
using Tiger.Schema.Shaders;

namespace Tiger.Schema;

public class LensFlare : Tag<SLensFlare>
{
    public MapTransform Transform { get; set; }
    public List<FileHash> Materials { get; set; }
    public TfxFeatureRenderer FeatureType = TfxFeatureRenderer.LensFlares;

    public LensFlare(FileHash hash) : base(hash)
    {
    }

    public void LoadIntoExporter(ExporterScene scene) // Not ideal
    {
        Exporter.Get().GetGlobalScene().AddToGlobalScene(this);
        Materials = new();
        using TigerReader reader = GetReader();
        for (int i = 0; i < _tag.Entries.Count; i++)
        {
            var entry = _tag.Entries.ElementAt(reader, i);
            if (entry.Material == null) continue;
            entry.Material.RenderStage = TfxRenderStage.LensFlares;
            scene.Materials.Add(new ExportMaterial(entry.Material));
            Materials.Add(entry.Material.Hash);
        }
    }
}

/// <summary>
/// Light Lens Flares
/// </summary>
[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "B5678080", 0x1C)]
public struct SMapLensFlareResource
{
    [SchemaField(0x10)]
    public LensFlare LensFlare; // S786A8080
}

/// <summary>
/// Unk data resource.
/// </summary>
[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "786A8080", 0x38)]
public struct SLensFlare
{
    public ulong FileSize;
    [SchemaField(0x18)]
    public Tag<S80808691> Unk18;
    [SchemaField(0x20)]
    public DynamicArrayUnloaded<SLensFlareEntry> Entries;
    public TigerHash Unk30;
}

/// <summary>
/// Unk data resource.
/// </summary>
[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "7D6A8080", 0xC)]
public struct SLensFlareEntry
{
    public Material Material;
    public Tag<S80808691> Unk04;
    public int Unk08;
}
