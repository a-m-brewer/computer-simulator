using ComputerSimulator.Core.Enums;
using ComputerSimulator.Core.Exceptions;
using ComputerSimulator.Core.Instructions;

namespace ComputerSimulator.Core.Programs;

public sealed class MachineProgramBuilder
{
    private readonly List<int> _bytes = [];
    private readonly Dictionary<string, int> _labels = new(StringComparer.Ordinal);
    private readonly List<(int OperandOffset, string Label)> _patches = [];

    public int Count => _bytes.Count;

    public void Add(int instruction)
    {
        ValidateByte(instruction, nameof(instruction));
        _bytes.Add(instruction);
    }

    public void Add(int instruction, int operand)
    {
        InstructionSet.Emit(_bytes, instruction, operand);
    }

    public void LoadConstant(int register, int value, int tempRegister)
    {
        if (value is < 0 or > 0xFFFF)
        {
            throw new ComputerSimulatorException($"Constant {value} must fit in one word");
        }

        if (value <= 0xFF)
        {
            Add(InstructionSet.Data(register), value);
            return;
        }

        Add(InstructionSet.Data(register), value & 0xFF);
        Add(InstructionSet.Data(tempRegister), (value >> 8) & 0xFF);
        for (var i = 0; i < 8; i++)
        {
            Add(InstructionSet.Shl(tempRegister, tempRegister));
        }

        Add(InstructionSet.Add(tempRegister, register));
    }

    public void JumpToSelf()
    {
        var address = Count;
        Add(InstructionSet.Jmp(), address);
    }

    public void Halt(int addressRegister, int tempRegister)
    {
        var address = Count;
        LoadConstant(addressRegister, address, tempRegister);
        Add(InstructionSet.Jmpr(addressRegister));
    }

    public void MarkLabel(string label)
    {
        if (!_labels.TryAdd(label, Count))
        {
            throw new ComputerSimulatorException($"Label '{label}' is already defined");
        }
    }

    public void Jump(string label)
    {
        AddPatched(InstructionSet.Jmp(), label);
    }

    public void JumpIf(JumpCondition condition, string label)
    {
        AddPatched(InstructionSet.JumpIf(condition), label);
    }

    public IReadOnlyList<int> ToBytes()
    {
        var bytes = _bytes.ToArray();
        foreach (var (operandOffset, label) in _patches)
        {
            if (!_labels.TryGetValue(label, out var address))
            {
                throw new ComputerSimulatorException($"Label '{label}' is not defined");
            }

            if (address > 0xFF)
            {
                throw new ComputerSimulatorException($"Patched jump target {address} must fit in one byte");
            }

            bytes[operandOffset] = address;
        }

        return bytes;
    }

    private void AddPatched(int instruction, string label)
    {
        Add(instruction);
        _patches.Add((_bytes.Count, label));
        Add(0);
    }

    private static void ValidateByte(int value, string parameterName)
    {
        if (value is < 0 or > 0xFF)
        {
            throw new ComputerSimulatorException($"{parameterName} must fit in one byte");
        }
    }
}
