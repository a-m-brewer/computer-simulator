using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Factories;

public class RightShifterWireFactory : IShifterWireFactory
{
    private readonly IWireFactory _wireFactory;

    public RightShifterWireFactory(IWireFactory wireFactory)
    {
        _wireFactory = wireFactory;
    }
    
    public (IWireGroup<bool> R1OutputGroup, IWireGroup<bool>  R2InputGroup) CreateInternalWires(IWire<bool> shiftIn, IWire<bool> shiftOut)
    {
        var internalWires = _wireFactory.CreateWireSet<bool>(_wireFactory.WordSize - 1);

        var r1OutputGroup = _wireFactory.CreateGroup(new[] { shiftOut }.Concat(internalWires).ToArray());
        var r2InputGroup = _wireFactory.CreateGroup(internalWires.Concat(new[] { shiftIn }).ToArray());

        return (r1OutputGroup, r2InputGroup);
    }
}