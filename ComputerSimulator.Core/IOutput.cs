using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core;

public interface IOutput<T> where T : new()
{
    IWire<T> Output { get; }
}