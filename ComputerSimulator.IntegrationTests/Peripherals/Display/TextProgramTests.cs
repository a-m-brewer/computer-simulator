using System.IO;
using System.Linq;
using System.Text;
using ComputerSimulator.Assembler;
using ComputerSimulator.Core;
using ComputerSimulator.Core.Peripherals.Display;
using ComputerSimulator.Core.Peripherals.Display.Text;
using FluentAssertions;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests.Peripherals.Display;

public class TextProgramTests : IntegrationTestBase
{
    private const int FontBaseAddress = 0x2000;

    [TestCase(DisplayScanMode.GateLevel)]
    [TestCase(DisplayScanMode.ScanBuffer)]
    public void AssembledCpuProgramDrawsCharacterFromRamFont(DisplayScanMode scanMode)
    {
        const int width = 32;
        const int height = 16;
        var image = AssembleGeneratedTextProgram("H", width, height, cellX: 1, cellY: 1);

        var output = RunAndRender(image, scanMode, width, height);

        AssertGlyph(output, 'H', cellX: 1, cellY: 1);
    }

    [TestCase(DisplayScanMode.GateLevel)]
    [TestCase(DisplayScanMode.ScanBuffer)]
    public void AssembledCpuProgramDrawsAdjacentCharacters(DisplayScanMode scanMode)
    {
        const int width = 32;
        const int height = 16;
        var image = AssembleGeneratedTextProgram("HI", width, height, cellX: 0, cellY: 0);

        var output = RunAndRender(image, scanMode, width, height);

        AssertGlyph(output, 'H', cellX: 0, cellY: 0);
        AssertGlyph(output, 'I', cellX: 1, cellY: 0);
    }

    [Test]
    public void AssembledCpuProgramHandlesNewlineAndWrap()
    {
        const int width = 32;
        const int height = 24;
        var image = AssembleGeneratedTextProgram("HI\nABC", width, height, cellX: 2, cellY: 0);

        var output = RunAndRender(image, DisplayScanMode.ScanBuffer, width, height);

        AssertGlyph(output, 'H', cellX: 2, cellY: 0);
        AssertGlyph(output, 'I', cellX: 3, cellY: 0);
        AssertGlyph(output, 'A', cellX: 0, cellY: 1);
        AssertGlyph(output, 'B', cellX: 1, cellY: 1);
        AssertGlyph(output, 'C', cellX: 2, cellY: 1);
    }

    [Test]
    public void AssembledHelloWorldProgramDrawsHelloWorld()
    {
        const int width = 96;
        const int height = 16;
        var options = new AssemblerOptions();
        options.Defines["BYTES_PER_ROW"] = width / 8;
        var image = ComputerSimulator.IntegrationTests.Assembler.DogfoodProgramTests.AssembleProgram("hello-world.asm", options);

        var output = RunAndRender(image, DisplayScanMode.ScanBuffer, width, height, updates: 220_000);

        const string expected = "HELLO WORLD";
        for (var i = 0; i < expected.Length; i++)
        {
            AssertGlyph(output, expected[i], cellX: i, cellY: 0);
        }
    }

    [Test]
    public void AssembledTextImageContainsRamLoadableFontRom()
    {
        var image = AssembleGeneratedTextProgram("A", width: 16, height: 8, cellX: 0, cellY: 0);
        var fontRom = AsciiFont8x8.CreateRomImage();

        image.Skip(FontBaseAddress).Take(fontRom.Length)
            .Should()
            .Equal(fontRom);
    }

    private byte[] AssembleGeneratedTextProgram(string text, int width, int height, int cellX, int cellY)
    {
        var options = new AssemblerOptions();
        options.Defines["BYTES_PER_ROW"] = width / 8;
        var source = BuildDrawStringSource(text, width, height, cellX, cellY);
        var sourcePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "programs", "text-fixture.asm");
        return new ScottAssembler().AssembleTextOrThrow(source, sourcePath, options);
    }

    private FakeDisplayOutput RunAndRender(byte[] image, DisplayScanMode scanMode, int width, int height, int updates = 120_000)
    {
        var computerPart = ComponentFactory.CreateComputerPart();
        var display = new DisplayAdapter(computerPart.IoBus, width, height, scanMode, ComponentFactory, WireFactory);
        computerPart.IoBus.ConnectedComponents.Add(display);
        ProgramLoader.Load(computerPart.Ram, image);

        for (var i = 0; i < updates; i++)
        {
            computerPart.Update();
        }

        var output = new FakeDisplayOutput();
        output.Initialize(width, height);
        display.RenderFrame(output);
        return output;
    }

    private static string BuildDrawStringSource(string text, int width, int height, int cellX, int cellY)
    {
        var source = new StringBuilder();
        source.AppendLine(".equ IO_DISPLAY, 0x07");
        source.AppendLine(".equ FONT_BASE, 0x2000");
        source.AppendLine("LDI R2, IO_DISPLAY, R3");
        source.AppendLine("OUT ADDR, R2");

        var columns = width / AsciiFont8x8.GlyphWidth;
        var currentX = cellX;
        var currentY = cellY;
        foreach (var character in text)
        {
            if (character == '\n')
            {
                currentX = 0;
                currentY++;
                continue;
            }

            if (currentX >= columns)
            {
                currentX = 0;
                currentY++;
            }

            currentY.Should().BeLessThan(height / AsciiFont8x8.GlyphHeight);
            EmitDrawCharacter(source, character, width, currentX, currentY);
            currentX++;
        }

        source.AppendLine("HALT R0, R3");
        source.AppendLine(".org FONT_BASE");
        source.AppendLine(".incbin \"assets/font8x8.bin\"");
        return source.ToString();
    }

    private static void EmitDrawCharacter(StringBuilder source, char character, int width, int cellX, int cellY)
    {
        var bytesPerRow = width / AsciiFont8x8.GlyphWidth;
        for (var row = 0; row < AsciiFont8x8.GlyphHeight; row++)
        {
            var fontAddress = FontBaseAddress + (character * AsciiFont8x8.GlyphHeight) + row;
            var displayByteAddress = (((cellY * AsciiFont8x8.GlyphHeight) + row) * bytesPerRow) + cellX;

            source.AppendLine($"LDI R0, {fontAddress}, R3");
            source.AppendLine("LD R1, [R0]");
            source.AppendLine($"LDI R2, {displayByteAddress}, R3");
            source.AppendLine("OUT ADDR, R2");
            source.AppendLine("OUT DATA, R1");
        }
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
