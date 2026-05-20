#if UNO || HAS_UNO
using Microsoft.UI.Xaml;
using Microsoft.Maui.DevFlow.Agent.Core;

namespace LeXtudio.Uno.DevFlow.Agent.Uno;

public static class UnoAgentServiceExtensions
{
    public static UnoAgentService AddUnoDevFlowAgent(this Application app, AgentOptions? options = null)
    {
        var service = new UnoAgentService(options);
        service.Start();
        return service;
    }
}
#endif
