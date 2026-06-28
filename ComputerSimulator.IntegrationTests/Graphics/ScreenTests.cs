using ComputerSimulator.Graphics;
using FluentAssertions;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests.Graphics;

public class ScreenTests
{
    [Test]
    public void FlushPacksPixelsIntoBrailleCells()
    {
        var console = new FakeConsole(80, 24);
        var screen = new Screen(console);
        screen.Initialize(2, 4);

        for (var y = 0; y < 4; y++)
        {
            for (var x = 0; x < 2; x++)
            {
                screen.SetPixel(x, y, true);
            }
        }

        screen.Flush();

        console.LastWrite.Should().Be("\u28ff");
    }

    [Test]
    public void FlushCropsToVisibleTerminalSize()
    {
        var console = new FakeConsole(1, 1);
        var screen = new Screen(console);
        screen.Initialize(4, 8);
        screen.SetPixel(0, 0, true);
        screen.SetPixel(2, 4, true);

        screen.Flush();

        console.LastWrite.Should().Be("\u2801");
    }

    [Test]
    public void FlushRendersLogsBelowTheDisplay()
    {
        var console = new FakeConsole(20, 6);
        var settings = new TerminalSettings { LogLines = 2 };
        var logSink = new TerminalLogSink();
        var screen = new Screen(console, settings, logSink);
        screen.Initialize(2, 4);
        logSink.Add("first log line");
        logSink.Add("second log line");

        screen.Flush();

        console.LastWrite.Should().Be(" \nfirst log line      \nsecond log line     ");
    }

    [Test]
    public void FlushWrapsLongLogsAcrossLogRows()
    {
        var console = new FakeConsole(10, 4);
        var settings = new TerminalSettings { LogLines = 2 };
        var logSink = new TerminalLogSink();
        var screen = new Screen(console, settings, logSink);
        screen.Initialize(2, 4);
        logSink.Add("0123456789abcdef");

        screen.Flush();

        console.LastWrite.Should().Be(" \n0123456789\nabcdef    ");
    }

    private class FakeConsole : IConsole
    {
        public FakeConsole(int windowWidth, int windowHeight)
        {
            WindowWidth = windowWidth;
            WindowHeight = windowHeight;
        }

        public int WindowHeight { get; }

        public int WindowWidth { get; }

        public string LastWrite { get; private set; } = string.Empty;

        public void SetCursorPosition(int left, int top)
        {
        }

        public void Clear()
        {
        }

        public void Write(string value)
        {
            LastWrite = value;
        }
    }
}
