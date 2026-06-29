namespace ComputerSimulator.Assembler;

public sealed class AssemblyResult
{
    public AssemblyResult(byte[] bytes, IReadOnlyList<AssemblyDiagnostic> diagnostics)
    {
        Bytes = bytes;
        Diagnostics = diagnostics;
    }

    public byte[] Bytes { get; }

    public IReadOnlyList<AssemblyDiagnostic> Diagnostics { get; }

    public bool Success => Diagnostics.Count == 0;
}
