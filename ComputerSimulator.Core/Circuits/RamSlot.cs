using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Gates;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Circuits;

public interface IRamSlot : ICircuit
{
    IWire<bool> Set { get; }

    IWire<bool> Enable { get; }

    IWire<bool> SetX { get; }

    IWire<bool> SetY { get; }
    
    IWire<bool> EnableX { get; }

    IWire<bool> EnableY { get; }

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

    private readonly IAnd _setAnd;
    private readonly IAnd _enableAnd;
    private readonly IAnd2 _setSelectorAnd;
    private readonly IAnd2 _enableSelectorAnd;

    public RamSlot(
        IWire<bool> x,
        IWire<bool> y,
        IWire<bool> set,
        IWire<bool> enable,
        IBus bus,
        IComponentFactory componentFactory,
        IWireFactory wireFactory) : this(x, y, x, y ,set, enable, bus, bus, componentFactory, wireFactory)
    {
    }
    
    public RamSlot(
        IWire<bool> setX,
        IWire<bool> setY,
        IWire<bool> enableX,
        IWire<bool> enableY,
        IWire<bool> set,
        IWire<bool> enable,
        IBus inputBus,
        IBus outputBus,
        IComponentFactory componentFactory,
        IWireFactory wireFactory) : base(componentFactory, wireFactory)
    {
        InputBus = inputBus;
        OutputBus = outputBus;

        _setSelectorAnd = ComponentFactory.CreateAnd2(setX, setY, WireFactory.CreateWire<bool>());
        _enableSelectorAnd = ComponentFactory.CreateAnd2(enableX,  enableY, WireFactory.CreateWire<bool>());

        _setAnd = ComponentFactory.CreateAnd(WireFactory.CreateGroup(_setSelectorAnd.Output, set), WireFactory.CreateWire<bool>());
        _enableAnd = ComponentFactory.CreateAnd(WireFactory.CreateGroup(_enableSelectorAnd.Output, enable), WireFactory.CreateWire<bool>());
        
        Memory = ComponentFactory.CreateRegister(_setAnd.Output, _enableAnd.Output, InputBus, OutputBus);
    }

    public IWire<bool> Set => _setAnd.Inputs[1];

    public IWire<bool> Enable => _enableAnd.Inputs[1];

    public IWire<bool> SetX => _setSelectorAnd.InputA;

    public IWire<bool> SetY => _setSelectorAnd.InputB;

    public IWire<bool> EnableX => _enableSelectorAnd.InputA;

    public IWire<bool> EnableY => _enableSelectorAnd.InputB;

    public IBus InputBus { get; }

    public IBus OutputBus { get; }
    
    public IRegister Memory { get; }

    public void Update()
    {
        _enableSelectorAnd.Update();
        _setSelectorAnd.Update();
        
        _setAnd.Update();
        _enableAnd.Update();
        
        Memory.Update();
    }
}