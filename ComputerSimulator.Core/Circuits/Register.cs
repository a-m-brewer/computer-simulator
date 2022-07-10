using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Models;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Circuits;

public interface IRegister : IWordComponent
{
    IWire<bool> Set { get; set; }
    
    IWire<bool> Enable { get; set; }
}

public class Register : ComponentBase, IRegister
{
    private readonly IEnabler _enabler;
    private readonly IWord _word;

    public Register(
        ComputerSettings computerSettings,
        IEnabler enabler,
        IWord word,
        IWireCupboard wireCupboard) : base(wireCupboard)
    {
        _enabler = enabler;
        _word = word;

        for (var i = 0; i < computerSettings.WordSize; i++)
        {
            _word.SetOutputWire(i, _enabler.GetInputWire(i));
        }
    }

    public IWire<bool> Set
    {
        get => _word.Set;
        set => _word.Set = value;
    }

    public IWire<bool> Enable
    {
        get => _enabler.Enable;
        set => _enabler.Enable = value;
    }

    public void SetInputWire(int index, IWire<bool> wire)
    {
        _word.SetInputWire(index, wire);
    }

    public void SetOutputWire(int index, IWire<bool> wire)
    {
        _enabler.SetOutputWire(index, wire);
    }

    public void SetInputWireValue(int index, bool value)
    {
        _word.SetInputWireValue(index, value);
    }

    public IWire<bool> GetInputWire(int index)
    {
        return _word.GetInputWire(index);
    }

    public bool GetOutputWireValue(int index)
    {
        return _enabler.GetOutputWireValue(index);
    }

    public override void Dispose()
    {
        _word.Dispose();
        _enabler.Dispose();

        GC.SuppressFinalize(this);
    }
}