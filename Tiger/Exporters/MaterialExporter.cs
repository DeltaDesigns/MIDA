using System.Collections.Concurrent;
using Tiger.Schema;

namespace Tiger.Exporters;

// TODO: Clean this up
public class MaterialExporter : AbstractExporter
{
    public override void Export(Exporter.ExportEventArgs args)
    {
        var _config = ConfigSubsystem.Get();

        var textures = new ConcurrentBag<(Texture, string)>();
        var materials = new ConcurrentBag<(ExportMaterial, string)>();

        Parallel.ForEach(args.Scenes, scene =>
        {
            string textureSaveDirectory;
            string shaderSaveDirectory;

            if (scene.DataType is DataExportType.Individual)
            {
                textureSaveDirectory = args.AggregateOutput ? args.OutputDirectory : Path.Join(args.OutputDirectory, scene.Name);
                shaderSaveDirectory = textureSaveDirectory;
            }
            else
            {
                textureSaveDirectory = args.AggregateOutput ? args.OutputDirectory : Path.Join(args.OutputDirectory, $"Maps");
                if (_config.GetSingleFolderMapAssetsEnabled())
                    textureSaveDirectory = $"{_config.GetExportSavePath()}/Maps/Assets/";
                shaderSaveDirectory = textureSaveDirectory;
            }

            textureSaveDirectory = Path.Combine(textureSaveDirectory, "Textures");

            Directory.CreateDirectory(textureSaveDirectory);
            Directory.CreateDirectory(shaderSaveDirectory);

            foreach (Texture texture in scene.ExternalTextures.Distinct())
            {
                if (texture is null) continue;
                string filePath = Path.Combine(textureSaveDirectory, texture.Hash);
                textures.Add((texture, filePath));
            }

            foreach (ExportMaterial material in scene.Materials.Distinct())
            {
                string filePath = shaderSaveDirectory;
                materials.Add((material, filePath));
            }
        });

        foreach ((Texture texture, string path) in textures)
        {
            texture.SavetoFile(path);
        }

        foreach ((ExportMaterial material, string path) in materials)
        {
            material.Material.Export(path);
        }
    }
}
