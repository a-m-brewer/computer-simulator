using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Gates;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Circuits;

public interface IRamSlot : IComponent
{
    IWire<bool> Set { get; set; }

    IWire<bool> Enable { get; set; }

    IWire<bool> X { get; set; }

    IWire<bool> Y { get; set; }
    
    IRegister Register { get; }
}

public class RamSlot : ComponentBase, IRamSlot
{
    private readonly IAnd _xAnd;
    private readonly IAnd _setAnd;
    private readonly IAnd _enableAnd;
    private readonly IWire<bool> _internalXAndOutput;
    private readonly IWire<bool> _internalSetAndOutput;
    private readonly IWire<bool> _internalEnableAndOutput;

    public RamSlot(
        IAnd xAnd,
        IAnd setAnd,
        IAnd enableAnd,
        IRegister register,
        IWireCupboard wireCupboard) : base(wireCupboard)
    {
        _internalXAndOutput = wireCupboard.Retrieve(false, this.GenerateLabel(nameof(_internalXAndOutput)));
        _internalSetAndOutput = wireCupboard.Retrieve(false, this.GenerateLabel(nameof(_internalSetAndOutput)));
        _internalEnableAndOutput = wireCupboard.Retrieve(false, this.GenerateLabel(nameof(_internalEnableAndOutput)));

        _xAnd = xAnd;
        _setAnd = setAnd;
        _enableAnd = enableAnd;
        Register = register;

        _xAnd.Output = _internalXAndOutput;
        _setAnd.SetInputWire(1, _internalXAndOutput);
        _enableAnd.SetInputWire(1, _internalXAndOutput);

        Register.Set = _internalSetAndOutput;
        Register.Enable = _internalEnableAndOutput;
    }

    public IWire<bool> Set
    {
        get => _setAnd.GetInputWire(0); 
        set => _setAnd.SetInputWire(0, value);
    }

    public IWire<bool> Enable
    {
        get => _enableAnd.GetInputWire(0); 
        set => _enableAnd.SetInputWire(0, value);
    }

    public IWire<bool> X
    {
        get => _xAnd.GetInputWire(0); 
        set => _xAnd.SetInputWire(0, value);
    }
    
    public IWire<bool> Y
    {
        get => _xAnd.GetInputWire(1); 
        set => _xAnd.SetInputWire(1, value);
    }

    public IRegister Register { get; }
}