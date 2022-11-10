using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Peripherals.Display;

public interface IIoBusControl : IPart
{
    IBus CpuBus { get; }
    
    IWireGroup<bool> DisplayRamSetMarBus { get; }
    
    IWire<bool> DisplayRamSetMarSet { get; }
    
    IWireGroup<bool> DisplayRamInputBus { get; }
    
    IWire<bool> DisplayRamSet { get; }
}

public class IoBusControl : PartsBase, IIoBusControl
{
    public IoBusControl(
        IBus cpuBus,
        IWireGroup<bool> displayRamSetMarBus,
        IWire<bool> displayRamSetMarSet,
        IWireGroup<bool> displayRamInputBus,
        IWire<bool> displayRamSet,
        IComponentFactory componentFactory, IWireFactory wireFactory) : base(componentFactory, wireFactory)
    {
        CpuBus = cpuBus;
        DisplayRamSetMarBus = displayRamSetMarBus;
        DisplayRamSetMarSet = displayRamSetMarSet;
        DisplayRamInputBus = displayRamInputBus;
        DisplayRamSet = displayRamSet;
    }

    public IBus CpuBus { get; }
    public IWireGroup<bool> DisplayRamSetMarBus { get; }
    public IWire<bool> DisplayRamSetMarSet { get; }
    public IWireGroup<bool> DisplayRamInputBus { get; }
    public IWire<bool> DisplayRamSet { get; }
    
    public void Update()
    {
        throw new NotImplementedException();
    }
}