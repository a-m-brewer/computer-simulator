using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Factories;

public class LeftShifterWireFactory : IShifterWireFactory
{
    private readonly IWire2Factory2 _wireFactory;

    public LeftShifterWireFactory(IWire2Factory2 wireFactory)
    {
        _wireFactory = wireFactory;
    }
    
    public (IWireGroup<bool> R1OutputGroup, IWireGroup<bool>  R2InputGroup) CreateInternalWires(IWire2<bool> shiftIn, IWire2<bool> shiftOut)
    {
        var internalWires = _wireFactory.CreateWireSet(false, _wireFactory.WordSize - 1);

        var r1OutputGroup = _wireFactory.CreateGroup(internalWires.Concat(new[] { shiftOut }).ToArray());
        var r2InputGroup = _wireFactory.CreateGroup(new[] { shiftIn }.Concat(internalWires).ToArray());

        return (r1OutputGroup, r2InputGroup);
    }
}