using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Circuits;

public interface IShifter : ICircuit
{
    IWire<bool> ShiftIn { get; }

    IWire<bool> ShiftOut { get; }
    
    IWireGroup<bool> Input { get; }

    IWireGroup<bool> Output { get; }
}