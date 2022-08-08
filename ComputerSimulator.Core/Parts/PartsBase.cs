using ComputerSimulator.Core.Circuits;
using ComputerSimulator.Core.Factories;

namespace ComputerSimulator.Core.Parts;

public abstract class PartsBase : CircuitBase
{
    protected PartsBase(IWire2Factory wireFactory) : base(wireFactory)
    {
    }
}