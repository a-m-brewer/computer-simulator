namespace ComputerSimulator.Assembler;

public sealed class AssemblyException : Exception
{
    public AssemblyException(IReadOnlyList<AssemblyDiagnostic> diagnostics)
        : base(diagnostics.Count == 0 ? "Assembly failed" : diagnostics[0].ToString())
    {
        Diagnostics = diagnostics;
    }

    public IReadOnlyList<AssemblyDiagnostic> Diagnostics { get; }
}
