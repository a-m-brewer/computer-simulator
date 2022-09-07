using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Gates;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Circuits;

public interface IIsZeroChecker : ICircuit
{
    IWireGroup<bool> Inputs { get; }
    
    IWire<bool> IsZero { get; }
}

public class IsZeroChecker : CircuitBase, IIsZeroChecker
{
    private readonly IOr _or;
    private readonly INot _not;

    public IsZeroChecker(
        IWireGroup<bool> inputs,
        IWire<bool> isZero,
        IComponentFactory componentFactory, 
        IWireFactory wireFactory) : base(componentFactory, wireFactory)
    {
        Inputs = inputs;
        IsZero = isZero;

        _or = ComponentFactory.CreateOr(Inputs, WireFactory.CreateWire(false));
        _not = ComponentFactory.CreateNot(_or.Output, IsZero);
    }

    public IWireGroup<bool> Inputs { get; }

    public IWire<bool> IsZero { get; }
    
    public void Update()
    {
        _or.Update();
        _not.Update();
    }
}