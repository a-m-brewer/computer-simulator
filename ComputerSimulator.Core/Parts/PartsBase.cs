using ComputerSimulator.Core.Circuits;
using ComputerSimulator.Core.Factories;

namespace ComputerSimulator.Core.Parts;

public abstract class PartsBase : CircuitBase
{
    protected PartsBase(IComponentFactory2 componentFactory, IWire2Factory2 wireFactory) : base(componentFactory, wireFactory)
    {
    }
}