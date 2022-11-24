using ComputerSimulator.Core.Enums;
using ComputerSimulator.Core.Extensions;

namespace ComputerSimulator.Core.Instructions;

public abstract class InstructionBase
{
    public abstract InstructionPrefix InstructionPrefix { get; }

    protected int InstructionPrefixInt => (int)InstructionPrefix;

    public abstract int AsInt();

    public bool[] AsBools()
    {
        return AsInt().ToBinaryBools(8);
    }

    public override string ToString()
    {
        return Convert.ToString(AsInt(), toBase: 2).PadLeft(8, '0');
    }
}