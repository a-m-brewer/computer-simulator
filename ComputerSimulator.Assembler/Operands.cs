namespace ComputerSimulator.Assembler;

internal abstract record Operand;

internal sealed record RegisterOperand(int Register) : Operand;

internal sealed record MemoryRegisterOperand(int Register) : Operand;

internal sealed record ValueOperand(ValueExpression Value) : Operand;

internal sealed record StringOperand(string Value) : Operand;

internal readonly record struct ValueExpression(int? Literal, string? Symbol)
{
    public static ValueExpression FromLiteral(int value) => new(value, null);

    public static ValueExpression FromSymbol(string symbol) => new(null, symbol);
}
