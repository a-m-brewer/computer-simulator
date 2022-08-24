using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Circuits;

public class Shifter : CircuitBase, IShifter
{
    private readonly IRegister _r1;
    private readonly IRegister _r2;

    public Shifter(
        IWire2<bool> shiftIn,
        IWire2<bool> shiftOut,
        IWireGroup<bool> input,
        IWireGroup<bool> output,
        IShifterWireFactory shifterWireFactory,
        IComponentFactory2 componentFactory,
        IWire2Factory2 wireFactory) : base(componentFactory, wireFactory)
    {
        ShiftIn = shiftIn;
        ShiftOut = shiftOut;
        Input = input;
        Output = output;

        var (r1OutputGroup, r2InputGroup) = shifterWireFactory.CreateInternalWires(ShiftIn, ShiftOut);

        _r1 = ComponentFactory.CreateRegister(WireFactory.PowerWire, WireFactory.PowerWire, input, r1OutputGroup);
        _r2 = ComponentFactory.CreateRegister(WireFactory.PowerWire, WireFactory.PowerWire, r2InputGroup, Output);
    }

    public IWire2<bool> ShiftIn { get; }
    public IWire2<bool> ShiftOut { get; }
    public IWireGroup<bool> Input { get; }
    public IWireGroup<bool> Output { get; }

    public void Update()
    {
        _r1.Update();
        _r2.Update();
    }
}