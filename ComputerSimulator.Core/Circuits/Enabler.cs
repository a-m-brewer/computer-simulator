using ComputerSimulator.Core.Events;
using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Gates;
using ComputerSimulator.Core.Parts;
using ComputerSimulator.Core.Services;

namespace ComputerSimulator.Core.Circuits;

public interface IEnabler : IComponent2
{
    IWire2<bool> Enable { get; set; }
    IWireGroup<bool> Inputs { get; set; }
    IWireGroup<bool> Outputs { get; set; }
}

public class Enabler : CircuitBase, IEnabler
{
    // Wires
    private IWire2<bool> _enable = DisconnectedWire<bool>.Instance;
    private IWireGroup<bool> _inputs = DisconnectedWireGroup<bool>.Instance;
    private IWireGroup<bool> _outputs = DisconnectedWireGroup<bool>.Instance;

    // Gates
    private readonly IAnd[] _ands;

    public Enabler(
        IComponentFactory2 componentFactory2,
        IWireService wireService) : base(wireService)
    {
        _ands = componentFactory2.CreateSet<IAnd>();
        foreach (var and in _ands)
        {
            and.Inputs = CreateInternalWireGroup<bool>();
        }
    }

    public IWire2<bool> Enable
    {
        get => _enable;
        set
        {
            _enable = value;
            foreach (var gate in _ands)
            {
                gate.Inputs.SetWire(1, _enable);
            }
        }
    }

    public IWireGroup<bool> Inputs
    {
        get => _inputs;
        set
        {
            WireGroupHelper.ReSubscribeWireChanged(_inputs, value, InputsOnWireChanged);
            _inputs = value;
            for (var i = 0; i < _inputs.Count; i++)
            {
                _ands[i].Inputs.SetWire(0, _inputs[i]);
            }
        }
    }

    public IWireGroup<bool> Outputs
    {
        get => _outputs;
        set
        {
            WireGroupHelper.ReSubscribeWireChanged(_outputs, value, OutputsOnWireChanged);
            _outputs = value;
            for (var i = 0; i < _outputs.Count; i++)
            {
                _ands[i].Output = _outputs[i];
            }
        }
    }

    private void InputsOnWireChanged(object? sender, WireGroupWireChangedEventArgs<bool> eventArgs)
    {
        _ands[eventArgs.Index].Inputs.SetWire(0, eventArgs.NewWire);
    }
    
    private void OutputsOnWireChanged(object? sender, WireGroupWireChangedEventArgs<bool> eventArgs)
    {
        _ands[eventArgs.Index].Output = eventArgs.NewWire;
    }
}