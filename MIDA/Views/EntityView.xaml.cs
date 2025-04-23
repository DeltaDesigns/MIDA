using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    public bool LoadEntity(FileHash entityHash, FbxHandler fbxHandler)
    {
        fbxHandler.Clear();
        Entity entity = FileResourcer.Get().GetFile<Entity>(entityHash);

        List<Entity> entities = new List<Entity> { entity };
        entities.AddRange(entity.GetEntityChildren());
        entities.ForEach(entity =>
        {
            AddEntity(entity, ModelView.GetSelectedLod(), fbxHandler);
        });
        return LoadUI(fbxHandler);
    }

    public bool LoadEntityModel(FileHash entityModelHash, FbxHandler fbxHandler)
    {
        fbxHandler.Clear();
        EntityModel entityModel = FileResourcer.Get().GetFile<EntityModel>(entityModelHash);

        var dynamicParts = entityModel.Load(ModelView.GetSelectedLod(), null);
        ModelView.SetGroupIndices(new HashSet<int>(dynamicParts.Select(x => x.GroupIndex)));
        if (ModelView.GetSelectedGroupIndex() != -1)
            dynamicParts = dynamicParts.Where(x => x.GroupIndex == ModelView.GetSelectedGroupIndex()).ToList();

        for (int i = 0; i < dynamicParts.Count; i++)
        {
            var dynamicPart = dynamicParts[i];
            fbxHandler.AddMeshPartToScene(dynamicPart, i, entityModelHash);
        }

        return LoadUI(fbxHandler);
    }

    public async void LoadEntityFromApi(TigerHash apiHash, FbxHandler fbxHandler)
    {
        fbxHandler.Clear();
        List<Entity> entities = Investment.Get().GetEntitiesFromHash(apiHash);
        foreach (var entity in entities)
        {
            // todo find out why sometimes this is null
            if (entity == null)
            {
                continue;
            }
            AddEntity(entity, ModelView.GetSelectedLod(), fbxHandler);
        }
        LoadUI(fbxHandler);
    }

    private void AddEntity(Entity entity, ExportDetailLevel detailLevel, FbxHandler fbxHandler)
    {
        var dynamicParts = entity.Load(detailLevel);
        ModelView.SetGroupIndices(new HashSet<int>(dynamicParts.Select(x => x.GroupIndex)));
        if (ModelView.GetSelectedGroupIndex() != -1)
            dynamicParts = dynamicParts.Where(x => x.GroupIndex == ModelView.GetSelectedGroupIndex()).ToList();
        fbxHandler.AddEntityToScene(entity, dynamicParts, detailLevel);
        Log.Verbose($"Adding entity {entity.Hash}/{entity.Model?.Hash} with {dynamicParts.Sum(p => p.Indices.Count)} vertices to fbx");
    }

    private bool LoadUI(FbxHandler fbxHandler)
    {
        MainViewModel MVM = (MainViewModel)ModelView.UCModelView.Resources["MVM"];
        ConfigSubsystem config = MIDAInstance.GetSubsystem<ConfigSubsystem>();
        string filePath = $"{config.GetExportSavePath()}/temp.fbx";
        fbxHandler.ExportScene(filePath);
        bool loaded = MVM.LoadEntityFromFbx(filePath);
        fbxHandler.Clear();
        return loaded;
    }

    public static void Export(List<Entity> entities, string name, ExportTypeFlag exportType, EntitySkeleton overrideSkeleton = null, ExporterScene scene = null)
    {
        ConfigSubsystem config = ConfigSubsystem.Get();
        name = Helpers.SanitizeString(name);
        string savePath = config.GetExportSavePath() + $"/{name}";

        if (scene == null)
            scene = Tiger.Exporters.Exporter.Get().CreateScene(name, ExportType.Entities);

        Log.Verbose($"Exporting entity model name: {name}");

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
        Log.Info($"Exported entity model {name} to {savePath.Replace('\\', '/')}/");
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

        Log.Info($"Exporting entity model name: {name}");

        ExportGearShader(item, name, savePath);

        // Export the model
        // todo bad, should be replaced
        EntitySkeleton overrideSkeleton = null;

        string skeleHash = "00008E9C51120FCF";
        Entity skele = FileResourcer.Get().GetFile<Entity>(new FileHash(Hash64Map.Get().GetHash32Checked(skeleHash))); // 64 bit more permanent
        overrideSkeleton = new EntitySkeleton(skele.Skeleton.Hash);

        var val = Investment.Get().GetPatternEntityFromHash(item.Parent != null ? item.Parent.TagData.InventoryItemHash : item.Item.TagData.InventoryItemHash);

        var entities = Investment.Get().GetEntitiesFromHash(item.Item.TagData.InventoryItemHash);

        if (val.Skeleton == null && overrideSkeleton != null)
            val.Skeleton = overrideSkeleton;

        List<BoneNode> boneNodes = overrideSkeleton != null ? overrideSkeleton.GetBoneNodes() : new List<BoneNode>();
        if (val.Skeleton != null && overrideSkeleton == null)
            boneNodes = val.Skeleton.GetBoneNodes();

        var dynamicParts = val.Load(ExportDetailLevel.MostDetailed);
        scene.AddEntity(val.Hash, dynamicParts, boneNodes, DestinyGenderDefinition.None);
        val.SaveMaterialsFromParts(scene, dynamicParts);
        //val.SaveTexturePlates(savePath);

        if (!aggregateOutput)
            Tiger.Exporters.Exporter.Get().Export();
        else
            Tiger.Exporters.Exporter.Get().Export(savePath);

        Log.Info($"Exported entity model {name} to {savePath.Replace('\\', '/')}/");
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

    private List<MainViewModel.DisplayPart> MakeEntityDisplayParts(Entity entity, ExportDetailLevel detailLevel)
    {
        ConcurrentBag<MainViewModel.DisplayPart> displayParts = new ConcurrentBag<MainViewModel.DisplayPart>();
        List<Entity> entities = new List<Entity> { entity };
        entities.AddRange(entity.GetEntityChildren());

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
                    displayParts.Add(displayPart);
                }
            }
        }

        return displayParts.ToList();
    }
}
