using System.IO;
using System.Linq;
using ComputerSimulator.Assembler;
using ComputerSimulator.Core;
using ComputerSimulator.Core.Programs;
using ComputerSimulator.Core.Peripherals.Display;
using ComputerSimulator.Core.Peripherals.Display.Text;
using ComputerSimulator.IntegrationTests.Peripherals.Display;
using FluentAssertions;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests.Assembler;

public class DogfoodProgramTests : IntegrationTestBase
{
    private const int DefaultWidth = 96;
    private const int DefaultHeight = 48;

    [Test]
    public void AssembledDisplayPatternProgramFillsTheDisplay()
    {
        const int width = 16;
        const int height = 8;

        var options = new AssemblerOptions();
        options.Defines["BYTES_PER_FRAME"] = (width / 8) * height;

        var image = AssembleProgram("display-pattern.asm", options);

        var computerPart = ComponentFactory.CreateComputerPart();
        var display = new DisplayAdapter(computerPart.IoBus, width, height, ComponentFactory, WireFactory);
        computerPart.IoBus.ConnectedComponents.Add(display);
        ProgramLoader.Load(computerPart.Ram, image);

        for (var i = 0; i < 30000; i++)
        {
            computerPart.Update();
        }

        var output = new FakeDisplayOutput();
        output.Initialize(width, height);
        display.RenderFrame(output);

        var bytesPerRow = width / 8;
        var expectedLit = 0;
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var address = (y * bytesPerRow) + (x / 8);
                var on = ((address & 0xFF) & (1 << (x % 8))) != 0;
                output.IsLit(x, y).Should().Be(on, $"pixel ({x},{y}) maps to address {address}");
                if (on)
                {
                    expectedLit++;
                }
            }
        }

        output.LitPixelCount.Should().Be(expectedLit);
        expectedLit.Should().BeGreaterThan(0);
    }

    [Test]
    public void FontAssetMatchesCoreFontRom()
    {
        var fontPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "programs", "assets", "font8x8.bin");

        File.ReadAllBytes(fontPath).Should().Equal(AsciiFont8x8.CreateRomImage());
    }

    [TestCase("display-pattern.asm", "display-pattern.bin")]
    [TestCase("hello-world.asm", "hello-world.bin")]
    [TestCase("echo.asm", "echo.bin")]
    public void DefaultBuiltInBinaryMatchesFreshAssembly(string sourceName, string binaryName)
    {
        var expected = AssembleProgram(sourceName, CreateDefaultOptions(sourceName));
        var binaryPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "programs", "bin", binaryName);

        File.ReadAllBytes(binaryPath).Should().Equal(expected);
    }

    [TestCase(BuiltInProgram.DisplayPattern)]
    [TestCase(BuiltInProgram.HelloWorld)]
    [TestCase(BuiltInProgram.Echo)]
    public void BuiltInProgramBinaryExistsInRuntimeOutput(BuiltInProgram builtInProgram)
    {
        File.Exists(BuiltInProgramImages.GetPath(builtInProgram)).Should().BeTrue();
    }

    internal static byte[] AssembleProgram(string sourceName, AssemblerOptions? options = null)
    {
        var sourcePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "programs", sourceName);
        return new ScottAssembler().AssembleFileOrThrow(sourcePath, options);
    }

    internal static AssemblerOptions CreateDefaultOptions(string sourceName)
    {
        var options = new AssemblerOptions();
        if (sourceName == "display-pattern.asm")
        {
            options.Defines["BYTES_PER_FRAME"] = (DefaultWidth / 8) * DefaultHeight;
            return options;
        }

        options.Defines["BYTES_PER_ROW"] = DefaultWidth / 8;
        if (sourceName == "echo.asm")
        {
            options.Defines["SCREEN_WIDTH"] = DefaultWidth;
        }

        return options;
    }
}
