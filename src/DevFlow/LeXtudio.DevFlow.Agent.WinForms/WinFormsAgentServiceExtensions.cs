using System.Windows.Forms;
using LeXtudio.DevFlow.Agent.Core;
using Microsoft.Maui.DevFlow.Agent.Core;

namespace LeXtudio.DevFlow.Agent.WinForms;

public static class WinFormsAgentServiceExtensions
{
    public static WinFormsAgentService AddWinFormsDevFlowAgent(this ApplicationContext app, AgentOptions? options = null)
    {
        options ??= new AgentOptions();
        DevFlowAgentPortResolver.ApplyDefaultPort(options);

        var service = new WinFormsAgentService(options);
        service.Start();
        Application.ApplicationExit += async (_, _) => await service.StopAsync().ConfigureAwait(false);
        return service;
    }
}
