namespace ComputerSimulator.Assembler;

public sealed class AssemblerOptions
{
    public IDictionary<string, int> Defines { get; } = new Dictionary<string, int>(StringComparer.Ordinal);
}
