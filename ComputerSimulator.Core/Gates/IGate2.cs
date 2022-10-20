using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Gates;

public interface IGate2 : ISingleOutput 
{
    IWire<bool> InputA { get; }
    IWire<bool> InputB { get; }
}