using ComputerSimulator.Core.Gates;
using ComputerSimulator.Core.Parts;
using ComputerSimulator.Core.Services;

namespace ComputerSimulator.Core.Circuits;

public interface IRamSlot : IComponent2
{
    IWire2<bool> Set { get; set; }

    IWire2<bool> Enable { get; set; }

    IWire2<bool> X { get; set; }

    IWire2<bool> Y { get; set; }

    IBus Io { get; set; }
}

public class RamSlot : PartsBase, IRamSlot
{
    // Gates
    private readonly IAnd _xAnd;
    private readonly IAnd _setAnd;
    private readonly IAnd _enableAnd;
    private readonly IRegister _register;
    private IBus _io = DisconnectedBus.Instance;

    public RamSlot(
        IAnd xAnd,
        IAnd setAnd,
        IAnd enableAnd,
        IRegister register,
        IWireService wireService) : base(wireService)
    {
        _xAnd = xAnd;
        _xAnd.Inputs = CreateInternalWireGroup<bool>("x-and-input");
        _xAnd.Output = CreateInternalWire("x-and-output", false);

        _setAnd = setAnd;
        _setAnd.Inputs = CreateInternalWireGroup<bool>("set-and-input");
        _setAnd.Inputs.SetWire(0, _xAnd.Output);
        _setAnd.Output = CreateInternalWire("set-and-output", false);

        _enableAnd = enableAnd;
        _enableAnd.Inputs = CreateInternalWireGroup<bool>("enable-and-input");
        _enableAnd.Inputs.SetWire(0, _xAnd.Output);
        _enableAnd.Output = CreateInternalWire("enable-and-output", false);

        _register = register;
        _register.Set = _setAnd.Output;
        _register.Enable = _enableAnd.Output;
    }

    public IWire2<bool> Set
    {
        get => _setAnd.Inputs[1];
        set => _setAnd.Inputs.SetWire(1, value);
    }

    public IWire2<bool> Enable
    {
        get => _enableAnd.Inputs[1];
        set => _enableAnd.Inputs.SetWire(1, value);
    }

    public IWire2<bool> X
    {
        get => _xAnd.Inputs[0];
        set => _xAnd.Inputs.SetWire(0, value);
    }

    public IWire2<bool> Y
    {
        get => _xAnd.Inputs[1];
        set => _xAnd.Inputs.SetWire(1, value);
    }

    public IBus Io
    {
        get => _io;
        set
        {
            _io = value;
            _register.Outputs = _io;
            _register.Inputs = _io;
        }
    }
}