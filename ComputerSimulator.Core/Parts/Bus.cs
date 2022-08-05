using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Models;

namespace ComputerSimulator.Core.Parts;

public interface IBus : IComponent
{
    void SetWire(int index, IWire<bool> wire);
    
    void SetWires(IOutputComponent outputComponent);

    void SetWireValue(int index, bool value);
    
    IWire<bool> GetWire(int index);

    int Length { get; }
}

public class Bus : ComponentBase, IBus
{
    private readonly ComputerSettings _settings;
    private readonly IWire<bool>[] _wires;

    public Bus(
        ComputerSettings settings,
        IWireCupboard wireCupboard) : base(wireCupboard)
    {
        _settings = settings;
        _wires = wireCupboard.RetrieveSet(false, this.GenerateLabel(nameof(_wires)));
    }


    public void SetWire(int index, IWire<bool> wire)
    {
        _wires[index] = wire;
    }

    public void SetWires(IOutputComponent outputComponent)
    {
        for (var i = 0; i < _settings.WordSize; i++)
        {
            _wires[i] = outputComponent.GetOutputWire(i);
        }
    }

    public void SetWireValue(int index, bool value)
    {
        _wires[index].Value = value;
    }

    public IWire<bool> GetWire(int index)
    {
        return _wires[index];
    }

    public int Length => _settings.WordSize;
}