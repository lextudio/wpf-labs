using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Xunit;

namespace LeXtudio.DevFlow.Agent.WinForms.Tests;

#pragma warning disable xUnit1051
public class WinFormsAgentIntegrationTests
{
    [Fact]
    public async Task StatusAndTree_Work()
    {
        var port = GetFreePort();
        await using var host = await StartHostAsync(port);
        using var client = new HttpClient { BaseAddress = new Uri($"http://localhost:{port}") };

        var status = await PollStatusAsync(client, TimeSpan.FromSeconds(15));
        Assert.True(status.GetProperty("running").GetBoolean());
        Assert.Equal("winforms", status.GetProperty("framework").GetString());
        Assert.True(status.GetProperty("capabilities").GetProperty("screenshots").GetBoolean());

        using var tree = await client.GetAsync("/api/v1/ui/tree");
        tree.EnsureSuccessStatusCode();
        using var treeDoc = JsonDocument.Parse(await tree.Content.ReadAsStreamAsync());
        Assert.True(treeDoc.RootElement.GetProperty("elements").GetArrayLength() > 0);
    }

    [Fact]
    public async Task TapFillClearFocus_Work()
    {
        var port = GetFreePort();
        await using var host = await StartHostAsync(port);
        using var client = new HttpClient { BaseAddress = new Uri($"http://localhost:{port}") };
        await PollStatusAsync(client, TimeSpan.FromSeconds(15));
        await WaitForElementAsync(client, "ActionButton", TimeSpan.FromSeconds(10));
        await WaitForElementAsync(client, "InputBox", TimeSpan.FromSeconds(10));
        await WaitForElementAsync(client, "ResponseLabel", TimeSpan.FromSeconds(10));

        (await client.PostAsync("/api/v1/ui/tap", Json("{" + "\"id\":\"ActionButton\"}"))).EnsureSuccessStatusCode();
        (await client.PostAsync("/api/v1/ui/actions/fill", Json("{" + "\"elementId\":\"InputBox\",\"text\":\"hello\"}"))).EnsureSuccessStatusCode();
        (await client.PostAsync("/api/v1/ui/actions/focus", Json("{" + "\"elementId\":\"InputBox\"}"))).EnsureSuccessStatusCode();
        (await client.PostAsync("/api/v1/ui/actions/clear", Json("{" + "\"elementId\":\"InputBox\"}"))).EnsureSuccessStatusCode();

        using var input = await client.GetAsync("/api/v1/ui/element?id=InputBox");
        input.EnsureSuccessStatusCode();
        using var inputDoc = JsonDocument.Parse(await input.Content.ReadAsStreamAsync());
        Assert.Equal(string.Empty, inputDoc.RootElement.GetProperty("text").GetString());

        using var responseLabel = await client.GetAsync("/api/v1/ui/element?id=ResponseLabel");
        responseLabel.EnsureSuccessStatusCode();
        using var labelDoc = JsonDocument.Parse(await responseLabel.Content.ReadAsStreamAsync());
        Assert.Equal("Button clicked", labelDoc.RootElement.GetProperty("text").GetString());
    }

