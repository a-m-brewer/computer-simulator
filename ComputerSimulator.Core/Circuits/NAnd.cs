using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Gates;
using ComputerSimulator.Core.Models;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Circuits;

public interface INAnd : IComponent2
{
    IWireGroup<bool> Inputs { get; }
    IWire2<bool> Output { get; }
}

public class NAnd : CircuitBase, INAnd
{
    // ReSharper disable once NotAccessedField.Local
    private readonly IAnd _andGate;
    // ReSharper disable once NotAccessedField.Local
    private readonly INot _notGate;

    public NAnd(
        IWireGroup<bool> inputs,
        IWire2<bool> output,
        ComputerSettings computerSettings,
        IComponentFactory2 componentFactory2,
        IWire2Factory2 wireFactory)
    : base(componentFactory2, wireFactory)
    {
        Inputs = inputs;
        Output = output;

        if (computerSettings.SimulateNAnd)
        {
            var andToNot = WireFactory.CreateWire(false);
            
            _andGate = ComponentFactory.CreateAnd(Inputs, andToNot);
            _notGate = ComponentFactory.CreateNot(andToNot, Output);
        }
        else
        {
            inputs.SubscribeToWireValuesChanged(ValueChanged);
            _andGate = null!;
            _notGate = null!;
        }
    }

    public IWireGroup<bool> Inputs { get; }

    public IWire2<bool> Output { get; }
    
    private void ValueChanged(object? sender, int e)
    {
        for (var i = 0; i < Inputs.Count; i++)
        {
            if (Inputs[i].Value)
            {
                continue;
            }

            Output.Value = true;
            return;
        }

        Output.Value = false;
    }
}