using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Tiger;
using Tiger.Schema.Investment;
using static MIDA.APIItemView;

namespace MIDA;

public partial class MainMenuView : UserControl
{
    private static MainWindow _mainWindow = null;
    private APITooltip ToolTip;

    public MainMenuView() // TODO buttons
    {
        InitializeComponent();

        ApiButton.IsEnabled = true;
        BagsButton.IsEnabled = false;
        WeaponAudioButton.IsEnabled = false;
        StaticsButton.IsEnabled = true;
        CollectionsButton.IsEnabled = false;

        Strategy.OnStrategyChangedEvent += delegate (StrategyEventArgs args)
        {
            Dispatcher.Invoke(() =>
            {
                ApiButton.IsEnabled = true;
                BagsButton.IsEnabled = false;
                WeaponAudioButton.IsEnabled = false;
                StaticsButton.IsEnabled = true;
                CollectionsButton.IsEnabled = false;
            });
        };
    }

    private void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
        _mainWindow = Window.GetWindow(this) as MainWindow;
        GameVersion.Text = $"Game Version: {_mainWindow.GameInfo?.FileVersion}";
        //MouseMove += UserControl_MouseMove; // I like the effect but Marathon doesnt use this

        ToolTip = new();
        Panel.SetZIndex(ToolTip, 50);
        MainContainer.Children.Add(ToolTip);
        SetupTimer();
    }

    private void CategoryButton_MouseEnter(object sender, MouseEventArgs e)
    {
        ToolTip.ActiveItem = (sender as Button);
        string[] text = (sender as Button).Tag.ToString().Split(":");

        PlugItem plugItem = new()
        {
            Name = $"{text[0]}",
            Description = $"{text[1]}",
            PlugRarityColor = MarathonTierType.Superior.GetColor(),
        };

        ToolTip.MakeTooltip(plugItem);
    }

    public void CategoryButton_MouseLeave(object sender, MouseEventArgs e)
    {
        ToolTip.ClearTooltip();
        ToolTip.ActiveItem = null;
    }

    private async void ApiViewButton_OnClick(object sender, RoutedEventArgs e)
    {
        await LoadInvestment();

        APIView apiView = new APIView();
        apiView.LoadContent();
        _mainWindow.MakeNewTab("api", apiView);
        _mainWindow.SetNewestTabSelected();
    }

    private async void CollectionsViewButton_OnClick(object sender, RoutedEventArgs e)
    {
        await LoadInvestment();

        CollectionsView apiView2 = new CollectionsView();
        apiView2.LoadContent();
        _mainWindow.MakeNewTab("Collections", apiView2);
        _mainWindow.SetNewestTabSelected();
    }

    private void NamedEntitiesBagsViewButton_OnClick(object sender, RoutedEventArgs e)
    {
        TagListViewerView tagListView = new TagListViewerView();
        tagListView.LoadContent(ETagListType.DestinationGlobalTagBagList);
        _mainWindow.MakeNewTab("destination global tag bag", tagListView);
        _mainWindow.SetNewestTabSelected();
    }

    private void AllEntitiesViewButton_OnClick(object sender, RoutedEventArgs e)
    {
        TagListViewerView tagListView = new TagListViewerView();
        tagListView.LoadContent(ETagListType.EntityList);
        _mainWindow.MakeNewTab("dynamics", tagListView);
        _mainWindow.SetNewestTabSelected();
    }

    private void ActivitiesViewButton_OnClick(object sender, RoutedEventArgs e)
    {
        TagListViewerView tagListView = new TagListViewerView();
        tagListView.LoadContent(ETagListType.ActivityList);
        _mainWindow.MakeNewTab("activities", tagListView);
        _mainWindow.SetNewestTabSelected();
    }

    private void AllStaticsViewButton_OnClick(object sender, RoutedEventArgs e)
    {
        TagListViewerView tagListView = new TagListViewerView();
        tagListView.LoadContent(ETagListType.StaticsList);
        _mainWindow.MakeNewTab("statics", tagListView);
        _mainWindow.SetNewestTabSelected();
    }

    private async void WeaponAudioViewButton_Click(object sender, RoutedEventArgs e)
    {
        await LoadInvestment();

        TagListViewerView tagListView = new TagListViewerView();
        tagListView.LoadContent(ETagListType.WeaponAudioGroupList);
        _mainWindow.MakeNewTab("weapon audio", tagListView);
        _mainWindow.SetNewestTabSelected();
    }

    private void AllAudioViewButton_OnClick(object sender, RoutedEventArgs e)
    {
        TagListViewerView tagListView = new TagListViewerView();
        tagListView.LoadContent(ETagListType.SoundsPackagesList);
        _mainWindow.MakeNewTab("sounds", tagListView);
        _mainWindow.SetNewestTabSelected();
    }

    private void AllBKHDViewButton_OnClick(object sender, RoutedEventArgs e)
    {
        TagListViewerView tagListView = new TagListViewerView();
        tagListView.LoadContent(ETagListType.BKHDGroupList);
        _mainWindow.MakeNewTab("sound banks", tagListView);
        _mainWindow.SetNewestTabSelected();
    }

    private void AllStringsViewButton_OnClick(object sender, RoutedEventArgs e)
    {
        TagListViewerView tagListView = new TagListViewerView();
        tagListView.LoadContent(ETagListType.StringContainersList);
        _mainWindow.MakeNewTab("strings", tagListView);
        _mainWindow.SetNewestTabSelected();
    }

    //private void AllTexturesViewButton_OnClick(object sender, RoutedEventArgs e)
    //{
    //    TagListViewerView tagListView = new TagListViewerView();
    //    tagListView.LoadContent(ETagListType.TextureList);
    //    _mainWindow.MakeNewTab("textures", tagListView);
    //    _mainWindow.SetNewestTabSelected();
    //}

    private void AllTexturesView2Button_OnClick(object sender, RoutedEventArgs e)
    {
        TextureListView textureListView = new TextureListView();
        textureListView.LoadContent();
        _mainWindow.MakeNewTab("textures", textureListView);
        _mainWindow.SetNewestTabSelected();
    }

    private void AllMaterialsViewButton_OnClick(object sender, RoutedEventArgs e)
    {
        TagListViewerView tagListView = new TagListViewerView();
        tagListView.LoadContent(ETagListType.MaterialList);
        _mainWindow.MakeNewTab("materials", tagListView);
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
            Icon = $"{Char.ConvertFromUtf32(0xEE3F)[0]}",
            //about.IconImage = MainWindow.GetBitmapSource(System.Drawing.Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetExecutingAssembly().Location));
            Title = $"MIDA {App.CurrentVersion.Id}",
            Subtitle = "MIDA is a fork of Charm designed soley for Marathon",
            Description =
            "MIDA was developed for 3D artists, to preserve content as much as possible, and for learning how the Tiger engine works in general!\n\n" +
            "Additional help/development from:\n" +
            "• nblock\n" +
            "• Cohae\n",
            Style = PopupBanner.PopupStyle.Information
        };
        about.Show();
    }

    private static Random random = new Random();
    private static Timer timer;
    private void SetupTimer()
    {
        if (timer is not null)
        {
            timer?.Stop();
            timer?.Dispose();
            timer = null;
        }

        timer = new Timer(1500); // 5 seconds
        timer.Elapsed += OnTimerElapsed;
        timer.AutoReset = true;
        timer.Enabled = true;
    }

    private void OnTimerElapsed(object sender, ElapsedEventArgs e)
    {
        int randomLength = random.Next(8, 17);
        Dispatcher.Invoke(() =>
        {
            Symbols.Text = GenerateRandomHexString(randomLength);
        });
    }

    private static string GenerateRandomHexString(int length)
    {
        StringBuilder sb = new StringBuilder(length * 2);

        for (int i = 0; i < length; i++)
        {
            int value = random.Next(0, 16);
            sb.Append(value.ToString("X"));
            if (i < length - 1)
                sb.Append(' ');
        }

        return sb.ToString();
    }
}
