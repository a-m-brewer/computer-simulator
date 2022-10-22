using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Circuits;

public interface ICaezRegister
{
    IWire<bool> Set { get; }

    IWire<bool> Enable { get; }

    ICaez<bool> Inputs { get; }

    ICaez<bool> Outputs { get; }
    
    /// <summary>
    /// Purely for debug/testing purposes only. Do not use for any actual code
    /// </summary>
    ICaez<bool> StoredValue { get; }
}

public class CaezRegister : Register, ICaezRegister
{
    public CaezRegister(
        IWire<bool> set, 
        ICaez<bool> inputs, 
        ICaez<bool> outputs,
        IComponentFactory componentFactory,
        IWireFactory wireFactory) : base(set, wireFactory.PowerWire, inputs, outputs, componentFactory, wireFactory)
    {
    }

    public new ICaez<bool> Inputs => base.Inputs as ICaez<bool> ?? throw new ArgumentException(nameof(Inputs));
    public new ICaez<bool> Outputs => base.Outputs as ICaez<bool> ?? throw new ArgumentException(nameof(Outputs));
    public new ICaez<bool> StoredValue => base.StoredValue as ICaez<bool> ?? throw new ArgumentException(nameof(StoredValue));
}