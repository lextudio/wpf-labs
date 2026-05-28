using Jalium.UI;
using Jalium.UI.Controls;
using Microsoft.Maui.DevFlow.Agent.Core;
using LeXtudio.DevFlow.Agent.Jalium;

var port = GetAgentPort();

var statusLabel = new TextBlock
{
    Text = "Starting DevFlow...",
    Tag = "ResponseText"
};

var pressButton = new Button
{
    Tag = "ActionButton",
    Content = "Press me"
};
pressButton.Click += (s, e) => statusLabel.Text = "Button pressed.";

var window = new Window
{
    Title = "Jalium DevFlow Test",
    Width = 900,
    Height = 700,
    Content = new StackPanel
    {
        Orientation = Orientation.Vertical,
        Margin = new Thickness(16),
        Children =
        {
            new TextBlock { Text = "Jalium DevFlow Sample", FontSize = 24 },
            pressButton,
            statusLabel,
            new TextBlock { Text = $"Open http://localhost:{port}/api/v1/agent/status to verify DevFlow is running." }
        }
    }
};

window.Loaded += (s, e) =>
{
    var agent = Application.Current!.AddJaliumDevFlowAgent(new AgentOptions { Port = port });
    statusLabel.Text = $"DevFlow agent started on port {agent.Port}.";
    Console.WriteLine($"[JaliumDevFlowTestApp] DevFlow agent started on port {agent.Port}.");
};

var app = new Application();

try
{
    app.Run(window);
}
catch (Exception ex)
{
    Console.WriteLine($"[JaliumDevFlowTestApp] Application.Run failed: {ex}");
    throw;
}

static int GetAgentPort()
{
    var portValue = Environment.GetEnvironmentVariable("DEVFLOW_AGENT_PORT");
    if (int.TryParse(portValue, out var port) && port > 0)
        return port;

    return LeXtudio.DevFlow.Agent.Core.DevFlowAgentPortResolver.GetPortFromAssemblyMetadata() ?? Microsoft.Maui.DevFlow.Agent.Core.AgentOptions.DefaultPort;
}
