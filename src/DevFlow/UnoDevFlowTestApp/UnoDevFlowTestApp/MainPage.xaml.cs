using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace UnoDevFlowTestApp;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();
    }

    private void ActionButton_Click(object sender, RoutedEventArgs e)
    {
        ResponseText.Text = $"Button clicked at {System.DateTime.Now:T}";
    }
}
