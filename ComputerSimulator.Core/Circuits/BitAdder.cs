using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Gates;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Circuits;

public interface IBitAdder : ICircuit
{
    IWire<bool> InputA { get; }

    IWire<bool> InputB { get; }
    
    IWire<bool> CarryIn { get; }
    
    IWire<bool> CarryOut { get; }
    
    IWire<bool> Sum { get; }
}

public class BitAdder : CircuitBase, IBitAdder
{
    private readonly IXOr2 _xOrInput;
    private readonly IXOr2 _xOrSum;
    private readonly IAnd2 _carryOutAnd1;
    private readonly IAnd2 _carryOutAnd2;
    private readonly IOr2 _carryOutOr;

    public BitAdder(
        IWire<bool> inputA,
        IWire<bool> inputB,
        IWire<bool> carryIn,
        IWire<bool> carryOut,
        IWire<bool> sum,
        IComponentFactory componentFactory,
        IWireFactory wireFactory) : base(componentFactory, wireFactory)
    {
        InputA = inputA;
        InputB = inputB;
        CarryIn = carryIn;
        CarryOut = carryOut;
        Sum = sum;

        _xOrInput = ComponentFactory.CreateXOr2(InputA, InputB, WireFactory.CreateWire<bool>());
        _xOrSum = ComponentFactory.CreateXOr2(_xOrInput.Output, CarryIn, Sum);
        _carryOutAnd1 = ComponentFactory.CreateAnd2(carryIn, _xOrInput.Output, WireFactory.CreateWire<bool>());
        _carryOutAnd2 = ComponentFactory.CreateAnd2(InputA, InputB, WireFactory.CreateWire<bool>());
        _carryOutOr = ComponentFactory.CreateOr2(_carryOutAnd1.Output, _carryOutAnd2.Output, CarryOut);
    }

    public IWire<bool> InputA { get; }

    public IWire<bool> InputB { get; }

    public IWire<bool> CarryIn { get; }

    public IWire<bool> CarryOut { get; }

    public IWire<bool> Sum { get; }
    
    public void Update()
    {
        _xOrInput.Update();
        _xOrSum.Update();
        _carryOutAnd1.Update();
        _carryOutAnd2.Update();
        _carryOutOr.Update();
    }
}