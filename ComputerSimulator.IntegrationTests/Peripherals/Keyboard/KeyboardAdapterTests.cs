using System.Linq;
using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Peripherals;
using ComputerSimulator.Core.Peripherals.Keyboard;
using FluentAssertions;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests.Peripherals.Keyboard;

public class KeyboardAdapterTests : IntegrationTestBase
{
    private static int[] _keyboardInputs =
        Enumerable.Range(0, 128).ToArray();
    
    private IKeyboardAdapter _sut;
    
    [SetUp]
    public void SetUp()
    {
        _sut = ComponentFactory.CreateKeyboardAdapter(
            WireFactory.CreateIoBus("io-bus"),
            WireFactory.CreateGroup<bool>(8, $"input"));
    }

    [Test]
    [TestCaseSource(nameof(_keyboardInputs))]
    public void KeyboardAdapterOutputsToBus(int keycode)
    {
        // Arrange
        var expected = (char)keycode;
        
        _sut.IoBus.CpuBus.SetValue(IoAddress.Keyboard.ToBinaryBools(8));
        _sut.Input.SetValue(expected.ToBinaryBools());

        _sut.IoBus.Clk.Set.Value = true;
        _sut.IoBus.DataAddress.Value = true;
        _sut.IoBus.InputOutput.Value = true;
        
        // Act
        _sut.Update();
        
        _sut.IoBus.Clk.Enable.Value = true;
        _sut.IoBus.Clk.Set.Value = false;
        _sut.IoBus.DataAddress.Value = false;
        _sut.IoBus.InputOutput.Value = false;
        
        _sut.Update();
        
        // Assert
        ((char)
                _sut.IoBus.CpuBus.ToInt())
            .Should()
            .Be(expected);
    }
}