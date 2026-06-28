using ComputerSimulator.Core.Enums;
using ComputerSimulator.Core.Exceptions;

namespace ComputerSimulator.Core.Instructions;

public static class InstructionSet
{
    public const int Clf = 0x60;

    public static int Ld(int registerA, int registerB) => RegisterPair(0x00, registerA, registerB);

    public static int St(int registerA, int registerB) => RegisterPair(0x10, registerA, registerB);

    public static int Data(int registerB) => RegisterB(0x20, registerB);

    public static int Jmpr(int registerB) => RegisterB(0x30, registerB);

    public static int Jmp() => 0x40;

    public static int JumpIf(JumpCondition condition)
    {
        if (condition == JumpCondition.None || ((int)condition & ~0x0F) != 0)
        {
            throw new ComputerSimulatorException($"{condition} is not a valid jump condition");
        }

        return 0x50 | (int)condition;
    }

    public static int Io(IoMode mode, DataAddress dataAddress, int registerB)
    {
        ValidateDefined(mode, nameof(mode));
        ValidateDefined(dataAddress, nameof(dataAddress));
        ValidateRegister(registerB);

        return 0x70 | ((int)mode << 3) | ((int)dataAddress << 2) | registerB;
    }

    public static int In(DataAddress dataAddress, int registerB) => Io(IoMode.Input, dataAddress, registerB);

    public static int Out(DataAddress dataAddress, int registerB) => Io(IoMode.Output, dataAddress, registerB);

    public static int Alu(OpCode opCode, int registerA, int registerB)
    {
        ValidateDefined(opCode, nameof(opCode));
        ValidateRegister(registerA);
        ValidateRegister(registerB);

        return 0x80 | ((int)opCode << 4) | (registerA << 2) | registerB;
    }

    public static int Add(int registerA, int registerB) => Alu(OpCode.Add, registerA, registerB);

    public static int Shr(int registerA, int registerB) => Alu(OpCode.Shr, registerA, registerB);

    public static int Shl(int registerA, int registerB) => Alu(OpCode.Shl, registerA, registerB);

    public static int Not(int registerA, int registerB) => Alu(OpCode.Not, registerA, registerB);

    public static int And(int registerA, int registerB) => Alu(OpCode.And, registerA, registerB);

    public static int Or(int registerA, int registerB) => Alu(OpCode.Or, registerA, registerB);

    public static int XOr(int registerA, int registerB) => Alu(OpCode.XOr, registerA, registerB);

    public static int Cmp(int registerA, int registerB) => Alu(OpCode.Cmp, registerA, registerB);

    public static void Emit(ICollection<int> program, int instruction, int operand)
    {
        ValidateByte(instruction, nameof(instruction));
        ValidateByte(operand, nameof(operand));

        program.Add(instruction);
        program.Add(operand);
    }

    private static int RegisterPair(int prefix, int registerA, int registerB)
    {
        ValidateRegister(registerA);
        ValidateRegister(registerB);

        return prefix | (registerA << 2) | registerB;
    }

    private static int RegisterB(int prefix, int registerB)
    {
        ValidateRegister(registerB);

        return prefix | registerB;
    }

    private static void ValidateRegister(int register)
    {
        if (register is < 0 or > 3)
        {
            throw new ComputerSimulatorException($"R{register} is not a valid register");
        }
    }

    private static void ValidateByte(int value, string parameterName)
    {
        if (value is < 0 or > 0xFF)
        {
            throw new ComputerSimulatorException($"{parameterName} must fit in one byte");
        }
    }

    private static void ValidateDefined<TEnum>(TEnum value, string parameterName)
        where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            throw new ComputerSimulatorException($"{parameterName} is not valid");
        }
    }
}
