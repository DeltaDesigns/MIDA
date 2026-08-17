using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Arithmic;
using Tiger;
using Tiger.Schema;
using Tiger.Schema.Activity;

namespace MIDA;

public partial class ActivityMapView : UserControl
{
    private IActivity _currentActivity;
    private DisplayBubble _currentBubble;
    private string _destinationName;

    public ActivityMapView()
    {
        InitializeComponent();
    }

    private void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
    {

    }

    public void LoadUI(IActivity activity)
    {
        _destinationName = activity.DestinationName;
        _currentActivity = activity;

        MapList.ItemsSource = GetMapList(activity);
        ExportControl.SetExportFunction(ExportFull, (int)ExportTypeFlag.Full, true); //| (int)ExportTypeFlag.ArrangedMap, true);
        ExportControl.SetExportInfo(activity.FileHash);

        QuickControls.Visibility = Visibility.Hidden;
        ManualControls.Visibility = Visibility.Hidden;
        ExportControl.Visibility = Visibility.Hidden;
    }

    private ObservableCollection<DisplayBubble> GetMapList(IActivity activity)
    {
        var maps = new ObservableCollection<DisplayBubble>();
        foreach (var bubble in activity.EnumerateBubbles())
        {
            DisplayBubble displayMap = new();
            displayMap.Name = bubble.Name;
            displayMap.Hash = bubble.ChildMapReference.Hash;
            maps.Add(displayMap);
        }
        return maps;
    }

    private async void GetBubbleContentsButton_OnClick(object sender, RoutedEventArgs e)
    {
        var bubble = (sender as ToggleButton).DataContext as DisplayBubble;
        _currentBubble = bubble;
        FileHash hash = new FileHash(bubble.Hash);

        Dispatcher.Invoke(() => MapControl.Visibility = Visibility.Hidden);
        MainWindow.Progress.SetProgressStages(new() { $"Loading Map Parts for {bubble.Name}" });

        Tag<SBubbleDefinition> bubbleMaps = FileResourcer.Get().GetSchemaTag<SBubbleDefinition>(hash);
        await Task.Run(() => PopulateStaticList(bubbleMaps));

        MainWindow.Progress.CompleteStage();
        Dispatcher.Invoke(() => MapControl.Visibility = Visibility.Visible);
        QuickControls.Visibility = Visibility.Visible;
        ManualControls.Visibility = Visibility.Visible;
        ExportControl.Visibility = Visibility.Visible;
    }

    private void PopulateStaticList(Tag<SBubbleDefinition> bubbleMaps)
    {
        ConcurrentBag<DisplayStaticMap> items = new ConcurrentBag<DisplayStaticMap>();
        Parallel.ForEach(bubbleMaps.TagData.MapResources, m =>
        {
            foreach (var dataTable in m.MapContainer.TagData.MapDataTables)
            {
                foreach (var entry in dataTable.MapDataTable.TagData.DataEntries)
                {
                    if (entry.DataResource.GetValue(dataTable.MapDataTable.GetReader()) is SMapDataResource resource)
                    {
                        resource.StaticMapParent?.Load();
                        if (resource.StaticMapParent is null || resource.StaticMapParent.TagData.StaticMap is null)
                            continue;

                        var tag = resource.StaticMapParent.TagData.StaticMap;

                        items.Add(new DisplayStaticMap
                        {
                            Hash = m.MapContainer.Hash,
                            Name = $"{m.MapContainer.Hash}: {tag.TagData.Instances.Count} instances, {tag.TagData.Statics.Count} uniques",
                            Instances = tag.TagData.Instances.Count
                        });

                    }
                }
            }
        });

        var sortedItems = new List<DisplayStaticMap>(items);
        sortedItems.Sort((a, b) => b.Instances.CompareTo(a.Instances));
        sortedItems.Insert(0, new DisplayStaticMap
        {
            Name = "Select all"
        });

        // Shouldnt be a problem in D2, but in D1 a map container can have multiple static map parents
        // it still exports fine (I think) but having multiple of the same map container entries can cause info.cfg read/write crashes
        Dispatcher.Invoke(() =>
        {
            StaticList.ItemsSource = sortedItems.DistinctBy(x => x.Hash);
        });
    }

