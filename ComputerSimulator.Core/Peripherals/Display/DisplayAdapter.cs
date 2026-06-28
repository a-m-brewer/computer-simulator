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
    private readonly DisplayScanMode _scanMode;
    private bool _needsFullScan = true;

    public DisplayAdapter(
        IIoBus ioBus,
        int width,
        int height,
        IComponentFactory componentFactory, IWireFactory wireFactory)
        : this(ioBus, width, height, DisplayScanMode.GateLevel, componentFactory, wireFactory)
    {
    }

    public DisplayAdapter(
        IIoBus ioBus,
        int width,
        int height,
        DisplayScanMode scanMode,
        IComponentFactory componentFactory, IWireFactory wireFactory) : base(componentFactory, wireFactory)
    {
        IoBus = ioBus;
        Width = width;
        Height = height;
        _scanMode = scanMode;

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
        if (_scanMode == DisplayScanMode.ScanBuffer)
        {
            RenderScanBufferFrame(output);
            return;
        }

        RenderGateLevelFrame(output);
    }

    private void RenderGateLevelFrame(IDisplayOutput output)
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
        _displayRam.ClearDirtyAddresses();
    }

    private void RenderScanBufferFrame(IDisplayOutput output)
    {
        if (!_needsFullScan && _displayRam.DirtyAddresses.Count == 0)
        {
            return;
        }

        if (_needsFullScan)
        {
            RenderAllDisplayBytes(output);
            _needsFullScan = false;
        }
        else
        {
            foreach (var byteAddress in _displayRam.DirtyAddresses)
            {
                RenderDisplayByte(output, byteAddress);
            }
        }

        _displayRam.ClearDirtyAddresses();
        output.Present();
    }

    private void RenderAllDisplayBytes(IDisplayOutput output)
    {
        var bytesPerRow = Width / 8;
        var byteCount = bytesPerRow * Height;
        for (var byteAddress = 0; byteAddress < byteCount; byteAddress++)
        {
            RenderDisplayByte(output, byteAddress);
        }
    }

    private void RenderDisplayByte(IDisplayOutput output, int byteAddress)
    {
        var bytesPerRow = Width / 8;
        var row = byteAddress / bytesPerRow;
        if (row < 0 || row >= Height)
        {
            return;
        }

        var byteColumn = byteAddress % bytesPerRow;
        var addressBitsPerAxis = WireFactory.WordSize / 2;
        var slot = _displayRam.GetSlot(byteAddress & ((1 << addressBitsPerAxis) - 1), byteAddress >> addressBitsPerAxis);
        var value = slot.Memory.StoredValue;

        for (var bit = 0; bit < 8; bit++)
        {
            var x = (byteColumn * 8) + bit;
            var on = value[bit].Value;
            output.SetPixel(x, row, on);
        }
    }
}
