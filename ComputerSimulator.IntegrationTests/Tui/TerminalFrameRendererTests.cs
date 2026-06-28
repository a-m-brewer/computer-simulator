using ComputerSimulator.Graphics;
using ComputerSimulator.Tui;
using FluentAssertions;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests.Tui;

public class TerminalFrameRendererTests
{
    [Test]
    public void RenderPacksPixelsIntoBrailleCells()
    {
        var pixels = new[] { true, true, true, true, true, true, true, true };
        var snapshot = new TerminalDisplaySnapshot(2, 4, pixels);

        var lines = TerminalFrameRenderer.Render(snapshot, TerminalPixelMode.Braille, 80, 24);

        lines.Should().ContainSingle().Which.Should().Be("\u28ff");
    }

    [Test]
    public void RenderCropsToVisibleSize()
    {
        var pixels = new bool[4 * 8];
        pixels[0] = true;
        pixels[(4 * 4) + 2] = true;
        var snapshot = new TerminalDisplaySnapshot(4, 8, pixels);

        var lines = TerminalFrameRenderer.Render(snapshot, TerminalPixelMode.Braille, 1, 1);

        lines.Should().ContainSingle().Which.Should().Be("\u2801");
    }

    [Test]
    public void RenderCanUseBlockPixels()
    {
        var snapshot = new TerminalDisplaySnapshot(3, 1, new[] { true, false, true });

        var lines = TerminalFrameRenderer.Render(snapshot, TerminalPixelMode.Block, 80, 24);

        lines.Should().ContainSingle().Which.Should().Be("█ █");
    }
}
