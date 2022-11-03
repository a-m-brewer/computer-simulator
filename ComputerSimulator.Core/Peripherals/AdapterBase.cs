using ComputerSimulator.Core.Circuits;
using ComputerSimulator.Core.Factories;

namespace ComputerSimulator.Core.Peripherals;

public class AdapterBase : CircuitBase
{
    public AdapterBase(IComponentFactory componentFactory, IWireFactory wireFactory) : base(componentFactory, wireFactory)
    {
    }
}