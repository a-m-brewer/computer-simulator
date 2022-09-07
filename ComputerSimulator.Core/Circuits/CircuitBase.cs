using ComputerSimulator.Core.Factories;

namespace ComputerSimulator.Core.Circuits;

public abstract class CircuitBase
{
    protected readonly IComponentFactory ComponentFactory;
    protected readonly IWireFactory WireFactory;

    protected CircuitBase(
        IComponentFactory componentFactory,
        IWireFactory wireFactory)
    {
        ComponentFactory = componentFactory;
        WireFactory = wireFactory;
    }
}