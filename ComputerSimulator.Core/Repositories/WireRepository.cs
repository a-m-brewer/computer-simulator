using System.Collections.Concurrent;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Repositories;

public interface IWireRepository
{
    ICollection<IWire2> Wires { get; }
    
    ICollection<IWireGroup> Groups { get; }

    void Add(IWire2 wire);
    void AddRange(IEnumerable<IWire2> wires);
    void AddGroup<T>(IWireGroup<T> wireGroup);
}

public class WireRepository : IWireRepository
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    private readonly ConcurrentDictionary<string, IWire2> _wires = new();
    private readonly ConcurrentDictionary<string, IWireGroup> _wireGroups = new();

    public ICollection<IWire2> Wires => _wires.Values;
    public ICollection<IWireGroup> Groups => _wireGroups.Values;

    public void Add(IWire2 wire)
    {
        if (_wires.ContainsKey(wire.Label))
        {
            throw new Exception("Wire already in wire repository");
        }

        _wires[wire.Label] = wire;
    }

    public void AddRange(IEnumerable<IWire2> wires)
    {
        foreach (var wire in wires)
        {
            Add(wire);
        }
    }

    public void AddGroup<T>(IWireGroup<T> wireGroup)
    {
        if (_wireGroups.ContainsKey(wireGroup.Label))
        {
            throw new Exception("Wire Group already in repository");
        }

        AddRange(wireGroup);
        
        _wireGroups[wireGroup.Label] = wireGroup;
    }
}