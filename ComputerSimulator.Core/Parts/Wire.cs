namespace ComputerSimulator.Core.Parts;

public interface IWire
{
}

public interface IWire<T> : IWire
{
    T Value { get; set; }
}

public class Wire<T> : IWire<T>
{
    private T _value;
    private bool _valueSet;

    public Wire(T value)
    {
        _value = value;
    }

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
