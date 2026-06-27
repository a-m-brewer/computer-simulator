using ComputerSimulator.Core.Circuits;
using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Peripherals.Display;

public interface IScreenControl : IPart
{
    IWireGroup<bool> DisplayRamEnableMarBus { get; }

    IWire<bool> DisplayRamEnableMarSet { get; }

    IWireGroup<bool> DisplayRamOutputBus { get; }

    IWire<bool> DisplayRamEnable { get; }

    IWire<bool> Clock { get; }

    IWireGroup<bool> HorizontalPosition { get; }

    IWireGroup<bool> VerticalPosition { get; }

    IWire<bool> Brightness { get; }

    int Width { get; }

    int Height { get; }

    int X { get; }

    int Y { get; }

    /// <summary>
    /// True when the current pixel starts a new display-RAM byte (8 pixels) and therefore needs a fresh read.
    /// </summary>
    bool AtByteBoundary { get; }

    /// <summary>
    /// Drives the display-RAM read MAR with the byte address of the current pixel and enables the read.
    /// Call before updating the display RAM so the byte appears on the output bus.
    /// </summary>
    void AddressCurrentByte();

    /// <summary>
    /// Returns the scanner to the top-left pixel.
    /// </summary>
    void Reset();
}

/// <summary>
/// Raster scanner for the display adapter. Sweeps the screen pixel by pixel using gate-level
/// horizontal/vertical position counters (a register incremented by a word adder), reads the
/// addressed byte through the display-RAM read port, and exposes the lit/unlit state of the
/// current pixel on <see cref="Brightness"/>.
/// </summary>
public class ScreenControl : PartsBase, IScreenControl
{
    private readonly int _bytesPerRow;

    private readonly IWireGroup<bool> _one;

    private readonly IRegister _horizontal;
    private readonly IWordAdder _horizontalAdder;
    private readonly IWire<bool> _horizontalSet;
    private readonly IWireGroup<bool> _horizontalInput;
    private readonly IWireGroup<bool> _horizontalNext;

    private readonly IRegister _vertical;
    private readonly IWordAdder _verticalAdder;
    private readonly IWire<bool> _verticalSet;
    private readonly IWireGroup<bool> _verticalInput;
    private readonly IWireGroup<bool> _verticalNext;

    public ScreenControl(
        IWireGroup<bool> displayRamEnableMarBus,
        IWire<bool> displayRamEnableMarSet,
        IWireGroup<bool> displayRamOutputBus,
        IWire<bool> displayRamEnable,
        IWire<bool> clock,
        IWireGroup<bool> horizontalPosition,
        IWireGroup<bool> verticalPosition,
        IWire<bool> brightness,
        int width,
        int height,
        IComponentFactory componentFactory, IWireFactory wireFactory) : base(componentFactory, wireFactory)
    {
        DisplayRamEnableMarBus = displayRamEnableMarBus;
        DisplayRamEnableMarSet = displayRamEnableMarSet;
        DisplayRamOutputBus = displayRamOutputBus;
        DisplayRamEnable = displayRamEnable;
        Clock = clock;
        HorizontalPosition = horizontalPosition;
        VerticalPosition = verticalPosition;
        Brightness = brightness;
        Width = width;
        Height = height;
        _bytesPerRow = width / 8;

        _one = WireFactory.CreateGroup<bool>("screen-one");
        _one.SetValue(1.ToBinaryBools(WireFactory.WordSize));

        // Horizontal position counter: register holding X, incremented by a word adder.
        _horizontalSet = WireFactory.CreateWire<bool>("horizontal-set");
        _horizontalInput = WireFactory.CreateGroup<bool>("horizontal-input");
        _horizontal = ComponentFactory.CreateRegister(
            _horizontalSet, WireFactory.PowerWire, _horizontalInput, HorizontalPosition);
        _horizontalNext = WireFactory.CreateGroup<bool>("horizontal-next");
        _horizontalAdder = ComponentFactory.CreateWordAdder(
            HorizontalPosition, _one, WireFactory.OffWire, WireFactory.CreateWire<bool>(), _horizontalNext);

        // Vertical position counter.
        _verticalSet = WireFactory.CreateWire<bool>("vertical-set");
        _verticalInput = WireFactory.CreateGroup<bool>("vertical-input");
        _vertical = ComponentFactory.CreateRegister(
            _verticalSet, WireFactory.PowerWire, _verticalInput, VerticalPosition);
        _verticalNext = WireFactory.CreateGroup<bool>("vertical-next");
        _verticalAdder = ComponentFactory.CreateWordAdder(
            VerticalPosition, _one, WireFactory.OffWire, WireFactory.CreateWire<bool>(), _verticalNext);

        Reset();
    }

    public IWireGroup<bool> DisplayRamEnableMarBus { get; }
    public IWire<bool> DisplayRamEnableMarSet { get; }
    public IWireGroup<bool> DisplayRamOutputBus { get; }
    public IWire<bool> DisplayRamEnable { get; }
    public IWire<bool> Clock { get; }
    public IWireGroup<bool> HorizontalPosition { get; }
    public IWireGroup<bool> VerticalPosition { get; }
    public IWire<bool> Brightness { get; }

    public int Width { get; }
    public int Height { get; }

    public int X => HorizontalPosition.ToInt();
    public int Y => VerticalPosition.ToInt();

    public bool AtByteBoundary => X % 8 == 0;

    public void AddressCurrentByte()
    {
        var byteAddress = (Y * _bytesPerRow) + (X / 8);

        DisplayRamEnableMarBus.SetValue(byteAddress.ToBinaryBools(WireFactory.WordSize));
        DisplayRamEnableMarSet.Value = true;
        DisplayRamEnable.Value = true;
    }

    public void Reset()
    {
        LoadCounter(_horizontalSet, _horizontalInput, _horizontal, 0);
        LoadCounter(_verticalSet, _verticalInput, _vertical, 0);
    }

    public void Update()
    {
        // Expose the lit/unlit state of the current pixel from the byte on the output bus.
        Brightness.Value = DisplayRamOutputBus[X % 8].Value;

        Advance();
    }

    private void Advance()
    {
        _horizontalAdder.Update();

        var nextX = _horizontalNext.ToInt();
        if (X + 1 >= Width)
        {
            nextX = 0;
            AdvanceVertical();
        }

        LoadCounter(_horizontalSet, _horizontalInput, _horizontal, nextX);
    }

    private void AdvanceVertical()
    {
        _verticalAdder.Update();

        var nextY = _verticalNext.ToInt();
        if (Y + 1 >= Height)
        {
            nextY = 0;
        }

        LoadCounter(_verticalSet, _verticalInput, _vertical, nextY);
    }

    private void LoadCounter(IWire<bool> set, IWireGroup<bool> input, IRegister register, int value)
    {
        input.SetValue(value.ToBinaryBools(WireFactory.WordSize));
        set.Value = true;
        register.Update();
        set.Value = false;
    }
}
