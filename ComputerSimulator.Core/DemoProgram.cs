using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Peripherals;

namespace ComputerSimulator.Core;

/// <summary>
/// Hand-assembled program that draws a pattern to the display. It selects the display device, then
/// loops over every display-RAM byte writing the low 8 bits of the address as the pixel byte. That
/// produces a textured pattern across the whole screen and proves the full IO + display path.
///
/// Uses a real counted loop (ADD to advance the address, CMP/JE to terminate) now that the CPU's
/// multi-instruction sequencing is correct.
/// </summary>
public static class DemoProgram
{
    // General purpose registers.
    private const int R0 = 0; // constant 1 (address increment)
    private const int R1 = 1; // scratch (constant building / display device id)
    private const int R2 = 2; // loop limit N
    private const int R3 = 3; // current byte address, also used as the pixel data

    public static IReadOnlyList<bool[]> Build(int width, int height)
    {
        var bytesPerFrame = (width / 8) * height;

        var program = new List<int>();

        // R2 = N, built from a possibly 16-bit constant.
        LoadConstant(program, R2, bytesPerFrame, temp: R1);

        // R0 = 1
        Emit(program, Data(R0), 1);

        // Select the display device (I/O address 0x07).
        Emit(program, Data(R1), (int)IoAddress.Display);
        program.Add(OutAddress(R1));

        // R3 = 0 (first display-RAM byte address)
        Emit(program, Data(R3), 0);

        var loop = program.Count;
        program.Add(OutAddress(R3)); // display-RAM address = R3
        program.Add(OutData(R3));    // write pixel byte = low 8 bits of R3
        program.Add(Add(R0, R3));    // R3 = R3 + 1
        program.Add(Clf);            // clear flags before the compare
        program.Add(Cmp(R3, R2));    // sets Equal when R3 == N
        program.Add(JumpIfEqual);
        var jumpToEndOperand = program.Count;
        program.Add(0);              // patched to the halt address below
        Emit(program, Jump, loop);   // otherwise loop again

        var end = program.Count;
        Emit(program, Jump, end);    // halt: jump to self

        program[jumpToEndOperand] = end;

        return program.Select(b => b.ToBinaryBools(8)).ToList();
    }

    // Loads a (possibly 16-bit) constant into reg, using temp to assemble the high byte.
    private static void LoadConstant(List<int> program, int reg, int value, int temp)
    {
        Emit(program, Data(reg), value & 0xFF);
        Emit(program, Data(temp), (value >> 8) & 0xFF);

        for (var i = 0; i < 8; i++)
        {
            program.Add(ShiftLeft(temp, temp)); // temp <<= 1
        }

        program.Add(Add(temp, reg)); // reg = temp + reg = (high << 8) + low
    }

    private static void Emit(List<int> program, int instruction, int operand)
    {
        program.Add(instruction);
        program.Add(operand);
    }

    // Instruction encodings (see ComputerPartTests for the canonical bit layouts).
    private static int Data(int rb) => 0b0010_0000 | rb;          // DATA RB, <next byte>
    private static int OutAddress(int rb) => 0b0111_1100 | rb;    // OUT Addr, RB
    private static int OutData(int rb) => 0b0111_1000 | rb;       // OUT Data, RB
    private static int Add(int ra, int rb) => 0b1000_0000 | (ra << 2) | rb;
    private static int ShiftLeft(int ra, int rb) => 0b1010_0000 | (ra << 2) | rb;
    private static int Cmp(int ra, int rb) => 0b1111_0000 | (ra << 2) | rb;
    private const int Clf = 0b0110_0000;          // clear flags
    private const int Jump = 0b0100_0000;         // JMP <next byte>
    private const int JumpIfEqual = 0b0101_0010;  // JE <next byte> (jump-if prefix 0101 + Equal select)
}
