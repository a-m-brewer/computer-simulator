using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Circuits;

public interface IWord : IWordComponent
{
    IWire<bool> Set { get; set; }
}

public class Word : WordComponentBase, IWord
{
    private string _label = nameof(Word);
    private IWire<bool> _set;
    private readonly IMemoryBit[] _memory;

    public Word(
        IComponentFactory componentFactory,
        IWireCupboard wireCupboard) : base( wireCupboard)
    {
        _set = WireCupboard.Retrieve(false, this.GenerateLabel(nameof(_set)));
        _memory = componentFactory.CreateSet<IMemoryBit>();

        for (var i = 0; i < _memory.Length; i++)
        {
            _memory[i].Label = this.GenerateLabel($"{nameof(_memory)}[{i}]");
            _memory[i].Set = _set;
            _memory[i].Input = Inputs[i];
            _memory[i].Output = Outputs[i];
        }
    }

    public override string Label
    {
        get => _label;
        set => _label = value;
    }

    public IWire<bool> Set
    {
        get => _set;
        set
        {
            _set = value;
            foreach (var bit in _memory)
            {
                bit.Set = _set;
            }
        }
    }

    public override void SetInputWire(int index, IWire<bool> wire)
    {
        base.SetInputWire(index, wire);
        _memory[index].Input = Inputs[index];
    }

    public override void SetOutputWire(int index, IWire<bool> wire)
    {
        base.SetOutputWire(index, wire);
        _memory[index].Output = Outputs[index];
    }

    public override void Dispose()
    {
        foreach (var bit in _memory)
        {
            bit.Dispose();
        }
        
        GC.SuppressFinalize(this);
    }
}