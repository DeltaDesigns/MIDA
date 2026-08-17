using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Arithmic;
using HelixToolkit.SharpDX;
using Tiger;
using Tiger.Exporters;
using Tiger.Schema;
using Tiger.Schema.Entity;

namespace MIDA;

public partial class EntityView : UserControl
{
    public FileHash Hash;
    private bool _isEntityModel = false;
    private MainViewModel MVM;

    public EntityView()
    {
        InitializeComponent();
    }

    public bool LoadEntity(FileHash entityHash)
    {
        if (_isEntityModel)
            LoadEntityModel(entityHash);

        Log.Info($"Loading Entity {entityHash}");

        Hash = entityHash;
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
        _isEntityModel = true;
        Hash = entityModelHash;
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

    public static void Export(List<Entity> entities, string name, string overridePath = null, ExportTypeFlag exportType = ExportTypeFlag.Full, EntitySkeleton overrideSkeleton = null)
    {
        ConfigSubsystem config = ConfigSubsystem.Get();
        name = Helpers.SanitizeString(name);
        string savePath = (overridePath is null ? config.GetExportSavePath() : overridePath) + $"/{name}";

        Log.Verbose($"Exporting entity model name: {name}");

        foreach (Entity entity in entities)
        {
            var scene = Tiger.Exporters.Exporter.Get().CreateScene(entity.Hash, ExportType.Entities);

            if (entity.Skeleton == null && overrideSkeleton != null)
                entity.Skeleton = overrideSkeleton;

            List<DynamicMeshPart> dynamicParts = entity.Load(ExportDetailLevel.MostDetailed);
            List<BoneNode> boneNodes = overrideSkeleton != null ? overrideSkeleton.GetBoneNodes() : new List<BoneNode>();
            if (entity.Skeleton != null && overrideSkeleton == null)
            {
                boneNodes = entity.Skeleton.GetBoneNodes();
            }
            scene.AddEntity(entity, dynamicParts, boneNodes);
            if (exportType == ExportTypeFlag.Full)
            {
                entity.SaveMaterialsFromParts(scene, dynamicParts);
                entity.SaveTexturePlates(savePath);
            }

            Tiger.Exporters.Exporter.Get().Export(savePath ?? null); // 'temp' fix for file in-use crash
        }

        //Tiger.Exporters.Exporter.Get().Export(savePath ?? null);
        Log.Info($"Exported entity model {name} to {savePath.Replace('\\', '/')}/");
    }

    private List<MainViewModel.DisplayPart> MakeEntityDisplayParts(List<Entity> entities, ExportDetailLevel detailLevel)
    {
        bool useTextures = ModelView.TextureCheckBox.IsChecked == true;

        ConcurrentBag<MainViewModel.DisplayPart> displayParts = new();
        foreach (Entity ent in entities)
        {
            var offsetTrans = Tiger.Schema.Vector3.Zero;
            var offsetRot = Tiger.Schema.Vector4.Quaternion;
            if (ent.HasGeometry())
            {
                List<DynamicMeshPart> dynamicParts = ent.Load(detailLevel);
                ModelView.SetGroupIndices(new HashSet<int>(dynamicParts.Select(x => x.GroupIndex)));
                if (ModelView.GetSelectedGroupIndex() != -1)
                    dynamicParts = dynamicParts.Where(x => x.GroupIndex == ModelView.GetSelectedGroupIndex()).ToList();

                offsetTrans = ent.Model.TranslationOffset.ToVec3();
                offsetRot = ent.Model.RotationOffset;
                foreach (DynamicMeshPart part in dynamicParts)
                {
                    MainViewModel.DisplayPart displayPart = new();
                    displayPart.BasePart = part;
                    displayPart.Translations.Add(Tiger.Schema.Vector3.Zero + offsetTrans);
                    displayPart.Rotations.Add(new(System.Numerics.Quaternion.Identity * offsetRot.ToQuat()));
                    displayPart.Scales.Add(System.Numerics.Vector3.One);

                    if (useTextures && part.Material?.Pixel.Textures.Any() == true && part.Material.Pixel.Textures[0].Texture is not null)
                    {
                        Stream texture = TextureView.RemoveAlpha(part.Material.Pixel.Textures[0].Texture.GetTexture());
                        displayPart.DiffuseMaterial = new()
                        {
                            DiffuseMap = new TextureModel(texture, true),
                        };
                    }

                    displayParts.Add(displayPart);
                }
            }

            if (ent.Skeleton != null && ModelView.SkeletonCheckBox.IsChecked == true)
            {
                MainViewModel.DisplayPart displayPart = new();
                displayPart.BoneNodes = ent.Skeleton.GetBoneNodes();
                displayPart.Translations.Add(offsetTrans);
                displayPart.Rotations.Add(offsetRot);
                displayPart.Scales.Add(System.Numerics.Vector3.One);

                displayParts.Add(displayPart);
            }
        }

        return displayParts.ToList();
    }

    // TODO combine with above, I don't like this
    private List<MainViewModel.DisplayPart> MakeEntityModelDisplayParts(EntityModel entModel, ExportDetailLevel detailLevel)
    {
        ConcurrentBag<MainViewModel.DisplayPart> displayParts = new();

        List<DynamicMeshPart> dynamicParts = entModel.Load(detailLevel, null);
        ModelView.SetGroupIndices(new HashSet<int>(dynamicParts.Select(x => x.GroupIndex)));
        if (ModelView.GetSelectedGroupIndex() != -1)
            dynamicParts = dynamicParts.Where(x => x.GroupIndex == ModelView.GetSelectedGroupIndex()).ToList();

        foreach (DynamicMeshPart part in dynamicParts)
        {
            MainViewModel.DisplayPart displayPart = new();
            displayPart.BasePart = part;
            displayPart.Translations.Add(System.Numerics.Vector3.Zero);
            displayPart.Rotations.Add(System.Numerics.Vector4.Zero);
            displayPart.Scales.Add(System.Numerics.Vector3.One);

            displayParts.Add(displayPart);
        }

        return displayParts.ToList();
    }

    private void SetupCheckboxHandlers()
    {
        ModelView.TextureCheckBox.Visibility = Visibility.Visible;
        ModelView.SkeletonCheckBox.Visibility = Visibility.Visible;

        // Detach first to prevent multiple subscriptions
        ModelView.TextureCheckBox.Checked -= ReloadEntity;
        ModelView.TextureCheckBox.Unchecked -= ReloadEntity;

        ModelView.SkeletonCheckBox.Checked -= ReloadEntity;
        ModelView.SkeletonCheckBox.Unchecked -= ReloadEntity;

        ModelView.TextureCheckBox.Checked += ReloadEntity;
        ModelView.TextureCheckBox.Unchecked += ReloadEntity;

        ModelView.SkeletonCheckBox.Checked += ReloadEntity;
        ModelView.SkeletonCheckBox.Unchecked += ReloadEntity;
    }

    private void ReloadEntity(object sender, RoutedEventArgs e) =>
        LoadEntity(Hash);
}
