using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Gates;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Circuits;

public interface IMemoryBit : IComponent
{
    public IWire<bool> Input { get; set; }

    public IWire<bool> Set { get; set; }

    public IWire<bool> Output { get; set; }
}

public class MemoryBit : ComponentBase, IMemoryBit
{
    private readonly INAnd _nAnd1;
    private readonly INAnd _nAnd2;
    private readonly INAnd _nAnd3;
    private readonly INAnd _nAnd4;
    private string _label = nameof(MemoryBit);
    private IWire<bool> _input;
    private IWire<bool> _set;
    private readonly IWire<bool> _a;
    private readonly IWire<bool> _b;
    private IWire<bool> _output;
    private readonly IWire<bool> _c;

    public MemoryBit(
        INAnd nAnd1,
        INAnd nAnd2,
        INAnd nAnd3,
        INAnd nAnd4,
        IWireCupboard wireCupboard) : base(wireCupboard)
    {
        _nAnd1 = nAnd1;
        _nAnd2 = nAnd2;
        _nAnd3 = nAnd3;
        _nAnd4 = nAnd4;

        _input = WireCupboard.Retrieve(false, this.GenerateLabel(nameof(_input)));
        _set = WireCupboard.Retrieve(false, this.GenerateLabel(nameof(_set)));
        _a = WireCupboard.Retrieve(false, this.GenerateLabel(nameof(_a)));
        _b = WireCupboard.Retrieve(false, this.GenerateLabel(nameof(_b)));
        _c = WireCupboard.Retrieve(false, this.GenerateLabel(nameof(_c)));
        _output = WireCupboard.Retrieve(false, this.GenerateLabel(nameof(_output)));
        
        _nAnd1.SetInputWire(0, _input);
        _nAnd1.SetInputWire(1, _set);
        _nAnd1.Output = _a;

        _nAnd2.SetInputWire(0, _a);
        _nAnd2.SetInputWire(1, _set);
        _nAnd2.Output = _b;
        
        _nAnd3.SetInputWire(0, _a);
        _nAnd3.SetInputWire(1, _c);
        _nAnd3.Output = _output;
        
        _nAnd4.SetInputWire(0, _output);
        _nAnd4.SetInputWire(1, _b);
        _nAnd4.Output = _c;
        
        GenerateSubLabels();
    }

    public override void Dispose()
    {
        GC.SuppressFinalize(this);
        
        _nAnd1.Dispose();
        _nAnd2.Dispose();
        _nAnd3.Dispose();
        _nAnd4.Dispose();
        
        base.Dispose();
    }

    public IWire<bool> Input
    {
        get => _input;
        set
        {
            _input = value;
            _nAnd1.SetInputWire(0, _input);
        }
    }

    public IWire<bool> Set
    {
        get => _set;
        set
        {
            _set = value;
            _nAnd1.SetInputWire(1, _set);
            _nAnd2.SetInputWire(1, _set);
        }
    }

    public IWire<bool> Output
    {
        get => _output;
        set
        {
            _output = value;
            _nAnd3.Output = _output;
            _nAnd4.SetInputWire(0, _output);
        }
    }

    public override string Label
    {
        get => _label;
        set
        {
            _label = value;
            GenerateSubLabels();
        }
    }

    private void GenerateSubLabels()
    {
        _input.Label = this.GenerateLabel(nameof(_input));
        _set.Label = this.GenerateLabel(nameof(_set));
        _a.Label = this.GenerateLabel(nameof(_a));
        _b.Label = this.GenerateLabel(nameof(_b));
        _c.Label = this.GenerateLabel(nameof(_c));
        _output.Label = this.GenerateLabel(nameof(_output));

        _nAnd1.Label = this.GenerateLabel(nameof(_nAnd1));
        _nAnd2.Label = this.GenerateLabel(nameof(_nAnd2));
        _nAnd3.Label = this.GenerateLabel(nameof(_nAnd3));
        _nAnd4.Label = this.GenerateLabel(nameof(_nAnd4));
    }
}