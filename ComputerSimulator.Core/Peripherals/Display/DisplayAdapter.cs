using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Peripherals.Display;

public interface IDisplayAdapter : IAdapter
{
    IIoBus IoBus { get; }

    int Width { get; }

    int Height { get; }

    /// <summary>
    /// Scans the whole of display RAM and pushes the current frame to the given render target.
    /// </summary>
    void RenderFrame(IDisplayOutput output);
}

public class DisplayAdapter : AdapterBase, IDisplayAdapter
{
    private readonly IIoBusControl _ioBusControl;
    private readonly IScreenControl _screenControl;
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly IClock _clock;
    private readonly IDisplayRam _displayRam;

    public DisplayAdapter(
        IIoBus ioBus,
        int width,
        int height,
        IComponentFactory componentFactory, IWireFactory wireFactory) : base(componentFactory, wireFactory)
    {
        IoBus = ioBus;
        Width = width;
        Height = height;

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
            WireFactory.CreateGroup<bool>(nameof(_screenControl.HorizontalPosition)),
            WireFactory.CreateGroup<bool>(nameof(_screenControl.VerticalPosition)),
            WireFactory.CreateWire<bool>(nameof(_screenControl.Brightness)),
            width,
            height
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
    public int Width { get; }
    public int Height { get; }

    // The write side of the adapter: handles CPU OUT instructions that load the display-RAM
    // address register and write pixel bytes. Driven once per IO-bus update via ConnectedComponents.
    public void Update()
    {
        _ioBusControl.Update();
        _displayRam.Update();
    }

    public void RenderFrame(IDisplayOutput output)
    {
        _screenControl.Reset();

        var pixels = Width * Height;
        for (var i = 0; i < pixels; i++)
        {
            var x = _screenControl.X;
            var y = _screenControl.Y;

            if (_screenControl.AtByteBoundary)
            {
                _screenControl.AddressCurrentByte();
                _displayRam.UpdateRead();
            }

            _screenControl.Update();

            output.SetPixel(x, y, _screenControl.Brightness.Value);
        }

        output.Present();
    }
}
