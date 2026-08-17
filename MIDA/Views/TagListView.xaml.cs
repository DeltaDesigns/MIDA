using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Arithmic;
using ConcurrentCollections;
using Tiger;
using Tiger.Schema;
using Tiger.Schema.Activity;
using Tiger.Schema.Activity.MARATHON;
using Tiger.Schema.Audio;
using Tiger.Schema.Entity;
using Tiger.Schema.Shaders;
using Activity = Tiger.Schema.Activity.MARATHON.Activity;

namespace MIDA;

public enum ETagListType
{
    [Description("None")]
    None,
    [Description("Destination Global Tag Bag List")]
    DestinationGlobalTagBagList,
    [Description("Destination Global Tag Bag")]
    DestinationGlobalTagBag,
    [Description("Budget Set")]
    BudgetSet,
    [Description("Entity [Final]")]
    Entity,
    [Description("BACK")]
    Back,

    [Description("Package")]
    Package,

    [Description("Activity List")]
    ActivityList,
    [Description("Activity [Final]")]
    Activity,


    [Description("Texture [Final]")]
    Texture,

    [Description("Dialogue List")]
    DialogueList,
    [Description("Dialogue [Final]")]
    Dialogue,

    [Description("Directive List")]
    DirectiveList,
    [Description("Directive [Final]")]
    Directive,

    [Description("Sounds Packages List")]
    SoundsPackagesList,
    [Description("Sounds Package [Final]")]
    SoundsPackage,
    [Description("Sounds List")]
    SoundsList,
    [Description("Sound [Final]")]
    Sound,

    [Description("Music List")]
    MusicList,
    [Description("Music [Final]")]
    Music,

    [Description("Weapon Audio Group List")]
    WeaponAudioGroupList,
    [Description("Weapon Audio Group [Final]")]
    WeaponAudioGroup,
    [Description("Weapon Audio List")]
    WeaponAudioList,
    [Description("Weapon Audio [Final]")]
    WeaponAudio,

    [Description("BKHD Group List")]
    BKHDGroupList,
    [Description("BKHD Group [Final]")]
    BKHDGroup,
    [Description("Weapon Audio List")]
    BKHDAudioList,
    [Description("Weapon Audio [Final]")]
    BKHDAudio,

    [Description("Material List [Packages]")]
    MaterialList,
    [Description("Material [Final]")]
    Material,
}

/// <summary>
/// The current implementation of Package is limited so you cannot have nested views below a Package.
/// For future, would be better to split the tag items up so we can cache them based on parents.
/// </summary>
public partial class TagListView : UserControl
{
    private struct ParentInfo
    {
        public string ParentName;
        public ETagListType TagListType;
        public TigerHash? Hash;
        public string SearchTerm;
        public ConcurrentBag<TagItem> AllTagItems;
    }

    private ConcurrentBag<TagItem> _allTagItems;
    private static MainWindow _mainWindow = null;
    private ETagListType _tagListType;
    private TigerHash? _currentHash = null;
    private Stack<ParentInfo> _parentStack = new Stack<ParentInfo>();
    private bool _bTrimName = true;
    private bool _bShowNamedOnly = false;
    private TagListView _tagListControl = null;
    private ToggleButton _previouslySelected = null;
    private int _selectedIndex = -1;
    private string _weaponItemName = null;

