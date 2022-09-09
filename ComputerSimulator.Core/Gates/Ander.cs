using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Gates;

public interface IAnder : IComponent
{
    IWireGroup<bool> InputsA { get; }

    IWireGroup<bool> InputsB { get; }

    IWireGroup<bool> Outputs { get; }
}

public class Ander : IAnder
{
    public Ander(IWireGroup<bool> inputsA, IWireGroup<bool> inputsB, IWireGroup<bool> outputs)
    {
        InputsA = inputsA;
        InputsB = inputsB;
        Outputs = outputs;
    }

    public IWireGroup<bool> InputsA { get; }
    public IWireGroup<bool> InputsB { get; }
    public IWireGroup<bool> Outputs { get; }
    
    public void Update()
    {
        for (var i = 0; i < InputsA.Count; i++)
        {
            Outputs[i].Value = InputsA[i].Value && InputsB[i].Value;
        }
    }
}