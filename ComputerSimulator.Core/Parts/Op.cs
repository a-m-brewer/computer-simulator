using System.Collections;
using ComputerSimulator.Core.Enums;
using ComputerSimulator.Core.Exceptions;

namespace ComputerSimulator.Core.Parts;

public interface IOp : IWireGroup<bool>
{
    void SetOpCode(OpCode opCode);
}

public class Op : IOp
{
    private readonly IList<IWire<bool>> _wires;

    public Op(IList<IWire<bool>> wires)
    {
        if (wires.Count != 3)
        {
            throw new ComputerSimulatorException("Op wire length should be 3");
        }
        
        _wires = wires;
    }
    
    public IEnumerator<IWire<bool>> GetEnumerator()
    {
        return _wires.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count => _wires.Count;

    public IWire<bool> this[int index] => _wires[index];

    public IWire<bool> this[OpCode index] => _wires[(int)index];
    public void SetValue(bool[] values)
    {
        for (var i = 0; i < values.Length; i++)
        {
            _wires[i].Value = values[i];
        }
    }

    public void SetOpCode(OpCode opCode)
    {
        switch (opCode)
        {
            case OpCode.Add:
                SetWires(false, false, false);
                break;
            case OpCode.Shr:
                SetWires(false, false, true);
                break;
            case OpCode.Shl:
                SetWires(false, true, false);
                break;
            case OpCode.Not:
                SetWires(false, true, true);
                break;
            case OpCode.And:
                SetWires(true, false, false);
                break;
            case OpCode.Or:
                SetWires(true, false, true);
                break;
            case OpCode.XOr:
                SetWires(true, true, false);
                break;
            case OpCode.Cmp:
                SetWires(true, true, true);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(opCode), opCode, null);
        }
    }

    // do the arguments backwards so it's easier to read as binary
    private void SetWires(bool two, bool one, bool zero)
    {
        _wires[0].Value = zero;
        _wires[1].Value = one;
        _wires[2].Value = two;
    }

    public void Reset()
    {
        foreach (var resettable in _wires.OfType<IResettable>())
        {
            resettable.Reset();
        }
    }
}