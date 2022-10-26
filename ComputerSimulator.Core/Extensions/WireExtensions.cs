using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Extensions;

public static class WireExtensions
{
    public static IWire<T> InstructionWire<T>(this IWireGroup<T> wires, int i) where T : new()
    {
        return wires[7 - i];
    }
}