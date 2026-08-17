using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Tiger;
using Tiger.Schema.Investment;

namespace MIDA;

public partial class MainMenuView : UserControl
{
    private static MainWindow _mainWindow = null;

    public MainMenuView()
    {
        InitializeComponent();

        ApiButton.IsEnabled = true;
        BagsButton.IsEnabled = false;
        WeaponAudioButton.IsEnabled = true;
        StaticsButton.IsEnabled = true;

        Strategy.OnStrategyChangedEvent += delegate (StrategyEventArgs args)
        {
            Dispatcher.Invoke(() =>
            {
                ApiButton.IsEnabled = true;
                BagsButton.IsEnabled = false; // TODO?
                WeaponAudioButton.IsEnabled = true;
                StaticsButton.IsEnabled = true;
            });
        };
    }

    private void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
        _mainWindow = Window.GetWindow(this) as MainWindow;
        GameVersion.Text = $"Game Version: {_mainWindow.GameInfo?.FileVersion}";
        //MouseMove += UserControl_MouseMove; // I like the effect but Marathon doesnt use this

        SetupTimer();

        if (Random.Shared.Next(0, 420) == 67) // haha im so funny. kill me.
        {
            funnyeasteregg();
        }
    }

    private async void ApiViewButton_OnClick(object sender, RoutedEventArgs e)
    {
        await LoadInvestment();

        GearView apiView = new GearView();
        apiView.LoadContent();
        _mainWindow.MakeNewTab("GEAR", apiView);
        _mainWindow.SetNewestTabSelected();
    }

    private void NamedEntitiesBagsViewButton_OnClick(object sender, RoutedEventArgs e)
    {
        TagListViewerView tagListView = new TagListViewerView();
        tagListView.LoadContent(ETagListType.DestinationGlobalTagBagList);
        _mainWindow.MakeNewTab("destination global tag bag", tagListView);
        _mainWindow.SetNewestTabSelected();
    }

    private void AllEntitiesView2Button_OnClick(object sender, RoutedEventArgs e)
    {
        EntityListView entityListView = new();
        entityListView.LoadContent();
        _mainWindow.MakeNewTab("Entities", entityListView);
        _mainWindow.SetNewestTabSelected();
    }

    private void ActivitiesViewButton_OnClick(object sender, RoutedEventArgs e)
    {
        TagListViewerView tagListView = new TagListViewerView();
        tagListView.LoadContent(ETagListType.ActivityList);
        _mainWindow.MakeNewTab("Activities", tagListView);
        _mainWindow.SetNewestTabSelected();
    }

    private void AllStaticsViewButton_OnClick(object sender, RoutedEventArgs e)
    {
        StaticListView statics = new StaticListView();
        statics.LoadContent();
        _mainWindow.MakeNewTab("Statics", statics);
        _mainWindow.SetNewestTabSelected();
    }

    private async void WeaponAudioViewButton_Click(object sender, RoutedEventArgs e)
    {
        await LoadInvestment();

        WeaponAudioListView weaponAudio = new();
        weaponAudio.LoadContent();
        _mainWindow.MakeNewTab("Weapon Audio", weaponAudio);
        _mainWindow.SetNewestTabSelected();
    }

    private void AllAudioViewButton_OnClick(object sender, RoutedEventArgs e)
    {
        AudioListView audioListView = new();
        audioListView.LoadContent();
        _mainWindow.MakeNewTab("Sounds", audioListView);
        _mainWindow.SetNewestTabSelected();
    }

    private void AllBKHDViewButton_OnClick(object sender, RoutedEventArgs e)
    {
        AudioListView audioListView = new(AudioListView.AudioListViewType.SoundBanks);
        audioListView.LoadContent();
        _mainWindow.MakeNewTab("Sound Banks", audioListView);
        _mainWindow.SetNewestTabSelected();
    }

    private void AllTexturesView2Button_OnClick(object sender, RoutedEventArgs e)
    {
        TextureListView textureListView = new TextureListView();
        textureListView.LoadContent();
        _mainWindow.MakeNewTab("Textures", textureListView);
        _mainWindow.SetNewestTabSelected();
    }

    private void AllMaterialsViewButton_OnClick(object sender, RoutedEventArgs e)
    {
        TagListViewerView tagListView = new TagListViewerView();
        tagListView.LoadContent(ETagListType.MaterialList);
        _mainWindow.MakeNewTab("Materials", tagListView);
        _mainWindow.SetNewestTabSelected();
    }

    private void GithubButton_OnClick(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo { FileName = "https://github.com/DeltaDesigns/MIDA", UseShellExecute = true });
    }

    private void UserControl_MouseMove(object sender, MouseEventArgs e)
    {
        System.Windows.Point position = e.GetPosition(this);
        TranslateTransform gridTransform = (TranslateTransform)MainContainer.RenderTransform;
        gridTransform.X = position.X * -0.0075;
        gridTransform.Y = position.Y * -0.0075;
    }

    private async Task LoadInvestment()
    {
        MainWindow.Progress.SetProgressStages(new() { "Loading Investment System" });
        await Task.Run(() => Investment.LazyInit());
        MainWindow.Progress.CompleteStage();
    }

    private void AboutButton_OnClick(object sender, RoutedEventArgs e)
    {
        PopupBanner about = new()
        {
            DarkenBackground = true,
            Icon = "",
            Title = $"MIDA {App.CurrentVersion.Id}",
            Subtitle = "Marathon Information and Data Assistant",
            Description =
            "MIDA is a fork of Charm designed for Marathon.\n" +
            "MIDA was developed for 3D artists, nerds, to preserve content as much as possible, and for learning how the Tiger engine works in general!\n\n" +
            "By using MIDA, you agree to not use it to spread leaks/spoilers or anything that may break Bungie's TOS.",
            Style = PopupBanner.PopupStyle.Information
        };
        about.Show();
    }

    private void ChangelogButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (!File.Exists($"./Changelog.json"))
        {
            NotificationBanner noChangelog = new()
            {
                Icon = "⚠️",
                Title = "CHANGELOG NOT FOUND",
                Description = "Changelog.json not found in the MIDA root folder.",
                Style = NotificationBanner.PopupStyle.Warning
            };
            noChangelog.Show();
            return;
        }

        Changelog changelog = new();
        MainWindow.Current.ViewboxGrid.Children.Add(changelog);
        changelog.Load();
    }

    private DispatcherTimer uiTimer;
    private void SetupTimer()
    {
        uiTimer?.Stop();
        uiTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1500) };
        uiTimer.Tick += (s, e) =>
        {
            int len = Random.Shared.Next(8, 16);
            Symbols.Text = GenerateRandomString(len);
        };
        uiTimer.Start();
    }

    private static string GenerateRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        StringBuilder sb = new StringBuilder(length * 2);

        for (int i = 0; i < length; i++)
        {
            char c = chars[Random.Shared.Next(chars.Length)];
            sb.Append(c);
            if (i < length - 1)
                sb.Append(' ');
        }

        return sb.ToString();
    }

    private static string GenerateRandomHexString(int length)
    {
        StringBuilder sb = new StringBuilder(length * 2);

        for (int i = 0; i < length; i++)
        {
            int value = Random.Shared.Next(0, 16);
            sb.Append(value.ToString("X"));
            if (i < length - 1)
                sb.Append(' ');
        }

        return sb.ToString();
    }

    private void funnyeasteregg()
    {
        var bitmapImage = new BitmapImage();
        bitmapImage.BeginInit();
        bitmapImage.UriSource = new Uri("https://images.contentstack.io/v3/assets/blt15f7b5c0d43ed112/blt0656353348a074b8/69806a3998d6ebf4e5ca3676/marathon.bungie.character.png"); ;
        bitmapImage.EndInit();

        PopupBanner about = new()
        {
            DarkenBackground = true,
            //Icon = "",
            IconImage = bitmapImage,
            Title = $"ATTENTION RUNNAH",
            Subtitle = "MIDA NEEDS YOU.",
            Description =
            "WRITE \"uesc sux\" ON 5 WALLS\n\n" +
            "WITH A PERMANENT MARKA\n\n" +
            "(in a single run)",
            Style = PopupBanner.PopupStyle.Warning
        };
        about.Show();
    }
}
