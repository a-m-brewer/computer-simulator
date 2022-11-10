using ComputerSimulator.Core.Circuits;
using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Gates;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Peripherals.Display;

public interface IIoBusControl : IPart
{
    IIoBus IoBus { get; }
    
    IAnd IoSelect { get; }
    
    IAnd IsAddressOutput { get; }
    
    IMemoryBit DisplayAdapterActiveBit { get; }
    
    IWireGroup<bool> DisplayRamSetMarBus { get; }
    
    IWire<bool> DisplayRamSetMarSet { get; }
    
    IWireGroup<bool> DisplayRamInputBus { get; }
    
    IWire<bool> DisplayRamSet { get; }
}

public class IoBusControl : PartsBase, IIoBusControl
{
    private readonly INot[] _ioSelectNots;

    public IoBusControl(
        IIoBus ioBus,
        IWireGroup<bool> displayRamSetMarBus,
        IWire<bool> displayRamSetMarSet,
        IWireGroup<bool> displayRamInputBus,
        IWire<bool> displayRamSet,
        IComponentFactory componentFactory, IWireFactory wireFactory) : base(componentFactory, wireFactory)
    {
        IoBus = ioBus;
        DisplayRamSetMarBus = displayRamSetMarBus;
        DisplayRamSetMarSet = displayRamSetMarSet;
        DisplayRamInputBus = displayRamInputBus;
        DisplayRamSet = displayRamSet;
        
        _ioSelectNots = 5
            .InitArray<INot>()
            .Fill(i => ComponentFactory.CreateNot(
                IoBus.CpuBus[i + 3], WireFactory.CreateWire<bool>($"{nameof(_ioSelectNots)}[{i}]-output")));

        IoSelect = ComponentFactory.CreateAnd(
            WireFactory.CreateGroup(
                IoBus.CpuBus[0],
                IoBus.CpuBus[1],
                IoBus.CpuBus[2],
                _ioSelectNots[0].Output,
                _ioSelectNots[1].Output,
                _ioSelectNots[2].Output,
                _ioSelectNots[3].Output,
                _ioSelectNots[4].Output
                ),
            WireFactory.CreateWire<bool>($"{nameof(IoSelect)}-output"));

        IsAddressOutput = ComponentFactory
            .CreateAnd(
                WireFactory.CreateGroup(
                    IoBus.Clk.Set,
                    IoBus.DataAddress,
                    IoBus.InputOutput),
                WireFactory.CreateWire<bool>($"{nameof(IsAddressOutput)}-output"));

        DisplayAdapterActiveBit =
            ComponentFactory.CreateMemoryBit(
                IoSelect.Output,
                WireFactory.CreateWire<bool>($"{nameof(DisplayAdapterActiveBit)}-output"),
                IsAddressOutput.Output);
    }

    public IAnd IsAddressOutput { get; }
    
    public IMemoryBit DisplayAdapterActiveBit { get; }

    public IIoBus IoBus { get; }
    public IAnd IoSelect { get; }
    public IWireGroup<bool> DisplayRamSetMarBus { get; }
    public IWire<bool> DisplayRamSetMarSet { get; }
    public IWireGroup<bool> DisplayRamInputBus { get; }
    public IWire<bool> DisplayRamSet { get; }
    
    public void Update()
    {
        _ioSelectNots.Update();
        IoSelect.Update();
        
        IsAddressOutput.Update();
        
        DisplayAdapterActiveBit.Update();
    }
}