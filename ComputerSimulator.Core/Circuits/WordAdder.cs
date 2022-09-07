using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Circuits;

public interface IWordAdder : ICircuit
{
    IWireGroup<bool> InputsA { get; }

    IWireGroup<bool> InputsB { get; }
    
    IWire<bool> CarryIn { get; }
    
    IWire<bool> CarryOut { get; }
    
    IWireGroup<bool> Sum { get; }
}

public class WordAdder : CircuitBase, IWordAdder
{
    private readonly IBitAdder[] _bitAdders;

    public WordAdder(
        IWireGroup<bool> inputsA,
        IWireGroup<bool> inputsB,
        IWire<bool> carryIn,
        IWire<bool> carryOut,
        IWireGroup<bool> sum,
        IComponentFactory componentFactory,
        IWireFactory wireFactory) : base(componentFactory, wireFactory)
    {
        InputsA = inputsA;
        InputsB = inputsB;
        CarryIn = carryIn;
        CarryOut = carryOut;
        Sum = sum;

        _bitAdders = new IBitAdder[WireFactory.WordSize];
        for (var i = 0; i < WireFactory.WordSize; i++)
        {
            _bitAdders[i] = ComponentFactory.CreateBitAdder(
                InputsA[i],
                InputsB[i],
                i == 0 ? CarryIn : _bitAdders[i - 1].CarryOut,
                i == WireFactory.WordSize - 1 ? CarryOut : WireFactory.CreateWire(false),
                Sum[i]);
        }
    }

    public IWireGroup<bool> InputsA { get; }

    public IWireGroup<bool> InputsB { get; }

    public IWire<bool> CarryIn { get; }

    public IWire<bool> CarryOut { get; }

    public IWireGroup<bool> Sum { get; }
    
    public void Update()
    {
        foreach (var bitAdder in _bitAdders)
        {
            bitAdder.Update();
        }
    }
}