using System.Diagnostics;

namespace ComputerSimulator.Core.Parts;

public interface IWire
{
}

public interface IWire<T> : IWire
{
    string Label { get; }
    T Value { get; set; }
}

[DebuggerDisplay("{Label}: {Value}")]
public class Wire<T> : IWire<T>
{
    private T _value;
    private bool _valueSet;
    private readonly string? _label;

    public Wire(T value, string? label = null)
    {
        _value = value;
        _label = label;
    }

    public string Label => _label ?? GetHashCode().ToString();

    public T Value
    {
        get => _value;
        set
        {
            if (_valueSet && EqualityComparer<T>.Default.Equals(_value, value))
            {
                return;
            }

            _valueSet = true;
            _value = value;
        }
    }
}
