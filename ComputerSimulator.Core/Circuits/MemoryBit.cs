using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Circuits;

public interface IMemoryBit : IComponent2
{
    public IWire2<bool> Input { get; set; }

    public IWire2<bool> Set { get; set; }

    public IWire2<bool> Output { get; set; }
}

public class MemoryBit : CircuitBase, IMemoryBit
{
    private readonly INAnd _nAnd1;
    private readonly INAnd _nAnd2;
    private readonly INAnd _nAnd3;
    private readonly INAnd _nAnd4;
    
    // External Wires
    private IWire2<bool> _input = DisconnectedWire<bool>.Instance;
    private IWire2<bool> _set = DisconnectedWire<bool>.Instance;
    private IWire2<bool> _output = DisconnectedWire<bool>.Instance;

    public MemoryBit(
        INAnd nAnd1,
        INAnd nAnd2,
        INAnd nAnd3,
        INAnd nAnd4,
        IWire2Factory wireFactory)
    : base(wireFactory)
    {
        _nAnd1 = nAnd1;
        _nAnd1.Inputs = CreateInternalWireGroup<bool>();
        
        _nAnd2 = nAnd2;
        _nAnd2.Inputs = CreateInternalWireGroup<bool>();
        
        _nAnd3 = nAnd3;
        _nAnd3.Inputs = CreateInternalWireGroup<bool>();
        
        _nAnd4 = nAnd4;
        _nAnd4.Inputs = CreateInternalWireGroup<bool>();

        var a = CreateInternalWire("a", false);
        var b = CreateInternalWire("b", false);
        var c = CreateInternalWire("c", false);

        _nAnd1.Output = a;

        _nAnd2.Inputs.SetWire(0, a);
        _nAnd2.Output = b;
        
        _nAnd3.Inputs.SetWire(0, a);
        _nAnd3.Inputs.SetWire(1, c);

        _nAnd4.Inputs.SetWire(1, b);
        _nAnd4.Output = c;
    }
    
    public IWire2<bool> Input
    {
        get => _input;
        set
        {
            _input = value;
            _nAnd1.Inputs.SetWire(0, _input);
        }
    }

    public IWire2<bool> Set
    {
        get => _set;
        set
        {
            _set = value;
            _nAnd1.Inputs.SetWire(1, _set);
            _nAnd2.Inputs.SetWire(1, _set);
        }
    }

    public IWire2<bool> Output
    {
        get => _output;
        set
        {
            _output = value;
            _nAnd3.Output = _output;
            _nAnd4.Inputs.SetWire(0, _output);
        }
    }
}