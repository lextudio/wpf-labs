using System.Windows;
using LeXtudio.DevFlow.Agent.Core;
using Microsoft.Web.WebView2.Core;

namespace WpfDevFlowTestApp;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            await WebViewHost.EnsureCoreWebView2Async();
            WebViewHost.CoreWebView2.NavigateToString("""
<!doctype html>
<html><body style="font-family:Segoe UI;padding:12px">
<h3 id="title">DevFlow WPF WebView Test</h3>
<p id="content">Deterministic inline HTML for screenshot validation.</p>
</body></html>
""");
        }
        catch
        {
            // Keep sample app resilient even when WebView2 runtime is unavailable.
        }
    }

    private void ActionButton_Click(object sender, RoutedEventArgs e)
    {
        ResponseText.Text = "Button clicked at " + System.DateTime.Now.ToLongTimeString();
    }

    [DevFlowAction("wpf.echo", Description = "Echoes an input string for invoke API tests.")]
    public static string Echo(string value) => $"echo:{value}";
}
