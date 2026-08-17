using System.Windows;
using System.Windows.Controls;
using Tiger;

namespace MIDA;

// Maybe one day this will be used again.
public partial class PortingConfigView : UserControl
{
    private ConfigSubsystem _config;
    public PortingConfigView()
    {
        InitializeComponent();
        _config = TigerInstance.GetSubsystem<ConfigSubsystem>();
    }

    public void OnControlLoaded(object sender, RoutedEventArgs e)
    {
    }
}
