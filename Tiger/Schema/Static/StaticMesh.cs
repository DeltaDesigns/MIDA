using Tiger.Exporters;
using Tiger.Schema.Static;

namespace Tiger.Schema;

public class StaticMesh : Tag<SStaticMesh>
{
    public StaticMesh(FileHash hash) : base(hash) { }

    public void SaveMaterialsFromParts(ExporterScene scene, List<StaticPart> parts)
    {
        foreach (var part in parts)
        {
            if (part.Material == null)
            {
                continue;
            }
            scene.Materials.Add(new ExportMaterial(part.Material));
        }
    }

    public List<StaticPart> Load(ExportDetailLevel detailLevel)
    {
        List<StaticPart> decalParts = LoadDecals(detailLevel);
        var mainParts = _tag.StaticData.Load(detailLevel, _tag);
        mainParts.AddRange(decalParts);
        return mainParts;
    }

    public Task<List<StaticPart>> LoadAsync(ExportDetailLevel detailLevel)
    {
        return Task.Run(() => Load(detailLevel));
    }

    private List<StaticPart> LoadDecals(ExportDetailLevel detailLevel)
    {
        List<StaticPart> parts = new List<StaticPart>();
        foreach (var decalPartEntry in _tag.Decals)
        {
            if (!Globals.Get().GetExportStages().Contains((TfxRenderStage)decalPartEntry.GetRenderStage()))
                continue;

            if (detailLevel == ExportDetailLevel.MostDetailed && !decalPartEntry.DetailLevel.IsHighestLevel())
                continue;

            if (detailLevel == ExportDetailLevel.LeastDetailed && decalPartEntry.DetailLevel.IsHighestLevel())
                continue;

            StaticPart part = new StaticPart(decalPartEntry);
            part.GetDecalData(decalPartEntry, _tag);
            if (decalPartEntry.Material is not null)
            {
                part.Material = decalPartEntry.Material;
                part.Material.RenderStage = (TfxRenderStage)decalPartEntry.GetRenderStage();
            }
            parts.Add(part);
        }

        return parts;
    }
}
