using ComputerSimulator.Core.Enums;
using ComputerSimulator.Core.Exceptions;
using ComputerSimulator.Core.Instructions;
using ComputerSimulator.Core.Peripherals;
using ComputerSimulator.Core.Peripherals.Display.Text;

namespace ComputerSimulator.Core.Programs;

public static class TextProgram
{
    public const int FontBaseAddress = 0x2000;

    private const int FontAddressRegister = 0;
    private const int GlyphRowRegister = 1;
    private const int DisplayAddressRegister = 2;
    private const int TempRegister = 3;

    public static byte[] BuildHelloWorldImage(int width, int height)
    {
        return BuildStringImage("HELLO WORLD", width, height, cellX: 0, cellY: 0);
    }

    public static byte[] BuildStringImage(string text, int width, int height, int cellX, int cellY)
    {
        var program = BuildStringProgram(text, width, height, cellX, cellY);
        if (program.Count > FontBaseAddress)
        {
            throw new ComputerSimulatorException($"Text program is {program.Count} bytes and overlaps the font ROM at {FontBaseAddress}");
        }

        var image = new byte[FontBaseAddress + AsciiFont8x8.RomByteCount];
        for (var i = 0; i < program.Count; i++)
        {
            image[i] = (byte)program[i];
        }

        AsciiFont8x8.CreateRomImage().CopyTo(image, FontBaseAddress);
        return image;
    }

    public static IReadOnlyList<int> BuildStringProgram(string text, int width, int height, int cellX, int cellY)
    {
        ValidateCell(width, height, cellX, cellY);

        var program = new MachineProgramBuilder();
        program.LoadConstant(DisplayAddressRegister, (int)IoAddress.Display, TempRegister);
        program.Add(InstructionSet.Out(DataAddress.Address, DisplayAddressRegister));

        EmitDrawString(program, text, width, height, cellX, cellY);

        program.Halt(FontAddressRegister, TempRegister);
        return program.ToBytes();
    }

    public static IReadOnlyList<int> BuildDrawCharacterProgram(char character, int width, int height, int cellX, int cellY)
    {
        return BuildStringProgram(character.ToString(), width, height, cellX, cellY);
    }

    private static void EmitDrawString(
        MachineProgramBuilder program,
        string text,
        int width,
        int height,
        int cellX,
        int cellY)
    {
        var columns = width / AsciiFont8x8.GlyphWidth;
        var currentX = cellX;
        var currentY = cellY;

        foreach (var character in text)
        {
            if (character == '\n')
            {
                currentX = 0;
                currentY++;
                EnsureRowInRange(height, currentY);
                continue;
            }

            if (currentX >= columns)
            {
                currentX = 0;
                currentY++;
            }

            ValidateCell(width, height, currentX, currentY);
            EmitDrawCharacter(program, character, width, currentX, currentY);
            currentX++;
        }
    }

    private static void EmitDrawCharacter(
        MachineProgramBuilder program,
        char character,
        int width,
        int cellX,
        int cellY)
    {
        var bytesPerRow = width / AsciiFont8x8.GlyphWidth;
        for (var row = 0; row < AsciiFont8x8.GlyphHeight; row++)
        {
            var fontAddress = FontBaseAddress + (character * AsciiFont8x8.GlyphHeight) + row;
            var displayByteAddress = (((cellY * AsciiFont8x8.GlyphHeight) + row) * bytesPerRow) + cellX;

            program.LoadConstant(FontAddressRegister, fontAddress, TempRegister);
            program.Add(InstructionSet.Ld(FontAddressRegister, GlyphRowRegister));
            program.LoadConstant(DisplayAddressRegister, displayByteAddress, TempRegister);
            program.Add(InstructionSet.Out(DataAddress.Address, DisplayAddressRegister));
            program.Add(InstructionSet.Out(DataAddress.Data, GlyphRowRegister));
        }
    }

    private static void ValidateCell(int width, int height, int cellX, int cellY)
    {
        if (cellX < 0 || cellX >= width / AsciiFont8x8.GlyphWidth)
        {
            throw new ComputerSimulatorException($"Character column {cellX} is outside the display");
        }

        EnsureRowInRange(height, cellY);
    }

    private static void EnsureRowInRange(int height, int cellY)
    {
        if (cellY < 0 || ((cellY + 1) * AsciiFont8x8.GlyphHeight) > height)
        {
            throw new ComputerSimulatorException($"Character row {cellY} is outside the display");
        }
    }
}