    [Fact]
    public async Task ScreenshotScrollAndKey_Work()
    {
        var port = GetFreePort();
        await using var host = await StartHostAsync(port);
        using var client = new HttpClient { BaseAddress = new Uri($"http://localhost:{port}") };
        await PollStatusAsync(client, TimeSpan.FromSeconds(15));
        await WaitForElementAsync(client, "ActionButton", TimeSpan.FromSeconds(10));
        await WaitForElementAsync(client, "InputBox", TimeSpan.FromSeconds(10));
        await WaitForElementAsync(client, "MainScrollPanel", TimeSpan.FromSeconds(10));

        using var fullShot = await client.GetAsync("/api/v1/ui/screenshot");
        fullShot.EnsureSuccessStatusCode();
        var fullBytes = await fullShot.Content.ReadAsByteArrayAsync();
        Assert.True(IsPng(fullBytes));

        using var elementShot = await client.GetAsync("/api/v1/ui/screenshot?id=ActionButton");
        elementShot.EnsureSuccessStatusCode();
        var elementBytes = await elementShot.Content.ReadAsByteArrayAsync();
        Assert.True(IsPng(elementBytes));

        using var selectorShot = await client.GetAsync("/api/v1/ui/screenshot?selector=%23ActionButton");
        selectorShot.EnsureSuccessStatusCode();
        var selectorBytes = await selectorShot.Content.ReadAsByteArrayAsync();
        Assert.True(IsPng(selectorBytes));

        (await client.PostAsync("/api/v1/ui/actions/scroll", Json("{" + "\"id\":\"MainScrollPanel\",\"deltaY\":120}"))).EnsureSuccessStatusCode();

        (await client.PostAsync("/api/v1/ui/actions/key", Json("{" + "\"elementId\":\"InputBox\",\"text\":\"A\"}"))).EnsureSuccessStatusCode();
        (await client.PostAsync("/api/v1/ui/actions/key", Json("{" + "\"elementId\":\"InputBox\",\"key\":\"backspace\"}"))).EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task QueryAndErrorEnvelope_Work()
    {
        var port = GetFreePort();
        await using var host = await StartHostAsync(port);
        using var client = new HttpClient { BaseAddress = new Uri($"http://localhost:{port}") };
        await PollStatusAsync(client, TimeSpan.FromSeconds(15));

        using var query = await client.GetAsync("/api/v1/ui/elements?type=TextBox");
        query.EnsureSuccessStatusCode();

        using var bad = await client.GetAsync("/api/v1/ui/element");
        Assert.Equal(HttpStatusCode.BadRequest, bad.StatusCode);
    }

    private static bool IsPng(byte[] bytes)
    {
        byte[] h = [137, 80, 78, 71, 13, 10, 26, 10];
        return bytes.Length >= h.Length && bytes.Take(h.Length).SequenceEqual(h);
    }

    private static StringContent Json(string body) => new(body, Encoding.UTF8, "application/json");
    private static async Task<JsonElement> PollStatusAsync(HttpClient client, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                using var response = await client.GetAsync("/api/v1/agent/status");
                if (response.IsSuccessStatusCode)
                {
                    using var doc = JsonDocument.Parse(await response.Content.ReadAsStreamAsync());
                    return doc.RootElement.Clone();
                }
            }
            catch { }
            await Task.Delay(250);
        }

        throw new InvalidOperationException("Agent status endpoint did not become available in time.");
    }

    private static int GetFreePort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }

    private static async Task WaitForElementAsync(HttpClient client, string elementId, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                using var response = await client.GetAsync($"/api/v1/ui/element?id={Uri.EscapeDataString(elementId)}");
                if (response.IsSuccessStatusCode)
                    return;
            }
            catch { }

            await Task.Delay(200);
        }

        throw new InvalidOperationException($"Element '{elementId}' was not available in time.");
    }

    private static async Task<DisposableProcessHost> StartHostAsync(int port)
    {
        var repoRoot = FindRepositoryRoot(Directory.GetCurrentDirectory());
        var hostProjectPath = Path.GetFullPath(Path.Combine(repoRoot, "src", "DevFlow", "WinFormsDevFlowTestApp", "WinFormsDevFlowTestApp.csproj"));
        var hostProjectDirectory = Path.GetDirectoryName(hostProjectPath)!;

        if (!RunCommand("dotnet", $"build \"{hostProjectPath}\" -c Debug", hostProjectDirectory, out var outp, out var err))
            throw new InvalidOperationException($"Failed to build WinForms host project:\n{err}\n{outp}");

        var exePath = Path.Combine(hostProjectDirectory, "bin", "Debug", "net8.0-windows", "WinFormsDevFlowTestApp.exe");
        var psi = new ProcessStartInfo(exePath)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = hostProjectDirectory,
        };
        psi.Environment["DEVFLOW_AGENT_PORT"] = port.ToString();

        var process = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start host process.");
        await Task.Delay(300);
        return new DisposableProcessHost(process);
    }

    private static string FindRepositoryRoot(string startFolder)
    {
        var current = new DirectoryInfo(startFolder);
        while (current != null)
        {
            if (Directory.Exists(Path.Combine(current.FullName, "src", "DevFlow"))) return current.FullName;
            current = current.Parent;
        }
        throw new InvalidOperationException("Unable to locate repository root.");
    }

    private static bool RunCommand(string command, string arguments, string workingDirectory, out string output, out string error)
    {
        using var process = new Process();
        process.StartInfo.FileName = command;
        process.StartInfo.Arguments = arguments;
        process.StartInfo.WorkingDirectory = workingDirectory;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.UseShellExecute = false;
        process.Start();
        output = process.StandardOutput.ReadToEnd();
        error = process.StandardError.ReadToEnd();
        process.WaitForExit();
        return process.ExitCode == 0;
    }

    private sealed class DisposableProcessHost(Process process) : IAsyncDisposable
    {
        public ValueTask DisposeAsync()
        {
            if (!process.HasExited)
            {
                process.Kill(true);
                process.WaitForExit(5000);
            }
            process.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
