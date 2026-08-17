using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Arithmic;
using Tiger;
using Tiger.Exporters;
using Tiger.Schema;
using Tiger.Schema.Entity;
using Tiger.Schema.Static;

namespace MIDA;

public partial class MapView : UserControl
{
    // public StaticMapData StaticMap;
    // public FileHash Hash;

    private static MainWindow _mainWindow = null;

    private static ConfigSubsystem _config = TigerInstance.GetSubsystem<ConfigSubsystem>();

    private void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
        _mainWindow = Window.GetWindow(this) as MainWindow;
        ModelView.LodCombobox.SelectedIndex = 1; // default to least detail
    }

    public MapView()
    {
        InitializeComponent();
    }

    public void LoadMap(FileHash fileHash, ExportDetailLevel detailLevel, bool isEntities = false)
    {
        if (isEntities)
            GetEntityMapData(fileHash, detailLevel);
    }

    private void GetEntityMapData(FileHash tagHash, ExportDetailLevel detailLevel)
    {
        SetEntityMapUI(tagHash, detailLevel);
    }

    private void SetEntityMapUI(FileHash dataentry, ExportDetailLevel detailLevel)
    {
        var displayParts = MakeEntityDisplayParts(dataentry, detailLevel);
        Dispatcher.Invoke(() =>
        {
            MainViewModel MVM = (MainViewModel)ModelView.UCModelView.Resources["MVM"];
            MVM.SetChildren(displayParts);
        });
        displayParts.Clear();
    }
    public bool LoadEntity(List<Entity> entities, FbxHandler fbxHandler)
    {
        fbxHandler.Clear();
        foreach (var entity in entities)
            AddEntity(entity, ExportDetailLevel.MostDetailed, fbxHandler);
        return LoadUI(fbxHandler);
    }

    private void AddEntity(Entity entity, ExportDetailLevel detailLevel, FbxHandler fbxHandler)
    {
        var dynamicParts = entity.Load(detailLevel);
        //ModelView.SetGroupIndices(new HashSet<int>(dynamicParts.Select(x => x.GroupIndex)));
        //dynamicParts = dynamicParts.Where(x => x.GroupIndex == ModelView.GetSelectedGroupIndex()).ToList();
        fbxHandler.AddEntityToScene(entity, dynamicParts, detailLevel);
        Log.Verbose($"Adding entity {entity.Hash}/{entity.Model?.Hash} with {dynamicParts.Sum(p => p.Indices.Count)} vertices to fbx");
    }

    private bool LoadUI(FbxHandler fbxHandler)
    {
        MainViewModel MVM = (MainViewModel)ModelView.UCModelView.Resources["MVM"];
        ConfigSubsystem config = TigerInstance.GetSubsystem<ConfigSubsystem>();
        string filePath = $"{config.GetExportSavePath()}/temp.fbx";
        fbxHandler.ExportScene(filePath);
        bool loaded = MVM.LoadEntityFromFbx(filePath);
        fbxHandler.Clear();
        return loaded;
    }

    public void Clear()
    {
        MainViewModel MVM = (MainViewModel)ModelView.UCModelView.Resources["MVM"];
        MVM.Clear();
    }

    public void Dispose()
    {
        MainViewModel MVM = (MainViewModel)ModelView.UCModelView.Resources["MVM"];
        MVM.Dispose();
    }

    public static void ExportFullMap(Tag<SMapContainer> map, string savePath)
    {
        Directory.CreateDirectory(savePath);

        ExtractDataTables(map, savePath);
    }

    private static void ExtractDataTables(Tag<SMapContainer> map, string savePath)
    {
        ExporterScene staticsScene = Exporter.Get().CreateScene(map.Hash.ToString(), ExportType.Statics, DataExportType.Map);
        ExporterScene terrainScene = Exporter.Get().CreateScene($"{map.Hash}_Terrain", ExportType.Terrain, DataExportType.Map);

        Parallel.ForEach(map.TagData.MapDataTables, data =>
        {
            data.MapDataTable.TagData.DataEntries.ForEach(entry =>
            {
                switch (entry.DataResource.GetValue(data.MapDataTable.GetReader()))
                {
                    case SMapDataResource staticMapResource:
                        staticMapResource.StaticMapParent?.Load();
                        if (staticMapResource.StaticMapParent is null)
                            return;

                        staticMapResource.StaticMapParent.TagData.StaticMap.LoadIntoExporterScene(staticsScene);
                        break;

                    case SStaticAOResource AO:
                        Exporter.Get().GetOrCreateGlobalScene().AddToGlobalScene(AO, true);
                        break;

                    case SMapTerrainResource terrain:
                        terrain.Terrain?.Load();
                        if (terrain.Terrain is null)
                            return;

                        terrain.Terrain.LoadIntoExporter(terrainScene, savePath, terrain.Identifier);
                        break;
                    default:
                        break;
                }
            });
        });
    }

    // TODO: Merge all this into one, or simplify it?
    private List<MainViewModel.DisplayPart> MakeDisplayParts(StaticMapData staticMap, ExportDetailLevel detailLevel)
    {
        ConcurrentBag<MainViewModel.DisplayPart> displayParts = new ConcurrentBag<MainViewModel.DisplayPart>();

        Parallel.ForEach(staticMap.TagData.InstanceCounts, c =>
        {
            // inefficiency as sometimes there are two instance count entries with same hash. why? idk
            var model = staticMap.TagData.Statics[c.StaticIndex].Static;
            var parts = model.Load(ExportDetailLevel.MostDetailed);
            for (int i = c.InstanceOffset; i < c.InstanceOffset + c.InstanceCount; i++)
            {
                foreach (var part in parts)
                {
                    MainViewModel.DisplayPart displayPart = new MainViewModel.DisplayPart();
                    displayPart.BasePart = part;
                    displayPart.Translations.Add(staticMap.TagData.Instances[i].Position);
                    displayPart.Rotations.Add(staticMap.TagData.Instances[i].Rotation);
                    displayPart.Scales.Add(new Vector3(staticMap.TagData.Instances[i].Scale.X));
                    displayParts.Add(displayPart);
                }

            }
        });

        return displayParts.ToList();
    }

    private List<MainViewModel.DisplayPart> MakeTerrainDisplayParts(Terrain terrain, ExportDetailLevel detailLevel)
    {
        ConcurrentBag<MainViewModel.DisplayPart> displayParts = new ConcurrentBag<MainViewModel.DisplayPart>();
        List<StaticPart> parts = new();
        foreach (var partEntry in terrain.TagData.StaticParts)
        {
            if (partEntry.DetailLevel == 0)
            {
                var part = terrain.MakePart(partEntry);
                terrain.TransformPositions(part);
                terrain.TransformTexcoords(part);
                parts.Add(part);
            }
        }

        foreach (var part in parts)
        {
            MainViewModel.DisplayPart displayPart = new MainViewModel.DisplayPart();
            displayPart.BasePart = part;
            displayPart.Translations.Add(Vector3.Zero);
            displayPart.Rotations.Add(Vector4.Zero);
            displayPart.Scales.Add(Vector3.One);
            displayParts.Add(displayPart);
        }
        return displayParts.ToList();
    }

    private List<MainViewModel.DisplayPart> MakeEntityDisplayParts(FileHash hash, ExportDetailLevel detailLevel)
    {
        ConcurrentBag<MainViewModel.DisplayPart> displayParts = new ConcurrentBag<MainViewModel.DisplayPart>();

        List<SMapDataEntry> dataEntries = new();
        dataEntries.AddRange(FileResourcer.Get().GetSchemaTag<SMapDataTable>(hash).TagData.DataEntries);

        Parallel.ForEach(dataEntries, entry =>
        {
            Entity entity = FileResourcer.Get().GetFile(typeof(Entity), entry.Entity.Hash);
            List<Entity> entities = new List<Entity> { entity };
            entities.AddRange(entity.GetEntityChildren());
            foreach (var ent in entities)
            {
                if (ent.HasGeometry())
                {
                    var parts = ent.Load(ExportDetailLevel.MostDetailed);

                    foreach (var part in parts)
                    {
                        MainViewModel.DisplayPart displayPart = new MainViewModel.DisplayPart();
                        displayPart.BasePart = part;
                        displayPart.Translations.Add(entry.Transfrom.Translation.ToVec3());
                        displayPart.Rotations.Add(entry.Transfrom.Rotation);
                        displayPart.Scales.Add(new Tiger.Schema.Vector3(entry.Transfrom.Translation.W, entry.Transfrom.Translation.W, entry.Transfrom.Translation.W));
                        displayParts.Add(displayPart);
                    }
                }
            }
        });
        return displayParts.ToList();
    }
}
