using Microsoft.Extensions.Logging;

namespace ComputerSimulator.Core.Parts;

public interface IWire : ILabel
{
    event EventHandler? ValueChanged;
}

public interface IWire<T> : IWire
{
    T Value { get; set; }
}

public class Wire
{
    public static void SetWire<T>(ref IWire<T> oldWire, IWire<T> newWire, EventHandler? action = null)
    {
        if (action != null)
        {
            oldWire.ValueChanged -= action;
        }

        oldWire = newWire;

        if (action != null)
        {
            newWire.ValueChanged += action;
        }
    }
}

public class Wire<T> : Wire, IWire<T>
{
    private T _value;

    private readonly ILogger<Wire<T>> _logger;
    private bool _firstValueChangedEventFired;

    public Wire(T initialValue, string label, ILogger<Wire<T>> logger)
    {
        _value = initialValue;
        _logger = logger;
        Label = label;
    }
    
    public string Label { get; set; }

    public T Value
    {
        get => _value;
        set
        {
            if (_firstValueChangedEventFired)
            {
                if (value == null && _value == null)
                {
                    return;
                }
            
                if (_value != null && _value.Equals(value))
                {
                    return;
                }
            }

            _firstValueChangedEventFired = true;
            
            _logger.LogTrace("{Label} wire changed from {OldValue} to {NewValue}",
                Label,
                _value,
                value);
            
            _value = value;
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler? ValueChanged;

    public override string ToString()
    {
        return Value?.ToString() ?? "null";
    }
}