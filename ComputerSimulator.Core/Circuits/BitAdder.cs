using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Gates;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Circuits;

public interface IBitAdder : ICircuit
{
    IWire2<bool> InputA { get; }

    IWire2<bool> InputB { get; }
    
    IWire2<bool> CarryIn { get; }
    
    IWire2<bool> CarryOut { get; }
    
    IWire2<bool> Sum { get; }
}

public class BitAdder : CircuitBase, IBitAdder
{
    private readonly IXOr2 _xOrInput;
    private readonly IXOr2 _xOrSum;
    private readonly IAnd2 _carryOutAnd1;
    private readonly IAnd2 _carryOutAnd2;
    private readonly IOr2 _carryOutOr;

    public BitAdder(
        IWire2<bool> inputA,
        IWire2<bool> inputB,
        IWire2<bool> carryIn,
        IWire2<bool> carryOut,
        IWire2<bool> sum,
        IComponentFactory2 componentFactory,
        IWire2Factory2 wireFactory) : base(componentFactory, wireFactory)
    {
        InputA = inputA;
        InputB = inputB;
        CarryIn = carryIn;
        CarryOut = carryOut;
        Sum = sum;

        _xOrInput = ComponentFactory.CreateXOr2(InputA, InputB, WireFactory.CreateWire(false));
        _xOrSum = ComponentFactory.CreateXOr2(_xOrInput.Output, CarryIn, Sum);
        _carryOutAnd1 = ComponentFactory.CreateAnd2(carryIn, _xOrInput.Output, WireFactory.CreateWire(false));
        _carryOutAnd2 = ComponentFactory.CreateAnd2(InputA, InputB, WireFactory.CreateWire(false));
        _carryOutOr = ComponentFactory.CreateOr2(_carryOutAnd1.Output, _carryOutAnd2.Output, CarryOut);
    }

    public IWire2<bool> InputA { get; }

    public IWire2<bool> InputB { get; }

    public IWire2<bool> CarryIn { get; }

    public IWire2<bool> CarryOut { get; }

    public IWire2<bool> Sum { get; }
    
    public void Update()
    {
        _xOrInput.Update();
        _xOrSum.Update();
        _carryOutAnd1.Update();
        _carryOutAnd2.Update();
        _carryOutOr.Update();
    }
}