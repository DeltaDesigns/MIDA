using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Arithmic;
using Tiger;
using Tiger.Schema.Investment;

namespace MIDA;

/// <summary>
/// Interaction logic for DareView2.xaml
/// </summary>
public partial class GearView : UserControl, INotifyPropertyChanged
{
    public ConcurrentDictionary<MarathonTraitID, List<InventoryItem>> SortedItems { get; set; } = new();

    private ObservableCollection<Dare_ItemCategory> _itemCategories = new();
    public ObservableCollection<Dare_ItemCategory> ItemCategories
    {
        get => _itemCategories;
        set
        {
            if (_itemCategories != value)
            {
                _itemCategories = value;
                OnPropertyChanged(nameof(ItemCategories));
            }
        }
    }

    private ObservableCollection<GearViewItem> _selectedItems = new();
    public ObservableCollection<GearViewItem> SelectedItems
    {
        get => _selectedItems;
        set
        {
            if (_selectedItems != value)
            {
                _selectedItems = value;
                OnPropertyChanged(nameof(SelectedItems));
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged(string propName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }

    private MarathonTraitID? TypeFilter = null;
    private MarathonTierType? RarityFilter = null;
    private MarathonTraitID? ReleaseFilter = null;

    // Attachment related
    private Dictionary<uint, WeaponAttachmentSelection> _cachedAttachments = new();

    public GearView()
    {
        //#if DEBUG
        PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Critical;
        //#endif

        InitializeComponent();
        Categories.CustomNextButton = NextPage;
        Categories.CustomPrevButton = PreviousPage;
        SelectedItemsList.Items = SelectedItems;

        // By default, DisplayItems only gets called if the whole collection is reassigned
        // These trigger if something in the collection changes (add/remove), which will call DisplayItems.
        SelectedItems.CollectionChanged += (s, e) => SelectedItemsList.DisplayItems();
        ItemCategories.CollectionChanged += (s, e) => Categories.DisplayItems();
    }

    private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        this.DataContext = this;
        Focusable = true;
        Focus();
    }

    public async void LoadContent()
    {
        List<string> loading = new() { "Loading API Items" };
        MainWindow.Progress.SetProgressStages(loading, false, true);

        await LoadApiList();
        CreateFilterOptions();
    }

    private void CreateFilterOptions()
    {
        int boxWidth = 300;

        List<ComboBoxItem> types = new();
        ComboBoxControl presets = new();
        presets.Text = "Type";
        presets.FontSize = 14;
        foreach (var type in SortedItems.Keys)//.Where(x => x.ToString().StartsWith("_")))
        {
            types.Add(new()
            {
                Content = type.GetTraitName(),
                Tag = type,
                FontSize = 10
            });
        }

        types = types
            .OrderBy(x => ((MarathonTraitID)x.Tag).GetTraitType())
            .ThenBy(x => ((MarathonTraitID)x.Tag).ToString().StartsWith("_") ? 1 : 0)
            .ThenBy(x => ((MarathonTraitID)x.Tag).GetTraitName()).ToList();

        types.Insert(0, new() { Content = "All", FontSize = 10 });
        presets.Combobox.ItemsSource = types;

        if (presets.Combobox.SelectedIndex == -1)
        {
            presets.Combobox.SelectedIndex = 0;
        }
        presets.Combobox.MinWidth = boxWidth;
        presets.Combobox.SelectionChanged += Filters_OnSelectionChanged;
        FilterOptions.Children.Add(presets);

        //--------------------------------------------

        List<ComboBoxItem> rarities = new();
        ComboBoxControl rarity_presets = new();
        rarity_presets.Text = "Rarity";
        rarity_presets.FontSize = 14;

        var values = Enum.GetValues(typeof(MarathonTierType)).Cast<MarathonTierType>().ToList();
        foreach (var rarity in values.Where(x => x != MarathonTierType.None))
        {
            rarities.Add(new()
            {
                Content = rarity.GetEnumDescription(),
                Tag = rarity,
                FontSize = 10
            });
        }

        rarities.Insert(0, new() { Content = "All", FontSize = 10 });
        rarity_presets.Combobox.ItemsSource = rarities;

        if (rarity_presets.Combobox.SelectedIndex == -1)
        {
            rarity_presets.Combobox.SelectedIndex = 0;
        }
        rarity_presets.Combobox.MinWidth = boxWidth;
        rarity_presets.Combobox.SelectionChanged += RarityFilters_OnSelectionChanged;
        FilterOptions.Children.Add(rarity_presets);

        //--------------------------------------------

        // TODO?
        //List<ComboBoxItem> releases = new();
        //ComboBoxControl release_presets = new();
        //release_presets.Text = "Release";
        //release_presets.FontSize = 14;

        //foreach (var type in SortedItems.Keys.Where(x => x.ToString().Contains("releases")))
        //{
        //    releases.Add(new()
        //    {
        //        Content = type.GetTraitName(),
        //        Tag = type,
        //        FontSize = 10
        //    });
        //}

        //releases = releases.OrderBy(x => ((MarathonTraitID)x.Tag).ToString().Split("releases_v")[1].Split("_")[0]).ToList();
        //releases.Insert(0, new() { Content = "All", FontSize = 10 });
        //release_presets.Combobox.ItemsSource = releases;

        //if (release_presets.Combobox.SelectedIndex == -1)
        //{
        //    release_presets.Combobox.SelectedIndex = 0;
        //}
        //release_presets.Combobox.MinWidth = boxWidth;
        //release_presets.Combobox.SelectionChanged += ReleaseFilters_OnSelectionChanged;
        //FilterOptions.Children.Add(release_presets);
    }

    private async Task LoadApiList()
    {
        ItemCategories.Clear();
        IEnumerable<InventoryItem> inventoryItems = await Investment.Get().GetInventoryItems();
        MainWindow.Progress.CompleteStage();

        List<string> mapStages = inventoryItems.Select((_, i) => $"Loading {i + 1}/{inventoryItems.Count()}").ToList();
        MainWindow.Progress.SetProgressStages(mapStages, false, true);

        await Parallel.ForEachAsync(inventoryItems, async (item, ct) =>
        {
            if (ShouldAddToList(item) && item.Name != string.Empty)
            {
                if (!item.ItemTraits.Any() || item.ItemTraits.Contains(MarathonTraitID.item_other))
                {
                    if (!SortedItems.ContainsKey(MarathonTraitID.item_other))
                        SortedItems[MarathonTraitID.item_other] = new List<InventoryItem>();

                    SortedItems[MarathonTraitID.item_other].Add(item);
                }

                foreach (var trait in item.ItemTraits)
                {
                    var _trait = trait;
                    if (!SortedItems.ContainsKey(_trait))
                        SortedItems[_trait] = new List<InventoryItem>();

                    SortedItems[_trait].Add(item);
                }
            }
            MainWindow.Progress.CompleteStage();
        });

        foreach ((var trait, var items) in SortedItems.OrderByDescending(x => x.Key.GetTraitName()).Where(x => x.Value.Count != 0))
        {
            ItemCategories.Add(new Dare_ItemCategory
            {
                CategoryName = trait.GetTraitName(),
                CategoryType = trait,
                ItemsPerPage = 8,
                Items = new ObservableCollection<GearViewItem>(
                    items.DistinctBy(x => x.ApiHash)
                    .Select(x => new GearViewItem(x))
                    .OrderByDescending(x => x.Item.GetItemIndex())
                    .OrderByDescending(x => x.Item.GetItemRarity()))

            });
        }
        Categories.Items = ItemCategories
            .OrderBy(x => x.CategoryType.GetTraitType())
            .ThenBy(x => x.CategoryType.ToString().StartsWith("_") ? 1 : 0)
            .ThenBy(x => x.CategoryType.GetTraitName());
    }

    private void RefreshItemList()
    {
        if (ItemCategories is null || ItemCategories.Count == 0)
            return;

        List<Dare_ItemCategory> curItems = new(ItemCategories.ToList());
        List<Dare_ItemCategory> itemCategories = new();
        string searchStr = SearchBox.Text;

        foreach (var item in curItems)
        {
            if (TypeFilter is not null && item.CategoryType != TypeFilter)
                continue;

            Dare_ItemCategory newItem = new()
            {
                CategoryName = item.CategoryName,
                CategoryType = item.CategoryType
            };

            if (TypeFilter is not null) // if we're filtering by type, display more items since there will be only one category
            {
                newItem.ItemsPerPage = 24;
                newItem.Columns = 3;
                Categories.Columns = 1;
            }
            else // meh, dont like doing this
            {
                Categories.Columns = 3;
            }

            if (searchStr is not null && searchStr != string.Empty)
            {
                newItem.Items = new ObservableCollection<GearViewItem>(item.Items
                .Where(x => x.Item.Name.Contains(searchStr, StringComparison.InvariantCultureIgnoreCase)
                            || x.Item.Type.Contains(searchStr, StringComparison.InvariantCultureIgnoreCase)
                            //|| x.Item.Parent?.GetItemName().Contains(searchStr, StringComparison.InvariantCultureIgnoreCase) == true
                            || $"{x.Hash}" == searchStr));
            }
            else
                newItem.Items = item.Items;

            if (RarityFilter is not null)
                newItem.Items = new ObservableCollection<GearViewItem>(newItem.Items.Where(x => x.Item.GetItemRarity() == RarityFilter));

            if (ReleaseFilter is not null)
                newItem.Items = new ObservableCollection<GearViewItem>(newItem.Items.Where(x => x.Item.ItemTraits.Contains(ReleaseFilter.Value)));

            if (newItem.Items.Count != 0)
                itemCategories.Add(newItem);
        }
        if (itemCategories.Count == 1)
        {
            itemCategories.First().ItemsPerPage = 24;
            itemCategories.First().Columns = 3;
            Categories.Columns = 1;
        }
        else // meh, dont like doing this
        {
            Categories.Columns = 3;
        }

        Categories.Items = itemCategories
            .OrderBy(x => x.CategoryType.GetTraitType())
            .ThenBy(x => x.CategoryType.ToString().StartsWith("_") ? 1 : 0)
            .ThenBy(x => x.CategoryType.GetTraitName());
    }

    private void SelectedDareEntry_Click(object sender, RoutedEventArgs e)
    {
        var element = (sender as FrameworkElement);
        GearViewItem apiItem = element.DataContext as GearViewItem;
        if (SelectedItems.Contains(apiItem) && !apiItem.IsNewlyAdded) // Using IsNewlyAdded just to stop multi-clicking 
        {
            apiItem.IsNewlyAdded = true;
            apiItem.WeaponAttachments?.ClearAll();

            UIHelper.AnimateSlide((UIElement)UIHelper.GetParentAtDepth(element, 2),
                0.1f, new(25, 0), new(0, 0), easing: new QuadraticEase { EasingMode = EasingMode.EaseInOut });

            UIHelper.AnimateFade((UIElement)UIHelper.GetParentAtDepth(element, 2), 0.1f, 0f, 1f, completed: async (s, e) =>
            {
                await Task.Delay(100);
                SelectedItems.Remove(apiItem);
            });
        }
    }

    private void DareEntry_Click(object sender, RoutedEventArgs e)
    {
        GearViewItem apiItem = (sender as FrameworkElement).DataContext as GearViewItem;
        if (!SelectedItems.Contains(apiItem))
        {
            apiItem.IsNewlyAdded = true;
            var invItem = apiItem.Item.Parent ?? apiItem.Item;
            if (apiItem.Item.IsWeapon)
            {
                if (_cachedAttachments.TryGetValue(apiItem.Hash, out var attachments))
                {
                    apiItem.WeaponAttachments = attachments;
                }
                else
                {
                    apiItem.WeaponAttachments = WeaponAttachmentSelection.GetWeaponAttachments(invItem);
                    _cachedAttachments.TryAdd(apiItem.Hash, apiItem.WeaponAttachments);
                }
            }

            SelectedItems.Add(apiItem);
            Log.Debug($"{apiItem.Item.Name} : {apiItem.Item.GetItemRarity()} ({apiItem.Item.ApiHash}, index {apiItem.Item.GetItemIndex()})) : Item {apiItem.Item.Hash}, Strings {apiItem.Item.GetItemStrings().Hash}, Icon {Investment.Get().GetItemIconContainer(apiItem.Item)?.Hash}");

            //if (apiItem.Item.TagData.UnkE0.GetValue(apiItem.Item.GetReader()) is S808092A5 sockets)
            //{
            //    foreach (var socket in sockets.SocketEntries)
            //    {
            //        var entry = Investment.Get().GetSocketType(socket.SocketTypeIndex);
            //        Console.WriteLine($"Socket Type Hash: {entry.SocketTypeHash.Reverse()}");
            //        Console.WriteLine($"Socket Category Hash: {entry.SocketCategoryHash.Reverse()}");
            //        Console.WriteLine($"Socket Subcategory Hash: {entry.SocketSubcategoryHash.Reverse()}");

            //        foreach (var item in entry.PlugWhitelists)
            //        {
            //            var socketItem = Investment.Get().GetInventoryItem(item.ItemIndex);
            //            Console.WriteLine($"\t-{socketItem.Name} : {socketItem.Rarity} : {socketItem.Hash}");
            //        }

            //        //foreach (var item in entry.Unk28)
            //        //{
            //        //    var socketItem = Investment.Get().GetInventoryItem(item.ItemIndex);
            //        //    Console.WriteLine($"\t-Unk28 {socketItem.Name} : {socketItem.Rarity} : {socketItem.Hash}");
            //        //}

            //        foreach (var item in socket.Unk08)
            //        {
            //            var socketItem = Investment.Get().GetInventoryItem(item.ItemIndex);
            //            Console.WriteLine($"\t-Unk08 {socketItem.Name} : {socketItem.Rarity} : {socketItem.Hash}");
            //        }
            //    }
            //}
        }
    }

    private void AttachmentItem_Checked(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is not GearViewItem)
            return;

        GearViewItem item = (GearViewItem)(sender as FrameworkElement).DataContext;
        if (item.IsSelected)
            item.ParentAttachmentEntry.SelectedAttachment = item;
    }

    private void AttachmentItem_Uncheck(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is not WeaponAttachmentEntry)
            return;

        WeaponAttachmentEntry item = (WeaponAttachmentEntry)(sender as FrameworkElement).DataContext;
        foreach (var att in item.Attachments)
        {
            att.ParentAttachmentEntry.SelectedAttachment = null;
            att.IsSelected = false;
        }
    }

