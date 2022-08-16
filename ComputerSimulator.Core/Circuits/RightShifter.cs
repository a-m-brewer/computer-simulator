using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Circuits;

public interface IRightShifter : IShifter {}

public class RightShifter : CircuitBase, IRightShifter
{
    private readonly IRegister _r1;
    private readonly IRegister _r2;

    public RightShifter(
        IWire2<bool> shiftIn,
        IWire2<bool> shiftOut,
        IWireGroup<bool> input,
        IWireGroup<bool> output,
        IComponentFactory2 componentFactory,
        IWire2Factory2 wireFactory) : base(componentFactory, wireFactory)
    {
        ShiftIn = shiftIn;
        ShiftOut = shiftOut;
        Input = input;
        Output = output;

        _r1 = ComponentFactory.CreateRegister(WireFactory.PowerWire, WireFactory.PowerWire, input, WireFactory.CreateGroup(false));
        _r2 = ComponentFactory.CreateRegister(WireFactory.PowerWire, WireFactory.PowerWire, , Output);
    }

    public IWire2<bool> ShiftIn { get; }
    public IWire2<bool> ShiftOut { get; }
    public IWireGroup<bool> Input { get; }
    public IWireGroup<bool> Output { get; }

    public void Update()
    {
        
    }
}