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
}
