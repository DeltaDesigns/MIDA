using Tiger.Exporters;
using Tiger.Schema.Entity;

namespace Tiger.Schema;

public class WaterDecals
{
    public TfxFeatureRenderer FeatureType = TfxFeatureRenderer.Water;
    public SMapWaterDecal Water;
    public MapTransform Transform;
    public WaterDecals(SMapWaterDecal entry)
    {
        Water = entry;
    }

    public void LoadIntoExporter(ExporterScene scene)
    {
        Transform transform = new Transform
        {
            Position = Transform.Translation.ToVec3(),
            Quaternion = Transform.Rotation,
            Rotation = Vector4.QuaternionToEulerAngles(Transform.Rotation),
            Scale = new(Transform.Translation.W)
        };

        var parts = Water.Model.Load(ExportDetailLevel.MostDetailed, null);

        scene.AddMapModelParts($"{Water.Model.Hash}", parts.Where(x => x.RenderStage != TfxRenderStage.WaterReflection).ToList(), transform);
        scene.AddMapModelParts($"{Water.Model.Hash}_Reflection", parts.Where(x => x.RenderStage == TfxRenderStage.WaterReflection).ToList(), transform);

        foreach (DynamicMeshPart part in parts)
        {
            if (part.Material == null) continue;
            scene.Materials.Add(new ExportMaterial(part.Material));
        }
    }
}

/// <summary>
/// Usually a flat plane for screen-space reflected water
/// </summary>

[SchemaStruct(TigerStrategy.MARATHON, "9C818080", 0x70)]
public struct SMapWaterDecal
{
    [SchemaField(0x10, TigerStrategy.MARATHON)]
    public EntityModel Model;
}