    private void DareSelectedEntry_Loaded(object sender, RoutedEventArgs e)
    {
        var element = (sender as FrameworkElement);
        GearViewItem apiItem = element.DataContext as GearViewItem;
        if (apiItem is null)
            return;

        if (apiItem.IsNewlyAdded)
        {
            UIHelper.AnimateSlide(element, 0.15f, new(0, 0), new(-15, 0));
            UIHelper.AnimateFade(element, 0.15f, completed: (s, e) =>
            {
                apiItem.IsNewlyAdded = false; // reset newly added state when the item is loaded
            });
        }
    }

    private void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        bool exported = true;
        if (SelectedItems.Count == 0)
        {
            NotificationBanner notify = new()
            {
                Icon = "⚠",
                Title = "NOTHING TO EXPORT!",
                Description = $"Select some items to export, you silly goose!",
                Style = NotificationBanner.PopupStyle.Warning
            };
            notify.Show();
            return;
        }

        List<string> apiStages = SelectedItems.Select((_, i) => $"Exporting {SelectedItems[i].Item.Name} ({i + 1}/{SelectedItems.Count})").ToList();
        ConfigSubsystem config = TigerInstance.GetSubsystem<ConfigSubsystem>();
        string savePath = config.GetExportSavePath();
        bool aggregateOutput = (bool)AggregateOutputButton.IsChecked;