    private void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
        _mainWindow = Window.GetWindow(this) as MainWindow;
    }

    public TagListView()
    {
        InitializeComponent();
    }

    private TagView GetViewer()
    {
        if (Parent is Grid)
        {
            if ((Parent as Grid).Parent is TagListViewerView)
                return ((Parent as Grid).Parent as TagListViewerView).TagView;
            else if ((Parent as Grid).Parent is TagView)
                return (Parent as Grid).Parent as TagView;
        }
        Log.Error($"Parent is not a TagListViewerView, is {Parent.GetType().Name}.");
        return null;
    }

    public async void LoadContent(ETagListType tagListType, TigerHash contentValue = null, bool bFromBack = false,
        ConcurrentBag<TagItem> overrideItems = null, TagItem fullTag = null)
    {
        Log.Verbose($"Loading content type {tagListType} contentValue {contentValue} from back {bFromBack}");
        if (overrideItems != null)
        {
            _allTagItems = overrideItems;
        }
        else
        {
            if (contentValue != null && !bFromBack && !EnumExtensions.GetEnumDescription(tagListType).Contains("[Final]")) // if the type nests no new info, it isnt a parent
            {
                _parentStack.Push(new ParentInfo
                {
                    ParentName = fullTag?.Name ?? "",
                    AllTagItems = _allTagItems,
                    Hash = _currentHash,
                    TagListType = _tagListType,
                    SearchTerm = SearchBox.Text
                });
            }

            switch (tagListType)
            {
                case ETagListType.DestinationGlobalTagBagList:
                    await LoadDestinationGlobalTagBagList();
                    break;
                case ETagListType.Back:
                    Back_Clicked();
                    return;
                case ETagListType.DestinationGlobalTagBag:
                    LoadDestinationGlobalTagBag(contentValue as FileHash);
                    break;
                case ETagListType.BudgetSet:
                    LoadBudgetSet(contentValue as FileHash);
                    break;
                case ETagListType.Entity:
                    LoadEntity(contentValue as FileHash);
                    break;

                case ETagListType.Package:
                    LoadPackage(contentValue as FileHash);
                    break;
                case ETagListType.ActivityList:
                    await LoadActivityList();
                    break;
                case ETagListType.Activity:
                    LoadActivity(contentValue as FileHash);
                    break;


                case ETagListType.Texture:
                    LoadTexture(contentValue as FileHash);
                    break;
                case ETagListType.DialogueList:
                    LoadDialogueList(contentValue as FileHash);
                    break;
                case ETagListType.Dialogue:
                    LoadDialogue(contentValue as FileHash);
                    break;

                case ETagListType.DirectiveList:
                    LoadDirectiveList(contentValue as FileHash);
                    break;
                case ETagListType.Directive:
                    LoadDirective(contentValue as FileHash);
                    break;

                case ETagListType.Sound:
                    LoadSound(contentValue as FileHash);
                    break;
                case ETagListType.MusicList:
                    LoadMusicList(contentValue as FileHash);
                    break;
                case ETagListType.Music:
                    LoadMusic(contentValue as FileHash, fullTag);
                    break;

                case ETagListType.MaterialList:
                    await LoadMaterialList();
                    break;
                case ETagListType.Material:
                    LoadMaterial(contentValue as FileHash);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        if (!EnumExtensions.GetEnumDescription(tagListType).Contains("[Final]"))
        {
            _currentHash = contentValue;
            _tagListType = tagListType;
            if (!bFromBack)
            {
                SearchBox.Text = "";
            }

            RefreshItemList();
        }

        Log.Verbose($"Loaded content type {tagListType} contentValue {contentValue} from back {bFromBack}");
    }

    /// <summary>
    /// For when we want stuff in packages, we then split up based on what the FileHash value is.
    /// I kinda cheat here, I store everything in one massive _allTagItems including the packages
    /// </summary>
    /// <param name="packageId">Package ID for this package to load data for.</param>
    private void LoadPackage(FileHash pkgHash)
    {
        int pkgId = pkgHash.PackageId;

        SetBulkGroup(pkgId.ToString("x4"));
        var collection = _allTagItems.Where(x => (x.Hash as FileHash).PackageId == pkgId && x.TagType != ETagListType.Package).ToList();
        _allTagItems = new ConcurrentBag<TagItem>(collection);
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        // if ((e.Key == Key.Down || e.Key == Key.Right))
        // {
        //     // find the selected one
        //     List<TagItem> tagItems = TagList.Items.OfType<TagItem>().ToList();
        //     var selected = tagItems.FirstOrDefault(x => x.IsChecked);
        //     if (selected != null)
        //     {
        //         int index = tagItems.IndexOf(selected);
        //         var z = TagList.ItemContainerGenerator.ContainerFromIndex(index);
        //         var w = GetChildOfType<ToggleButton>(z);
        //         w.IsChecked = false;
        //         var x = TagList.ItemContainerGenerator.ContainerFromIndex(index+1);
        //         var y = GetChildOfType<ToggleButton>(x);
        //         y.IsChecked = true;
        //     }
        //     var item = TagList.SelectedItem;
        //     var a = 0;
        // }
    }

    private void SetItemListByString(string searchStr, bool bPackageSearchAllOverride = false)
    {
        if (_allTagItems == null)
            return;
        if (_allTagItems.IsEmpty)
            return;

        bool bShowTrimCheckbox = false;
        bool bNoName = false;
        bool bName = false;

        var displayItems = new ConcurrentBag<TagItem>();
        // Select and sort by relevance to selected string
        Parallel.ForEach(_allTagItems, item =>
        {
            if (item.Name.Contains('\\'))
                bShowTrimCheckbox = true;
            if (item.Name == String.Empty)
                bNoName = true;
            if (item.Name != String.Empty)
                bName = true;

            if (_bShowNamedOnly && item.Name == String.Empty)
            {
                return;
            }

            if (EnumExtensions.GetEnumDescription(_tagListType).Contains("[Packages]") && !bPackageSearchAllOverride)
            {
                // Package-enabled lists have [Packages] in their enum
                if (item.TagType != ETagListType.Package)
                {
                    return;
                }
            }

            string name = item.Name != "" ? item.Name : item.Hash;
            bool bWasTrimmed = false;
            if (item.Name.Contains("\\") && _bTrimName)
            {
                name = TrimName(name);
                bWasTrimmed = true;
            }

            // bool bWasTrimmed = name != item.Name;
            if (name.ToLower().Contains(searchStr)
                || item.Hash.ToString().ToLower().Contains(searchStr)
                || item.Hash.Hash32.ToString().Contains(searchStr)
                || (item.Subname != null && item.Subname.ToLower().Contains(searchStr)))
            {
                Package pkg = (item.Hash as FileHash) is not null ? PackageResourcer.Get().GetPackage((item.Hash as FileHash).PackageId) : null;
                if (pkg is not null && pkg.GetPackageMetadata().Name.Contains("redacted"))
                    name = $"🔐 {name}";

                string subname = searchStr != string.Empty && item.Type != "Package" ?
                            $"{item.Subname}" + (pkg != null ? $" : [{pkg.GetPackageMetadata().Name}]" : "")
                            : item.Subname;

                displayItems.Add(new TagItem
                {
                    Hash = item.Hash,
                    Name = name,
                    TagType = item.TagType,
                    Type = item.Type,
                    Subname = subname,
                    FontSize = _bTrimName || !bWasTrimmed ? 16 : 12,
                    Extra = item.Extra
                });
            }
        });

        // Check if trim names and filter named should be visible (if there any named items)
        TrimCheckbox.Visibility = bShowTrimCheckbox ? Visibility.Visible : Visibility.Hidden;
        ShowNamedCheckbox.Visibility = bName && bNoName ? Visibility.Visible : Visibility.Hidden;

        if (bNoName)
        {
            _bShowNamedOnly = false;
        }

        if (displayItems.Count == 0 && EnumExtensions.GetEnumDescription(_tagListType).Contains("[Packages]") && !bPackageSearchAllOverride)
        {
            SetItemListByString(searchStr, true);
            return;
        }

        List<TagItem> tagItems = displayItems.ToList();
        if (tagItems.Any() && tagItems.First().Type == "Package")
        {
            tagItems.Sort((p, q) => string.Compare(p.Name, q.Name, StringComparison.OrdinalIgnoreCase));
        }
        else
        {
            //tagItems.Sort((a, b) => a.Hash.Hash32 > b.Hash.Hash32 ? 1 : -1);
            tagItems = tagItems.OrderBy(x => x.Hash.Hash32).ToList();
        }
        //tagItems = tagItems.DistinctBy(t => t.Hash).ToList();
        // If we have a parent, add a TagItem that is actually a back button as first
        if (_parentStack.Count > 0)
        {
            tagItems.Insert(0, new TagItem
            {
                Name = "BACK",
                Subname = $"{_parentStack.First().ParentName}",
                TagType = ETagListType.Back,
                FontSize = 24
            });
        }

        TagList.ItemsSource = tagItems;
    }

    /// <summary>
    /// From all the existing items in _allTagItems, we generate the packages for it
    /// and add but only if packages don't exist already.
    /// </summary>
    private void MakePackageTagItems()
    {
        ConcurrentHashSet<int> packageIds = new ConcurrentHashSet<int>();
        bool bBroken = false;
        Parallel.ForEach(_allTagItems, (item, state) =>
        {
            if (item.TagType == ETagListType.Package)
            {
                bBroken = true;
                state.Break();
            }

            packageIds.Add((item.Hash as FileHash).PackageId);  // todo fix this garbage 'as' call
        });

        if (bBroken)
            return;

        Parallel.ForEach(packageIds, pkgId =>
        {
            _allTagItems.Add(new TagItem
            {
                Name = string.Join('_', PackageResourcer.Get().PackagePathsCache.GetPackagePathFromId((ushort)pkgId).Split('_').Skip(1).SkipLast(1)),
                Hash = new FileHash(pkgId, 0),
                TagType = ETagListType.Package
            });
        });
    }

    private void RefreshItemList()
    {
        var searchStr = SearchBox.Text;

        // Flips tag hash to the "intended" way (sigh) ex 80BB6216 -> 1662BB80
        if (Helpers.ParseHash(searchStr, out uint parsedHash))
        {
            searchStr = new TigerHash(parsedHash).ToString();
        }
        SetItemListByString(searchStr.ToLower());
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        RefreshItemList();
    }

    /// <summary>
    /// This onclick is used by all the different types.
    /// </summary>
    private void TagItem_OnClick(object sender, RoutedEventArgs e)
    {
        var btn = sender as ToggleButton;
        TagItem tagItem = btn.DataContext as TagItem;
        TigerHash tigerHash = tagItem.Hash;

        if (_previouslySelected != null)
            _previouslySelected.IsChecked = false;
        _selectedIndex = TagList.Items.IndexOf(tagItem);
        // if (_previouslySelected == btn)
        // _previouslySelected.IsChecked = !_previouslySelected.IsChecked;
        _previouslySelected = btn;

        Package pkg = (tagItem.Hash as FileHash) is not null ? PackageResourcer.Get().GetPackage((tagItem.Hash as FileHash).PackageId) : null;
        if (pkg is not null && pkg.GetPackageMetadata().Name.Contains("redacted"))
        {
            if (!PackageResourcer.Get().Keys.ContainsKey(pkg.GetPackageMetadata().PackageGroup))
            {
                //MessageBox.Show($"No decryption key found, can not display content.", $"This item belongs to a redacted package.", MessageBoxButton.OK);

                // This could be a lot better probably but oh well
                PopupBanner warn = new()
                {
                    Icon = "🔐",
                    Title = "ERROR",
                    Subtitle = "No decryption key found, can not display content.",
                    Description = "This item belongs to a redacted package, which means its content can not be shown.",
                    Style = PopupBanner.PopupStyle.Warning
                };
                warn.Show();

                btn.IsChecked = false;
                return;
            }
        }
        LoadContent(tagItem.TagType, tigerHash, fullTag: tagItem);
    }

    public static T GetChildOfType<T>(DependencyObject depObj)
        where T : DependencyObject
    {
        if (depObj == null) return null;

        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
        {
            var child = VisualTreeHelper.GetChild(depObj, i);

            var result = (child as T) ?? GetChildOfType<T>(child);
            if (result != null) return result;
        }
        return null;
    }

    public static List<T> GetChildrenOfType<T>(DependencyObject depObj)
        where T : DependencyObject
    {
        var children = new List<T>();
        if (depObj == null) return children;

        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
        {
            var child = VisualTreeHelper.GetChild(depObj, i);

            if (child is T)
            {
                children.Add(child as T);
            }
            else
            {
                children.AddRange(GetChildrenOfType<T>(child));
            }
        }
        return children;
    }

    /// <summary>
    /// Use the ParentInfo to go back to previous tag data.
    /// </summary>
    private void Back_Clicked()
    {
        ParentInfo parentInfo = _parentStack.Pop();
        SearchBox.Text = parentInfo.SearchTerm;
        LoadContent(parentInfo.TagListType, parentInfo.Hash, true, parentInfo.AllTagItems);
    }

    private void TrimCheckbox_OnChecked(object sender, RoutedEventArgs e)
    {
        _bTrimName = true;
        RefreshItemList();
    }

    private void TrimCheckbox_OnUnchecked(object sender, RoutedEventArgs e)
    {
        _bTrimName = false;
        RefreshItemList();
    }

    private string TrimName(string name)
    {
        return name.Split("\\").Last().Split(".")[0];
    }

    private void ShowNamedCheckbox_OnChecked(object sender, RoutedEventArgs e)
    {
        _bShowNamedOnly = true;
        RefreshItemList();
    }

    private void ShowNamedCheckbox_OnUnchecked(object sender, RoutedEventArgs e)
    {
        _bShowNamedOnly = false;
        RefreshItemList();
    }

    /// <summary>
    /// We only allow one viewer visible at a time, so setting the viewer hides the rest.
    /// </summary>
    /// <param name="eViewerType">Viewer type to set visible.</param>
    private void SetViewer(TagView.EViewerType eViewerType)
    {
        var viewer = GetViewer();
        viewer.SetViewer(eViewerType);
    }

    private void TagList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_selectedIndex == -1)
            return;
        if (TagList.SelectedIndex > _selectedIndex)
        {
            var currentButton = GetChildOfType<ToggleButton>(TagList.ItemContainerGenerator.ContainerFromIndex(_selectedIndex));
            if (currentButton == null)
                return;
            currentButton.IsChecked = false;
            var nextButton = GetChildOfType<ToggleButton>(TagList.ItemContainerGenerator.ContainerFromIndex(_selectedIndex + 1));
            if (nextButton == null)
                return;
            nextButton.IsChecked = true;
            _selectedIndex++;
            TagItem_OnClick(nextButton, null);
        }

        else if (TagList.SelectedIndex < _selectedIndex)
        {
            var currentButton = GetChildOfType<ToggleButton>(TagList.ItemContainerGenerator.ContainerFromIndex(_selectedIndex));
            if (currentButton == null)
                return;
            currentButton.IsChecked = false;
            var nextButton = GetChildOfType<ToggleButton>(TagList.ItemContainerGenerator.ContainerFromIndex(_selectedIndex - 1));
            if (nextButton == null)
                return;
            nextButton.IsChecked = true;
            _selectedIndex--;
            TagItem_OnClick(nextButton, null);

        }
    }

    public void ShowBulkExportButton()
    {
        BulkExportButton.Visibility = Visibility.Visible;
    }

    public void SetBulkGroup(string group)
    {
        var tab = ((Parent as Grid).Parent as TagListViewerView).Parent as TabItem;
        BulkExportButton.Tag = $"{group}_{tab.Header}";
    }

    private async void BulkExport_OnClick(object sender, RoutedEventArgs e)
    {
        if (BulkExportButton.Tag == null)
        {
            return;
        }

        var groupName = BulkExportButton.Tag as string;
        var viewer = GetViewer();
        bool bStaticShowing = viewer.StaticControl.Visibility == Visibility.Visible;
        bool bEntityShowing = viewer.EntityControl.Visibility == Visibility.Visible;
        viewer.StaticControl.Visibility = bStaticShowing ? Visibility.Hidden : viewer.StaticControl.Visibility;
        viewer.EntityControl.Visibility = bEntityShowing ? Visibility.Hidden : viewer.EntityControl.Visibility;

        // Iterate over all buttons and export it
        var items = TagList.ItemsSource.Cast<TagItem>();
        var exportItems = items.Where(x => x.TagType != ETagListType.Back && x.TagType != ETagListType.Package).ToList();
        if (exportItems.Count == 0)
        {
            MessageBox.Show("No tags to export.");
            return;
        }
        MainWindow.Progress.SetProgressStages(exportItems.Select((x, i) => $"Exporting {i + 1}/{exportItems.Count}: {x.Hash}").ToList());
        await Task.Run(() =>
        {
            foreach (var tagItem in exportItems)
            {
                var name = tagItem.Name == String.Empty ? tagItem.Hash : tagItem.Name;
                var exportInfo = new ExportInfo
                {
                    Hash = tagItem.Hash as FileHash,
                    Name = name,
                    SubPath = $"Bulk_{groupName}",
                    ExportType = ExportTypeFlag.Full
                };
                viewer.ExportControl.RoutedFunction(exportInfo);
                MainWindow.Progress.CompleteStage();
            }
        });
        viewer.StaticControl.Visibility = bStaticShowing ? Visibility.Visible : viewer.StaticControl.Visibility;
        viewer.EntityControl.Visibility = bEntityShowing ? Visibility.Visible : viewer.EntityControl.Visibility;
    }

    private void SetExportFunction(Action<ExportInfo> function, int exportTypeFlags, bool disableLoadingBar = false, bool hideBulkExport = false)
    {
        var viewer = GetViewer();
        viewer.ExportControl.SetExportFunction(function, exportTypeFlags, disableLoadingBar);
        if (!hideBulkExport)
            ShowBulkExportButton();
        else
            BulkExportButton.Visibility = Visibility.Hidden;
    }

    #region Destination Global Tag Bag

    /// <summary>
    /// Type 0x8080471D and only in sr_destination_metadata_010a?
    /// </summary>
    private async Task LoadDestinationGlobalTagBagList()
    {
        _allTagItems = new ConcurrentBag<TagItem>();
        var vals = await PackageResourcer.Get().GetAllHashesAsync<S8080AB5F>();
        Parallel.ForEach(vals, val =>
        {
            Tag<S8080AB5F> dgtbParent = FileResourcer.Get().GetSchemaTag<S8080AB5F>(val);
            if (dgtbParent.TagData.DestinationGlobalTagBags.Count == 0)
                return;

            foreach (var destinationGlobalTagBag in dgtbParent.TagData.DestinationGlobalTagBags)
            {
                if (!destinationGlobalTagBag.DestinationGlobalTagBag.IsValid())
                    continue;

                _allTagItems.Add(new TagItem
                {
                    Hash = destinationGlobalTagBag.DestinationGlobalTagBag,
                    Name = destinationGlobalTagBag.DestinationGlobalTagBagName,
                    Subname = $"{Helpers.GetReadableSize(destinationGlobalTagBag.DestinationGlobalTagBag.GetFileMetadata().Size)}",
                    TagType = ETagListType.DestinationGlobalTagBag
                });
            }
        });
    }

    private void LoadDestinationGlobalTagBag(FileHash hash)
    {
        Tag<S8080AB68> destinationGlobalTagBag = FileResourcer.Get().GetSchemaTag<S8080AB68>(hash);

        _allTagItems = new ConcurrentBag<TagItem>();
        Parallel.ForEach(destinationGlobalTagBag.TagData.Unk18, val =>
        {
            if (val.Tag == null)
                return;
            FileHash reference = val.Tag.Hash.GetReferenceHash();
            ETagListType tagType;
            string overrideType = String.Empty;
            switch (reference.Hash32)
            {
                case 0x8080987e:
                    tagType = ETagListType.BudgetSet;
                    break;
                case 0x8080BAAD:
                    tagType = ETagListType.Entity;
                    break;

                default:
                    if (val.Tag.Hash.GetFileMetadata().Type == 32)
                    {
                        tagType = ETagListType.Texture;
                        break;
                    }
                    tagType = ETagListType.None;
                    overrideType = reference;
                    break;
            }
            _allTagItems.Add(new TagItem
            {
                Hash = val.Tag.Hash,
                Name = val.TagPath,
                Subname = val.TagNote,
                TagType = tagType,
                Type = overrideType
            });
        });
    }

    #endregion

    #region Budget Set

    private void LoadBudgetSet(FileHash hash)
    {
        Tag<S7E988080> budgetSetHeader = FileResourcer.Get().GetSchemaTag<S7E988080>(hash);
        Tag<SED9E8080> budgetSet = FileResourcer.Get().GetSchemaTag<SED9E8080>(budgetSetHeader.TagData.Unk00.Hash);
        _allTagItems = new ConcurrentBag<TagItem>();
        Parallel.ForEach(budgetSet.TagData.Unk28, val =>
        {
            if (!val.Tag.Hash.IsValid())
            {
                Log.Error($"BudgetSet {budgetSetHeader.TagData.Unk00.Hash} has an invalid tag hash.");
                return;
            }
            ETagListType tagType = ETagListType.None;
            FileHash reference = val.Tag.Hash.GetReferenceHash();
            string overrideType = String.Empty;
            switch (reference.Hash32)
            {
                case 0x8080BAAD:
                    tagType = ETagListType.Entity;
                    break;
                default:
                    if (val.Tag.Hash.GetFileMetadata().Type == 32)
                    {
                        tagType = ETagListType.Texture;
                        break;
                    }
                    tagType = ETagListType.None;
                    overrideType = reference;
                    break;
            }
            _allTagItems.Add(new TagItem
            {
                Hash = val.Tag.Hash,
                Name = val.TagPath,
                TagType = tagType,
                Type = overrideType
            });
        });
    }

    #endregion

    #region Entity

    private void LoadEntity(FileHash fileHash)
    {
        var viewer = GetViewer();
        SetViewer(TagView.EViewerType.Entity);
        bool bLoadedSuccessfully = viewer.EntityControl.LoadEntity(fileHash);
        if (!bLoadedSuccessfully)
        {
            Log.Error($"UI failed to load entity for hash {fileHash}. You can still try to export the full model instead.");
            _mainWindow.SetLoggerSelected();
        }
        SetExportFunction(ExportEntity, (int)ExportTypeFlag.Full | (int)ExportTypeFlag.Minimal);
        viewer.ExportControl.ExportChildrenBox.Visibility = Visibility.Visible;
        viewer.ExportControl.SetExportInfo(fileHash);
        viewer.EntityControl.ModelView.SetModelFunction(() => viewer.EntityControl.LoadEntity(fileHash));
    }

    private void ExportEntity(ExportInfo info)
    {
        var viewer = GetViewer();
        Entity entity = FileResourcer.Get().GetFile<Entity>(info.Hash);
        List<Entity> entities = new List<Entity> { entity };
        Dispatcher.Invoke(() =>
        {
            if (viewer.ExportControl.ExportChildrenBox.Visibility == Visibility.Visible && viewer.ExportControl.ExportChildrenBox.IsChecked.Value == true)
                entities.AddRange(entity.GetEntityChildren());
            viewer.EntityControl.ModelView.Visibility = Visibility.Hidden;
        });
        EntityView.Export(entities, info.Name, exportType: info.ExportType);

        Dispatcher.Invoke(() =>
        {
            NotificationBanner notify = new()
            {
                Icon = "☑️",
                Title = "Export Complete",
                Description = $"Exported Entity {info.Name} to \"{ConfigSubsystem.Get().GetExportSavePath()}\\{info.Name}\\\"",
                Style = NotificationBanner.PopupStyle.Information
            };
            notify.OnProgressComplete += () => Dispatcher.Invoke(() => viewer.EntityControl.ModelView.Visibility = Visibility.Visible);
            notify.Show();
        });
    }

    #endregion

    #region Activity

    /// <summary>
    /// Type 0x80808e8e, but we use a child of it (0x80808e8b) so we can get the location.
    /// </summary>
    private async Task LoadActivityList()
    {
        _allTagItems = new ConcurrentBag<TagItem>();

        // Getting names
        ConcurrentDictionary<string, StringHash> nameHashes = new();
        ConcurrentDictionary<string, string> names = new();

        var valsChild = await PackageResourcer.Get().GetAllHashesAsync<S8080B383>();
        Parallel.ForEach(valsChild, val =>
        {
            Tag<S8080B383> tag = FileResourcer.Get().GetSchemaTag<S8080B383>(val);
            nameHashes.TryAdd(tag.TagData.DestinationName, tag.TagData.LocationName);
            GlobalStrings.Get().AddStrings(tag.TagData.StringContainer);
        });

        foreach (var keyValuePair in nameHashes)
        {
            names[keyValuePair.Key] = GlobalStrings.Get().GetString(keyValuePair.Value);
        }

        var vals = await PackageResourcer.Get().GetAllHashesAsync<IActivity>();
        Parallel.ForEach(vals, val =>
        {
            var activityName = PackageResourcer.Get().GetActivityName(val);
            var first = activityName.Split(".").First();

            // These are silly
            if (activityName.EndsWith("_ls") || activityName.Contains("_ls_"))
                activityName = $" {activityName}"; // Lost sector icon
            if (activityName.Contains("exotic"))
                activityName = $" {activityName}"; // Quest crown icon
            if (activityName.Contains("dungeon") || activityName.Contains("raid") || activityName.Contains("kingsfall"))
                activityName = $" {activityName}"; // Revive token icon (could do 💀 if people dont like it)

            _allTagItems.Add(new TagItem
            {
                Hash = val,
                Name = activityName,
                Subname = names.ContainsKey(first) ? names[first] : "",
                TagType = ETagListType.Activity
            });
        });

    }

    private void LoadActivity(FileHash fileHash)
    {
        ActivityView activityView = new ActivityView();
        _mainWindow.MakeNewTab(PackageResourcer.Get().GetActivityName(fileHash), activityView);
        activityView.LoadActivity(fileHash);
        _mainWindow.SetNewestTabSelected();
        // ExportControl.SetExportFunction(ExportActivityMapFull);
        // ExportControl.SetExportInfo(fileHash);
    }

    private void ExportActivityMapFull(object sender, RoutedEventArgs e)
    {
        var btn = sender as Button;
        ExportInfo info = (ExportInfo)btn.Tag;
        // ActivityControl.ExportFull();
    }

    #endregion

    #region Texture
    /// <summary>
    /// I could do it tiled, but cba to bother with it when you can just batch export to filesystem.
    /// </summary>
    private void LoadTexture(FileHash fileHash)
    {
        var viewer = GetViewer();
        Texture textureHeader = FileResourcer.Get().GetFile<Texture>(fileHash);
        if (textureHeader.IsCubemap())
        {
            SetViewer(TagView.EViewerType.TextureCube);
            viewer.CubemapControl.LoadCubemap(textureHeader);
        }
        else
        {
            SetViewer(TagView.EViewerType.Texture2D);
            viewer.TextureControl.LoadTexture(textureHeader);
        }
        SetExportFunction(ExportTexture, (int)ExportTypeFlag.Full);
        viewer.ExportControl.SetExportInfo(fileHash);
    }

    private void ExportTexture(ExportInfo info)
    {
        TextureExtractor.ExportTexture(info.Hash as FileHash);
    }

    #endregion

    #region Dialogue

    /// <summary>
    /// We assume all dialogue tables come from activities.
    /// </summary>
    private void LoadDialogueList(FileHash fileHash)
    {
        _allTagItems = new ConcurrentBag<TagItem>();

        ConcurrentDictionary<string, FileHash> dialogueTables = new();
        Activity activity = FileResourcer.Get().GetFile<Activity>(fileHash);
        if (activity.TagData.Unk18.GetValue(activity.GetReader()) is S6A988080 entry)
        {
            foreach (var dirtable in entry.DialogueTables)
            {
                if (dirtable.DialogueTable != null)
                    dialogueTables.TryAdd(dirtable.DialogueTable.Hash, dirtable.DialogueTable.Hash);
            }
        }

        Parallel.ForEach(activity.TagData.Unk50, val =>
        {
            foreach (var d2Class48898080 in val.Unk18)
            {
                var resource = d2Class48898080.UnkEntityReference.TagData.Unk10.GetValue(d2Class48898080.UnkEntityReference.GetReader());
                if (resource is S8080AB34 || resource is S8080AB38)
                {
                    if (resource.DialogueTable != null)
                        dialogueTables.TryAdd(resource.DialogueTable.Hash, resource.DialogueTable.Hash);
                }
            }
        });

        Parallel.ForEach(dialogueTables, entry =>
        {
            _allTagItems.Add(new TagItem
            {
                Name = entry.Key,
                Hash = entry.Value,
                TagType = ETagListType.Dialogue
            });
        });
    }


    // TODO replace this by deleting DialogueControl and using TagList instead
    private void LoadDialogue(FileHash fileHash)
    {
        var viewer = GetViewer();
        SetViewer(TagView.EViewerType.Dialogue);
        viewer.DialogueControl.Load(fileHash, viewer);
    }

    #endregion

    #region Directive

    private void LoadDirectiveList(FileHash fileHash)
    {
        _allTagItems = new ConcurrentBag<TagItem>();

        // Dialogue tables can be in the 0x80808948 entries

        Activity activityWQ = FileResourcer.Get().GetFile<Activity>(fileHash);
        if (activityWQ.TagData.Unk18.GetValue(activityWQ.GetReader()) is S6A988080 a988080)
        {
            var directiveTables = a988080.DirectiveTables.Select(x => x.DirectiveTable.Hash);

            Parallel.ForEach(directiveTables, hash =>
            {
                _allTagItems.Add(new TagItem
                {
                    Hash = hash,
                    Name = hash,
                    TagType = ETagListType.Directive
                });
            });
        }
        else if (activityWQ.TagData.Unk18.GetValue(activityWQ.GetReader()) is S20978080 class20978080)
        {
            var directiveTables = class20978080.PEDirectiveTables.Select(x => x.DirectiveTable.Hash);

            Parallel.ForEach(directiveTables, hash =>
            {
                _allTagItems.Add(new TagItem
                {
                    Hash = hash,
                    Name = hash,
                    TagType = ETagListType.Directive
                });
            });
        }
    }

    // TODO replace with taglist control
    private void LoadDirective(FileHash fileHash)
    {
        SetViewer(TagView.EViewerType.Directive);
        var viewer = GetViewer();
        viewer.DirectiveControl.Load(fileHash);
    }

    #endregion

    #region Sound
    private void LoadSound(FileHash fileHash)
    {
        var viewer = GetViewer();
        if (viewer.MusicPlayer.SetWem(FileResourcer.Get().GetFile<Wem>(fileHash)))
        {
            viewer.MusicPlayer.Play();
            SetExportFunction(ExportWav, (int)ExportTypeFlag.Full);
            viewer.ExportControl.SetExportInfo(fileHash);
        }
    }

    private void ExportSound(ExportInfo info)
    {
        ConfigSubsystem config = TigerInstance.GetSubsystem<ConfigSubsystem>();

        WwiseSound sound = FileResourcer.Get().GetFile<WwiseSound>(info.Hash);
        string saveDirectory = config.GetExportSavePath() + $"/Sound/{(_weaponItemName == null ? "" : $"{_weaponItemName}/")}{info.Hash}_{info.Name}/";
        Directory.CreateDirectory(saveDirectory);
        sound.ExportSound(saveDirectory);
    }

    private void ExportWav(ExportInfo info)
    {
        // exporting while playing the audio causes a hang
        var viewer = GetViewer();
        Dispatcher.Invoke(() =>
        {
            if (viewer.MusicPlayer.IsPlaying())
                viewer.MusicPlayer.Pause();
        });

        ConfigSubsystem config = TigerInstance.GetSubsystem<ConfigSubsystem>();
        Wem wem = FileResourcer.Get().GetFile<Wem>(info.Hash);
        string saveDirectory = config.GetExportSavePath() + $"/Sound/{info.Hash}_{info.Name}/";
        Directory.CreateDirectory(saveDirectory);
        wem.SaveToFile($"{saveDirectory}/{info.Name}.wav");
    }

    #endregion

    #region Music

    /// <summary>
    /// We assume all music tables come from activities.
    /// </summary>
    private void LoadMusicList(FileHash fileHash)
    {
        Activity activity = FileResourcer.Get().GetFile<Activity>(fileHash);
        _allTagItems = new ConcurrentBag<TagItem>();

        ConcurrentBag<FileHash> musics = new();

        Parallel.ForEach(activity.TagData.Unk50, val =>
        {
            foreach (var d2Class48898080 in val.Unk18)
            {
                var resource = d2Class48898080.UnkEntityReference.TagData.Unk10.GetValue(d2Class48898080.UnkEntityReference.GetReader());
                if (resource is SD5908080 res)
                {
                    if (res.Music != null)
                    {
                        musics.Add(res.Music.Hash);
                    }
                }
                else if (resource is S18978080 res2)
                {
                    if (res2.Unk1C != null)
                    {
                        musics.Add(res2.Unk1C.Hash);
                    }
                }
            }
        });
        if (activity.TagData.Unk18.GetValue(activity.GetReader()) is S6A988080 res)
        {
            if (res.Music != null)
                musics.Add(res.Music.Hash);

            //if (res.Music2 is not null)
            //{
            //    _allTagItems.Add(new TagItem
            //    {
            //        Hash = res.Music2.Hash,
            //        Name = res.Music2.Hash,
            //        TagType = ETagListType.Music,
            //        Extra = res.Music2
            //    });
            //}

            if (res.DescentMusic is not null)
            {
                _allTagItems.Add(new TagItem
                {
                    Hash = res.DescentMusic.Hash,
                    Name = res.DescentMusicPath.Value,
                    TagType = ETagListType.Music,
                    Extra = res.DescentMusic
                });
            }
        }
        if (activity.TagData.Unk18.GetValue(activity.GetReader()) is S20978080 res2)
        {
            if (res2.Music != null)
                musics.Add(res2.Music.Hash);
        }

        Parallel.ForEach(musics.Distinct(), hash =>
        {
            _allTagItems.Add(new TagItem
            {
                Hash = hash,
                Name = hash,
                TagType = ETagListType.Music
            });
        });
    }

    private void LoadMusic(FileHash fileHash, TagItem extra = null)
    {
        var viewer = GetViewer();
        SetViewer(TagView.EViewerType.Music);
        if (extra is not null)
            viewer.MusicControl.Load(fileHash, extra.Extra);
        else
            viewer.MusicControl.Load(fileHash);

        SetExportFunction(viewer.MusicControl.Export, (int)ExportTypeFlag.Full, true);
        viewer.ExportControl.SetExportInfo(fileHash);
    }

    #endregion


    #region Material
    private async Task LoadMaterialList()
    {
        // If there are packages, we don't want to reload the view as very poor for performance.
        if (_allTagItems != null)
            return;

        MainWindow.Progress.SetProgressStages(new List<string>
        {
            "Caching Materials",
            "Adding Materials to UI",
        });

        await Task.Run(() =>
        {
            _allTagItems = new ConcurrentBag<TagItem>();

            ConcurrentHashSet<FileHash> mats = PackageResourcer.Get().GetAllHashes<Material>();
            MainWindow.Progress.CompleteStage();

            // named render global materials
            ConcurrentDictionary<string, FileHash> _added = new();
            var globals = Globals.Get().RenderGlobals;
            Parallel.ForEach(globals.TagData.Pipelines.Enumerate(globals.GetReader()), pipeline =>
            {
                if (pipeline.Technique.IsInvalid())
                    return;

                if (!_added.TryAdd(pipeline.Name, pipeline.Technique))
                    return;

                FileMetadata metadata = pipeline.Technique.GetFileMetadata();
                _allTagItems.Add(new TagItem
                {
                    Hash = pipeline.Technique,
                    Name = $"Pipeline: {pipeline.Name.Value}",
                    Subname = Helpers.GetReadableSize(metadata.Size),
                    TagType = ETagListType.Material
                });
            });

            HashSet<FileHash> remainingVals = new HashSet<FileHash>(mats);
            remainingVals.ExceptWith(_added.Values);

            Parallel.ForEach(remainingVals, val =>
            {
                FileMetadata metadata = val.GetFileMetadata();
                _allTagItems.Add(new TagItem
                {
                    Hash = val,
                    Name = $"Material {metadata.FileIndex}",
                    Subname = $"{Helpers.GetReadableSize(metadata.Size)}",
                    TagType = ETagListType.Material
                });

                //Material mat = FileResourcer.Get().GetFile<Material>(val, shouldCache: false);
                //var matOps = mat.Pixel.GetBytecode();
                //if (matOps.Opcodes.Any(x => x.op == TfxBytecode.Clamp))
                //    Console.WriteLine($"{mat.Hash}");
            });

            MainWindow.Progress.CompleteStage();

            MakePackageTagItems();
        });

        RefreshItemList();  // bc of async stuff
    }

    private void LoadMaterial(FileHash fileHash)
    {
        var materialView = new MaterialView();
        materialView.Load(fileHash);
        _mainWindow.MakeNewTab(fileHash, materialView);
        _mainWindow.SetNewestTabSelected();
    }
    #endregion

    private async void TagImage_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is Image img && img.DataContext is TagItem tag)
        {
            //Console.WriteLine($"Loaded {tag.Hash}");
            img.Tag = tag;
            await tag.LoadTagImageAsync();
        }
    }

    private void TagImage_Unloaded(object sender, RoutedEventArgs e)
    {
        if (sender is Image img && img.Tag is TagItem tag)
        {
            tag.ClearImageSource();
            img.Source = null;
            img.Tag = null;
        }
    }
}

