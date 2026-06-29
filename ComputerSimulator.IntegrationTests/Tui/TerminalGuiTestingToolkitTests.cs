using ComputerSimulator.Tui;
using FluentAssertions;
using NUnit.Framework;
using Terminal.Gui.App;
using Terminal.Gui.Drivers;
using Terminal.Gui.Input;
using Terminal.Gui.Testing;
using Terminal.Gui.Time;

namespace ComputerSimulator.IntegrationTests.Tui;

public class TerminalGuiTestingToolkitTests
{
    [Test]
    public void CanInjectKeyboardInputThroughTerminalGuiTestingToolkit()
    {
        using var app = Application.Create(new VirtualTimeProvider()).Init(DriverRegistry.Names.ANSI);
        var keyDownRaised = false;

        app.Keyboard.KeyDown += (_, _) => keyDownRaised = true;

        app.InjectKey(Key.Esc);

        keyDownRaised.Should().BeTrue();
    }

    [Test]
    public void MapsPrintableKeyToAscii()
    {
        TerminalKeyboardInput.TryMapKey(new Key('A'), out var keycode).Should().BeTrue();

        keycode.Should().Be((byte)'A');
    }

    [Test]
    public void MapsEnterToCarriageReturn()
    {
        TerminalKeyboardInput.TryMapKey(Key.Enter, out var keycode).Should().BeTrue();

        keycode.Should().Be(13);
    }

    [Test]
    public void MapsBackspaceToAsciiBackspace()
    {
        TerminalKeyboardInput.TryMapKey(Key.Backspace, out var keycode).Should().BeTrue();

        keycode.Should().Be(8);
    }

    [Test]
    public void IgnoresNonTextKeys()
    {
        TerminalKeyboardInput.TryMapKey(Key.Esc, out _).Should().BeFalse();
    }
}
