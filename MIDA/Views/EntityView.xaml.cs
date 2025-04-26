using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Arithmic;
using Tiger;
using Tiger.Exporters;
using Tiger.Schema;
using Tiger.Schema.Entity;
using Tiger.Schema.Investment;

namespace MIDA;

public partial class EntityView : UserControl
{
    public FileHash Hash;
    private string Name;

    public EntityView()
    {
        InitializeComponent();
    }

    private MainViewModel MVM;
    private FileHash currentHash;

    public bool LoadEntity(FileHash entityHash)
    {
        currentHash = entityHash;
        SetupCheckboxHandlers();

        Entity entity = FileResourcer.Get().GetFile<Entity>(entityHash);

        List<Entity> entities = new List<Entity> { entity };
        entities.AddRange(entity.GetEntityChildren());

        if (MVM is null)
            MVM = (MainViewModel)ModelView.UCModelView.Resources["MVM"];

        MVM.Clear();
        var displayParts = MakeEntityDisplayParts(entities, ModelView.GetSelectedLod());
        MVM.SetChildren(displayParts);
        MVM.Title = entityHash;
        MVM.SubTitle = $"{displayParts.Sum(p => p.BasePart.Indices.Count)} triangles";

        return true;
    }

    public bool LoadEntityModel(FileHash entityModelHash)
    {
        currentHash = entityModelHash;
        SetupCheckboxHandlers();

        EntityModel entityModel = FileResourcer.Get().GetFile<EntityModel>(entityModelHash);

        if (MVM is null)
            MVM = (MainViewModel)ModelView.UCModelView.Resources["MVM"];

        MVM.Clear();
        var displayParts = MakeEntityModelDisplayParts(entityModel, ModelView.GetSelectedLod());
        MVM.SetChildren(displayParts);
        MVM.Title = entityModelHash;
        MVM.SubTitle = $"{displayParts.Sum(p => p.BasePart.Indices.Count)} triangles";

        return true;
    }

    public static void Export(List<Entity> entities, string name, ExportTypeFlag exportType, EntitySkeleton overrideSkeleton = null, ExporterScene scene = null)
    {
        ConfigSubsystem config = ConfigSubsystem.Get();
        name = Helpers.SanitizeString(name);
        string savePath = config.GetExportSavePath() + $"/{name}";

        if (scene == null)
            scene = Tiger.Exporters.Exporter.Get().CreateScene(name, ExportType.Entities);

        Log.Verbose($"Exporting entity: {name}");

        foreach (var entity in entities)
        {
            if (entity.Skeleton == null && overrideSkeleton != null)
                entity.Skeleton = overrideSkeleton;

            var dynamicParts = entity.Load(ExportDetailLevel.MostDetailed);

            // TODO TEMP, add player skeleton for anything with no skeleton but has weights
            if (entity.Skeleton == null && overrideSkeleton == null && dynamicParts.Any(x => x.VertexWeights.Any()))
            {
                string skeleHash = "00008E9C51120FCF";
                Entity skele = FileResourcer.Get().GetFile<Entity>(new FileHash(Hash64Map.Get().GetHash32Checked(skeleHash))); // 64 bit more permanent
                overrideSkeleton = new EntitySkeleton(skele.Skeleton.Hash);
            }
            //--------

            List<BoneNode> boneNodes = overrideSkeleton != null ? overrideSkeleton.GetBoneNodes() : new List<BoneNode>();
            if (entity.Skeleton != null && overrideSkeleton == null)
            {
                boneNodes = entity.Skeleton.GetBoneNodes();
            }
            scene.AddEntity(entity.Hash, dynamicParts, boneNodes, entity.Gender);
            if (exportType == ExportTypeFlag.Full)
            {
                entity.SaveMaterialsFromParts(scene, dynamicParts);
                entity.SaveTexturePlates(savePath);
            }
        }

        if (exportType == ExportTypeFlag.Full)
        {
            if (config.GetUnrealInteropEnabled())
            {
                AutomatedExporter.SaveInteropUnrealPythonFile(savePath, name, AutomatedExporter.ImportType.Entity, config.GetOutputTextureFormat());
            }
        }

        // Scale and rotate
        // fbxHandler.ScaleAndRotateForBlender(boneNodes[0]);
        Tiger.Exporters.Exporter.Get().Export();
        Log.Info($"Exported entity {name} to {savePath.Replace('\\', '/')}/");
    }

