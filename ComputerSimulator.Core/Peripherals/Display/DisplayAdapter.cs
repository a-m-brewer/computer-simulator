using ComputerSimulator.Core.Extensions;
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
    private readonly int _bytesPerRow;
    private readonly int _displayByteCount;
    private readonly int _addressBitsPerAxis;
    private readonly int _addressMask;
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
        _bytesPerRow = width / 8;
        _displayByteCount = _bytesPerRow * height;
        _addressBitsPerAxis = WireFactory.WordSize / 2;
        _addressMask = (1 << _addressBitsPerAxis) - 1;

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
        for (var byteAddress = 0; byteAddress < _displayByteCount; byteAddress++)
        {
            RenderDisplayByte(output, byteAddress);
        }
    }

    private void RenderDisplayByte(IDisplayOutput output, int byteAddress)
    {
        var row = byteAddress / _bytesPerRow;
        if (row < 0 || row >= Height)
        {
            return;
        }

        var byteColumn = byteAddress % _bytesPerRow;
        var slotExists = _displayRam.TryGetSlot(byteAddress & _addressMask, byteAddress >> _addressBitsPerAxis, out var slot);
        var value = slotExists ? slot.Memory.StoredValue.ToInt() : 0;

        if (output is IDisplayByteOutput byteOutput)
        {
            byteOutput.SetPixelByte(byteColumn * 8, row, value);
            return;
        }

        for (var bit = 0; bit < 8; bit++)
        {
            var x = (byteColumn * 8) + bit;
            var on = (value & (1 << bit)) != 0;
            output.SetPixel(x, row, on);
        }
    }
}
