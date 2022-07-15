using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Models;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core;

public abstract class WordComponentBase : ComponentBase, IWordComponent
{
    protected readonly IWire<bool>[] Inputs;
    protected readonly IWire<bool>[] Outputs;

    protected WordComponentBase(
        IWireCupboard wireCupboard) : base(wireCupboard)
    {
        Inputs = WireCupboard.RetrieveSet(false, this.GenerateLabel(nameof(Inputs)));
        Outputs = WireCupboard.RetrieveSet(false, this.GenerateLabel(nameof(Outputs)));
    }

    public void SetInputs(IBus bus)
    {
        for (var i = 0; i < bus.Length; i++)
        {
            Inputs[i] = bus.GetWire(i);
        }
    }

    public virtual void SetInputWire(int index, IWire<bool> wire)
    {
        Inputs[index] = wire;
    }

    public void SetOutputs(IBus bus)
    {
        for (var i = 0; i < bus.Length; i++)
        {
            Outputs[i] = bus.GetWire(i);
        }
    }

    public virtual void SetOutputWire(int index, IWire<bool> wire)
    {
        Outputs[index] = wire;
    }

    public IWire<bool> GetOutputWire(int index)
    {
        return Outputs[index];
    }

    public virtual void SetInputWireValue(int index, bool value)
    {
        Inputs[index].Value = value;
    }

    public virtual IWire<bool> GetInputWire(int index)
    {
        return Inputs[index];
    }

    public virtual bool GetOutputWireValue(int index)
    {
        return Outputs[index].Value;
    }
}