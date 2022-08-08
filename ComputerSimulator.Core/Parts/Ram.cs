using ComputerSimulator.Core.Circuits;
using ComputerSimulator.Core.Factories;

namespace ComputerSimulator.Core.Parts;

public interface IRam : IComponent2
{
    IBus MarInputBus { get; set; }

    IWire2<bool> MarSet { get; set; }

    IBus Io { get; set; }

    IWire2<bool> Set { get; set; }

    IWire2<bool> Enable { get; set; }
}

public class Ram : PartsBase, IRam
{
    private readonly IRegister _mar;
    
    // External Wires
    private IWire2<bool> _set = DisconnectedWire<bool>.Instance;
    private IWire2<bool> _enable = DisconnectedWire<bool>.Instance;

    public Ram(
        IRegister mar,
        IWire2Factory wireFactory) : base(wireFactory)
    {
        _mar = mar;
        // enable always true for MAR
        _mar.Enable = CreateInternalWire("mar_enable", true);
        _mar.Outputs = CreateInternalWireGroup("mar_output", false);
        _mar.Outputs.ConnectOutputs(Id, OnMarValuesChanged);
    }

    public IBus MarInputBus
    {
        get => _mar.Inputs as IBus ?? throw new Exception("expected that MAR is using a IBus WireGroup<bool>"); 
        set => _mar.Inputs = value;
    }

    public IWire2<bool> MarSet
    {
        get => _mar.Set; 
        set => _mar.Set = value;
    }

    public IBus Io { get; set; } = DisconnectedBus.Instance;

    public IWire2<bool> Set
    {
        get => _set;
        set => WireHelper.SetWire(ref _set, value, Id, OnSetChanged);
    }

    public IWire2<bool> Enable
    {
        get => _enable;
        set => WireHelper.SetWire(ref _enable, value, Id, OnEnableChanged);
    }

    private void OnSetChanged(bool obj)
    {
        throw new NotImplementedException();
    }
    
    private void OnEnableChanged(bool obj)
    {
        throw new NotImplementedException();
    }
    
    private void OnMarValuesChanged(IEnumerable<bool> marValues)
    {
        throw new NotImplementedException();
    }
}