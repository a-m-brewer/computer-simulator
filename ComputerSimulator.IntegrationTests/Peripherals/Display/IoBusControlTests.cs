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

    private static object[] _canCheckIfAddressOutputModeTestCases =
        Enumerable.Range(0, 8)
            .Select(s => new[] { s })
            .ToArray();
    
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
}