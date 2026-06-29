using ComputerSimulator.Core.Enums;
using ComputerSimulator.Core.Exceptions;
using ComputerSimulator.Core.Instructions;
using ComputerSimulator.Core.Peripherals;
using ComputerSimulator.Core.Peripherals.Display.Text;

namespace ComputerSimulator.Core.Programs;

public static class EchoProgram
{
    public const int CursorAddress = 0x02;
    public const int CursorColumn = 0x03;
    public const int LineBaseAddress = 0x04;

    private const int AddressRegister = 0;
    private const int KeyRegister = 1;
    private const int CursorRegister = 2;
    private const int TempRegister = 3;

    public static byte[] BuildImage(int width, int height)
    {
        var program = BuildProgram(width, height);
        if (program.Count > TextProgram.FontBaseAddress)
        {
            throw new ComputerSimulatorException($"Echo program is {program.Count} bytes and overlaps the font ROM at {TextProgram.FontBaseAddress}");
        }

        var image = new byte[TextProgram.FontBaseAddress + AsciiFont8x8.RomByteCount];
        for (var i = 0; i < program.Count; i++)
        {
            image[i] = (byte)program[i];
        }

        AsciiFont8x8.CreateRomImage().CopyTo(image, TextProgram.FontBaseAddress);
        return image;
    }

    public static IReadOnlyList<int> BuildProgram(int width, int height)
    {
        ValidateDisplay(width, height);

        var program = new MachineProgramBuilder();

        program.Jump("Start");
        program.Add(0);
        program.Add(0);
        program.Add(0);

        program.MarkLabel("Start");

        SelectDevice(program, IoAddress.Display);
        SelectDevice(program, IoAddress.Keyboard);

        program.MarkLabel("Poll");
        SelectDevice(program, IoAddress.Keyboard);
        program.Add(InstructionSet.In(DataAddress.Data, KeyRegister));

        JumpIfRegisterEquals(program, KeyRegister, 0, "Poll");
        JumpIfRegisterEquals(program, KeyRegister, 13, "Enter");
        JumpIfRegisterEquals(program, KeyRegister, 8, "Backspace");
        program.JumpLong("Printable", AddressRegister, TempRegister);

        program.MarkLabel("Enter");
        EmitEnter(program, width);
        program.JumpLong("Poll", AddressRegister, TempRegister);

        program.MarkLabel("Backspace");
        EmitBackspace(program, width);
        program.JumpLong("Poll", AddressRegister, TempRegister);

        program.MarkLabel("Printable");
        EmitDrawCharacter(program, width);
        IncrementVariable(program, CursorAddress, 1);
        IncrementVariable(program, CursorColumn, 1);
        program.JumpLong("Poll", AddressRegister, TempRegister);

        return program.ToBytes();
    }

    private static void SelectDevice(MachineProgramBuilder program, IoAddress address)
    {
        program.LoadConstant(AddressRegister, (int)address, TempRegister);
        program.Add(InstructionSet.Out(DataAddress.Address, AddressRegister));
    }

    private static void JumpIfRegisterEquals(MachineProgramBuilder program, int register, int value, string label)
    {
        program.LoadConstant(AddressRegister, value, TempRegister);
        program.Add(InstructionSet.Clf);
        program.Add(InstructionSet.Cmp(register, AddressRegister));
        program.JumpIf(JumpCondition.Equal, label);
    }

    private static void EmitEnter(MachineProgramBuilder program, int width)
    {
        LoadVariable(program, LineBaseAddress, CursorRegister);
        program.LoadConstant(TempRegister, width, KeyRegister);
        program.Add(InstructionSet.Add(TempRegister, CursorRegister));
        StoreVariable(program, LineBaseAddress, CursorRegister);
        StoreVariable(program, CursorAddress, CursorRegister);
        StoreConstant(program, CursorColumn, 0);
    }

    private static void EmitBackspace(MachineProgramBuilder program, int width)
    {
        LoadVariable(program, CursorColumn, CursorRegister);
        JumpIfRegisterEquals(program, CursorRegister, 0, "Poll");

        IncrementVariable(program, CursorColumn, 0xFFFF);
        IncrementVariable(program, CursorAddress, 0xFFFF);
        program.LoadConstant(KeyRegister, ' ', TempRegister);
        EmitDrawCharacter(program, width);
    }

    private static void EmitDrawCharacter(MachineProgramBuilder program, int width)
    {
        var bytesPerRow = width / AsciiFont8x8.GlyphWidth;

        program.LoadConstant(AddressRegister, 0, TempRegister);
        program.Add(InstructionSet.Add(KeyRegister, AddressRegister));
        for (var i = 0; i < 3; i++)
        {
            program.Add(InstructionSet.Shl(AddressRegister, AddressRegister));
        }

        program.LoadConstant(TempRegister, TextProgram.FontBaseAddress, CursorRegister);
        program.Add(InstructionSet.Add(TempRegister, AddressRegister));
        program.LoadConstant(CursorRegister, CursorAddress, TempRegister);
        program.Add(InstructionSet.Ld(CursorRegister, CursorRegister));

        for (var row = 0; row < AsciiFont8x8.GlyphHeight; row++)
        {
            program.Add(InstructionSet.Ld(AddressRegister, KeyRegister));
            program.Add(InstructionSet.Out(DataAddress.Address, CursorRegister));
            program.Add(InstructionSet.Out(DataAddress.Data, KeyRegister));

            if (row == AsciiFont8x8.GlyphHeight - 1)
            {
                continue;
            }

            program.LoadConstant(TempRegister, 1, KeyRegister);
            program.Add(InstructionSet.Add(TempRegister, AddressRegister));
            program.LoadConstant(TempRegister, bytesPerRow, KeyRegister);
            program.Add(InstructionSet.Add(TempRegister, CursorRegister));
        }
    }

    private static void IncrementVariable(MachineProgramBuilder program, int address, int increment)
    {
        LoadVariable(program, address, CursorRegister);
        program.LoadConstant(TempRegister, increment, KeyRegister);
        program.Add(InstructionSet.Add(TempRegister, CursorRegister));
        StoreVariable(program, address, CursorRegister);
    }

    private static void LoadVariable(MachineProgramBuilder program, int address, int targetRegister)
    {
        program.LoadConstant(AddressRegister, address, TempRegister);
        program.Add(InstructionSet.Ld(AddressRegister, targetRegister));
    }

    private static void StoreVariable(MachineProgramBuilder program, int address, int sourceRegister)
    {
        program.LoadConstant(AddressRegister, address, TempRegister);
        program.Add(InstructionSet.St(AddressRegister, sourceRegister));
    }

    private static void StoreConstant(MachineProgramBuilder program, int address, int value)
    {
        program.LoadConstant(KeyRegister, value, TempRegister);
        StoreVariable(program, address, KeyRegister);
    }

    private static void ValidateDisplay(int width, int height)
    {
        if (width <= 0 || width % AsciiFont8x8.GlyphWidth != 0)
        {
            throw new ComputerSimulatorException("Display width must be a positive multiple of the glyph width");
        }

        if (height < AsciiFont8x8.GlyphHeight)
        {
            throw new ComputerSimulatorException("Display height must fit at least one glyph row");
        }
    }
}
