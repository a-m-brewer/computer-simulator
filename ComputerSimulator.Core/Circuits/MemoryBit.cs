using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Circuits;

public interface IMemoryBit : ICircuit
{
    public IWire2<bool> Input { get; }

    public IWire2<bool> Set { get; }

    public IWire2<bool> Output { get; }
}

public class MemoryBit : CircuitBase, IMemoryBit
{
    // ReSharper disable once NotAccessedField.Local
    private readonly INAnd2 _nAnd1;
    // ReSharper disable once NotAccessedField.Local
    private readonly INAnd2 _nAnd2;
    // ReSharper disable once NotAccessedField.Local
    private readonly INAnd2 _nAnd3;
    // ReSharper disable once NotAccessedField.Local
    private readonly INAnd2 _nAnd4;

    public MemoryBit(
        IWire2<bool> input,
        IWire2<bool> set,
        IWire2<bool> output,
        IComponentFactory2 componentFactory,
        IWire2Factory2 wireFactory)
    : base(componentFactory, wireFactory)
    {
        Input = input;
        Output = output;
        Set = set;
        
        var a = WireFactory.CreateWire(false);
        var b = WireFactory.CreateWire(false);
        var c = WireFactory.CreateWire(false);
        
        
        _nAnd1 = ComponentFactory.CreateNAnd2(Input, Set, a);
        _nAnd2 = ComponentFactory.CreateNAnd2(a, Set, b);
        _nAnd3 = ComponentFactory.CreateNAnd2(a, c, Output);
        _nAnd4 = ComponentFactory.CreateNAnd2(Output, b, c);
    }
    
    public IWire2<bool> Input { get; }

    public IWire2<bool> Set { get; }

    public IWire2<bool> Output { get; }

    public void Update()
    {
        _nAnd1.Update();
        _nAnd2.Update();
        
        // This is on purpose need to update 4 to compute value of 3
        _nAnd4.Update();
        
        _nAnd3.Update();
    }
}