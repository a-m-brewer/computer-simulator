namespace ComputerSimulator.Assembler;

public sealed record AssemblyDiagnostic(string Path, int Line, int Column, string Message)
{
    public override string ToString()
    {
        return $"{Path}({Line},{Column}): {Message}";
    }
}
