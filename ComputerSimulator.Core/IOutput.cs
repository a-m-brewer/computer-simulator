using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core;

public interface IOutput<T>
{
    IWire<T> Output { get; }
}