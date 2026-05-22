using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using LeXtudio.DevFlow.Driver;

namespace LeXtudio.MewUI.Cli
{
    public static class CommandHandlers
    {
        public static int RunDoctor(OutputOptions options)
        {
            return WriteResult("doctor", "Validated the MewUI development environment.", options);
        }

        public static int RunVersion(OutputOptions options)
        {
            return WriteResult("version", "LeXtudio.MewUI.Cli version 1.0.0", options);
        }

        public static int RunNew(Queue<string> tokens, OutputOptions options)
        {
            var name = tokens.Count > 0 ? tokens.Dequeue() : "MewUIApp";
            var targetDirectory = Path.GetFullPath(name);

            if (Directory.Exists(targetDirectory) || File.Exists(targetDirectory))
                return WriteResult("new", $"Target already exists: {targetDirectory}", options);

            if (options.DryRun)
            {
                Console.WriteLine($"[dry-run] create new MewUI app in {targetDirectory}");
                return 0;
            }

            Directory.CreateDirectory(targetDirectory);
            var projectReference = TryResolveLocalAgentProjectReference(targetDirectory);
            var includeAgent = projectReference != null;
            File.WriteAllText(Path.Combine(targetDirectory, $"{name}.csproj"), GetMewUIProjectFile(name, projectReference));
            File.WriteAllText(Path.Combine(targetDirectory, "Program.cs"), GetMewUIProgramCode(includeAgent));

            var message = includeAgent
                ? $"Created MewUI app '{name}' with DevFlow agent support in {targetDirectory}"
                : $"Created MewUI app '{name}' in {targetDirectory}. DevFlow agent support not wired because repository reference could not be resolved.";

            return WriteResult("new", message, options);
        }

        public static int RunBuild(Queue<string> tokens, OutputOptions options)
        {
            var target = ParseTarget(tokens, out var configuration, out var runtime, out var framework, out var outputDirectory);
            var args = new StringBuilder();
            args.Append("build");
            args.Append(target != null ? $" \"{target}\"" : string.Empty);
            args.Append($" -c {configuration}");
            if (!string.IsNullOrEmpty(runtime)) args.Append($" -r {runtime}");
            if (!string.IsNullOrEmpty(framework)) args.Append($" -f {framework}");
            if (!string.IsNullOrEmpty(outputDirectory)) args.Append($" -o \"{outputDirectory}\"");

            return RunDotnetCommand(args.ToString(), options, Path.GetFullPath("."));
        }

        public static int RunRun(Queue<string> tokens, OutputOptions options)
        {
            var target = ParseTarget(tokens, out var configuration, out var runtime, out var framework, out var outputDirectory);
            var args = new StringBuilder();
            args.Append("run");
            args.Append(target != null ? $" --project \"{target}\"" : string.Empty);
            args.Append($" -c {configuration}");
            if (!string.IsNullOrEmpty(runtime)) args.Append($" -r {runtime}");
            if (!string.IsNullOrEmpty(framework)) args.Append($" -f {framework}");

            return RunDotnetCommand(args.ToString(), options, Path.GetFullPath("."));
        }

        public static int RunPublish(Queue<string> tokens, OutputOptions options)
        {
            var target = ParseTarget(tokens, out var configuration, out var runtime, out var framework, out var outputDirectory);
            var args = new StringBuilder();
            args.Append("publish");
            args.Append(target != null ? $" \"{target}\"" : string.Empty);
            args.Append($" -c {configuration}");
            if (!string.IsNullOrEmpty(runtime)) args.Append($" -r {runtime}");
            if (!string.IsNullOrEmpty(framework)) args.Append($" -f {framework}");
            if (!string.IsNullOrEmpty(outputDirectory)) args.Append($" -o \"{outputDirectory}\"");

            return RunDotnetCommand(args.ToString(), options, Path.GetFullPath("."));
        }

        public static int RunPackage(Queue<string> tokens, OutputOptions options)
        {
            var target = ParseTarget(tokens, out var configuration, out var runtime, out var framework, out var outputDirectory);
            var publishFolder = !string.IsNullOrEmpty(outputDirectory)
                ? outputDirectory
                : Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}");

            var publishArgs = new StringBuilder();
            publishArgs.Append("publish");
            publishArgs.Append(target != null ? $" \"{target}\"" : string.Empty);
            publishArgs.Append($" -c {configuration}");
            if (!string.IsNullOrEmpty(runtime)) publishArgs.Append($" -r {runtime}");
            if (!string.IsNullOrEmpty(framework)) publishArgs.Append($" -f {framework}");
            publishArgs.Append($" -o \"{publishFolder}\"");