    private async void QuickControl_OnClick(object sender, RoutedEventArgs e)
    {
        int exportType = Int32.Parse(((sender as Button).DataContext as string));
        MapControl.Visibility = Visibility.Hidden;

        switch (exportType)
        {
            case 0: // All Map
                {
                    var tcs = new TaskCompletionSource<bool>();
                    PopupBanner warn = new()
                    {
                        DarkenBackground = true,
                        Title = "HEADS UP",
                        Subtitle = "You're about to export an ENTIRE map!",
                        Description = "Marathon maps are EXTREMELY model and instance heavy. Exporting an entire map WILL TAKE A LONG TIME!" +
                        "\nThousands (maybe even 10K+) of models will be exported!" +
                        "\n\nYou are about to export this maps statics, entities and activity entities." +
                        "\nAre you sure you want to continue?",
                        Style = PopupBanner.PopupStyle.Warning,
                        UserInput = "Yep.",
                        UserInputSecondary = "Nevermind..."
                    };
                    warn.MouseLeftButtonDown += (s, e) =>
                    {
                        tcs.TrySetResult(true);
                    };

                    warn.MouseRightButtonDown += (s, e) =>
                    {
                        tcs.TrySetResult(false);
                    };
                    warn.Show();

                    bool confirmed = await tcs.Task;
                    warn.Remove();
                    if (!confirmed)
                    {
                        MapControl.Visibility = Visibility.Visible;
                        return;
                    }

                    await Task.Run(() => ExportStaticMap());
                    await Task.Run(() => ExportResources());
                    await Task.Run(() => ExportActivityEntities());
                }
                break;

            case 1: // Static Map
                await Task.Run(() => ExportStaticMap());
                break;

            case 2: // Map Resources
                await Task.Run(() => ExportResources());
                break;

            case 3: // Activity Entities
                await Task.Run(() => ExportActivityEntities());
                break;
        }

        Dispatcher.Invoke(() => MapControl.Visibility = Visibility.Hidden);
        NotificationBanner notify = new()
        {
            Icon = "☑️",
            Title = "Export Complete",
            Description = $"Exported {_currentBubble.Name} to \"{ConfigSubsystem.Get().GetExportSavePath()}/Maps/{_currentActivity.DestinationName}/\"",
            Style = NotificationBanner.PopupStyle.Information
        };
        notify.OnProgressComplete += () => Dispatcher.Invoke(() => MapControl.Visibility = Visibility.Visible);
        notify.Show();
    }

    public async void ExportActivityEntities()
    {
        MainWindow.Progress.SetProgressStages(new() { $"Gathering Activity Entities for {_currentBubble.Name}..." });

        var maps = new ConcurrentDictionary<FileHash, List<FileHash>>();
        var entries = _currentActivity.EnumerateActivityEntities().Where(x => x.BubbleName == _currentBubble.Name).ToList();

        var tag = (_currentActivity as Tiger.Schema.Activity.MARATHON.Activity).TagData.AmbientActivity;
        if (tag is not null)
        {
            var ambient = FileResourcer.Get().GetFileInterface<IActivity>(tag.Hash);
            entries.AddRange(ambient.EnumerateActivityEntities().Where(x => x.BubbleName == _currentBubble.Name).ToList());
        }


        foreach (var entry in entries)
        {
            if (entry.DataTables.Count > 0)
            {
                var containerHash = entry.Hash;
                if (!maps.ContainsKey(containerHash))
                    maps.TryAdd(containerHash, new());

                foreach (var hash in entry.DataTables)
                {
                    if (!maps[containerHash].Contains(hash))
                        maps[containerHash].Add(hash);
                }
            }
        }
        MainWindow.Progress.CompleteStage();

        ExportResources(maps);
    }

