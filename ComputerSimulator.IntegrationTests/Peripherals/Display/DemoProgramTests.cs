using ComputerSimulator.Core;
using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Peripherals.Display;
using FluentAssertions;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests.Peripherals.Display;

public class DemoProgramTests : IntegrationTestBase
{
    [Test]
    public void DemoProgramFillsTheDisplayWithItsAddressPattern()
    {
        const int width = 16;
        const int height = 8;

        var computerPart = ComponentFactory.CreateComputerPart();
        var display = new DisplayAdapter(computerPart.IoBus, width, height, ComponentFactory, WireFactory);
        computerPart.IoBus.ConnectedComponents.Add(display);

        var program = DemoProgram.Build(width, height);
        for (var address = 0; address < program.Count; address++)
        {
            computerPart.Ram
                .GetSlot(address & 0xFF, address >> 8)
                .Memory
                .SetRegisterValue(program[address]);
        }

        // Enough updates for the unrolled program to write every display byte.
        for (var i = 0; i < 30000; i++)
        {
            computerPart.Update();
        }

        var output = new FakeDisplayOutput();
        output.Initialize(width, height);
        display.RenderFrame(output);

        // The program writes byte value == address, so pixel (x, y) is lit iff bit (x % 8) of the
        // byte at address (y * bytesPerRow + x / 8) is set.
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