    public static void ExportInventoryItem(ApiItem item, string savePath, bool aggregateOutput = false)
    {
        ConfigSubsystem config = ConfigSubsystem.Get();
        string name = Helpers.SanitizeString(item.ItemName);
        if (!aggregateOutput)
            savePath = config.GetExportSavePath() + $"/{name}";

        Directory.CreateDirectory(savePath);
        Directory.CreateDirectory($"{savePath}/Textures");
        var scene = Tiger.Exporters.Exporter.Get().CreateScene(name, ExportType.API);

        Log.Info($"Exporting entity: {name}");

        ExportGearShader(item, name, savePath);

        // Export the model
        // todo bad, should be replaced
        EntitySkeleton overrideSkeleton = null;

        string skeleHash = "00008E9C51120FCF";
        Entity skele = FileResourcer.Get().GetFile<Entity>(new FileHash(Hash64Map.Get().GetHash32Checked(skeleHash))); // 64 bit more permanent
        overrideSkeleton = new EntitySkeleton(skele.Skeleton.Hash);

        var val = Investment.Get().GetPatternEntityFromHash(item.Parent != null ? item.Parent.TagData.InventoryItemHash : item.Item.TagData.InventoryItemHash);
        if (val != null && val.Skeleton != null)
        {
            overrideSkeleton = val.Skeleton;
        }

        var entities = Investment.Get().GetEntitiesFromPattern(val);
        //var entities2 = Investment.Get().GetEntitiesFromHash(item.Item.TagData.InventoryItemHash);

        entities.Add(Investment.Get().GetEntityFromCosmeticMap(item.Item));

        foreach (var entity in entities)
        {
            if (entity is null)
                continue;

            if (entity.Skeleton == null && overrideSkeleton != null)
                entity.Skeleton = overrideSkeleton;

            var dynamicParts = entity.Load(ExportDetailLevel.MostDetailed);
            List<BoneNode> boneNodes = overrideSkeleton != null ? overrideSkeleton.GetBoneNodes() : new List<BoneNode>();
            if (entity.Skeleton != null)// && overrideSkeleton == null)
            {
                boneNodes = entity.Skeleton.GetBoneNodes();
            }
            scene.AddEntity(entity.Hash, dynamicParts, boneNodes, entity.Gender);
            entity.SaveMaterialsFromParts(scene, dynamicParts);
            entity.SaveTexturePlates(savePath);
        }


        if (!aggregateOutput)
            Tiger.Exporters.Exporter.Get().Export();
        else
            Tiger.Exporters.Exporter.Get().Export(savePath);

        Log.Info($"Exported entity {name} to {savePath.Replace('\\', '/')}/");
    }

    // I don't like this
    public static void ExportGearShader(ApiItem item, string itemName, string savePath) // TODO
    {
        //        var config = ConfigSubsystem.Get();

        //        Log.Info($"Exporting Gear Shader for: {item.ItemName}");

        //        Dictionary<TigerHash, Dye> dyes = new Dictionary<TigerHash, Dye>();
        //        if (item.Item.TagData.Unk90.GetValue(item.Item.GetReader()) is S77738080 translationBlock)
        //        {
        //            foreach (var dyeEntry in translationBlock.DefaultDyes)
        //            {
        //                Dye dye = Investment.Get().GetDyeFromIndex(dyeEntry.DyeIndex);
        //                dyes.Add(Investment.Get().GetChannelHashFromIndex(dyeEntry.ChannelIndex), dye);
        //#if DEBUG
        //                System.Console.WriteLine($"{item.ItemName}: DefaultDye {dye.Hash}");
        //#endif
        //            }
        //            foreach (var dyeEntry in translationBlock.LockedDyes)
        //            {
        //                Dye dye = Investment.Get().GetDyeFromIndex(dyeEntry.DyeIndex);
        //                dyes.Add(Investment.Get().GetChannelHashFromIndex(dyeEntry.ChannelIndex), dye);
        //#if DEBUG
        //                System.Console.WriteLine($"{item.ItemName}: LockedDye {dye.Hash}");
        //#endif
        //            }
        //        }

        //        AutomatedExporter.SaveBlenderApiFile(savePath, itemName,
        //            config.GetOutputTextureFormat(), dyes.Values.ToList());

        //        var iridesceneLookup = Globals.Get().RenderGlobals.TagData.Textures.TagData.IridescenceLookup;
        //        TextureExtractor.SaveTextureToFile($"{savePath}/Textures/Iridescence_Lookup", iridesceneLookup.GetScratchImage());

        //        Log.Info($"Exported Gear Shader for: {item.ItemName}");
    }

