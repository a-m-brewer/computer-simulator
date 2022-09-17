namespace ComputerSimulator.Core.Parts;

public class ReadOnlyWire<T> : IWire<T>
{
    private readonly T _value;

    public ReadOnlyWire(T value, string label)
    {
        Label = label;
        _value = value;
    }

    public string Label { get; }

    public T Value
    {
        get => _value;
        set { }
    }
}