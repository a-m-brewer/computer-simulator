using System.Linq;
using ComputerSimulator.Core;
using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Parts;
using ComputerSimulator.Core.Peripherals.Display;
using ComputerSimulator.Core.Peripherals.Display.Text;
using ComputerSimulator.Core.Peripherals.Keyboard;
using ComputerSimulator.Core.Programs;
using ComputerSimulator.IntegrationTests.Peripherals.Display;
using FluentAssertions;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests.Peripherals.Keyboard;

public class EchoProgramTests : IntegrationTestBase
{
    [Test]
    public void EchoProgramDrawsQueuedCharacter()
    {
        var output = RunEchoProgram(32, 16, 'A');

        AssertGlyph(output, 'A', cellX: 0, cellY: 0);
    }

    [Test]
    public void EchoProgramDrawsCharacterPressedDuringPolling()
    {
        // Key is pushed AFTER the computer enters the poll loop — the runtime scenario.
        const int width = 32;
        const int height = 16;

        var keyboardInput = GetRequiredService<IKeyboardInput>();
        while (keyboardInput.TryRead(out _)) { }

        var computerPart = ComponentFactory.CreateComputerPart();
        var display = new DisplayAdapter(computerPart.IoBus, width, height, DisplayScanMode.ScanBuffer, ComponentFactory, WireFactory);
        var keyboard = ComponentFactory.CreateKeyboardAdapter(computerPart.IoBus);
        computerPart.IoBus.ConnectedComponents.Add(display);
        computerPart.IoBus.ConnectedComponents.Add(keyboard);
        ProgramLoader.Load(computerPart.Ram, EchoProgram.BuildImage(width, height));

        // Run until the program is inside the Poll loop — ~5000 updates with no key queued
        for (var i = 0; i < 5_000; i++)
        {
            computerPart.Update();
        }

        // Now push the key — simulating what the TUI does at runtime
        keyboardInput.Push((byte)'A');

        var drawn = false;
        for (var i = 0; i < 30_000; i++)
        {
            computerPart.Update();
            if (ReadVariable(computerPart, EchoProgram.CursorAddress) == 1
                && ReadVariable(computerPart, EchoProgram.CursorColumn) == 1)
            {
                drawn = true;
                break;
            }
        }

        drawn.Should().BeTrue("echo program should draw a character pressed during polling");

        var output = new FakeDisplayOutput();
        output.Initialize(width, height);
        display.RenderFrame(output);
        AssertGlyph(output, 'A', cellX: 0, cellY: 0);
    }

    [Test]
    public void EchoProgramDrawsCharacterWithRuntimeDisplaySize()
    {
        // Matches appsettings.json: ScreenWidth=96, ScreenHeight=48
        var output = RunEchoProgram(96, 48, 'A');
        AssertGlyph(output, 'A', cellX: 0, cellY: 0);
    }

    [Test]
    public void EchoProgramExactRuntimeScenario()
    {
        // Exactly mirrors appsettings.json runtime: 96x48, GateLevel, key pressed DURING polling.
        const int width = 96;
        const int height = 48;

        var keyboardInput = GetRequiredService<IKeyboardInput>();
        while (keyboardInput.TryRead(out _)) { }

        var computerPart = ComponentFactory.CreateComputerPart();
        var display = new DisplayAdapter(computerPart.IoBus, width, height, DisplayScanMode.GateLevel, ComponentFactory, WireFactory);
        var keyboard = ComponentFactory.CreateKeyboardAdapter(computerPart.IoBus);
        computerPart.IoBus.ConnectedComponents.Add(display);
        computerPart.IoBus.ConnectedComponents.Add(keyboard);
        ProgramLoader.Load(computerPart.Ram, EchoProgram.BuildImage(width, height));

        for (var i = 0; i < 5_000; i++)
        {
            computerPart.Update();
        }

        keyboardInput.Push((byte)'a'); // lowercase, exactly what the user pressed

        var drawn = false;
        for (var i = 0; i < 60_000; i++)
        {
            computerPart.Update();
            if (ReadVariable(computerPart, EchoProgram.CursorAddress) == 1
                && ReadVariable(computerPart, EchoProgram.CursorColumn) == 1)
            {
                drawn = true;
                break;
            }
        }

        drawn.Should().BeTrue("echo program should draw a lowercase key pressed during polling at runtime size");

        var output = new FakeDisplayOutput();
        output.Initialize(width, height);
        display.RenderFrame(output);
        AssertGlyph(output, 'a', cellX: 0, cellY: 0);
    }