    public async void ExportStaticMap()
    {
        Log.Info($"Exporting Static Map: {_currentBubble.Name}, {_currentBubble.Hash}");
        Dispatcher.Invoke(() =>
        {
            MapControl.Visibility = Visibility.Hidden;
        });

        Tag<SBubbleDefinition> bubbleMaps = FileResourcer.Get().GetSchemaTag<SBubbleDefinition>(_currentBubble.Hash);
        var maps = new List<FileHash>();
        bubbleMaps.TagData.MapResources.ForEach(m =>
        {
            var containerHash = m.MapContainer.Hash;
            if (!maps.Contains(containerHash))
                maps.Add(containerHash);
        });

        List<string> mapStages = maps.Select((x, i) => $"Preparing {x} ({i + 1}/{maps.Count()})\nThis may take some time.").ToList();
        mapStages.Add("Exporting Static Map...\nThis will take a while.");
        MainWindow.Progress.SetProgressStages(mapStages);

        Tiger.Exporters.Exporter.Get().GetOrCreateGlobalScene();
        string savePath = $"{ConfigSubsystem.Get().GetExportSavePath()}/Maps/{_currentActivity.DestinationName}/";
        maps.ForEach(map =>
        {
            MapView.ExportFullMap(FileResourcer.Get().GetSchemaTag<SMapContainer>(map), savePath);
            MainWindow.Progress.CompleteStage();
        });

        Tiger.Exporters.Exporter.Get().Export(savePath);
        MainWindow.Progress.CompleteStage();

        Dispatcher.Invoke(() =>
        {
            MapControl.Visibility = Visibility.Visible;
        });
        Log.Info($"Exported Static Map: {_currentBubble.Name}, {_currentBubble.Hash}");
    }

    public async void ExportResources(ConcurrentDictionary<FileHash, List<FileHash>> maps = null)
    {
        // this is dumb but whatever lol
        string type = maps == null ? "Map Resources" : "Activity Entities";
        Dispatcher.Invoke(() =>
        {
            MapControl.Visibility = Visibility.Hidden;
        });

        if (maps is null)
        {
            Tag<SBubbleDefinition> bubbleMaps = FileResourcer.Get().GetSchemaTag<SBubbleDefinition>(_currentBubble.Hash);
            maps = new ConcurrentDictionary<FileHash, List<FileHash>>();
            bubbleMaps.TagData.MapResources.ForEach(m =>
            {
                var containerHash = m.MapContainer.Hash;
                if (!maps.ContainsKey(containerHash))
                    maps.TryAdd(m.MapContainer.Hash, new());

                foreach (var dataTable in m.MapContainer.TagData.MapDataTables)
                {
                    var hash = dataTable.MapDataTable;
                    if (dataTable.MapDataTable is not null && !maps[containerHash].Contains(hash.Hash))
                        maps[containerHash].Add(hash.Hash);
                }
            });
        }

        Log.Info($"Exporting {type}: {_currentBubble.Name}, {_currentBubble.Hash}");
        List<string> mapStages = maps.Select((x, i) => $"Preparing {_currentBubble.Name} ({i + 1}/{maps.Count()})\nThis may take some time.").ToList();
        mapStages.Add($"Exporting {type}...\nThis will take a while.");

        MainWindow.Progress.SetProgressStages(mapStages);

        Tiger.Exporters.Exporter.Get().GetOrCreateGlobalScene();
        string savePath = $"{ConfigSubsystem.Get().GetExportSavePath()}/Maps/{_currentActivity.DestinationName}/";
        foreach ((FileHash container, List<FileHash> hashes) in maps)
        {
            ActivityMapEntityView.ExportFull(hashes, container, savePath);
            MainWindow.Progress.CompleteStage();
        }

        Tiger.Exporters.Exporter.Get().Export(savePath);
        MainWindow.Progress.CompleteStage();

        Dispatcher.Invoke(() =>
        {
            MapControl.Visibility = Visibility.Visible;
        });
        Log.Info($"Exported {type}: {_currentBubble.Name}, {_currentBubble.Hash}");
    }

