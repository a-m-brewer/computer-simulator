using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Peripherals.Display;

public interface IScreenControl : IPart
{
    IWireGroup<bool> Inputs { get; }
    
    IWire<bool> DisplayRamEnable { get; }
}

public class ScreenControl : IScreenControl
{
    public void Update()
    {
        throw new NotImplementedException();
    }
}