        if (aggregateOutput)// && SelectedItems.Any(x => !x.Item.IsShader))
            savePath = CreateNextOutputFolder(config.GetExportSavePath());

        MainWindow.Progress.SetProgressStages(apiStages);
        Task.Run(() =>
        {
            var dupNames = new HashSet<string>();
            SelectedItems.ToList().ForEach(item =>
            {
                var curItem = item.Item;
                if (!dupNames.Add(curItem.Name))
                    curItem.Name += $" {curItem.ApiHash}";

                if (curItem.GetArtArrangementIndex() != -1 || curItem.GetPatternIndex() != -1)
                {
                    Dictionary<MarathonAttachmentType, InventoryItem> attachments = null;
                    if (item.WeaponAttachments is not null && item.WeaponAttachments.Entries.Count != 0)
                    {
                        attachments = new();
                        foreach (var att in item.WeaponAttachments.Entries)
                        {
                            if (att.SelectedAttachment is null)
                                continue;

                            attachments.TryAdd(att.AttachmentType, att.SelectedAttachment.Item);
                        }
                    }
                    // if has a model
                    Investment.ExportInventoryItem(curItem, savePath, aggregateOutput, attachments);
                }
                //else if (curItem.IsShader)
                //{
                //    // shader
                //    string itemName = Helpers.SanitizeString(curItem.Name);
                //    string savePath = config.GetExportSavePath(); // need to set again here
                //    savePath += $"/{itemName}";
                //    Directory.CreateDirectory(savePath);
                //    Directory.CreateDirectory(savePath + "/Textures");
                //    Investment.Get().ExportShader(curItem, savePath, itemName, config.GetOutputTextureFormat());
                //}
                else
                {
                    Log.Error($"Can't export item '{curItem.Name}' because it doesn't have a 3D model.");
                    exported = false;
                }
                MainWindow.Progress.CompleteStage();
            });

            Dispatcher.Invoke(() =>
            {
                NotificationBanner notify = new()
                {
                    Icon = "☑️",
                    Title = "EXPORT " + (exported ? "COMPLETE" : "FAILED"),
                    Description = exported ? $"Exported " +
                    $"{(SelectedItems.Count == 1 ? $"{SelectedItems.First().Item.Name}" : $"{SelectedItems.Count} items")}" +
                    $" to \"{config.GetExportSavePath()}\""
                    : $"Can't export item '{SelectedItems.First().Item.Name}' because it doesn't have a 3D model.",
                    Style = exported ? NotificationBanner.PopupStyle.Information : NotificationBanner.PopupStyle.Warning
                };
                notify.Show();
            });
        });
    }

    private void OpenOutputFolder_Click(object sender, RoutedEventArgs e)
    {
        ConfigSubsystem config = TigerInstance.GetSubsystem<ConfigSubsystem>();
        Process.Start("explorer.exe", config.GetExportSavePath());
    }


    private bool _isClearing = false;
    private async void ClearAllButton_Click(object sender, RoutedEventArgs e)
    {
        if (_isClearing) return;
        _isClearing = true;

        var items = SelectedItemsList.CurrentPageItems.Where(x => !x.IsPlaceholder).ToList();
        foreach (var item in items)
        {
            var element = SelectedItemsList.ItemList.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
            if (!item.IsNewlyAdded) // Using IsNewlyAdded just to stop multi-clicking 
            {
                item.IsNewlyAdded = false;
                ((GearViewItem)item).WeaponAttachments?.ClearAll();

                UIHelper.AnimateSlide(element, 0.1f, new(25, 0), new(0, 0), easing: new QuadraticEase { EasingMode = EasingMode.EaseInOut });

                UIHelper.AnimateFade(element, 0.1f, 0f, 1f);
            }
            await Task.Delay(50);
        }
        SelectedItems.Clear();
        _isClearing = false;
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        RefreshItemList();
    }

    private void Filters_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (((sender as ComboBox).SelectedItem as ComboBoxItem).Tag is not null)
            TypeFilter = (MarathonTraitID)((sender as ComboBox).SelectedItem as ComboBoxItem).Tag;
        else
            TypeFilter = null;

        RefreshItemList();
    }

    private void RarityFilters_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (((sender as ComboBox).SelectedItem as ComboBoxItem).Tag is not null)
            RarityFilter = (MarathonTierType)((sender as ComboBox).SelectedItem as ComboBoxItem).Tag;
        else
            RarityFilter = null;

        RefreshItemList();
    }

    private void ReleaseFilters_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (((sender as ComboBox).SelectedItem as ComboBoxItem).Tag is not null)
            ReleaseFilter = (MarathonTraitID)((sender as ComboBox).SelectedItem as ComboBoxItem).Tag;
        else
            ReleaseFilter = null;

        RefreshItemList();
    }

    private void UserControl_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Down)
        {
            Categories.SelectNextPage();
        }

        if (e.Key == Key.Up)
        {
            Categories.SelectPreviousPage();
        }
    }

    public static bool ShouldAddToList(InventoryItem item)
    {
        return item.GetArtArrangementIndex() != -1 || item.GetPatternIndex() != -1;

        //MarathonTraitID[] blacklist = new[]
        //{
        //    MarathonTraitID.item_ghost_hologram,
        //    MarathonTraitID.item_emote,
        //    MarathonTraitID.item_finisher,
        //};

        //MarathonTraitID[] whitelist = new[]
        //{
        //    // TODO: Add emotes and ghost projections for fx mesh exporting
        //    MarathonTraitID.item_shader,
        //};

        //if (item.ItemTraits.Any(trait => blacklist.Contains(trait)))
        //    return false;

        //return
        //    item.GetArtArrangementIndex() != -1;
        //|| item.ItemTraits.Any(trait => whitelist.Contains(trait));
    }

    // For aggregated outputs
    public static string CreateNextOutputFolder(string baseDirectory)
    {
        // Get all subdirectories that match the "Output#" pattern
        string[] existingFolders = Directory.GetDirectories(baseDirectory, "ApiOutput*");
        int maxNumber = 0;

        // Regex to capture the numeric part of "Output#"
        Regex regex = new(@"ApiOutput(\d+)$");

        foreach (string folder in existingFolders)
        {
            Match match = regex.Match(Path.GetFileName(folder));
            if (match.Success)
            {
                // Parse the number from the folder name
                int folderNumber = int.Parse(match.Groups[1].Value);
                if (folderNumber > maxNumber)
                {
                    maxNumber = folderNumber;
                }
            }
        }

        // Increment the max number to get the next available folder
        int nextNumber = maxNumber + 1;
        string newFolderName = $"ApiOutput{nextNumber}";
        string newFolderPath = Path.Combine(baseDirectory, newFolderName);

        // Create the new directory
        Directory.CreateDirectory(newFolderPath);

        return newFolderPath;
    }

    private void HelpButton_Click(object sender, RoutedEventArgs e)
    {
        PopupBanner about = new()
        {
            DarkenBackground = true,
            //Icon = "❓",
            Title = $"MARATHON GEAR VIEWER (WIP)",
            Subtitle = "Here you can export runner shells, weapons, and practically any item you can pick up in the world.",
            Description =
            "• Use the search bar to look for specific items and/or the drop downs to filter them." +
            "\n• Clicking an items icon will add it to the export list on the right side." +
            "\n• You can Shift+Click page arrows to skip to the start/end of a category, or Ctrl+Click to skip 1/4." +
            "\n• Holding Shift before hovering over an item will show its hash next to its type.",
            Style = PopupBanner.PopupStyle.Information
        };
        about.Show();
    }

    private void AttachmentItem_MouseEnter(object sender, MouseEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is WeaponAttachmentEntry a && a.SelectedAttachment is not null)
        {
            var tooltip = MainWindow.Current.ToolTip;
            tooltip.ActiveItem = sender;
            tooltip.MakeTooltip(a.SelectedAttachment.Item);
        }
    }

    private void AttachmentItem_MouseLeave(object sender, MouseEventArgs e)
    {
        var tooltip = MainWindow.Current.ToolTip;
        tooltip.ActiveItem = null;
        tooltip.ClearTooltip();
    }

    public class Dare_ItemCategory : MIDAUIElement
    {
        private string _categoryName;
        public string CategoryName
        {
            get => _categoryName;
            set
            {
                if (_categoryName != value)
                {
                    _categoryName = value;
                    OnPropertyChanged(nameof(CategoryName));
                }
            }
        }

        private MarathonTraitID _categoryType;
        public MarathonTraitID CategoryType
        {
            get => _categoryType;
            set
            {
                if (_categoryType != value)
                {
                    _categoryType = value;
                    OnPropertyChanged(nameof(CategoryType));
                }
            }
        }

        private ObservableCollection<GearViewItem> _items = new();
        public ObservableCollection<GearViewItem> Items
        {
            get => _items;
            set
            {
                if (_items != value)
                {
                    _items = value;
                    OnPropertyChanged(nameof(Items));
                }
            }
        }

        private int _itemsPerPage = 8;
        public int ItemsPerPage
        {
            get => _itemsPerPage;
            set
            {
                if (_itemsPerPage != value)
                {
                    _itemsPerPage = value;
                    OnPropertyChanged(nameof(ItemsPerPage));
                }
            }
        }

        private int _columns = 1;
        public int Columns
        {
            get => _columns;
            set
            {
                if (_columns != value)
                {
                    _columns = value;
                    OnPropertyChanged(nameof(Columns));
                }
            }
        }
    }
}

