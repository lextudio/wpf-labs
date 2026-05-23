using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using LeXtudio.DevFlow.Agent.Core;

namespace UnoDevFlowTestApp;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();
        Loaded += MainPage_Loaded;
    }

    private void MainPage_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            WebViewHost.NavigateToString("""
<!doctype html>
<html><body style="font-family:Segoe UI;padding:12px">
<h3 id="title">DevFlow Uno WebView Test</h3>
<p id="content">Deterministic inline HTML for screenshot validation.</p>
</body></html>
""");
        }
        catch
        {
        }
    }

    private void ActionButton_Click(object sender, RoutedEventArgs e)
    {
        ResponseText.Text = $"Button clicked at {System.DateTime.Now:T}";
    }

    [DevFlowAction("uno.echo", Description = "Echoes an input string for invoke API tests.")]
    public static string Echo(string value) => $"echo:{value}";
}