    private List<MainViewModel.DisplayPart> MakeEntityDisplayParts(List<Entity> entities, ExportDetailLevel detailLevel)
    {
        bool useTextures = ModelView.TextureCheckBox.IsChecked == true;
        ConcurrentBag<MainViewModel.DisplayPart> displayParts = new ConcurrentBag<MainViewModel.DisplayPart>();

        foreach (var ent in entities)
        {
            if (ent.HasGeometry())
            {
                var dynamicParts = ent.Load(detailLevel);
                ModelView.SetGroupIndices(new HashSet<int>(dynamicParts.Select(x => x.GroupIndex)));
                if (ModelView.GetSelectedGroupIndex() != -1)
                    dynamicParts = dynamicParts.Where(x => x.GroupIndex == ModelView.GetSelectedGroupIndex()).ToList();

                foreach (var part in dynamicParts)
                {
                    MainViewModel.DisplayPart displayPart = new MainViewModel.DisplayPart();
                    displayPart.BasePart = part;
                    displayPart.Translations.Add(Vector3.Zero);
                    displayPart.Rotations.Add(Vector4.Zero);
                    displayPart.Scales.Add(Vector3.One);

                    if (useTextures && part.Material?.Pixel.Textures.Any() == true)
                    {
                        var texture = TextureView.RemoveAlpha(part.Material.Pixel.Textures[0].Texture.GetTexture());
                        displayPart.DiffuseMaterial = new()
                        {
                            DiffuseMap = new HelixToolkit.SharpDX.Core.TextureModel(texture, true),
                        };
                    }

                    displayParts.Add(displayPart);
                }
            }

            if (ent.Skeleton != null)
            {
                MainViewModel.DisplayPart displayPart = new MainViewModel.DisplayPart();
                displayPart.BoneNodes = ent.Skeleton.GetBoneNodes();
                displayPart.Translations.Add(Vector3.Zero);
                displayPart.Rotations.Add(Vector4.Zero);
                displayPart.Scales.Add(Vector3.One);

                displayParts.Add(displayPart);
            }
        }

        return displayParts.ToList();
    }



    // TODO combine with above, I don't like this
    private List<MainViewModel.DisplayPart> MakeEntityModelDisplayParts(EntityModel entModel, ExportDetailLevel detailLevel)
    {
        bool useTextures = ModelView.TextureCheckBox.IsChecked == true;
        ConcurrentBag<MainViewModel.DisplayPart> displayParts = new ConcurrentBag<MainViewModel.DisplayPart>();

        var dynamicParts = entModel.Load(detailLevel, null);
        ModelView.SetGroupIndices(new HashSet<int>(dynamicParts.Select(x => x.GroupIndex)));
        if (ModelView.GetSelectedGroupIndex() != -1)
            dynamicParts = dynamicParts.Where(x => x.GroupIndex == ModelView.GetSelectedGroupIndex()).ToList();

        foreach (var part in dynamicParts)
        {
            MainViewModel.DisplayPart displayPart = new MainViewModel.DisplayPart();
            displayPart.BasePart = part;
            displayPart.Translations.Add(Vector3.Zero);
            displayPart.Rotations.Add(Vector4.Zero);
            displayPart.Scales.Add(Vector3.One);

            if (useTextures && part.Material?.Pixel.Textures.Any() == true)
            {
                var texture = part.Material.Pixel.Textures[0].Texture.GetTexture();
                displayPart.DiffuseMaterial = new()
                {
                    DiffuseMap = new HelixToolkit.SharpDX.Core.TextureModel(texture, true),
                };
            }

            displayParts.Add(displayPart);
        }

        return displayParts.ToList();
    }

    private void SetupCheckboxHandlers()
    {
        ModelView.TextureCheckBox.Visibility = Visibility.Visible;

        // Detach first to prevent multiple subscriptions
        ModelView.TextureCheckBox.Checked -= TextureCheckBox_Checked;
        ModelView.TextureCheckBox.Unchecked -= TextureCheckBox_Unchecked;

        ModelView.TextureCheckBox.Checked += TextureCheckBox_Checked;
        ModelView.TextureCheckBox.Unchecked += TextureCheckBox_Unchecked;
    }

    private void TextureCheckBox_Checked(object sender, RoutedEventArgs e) =>
        LoadEntity(currentHash);

    private void TextureCheckBox_Unchecked(object sender, RoutedEventArgs e) =>
        LoadEntity(currentHash);
}
