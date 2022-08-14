using ComputerSimulator.Core.Factories;

namespace ComputerSimulator.Core.Circuits;

public abstract class CircuitBase : ComponentBase2, ICircuit
{
    protected readonly IComponentFactory2 ComponentFactory;
    protected readonly IWire2Factory2 WireFactory;

    protected CircuitBase(
        IComponentFactory2 componentFactory,
        IWire2Factory2 wireFactory)
    {
        ComponentFactory = componentFactory;
        WireFactory = wireFactory;
    }
}