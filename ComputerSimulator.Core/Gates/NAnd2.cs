using ComputerSimulator.Core.Circuits;
using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Gates;

public interface INAnd2 : ICircuit
{
    IWire2<bool> InputA { get; }
    IWire2<bool> InputB { get; }
    IWire2<bool> Output { get; }
}

public class NAnd2 : CircuitBase, INAnd2
{
    public NAnd2(
        IWire2<bool> inputA,
        IWire2<bool> inputB,
        IWire2<bool> output,
        IComponentFactory2 componentFactory2,
        IWire2Factory2 wireFactory)
        : base(componentFactory2, wireFactory)
    {
        InputA = inputA;
        InputB = inputB;
        Output = output;
    }

    public IWire2<bool> InputA { get; }

    public IWire2<bool> InputB { get; }

    public IWire2<bool> Output { get; }

    public void Update()
    {
        Output.Value = !(InputA.Value && InputB.Value);
    }
}