    [Test]
    public void EchoProgramRendersCorrectlyInGateLevelMode()
    {
        // GateLevel is the runtime display scan mode; ScanBuffer is used in other tests.
        const int width = 32;
        const int height = 16;

        var keyboardInput = GetRequiredService<IKeyboardInput>();
        while (keyboardInput.TryRead(out _)) { }

        var computerPart = ComponentFactory.CreateComputerPart();
        var display = new DisplayAdapter(computerPart.IoBus, width, height, DisplayScanMode.GateLevel, ComponentFactory, WireFactory);
        var keyboard = ComponentFactory.CreateKeyboardAdapter(computerPart.IoBus);
        computerPart.IoBus.ConnectedComponents.Add(display);
        computerPart.IoBus.ConnectedComponents.Add(keyboard);
        ProgramLoader.Load(computerPart.Ram, EchoProgram.BuildImage(width, height));

        keyboardInput.Push((byte)'A');

        for (var i = 0; i < 30_000; i++)
        {
            computerPart.Update();
            if (ReadVariable(computerPart, EchoProgram.CursorAddress) == 1
                && ReadVariable(computerPart, EchoProgram.CursorColumn) == 1)
            {
                break;
            }
        }

        var output = new FakeDisplayOutput();
        output.Initialize(width, height);
        display.RenderFrame(output);
        AssertGlyph(output, 'A', cellX: 0, cellY: 0);
    }

    [Test]
    public void EchoImageContainsRamLoadableFontRom()
    {
        var image = EchoProgram.BuildImage(width: 32, height: 16);
        var fontRom = AsciiFont8x8.CreateRomImage();

        image.Skip(TextProgram.FontBaseAddress).Take(fontRom.Length)
            .Should()
            .Equal(fontRom);
    }

    private FakeDisplayOutput RunEchoProgram(int width, int height, params char[] keys)
    {
        var keyboardInput = GetRequiredService<IKeyboardInput>();
        while (keyboardInput.TryRead(out _))
        {
        }

        var computerPart = ComponentFactory.CreateComputerPart();
        var display = new DisplayAdapter(computerPart.IoBus, width, height, DisplayScanMode.ScanBuffer, ComponentFactory, WireFactory);
        var keyboard = ComponentFactory.CreateKeyboardAdapter(computerPart.IoBus);
        computerPart.IoBus.ConnectedComponents.Add(display);
        computerPart.IoBus.ConnectedComponents.Add(keyboard);
        ProgramLoader.Load(computerPart.Ram, EchoProgram.BuildImage(width, height));

        var expectedCursor = 0;
        var expectedColumn = 0;
        var expectedLineBase = 0;

        foreach (var key in keys)
        {
            keyboardInput.Push((byte)key);
            switch (key)
            {
                case (char)13:
                    expectedLineBase += width;
                    expectedCursor = expectedLineBase;
                    expectedColumn = 0;
                    break;
                case (char)8 when expectedColumn > 0:
                    expectedCursor--;
                    expectedColumn--;
                    break;
                case (char)8:
                    break;
                default:
                    expectedCursor++;
                    expectedColumn++;
                    break;
            }

            for (var i = 0; i < 30_000; i++)
            {
                computerPart.Update();
                if (ReadVariable(computerPart, EchoProgram.CursorAddress) == expectedCursor
                    && ReadVariable(computerPart, EchoProgram.CursorColumn) == expectedColumn
                    && ReadVariable(computerPart, EchoProgram.LineBaseAddress) == expectedLineBase)
                {
                    break;
                }
            }
        }

        var output = new FakeDisplayOutput();
        output.Initialize(width, height);
        display.RenderFrame(output);
        return output;
    }

    private static int ReadVariable(IComputerPart computerPart, int address)
    {
        return computerPart.Ram.GetSlot(address, 0).Memory.StoredValue.ToInt();
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
