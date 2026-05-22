using System;
using System.Collections.Generic;
using System.Linq;

namespace LeXtudio.MewUI.Cli
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            var tokens = new Queue<string>(args ?? Array.Empty<string>());
            var options = ParseGlobalOptions(tokens);

            if (options.ShowHelp || tokens.Count == 0)
            {
                ShowHelp();
                return 0;
            }

            var command = tokens.Dequeue().ToLowerInvariant();
            return RunCommand(command, tokens, options);
        }

        private static OutputOptions ParseGlobalOptions(Queue<string> tokens)
        {
            var options = new OutputOptions();
            var preserved = new List<string>();

            while (tokens.Count > 0)
            {
                var token = tokens.Peek();
                switch (token)
                {
                    case "--json":
                        options = options with { Json = true };
                        tokens.Dequeue();
                        break;
                    case "--verbose":
                    case "-v":
                        options = options with { Verbose = true };
                        tokens.Dequeue();
                        break;
                    case "--dry-run":
                        options = options with { DryRun = true };
                        tokens.Dequeue();
                        break;
                    case "--ci":
                        options = options with { Ci = true };
                        tokens.Dequeue();
                        break;
                    case "--help":
                    case "-h":
                        options = options with { ShowHelp = true };
                        tokens.Dequeue();
                        break;
                    default:
                        preserved.Add(tokens.Dequeue());
                        break;
                }
            }

            while (preserved.Count > 0)
            {
                tokens.Enqueue(preserved[0]);
                preserved.RemoveAt(0);
            }

            return options;
        }

        private static int RunCommand(string command, Queue<string> tokens, OutputOptions options)
        {
            return command switch
            {
                "doctor" => CommandHandlers.RunDoctor(options),
                "version" => CommandHandlers.RunVersion(options),
                "new" => CommandHandlers.RunNew(tokens, options),
                "build" => CommandHandlers.RunBuild(tokens, options),
                "run" => CommandHandlers.RunRun(tokens, options),
                "publish" => CommandHandlers.RunPublish(tokens, options),
                "package" => CommandHandlers.RunPackage(tokens, options),
                "diagnostics" => CommandHandlers.RunDiagnostics(tokens, options),
                "env" => CommandHandlers.RunEnv(tokens, options),
                "devflow" => CommandHandlers.RunDevFlow(tokens, options),
                "help" => ShowHelpAndReturn(),
                _ => UnknownCommand(command)
            };
        }

        private static int ShowHelpAndReturn()
        {
            ShowHelp();
            return 0;
        }

        private static int UnknownCommand(string command)
        {
            Console.Error.WriteLine($"Unknown command: {command}");
            Console.Error.WriteLine("Run 'dotnet mewui --help' for available commands.");
            return 1;
        }

        private static void ShowHelp()
        {
            Console.WriteLine("LeXtudio.MewUI.Cli - MewUI command line utility");
            Console.WriteLine();
            Console.WriteLine("Usage: dotnet mewui [options] <command> [command-options]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  --json      Emit structured JSON output");
            Console.WriteLine("  -v, --verbose  Enable verbose logging");
            Console.WriteLine("  --dry-run   Show what would be done without making changes");
            Console.WriteLine("  --ci        Run in CI-friendly non-interactive mode");
            Console.WriteLine("  --help, -h  Show help information");
            Console.WriteLine();
            Console.WriteLine("Commands:");
            Console.WriteLine("  doctor       Validate the MewUI development environment");
            Console.WriteLine("  version      Display CLI and environment version information");
            Console.WriteLine("  new          Scaffold a new MewUI app");
            Console.WriteLine("  build        Build a MewUI project");
            Console.WriteLine("  run          Run a MewUI application");
            Console.WriteLine("  publish      Publish a MewUI application");
            Console.WriteLine("  package      Package MewUI output artifacts");
            Console.WriteLine("  diagnostics  Run MewUI diagnostics and validation");
            Console.WriteLine("  env          Inspect installed SDKs and tooling");
            Console.WriteLine("  devflow      Query a running MewUI DevFlow agent and inspect runtime state");
            Console.WriteLine("    status     Show DevFlow agent status");
            Console.WriteLine("    screenshot Capture a screenshot from a running DevFlow agent");
            Console.WriteLine("    tap        Send a tap action to a running DevFlow agent");
        }
    }
}
