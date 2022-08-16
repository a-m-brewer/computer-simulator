using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Circuits;

public interface IShifter : ICircuit
{
    IWire2<bool> ShiftIn { get; }

    IWire2<bool> ShiftOut { get; }
    
    IWireGroup<bool> Input { get; }

    IWireGroup<bool> Output { get; }
}