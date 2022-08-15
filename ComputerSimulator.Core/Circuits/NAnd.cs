using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Gates;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Circuits;

public interface INAnd : ICircuit
{
    IWireGroup<bool> Inputs { get; }
    IWire2<bool> Output { get; }
}

public class NAnd : CircuitBase, INAnd
{
    // ReSharper disable once NotAccessedField.Local
    private readonly IAnd _andGate;
    // ReSharper disable once NotAccessedField.Local
    private readonly INot _notGate;

    public NAnd(
        IWireGroup<bool> inputs,
        IWire2<bool> output,
        IComponentFactory2 componentFactory2,
        IWire2Factory2 wireFactory)
    : base(componentFactory2, wireFactory)
    {
        Inputs = inputs;
        Output = output;

        var andToNot = WireFactory.CreateWire(false);
            
        _andGate = ComponentFactory.CreateAnd(Inputs, andToNot);
        _notGate = ComponentFactory.CreateNot(andToNot, Output);
    }

    public IWireGroup<bool> Inputs { get; }

    public IWire2<bool> Output { get; }

    public void Update()
    {
        _andGate.Update();
        _notGate.Update();
    }
}