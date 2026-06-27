using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Parts;
using ComputerSimulator.Core.Peripherals;
using ComputerSimulator.Core.Peripherals.Display;
using FluentAssertions;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests.Peripherals.Display;

public class DisplayAdapterTests : IntegrationTestBase
{
    private IIoBus _ioBus = null!;
    private IDisplayAdapter _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _ioBus = WireFactory.CreateIoBus("io");
        _sut = ComponentFactory.CreateDisplayAdapter(_ioBus);
    }

    [Test]
    public void BlankDisplayRendersAllPixelsOff()
    {
        var output = new FakeDisplayOutput();

        output.Initialize(_sut.Width, _sut.Height);

        _sut.RenderFrame(output);

        output.LitPixelCount.Should().Be(0);
    }

    [Test]
    public void WritingAByteLightsTheCorrespondingPixels()
    {
        // Simulate the CPU OUT sequence directly on the IO bus.
        SelectDisplay();
        OutAddress(5);   // display-RAM byte 5
        OutData(0b1111_1111);

        var output = new FakeDisplayOutput();
        output.Initialize(_sut.Width, _sut.Height);
        _sut.RenderFrame(output);

        // Byte 5 maps to row 0, byte-column 5 -> pixels x = 40..47 on row 0.
        var bytesPerRow = _sut.Width / 8;
        var row = 5 / bytesPerRow;
        var startX = (5 % bytesPerRow) * 8;

        output.LitPixelCount.Should().Be(8);
        for (var bit = 0; bit < 8; bit++)
        {
            output.IsLit(startX + bit, row).Should().BeTrue();
        }
    }

    [Test]
    public void WritingAPatternByteLightsOnlyTheSetBits()
    {
        SelectDisplay();
        OutAddress(0);
        OutData(0b0000_0101); // bits 0 and 2 set

        var output = new FakeDisplayOutput();
        output.Initialize(_sut.Width, _sut.Height);
        _sut.RenderFrame(output);

        output.IsLit(0, 0).Should().BeTrue();
        output.IsLit(1, 0).Should().BeFalse();
        output.IsLit(2, 0).Should().BeTrue();
        output.IsLit(3, 0).Should().BeFalse();
        output.LitPixelCount.Should().Be(2);
    }

    private void SelectDisplay() => OutAddress((int)IoAddress.Display);

    private void OutAddress(int value)
    {
        _ioBus.CpuBus.SetValue(value.ToBinaryBools(8));
        _ioBus.DataAddress.Value = true;
        _ioBus.InputOutput.Value = true;
        _ioBus.Clk.Set.Value = true;
        _sut.Update();
        ClearStrobe();
    }

    private void OutData(int value)
    {
        _ioBus.CpuBus.SetValue(value.ToBinaryBools(8));
        _ioBus.DataAddress.Value = false;
        _ioBus.InputOutput.Value = true;
        _ioBus.Clk.Set.Value = true;
        _sut.Update();
        ClearStrobe();
    }

    // Drop the clock strobe so the write port is not re-triggered on later updates.
    private void ClearStrobe()
    {
        _ioBus.Clk.Set.Value = false;
        _sut.Update();
    }
}
