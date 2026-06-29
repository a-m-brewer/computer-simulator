namespace ComputerSimulator.Assembler;

internal sealed class AssemblySourceLoader
{
    private readonly List<AssemblyDiagnostic> _diagnostics;
    private readonly HashSet<string> _includeStack = new(StringComparer.Ordinal);

    public AssemblySourceLoader(List<AssemblyDiagnostic> diagnostics)
    {
        _diagnostics = diagnostics;
    }

    public IReadOnlyList<Statement> Load(string path)
    {
        var fullPath = Path.GetFullPath(path);
        return LoadFile(fullPath);
    }

    private IReadOnlyList<Statement> LoadFile(string path)
    {
        if (!_includeStack.Add(path))
        {
            _diagnostics.Add(new AssemblyDiagnostic(path, 1, 1, "Recursive include detected"));
            return [];
        }

        try
        {
            if (!File.Exists(path))
            {
                _diagnostics.Add(new AssemblyDiagnostic(path, 1, 1, "Source file does not exist"));
                return [];
            }

            var parser = new AssemblyParser(path, _diagnostics);
            var statements = parser.Parse(File.ReadAllLines(path));
            var expanded = new List<Statement>();
            foreach (var statement in statements)
            {
                if (TryGetInclude(statement, out var includePath))
                {
                    expanded.AddRange(LoadFile(Path.GetFullPath(Path.Combine(Path.GetDirectoryName(path) ?? string.Empty, includePath))));
                    continue;
                }

                expanded.Add(statement);
            }

            return expanded;
        }
        finally
        {
            _includeStack.Remove(path);
        }
    }

    private bool TryGetInclude(Statement statement, out string path)
    {
        path = string.Empty;
        if (statement is not DirectiveStatement { Name: ".include", Operands.Count: 1 } directive
            || directive.Operands[0] is not StringOperand stringOperand)
        {
            return false;
        }

        path = stringOperand.Value;
        return true;
    }
}
