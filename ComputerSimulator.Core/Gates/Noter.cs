using ComputerSimulator.Core.Circuits;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Gates;

public interface INoter : ICircuit
{
    IWireGroup<bool> Inputs { get; }

    IWireGroup<bool> Outputs { get; }
}

public class Noter : INoter
{
    public Noter(
        IWireGroup<bool> inputs,
        IWireGroup<bool> outputs)
    {
        Inputs = inputs;
        Outputs = outputs;
    }

    public IWireGroup<bool> Inputs { get; }

    public IWireGroup<bool> Outputs { get; }
    
    public void Update()
    {
        for (var i = 0; i < Inputs.Count; i++)
        {
            Outputs[i].Value = !Inputs[i].Value;
        }
    }
}