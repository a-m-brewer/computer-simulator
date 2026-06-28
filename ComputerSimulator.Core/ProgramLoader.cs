using ComputerSimulator.Core.Exceptions;
using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core;

public static class ProgramLoader
{
    public const int MaxImageSize = 1 << 16;

    public static byte[] ReadBinaryImage(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ComputerSimulatorException("Program path cannot be empty");
        }

        var bytes = File.ReadAllBytes(path);
        ValidateImageSize(bytes.Length);
        return bytes;
    }

    public static void Load(IRam ram, IReadOnlyList<byte> image, int startAddress = 0)
    {
        if (startAddress < 0)
        {
            throw new ComputerSimulatorException("Program start address cannot be negative");
        }

        ValidateImageSize(startAddress + image.Count);

        for (var offset = 0; offset < image.Count; offset++)
        {
            var address = startAddress + offset;
            ram.GetSlot(address & 0xFF, address >> 8)
                .Memory
                .SetRegisterValue(((int)image[offset]).ToBinaryBools(8));
        }
    }

    public static void Load(IRam ram, IReadOnlyList<int> image, int startAddress = 0)
    {
        var bytes = new byte[image.Count];
        for (var i = 0; i < image.Count; i++)
        {
            if (image[i] is < 0 or > 0xFF)
            {
                throw new ComputerSimulatorException($"Program byte at offset {i} must fit in one byte");
            }

            bytes[i] = (byte)image[i];
        }

        Load(ram, bytes, startAddress);
    }

    public static void Load(IRam ram, IReadOnlyList<bool[]> program, int startAddress = 0)
    {
        var bytes = new byte[program.Count];
        for (var i = 0; i < program.Count; i++)
        {
            bytes[i] = (byte)program[i].ToInt();
        }

        Load(ram, bytes, startAddress);
    }

    private static void ValidateImageSize(int size)
    {
        if (size > MaxImageSize)
        {
            throw new ComputerSimulatorException($"Program image is {size} bytes, but RAM holds {MaxImageSize} bytes");
        }
    }
}
