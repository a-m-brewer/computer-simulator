namespace ComputerSimulator.Assembler;

internal abstract record Statement(SourceLocation Location);

internal sealed record LabelStatement(SourceLocation Location, string Name) : Statement(Location);

internal sealed record InstructionStatement(SourceLocation Location, string Mnemonic, IReadOnlyList<Operand> Operands) : Statement(Location);

internal sealed record DirectiveStatement(SourceLocation Location, string Name, IReadOnlyList<Operand> Operands) : Statement(Location);
