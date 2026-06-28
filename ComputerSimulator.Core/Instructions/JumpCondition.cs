namespace ComputerSimulator.Core.Instructions;

[Flags]
public enum JumpCondition
{
    None = 0,
    Zero = 1,
    Equal = 2,
    Above = 4,
    Carry = 8
}
