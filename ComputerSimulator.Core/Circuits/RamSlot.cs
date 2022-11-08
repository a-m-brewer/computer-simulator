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

    IBus InputBus { get; }

    IBus OutputBus { get; }
    
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
        IBus bus,
        IComponentFactory componentFactory,
        IWireFactory wireFactory) : this(x, y, set, enable, bus, bus, componentFactory, wireFactory)
    {
    }
    
    public RamSlot(
        IWire<bool> x,
        IWire<bool> y,
        IWire<bool> set,
        IWire<bool> enable,
        IBus inputBus,
        IBus outputBus,
        IComponentFactory componentFactory,
        IWireFactory wireFactory) : base(componentFactory, wireFactory)
    {
        InputBus = inputBus;
        OutputBus = outputBus;
        _xAnd = ComponentFactory.CreateAnd(WireFactory.CreateGroup(x, y), WireFactory.CreateWire<bool>());
        _setAnd = ComponentFactory.CreateAnd(WireFactory.CreateGroup(_xAnd.Output, set), WireFactory.CreateWire<bool>());
        _enableAnd = ComponentFactory.CreateAnd(WireFactory.CreateGroup(_xAnd.Output, enable), WireFactory.CreateWire<bool>());
        
        Memory = ComponentFactory.CreateRegister(_setAnd.Output, _enableAnd.Output, InputBus, OutputBus);
    }

    public IWire<bool> Set => _setAnd.Inputs[1];

    public IWire<bool> Enable => _enableAnd.Inputs[1];

    public IWire<bool> X => _xAnd.Inputs[0];

    public IWire<bool> Y => _xAnd.Inputs[1];

    public IBus InputBus { get; }

    public IBus OutputBus { get; }
    
    public IRegister Memory { get; }

    public void Update()
    {
        _xAnd.Update();
        _setAnd.Update();
        _enableAnd.Update();
        
        Memory.Update();
    }
}