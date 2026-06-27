using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Parts;
using ComputerSimulator.Core.Peripherals.Display;
using FluentAssertions;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests.Peripherals.Display;

/// <summary>
/// Exercises the full pipeline: a program running on the CPU writes a pixel byte to the display
/// through OUT instructions, the display adapter receives it via the IO bus, and the raster
/// scanner renders it back out.
/// </summary>
public class ComputerDisplayTests : IntegrationTestBase
{
    [Test]
    public void ProgramWritingAPixelByteLightsThePixelsOnScreen()
    {
        var computerPart = ComponentFactory.CreateComputerPart();
        var display = ComponentFactory.CreateDisplayAdapter(computerPart.IoBus);
        computerPart.IoBus.ConnectedComponents.Add(display);

        // DATA R1, 0x07 ; OUT Addr R1   (select display)
        // DATA R1, 0x00 ; OUT Addr R1   (display-RAM address 0)
        // DATA R1, 0xFF ; OUT Data R1   (write byte 0xFF)
        // JMP self                      (halt)
        var program = new[]
        {
            0x21, 0x07,
            0x7D,
            0x21, 0x00,
            0x7D,
            0x21, 0xFF,
            0x79,
            0x40, 0x09,
        };

        for (var address = 0; address < program.Length; address++)
        {
            computerPart.Ram
                .GetSlot(address & 0xFF, address >> 8)
                .Memory
                .SetRegisterValue(program[address].ToBinaryBools(8));
        }

        for (var i = 0; i < 400; i++)
        {
            computerPart.Update();
        }

        var output = new FakeDisplayOutput();
        output.Initialize(display.Width, display.Height);
        display.RenderFrame(output);

        output.LitPixelCount.Should().Be(8);
        for (var x = 0; x < 8; x++)
        {
            output.IsLit(x, 0).Should().BeTrue();
        }
    }
}
