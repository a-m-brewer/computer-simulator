using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Enums;
using ComputerSimulator.Core.Instructions;
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
        InstructionSet.Emit(program, InstructionSet.Data(R0), 1);

        // Select the display device (I/O address 0x07).
        InstructionSet.Emit(program, InstructionSet.Data(R1), (int)IoAddress.Display);
        program.Add(InstructionSet.Out(DataAddress.Address, R1));

        // R3 = 0 (first display-RAM byte address)
        InstructionSet.Emit(program, InstructionSet.Data(R3), 0);

        var loop = program.Count;
        program.Add(InstructionSet.Out(DataAddress.Address, R3)); // display-RAM address = R3
        program.Add(InstructionSet.Out(DataAddress.Data, R3));    // write pixel byte = low 8 bits of R3
        program.Add(InstructionSet.Add(R0, R3));                  // R3 = R3 + 1
        program.Add(InstructionSet.Clf);                          // clear flags before the compare
        program.Add(InstructionSet.Cmp(R3, R2));                  // sets Equal when R3 == N
        program.Add(InstructionSet.JumpIf(JumpCondition.Equal));
        var jumpToEndOperand = program.Count;
        program.Add(0);              // patched to the halt address below
        InstructionSet.Emit(program, InstructionSet.Jmp(), loop); // otherwise loop again

        var end = program.Count;
        InstructionSet.Emit(program, InstructionSet.Jmp(), end);  // halt: jump to self

        program[jumpToEndOperand] = end;

        return program.Select(b => b.ToBinaryBools(8)).ToList();
    }

    // Loads a (possibly 16-bit) constant into reg, using temp to assemble the high byte.
    private static void LoadConstant(List<int> program, int reg, int value, int temp)
    {
        InstructionSet.Emit(program, InstructionSet.Data(reg), value & 0xFF);
        InstructionSet.Emit(program, InstructionSet.Data(temp), (value >> 8) & 0xFF);

        for (var i = 0; i < 8; i++)
        {
            program.Add(InstructionSet.Shl(temp, temp)); // temp <<= 1
        }

        program.Add(InstructionSet.Add(temp, reg)); // reg = temp + reg = (high << 8) + low
    }
}
