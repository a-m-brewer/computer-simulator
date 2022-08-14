using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Circuits;

public interface IWord : IComponent2
{
    IWire2<bool> Set { get; }
    
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
        IWire2<bool> set,
        IComponentFactory2 componentFactory,
        IWire2Factory2 wireFactory) : base(componentFactory, wireFactory)
    {
        Inputs = inputs;
        Outputs = outputs;
        Set = set;

        _memory = componentFactory.CreateMemoryBitSet(Inputs, Outputs, Set);
    }

    public IWire2<bool> Set { get; }

    public IWireGroup<bool> Inputs { get; }

    public IWireGroup<bool> Outputs { get; }
}