using System.Reflection;
using Microsoft.Maui.DevFlow.Agent.Core;

namespace LeXtudio.DevFlow.Agent.Core;

public static class DevFlowAgentPortResolver
{
    private const int FallbackPort = 9223;

    public static void ApplyDefaultPort(AgentOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (options.Port != AgentOptions.DefaultPort)
        {
            return;
        }

        options.Port = GetPortFromAssemblyMetadata() ?? FallbackPort;
    }

    public static int? GetPortFromAssemblyMetadata()
    {
        var entryAssembly = Assembly.GetEntryAssembly();
        if (entryAssembly == null)
        {
            return null;
        }

        foreach (var metadata in entryAssembly.GetCustomAttributes<AssemblyMetadataAttribute>())
        {
            if (metadata.Key == "Microsoft.Maui.DevFlowPort" && int.TryParse(metadata.Value, out var port) && port > 0)
            {
                return port;
            }
        }

        return null;
    }
}
