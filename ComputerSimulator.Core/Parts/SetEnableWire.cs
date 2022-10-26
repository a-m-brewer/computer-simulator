namespace ComputerSimulator.Core.Parts;

public interface ISetEnableWire<T> where T : new()
{
    IWire<T> Set { get; }

    IWire<T> Enable { get; }
}

public class SetEnableWire<T> : ISetEnableWire<T> where T : new()
{
    public SetEnableWire(IWire<T> set, IWire<T> enable)
    {
        Set = set;
        Enable = enable;
    }

    public IWire<T> Set { get; }
    public IWire<T> Enable { get; }
}