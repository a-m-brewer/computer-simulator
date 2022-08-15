namespace ComputerSimulator.Core.Parts;

public interface IWire2
{
}

public interface IWire2<T> : IWire2
{
    T Value { get; set; }
}

public class EventWire<T> : IWire2<T>
{
    private T _value;
    private bool _valueSet;

    public EventWire(T value)
    {
        _value = value;
    }

    public event EventHandler? ValueChanged;

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
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
