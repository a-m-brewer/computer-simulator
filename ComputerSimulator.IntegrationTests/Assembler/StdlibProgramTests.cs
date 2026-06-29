using System;
using System.IO;
using ComputerSimulator.Assembler;
using ComputerSimulator.Core;
using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Parts;
using ComputerSimulator.Core.Peripherals.Display;
using ComputerSimulator.Core.Peripherals.Display.Text;
using ComputerSimulator.Core.Peripherals.Keyboard;
using ComputerSimulator.IntegrationTests.Peripherals.Display;
using FluentAssertions;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests.Assembler;

public class StdlibProgramTests : IntegrationTestBase
{
    private const int ResultMul = 0x0300;
    private const int ResultDivQuotient = 0x0301;
    private const int ResultDivRemainder = 0x0302;
    private const int MemcpySource = 0x0320;
    private const int MemcpyDestination = 0x0340;
    private const int ReadLineBuffer = 0x0360;
    private const int ReadLineCount = 0x0370;

    [Test]
    public void MathRoutinesCanBeIncludedAndCalled()
    {
        var image = AssembleStdlibFixture("""
            JMP16 Start, R0, R1
            .org 0x20
            .include "stdlib/math.asm"

            Start:
            LDI R0, 6
            LDI R1, 7
            LDI R3, AfterMul
            JMP stdlib_mul

            AfterMul:
            LDI R2, RESULT_MUL, R1
            ST [R2], R0

            LDI R0, 17
            LDI R1, 5
            LDI R3, AfterDiv
            JMP stdlib_div

            AfterDiv:
            LDI R2, RESULT_DIV_QUOTIENT, R3
            ST [R2], R0
            LDI R2, RESULT_DIV_REMAINDER, R3
            ST [R2], R1
            HALT R0, R3

            .org RESULT_MUL
            .byte 0, 0, 0
            """, options =>
        {
            options.Defines["RESULT_MUL"] = ResultMul;
            options.Defines["RESULT_DIV_QUOTIENT"] = ResultDivQuotient;
            options.Defines["RESULT_DIV_REMAINDER"] = ResultDivRemainder;
        });

        var computerPart = RunProgram(image, updates: 80_000);

        ReadRam(computerPart, ResultMul).Should().Be(42);
        ReadRam(computerPart, ResultDivQuotient).Should().Be(3);
        ReadRam(computerPart, ResultDivRemainder).Should().Be(2);
    }

    [Test]
    public void MemcpyRoutineCanBeIncludedAndCalled()
    {
        var image = AssembleStdlibFixture("""
            JMP16 Start, R0, R1
            .org 0x20
            .include "stdlib/memory.asm"

            Start:
            LDI R0, STDLIB_MEMCPY_LENGTH
            LDI R1, 4
            ST [R0], R1
            LDI R0, DESTINATION, R2
            LDI R1, SOURCE, R2
            LDI R3, AfterMemcpy
            JMP stdlib_memcpy

            AfterMemcpy:
            HALT R0, R2

            .org SOURCE
            .byte 0x11, 0x22, 0x33, 0x44
            .org DESTINATION
            .byte 0, 0, 0, 0
            """, options =>
        {
            options.Defines["SOURCE"] = MemcpySource;
            options.Defines["DESTINATION"] = MemcpyDestination;
        });

        var computerPart = RunProgram(image, updates: 80_000);

        ReadRam(computerPart, MemcpyDestination).Should().Be(0x11);
        ReadRam(computerPart, MemcpyDestination + 1).Should().Be(0x22);
        ReadRam(computerPart, MemcpyDestination + 2).Should().Be(0x33);
        ReadRam(computerPart, MemcpyDestination + 3).Should().Be(0x44);
    }

    [Test]
    public void PrintCharRoutineCanBeIncludedAndCalled()
    {
        const int width = 32;
        const int height = 16;
        var image = AssembleStdlibFixture("""
            JMP16 Start, R0, R1
            .org 0x20
            .include "stdlib/display.asm"

            Start:
            LDI R1, 'A'
            LDI R2, 1
            LDI R3, Done, R0
            JMP stdlib_print_char

            Done:
            HALT R0, R1

            .org FONT_BASE
            .incbin "assets/font8x8.bin"
            """, options => options.Defines["BYTES_PER_ROW"] = width / 8);

        var output = RunDisplayProgram(image, width, height, updates: 60_000);

        AssertGlyph(output, 'A', cellX: 1, cellY: 0);
    }

