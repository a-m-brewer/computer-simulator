using System.Linq;
using ComputerSimulator.Core;
using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Peripherals.Display;
using ComputerSimulator.Core.Peripherals.Display.Text;
using ComputerSimulator.Core.Programs;
using FluentAssertions;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests.Peripherals.Display;

public class TextProgramTests : IntegrationTestBase
{
    [TestCase(DisplayScanMode.GateLevel)]
    [TestCase(DisplayScanMode.ScanBuffer)]
    public void CpuProgramDrawsCharacterFromRamFont(DisplayScanMode scanMode)
    {
        const int width = 32;
        const int height = 16;
        var image = TextProgram.BuildStringImage("H", width, height, cellX: 1, cellY: 1);

        var output = RunAndRender(image, TextProgram.BuildStringProgram("H", width, height, 1, 1).Count, scanMode, width, height);

        AssertGlyph(output, 'H', cellX: 1, cellY: 1);
    }

    [TestCase(DisplayScanMode.GateLevel)]
    [TestCase(DisplayScanMode.ScanBuffer)]
    public void CpuProgramDrawsAdjacentCharacters(DisplayScanMode scanMode)
    {
        const int width = 32;
        const int height = 16;
        var image = TextProgram.BuildStringImage("HI", width, height, cellX: 0, cellY: 0);

        var output = RunAndRender(image, TextProgram.BuildStringProgram("HI", width, height, 0, 0).Count, scanMode, width, height);

        AssertGlyph(output, 'H', cellX: 0, cellY: 0);
        AssertGlyph(output, 'I', cellX: 1, cellY: 0);
    }

    [Test]
    public void CpuProgramHandlesNewlineAndWrap()
    {
        const int width = 32;
        const int height = 24;
        var image = TextProgram.BuildStringImage("HI\nABC", width, height, cellX: 2, cellY: 0);

        var output = RunAndRender(image, TextProgram.BuildStringProgram("HI\nABC", width, height, 2, 0).Count, DisplayScanMode.ScanBuffer, width, height);

        AssertGlyph(output, 'H', cellX: 2, cellY: 0);
        AssertGlyph(output, 'I', cellX: 3, cellY: 0);
        AssertGlyph(output, 'A', cellX: 0, cellY: 1);
        AssertGlyph(output, 'B', cellX: 1, cellY: 1);
        AssertGlyph(output, 'C', cellX: 2, cellY: 1);
    }

    [Test]
    public void CpuProgramDrawsHelloWorld()
    {
        const int width = 96;
        const int height = 16;
        var image = TextProgram.BuildHelloWorldImage(width, height);

        var output = RunAndRender(image, TextProgram.BuildStringProgram("HELLO WORLD", width, height, 0, 0).Count, DisplayScanMode.ScanBuffer, width, height);

        var expected = "HELLO WORLD";
        for (var i = 0; i < expected.Length; i++)
        {
            AssertGlyph(output, expected[i], cellX: i, cellY: 0);
        }
    }

    [Test]
    public void TextImageContainsRamLoadableFontRom()
    {
        var image = TextProgram.BuildStringImage("A", width: 16, height: 8, cellX: 0, cellY: 0);
        var fontRom = AsciiFont8x8.CreateRomImage();

        image.Skip(TextProgram.FontBaseAddress).Take(fontRom.Length)
            .Should()
            .Equal(fontRom);
    }

    private FakeDisplayOutput RunAndRender(byte[] image, int programByteCount, DisplayScanMode scanMode, int width, int height)
    {
        var computerPart = ComponentFactory.CreateComputerPart();
        var display = new DisplayAdapter(computerPart.IoBus, width, height, scanMode, ComponentFactory, WireFactory);
        computerPart.IoBus.ConnectedComponents.Add(display);
        ProgramLoader.Load(computerPart.Ram, image);

        for (var i = 0; i < programByteCount * 32; i++)
        {
            computerPart.Update();
        }

        var output = new FakeDisplayOutput();
        output.Initialize(width, height);
        display.RenderFrame(output);
        return output;
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
}
