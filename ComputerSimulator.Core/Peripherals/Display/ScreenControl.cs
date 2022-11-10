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

    IWire<bool> HorizontalPosition { get; }

    IWire<bool> VerticalPosition { get; }

    IWire<bool> Brightness { get; }
}

public class ScreenControl : PartsBase, IScreenControl
{
    public ScreenControl(
        IWireGroup<bool> displayRamEnableMarBus,
        IWire<bool> displayRamEnableMarSet,
        IWireGroup<bool> displayRamOutputBus, 
        IWire<bool> displayRamEnable,
        IWire<bool> clock,
        IWire<bool> horizontalPosition,
        IWire<bool> verticalPosition, 
        IWire<bool> brightness,
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
    }

    public IWireGroup<bool> DisplayRamEnableMarBus { get; }
    public IWire<bool> DisplayRamEnableMarSet { get; }
    public IWireGroup<bool> DisplayRamOutputBus { get; }
    public IWire<bool> DisplayRamEnable { get; }
    public IWire<bool> Clock { get; }
    public IWire<bool> HorizontalPosition { get; }
    public IWire<bool> VerticalPosition { get; }
    public IWire<bool> Brightness { get; }
    
    public void Update()
    {
        throw new NotImplementedException();
    }
}