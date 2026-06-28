using ComputerSimulator.Core.Exceptions;
using ComputerSimulator.Core.Peripherals.Display;
using ComputerSimulator.Core.Peripherals.Display.Text;
using FluentAssertions;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests.Peripherals.Display;

public class GlyphRendererTests : IntegrationTestBase
{
    [TestCase(DisplayScanMode.GateLevel)]
    [TestCase(DisplayScanMode.ScanBuffer)]
    public void DrawCharacterWritesGlyphPixelsToDisplayRam(DisplayScanMode scanMode)
    {
        var display = CreateDisplay(scanMode, width: 32, height: 16);

        GlyphRenderer.DrawCharacter(display, 'H', cellX: 1, cellY: 1);

        var output = Render(display);

        AssertGlyph(output, 'H', cellX: 1, cellY: 1);
    }

    [TestCase(DisplayScanMode.GateLevel)]
    [TestCase(DisplayScanMode.ScanBuffer)]
    public void DrawStringWritesAdjacentCharacters(DisplayScanMode scanMode)
    {
        var display = CreateDisplay(scanMode, width: 32, height: 16);

        GlyphRenderer.DrawString(display, "HI", cellX: 0, cellY: 0);

        var output = Render(display);

        AssertGlyph(output, 'H', cellX: 0, cellY: 0);
        AssertGlyph(output, 'I', cellX: 1, cellY: 0);
    }

    [TestCase(DisplayScanMode.GateLevel)]
    [TestCase(DisplayScanMode.ScanBuffer)]
    public void DrawStringResetsToColumnZeroOnNewline(DisplayScanMode scanMode)
    {
        var display = CreateDisplay(scanMode, width: 32, height: 16);

        GlyphRenderer.DrawString(display, "HI\nA", cellX: 2, cellY: 0);

        var output = Render(display);

        AssertGlyph(output, 'H', cellX: 2, cellY: 0);
        AssertGlyph(output, 'I', cellX: 3, cellY: 0);
        AssertGlyph(output, 'A', cellX: 0, cellY: 1);
    }

    [TestCase(DisplayScanMode.GateLevel)]
    [TestCase(DisplayScanMode.ScanBuffer)]
    public void DrawStringWrapsToNextLine(DisplayScanMode scanMode)
    {
        var display = CreateDisplay(scanMode, width: 32, height: 16);

        GlyphRenderer.DrawString(display, "ABC", cellX: 2, cellY: 0);

        var output = Render(display);

        AssertGlyph(output, 'A', cellX: 2, cellY: 0);
        AssertGlyph(output, 'B', cellX: 3, cellY: 0);
        AssertGlyph(output, 'C', cellX: 0, cellY: 1);
    }

    [Test]
    public void DrawStringRejectsOverflowPastTheDisplay()
    {
        var display = CreateDisplay(DisplayScanMode.ScanBuffer, width: 16, height: 8);

        FluentActions.Invoking(() => GlyphRenderer.DrawString(display, "ABC", cellX: 0, cellY: 0))
            .Should()
            .Throw<ComputerSimulatorException>();
    }

    [Test]
    public void DrawStringRejectsNewlinesPastTheDisplay()
    {
        var display = CreateDisplay(DisplayScanMode.ScanBuffer, width: 16, height: 8);

        FluentActions.Invoking(() => GlyphRenderer.DrawString(display, "A\n", cellX: 0, cellY: 0))
            .Should()
            .Throw<ComputerSimulatorException>();
    }

    private static void AssertGlyph(FakeDisplayOutput output, char character, int cellX, int cellY)
    {
        var expectedRows = AsciiFont8x8.GetGlyphRows(character);
        var startX = cellX * AsciiFont8x8.GlyphWidth;
        var startY = cellY * AsciiFont8x8.GlyphHeight;

        for (var row = 0; row < AsciiFont8x8.GlyphHeight; row++)
        {
            for (var column = 0; column < AsciiFont8x8.GlyphWidth; column++)
            {
                var expected = (expectedRows[row] & (1 << column)) != 0;
                output.IsLit(startX + column, startY + row)
                    .Should()
                    .Be(expected, $"glyph pixel ({column},{row}) should match the font row");
            }
        }
    }

    private DisplayAdapter CreateDisplay(DisplayScanMode scanMode, int width, int height)
    {
        return new DisplayAdapter(
            WireFactory.CreateIoBus($"text-display-{scanMode}-{width}x{height}"),
            width,
            height,
            scanMode,
            ComponentFactory,
            WireFactory);
    }

    private static FakeDisplayOutput Render(IDisplayAdapter display)
    {
        var output = new FakeDisplayOutput();
        output.Initialize(display.Width, display.Height);
        display.RenderFrame(output);
        return output;
    }

    [Test]
    public void DrawCharacterRejectsCellsOutsideTheDisplay()
    {
        var display = CreateDisplay(DisplayScanMode.ScanBuffer, width: 16, height: 8);

        FluentActions.Invoking(() => GlyphRenderer.DrawCharacter(display, 'A', cellX: 2, cellY: 0))
            .Should()
            .Throw<ComputerSimulatorException>();
    }
}
