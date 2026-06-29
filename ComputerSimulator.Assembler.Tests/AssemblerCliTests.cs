using FluentAssertions;
using NUnit.Framework;

namespace ComputerSimulator.Assembler.Tests;

public class AssemblerCliTests
{
    [Test]
    public void CliAssemblesSourceFileToBinary()
    {
        var directory = TestContext.CurrentContext.WorkDirectory;
        var sourcePath = Path.Combine(directory, $"program-{Guid.NewGuid():N}.asm");
        var outputPath = Path.Combine(directory, $"program-{Guid.NewGuid():N}.bin");

        try
        {
            File.WriteAllText(sourcePath, "DATA R0, VALUE\nHALT\n");

            var exitCode = ComputerSimulator.Assembler.Cli.Program.Main([sourcePath, "-o", outputPath, "-D", "VALUE=0x2A"]);

            exitCode.Should().Be(0);
            File.ReadAllBytes(outputPath).Select(b => (int)b).Should().Equal(0x20, 0x2A, 0x40, 0x02);
        }
        finally
        {
            File.Delete(sourcePath);
            File.Delete(outputPath);
        }
    }
}
