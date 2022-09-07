using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Factories;

public interface IShifterWireFactory
{
    (IWireGroup<bool> R1OutputGroup, IWireGroup<bool>  R2InputGroup) CreateInternalWires(IWire<bool> shiftIn, IWire<bool> shiftOut);
}