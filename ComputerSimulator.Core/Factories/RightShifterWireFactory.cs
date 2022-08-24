using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Factories;

public class RightShifterWireFactory : IShifterWireFactory
{
    private readonly IWire2Factory2 _wireFactory;

    public RightShifterWireFactory(IWire2Factory2 wireFactory)
    {
        _wireFactory = wireFactory;
    }
    
    public (IWireGroup<bool> R1OutputGroup, IWireGroup<bool>  R2InputGroup) CreateInternalWires(IWire2<bool> shiftIn, IWire2<bool> shiftOut)
    {
        var internalWires = _wireFactory.CreateWireSet(false, _wireFactory.WordSize - 1);

        var r1OutputGroup = _wireFactory.CreateGroup(new[] { shiftOut }.Concat(internalWires).ToArray());
        var r2InputGroup = _wireFactory.CreateGroup(internalWires.Concat(new[] { shiftIn }).ToArray());

        return (r1OutputGroup, r2InputGroup);
    }
}