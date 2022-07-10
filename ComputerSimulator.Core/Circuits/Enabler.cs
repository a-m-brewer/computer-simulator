using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Gates;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Circuits;

public interface IEnabler : IWordComponent
{
    IWire<bool> Enable { get; set; }
}

public class Enabler : WordComponentBase, IEnabler
{
    private IWire<bool> _enable;
    private readonly IAnd[] _gates;

    public Enabler(
        IComponentFactory componentFactory,
        IWireCupboard wireCupboard) : base(wireCupboard)
    {
        _enable = WireCupboard.Retrieve(false, this.GenerateLabel(nameof(_enable)));
        _gates = componentFactory.CreateSet<IAnd>();

        for (var i = 0; i < _gates.Length; i++)
        {
            _gates[i].Label = this.GenerateLabel($"{nameof(_gates)}[{i}]");
            _gates[i].SetInputWire(0, Inputs[i]);
            _gates[i].SetInputWire(1, _enable);
            _gates[i].Output = Outputs[i];
        }
    }

    public IWire<bool> Enable
    {
        get => _enable;
        set
        {
            _enable = value;
            foreach (var gate in _gates)
            {
                gate.SetInputWire(1, _enable);
            }
        }
    }

    public override void SetInputWire(int index, IWire<bool> wire)
    {
        base.SetInputWire(index, wire);
        _gates[index].SetInputWire(0, Inputs[index]);
    }

    public override void SetOutputWire(int index, IWire<bool> wire)
    {
        base.SetOutputWire(index, wire);
        _gates[index].Output = Outputs[index];
    }
}