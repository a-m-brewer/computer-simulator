using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Circuits;

public interface IDecoder : ICircuit
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
        IComponentFactory componentFactory,
        IWireFactory wireFactory) : base(componentFactory, wireFactory)
    {
        Inputs = inputs;
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

    public void Update()
    {
        // Every output must be driven each update. Breaking out as soon as the matching row is found
        // would leave higher-index outputs holding stale values from a previous decode, which causes
        // spurious register enables/sets across consecutive instructions.
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

            if (rowOutput)
            {
                EnabledIndex = row;
            }
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