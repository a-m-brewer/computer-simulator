using System.Diagnostics;

namespace ComputerSimulator.Core.Parts;

public interface IWire
{
}

public interface IWire<T> : IWire where T : new()
{
    string Label { get; }
    T Value { get; set; }
}

public interface IResettableWire<T> : IWire<T>, IResettable where T : new()
{
}

[DebuggerDisplay("{Label}: {Value}")]
public class Wire<T> : IResettableWire<T> where T : new()
{
    private T _value;
    private readonly string? _label;

    public Wire(string? label = null)
    {
        _value = new T();
        _label = label;
    }

    public string Label => _label ?? GetHashCode().ToString();

    public T Value
    {
        get => _value;
        set => _value = value;
    }

    public void Reset()
    {
        _value = new T();
    }
}
