using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Parts;
using ComputerSimulator.Core.Repositories;

namespace ComputerSimulator.Core.Services;

public interface IWireService
{
    IWire2<T> Create<T>(string label, T initialValue);
    IWireGroup<T> CreateGroup<T>(string label);
    IWireGroup<T> CreateGroup<T>(string label, T initialValue);
    IWireGroup<T> CreateGroup<T>(string label, T initialValue, int size);
}

public class WireService : IWireService
{
    private readonly IWire2Factory2 _wire2Factory;
    private readonly IWireRepository _wireRepository;

    public WireService(
        IWire2Factory2 wire2Factory,
        IWireRepository wireRepository)
    {
        _wire2Factory = wire2Factory;
        _wireRepository = wireRepository;
    }
    
    public IWire2<T> Create<T>(string label, T initialValue)
    {
        var wire = _wire2Factory.Create(label, initialValue);
        _wireRepository.Add(wire);

        return wire;
    }

    public IWireGroup<T> CreateGroup<T>(string label)
    {
        var group = _wire2Factory.CreateGroup<T>(label);
        
        _wireRepository.AddGroup(group);

        return group;
    }

    public IWireGroup<T> CreateGroup<T>(string label, T initialValue)
    {
        var group = _wire2Factory.CreateGroup(label, initialValue);

        _wireRepository.AddGroup(group);
        
        return group;
    }

    public IWireGroup<T> CreateGroup<T>(string label, T initialValue, int size)
    {
        var group = _wire2Factory.CreateGroup(label, initialValue, size);

        _wireRepository.AddGroup(group);

        return group;
    }
}