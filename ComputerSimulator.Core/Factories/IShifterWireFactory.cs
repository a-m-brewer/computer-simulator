using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Factories;

public interface IShifterWireFactory
{
    (IWireGroup<bool> R1OutputGroup, IWireGroup<bool>  R2InputGroup) CreateInternalWires(IWire2<bool> shiftIn, IWire2<bool> shiftOut);
}