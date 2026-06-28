using ComputerSimulator.Graphics;
using ComputerSimulator.Tui;
using FluentAssertions;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests.Tui;

public class ComputerSimulatorWindowTests
{
    [Test]
    public void WindowContainsDisplayLogsAndBlankRightPanel()
    {
        var window = new ComputerSimulatorWindow(new TerminalDisplayBuffer(), new TerminalSettings(), new TerminalLogSink());

        window.DisplayFrame.Title.ToString().Should().Be("Display");
        window.LogFrame.Title.ToString().Should().Be("Logs");
        window.RightFrame.SubViews.Should().BeEmpty();
        window.SubViews.Should().Contain(new[] { window.DisplayFrame, window.LogFrame, window.RightFrame });
    }
}