            var exitCode = RunDotnetCommand(publishArgs.ToString(), options, Path.GetFullPath("."));
            if (exitCode != 0)
                return exitCode;

            if (options.DryRun)
                return 0;

            var packagePath = Path.Combine(Path.GetFullPath("."), "publish.zip");
            if (File.Exists(packagePath))
                File.Delete(packagePath);

            ZipFile.CreateFromDirectory(publishFolder, packagePath, CompressionLevel.Optimal, false);
            return WriteResult("package", $"Packaged output to {packagePath}", options);
        }

        public static int RunDiagnostics(Queue<string> _, OutputOptions options)
        {
            return RunDotnetCommand("--info", options, Path.GetFullPath("."));
        }

        public static int RunEnv(Queue<string> _, OutputOptions options)
        {
            Console.WriteLine($"OS: {Environment.OSVersion}");
            Console.WriteLine($"Process architecture: {RuntimeInformation.ProcessArchitecture}");
            return RunDotnetCommand("--info", options, Path.GetFullPath("."));
        }

        public static int RunDevFlow(Queue<string> tokens, OutputOptions options)
        {
            if (tokens.Count == 0)
            {
                return WriteResult("devflow", "Usage: dotnet mewui devflow <status|screenshot|tap> [options]", options);
            }

            var subcommand = tokens.Dequeue().ToLowerInvariant();
            return subcommand switch
            {
                "status" => RunDevFlowStatus(tokens, options),
                "screenshot" => RunDevFlowScreenshot(tokens, options),
                "tap" => RunDevFlowTap(tokens, options),
                "help" => WriteResult("devflow", "Usage: dotnet mewui devflow <status|screenshot|tap> [options]", options),
                "--help" => WriteResult("devflow", "Usage: dotnet mewui devflow <status|screenshot|tap> [options]", options),
                "-h" => WriteResult("devflow", "Usage: dotnet mewui devflow <status|screenshot|tap> [options]", options),
                _ => UnknownDevFlowSubcommand(subcommand)
            };
        }

        private static int RunDevFlowStatus(Queue<string> tokens, OutputOptions options)
        {
            ParseDevFlowOptions(tokens, out var host, out var port, out _);
            using var client = new AgentClient(host, port);
            AgentStatus? status;
            try
            {
                status = client.GetStatusAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException)
            {
                return WriteResult("devflow", $"Unable to contact MewUI DevFlow agent at {host}:{port}: {ex.Message}", options);
            }

            if (status == null)
                return WriteResult("devflow", "Unable to retrieve agent status.", options);

            if (options.Json)
            {
                Console.WriteLine(JsonSerializer.Serialize(status, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }));
                return 0;
            }

