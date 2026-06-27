using System.Linq;
using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Peripherals;
using ComputerSimulator.Core.Peripherals.Display;
using FluentAssertions;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests.Peripherals.Display;

public class IoBusControlTests : IntegrationTestBase
{
    private IIoBusControl _sut;

    [SetUp]
    public void SetUp()
    {
        _sut = ComponentFactory.CreateIoBusControl(
            WireFactory.CreateIoBus("io-bus"),
            WireFactory.CreateGroup<bool>(nameof(_sut.DisplayRamSetMarBus)),
            WireFactory.CreateWire<bool>(nameof(_sut.DisplayRamSetMarSet)),
            WireFactory.CreateGroup<bool>(nameof(_sut.DisplayRamInputBus)),
            WireFactory.CreateWire<bool>(nameof(_sut.DisplayRamSet))
        );
    }

    [Test]
    [TestCase(IoAddress.Keyboard, false)]
    [TestCase(IoAddress.Display, true)]
    public void CanSelectTheDisplay(IoAddress keyboardAddress, bool selected)
    {
        // Arrange
        _sut.IoBus.CpuBus.SetValue(keyboardAddress.ToBinaryBools(8));

        // Act
        _sut.Update();

        // Assert
        _sut.IoSelect
            .Output
            .Value
            .Should()
            .Be(selected);
    }

    private static int[] _canCheckIfAddressOutputModeTestCases =
        Enumerable.Range(0, 8).ToArray();

    [Test]
    [TestCaseSource(nameof(_canCheckIfAddressOutputModeTestCases))]
    public void CanCheckIfAddressOutputMode(int i)
    {
        // Arrange
        var testCase = i.ToBinaryBools(3);

        _sut.IoBus.DataAddress.Value = testCase[0];
        _sut.IoBus.InputOutput.Value = testCase[1];
        _sut.IoBus.Clk.Set.Value = testCase[2];

        // Act
        _sut.Update();

        // Assert
        _sut.IsAddressOutput.Output.Value
            .Should()
            .Be(i == 7);
    }

    [Test]
    public void OutAddrWithDisplayAddressSelectsTheDisplay()
    {
        SelectDisplay();

        _sut.DisplayAdapterActiveBit.Output.Value
            .Should()
            .BeTrue();
    }

    [Test]
    public void SelectionIsStickyAcrossSubsequentInstructions()
    {
        SelectDisplay();

        // A later OUT Addr carrying a non-display address must not deselect the display.
        OutAddress(42);

        _sut.DisplayAdapterActiveBit.Output.Value
            .Should()
            .BeTrue();
    }

    [Test]
    public void CanSendAddressToDisplayRam()
    {
        SelectDisplay();

        const int ramAddress = 42;
        OutAddress(ramAddress);

        _sut.DisplayRamSetMarBus.ToInt().Should().Be(ramAddress);
        _sut.DisplayRamSetMarSet.Value.Should().BeTrue();
        _sut.DisplayRamSet.Value.Should().BeFalse();
    }

    [Test]
    public void CanSendDataToDisplayRam()
    {
        SelectDisplay();
        OutAddress(1);

        const int data = 55;
        OutData(data);

        _sut.DisplayRamInputBus.ToInt().Should().Be(data);
        _sut.DisplayRamSet.Value.Should().BeTrue();
        _sut.DisplayRamSetMarSet.Value.Should().BeFalse();
    }

    private void SelectDisplay()
    {
        OutAddress((int)IoAddress.Display);
    }

    private void OutAddress(int value)
    {
        _sut.IoBus.CpuBus.SetValue(value.ToBinaryBools(8));
        _sut.IoBus.DataAddress.Value = true;
        _sut.IoBus.InputOutput.Value = true;
        _sut.IoBus.Clk.Set.Value = true;
        _sut.Update();
    }

    private void OutData(int value)
    {
        _sut.IoBus.CpuBus.SetValue(value.ToBinaryBools(8));
        _sut.IoBus.DataAddress.Value = false;
        _sut.IoBus.InputOutput.Value = true;
        _sut.IoBus.Clk.Set.Value = true;
        _sut.Update();
    }
}
