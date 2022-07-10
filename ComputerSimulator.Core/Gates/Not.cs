using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Gates;

public interface INot : IComponent
{
    IWire<bool> Input { get; set; }
    IWire<bool> Output { get; set; }
}

public class Not : ComponentBase, INot
{
    private IWire<bool> _input;
    private IWire<bool> _output;
    private string _label = nameof(Not);

    public Not(IWireCupboard wireCupboard) : base(wireCupboard)
    {
        _input = WireCupboard.Retrieve(false, $"{nameof(Not)}.{nameof(Input)}");
        _output = WireCupboard.Retrieve(false, $"{nameof(Not)}.{nameof(Output)}");
        
        Input.ValueChanged += InputChanged;
        
        GenerateLabels();
    }

    public IWire<bool> Input
    {
        get => _input;
        set => Wire.SetWire(ref _input, value, InputChanged);
    }

    public IWire<bool> Output
    {
        get => _output;
        set => Wire.SetWire(ref _output, value);
    }

    private void InputChanged(object? sender, EventArgs e)
    {
        Output.Value = !Input.Value;
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

    private void GenerateLabels()
    {
        Input.Label = this.GenerateLabel(nameof(Input));
        Output.Label = this.GenerateLabel(nameof(Output));
    }

    public override void Dispose()
    {
        Input.ValueChanged -= InputChanged;
        
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}