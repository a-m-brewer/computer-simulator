using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Circuits;

public interface IWordComparator : ICircuit
{
    IWireGroup<bool> InputsA { get; }
    
    IWireGroup<bool> InputsB { get; }
    
    IWire2<bool> AllBitsAboveEqual { get; }
    
    IWire2<bool> AAboveLarger { get; }

    IWireGroup<bool> UnEqual { get; }
    
    IWire2<bool> Equal { get; }

    IWire2<bool> ALarger { get; }
}

public class WordComparator : CircuitBase, IWordComparator
{
    private readonly IBitComparator[] _bitComparators;

    public WordComparator(
        IWireGroup<bool> inputsA,
        IWireGroup<bool> inputsB,
        IWire2<bool> allBitsAboveEqual,
        IWire2<bool> aAboveLarger,
        IWireGroup<bool> unEqual,
        IWire2<bool> equal, 
        IWire2<bool> aLarger,
        IComponentFactory2 componentFactory,
        IWire2Factory2 wireFactory) : base(componentFactory, wireFactory)
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
    public IWire2<bool> AllBitsAboveEqual { get; }
    public IWire2<bool> AAboveLarger { get; }
    public IWireGroup<bool> UnEqual { get; }
    public IWire2<bool> Equal { get; }
    public IWire2<bool> ALarger { get; }
    
    public void Update()
    {
        for (var i = WireFactory.WordSize - 1; i >= 0; i--)
        {
            _bitComparators[i].Update();
        }
    }
}