public class TagItem : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged(string propName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));


    private string _name = String.Empty;
    public string Name
    {
        get => _name; set => _name = value;
    }

    private string _subname = String.Empty;
    public string Subname
    {
        get => _subname;
        set
        {
            _subname = value;
            OnPropertyChanged(nameof(Subname));
        }
    }

    public TigerHash Hash { get; set; }

    public string HashString
    {
        get
        {
            if (Name == "BACK")
                return "";

            if (TagType == ETagListType.Package)
                return $"[{(Hash as FileHash).PackageId:X4}]";
            return $"[{Hash:X8}]";
        }
    }

    public int FontSize { get; set; } = 16;

    private string _type = String.Empty;
    public string Type
    {
        get
        {
            if (_type == String.Empty)
            {
                var t = EnumExtensions.GetEnumDescription(TagType);
                if (t.Contains("[Final]"))
                    return t.Split("[Final]")[0].Trim();
                return t;
            }
            return _type;
        }
        set => _type = value;
    }

    public ETagListType TagType { get; set; }

    public dynamic? Extra { get; set; } // This is dumb and should only be used sparingly

    private ImageSource _tagImageSource;
    public ImageSource TagImageSource
    {
        get => _tagImageSource;
        private set
        {
            _tagImageSource = value;
            OnPropertyChanged(nameof(TagImageSource));
        }
    }

    public async Task LoadTagImageAsync()
    {
        if (TagType != ETagListType.Texture || Hash == null || TagImageSource != null)
            return;

        var texture = await Task.Run(() => FileResourcer.Get().GetFileAsync<Texture>(Hash, shouldCache: false));
        if (texture == null)
            return;

        var image = await Task.Run(() => TextureLoader.LoadTexture(texture, 96, 96));

        if (image != null)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                TagImageSource = image;
                // Sets the Subname to add the Textures dimensions, this gets set after the tag is
                // added to _allTagItems so you can't search by its pixel dimensions, which is why
                // GetTextureDimensionsRaw is used in SortItemListByString()
                Subname = $"{texture.GetDimension().GetEnumDescription()} Texture : {texture.Width}x{texture.Height}";
            });
        }
    }

    public void ClearImageSource()
    {
        TagImageSource = null;
    }
}
