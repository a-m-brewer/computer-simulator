using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Gates;

namespace ComputerSimulator.Core.Parts;

public interface IBus1 : IPart
{
    IWire<bool> Bit { get; }
    
    IWireGroup<bool> Inputs { get; }
    
    IWireGroup<bool> Outputs { get; }
}

public class Bus1 : PartsBase, IBus1
{
    private readonly INot _not;
    private readonly IAnd2[] _ands;
    private readonly IOr2 _or;

    public Bus1(
        IWire<bool> bit,
        IWireGroup<bool> inputs,
        IWireGroup<bool> outputs,
        IComponentFactory componentFactory, 
        IWireFactory wireFactory) : base(componentFactory, wireFactory)
    {
        Bit = bit;
        Inputs = inputs;
        Outputs = outputs;

        _not = ComponentFactory.CreateNot(Bit, WireFactory.CreateWire<bool>());
        _or = ComponentFactory.CreateOr2(Inputs[0], Bit, Outputs[0]);
        _ands = (WireFactory.WordSize - 1)
            .InitArray<IAnd2>()
            .Fill(i => ComponentFactory.CreateAnd2(Inputs[i + 1], _not.Output, Outputs[i + 1]));
    }

    public IWire<bool> Bit { get; }
    public IWireGroup<bool> Inputs { get; }
    public IWireGroup<bool> Outputs { get; }
    
    public void Update()
    {
        _not.Update();
        _or.Update();
        foreach (var and2 in _ands)
        {
            and2.Update();
        }
    }
}