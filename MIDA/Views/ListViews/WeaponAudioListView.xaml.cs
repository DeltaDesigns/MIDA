using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Arithmic;
using NAudio.Wave;
using Restless.WaveForm.Renderer;
using Restless.WaveForm.Settings;
using Tiger;
using Tiger.Schema.Audio;
using Tiger.Schema.Entity;
using Tiger.Schema.Investment;

namespace MIDA;

// All the duplicate code across these views is starting to get out of hand...

public partial class WeaponAudioListView : UserControl
{
    private static SineSettings _sinePreviewSettings = SineSettings.CreatePreview();
    private static SineSettings _sineExportSettings = SineSettings.CreateExport();
    private ConfigSubsystem Config = TigerInstance.GetSubsystem<ConfigSubsystem>();

    public ConcurrentBag<WeaponItem> WeaponItems;
    private ConcurrentBag<WeaponAudioCategory> Sounds = new();

    private WeaponAudioItem _currentSound;
    private WaveStream _currentSoundStream;

    private WeaponItem _currentWeapon;

    public WeaponAudioListView()
    {
        InitializeComponent();
#if DEBUG
        // I can't be asked to fix these seemingly harmless but lag inducing xaml binding errors
        PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Critical;
#endif

    }

    private void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
        MusicPlayer.ProgressBar.ValueChanged -= (s, e) => UpdateWaveformProgress();
        MusicPlayer.ProgressBar.ValueChanged += (s, e) => UpdateWaveformProgress();
    }

    public async void LoadContent()
    {
        MainWindow.Progress.SetProgressStages(new List<string>
        {
            "Creating Audio List",
        });
        await MakeWeaponItems();
        MainWindow.Progress.CompleteStage();
    }

    public async Task MakeWeaponItems()
    {
        if (WeaponItems != null)
            return;

        WeaponItems = new();

        IEnumerable<InventoryItem> inventoryItems = await Investment.Get().GetInventoryItems();
        Parallel.ForEach(inventoryItems, item =>
        {
            if (!item.IsWeapon || (item.IsWeapon && item.Parent != null)) // skip weapon skins
                return;

            string name = item.Name ?? "";
            string type = item.Type ?? "";

            WeaponItems.Add(new WeaponItem
            {
                Hash = item.TagData.InventoryItemHash,
                Name = name,
                Rarity = item.GetItemRarity().ToString(),
                Type = type.Trim(),
            });
        });

        RefreshWeaponList();
    }

    public void RefreshWeaponList()
    {
        if (WeaponItems == null)
            return;
        if (WeaponItems.IsEmpty)
            return;

        string searchStr = SearchBox.Text;
        var displayItems = new ConcurrentBag<WeaponItem>();
        Parallel.ForEach(WeaponItems, item =>
        {
            if (searchStr == item.Hash.Hash32.ToString()
            || item.Name.Contains(searchStr, StringComparison.OrdinalIgnoreCase)
            || item.Rarity.Contains(searchStr, StringComparison.OrdinalIgnoreCase)
            || item.Type.Contains(searchStr, StringComparison.OrdinalIgnoreCase))
                displayItems.Add(item);
        });

        List<WeaponItem> items = displayItems.DistinctBy(x => x.Name).OrderBy(x => x.Name).ToList();
        WeaponListView.ItemsSource = items;
    }

    private void WeaponItem_Checked(object sender, RoutedEventArgs e)
    {
        if ((sender as RadioButton) is null)
            return;

        WeaponItem item = ((RadioButton)sender).DataContext as WeaponItem;
        _currentWeapon = item;
        LoadWeaponAudioList(item.Hash);
    }

    private void RefreshSoundList()
    {
        if (Sounds == null)
            return;
        if (Sounds.IsEmpty)
            return;

        string searchStr = AudioSearchBox.Text;

        uint parsedHash = 0;
        bool isHash = Helpers.ParseHash(searchStr, out parsedHash);

        var displayItems = new ConcurrentBag<WeaponAudioCategory>();
        Parallel.ForEach(Sounds, sound =>
        {
            sound.Sounds = sound.Sounds.OrderBy(x => x.Seconds).ToList();
            displayItems.Add(sound);
        });

        List<WeaponAudioCategory> items = displayItems
            .DistinctBy(x => x.Name)
            .OrderBy(x => x.Name).ToList();
        //List<WeaponAudioCategory> items = displayItems
        //    .DistinctBy(category =>
        //        string.Join("|",
        //            category.Sounds
        //                .OrderBy(s => s.Hash.ToString())
        //                .Select(s => s.Hash.ToString())))
        //    .OrderBy(x => x.Name)
        //    .ToList();

        WeaponAudioEntries.ItemsSource = items;
        if (items.Count > 0)
            WeaponAudioEntries.ScrollIntoView(items[0]);
        BulkExportButton.IsEnabled = items.Count > 0;
    }

    private void Audio_OnClick(object sender, RoutedEventArgs e)
    {
        if ((sender as RadioButton) is null)
            return;

        WeaponAudioItem item = ((RadioButton)sender).DataContext as WeaponAudioItem;
        _currentSound = item;

        LoadSound(item.Hash);
    }

    private void LoadSound(FileHash hash)
    {
        Wem wem = FileResourcer.Get().GetFile<Wem>(hash, true, false);

        if (MusicPlayer.SetWem(wem))
        {
            _currentSoundStream = wem.Clone();
            MusicPlayer.Play();
            DrawWaveform();
            Log.Verbose($"Playing {wem.Hash}");
        }

        ExportButton.IsEnabled = true;
        ExportWaveform.IsEnabled = true;
        //_currentSoundStream?.Dispose();
    }

    private void AudioSearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        RefreshSoundList();
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        RefreshWeaponList();
    }

    private void SortBy_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        RefreshSoundList();
    }

    private void Presets_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        string preset = (string)((sender as ComboBox).SelectedItem as ComboBoxItem).Tag;
        AudioSearchBox.Text = preset;
    }

    private async void BulkExportButton_Click(object sender, RoutedEventArgs e)
    {
        if (WeaponAudioEntries.ItemsSource is not IEnumerable<WeaponAudioCategory> items || !items.Any())
            return;

        string wepName = _currentWeapon.Name;
        string savePath = Config.GetExportSavePath() + $"/Sound/{wepName}/";
        Directory.CreateDirectory(savePath);

        int totalSounds = items.Sum(x => x.Sounds?.Count ?? 0);
        int currentIndex = 0;

        MainWindow.Progress.SetProgressStages(
            items.SelectMany(category => category.Sounds.Select(sound =>
                $"Exporting {++currentIndex}/{totalSounds}: {category.Name}")).ToList()
        );

        await Task.Run(() =>
        {
            foreach (var category in items)
            {
                string savePath = Config.GetExportSavePath() + $"/Sound/{wepName}/{category.Name}/";
                Directory.CreateDirectory(savePath);

                Parallel.ForEach(category.Sounds, item =>
                {
                    Wem wem = FileResourcer.Get().GetFile<Wem>(item.Hash, false, false);
                    wem.SaveToFile($"{savePath}/{wem.GetReferenceHash()}_{wem.Hash}.wav");
                    MainWindow.Progress.CompleteStage();
                });
            }
        });

        NotificationBanner notify = new()
        {
            Icon = "☑️",
            Title = "Bulk Export Complete",
            Description = $"Exported {totalSounds} Sounds to \"{savePath}\"",
            Style = NotificationBanner.PopupStyle.Information
        };
        notify.Show();
    }

    private async void BulkExportCategoryButton_Click(object sender, RoutedEventArgs e)
    {
        var datacontext = (UIHelper.GetParentAtDepth(sender as FrameworkElement, 3) as FrameworkElement).DataContext;
        WeaponAudioCategory category = datacontext as WeaponAudioCategory;
        var items = category.Sounds;
        if (items.Count == 0)
            return;

        string wepName = _currentWeapon.Name;
        string savePath = Config.GetExportSavePath() + $"/Sound/{wepName}/{category.Name}/";
        Directory.CreateDirectory(savePath);

        MainWindow.Progress.SetProgressStages(items.Select((x, i) => $"Exporting {i + 1}/{items.Count()}: {x.Hash}").ToList());
        await Task.Run(() =>
        {
            Parallel.ForEach(items, item =>
            {
                Wem wem = FileResourcer.Get().GetFile<Wem>(item.Hash, false, false);
                wem.SaveToFile($"{savePath}/{wem.GetReferenceHash()}_{wem.Hash}.wav");
                MainWindow.Progress.CompleteStage();
            });
        });

        NotificationBanner notify = new()
        {
            Icon = "☑️",
            Title = "Bulk Export Complete",
            Description = $"Exported {items.Count()} Sounds to \"{savePath}\"",
            Style = NotificationBanner.PopupStyle.Information
        };
        notify.Show();
    }

    private void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentSound is null)
            return;

        Wem wem = FileResourcer.Get().GetFile<Wem>(_currentSound.Hash, false, false);

        string wepName = _currentWeapon.Name;
        string savePath = Config.GetExportSavePath() + $"/Sound/{wepName}/{_currentSound.ParentCategory}/";
        Directory.CreateDirectory(savePath);

        wem.SaveToFile($"{savePath}/{wem.GetReferenceHash()}_{wem.Hash}.wav");

        NotificationBanner notify = new()
        {
            Icon = "☑️",
            Title = "Export Complete",
            Description = $"Exported {wem.Hash} to \"{savePath}\"",
            Style = NotificationBanner.PopupStyle.Information
        };
        notify.Show();
    }

    private void ExportWaveform_Click(object sender, RoutedEventArgs e)
    {
        if (_currentSound is null)
            return;

        Wem wem = FileResourcer.Get().GetFile<Wem>(_currentSound.Hash, false, false);

        string pkgName = PackageResourcer.Get()
            .GetPackage(_currentSound.Hash.PackageId)
            .GetPackageMetadata()
            .Name.Split(".")[0];

        string savePath = Path.Combine(Config.GetExportSavePath(), "Sound", pkgName);
        Directory.CreateDirectory(savePath);

        wem.Load();
        using var stream = _currentSoundStream;
        var wave = WaveFormRenderer.Create(stream, _sineExportSettings);

        // Overlay Right and Left
        using var combined = new Bitmap(wave.ImageLeft.Width, wave.ImageLeft.Height);
        using (var g = Graphics.FromImage(combined))
        {
            g.DrawImage(wave.ImageLeft, 0, 0);
            g.DrawImage(wave.ImageRight, 0, 0);
        }

        string saveFile = Path.Combine(savePath, $"{_currentSound.Hash}_Waveform.png");
        combined.Save(saveFile, ImageFormat.Png);

        new NotificationBanner
        {
            Icon = "☑️",
            Title = "Export Complete",
            Description = $"Exported Waveform to \"{savePath}\"",
            Style = NotificationBanner.PopupStyle.Information
        }.Show();
    }


    #region Audio loading
    private async void LoadWeaponAudioList(TigerHash apiHash)
    {
        if (Sounds.Count != 0)
            Sounds.Clear();

        Entity? val = Investment.Get().GetPatternEntityFromHash(apiHash);
        if (val == null)
            return;

        var resourceUnnamed = (S808040D4)val.PatternAudioUnnamed.TagData.Unk18.GetValue(val.PatternAudioUnnamed.GetReader());
        var resource = (S80804A14)val.PatternAudio.TagData.Unk18.GetValue(val.PatternAudio.GetReader());

        InventoryItem item = Investment.Get().GetInventoryItem(apiHash);
        TigerHash weaponContentGroupHash = Investment.Get().GetWeaponContentGroupHash(item);

        Log.Verbose($"Loading weapon entity audio {val.Hash}, ContentGroupHash {weaponContentGroupHash}");

        // I don't think this is used anymore but will keep just in case
        //if (!resource.PatternAudioGroups.Where(x => x.WeaponContentGroup1Hash == weaponContentGroupHash).Any())
        //{
        //    Log.Verbose($"No PatterAudioGroups with matching Content Group Hash {weaponContentGroupHash}, trying fallback audio");
        //    if (resource.FallbackAudioGroup != null)
        //    {
        //        audioGroup = FileResourcer.Get().GetSchemaTag<S8080AE2E>(resource.FallbackAudioGroup.TagData.EntityData);
        //    }
        //}
        //else
        //{
        //    foreach (S9B318080 entry in resource.PatternAudioGroups)
        //    {
        //        if (entry.WeaponContentGroup1Hash.Equals(weaponContentGroupHash) && entry.AudioGroup != null)
        //        {
        //            audioGroup = FileResourcer.Get().GetSchemaTag<S8080AE2E>(entry.AudioGroup.TagData.EntityData);
        //        }
        //    }
        //}

        // Named
        var audioGroup = resource.Audio;
        if (audioGroup != null)
        {
            foreach (var audio in audioGroup.TagData.Audio)
            {
                foreach (var s in audio.Sounds)
                {
                    WwiseSound categorySounds = FileResourcer.Get().GetFile<WwiseSound>(s.Data);
                    if (categorySounds == null)
                        continue;

                    WeaponAudioCategory category = new()
                    {
                        Name = s.WwiseEventName.Value?.Split("\\").Last().Split(".")[0] ?? "",
                        Sounds = new()
                    };
                    foreach (var sound in categorySounds.TagData.Wems)
                    {
                        if (sound is null)
                            continue;

                        WeaponAudioItem soundItem = new()
                        {
                            ParentCategory = category.Name,
                            Hash = sound.Hash,
                            DisplayHash = $"[{sound.Hash}]"
                        };
                        await soundItem.LoadWEMAsync();

                        category.Sounds.Add(soundItem);
                    }

                    Sounds.Add(category);
                }
            }
        }

        // Unnamed
        List<Entity> entities = new()
        {
            resourceUnnamed.Entity1,
            resourceUnnamed.Entity2,
            resourceUnnamed.Entity3,
            resourceUnnamed.Entity4,
            resourceUnnamed.Entity5,
            resourceUnnamed.Entity6,
            resourceUnnamed.Entity7,
            resourceUnnamed.Entity8,
            resourceUnnamed.Entity9,
            resourceUnnamed.Entity10,
            resourceUnnamed.Entity11,
            resourceUnnamed.Entity12,
            resourceUnnamed.Entity13,
        };
        foreach (var entity in entities)
        {
            if (entity is null)
                continue;

            foreach (var entry in entity.Sequences)
            {
                if (entry.GetUnk18() is not S80809F51 sequencer)
                    continue;

                foreach (var entry2 in sequencer.Array2)
                {
                    if (entry2.Unk10.GetValue(entry.GetReader()) is not S80807ECA sound || sound.Audio is null)
                        continue;

                    string name = sound.Audio.Hash;
                    WeaponAudioCategory category = new()
                    {
                        Name = name,
                        Sounds = new()
                    };
                    foreach (var wem in sound.Audio.TagData.Wems)
                    {
                        if (wem is null)
                            continue;

                        WeaponAudioItem soundItem = new()
                        {
                            ParentCategory = category.Name,
                            Hash = wem.Hash,
                            DisplayHash = $"[{wem.Hash}]"
                        };
                        await soundItem.LoadWEMAsync();

                        category.Sounds.Add(soundItem);
                    }

                    Sounds.Add(category);
                }
            }
        }

        RefreshSoundList();
    }

    #endregion


    private CancellationTokenSource _audioSelectionCts;
    private async void AudioList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var list = sender as ListView;
        _audioSelectionCts?.Cancel();
        _audioSelectionCts = new CancellationTokenSource();
        var token = _audioSelectionCts.Token;

        try
        {
            await Task.Delay(100, token); // Debounce time
            if (token.IsCancellationRequested)
                return;

            Dispatcher.Invoke(() =>
            {
                if (list.SelectedIndex >= 0)
                {
                    var container = list.ItemContainerGenerator.ContainerFromIndex(list.SelectedIndex);
                    RadioButton currentButton = UIHelper.GetChildOfType<RadioButton>(container);
                    if (currentButton != null)
                        currentButton.IsChecked = true;
                }
            });
        }
        catch (TaskCanceledException)
        {
        }
    }

    private async void DrawWaveform()
    {
        await Task.Run(() =>
        {
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                Waveform.Source = null;
                WaveformLoading.Visibility = Visibility.Visible;
            });
            if (_currentSoundStream is null || _currentSound.Channels > 4)
                return;

            // Somethings up with the wave stream somehow becoming null? Idfk whats going on
            var wave = WaveFormRenderer.Create(_currentSoundStream, _sinePreviewSettings);

            // Overlay Right and Left
            using var combined = new Bitmap(wave.ImageLeft.Width, wave.ImageLeft.Height);
            using (var g = Graphics.FromImage(combined))
            {
                g.DrawImage(wave.ImageLeft, 0, 0);
                g.DrawImage(wave.ImageRight, 0, 0);
            }

            using var memory = new MemoryStream();
            combined.Save(memory, ImageFormat.Png);
            memory.Position = 0;

            var bitmapImage = ApiImageUtils.MakeBitmapImage(memory, wave.ImageLeft.Width, wave.ImageLeft.Height);

            Application.Current.Dispatcher.Invoke(() =>
            {
                Waveform.Source = bitmapImage;
                WaveformLoading.Visibility = Visibility.Collapsed;
            });
        });
    }

    private void UpdateWaveformProgress()
    {
        Task.Run(() =>
        {
            if (_currentSound is null)
                return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                double width = Waveform.ActualWidth;
                double height = Waveform.ActualHeight;
                double progress = MusicPlayer.ProgressBar.Value;

                WaveformProgressBar.Width = width;
                WaveformProgressBar.Height = height;

                WaveformTintClip.Rect = new Rect(0, 0, width * progress, height);
            });
        });
    }

    public class WeaponItem
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Rarity { get; set; }
        public TigerHash Hash { get; set; }
        public bool IsSelected { get; set; } = false;
    }

    public class WeaponAudioCategory
    {
        public string Name { get; set; }
        public List<WeaponAudioItem> Sounds { get; set; }
    }

    public class WeaponAudioItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        public FileHash Hash { get; set; }

        public string ParentCategory { get; set; }

        private string _displayHash;
        public string DisplayHash
        {
            get => _displayHash;
            set
            {
                _displayHash = value;
                OnPropertyChanged(nameof(DisplayHash));
            }
        }

        private string _duration;
        public string Duration
        {
            get => _duration;
            set
            {
                _duration = value;
                OnPropertyChanged(nameof(Duration));
            }
        }

        private float _seconds;
        public float Seconds
        {
            get => _seconds;
            set
            {
                _seconds = value;
                OnPropertyChanged(nameof(Seconds));
            }
        }

        private int _channels;
        public int Channels
        {
            get => _channels;
            set
            {
                _channels = value;
                OnPropertyChanged(nameof(Channels));
            }
        }

        private int _sampleRate;
        public int SampleRate
        {
            get => _sampleRate;
            set
            {
                _sampleRate = value;
                OnPropertyChanged(nameof(SampleRate));
            }
        }

        public async Task LoadWEMAsync()
        {
            if (Hash == null)
                return;

            Wem wem = await FileResourcer.Get().GetFileAsync<Wem>(Hash, false, false);
            if (wem == null || wem.GetReferenceHash().IsInvalid())
                return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                DisplayHash = $"[{Hash}] {(wem.Channels > 2 ? "⚠" : "")}";
                Duration = wem.Duration;
                Seconds = wem.Seconds;
                Channels = wem.Channels;
                SampleRate = wem.SampleRate;
            });
        }
    }

    private class SineSettings : RenderSettings
    {
        public SineSettings(int width, int height)
        {
            DisplayName = "Sine";
            Width = width;
            Height = height;
            SampleResolution = 8;
            PrimaryLineColor = System.Drawing.Color.White;
            LineThickness = 1f;
            CenterLineColor = System.Drawing.Color.Transparent;
            XStep = 2f;
            VolumeBoost = 1f;
            AutoWidth = false;
        }

        public static SineSettings CreatePreview() => new SineSettings(800, 200);
        public static SineSettings CreateExport() => new SineSettings(4096, 1024);
    }
}


