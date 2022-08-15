using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Gates;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Circuits;

public interface IRamSlot : ICircuit
{
    IWire2<bool> Set { get; }

    IWire2<bool> Enable { get; }

    IWire2<bool> X { get; }

    IWire2<bool> Y { get; }

    IBus Io { get; }
    
    /// <summary>
    /// Purely for debug/testing purposes only. Do not use for any actual code
    /// </summary>
    bool[] StoredValues { get; }
}

public class RamSlot : PartsBase, IRamSlot
{
    // Gates
    private readonly IAnd _xAnd;
    private readonly IAnd _setAnd;
    private readonly IAnd _enableAnd;
    // ReSharper disable once NotAccessedField.Local
    private readonly IRegister _register;

    public RamSlot(
        IWire2<bool> x,
        IWire2<bool> y,
        IWire2<bool> set,
        IWire2<bool> enable,
        IBus io,
        IComponentFactory2 componentFactory,
        IWire2Factory2 wireFactory) : base(componentFactory, wireFactory)
    {
        Io = io;
        _xAnd = ComponentFactory.CreateAnd(WireFactory.CreateGroup(x, y), WireFactory.CreateWire(false));
        _setAnd = ComponentFactory.CreateAnd(WireFactory.CreateGroup(_xAnd.Output, set), WireFactory.CreateWire(false));
        _enableAnd = ComponentFactory.CreateAnd(WireFactory.CreateGroup(_xAnd.Output, enable), WireFactory.CreateWire(false));
        
        _register = ComponentFactory.CreateRegister(_setAnd.Output, _enableAnd.Output, Io, Io);
    }

    public IWire2<bool> Set => _setAnd.Inputs[1];

    public IWire2<bool> Enable => _enableAnd.Inputs[1];

    public IWire2<bool> X => _xAnd.Inputs[0];

    public IWire2<bool> Y => _xAnd.Inputs[1];

    public IBus Io { get; }

    public bool[] StoredValues => _register.StoredValues;

    public void Update()
    {
        _xAnd.Update();
        _setAnd.Update();
        _enableAnd.Update();
        
        _register.Update();
    }
}