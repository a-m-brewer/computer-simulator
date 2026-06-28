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
        var display = new DisplayAdapter(
            WireFactory.CreateIoBus("text-display"),
            width: 32,
            height: 16,
            scanMode,
            ComponentFactory,
            WireFactory);

        GlyphRenderer.DrawCharacter(display, 'H', cellX: 1, cellY: 1);

        var output = new FakeDisplayOutput();
        output.Initialize(display.Width, display.Height);
        display.RenderFrame(output);

        var expectedRows = AsciiFont8x8.GetGlyphRows('H');
        var startX = AsciiFont8x8.GlyphWidth;
        var startY = AsciiFont8x8.GlyphHeight;

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

    [Test]
    public void DrawCharacterRejectsCellsOutsideTheDisplay()
    {
        var display = new DisplayAdapter(
            WireFactory.CreateIoBus("small-text-display"),
            width: 16,
            height: 8,
            DisplayScanMode.ScanBuffer,
            ComponentFactory,
            WireFactory);

        FluentActions.Invoking(() => GlyphRenderer.DrawCharacter(display, 'A', cellX: 2, cellY: 0))
            .Should()
            .Throw<ComputerSimulatorException>();
    }
}