            Console.WriteLine("MewUI DevFlow agent status:");
            Console.WriteLine($"  Name:        {status.Name}");
            Console.WriteLine($"  Id:          {status.Id}");
            Console.WriteLine($"  Framework:   {status.Framework}");
            Console.WriteLine($"  Version:     {status.Version}");
            Console.WriteLine($"  Application: {status.Application}");
            Console.WriteLine($"  Running:     {status.Running}");
            Console.WriteLine($"  Port:        {status.Port}");
            return 0;
        }

        private static int RunDevFlowScreenshot(Queue<string> tokens, OutputOptions options)
        {
            ParseDevFlowOptions(tokens, out var host, out var port, out var outputFile);
            outputFile ??= "mewui-devflow-screenshot.png";

            if (options.DryRun)
            {
                Console.WriteLine($"[dry-run] save screenshot from http://{host}:{port}/api/v1/ui/screenshot to {outputFile}");
                return 0;
            }

            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            var url = new Uri($"http://{host}:{port}/api/v1/ui/screenshot");
            try
            {
                using var response = http.GetAsync(url).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();
                var bytes = response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
                File.WriteAllBytes(outputFile, bytes);
                return WriteResult("devflow", $"Saved screenshot to {outputFile}", options);
            }
            catch (Exception ex)
            {
                return WriteResult("devflow", $"Failed to capture screenshot: {ex.Message}", options);
            }
        }

        private static int RunDevFlowTap(Queue<string> tokens, OutputOptions options)
        {
            ParseDevFlowOptions(tokens, out var host, out var port, out _);
            string? elementId = null;

            while (tokens.Count > 0)
            {
                var token = tokens.Dequeue();
                if (token == "--id" && tokens.Count > 0)
                {
                    elementId = tokens.Dequeue();
                    continue;
                }

                Console.Error.WriteLine($"Unknown option: {token}");
                return 1;
            }

            if (string.IsNullOrEmpty(elementId))
                return WriteResult("devflow", "Missing --id <elementId> for devflow tap.", options);

            using var client = new AgentClient(host, port);
            try
            {
                var result = client.TapAsync(elementId).GetAwaiter().GetResult();
                return WriteResult("devflow", result ? $"Tapped element '{elementId}'" : $"Failed to tap element '{elementId}'", options);
            }
            catch (Exception ex)
            {
                return WriteResult("devflow", $"DevFlow tap failed: {ex.Message}", options);
            }
        }

        private static void ParseDevFlowOptions(Queue<string> tokens, out string host, out int port, out string? outputFile)
        {
            host = "localhost";
            port = 5500;
            outputFile = null;

            while (tokens.Count > 0)
            {
                var token = tokens.Dequeue();
                if (token == "--host" && tokens.Count > 0)
                {
                    host = tokens.Dequeue();
                    continue;
                }

                if (token == "--port" && tokens.Count > 0 && int.TryParse(tokens.Dequeue(), out var parsedPort))
                {
                    port = parsedPort;
                    continue;
                }

                if (token == "--output" && tokens.Count > 0)
                {
                    outputFile = tokens.Dequeue();
                    continue;
                }

                Console.Error.WriteLine($"Unknown option: {token}");
                break;
            }
        }

        private static string? ParseTarget(Queue<string> tokens, out string configuration, out string runtime, out string framework, out string outputDirectory)
        {
            configuration = "Debug";
            runtime = string.Empty;
            framework = string.Empty;
            outputDirectory = string.Empty;
            string? target = null;

            while (tokens.Count > 0)
            {
                var token = tokens.Dequeue();
                switch (token)
                {
                    case "--configuration":
                    case "-c":
                        if (tokens.Count > 0)
                            configuration = tokens.Dequeue();
                        break;
                    case "--runtime":
                    case "-r":
                        if (tokens.Count > 0)
                            runtime = tokens.Dequeue();
                        break;
                    case "--framework":
                        if (tokens.Count > 0)
                            framework = tokens.Dequeue();
                        break;
                    case "--output":
                        if (tokens.Count > 0)
                            outputDirectory = tokens.Dequeue();
                        break;
                    default:
                        if (target == null)
                            target = token;
                        break;
                }
            }

            return target;
        }

        private static string? TryResolveLocalAgentProjectReference(string newProjectDirectory)
        {
            var repoRoot = FindRepositoryRoot(Path.GetFullPath(newProjectDirectory));
            if (repoRoot == null)
                return null;

            var projectPath = Path.Combine(repoRoot, "src", "DevFlow", "LeXtudio.DevFlow.Agent.MewUI", "LeXtudio.DevFlow.Agent.MewUI.csproj");
            if (!File.Exists(projectPath))
                return null;

            var relative = Path.GetRelativePath(newProjectDirectory, projectPath).Replace(Path.DirectorySeparatorChar, '/');
            return relative;
        }

        private static string? FindRepositoryRoot(string directory)
        {
            var current = new DirectoryInfo(directory);
            while (current != null)
            {
                if (File.Exists(Path.Combine(current.FullName, ".git")) || Directory.Exists(Path.Combine(current.FullName, ".git")))
                {
                    return current.FullName;
                }

                current = current.Parent;
            }

            return null;
        }

        private static string GetMewUIProjectFile(string name, string? projectReference)
        {
            var referenceText = string.Empty;
            if (!string.IsNullOrEmpty(projectReference))
            {
                referenceText = $"  <ItemGroup>\n    <ProjectReference Include=\"{projectReference}\" />\n  </ItemGroup>\n";
            }

            return $"<Project Sdk=\"Microsoft.NET.Sdk\">\n" +
                   "  <PropertyGroup>\n" +
                   "    <OutputType>Exe</OutputType>\n" +
                   "    <TargetFramework>net10.0</TargetFramework>\n" +
                   "    <RuntimeIdentifiers>win-x64;win-arm64;linux-x64;linux-arm64;osx-x64;osx-arm64</RuntimeIdentifiers>\n" +
                   "    <ImplicitUsings>enable</ImplicitUsings>\n" +
                   "    <Nullable>enable</Nullable>\n" +
                   "    <AssemblyName>" + name + "</AssemblyName>\n" +
                   "    <RootNamespace>" + name + "</RootNamespace>\n" +
                   "  </PropertyGroup>\n" +
                   "  <ItemGroup>\n" +
                   "    <PackageReference Include=\"Aprillz.MewUI.Core\" />\n" +
                   "    <PackageReference Include=\"Aprillz.MewUI.Platform.Win32\" Condition=\"$(RuntimeIdentifier.StartsWith('win'))\" />\n" +
                   "    <PackageReference Include=\"Aprillz.MewUI.Platform.MacOS\" Condition=\"$(RuntimeIdentifier.StartsWith('osx'))\" />\n" +
                   "    <PackageReference Include=\"Aprillz.MewUI.Platform.X11\" Condition=\"$(RuntimeIdentifier.StartsWith('linux'))\" />\n" +
                   "    <PackageReference Include=\"Aprillz.MewUI.Backend.Direct2D\" Condition=\"$(RuntimeIdentifier.StartsWith('win'))\" />\n" +
                   "    <PackageReference Include=\"Aprillz.MewUI.Backend.MewVG.MacOS\" Condition=\"$(RuntimeIdentifier.StartsWith('osx'))\" />\n" +
                   "    <PackageReference Include=\"Aprillz.MewUI.Backend.MewVG.X11\" Condition=\"$(RuntimeIdentifier.StartsWith('linux'))\" />\n" +
                   "  </ItemGroup>\n" +
                   referenceText +
                   "</Project>\n";
        }

        private static string GetMewUIProgramCode(bool includeAgent)
        {
            if (!includeAgent)
            {
                return """
using System;
using System.Reflection;
using Aprillz.MewUI;
using Aprillz.MewUI.Controls;

var window = new Window()
    .Title("MewUI App")
    .Resizable(900, 700)
    .Content(
        new StackPanel()
            .Padding(16)
            .Spacing(12)
            .Children(
                new Label { Text = "MewUI app created by LeXtudio.MewUI.Cli" },
                new Label { Text = "DevFlow agent support was not detected in this repo." }
            )
    );

TryRegisterPlatformHost();
try
{
    Application.Run(window);
}
catch (Exception ex)
{
    Console.WriteLine($"[MewUIApp] Application.Run failed: {ex}");
    throw;
}

static void TryRegisterPlatformHost()
{
    if (OperatingSystem.IsWindows())
    {
        TryRegisterPlatform("Aprillz.MewUI.Win32Platform, Aprillz.MewUI.Platform.Win32");
        TryRegisterPlatform("Aprillz.MewUI.Direct2DBackend, Aprillz.MewUI.Backend.Direct2D");
        return;
    }

    if (OperatingSystem.IsLinux())
    {
        TryRegisterPlatform("Aprillz.MewUI.X11Platform, Aprillz.MewUI.Platform.X11");
        TryRegisterPlatform("Aprillz.MewUI.MewVGX11Backend, Aprillz.MewUI.Backend.MewVG.X11");
        return;
    }

    if (OperatingSystem.IsMacOS())
    {
        TryRegisterPlatform("Aprillz.MewUI.MacOSPlatform, Aprillz.MewUI.Platform.MacOS");
        TryRegisterPlatform("Aprillz.MewUI.MewVGMacOSBackend, Aprillz.MewUI.Backend.MewVG.MacOS");
        return;
    }
}

static void TryRegisterPlatform(string typeName)
{
    var type = Type.GetType(typeName, throwOnError: false, ignoreCase: false);
    if (type == null)
    {
        Console.WriteLine($"[MewUIApp] Platform registration type not found: {typeName}");
        return;
    }

    var registerMethod = type.GetMethod("Register", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
    if (registerMethod == null)
    {
        Console.WriteLine($"[MewUIApp] Register method not found on {typeName}");
        return;
    }

    registerMethod.Invoke(null, null);
}
""";
            }

            return """
using System;
using System.Reflection;
using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using Microsoft.Maui.DevFlow.Agent.Core;
using LeXtudio.DevFlow.Agent.MewUI;

var port = GetAgentPort();
var statusLabel = new Label
{
    Text = "Starting DevFlow...",
    Tag = "ResponseText"
};

var pressButton = new Button
{
    Tag = "ActionButton",
    Content = new Label { Text = "Press me" }
};
pressButton.Click += () => statusLabel.Text = "Button pressed.";

var window = new Window()
    .Title("MewUI DevFlow App")
    .Resizable(900, 700)
    .Content(
        new StackPanel()
            .Padding(16)
            .Spacing(12)
            .Children(
                new Label
                {
                    Text = "MewUI DevFlow Sample",
                    FontSize = 24
                },
                pressButton,
                statusLabel,
                new Label { Text = $"Open http://localhost:{port}/api/v1/agent/status to verify DevFlow is running." }
            )
    )
    .OnLoaded(() =>
    {
        var agent = Application.Current.AddMewUIDevFlowAgent(new AgentOptions { Port = port });
        statusLabel.Text = $"DevFlow agent started on port {agent.Port}.";
    });

RegisterMewUIPlatformHost();

try
{
    Application.Run(window);
}
catch (Exception ex)
{
    Console.WriteLine($"[MewUIApp] Application.Run failed: {ex}");
    throw;
}

static void RegisterMewUIPlatformHost()
{
    if (OperatingSystem.IsWindows())
    {
        TryRegisterPlatform("Aprillz.MewUI.Win32Platform, Aprillz.MewUI.Platform.Win32");
        TryRegisterPlatform("Aprillz.MewUI.Direct2DBackend, Aprillz.MewUI.Backend.Direct2D");
        return;
    }

    if (OperatingSystem.IsLinux())
    {
        TryRegisterPlatform("Aprillz.MewUI.X11Platform, Aprillz.MewUI.Platform.X11");
        TryRegisterPlatform("Aprillz.MewUI.MewVGX11Backend, Aprillz.MewUI.Backend.MewVG.X11");
        return;
    }

    if (OperatingSystem.IsMacOS())
    {
        TryRegisterPlatform("Aprillz.MewUI.MacOSPlatform, Aprillz.MewUI.Platform.MacOS");
        TryRegisterPlatform("Aprillz.MewUI.MewVGMacOSBackend, Aprillz.MewUI.Backend.MewVG.MacOS");
        return;
    }

    Console.WriteLine("[MewUIApp] No explicit MewUI platform host registration performed for this OS.");
}

static void TryRegisterPlatform(string typeName)
{
    var type = Type.GetType(typeName, throwOnError: false, ignoreCase: false);
    if (type == null)
    {
        Console.WriteLine($"[MewUIApp] Platform registration type not found: {typeName}");
        return;
    }

    var registerMethod = type.GetMethod("Register", BindingFlags.Public | BindingFlags.Static);
    if (registerMethod == null)
    {
        Console.WriteLine($"[MewUIApp] Register method not found on {typeName}");
        return;
    }

    registerMethod.Invoke(null, null);
}

static int GetAgentPort()
{
    var portValue = Environment.GetEnvironmentVariable("DEVFLOW_AGENT_PORT");
    return int.TryParse(portValue, out var port) && port > 0 ? port : 5500;
}
""";
        }
        private static int RunDotnetCommand(string arguments, OutputOptions options, string workingDirectory)
        {
            if (options.DryRun)
            {
                Console.WriteLine($"[dry-run] dotnet {arguments}");
                return 0;
            }

            var startInfo = new ProcessStartInfo("dotnet", arguments)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                WorkingDirectory = workingDirectory,
            };

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                Console.Error.WriteLine("Failed to start dotnet process.");
                return 1;
            }

            process.OutputDataReceived += (_, e) => { if (e.Data != null) Console.WriteLine(e.Data); };
            process.ErrorDataReceived += (_, e) => { if (e.Data != null) Console.Error.WriteLine(e.Data); };
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            return process.ExitCode;
        }

        private static int UnknownDevFlowSubcommand(string subcommand)
        {
            Console.Error.WriteLine($"Unknown devflow subcommand: {subcommand}");
            Console.Error.WriteLine("Run 'dotnet mewui devflow --help' for available commands.");
            return 1;
        }

        private static int WriteResult(string command, string message, OutputOptions options)
        {
            if (options.Json)
            {
                var payload = new { command, message, timestamp = DateTime.UtcNow };
                Console.WriteLine(JsonSerializer.Serialize(payload));
                return 0;
            }

            Console.WriteLine(message);
            if (options.Verbose)
            {
                Console.WriteLine("(verbose mode enabled)");
            }

            if (options.DryRun)
            {
                Console.WriteLine("(dry run: no changes will be made)");
            }

            return 0;
        }
    }
}
