namespace ComputerSimulator.Core.Parts;

public interface ISetEnableWire<T>
{
    IWire<T> Set { get; }

    IWire<T> Enable { get; }
}

public class SetEnableWire<T> : ISetEnableWire<T>
{
    public SetEnableWire(IWire<T> set, IWire<T> enable)
    {
        Set = set;
        Enable = enable;
    }

    public IWire<T> Set { get; }
    public IWire<T> Enable { get; }
}