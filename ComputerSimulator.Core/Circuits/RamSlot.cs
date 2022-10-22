using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Gates;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Circuits;

public interface IRamSlot : ICircuit
{
    IWire<bool> Set { get; }

    IWire<bool> Enable { get; }

    IWire<bool> X { get; }

    IWire<bool> Y { get; }

    IBus Io { get; }
    
    /// <summary>
    /// Purely for debug/testing purposes only. Do not use for any actual code
    /// </summary>
    IRegister Memory { get; }
}

public class RamSlot : PartsBase, IRamSlot
{
    // Gates
    private readonly IAnd _xAnd;
    private readonly IAnd _setAnd;
    private readonly IAnd _enableAnd;

    public RamSlot(
        IWire<bool> x,
        IWire<bool> y,
        IWire<bool> set,
        IWire<bool> enable,
        IBus io,
        IComponentFactory componentFactory,
        IWireFactory wireFactory) : base(componentFactory, wireFactory)
    {
        Io = io;
        _xAnd = ComponentFactory.CreateAnd(WireFactory.CreateGroup(x, y), WireFactory.CreateWire(false));
        _setAnd = ComponentFactory.CreateAnd(WireFactory.CreateGroup(_xAnd.Output, set), WireFactory.CreateWire(false));
        _enableAnd = ComponentFactory.CreateAnd(WireFactory.CreateGroup(_xAnd.Output, enable), WireFactory.CreateWire(false));
        
        Memory = ComponentFactory.CreateRegister(_setAnd.Output, _enableAnd.Output, Io, Io);
    }

    public IWire<bool> Set => _setAnd.Inputs[1];

    public IWire<bool> Enable => _enableAnd.Inputs[1];

    public IWire<bool> X => _xAnd.Inputs[0];

    public IWire<bool> Y => _xAnd.Inputs[1];

    public IBus Io { get; }

    public IRegister Memory { get; }

    public void Update()
    {
        _xAnd.Update();
        _setAnd.Update();
        _enableAnd.Update();
        
        Memory.Update();
    }
}