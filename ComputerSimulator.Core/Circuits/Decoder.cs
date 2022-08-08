using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Circuits;

public interface IDecoder : IComponent2
{
    /// <summary>
    /// Initialize the decode e.g passing 3 will make a 3x8 decoder and so on.
    /// </summary>
    /// <param name="inputSize">Size of inputs</param>
    public void Initialize(int inputSize);
    
    int EnabledIndex { get; }
    
    int OutputSize { get; }

    public IWireGroup<bool> Inputs { get; set; }

    public IWireGroup<bool> Outputs { get; set; }
}

public class Decoder : CircuitBase, IDecoder
{
    private bool[][] _truthTable = Array.Empty<bool[]>();
    private int _inputSize;
    private IWireGroup<bool> _inputs = DisconnectedWireGroup<bool>.Instance;

    public Decoder(
        IWire2Factory wireFactory) : base(wireFactory)
    {
    }

    public void Initialize(int inputSize)
    {
        _inputSize = inputSize;
        OutputSize = (int) Math.Pow(2, inputSize);

        _truthTable = GenerateCombinations();
    }

    public int EnabledIndex { get; private set; }

    public int OutputSize { get; private set; }

    public IWireGroup<bool> Inputs
    {
        get => _inputs;
        set
        {
            WireGroupHelper.ReSubscribeWireValuesChanged(_inputs, value, InputsChanged);
            _inputs = value;
        }
    }

    public IWireGroup<bool> Outputs { get; set; } = DisconnectedWireGroup<bool>.Instance;

    private void InputsChanged(object? sender, EventArgs eventArgs)
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

            Outputs[row].Value = rowOutput;

            if (!Outputs[row].Value) continue;
                
            EnabledIndex = row;
            break;
        }
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
        return $"{string.Join(" ", _inputs.Reverse())} | {string.Join(" ", Outputs.Select(v => v.Value))}";
    }
}