    // Kept for individual / old method of exporting
    public async void ExportFull(ExportInfo info)
    {
        // todo figure out how to make this work
        IActivity activity = FileResourcer.Get().GetFileInterface<IActivity>(info.Hash);
        Log.Info($"Exporting activity data name: {PackageResourcer.Get().GetActivityName(activity.FileHash)}, hash: {activity.FileHash}, export type {info.ExportType.ToString()}");
        Dispatcher.Invoke(() =>
        {
            MapControl.Visibility = Visibility.Hidden;
        });
        var maps = new List<Tag<SMapContainer>>();
        bool bSelectAll = false;
        foreach (DisplayStaticMap item in StaticList.Items)
        {
            if (item.Selected && item.Name == "Select all")
            {
                bSelectAll = true;
                Log.Info($"Selected all maps");
            }
            else
            {
                if (item.Selected || bSelectAll)
                {
                    maps.Add(FileResourcer.Get().GetSchemaTag<SMapContainer>(item.Hash));
                    Log.Info($"Selected map: {item.Hash}");
                }
            }
        }

        if (maps.Count == 0)
        {
            Log.Error("No maps selected for export.");
            //MessageBox.Show("No maps selected for export.");

            Dispatcher.Invoke(() =>
            {
                MapControl.Visibility = Visibility.Hidden;

                NotificationBanner warn = new()
                {
                    Icon = "⚠️",
                    Title = "WARNING",
                    Description = $"No map parts selected for export!",
                    Style = NotificationBanner.PopupStyle.Warning
                };
                warn.OnProgressComplete += () => Dispatcher.Invoke(() => MapControl.Visibility = Visibility.Visible);
                warn.Show();
            });
            return;
        }

        List<string> mapStages = maps.Select((x, i) => $"Preparing {i + 1}/{maps.Count}\nThis may take some time.").ToList();
        mapStages.Add("Exporting...\nThis will take a while.");
        MainWindow.Progress.SetProgressStages(mapStages);

        Tiger.Exporters.Exporter.Get().GetOrCreateGlobalScene();
        string savePath = $"{ConfigSubsystem.Get().GetExportSavePath()}/Maps/{_currentActivity.DestinationName}/";
        maps.ForEach(map =>
        {
            MapView.ExportFullMap(map, savePath);
            MainWindow.Progress.CompleteStage();
        });

        Tiger.Exporters.Exporter.Get().Export(savePath);

        MainWindow.Progress.CompleteStage();

        Dispatcher.Invoke(() =>
        {
            MapControl.Visibility = Visibility.Visible;
        });
        Log.Info($"Exported activity data name: {PackageResourcer.Get().GetActivityName(activity.FileHash)}, hash: {activity.FileHash}");
        //MessageBox.Show("Activity map data exported completed.");

        Dispatcher.Invoke(() =>
        {
            MapControl.Visibility = Visibility.Hidden;
            NotificationBanner notify = new()
            {
                Icon = "☑️",
                Title = "Export Complete",
                Description = $"Exported activity data from {PackageResourcer.Get().GetActivityName(activity.FileHash)} to \"{ConfigSubsystem.Get().GetExportSavePath()}/Maps/{_currentActivity.DestinationName}/\"",
                Style = NotificationBanner.PopupStyle.Information
            };
            notify.OnProgressComplete += () => Dispatcher.Invoke(() => MapControl.Visibility = Visibility.Visible);
            notify.Show();
        });

    }

    public void Dispose()
    {
        MapControl.Dispose();
    }
}

public class DisplayBubble
{
    public string Name { get; set; }
    public string Hash { get; set; }
}

public class DisplayStaticMap
{
    public string Name { get; set; }
    public string Hash { get; set; }
    public int Instances { get; set; }

    public bool Selected { get; set; }
}