public class WeaponAttachmentSelection : MIDAUIElement
{
    public WeaponAttachmentSelection() { }
    public List<WeaponAttachmentEntry> Entries { get; set; } = new();
    private static readonly Dictionary<int, (ImageSource normal, ImageSource selected)> _iconCache = new();

    public static WeaponAttachmentSelection GetWeaponAttachments(InventoryItem invItem)
    {
        WeaponAttachmentSelection selectionEntry = new();
        if (invItem.TagData.UnkE0.GetValue(invItem.GetReader()) is S808092A5 sockets)
        {
            int i = 0;
            foreach (var socket in sockets.SocketEntries)
            {
                i++;
                var entry = Investment.Get().GetSocketType(socket.SocketTypeIndex);
                //Console.WriteLine($"Socket Type Hash: {entry.SocketTypeHash.Reverse()} : {entry.SocketTypeHash.Hash32}");
                //Console.WriteLine($"Socket Category Hash: {entry.SocketCategoryHash.Reverse()}: {entry.SocketCategoryHash.Hash32}");
                //Console.WriteLine($"Socket Subcategory Hash: {entry.SocketSubcategoryHash.Reverse()} : {entry.SocketSubcategoryHash.Hash32}");

                WeaponAttachmentEntry attachmentEntry;
                switch ((MarathonAttachmentType)entry.SocketSubcategoryHash.Hash32)
                {
                    case MarathonAttachmentType.Magazine:
                    case MarathonAttachmentType.Grip:
                    case MarathonAttachmentType.Sight:
                    case MarathonAttachmentType.Barrel:
                    case MarathonAttachmentType.Muzzle:
                    case MarathonAttachmentType.Shield:
                    case MarathonAttachmentType.Generator:
                    case MarathonAttachmentType.FoldingStock:
                        attachmentEntry = new((MarathonAttachmentType)entry.SocketSubcategoryHash.Hash32);
                        break;
                    default:
                        continue;
                }

                attachmentEntry.Attachments = new();
                foreach (var item in entry.PlugWhitelists)
                {
                    var socketItem = Investment.Get().GetInventoryItem(item.ItemIndex);
                    var entryItem = new GearViewItem(socketItem);
                    entryItem.ParentAttachmentEntry = attachmentEntry;

                    attachmentEntry.Attachments.Add(entryItem);
                    //Console.WriteLine($"\t-{socketItem.Name} : {socketItem.Rarity}");
                }

                foreach (var item in socket.Unk08)
                {
                    var socketItem = Investment.Get().GetInventoryItem(item.ItemIndex);
                    var entryItem = new GearViewItem(socketItem);
                    entryItem.ParentAttachmentEntry = attachmentEntry;

                    attachmentEntry.Attachments.Add(entryItem);
                    //Console.WriteLine($"\t-Unique? {socketItem.Name} : {socketItem.Rarity}");
                }

                var iconIndex = Investment.Get().SocketCategoryStringThings[socket.SocketTypeIndex].IconIndex;
                if (iconIndex != -1)
                {
                    if (!_iconCache.TryGetValue(iconIndex, out var icons))
                    {
                        var normal = ApiImageUtils.MakeIcon(iconIndex);
                        var selected = ApiImageUtils.MakeIcon(iconIndex, 0, 1);

                        icons = (normal, selected);
                        _iconCache[iconIndex] = icons;
                    }

                    attachmentEntry.EntryIcon = icons.normal;
                    attachmentEntry.EntryIconSelected = icons.selected;
                }

                attachmentEntry.Index = socket.SocketTypeIndex;
                attachmentEntry.Attachments = attachmentEntry.Attachments.DistinctBy(x => x.Item.Name).OrderByDescending(x => x.Item.Rarity).ToList();
                selectionEntry.Entries.Add(attachmentEntry);
            }
        }

        return selectionEntry;
    }

