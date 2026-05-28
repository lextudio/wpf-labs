using Jalium.UI;
using LeXtudio.DevFlow.Agent.Core;
using Microsoft.Maui.DevFlow.Agent.Core;

namespace LeXtudio.DevFlow.Agent.Jalium;

public static class JaliumAgentServiceExtensions
{
    public static JaliumAgentService AddJaliumDevFlowAgent(this Application app, AgentOptions? options = null)
    {
        options ??= new AgentOptions();
        DevFlowAgentPortResolver.ApplyDefaultPort(options);

        var service = new JaliumAgentService(options);
        service.Start();
        return service;
    }
}
