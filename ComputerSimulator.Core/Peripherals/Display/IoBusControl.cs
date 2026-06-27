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

    // Mirrors the CPU bus onto the display-RAM input buses. The actual latching is
    // gated by the Set pulses below, so it is safe for these to follow the bus continuously.
    private readonly IEnabler _setMarBusMirror;
    private readonly IEnabler _inputBusMirror;

    private readonly INot _dataAddressNot;
    private readonly IAnd _isDataOutput;

    // active = "the display device has been selected via OUT Addr 0x07". Sticky: once set it stays on.
    private readonly IAnd2 _activeSetAnd;

    // Set pulses that latch an address into the display-RAM MAR / write a byte into display RAM.
    private readonly IAnd2 _setMarSetAnd;
    private readonly IAnd2 _dataSetAnd;

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

        // Detects the display's I/O address (0b0000_0111) on the low byte of the bus.
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

        // OUT Addr: clk set pulse + Data/Address = Address + I/O = Output.
        IsAddressOutput = ComponentFactory
            .CreateAnd(
                WireFactory.CreateGroup(
                    IoBus.Clk.Set,
                    IoBus.DataAddress,
                    IoBus.InputOutput),
                WireFactory.CreateWire<bool>($"{nameof(IsAddressOutput)}-output"));

        // OUT Data: clk set pulse + Data/Address = Data + I/O = Output.
        _dataAddressNot = ComponentFactory
            .CreateNot(IoBus.DataAddress, WireFactory.CreateWire<bool>($"{nameof(_dataAddressNot)}-output"));
        _isDataOutput = ComponentFactory
            .CreateAnd(
                WireFactory.CreateGroup(
                    IoBus.Clk.Set,
                    _dataAddressNot.Output,
                    IoBus.InputOutput),
                WireFactory.CreateWire<bool>($"{nameof(_isDataOutput)}-output"));

        // Select the device when its address is output. Held on by the memory bit's PowerWire input.
        _activeSetAnd = ComponentFactory.CreateAnd2(
            IsAddressOutput.Output,
            IoSelect.Output,
            WireFactory.CreateWire<bool>($"{nameof(_activeSetAnd)}-output"));

        DisplayAdapterActiveBit =
            ComponentFactory.CreateMemoryBit(
                WireFactory.PowerWire,
                WireFactory.CreateWire<bool>($"{nameof(DisplayAdapterActiveBit)}-output"),
                _activeSetAnd.Output);

        // Continuously mirror the CPU bus onto the display-RAM input buses.
        _setMarBusMirror = ComponentFactory.CreateEnabler(WireFactory.PowerWire, IoBus.CpuBus, DisplayRamSetMarBus);
        _inputBusMirror = ComponentFactory.CreateEnabler(WireFactory.PowerWire, IoBus.CpuBus, DisplayRamInputBus);

        // While selected: OUT Addr latches the display-RAM address, OUT Data writes the byte.
        _setMarSetAnd = ComponentFactory.CreateAnd2(
            DisplayAdapterActiveBit.Output,
            IsAddressOutput.Output,
            DisplayRamSetMarSet);
        _dataSetAnd = ComponentFactory.CreateAnd2(
            DisplayAdapterActiveBit.Output,
            _isDataOutput.Output,
            DisplayRamSet);
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
        _setMarBusMirror.Update();
        _inputBusMirror.Update();

        _ioSelectNots.Update();
        IoSelect.Update();

        IsAddressOutput.Update();

        _dataAddressNot.Update();
        _isDataOutput.Update();

        _activeSetAnd.Update();
        DisplayAdapterActiveBit.Update();

        _setMarSetAnd.Update();
        _dataSetAnd.Update();
    }
}