    public void ClearAll()
    {
        foreach (var att in Entries)
            att.ClearSelection();
    }
}

public class WeaponAttachmentEntry : MIDAUIElement
{
    public WeaponAttachmentEntry(MarathonAttachmentType type)
    {
        AttachmentType = type;
        AttachmentTypeString = type.ToString();
    }

    public ImageSource EntryIcon { get; set; }
    public ImageSource EntryIconSelected { get; set; }


    private GearViewItem _selectedAttachment = null;
    public GearViewItem SelectedAttachment
    {
        get => _selectedAttachment;
        set
        {
            if (_selectedAttachment != value)
            {
                _selectedAttachment = value;
                OnPropertyChanged();
            }
        }
    }
    public MarathonAttachmentType AttachmentType { get; set; }
    public string AttachmentTypeString { get; set; }
    public List<GearViewItem> Attachments { get; set; }

    public void ClearSelection()
    {
        SelectedAttachment = null;
        foreach (var att in Attachments)
            att.IsSelected = false;
    }
}

public class GearViewItem : MIDAUIElement
{
    public GearViewItem() { }

    public GearViewItem(InventoryItem item)
    {
        if (!item.IsRunner) // TODO temp
            _iconLoader = new AsyncImageLoader(
                () => ApiImageUtils.MakeItemIconForeground(item),
                OnPropertyChanged,
                nameof(Icon));

        Item = item;
        Hash = item.ApiHash;
    }

    public string OverrideName { get; set; }
    public string OverrideDescription { get; set; }

    public DestinySocketCategoryStyle ParentSocketStyle { get; set; } // meh
    public Color RarityColor => Item?.GetItemRarity().GetColor() ?? Color.FromArgb(0, 0, 0, 0);

    public uint Hash { get; set; }

    private InventoryItem _item = null;
    public InventoryItem Item
    {
        get => _item;
        set
        {
            if (_item == value)
                return;

            _item = value;
            OnPropertyChanged(nameof(Item));
        }
    }

    public WeaponAttachmentSelection WeaponAttachments { get; set; } = new();
    public WeaponAttachmentEntry ParentAttachmentEntry { get; set; }

    protected internal AsyncImageLoader _iconLoader;

    public ImageSource Icon
    {
        get => _iconLoader?.GetImage() ?? null;
        set
        {
            _iconLoader?.SetImage(value);
            OnPropertyChanged(nameof(Icon));
        }
    }
}
