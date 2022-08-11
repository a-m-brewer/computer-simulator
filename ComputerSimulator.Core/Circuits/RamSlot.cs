using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Gates;
using ComputerSimulator.Core.Parts;
using ComputerSimulator.Core.Services;

namespace ComputerSimulator.Core.Circuits;

public interface IRamSlot : IComponent2
{
    IWire2<bool> Set { get; set; }

    IWire2<bool> Enable { get; set; }

    IWire2<bool> X { get; set; }

    IWire2<bool> Y { get; set; }
    
    IRegister Register { get; }
}

public class RamSlot : PartsBase, IRamSlot
{
    // Gates
    private readonly IAnd _xAnd;
    private readonly IAnd _setAnd;
    private readonly IAnd _enableAnd;

    public RamSlot(
        IAnd xAnd,
        IAnd setAnd,
        IAnd enableAnd,
        IRegister register,
        IWireService wireService) : base(wireService)
    {
        var internalXAndOutput = CreateInternalWire("internalXAndOutput", false);
        var internalSetAndOutput = CreateInternalWire("internalSetAndOutput", false);
        var internalEnableAndOutput = CreateInternalWire("internalEnableAndOutput", false);

        _xAnd = xAnd;
        _setAnd = setAnd;
        _enableAnd = enableAnd;
        Register = register;

        _xAnd.Output = internalXAndOutput;
        _setAnd.Inputs.SetWire(1, internalXAndOutput);
        _enableAnd.Inputs.SetWire(1, internalXAndOutput);

        Register.Set = internalSetAndOutput;
        Register.Enable = internalEnableAndOutput;
    }

    public IWire2<bool> Set
    {
        get => _setAnd.Inputs[0]; 
        set => _setAnd.Inputs.SetWire(0, value);
    }

    public IWire2<bool> Enable
    {
        get => _enableAnd.Inputs[0]; 
        set => _enableAnd.Inputs.SetWire(0, value);
    }

    public IWire2<bool> X
    {
        get => _xAnd.Inputs[0]; 
        set => _xAnd.Inputs.SetWire(0, value);
    }
    
    public IWire2<bool> Y
    {
        get => _xAnd.Inputs[1]; 
        set => _xAnd.Inputs.SetWire(1, value);
    }

    public IRegister Register { get; }
}