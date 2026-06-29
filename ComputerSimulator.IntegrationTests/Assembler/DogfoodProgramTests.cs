using System.IO;
using ComputerSimulator.Assembler;
using ComputerSimulator.Core;
using ComputerSimulator.Core.Peripherals.Display;
using ComputerSimulator.IntegrationTests.Peripherals.Display;
using FluentAssertions;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests.Assembler;

public class DogfoodProgramTests : IntegrationTestBase
{
    [Test]
    public void AssembledDisplayPatternProgramFillsTheDisplay()
    {
        const int width = 16;
        const int height = 8;

        var options = new AssemblerOptions();
        options.Defines["BYTES_PER_FRAME"] = (width / 8) * height;

        var sourcePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "programs", "display-pattern.asm");
        var image = new ScottAssembler().AssembleFileOrThrow(sourcePath, options);

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
}
