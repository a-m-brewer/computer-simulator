using System.Collections;
using ComputerSimulator.Core.Enums;

namespace ComputerSimulator.Core.Parts;

public interface IOp : IWireGroup<bool> 
{}

public class Op : IOp
{
    private readonly IList<IWire2<bool>> _wires;

    public Op(IWire2<bool> zero, IWire2<bool> one, IWire2<bool> two)
    {
        _wires = new[] { zero, one, two };
    }
    
    public IEnumerator<IWire2<bool>> GetEnumerator()
    {
        return _wires.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count => _wires.Count;

    public IWire2<bool> this[int index] => _wires[index];

    public IWire2<bool> this[OpCode index] => _wires[(int)index];
}