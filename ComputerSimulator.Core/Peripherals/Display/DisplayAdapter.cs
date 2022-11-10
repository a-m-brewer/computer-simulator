using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Peripherals.Display;

public interface IDisplayAdapter : IAdapter
{
    IIoBus IoBus { get; }
    
    IWire<bool> HorizontalPosition { get; }

    IWire<bool> VerticalPosition { get; }

    IWire<bool> Brightness { get; }
}

public class DisplayAdapter : AdapterBase, IDisplayAdapter
{
    private readonly IIoBusControl _ioBusControl;
    private readonly IScreenControl _screenControl;
    private readonly IClock _clock;
    private readonly IDisplayRam _displayRam;

    public DisplayAdapter(
        IIoBus ioBus,
        IComponentFactory componentFactory, IWireFactory wireFactory) : base(componentFactory, wireFactory)
    {
        IoBus = ioBus;

        _clock = ComponentFactory.CreateClock(WireFactory.CreateWire<bool>("display-clock"));

        _ioBusControl = ComponentFactory.CreateIoBusControl(
            IoBus,
            WireFactory.CreateGroup<bool>(nameof(_ioBusControl.DisplayRamSetMarBus)),
            WireFactory.CreateWire<bool>(nameof(_ioBusControl.DisplayRamSetMarSet)),
            WireFactory.CreateGroup<bool>(nameof(_ioBusControl.DisplayRamInputBus)),
            WireFactory.CreateWire<bool>(nameof(_ioBusControl.DisplayRamSet))
        );

        _screenControl = ComponentFactory.CreateScreenControl(
            WireFactory.CreateGroup<bool>(nameof(_screenControl.DisplayRamEnableMarBus)),
            WireFactory.CreateWire<bool>(nameof(_screenControl.DisplayRamEnableMarSet)),
            WireFactory.CreateGroup<bool>(nameof(_screenControl.DisplayRamOutputBus)),
            WireFactory.CreateWire<bool>(nameof(_screenControl.DisplayRamEnable)),
            _clock.Clk,
            WireFactory.CreateWire<bool>(nameof(_screenControl.HorizontalPosition)),
            WireFactory.CreateWire<bool>(nameof(_screenControl.VerticalPosition)),
            WireFactory.CreateWire<bool>(nameof(_screenControl.Brightness))
        );

        _displayRam = ComponentFactory.CreateDisplayRam(
            _ioBusControl.DisplayRamSetMarSet,
            _screenControl.DisplayRamEnableMarSet,
            _ioBusControl.DisplayRamSetMarBus,
            _screenControl.DisplayRamEnableMarBus,
            _ioBusControl.DisplayRamSet,
            _screenControl.DisplayRamEnable,
            _ioBusControl.DisplayRamInputBus,
            _screenControl.DisplayRamOutputBus
        );
    }

    public IIoBus IoBus { get; }
    public IWire<bool> HorizontalPosition => _screenControl.HorizontalPosition;
    public IWire<bool> VerticalPosition => _screenControl.VerticalPosition;
    public IWire<bool> Brightness => _screenControl.Brightness;

    public void Update()
    {
        _clock.Update();
        _ioBusControl.Update();
        _displayRam.Update();
        _screenControl.Update();
        _displayRam.Update();
    }
}