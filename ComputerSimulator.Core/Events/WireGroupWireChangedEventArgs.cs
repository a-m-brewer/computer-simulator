using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Events;

public class WireGroupWireChangedEventArgs<T> : EventArgs
{
    public WireGroupWireChangedEventArgs(
        int index,
        IWire2<T> newWire)
    {
        Index = index;
        NewWire = newWire;
    }
    
    public int Index { get; }
    public IWire2<T>? OldWire { get; set; }
    public IWire2<T> NewWire { get; }
}