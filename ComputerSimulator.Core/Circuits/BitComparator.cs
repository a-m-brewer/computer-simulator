using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Gates;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Circuits;

public interface IBitComparator : ICircuit
{
    IWire<bool> InputA { get; }
    
    IWire<bool> InputB { get; }
    
    IWire<bool> AllBitsAboveEqual { get; }
    
    IWire<bool> AAboveLarger { get; }

    IWire<bool> UnEqual { get; }
    
    IWire<bool> Equal { get; }

    IWire<bool> ALarger { get; }
}

public class BitComparator : CircuitBase, IBitComparator
{
    private readonly IXOr2 _xor1;
    private readonly INot _not2;
    private readonly IAnd2 _and3;
    private readonly IAnd _and4;
    private readonly IOr2 _or5;

    public BitComparator(
        IWire<bool> inputA, 
        IWire<bool> inputB,
        IWire<bool> allBitsAboveEqual,
        IWire<bool> aAboveLarger,
        IWire<bool> unEqual,
        IWire<bool> equal,
        IWire<bool> aLarger,
        IComponentFactory componentFactory,
        IWireFactory wireFactory) : base(componentFactory, wireFactory)
    {
        InputA = inputA;
        InputB = inputB;
        UnEqual = unEqual;
        AllBitsAboveEqual = allBitsAboveEqual;
        AAboveLarger = aAboveLarger;
        Equal = equal;
        ALarger = aLarger;

        _xor1 = ComponentFactory.CreateXOr2(InputA, InputB, UnEqual);
        _not2 = ComponentFactory.CreateNot(_xor1.Output, WireFactory.CreateWire<bool>());
        _and3 = ComponentFactory.CreateAnd2(_not2.Output, AllBitsAboveEqual, Equal);
        _and4 = ComponentFactory.CreateAnd(WireFactory.CreateGroup(AllBitsAboveEqual, InputA, UnEqual), WireFactory.CreateWire<bool>());
        _or5 = ComponentFactory.CreateOr2(_and4.Output, AAboveLarger, ALarger);
    }

    public IWire<bool> InputA { get; }

    public IWire<bool> InputB { get; }

    public IWire<bool> AllBitsAboveEqual { get; }
    
    public IWire<bool> AAboveLarger { get; }

    public IWire<bool> UnEqual { get; }

    public IWire<bool> Equal { get; }

    public IWire<bool> ALarger { get; }

    public void Update()
    {
        _xor1.Update();
        _not2.Update();
        _and3.Update();
        _and4.Update();
        _or5.Update();
    }
}