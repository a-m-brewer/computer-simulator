using ComputerSimulator.Core.Constants;
using ComputerSimulator.Core.Enums;
using ComputerSimulator.Core.Exceptions;

namespace ComputerSimulator.Core.Instructions;

public class IoInstruction : InstructionBase
{
    private int _registerB;

    public override InstructionPrefix InstructionPrefix => InstructionPrefix.Io;

    public IoMode Mode { get; set; }

    public DataAddress DataAddress { get; set; }

    public int RegisterB
    {
        get => _registerB;
        set
        {
            if (value is < 0 or >= WireConstants.ExpectedNumberOfGeneralPurposeRegisters)
            {
                throw new ComputerSimulatorException($"{value} is not a valid register address");
            }

            _registerB = value;
        }
    }

    public override int AsInt()
    {
        var prefix = InstructionPrefixInt << 4;
        var inputOutput = (int)Mode << 3;
        var dataAddress = (int)DataAddress << 2;

        return prefix + inputOutput + dataAddress + _registerB;
    }
}