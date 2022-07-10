using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Circuits;

public interface IDecoder : IInputComponent, IOutputComponent
{
    /// <summary>
    /// Initialize the decode e.g passing 3 will make a 3x8 decoder and so on.
    /// </summary>
    /// <param name="inputSize">Size of inputs</param>
    public void Initialize(int inputSize);
    
    int EnabledIndex { get; }
    
    int OutputSize { get; }
}

public class Decoder : ComponentBase, IDecoder
{
    private readonly Dictionary<int, IWire<bool>> _inputs = new();
    private readonly Dictionary<int, IWire<bool>> _outputs = new();
    
    private bool[][] _truthTable = Array.Empty<bool[]>();
    private int _inputSize;

    public Decoder(IWireCupboard wireCupboard) : base(wireCupboard)
    {
    }

    public void Initialize(int inputSize)
    {
        _inputs.Clear();
        _outputs.Clear();
        
        _inputSize = inputSize;
        OutputSize = (int) Math.Pow(2, inputSize);

        _truthTable = GenerateCombinations();

        for (var i = 0; i < _inputSize; i++)
        {
            _inputs[i] = WireCupboard.Retrieve(false, this.GenerateLabel($"{nameof(_inputs)}[{i}]"));
            _inputs[i].ValueChanged += InputsChanged;
        }

        for (var i = 0; i < OutputSize; i++)
        {
            _outputs[i] = WireCupboard.Retrieve(false, this.GenerateLabel($"{nameof(_outputs)}[{i}]"));
        }
    }

    public int EnabledIndex { get; private set; }

    public int OutputSize { get; private set; }

    private void InputsChanged(object? sender, EventArgs e)
    {
        for (var row = 0; row < OutputSize; row++)
        {
            var rowOutput = true;
                
            for (var col = 0; col < _inputSize; col++)
            {
                if (_truthTable[row][col] ? _inputs[col].Value : !_inputs[col].Value) continue;
                    
                rowOutput = false;
                break;
            }

            _outputs[row].Value = rowOutput;

            if (!_outputs[row].Value) continue;
                
            EnabledIndex = row;
            break;
        }
    }

    public void SetInputWire(int index, IWire<bool> wire)
    {
        if (_inputs.TryGetValue(index, out var oldWire))
        {
            oldWire.ValueChanged -= InputsChanged;
        }

        _inputs[index] = wire;
        _inputs[index].ValueChanged += InputsChanged;
    }

    public void SetInputWireValue(int index, bool value)
    {
        if (_inputSize <= index)
        {
            throw new ArgumentException($"{index} is out of range of {_inputSize}");
        }
        
        _inputs[index].Value = value;
    }

    public IWire<bool> GetInputWire(int index)
    {
        return _inputs[index];
    }

    public void SetOutputWire(int index, IWire<bool> wire)
    {
        if (OutputSize <= index)
        {
            throw new ArgumentException($"{index} is out of range of {OutputSize}");
        }
        
        _outputs[index] = wire;
    }

    public bool GetOutputWireValue(int index)
    {
        return _outputs[index].Value;
    }
    
    private bool[][] GenerateCombinations()
    {
        var output = new bool[OutputSize][];

        for (var i = 0; i < OutputSize; i++)
        {
            output[i] = i.ToBinaryBools(_inputSize);
        }

        return output;
    }

    public override string ToString()
    {
        return $"{string.Join(" ", _inputs.Values.Reverse())} | {string.Join(" ", _outputs.Values)}";
    }
}