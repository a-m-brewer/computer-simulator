using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Gates;

public interface INAnd : IInputComponent
{
    IWire<bool> Output { get; set; }
}

public class NAnd : ComponentBase, INAnd
{
    private readonly IAnd _and;
    private readonly INot _not;
    private string _label = nameof(NAnd);
    private readonly IWire<bool> _andToNand;

    public NAnd(
        IAnd and,
        INot not,
        IWireCupboard wireCupboard) : base(wireCupboard)
    {
        _and = and;
        _not = not;

        _andToNand = WireCupboard.Retrieve(false, this.GenerateLabel(nameof(_andToNand)));
        _and.Output = _andToNand;
        _not.Input = _andToNand;
    }

    public IWire<bool> Output
    { 
        get => _not.Output;
        set => _not.Output = value;
    }

    public override string Label
    {
        get => _label;
        set
        {
            _label = value;
            // _not.Label = this.GenerateLabel(nameof(InputA));
            // _and.Label = this.GenerateLabel(nameof(InputB));
            _andToNand.Label = this.GenerateLabel(nameof(_andToNand));
        }
    }

    public void SetInputs(IBus bus)
    {
        _and.SetInputs(bus);
    }

    public void SetInputWire(int index, IWire<bool> wire)
    {
        _and.SetInputWire(index, wire);
    }

    public void SetInputWireValue(int index, bool value)
    {
        _and.SetInputWireValue(index, value);
    }

    public IWire<bool> GetInputWire(int index)
    {
        return _and.GetInputWire(index);
    }

    // public override void SetInternalLabels(string label)
    // {
    //     _andToNand.Label = $"{label}-{_label}-{nameof(_andToNand)}";
    // }

    public override void Dispose()
    {
        _not.Dispose();
        _and.Dispose();
        
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}