using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Circuits;

public interface IWord : ICircuit
{
    IWire<bool> Set { get; }
    
    IWireGroup<bool> Inputs { get; }
    
    IWireGroup<bool> Outputs { get; }
}

public class Word : CircuitBase, IWord
{
    // Circuits
    // ReSharper disable once NotAccessedField.Local
    private readonly IMemoryBit[] _memory;

    public Word(
        IWireGroup<bool> inputs,
        IWireGroup<bool> outputs,
        IWire<bool> set,
        IComponentFactory componentFactory,
        IWireFactory wireFactory) : base(componentFactory, wireFactory)
    {
        Inputs = inputs;
        Outputs = outputs;
        Set = set;

        _memory = componentFactory.CreateMemoryBitSet(Inputs, Outputs, Set);
    }

    public IWire<bool> Set { get; }

    public IWireGroup<bool> Inputs { get; }

    public IWireGroup<bool> Outputs { get; }

    public void Update()
    {
        for (var i = 0; i < _memory.Length; i++)
        {
            _memory[i].Update();
        }
    }
}