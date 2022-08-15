using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Circuits;

public interface IDecoder : IComponent2
{
    int EnabledIndex { get; }
    
    int OutputSize { get; }

    public IWireGroup<bool> Inputs { get; }

    public IWireGroup<bool> Outputs { get; }
}

public class Decoder : CircuitBase, IDecoder
{
    private readonly bool[][] _truthTable;

    public Decoder(
        IWireGroup<bool> inputs,
        IWireGroup<bool> outputs,
        IComponentFactory2 componentFactory,
        IWire2Factory2 wireFactory) : base(componentFactory, wireFactory)
    {
        Inputs = inputs.SubscribeToWireValuesChanged(InputsChanged);
        OutputSize = CalculateOutputSize(inputs.Count);
        Outputs = outputs;

        _truthTable = GenerateCombinations();
    }

    public static int CalculateOutputSize(int inputSize)
    {
        return (int) Math.Pow(2, inputSize);
    }

    public int EnabledIndex { get; private set; }

    public int OutputSize { get; }

    public IWireGroup<bool> Inputs { get; }

    public IWireGroup<bool> Outputs { get; }

    private void InputsChanged(object? sender, int index)
    {
        for (var row = 0; row < OutputSize; row++)
        {
            var rowOutput = true;
                
            for (var col = 0; col < Inputs.Count; col++)
            {
                if (_truthTable[row][col] ? Inputs[col].Value : !Inputs[col].Value) continue;
                    
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
            output[i] = i.ToBinaryBools(Inputs.Count);
        }

        return output;
    }
}