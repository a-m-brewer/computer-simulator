using ComputerSimulator.Core.Peripherals.Keyboard;
using FluentAssertions;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests.Peripherals.Keyboard;

public class BufferedKeyboardInputTests
{
    [Test]
    public void EmptyInputReturnsFalseAndZero()
    {
        var input = new BufferedKeyboardInput();

        input.TryRead(out var keycode).Should().BeFalse();
        keycode.Should().Be(0);
    }

    [Test]
    public void PushedKeycodeIsReadOnce()
    {
        var input = new BufferedKeyboardInput();

        input.Push((byte)'A');

        input.TryRead(out var keycode).Should().BeTrue();
        keycode.Should().Be((byte)'A');
        input.TryRead(out _).Should().BeFalse();
    }

    [Test]
    public void KeycodesAreReadInFifoOrder()
    {
        var input = new BufferedKeyboardInput();

        input.Push((byte)'A');
        input.Push((byte)'B');

        input.TryRead(out var first).Should().BeTrue();
        input.TryRead(out var second).Should().BeTrue();
        first.Should().Be((byte)'A');
        second.Should().Be((byte)'B');
    }
}
