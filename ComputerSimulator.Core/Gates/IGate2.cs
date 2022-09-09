using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Gates;

public interface IGate2 : IComponent 
{
    IWire<bool> InputA { get; }
    IWire<bool> InputB { get; }
    IWire<bool> Output { get; }
}