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
    private IKeyboardInput _keyboardInput = null!;
    
    [SetUp]
    public void SetUp()
    {
        _keyboardInput = GetRequiredService<IKeyboardInput>();
        while (_keyboardInput.TryRead(out _))
        {
        }

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

        _keyboardInput.Push((byte)keycode);
        _sut.IoBus.CpuBus.SetValue(IoAddress.Keyboard.ToBinaryBools(8));

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

    [Test]
    public void KeyboardAdapterReturnsZeroAfterKeycodeIsRead()
    {
        _keyboardInput.Push((byte)'A');
        SelectKeyboard();

        ReadKeyboardData();
        _sut.IoBus.CpuBus.ToInt().Should().Be('A');

        ReleaseKeyboardData();
        ReadKeyboardData();
        _sut.IoBus.CpuBus.ToInt().Should().Be(0);
    }

    [Test]
    public void KeyboardAdapterDoesNotConsumeKeycodeWhenNotSelected()
    {
        _keyboardInput.Push((byte)'B');

        ReadKeyboardData();
        _sut.IoBus.CpuBus.ToInt().Should().Be(0);

        SelectKeyboard();
        ReadKeyboardData();
        _sut.IoBus.CpuBus.ToInt().Should().Be('B');
    }

    private void SelectKeyboard()
    {
        _sut.IoBus.CpuBus.SetValue((int)IoAddress.Keyboard);
        _sut.IoBus.Clk.Set.Value = true;
        _sut.IoBus.Clk.Enable.Value = false;
        _sut.IoBus.DataAddress.Value = true;
        _sut.IoBus.InputOutput.Value = true;
        _sut.Update();
    }

    private void ReadKeyboardData()
    {
        _sut.IoBus.CpuBus.Reset();
        _sut.IoBus.Clk.Set.Value = false;
        _sut.IoBus.Clk.Enable.Value = true;
        _sut.IoBus.DataAddress.Value = false;
        _sut.IoBus.InputOutput.Value = false;
        _sut.Update();
    }

    private void ReleaseKeyboardData()
    {
        _sut.IoBus.Clk.Enable.Value = false;
        _sut.Update();
    }
}
