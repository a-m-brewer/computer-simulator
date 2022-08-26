using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Gates;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Circuits;

public interface IBitComparator : ICircuit
{
    IWire2<bool> InputA { get; }
    
    IWire2<bool> InputB { get; }
    
    IWire2<bool> AllBitsAboveEqual { get; }
    
    IWire2<bool> AAboveLarger { get; }

    IWire2<bool> UnEqual { get; }
    
    IWire2<bool> Equal { get; }

    IWire2<bool> ALarger { get; }
}

public class BitComparator : CircuitBase, IBitComparator
{
    private readonly IXOr2 _xor1;
    private readonly INot _not2;
    private readonly IAnd2 _and3;
    private readonly IAnd _and4;
    private readonly IOr2 _or5;

    public BitComparator(
        IWire2<bool> inputA, 
        IWire2<bool> inputB,
        IWire2<bool> allBitsAboveEqual,
        IWire2<bool> aAboveLarger,
        IWire2<bool> unEqual,
        IWire2<bool> equal,
        IWire2<bool> aLarger,
        IComponentFactory2 componentFactory,
        IWire2Factory2 wireFactory) : base(componentFactory, wireFactory)
    {
        InputA = inputA;
        InputB = inputB;
        UnEqual = unEqual;
        AllBitsAboveEqual = allBitsAboveEqual;
        AAboveLarger = aAboveLarger;
        Equal = equal;
        ALarger = aLarger;

        _xor1 = ComponentFactory.CreateXOr2(InputA, InputB, UnEqual);
        _not2 = ComponentFactory.CreateNot(_xor1.Output, WireFactory.CreateWire(false));
        _and3 = ComponentFactory.CreateAnd2(_not2.Output, AllBitsAboveEqual, Equal);
        _and4 = ComponentFactory.CreateAnd(WireFactory.CreateGroup(AllBitsAboveEqual, InputA, UnEqual), WireFactory.CreateWire(false));
        _or5 = ComponentFactory.CreateOr2(_and4.Output, AAboveLarger, ALarger);
    }

    public IWire2<bool> InputA { get; }

    public IWire2<bool> InputB { get; }

    public IWire2<bool> AllBitsAboveEqual { get; }
    
    public IWire2<bool> AAboveLarger { get; }

    public IWire2<bool> UnEqual { get; }

    public IWire2<bool> Equal { get; }

    public IWire2<bool> ALarger { get; }

    public void Update()
    {
        _xor1.Update();
        _not2.Update();
        _and3.Update();
        _and4.Update();
        _or5.Update();
    }
}