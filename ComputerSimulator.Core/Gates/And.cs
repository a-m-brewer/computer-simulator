using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Gates;

public interface IAnd : IInputComponent
{
    IWire<bool> Output { get; set; }
}

public class And : ComponentBase, IAnd
{
    private Dictionary<int, IWire<bool>> _inputs = new();
    private IWire<bool> _output;
    private string _label = nameof(And);

    public And(IWireCupboard wireCupboard) : base(wireCupboard)
    {
        _output = WireCupboard.Retrieve(false, this.GenerateLabel(nameof(Output)));
        GenerateLabels();
    }

    public IWire<bool> Output
    {
        get => _output;
        set => Wire.SetWire(ref _output, value);
    }

    private void InputChanged(object? sender, EventArgs e)
    {
        Output.Value = _inputs.Values.All(a => a.Value);
    }

    public override string Label
    {
        get => _label;
        set
        {
            _label = value;
            GenerateLabels();
        }
    }
    
    public void SetInputWire(int index, IWire<bool> wire)
    {
        if (_inputs.TryGetValue(index, out var existingWire))
        {
            existingWire.ValueChanged -= InputChanged;
        }

        _inputs[index] = wire;
        _inputs[index].ValueChanged += InputChanged;
    }

    public void SetInputWireValue(int index, bool value)
    {
        _inputs[index].Value = value;
    }

    public IWire<bool> GetInputWire(int index)
    {
        return _inputs[index];
    }

    private void GenerateLabels()
    {
        foreach (var input in _inputs)
        {
            input.Value.Label = this.GenerateLabel($"{nameof(_inputs)}[{input.Key}]");
        }
        
        Output.Label = this.GenerateLabel(nameof(Output));
    }
    
    public override void Dispose()
    {
        foreach (var input in _inputs.Values)
        {
            input.ValueChanged -= InputChanged;
        }
        
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}