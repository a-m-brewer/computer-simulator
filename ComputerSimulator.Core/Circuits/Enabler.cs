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
    private IWireGroup<bool> _outputsExternal = DisconnectedWireGroup<bool>.Instance;

    // Gates
    private readonly IAnd[] _ands;
    private readonly IWireGroup<bool> _internalOutput;

    public Enabler(
        IComponentFactory2 componentFactory2,
        IWireService wireService) : base(wireService)
    {
        _ands = componentFactory2.CreateSet<IAnd>();
        for (var i = 0; i < _ands.Length; i++)
        {
            _ands[i].Inputs = CreateInternalWireGroup<bool>($"and-inputs-{i}");
        }

        _internalOutput = CreateInternalWireGroup($"internal-output", false);
        _internalOutput.WireValuesChanged += InternalOutputOnWireValuesChanged;
        
        for (var i = 0; i < _internalOutput.Count; i++)
        {
            _ands[i].Output = _internalOutput[i];
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
        get => _outputsExternal;
        set
        {
            WireGroupHelper.ReSubscribeWireChanged(_outputsExternal, value, OutputsOnWireChanged);
            _outputsExternal = value;
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
    
    private void InternalOutputOnWireValuesChanged(object? sender, int e)
    {
        if (Enable.Value)
        {
            _outputsExternal[e].Value = _internalOutput[e].Value;
        }
    }
}