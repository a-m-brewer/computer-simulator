using ComputerSimulator.Core.Circuits;
using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Gates;

public interface INAnd2 : ICircuit
{
    IWire<bool> InputA { get; }
    IWire<bool> InputB { get; }
    IWire<bool> Output { get; }
}

public class NAnd2 : CircuitBase, INAnd2
{
    public NAnd2(
        IWire<bool> inputA,
        IWire<bool> inputB,
        IWire<bool> output,
        IComponentFactory componentFactory,
        IWireFactory wireFactory)
        : base(componentFactory, wireFactory)
    {
        InputA = inputA;
        InputB = inputB;
        Output = output;
    }

    public IWire<bool> InputA { get; }

    public IWire<bool> InputB { get; }

    public IWire<bool> Output { get; }

    public void Update()
    {
        Output.Value = !(InputA.Value && InputB.Value);
    }
}