    [Test]
    public void PrintStringRoutineCanBeIncludedAndCalled()
    {
        const int width = 32;
        const int height = 16;
        var image = AssembleStdlibFixture("""
            JMP16 Start, R0, R1
            .org 0x20
            .include "stdlib/display.asm"

            Start:
            LDI R0, Message, R2
            LDI R2, 0
            LDI R3, Done, R1
            JMP stdlib_print_string

            Done:
            HALT R0, R1

            Message:
            .asciz "HI"

            .org FONT_BASE
            .incbin "assets/font8x8.bin"
            """, options => options.Defines["BYTES_PER_ROW"] = width / 8);

        var output = RunDisplayProgram(image, width, height, updates: 100_000);

        AssertGlyph(output, 'H', cellX: 0, cellY: 0);
        AssertGlyph(output, 'I', cellX: 1, cellY: 0);
    }

    [Test]
    public void ReadLineRoutineCanBeIncludedAndCalled()
    {
        var image = AssembleStdlibFixture("""
            JMP16 Start, R0, R1
            .org 0x20
            .include "stdlib/keyboard.asm"

            Start:
            LDI R0, BUFFER, R2
            LDI R1, 8
            LDI R3, AfterRead, R2
            JMP stdlib_read_line

            AfterRead:
            LDI R0, COUNT_RESULT, R2
            ST [R0], R1
            HALT R0, R2

            .org BUFFER
            .byte 0, 0, 0, 0, 0, 0, 0, 0, 0
            .org COUNT_RESULT
            .byte 0
            """, options =>
        {
            options.Defines["BUFFER"] = ReadLineBuffer;
            options.Defines["COUNT_RESULT"] = ReadLineCount;
        });

        var keyboardInput = GetRequiredService<IKeyboardInput>();
        while (keyboardInput.TryRead(out _))
        {
        }

        var computerPart = ComponentFactory.CreateComputerPart();
        var keyboard = ComponentFactory.CreateKeyboardAdapter(computerPart.IoBus);
        computerPart.IoBus.ConnectedComponents.Add(keyboard);
        ProgramLoader.Load(computerPart.Ram, image);

        keyboardInput.Push((byte)'A');
        keyboardInput.Push((byte)'B');
        keyboardInput.Push(13);

        for (var i = 0; i < 120_000; i++)
        {
            computerPart.Update();
        }

        ReadRam(computerPart, ReadLineBuffer).Should().Be('A');
        ReadRam(computerPart, ReadLineBuffer + 1).Should().Be('B');
        ReadRam(computerPart, ReadLineBuffer + 2).Should().Be(0);
        ReadRam(computerPart, ReadLineCount).Should().Be(2);
    }

    private static byte[] AssembleStdlibFixture(string source, Action<AssemblerOptions>? configureOptions = null)
    {
        var options = new AssemblerOptions();
        configureOptions?.Invoke(options);
        var sourcePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "programs", "stdlib-fixture.asm");
        File.WriteAllText(sourcePath, source);
        return new ScottAssembler().AssembleFileOrThrow(sourcePath, options);
    }

    private IComputerPart RunProgram(byte[] image, int updates)
    {
        var computerPart = ComponentFactory.CreateComputerPart();
        ProgramLoader.Load(computerPart.Ram, image);

        for (var i = 0; i < updates; i++)
        {
            computerPart.Update();
        }

        return computerPart;
    }

    private FakeDisplayOutput RunDisplayProgram(byte[] image, int width, int height, int updates)
    {
        var computerPart = ComponentFactory.CreateComputerPart();
        var display = new DisplayAdapter(computerPart.IoBus, width, height, DisplayScanMode.ScanBuffer, ComponentFactory, WireFactory);
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

    private static int ReadRam(IComputerPart computerPart, int address)
    {
        return computerPart.Ram.GetSlot(address & 0xFF, address >> 8).Memory.StoredValue.ToInt();
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
