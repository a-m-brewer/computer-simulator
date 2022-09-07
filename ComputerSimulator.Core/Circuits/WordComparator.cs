using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Circuits;

public interface IWordComparator : ICircuit
{
    IWireGroup<bool> InputsA { get; }
    
    IWireGroup<bool> InputsB { get; }
    
    IWire<bool> AllBitsAboveEqual { get; }
    
    IWire<bool> AAboveLarger { get; }

    IWireGroup<bool> UnEqual { get; }
    
    IWire<bool> Equal { get; }

    IWire<bool> ALarger { get; }
}

public class WordComparator : CircuitBase, IWordComparator
{
    private readonly IBitComparator[] _bitComparators;

    public WordComparator(
        IWireGroup<bool> inputsA,
        IWireGroup<bool> inputsB,
        IWire<bool> allBitsAboveEqual,
        IWire<bool> aAboveLarger,
        IWireGroup<bool> unEqual,
        IWire<bool> equal, 
        IWire<bool> aLarger,
        IComponentFactory componentFactory,
        IWireFactory wireFactory) : base(componentFactory, wireFactory)
    {
        InputsA = inputsA;
        InputsB = inputsB;
        AllBitsAboveEqual = allBitsAboveEqual;
        AAboveLarger = aAboveLarger;
        UnEqual = unEqual;
        Equal = equal;
        ALarger = aLarger;

        _bitComparators = new IBitComparator[WireFactory.WordSize];
        for (var i = WireFactory.WordSize - 1; i >= 0 ; i--)
        {
            _bitComparators[i] = ComponentFactory.CreateBitComparator(
                InputsA[i],
                InputsB[i],
                i == WireFactory.WordSize - 1 ? AllBitsAboveEqual : _bitComparators[i + 1].Equal,
                i == WireFactory.WordSize - 1 ? AAboveLarger : _bitComparators[i + 1].ALarger,
                UnEqual[i],
                i == 0 ? Equal : WireFactory.CreateWire(false),
                i == 0 ? ALarger : WireFactory.CreateWire(false));
        }
    }

    public IWireGroup<bool> InputsA { get; }
    public IWireGroup<bool> InputsB { get; }
    public IWire<bool> AllBitsAboveEqual { get; }
    public IWire<bool> AAboveLarger { get; }
    public IWireGroup<bool> UnEqual { get; }
    public IWire<bool> Equal { get; }
    public IWire<bool> ALarger { get; }
    
    public void Update()
    {
        for (var i = WireFactory.WordSize - 1; i >= 0; i--)
        {
            _bitComparators[i].Update();
        }
    }
}