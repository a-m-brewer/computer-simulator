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

    [Test]
    public void KeyboardAdapterConsumesOnlyOneKeycodePerReadWindow()
    {
        // Fast typing queues several keys before the CPU reads. A single `In`
        // read must consume exactly one keycode no matter how many update
        // cycles its read window spans, otherwise queued keys are dropped.
        _keyboardInput.Push((byte)'A');
        _keyboardInput.Push((byte)'B');

        SelectKeyboard();

        // Hold the read window open across many updates (a real `In` keeps the
        // read-enable line high for far more than a handful of adapter updates).
        for (var i = 0; i < 20; i++)
        {
            ReadKeyboardData();
            _sut.IoBus.CpuBus.ToInt().Should().Be('A', "the read window must keep returning the first key");
        }

        // 'B' must still be queued for the next read.
        _keyboardInput.TryRead(out var remaining).Should().BeTrue();
        remaining.Should().Be((byte)'B');
    }

    [Test]
    public void KeyboardAdapterReadsQueuedKeysInOrderAcrossSeparateReads()
    {
        _keyboardInput.Push((byte)'A');
        _keyboardInput.Push((byte)'B');

        SelectKeyboard();

        ReadKeyboardData();
        _sut.IoBus.CpuBus.ToInt().Should().Be('A');

        ReleaseKeyboardData();
        ReadKeyboardData();
        _sut.IoBus.CpuBus.ToInt().Should().Be('B');

        ReleaseKeyboardData();
        ReadKeyboardData();
        _sut.IoBus.CpuBus.ToInt().Should().Be(0);
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
