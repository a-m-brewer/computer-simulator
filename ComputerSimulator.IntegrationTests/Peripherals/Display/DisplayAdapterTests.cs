using System.Collections.Generic;
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

    [TestCase(DisplayScanMode.GateLevel)]
    [TestCase(DisplayScanMode.ScanBuffer)]
    public void BlankDisplayRendersAllPixelsOff(DisplayScanMode scanMode)
    {
        _sut = CreateDisplayAdapter(_ioBus, scanMode);
        var output = new FakeDisplayOutput();

        output.Initialize(_sut.Width, _sut.Height);

        _sut.RenderFrame(output);

        output.LitPixelCount.Should().Be(0);
    }

    [TestCase(DisplayScanMode.GateLevel)]
    [TestCase(DisplayScanMode.ScanBuffer)]
    public void WritingAByteLightsTheCorrespondingPixels(DisplayScanMode scanMode)
    {
        _sut = CreateDisplayAdapter(_ioBus, scanMode);
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

    [TestCase(DisplayScanMode.GateLevel)]
    [TestCase(DisplayScanMode.ScanBuffer)]
    public void WritingAPatternByteLightsOnlyTheSetBits(DisplayScanMode scanMode)
    {
        _sut = CreateDisplayAdapter(_ioBus, scanMode);
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

    [Test]
    public void ScanBufferRendersTheSamePixelsAsGateLevel()
    {
        var gateIoBus = WireFactory.CreateIoBus("gate-io");
        var gateLevel = CreateDisplayAdapter(gateIoBus, DisplayScanMode.GateLevel);
        WriteBytes(gateIoBus, gateLevel, new Dictionary<int, int>
        {
            [0] = 0b0000_0101,
            [5] = 0b1111_0000,
            [13] = 0b1010_1010
        });

        var scanIoBus = WireFactory.CreateIoBus("scan-io");
        var scanBuffer = CreateDisplayAdapter(scanIoBus, DisplayScanMode.ScanBuffer);
        WriteBytes(scanIoBus, scanBuffer, new Dictionary<int, int>
        {
            [0] = 0b0000_0101,
            [5] = 0b1111_0000,
            [13] = 0b1010_1010
        });

        var gateOutput = new FakeDisplayOutput();
        gateOutput.Initialize(gateLevel.Width, gateLevel.Height);
        gateLevel.RenderFrame(gateOutput);

        var scanOutput = new FakeDisplayOutput();
        scanOutput.Initialize(scanBuffer.Width, scanBuffer.Height);
        scanBuffer.RenderFrame(scanOutput);

        for (var y = 0; y < gateLevel.Height; y++)
        {
            for (var x = 0; x < gateLevel.Width; x++)
            {
                scanOutput.IsLit(x, y).Should().Be(gateOutput.IsLit(x, y), $"pixel ({x},{y}) should match");
            }
        }
    }

    [Test]
    public void ScanBufferSkipsPresentWhenDisplayIsUnchanged()
    {
        _sut = CreateDisplayAdapter(_ioBus, DisplayScanMode.ScanBuffer);
        var output = new FakeDisplayOutput();
        output.Initialize(_sut.Width, _sut.Height);

        _sut.RenderFrame(output);
        _sut.RenderFrame(output);

        output.Presents.Should().Be(1);
    }

    [Test]
    public void ScanBufferPresentsAgainWhenAByteChanges()
    {
        _sut = CreateDisplayAdapter(_ioBus, DisplayScanMode.ScanBuffer);
        var output = new FakeDisplayOutput();
        output.Initialize(_sut.Width, _sut.Height);

        _sut.RenderFrame(output);
        SelectDisplay();
        OutAddress(0);
        OutData(0b0000_0001);
        _sut.RenderFrame(output);

        output.Presents.Should().Be(2);
        output.IsLit(0, 0).Should().BeTrue();
    }

    private IDisplayAdapter CreateDisplayAdapter(IIoBus ioBus, DisplayScanMode scanMode)
    {
        return new DisplayAdapter(
            ioBus,
            ComputerSettings.ScreenWidth,
            ComputerSettings.ScreenHeight,
            scanMode,
            ComponentFactory,
            WireFactory);
    }

    private void WriteBytes(IIoBus ioBus, IDisplayAdapter displayAdapter, IReadOnlyDictionary<int, int> bytes)
    {
        foreach (var (address, value) in bytes)
        {
            OutAddress(ioBus, displayAdapter, (int)IoAddress.Display);
            OutAddress(ioBus, displayAdapter, address);
            OutData(ioBus, displayAdapter, value);
        }
    }

    private void SelectDisplay() => OutAddress((int)IoAddress.Display);

    private void OutAddress(int value)
    {
        OutAddress(_ioBus, _sut, value);
    }

    private void OutData(int value)
    {
        OutData(_ioBus, _sut, value);
    }

    private static void OutAddress(IIoBus ioBus, IDisplayAdapter displayAdapter, int value)
    {
        ioBus.CpuBus.SetValue(value.ToBinaryBools(8));
        ioBus.DataAddress.Value = true;
        ioBus.InputOutput.Value = true;
        ioBus.Clk.Set.Value = true;
        displayAdapter.Update();
        ClearStrobe(ioBus, displayAdapter);
    }

    private static void OutData(IIoBus ioBus, IDisplayAdapter displayAdapter, int value)
    {
        ioBus.CpuBus.SetValue(value.ToBinaryBools(8));
        ioBus.DataAddress.Value = false;
        ioBus.InputOutput.Value = true;
        ioBus.Clk.Set.Value = true;
        displayAdapter.Update();
        ClearStrobe(ioBus, displayAdapter);
    }

    // Drop the clock strobe so the write port is not re-triggered on later updates.
    private void ClearStrobe()
    {
        ClearStrobe(_ioBus, _sut);
    }

    private static void ClearStrobe(IIoBus ioBus, IDisplayAdapter displayAdapter)
    {
        ioBus.Clk.Set.Value = false;
        displayAdapter.Update();
    }
}
