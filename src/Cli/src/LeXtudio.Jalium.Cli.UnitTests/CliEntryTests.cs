using System;
using System.IO;
using Xunit;

namespace LeXtudio.Jalium.Cli.UnitTests
{
    public class CliEntryTests
    {
        [Fact]
        public void NoArguments_ShowsHelp()
        {
            var output = CaptureConsole(() => Program.Main(Array.Empty<string>()));
            Assert.Contains("LeXtudio.Jalium.Cli - Jalium command line utility", output);
        }

        [Fact]
        public void DoctorCommand_PrintsDoctorMessage()
        {
            var output = CaptureConsole(() => Program.Main(new[] { "doctor" }));
            Assert.Contains("Validated the Jalium development environment.", output);
        }

        private static string CaptureConsole(Func<int> action)
        {
            var stdout = Console.Out;
            try
            {
                using var writer = new StringWriter();
                Console.SetOut(writer);
                action();
                return writer.ToString();
            }
            finally
            {
                Console.SetOut(stdout);
            }
        }
    }
}
