using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using ConcurrentCollections;
using Tiger;
using Tiger.Schema;
using static MIDA.APIItemView;

namespace MIDA;

/// <summary>
/// Interaction logic for TextureListView.xaml
/// </summary>
public partial class TextureListView : UserControl
{
    private static MainWindow _mainWindow = null;
    private ConfigSubsystem Config = TigerInstance.GetSubsystem<ConfigSubsystem>();
    private APITooltip ToolTip;

    private ConcurrentBag<PackageItem> PackageItems;
    private ConcurrentBag<TextureItem> Textures = new();

    private int SortByIndex = 4;

    private FileHash _currentDisplayedTexture;

    public TextureListView()
    {
#if DEBUG
        // I can't be asked to fix these seemingly harmless but lag inducing xaml binding errors
        PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Critical;
#endif
        InitializeComponent();
    }

    private void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
        _mainWindow = Window.GetWindow(this) as MainWindow;
        MouseMove += UserControl_MouseMove;

        ToolTip = new();
        Panel.SetZIndex(ToolTip, 50);
        MainContainer.Children.Add(ToolTip);

        //if (ConfigSubsystem.Get().GetAnimatedBackground())
        //{
        //    SpinnerShader _spinner = new SpinnerShader();
        //    Spinner.Effect = _spinner;
        //    SizeChanged += _spinner.OnSizeChanged;
        //    _spinner.ScreenWidth = (float)ActualWidth;
        //    _spinner.ScreenHeight = (float)ActualHeight;
        //    _spinner.Scale = new(0, 0);
        //    _spinner.Offset = new(-3.6, -3.3);
        //    SpinnerContainer.Visibility = Visibility.Visible;
        //}
    }

    public async void LoadContent()
    {
        MainWindow.Progress.SetProgressStages(new List<string>
        {
            "Creating Texture List",
        });
        await MakePackageItems();
        MainWindow.Progress.CompleteStage();

        CreateFilterOptions();
    }


    private void CreateFilterOptions()
    {
        ComboBoxControl presets = new ComboBoxControl();
        presets.Text = "Presets";
        presets.FontSize = 14;
        presets.Combobox.ItemsSource = new List<ComboBoxItem>()
        {
            new ComboBoxItem { Content = "None", Tag = "", FontSize = 10 },
            new ComboBoxItem { Content = "(De)Buff Icons", Tag = "75x75", FontSize = 10 },
            new ComboBoxItem { Content = "Items/Perks", Tag = "96x96", FontSize = 10 },
            new ComboBoxItem { Content = "Ability Icons", Tag = "54x54", FontSize = 10 },
            new ComboBoxItem { Content = "Weapon Icons", Tag = "137x76", FontSize = 10 },
            new ComboBoxItem { Content = "Upsell Screen", Tag = "1920x830", FontSize = 10 },
            new ComboBoxItem { Content = "Cubemap", Tag = "Cubemap", FontSize = 10 },
            new ComboBoxItem { Content = "Volume", Tag = "Volume", FontSize = 10 },
            new ComboBoxItem { Content = "1K", Tag = "1024", FontSize = 10 },
            new ComboBoxItem { Content = "2K", Tag = "2048", FontSize = 10 },
            new ComboBoxItem { Content = "4K", Tag = "4096", FontSize = 10 }

        };
        if (presets.Combobox.SelectedIndex == -1)
        {
            presets.Combobox.SelectedIndex = 0;
        }
        presets.Combobox.MinWidth = 175;
        presets.Combobox.ToolTip = "Based on texture resolutions";
        presets.Combobox.SelectionChanged += Presets_OnSelectionChanged;
        FilterOptions.Children.Add(presets);

        //----------------------------------------------

        ComboBoxControl sortBy = new ComboBoxControl();
        sortBy.Text = "Sort By";
        sortBy.FontSize = 14;
        sortBy.Combobox.ItemsSource = new List<ComboBoxItem>()
        {
            new ComboBoxItem { Content = "Hash ↓", Tag = 4 },
            new ComboBoxItem { Content = "Hash ↑", Tag = 3 },
            new ComboBoxItem { Content = "Size ↓", Tag = 2 },
            new ComboBoxItem { Content = "Size ↑", Tag = 1 }
        };
        if (sortBy.Combobox.SelectedIndex == -1)
        {
            sortBy.Combobox.SelectedIndex = 0;
        }

        sortBy.Combobox.SelectionChanged += SortBy_OnSelectionChanged;
        FilterOptions.Children.Add(sortBy);
    }

    private async Task MakePackageItems()
    {
        if (PackageItems != null)
            return;

        await Task.Run(() =>
        {
            PackageItems = new();
            ConcurrentDictionary<int, ConcurrentHashSet<FileHash>> packageIds = new();
            var hashes = PackageResourcer.Get().GetAllHashes<Texture>();

            foreach (var item in hashes)
            {
                if (packageIds.ContainsKey(item.PackageId))
                    packageIds[item.PackageId].Add(item);
                else
                    packageIds[item.PackageId] = new();
            }

            Parallel.ForEach(packageIds, pkgId =>
            {
                if (pkgId.Value.Count == 0)
                    return;

                var name = string.Join('_', PackageResourcer.Get().GetPackage((ushort)pkgId.Key).GetPackageMetadata().Name.Split('_').Skip(1).SkipLast(1));
                PackageItems.Add(new PackageItem
                {
                    Name = name,
                    ID = pkgId.Key,
                    Count = pkgId.Value.Count,
                    Hashes = pkgId.Value
                });
            });
        });

        //PackageList.ItemsSource = PackageItems.OrderBy(x => x.Name);
        RefreshPackageList();
    }

    private async void PackageItem_Checked(object sender, RoutedEventArgs e)
    {
        var btn = sender as ToggleButton;
        if (btn is null)
            return;

        PackageItem item = ((ToggleButton)sender).DataContext as PackageItem;
        await LoadTextureList(item);
    }

    private async Task LoadTextureList(PackageItem item)
    {
        if (Textures.Count != 0)
            Textures.Clear();

        await Task.Run(() => Parallel.ForEachAsync(item.Hashes, async (hash, ct) =>
        {
            // Get the textures dimensions directly from the raw data but only if we're loading from a parent pkg.
            // Adds a slight delay to loading but allows searching by dimensions
            var dims = Helpers.GetTextureDimensionsRaw(hash);
            string dims_str = $"{dims.width}x{dims.height}";

            Textures.Add(new()
            {
                Hash = hash,
                Dimensions = dims_str,
                Width = dims.width,
                Height = dims.height,
                Depth = dims.depth,
                ArraySize = dims.array_size
            });
        }));

        TextureList.ItemsSource = Textures.OrderBy(x => x.Hash);
        RefreshTextureList();
    }

    private void RefreshPackageList()
    {
        if (PackageItems == null)
            return;
        if (PackageItems.IsEmpty)
            return;

        var searchStr = SearchBox.Text;

        uint parsedHash = 0;
        bool isHash = Helpers.ParseHash(searchStr, out parsedHash);

        var displayItems = new ConcurrentBag<PackageItem>();
        Parallel.ForEach(PackageItems, pkg =>
        {
            if (isHash && pkg.Hashes.Any(x => x.Hash32 == parsedHash)) // hacky but eh
            {
                var hashes = pkg.Hashes.Where(x => x.Hash32 == parsedHash);
                displayItems.Add(new PackageItem
                {
                    Name = pkg.Name,
                    ID = pkg.ID,
                    Count = hashes.Count(),
                    Hashes = new(hashes)
                });
            }
            else if (pkg.Name.Contains(searchStr, StringComparison.OrdinalIgnoreCase))
            {
                displayItems.Add(pkg);
            }
        });

        List<PackageItem> items = displayItems.OrderBy(x => x.Name).ToList();
        PackageList.ItemsSource = items;
    }

    private void RefreshTextureList()
    {
        if (Textures == null)
            return;
        if (Textures.IsEmpty)
            return;

        var searchStr = TextureSearchBox.Text;

        uint parsedHash = 0;
        bool isHash = Helpers.ParseHash(searchStr, out parsedHash);

        var displayItems = new ConcurrentBag<TextureItem>();
        Parallel.ForEach(Textures, tex =>
        {
            if (isHash && tex.Hash.Hash32 == parsedHash) // hacky but eh
            {
                displayItems.Add(tex);
            }
            else if ((searchStr == "Cubemap" && tex.ArraySize == 6) || (searchStr == "Volume" && tex.Depth > 1)) // also dumb
            {
                displayItems.Add(tex);
            }
            else if (tex.Dimensions.Contains(searchStr, StringComparison.OrdinalIgnoreCase))
            {
                displayItems.Add(tex);
            }
        });

        List<TextureItem> items = displayItems.ToList();

        items = SortByIndex switch
        {
            4 => items.OrderBy(x => x.Hash).ToList(),
            3 => items.OrderByDescending(x => x.Hash).ToList(),
            2 => items.OrderBy(x => x.Width * x.Height).ToList(),
            1 => items.OrderByDescending(x => x.Width * x.Height).ToList(),
            _ => items
        };

        TextureList.ItemsSource = items;

        BulkExportButton.IsEnabled = items.Count > 0;
    }

    private void Texture_OnClick(object sender, RoutedEventArgs e)
    {
        if ((sender as Button) is null)
            return;

        TextureItem item = ((Button)sender).DataContext as TextureItem;
        LoadTexture(item.Hash);
    }

    private void LoadTexture(FileHash fileHash)
    {
        Texture textureHeader = FileResourcer.Get().GetFile<Texture>(fileHash);
        _currentDisplayedTexture = fileHash;

        TextureControl.Visibility = textureHeader.IsCubemap() ? Visibility.Hidden : Visibility.Visible;
        CubemapControl.Visibility = !textureHeader.IsCubemap() ? Visibility.Hidden : Visibility.Visible;
        ExportButton.IsEnabled = true;

        if (textureHeader.IsCubemap())
        {
            CubemapControl.LoadCubemap(textureHeader);
        }
        else
        {
            TextureControl.CurrentSlice = 0;
            TextureControl.LoadTexture(textureHeader);
        }
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        RefreshPackageList();
    }

    private void TextureSearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        RefreshTextureList();
    }

    private void SortBy_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SortByIndex = (int)((sender as ComboBox).SelectedItem as ComboBoxItem).Tag;
        RefreshTextureList();
    }

    private void Presets_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var preset = (string)((sender as ComboBox).SelectedItem as ComboBoxItem).Tag;
        TextureSearchBox.Text = preset;
    }

    private async void BulkExportButton_Click(object sender, RoutedEventArgs e)
    {
        var items = TextureList.ItemsSource as IEnumerable<TextureItem>;
        if (items is null || items.Count() == 0)
            return;

        // Hopefully this works fine, and not just for me
        MainWindow.Progress.SetProgressStages(items.Select((x, i) => $"Exporting {i + 1}/{items.Count()}: {x.Hash}").ToList());
        await Task.Run(() =>
        {
            Parallel.ForEach(items, item =>
            {
                TextureExtractor.ExportTexture(item.Hash);
                MainWindow.Progress.CompleteStage();
            });
        });

        string pkgName = PackageResourcer.Get().GetPackage(items.First().Hash.PackageId).GetPackageMetadata().Name.Split(".")[0];
        string savePath = Config.GetExportSavePath() + $"/Textures/{pkgName}";
        NotificationBanner notify = new()
        {
            Icon = "☑️",
            Title = "Bulk Export Complete",
            Description = $"Exported {items.Count()} textures to \"{savePath}\"",
            Style = NotificationBanner.PopupStyle.Information
        };
        notify.Show();
    }

    private void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentDisplayedTexture is null)
            return;

        var hash = _currentDisplayedTexture;
        TextureControl.ExportCurrent();

        string pkgName = PackageResourcer.Get().GetPackage(hash.PackageId).GetPackageMetadata().Name.Split(".")[0];
        string savePath = Config.GetExportSavePath() + $"/Textures/{pkgName}";
        NotificationBanner notify = new()
        {
            Icon = "☑️",
            Title = "Export Complete",
            Description = $"Exported {hash} to \"{savePath}\"",
            Style = NotificationBanner.PopupStyle.Information
        };
        notify.Show();
    }

    private void ExportButtons_MouseEnter(object sender, MouseEventArgs e)
    {
        if (!(sender as Button).IsEnabled)
            return;

        ToolTip.ActiveItem = (sender as Button);
        string[] text = (sender as Button).Tag.ToString().Split(":");

        PlugItem plugItem = new()
        {
            Name = $"{text[0]}",
            Description = $"{text[1]}",
            PlugRarityColor = MarathonTierType.Standard.GetColor(),
        };

        ToolTip.MakeTooltip(plugItem);
    }

    public void ExportButtons_MouseLeave(object sender, MouseEventArgs e)
    {
        ToolTip.ClearTooltip();
        ToolTip.ActiveItem = null;
    }

    private void UserControl_MouseMove(object sender, MouseEventArgs e)
    {
        // Currently causing cubemap viewer to not update with everything else
        //System.Windows.Point position = e.GetPosition(this);
        //TranslateTransform gridTransform = (TranslateTransform)MainContainer.RenderTransform;
        //gridTransform.X = position.X * -0.0075;
        //gridTransform.Y = position.Y * -0.0075;
    }


    private async void TagImage_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is Image img && img.DataContext is TextureItem tag)
        {
            //Console.WriteLine($"Loaded {tag.Hash}");
            await tag.LoadTagImageAsync();
            img.Tag = tag;
        }
    }

    private void TagImage_Unloaded(object sender, RoutedEventArgs e)
    {
        if (sender is Image img && img.Tag is TextureItem tag)
        {
            tag.ClearImageSource();
            //img.Source = null;
        }
    }

    private class TextureItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        public FileHash Hash { get; set; }
        public string Dimensions { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }
        public int Depth { get; set; }
        public int ArraySize { get; set; }

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
            if (Hash == null || TagImageSource != null)
                return;

            var texture = await FileResourcer.Get().GetFileAsync<Texture>(Hash, shouldCache: true);
            if (texture == null)
                return;

            var image = await Task.Run(() => TextureLoader.LoadTexture(texture, 96, 96));

            if (image != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    TagImageSource = image;
                });
            }
        }

        public void ClearImageSource()
        {
            TagImageSource = null;
        }
    }

    private class PackageItem
    {
        public string Name { get; set; }
        public int ID { get; set; }
        public int Count { get; set; }
        public ConcurrentHashSet<FileHash> Hashes { get; set; }
        public bool IsSelected { get; set; } = false;
    }
}


