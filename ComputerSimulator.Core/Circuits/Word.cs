using ComputerSimulator.Core.Events;
using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Circuits;

public interface IWord : IComponent2
{
    IWire2<bool> Set { get; set; }
    
    IWireGroup<bool> Inputs { get; set; }
    
    IWireGroup<bool> Outputs { get; set; }
}

public class Word : CircuitBase, IWord
{
    // Wires
    private IWire2<bool> _set = DisconnectedWire<bool>.Instance;
    private IWireGroup<bool> _inputs = DisconnectedWireGroup<bool>.Instance;
    private IWireGroup<bool> _outputs = DisconnectedWireGroup<bool>.Instance;
    
    // Circuits
    private readonly IMemoryBit[] _memory;

    public Word(
        IComponentFactory2 componentFactory,
        IWire2Factory wireFactory) : base(wireFactory)
    {
        _memory = componentFactory.CreateSet<IMemoryBit>();
    }

    public IWire2<bool> Set
    {
        get => _set;
        set
        {
            _set = value;
            foreach (var bit in _memory)
            {
                bit.Set = _set;
            }
        }
    }

    public IWireGroup<bool> Inputs
    {
        get => _inputs;
        set => WireGroupHelper.SetWireGroup(ref _inputs, value, InputsOnWireChanged);
    }

    public IWireGroup<bool> Outputs
    {
        get => _outputs;
        set => WireGroupHelper.SetWireGroup(ref _outputs, value, OutputsOnWireChanged);
    }

    private void InputsOnWireChanged(object? sender, WireGroupWireChangedEventArgs<bool> eventArgs)
    {
        _memory[eventArgs.Index].Input = eventArgs.NewWire;
    }
    
    private void OutputsOnWireChanged(object? sender, WireGroupWireChangedEventArgs<bool> eventArgs)
    {
        _memory[eventArgs.Index].Output = eventArgs.NewWire;
    }
}