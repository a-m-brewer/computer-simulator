using System.Globalization;

namespace ComputerSimulator.Assembler;

internal sealed class AssemblyParser
{
    private readonly string _path;
    private readonly List<AssemblyDiagnostic> _diagnostics;

    public AssemblyParser(string path, List<AssemblyDiagnostic> diagnostics)
    {
        _path = path;
        _diagnostics = diagnostics;
    }

    public IReadOnlyList<Statement> Parse(IReadOnlyList<string> lines)
    {
        var statements = new List<Statement>();
        for (var i = 0; i < lines.Count; i++)
        {
            ParseLine(lines[i], i + 1, statements);
        }

        return statements;
    }

    private void ParseLine(string rawLine, int lineNumber, List<Statement> statements)
    {
        var line = StripComment(rawLine).Trim();
        if (line.Length == 0)
        {
            return;
        }

        while (true)
        {
            var colonIndex = FindLabelColon(line);
            if (colonIndex <= 0)
            {
                break;
            }

            var label = line[..colonIndex].Trim();
            if (!IsIdentifier(label))
            {
                AddDiagnostic(lineNumber, 1, $"'{label}' is not a valid label");
                return;
            }

            statements.Add(new LabelStatement(new SourceLocation(_path, lineNumber, 1), label));
            line = line[(colonIndex + 1)..].Trim();
            if (line.Length == 0)
            {
                return;
            }
        }

        var firstWhitespace = line.IndexOfAny([' ', '\t']);
        var head = firstWhitespace < 0 ? line : line[..firstWhitespace];
        var operandText = firstWhitespace < 0 ? string.Empty : line[(firstWhitespace + 1)..].Trim();
        var operands = ParseOperands(operandText, lineNumber);

        if (head.StartsWith(".", StringComparison.Ordinal))
        {
            statements.Add(new DirectiveStatement(new SourceLocation(_path, lineNumber, 1), head.ToLowerInvariant(), operands));
            return;
        }

        statements.Add(new InstructionStatement(new SourceLocation(_path, lineNumber, 1), head.ToUpperInvariant(), operands));
    }

    private IReadOnlyList<Operand> ParseOperands(string operandText, int lineNumber)
    {
        if (operandText.Length == 0)
        {
            return [];
        }

        var parts = SplitOperands(operandText, lineNumber);
        var operands = new List<Operand>(parts.Count);
        foreach (var part in parts)
        {
            operands.Add(ParseOperand(part.Trim(), lineNumber));
        }

        return operands;
    }

    private List<string> SplitOperands(string operandText, int lineNumber)
    {
        var parts = new List<string>();
        var start = 0;
        var inString = false;
        var inChar = false;
        for (var i = 0; i < operandText.Length; i++)
        {
            var ch = operandText[i];
            if (ch == '"' && !inChar && !IsEscaped(operandText, i))
            {
                inString = !inString;
            }
            else if (ch == '\'' && !inString && !IsEscaped(operandText, i))
            {
                inChar = !inChar;
            }
            else if (ch == ',' && !inString && !inChar)
            {
                parts.Add(operandText[start..i]);
                start = i + 1;
            }
        }

        if (inString || inChar)
        {
            AddDiagnostic(lineNumber, 1, "Unterminated string or character literal");
        }

        parts.Add(operandText[start..]);
        return parts;
    }

    private Operand ParseOperand(string text, int lineNumber)
    {
        if (text.Length == 0)
        {
            AddDiagnostic(lineNumber, 1, "Empty operand");
            return new ValueOperand(ValueExpression.FromLiteral(0));
        }

        if (text.StartsWith('"') && text.EndsWith('"') && text.Length >= 2)
        {
            return new StringOperand(Unescape(text[1..^1], lineNumber));
        }

        if (TryParseRegister(text, out var register))
        {
            return new RegisterOperand(register);
        }

        if (text.StartsWith('[') && text.EndsWith(']') && TryParseRegister(text[1..^1].Trim(), out register))
        {
            return new MemoryRegisterOperand(register);
        }

        if (TryParseNumberOrChar(text, lineNumber, out var value))
        {
            return new ValueOperand(ValueExpression.FromLiteral(value));
        }

        if (IsIdentifier(text))
        {
            return new ValueOperand(ValueExpression.FromSymbol(text));
        }

        AddDiagnostic(lineNumber, 1, $"Could not parse operand '{text}'");
        return new ValueOperand(ValueExpression.FromLiteral(0));
    }

    private static string StripComment(string line)
    {
        var inString = false;
        var inChar = false;
        for (var i = 0; i < line.Length; i++)
        {
            var ch = line[i];
            if (ch == '"' && !inChar && !IsEscaped(line, i))
            {
                inString = !inString;
            }
            else if (ch == '\'' && !inString && !IsEscaped(line, i))
            {
                inChar = !inChar;
            }
            else if (ch == ';' && !inString && !inChar)
            {
                return line[..i];
            }
        }

        return line;
    }

    private static int FindLabelColon(string line)
    {
        var colonIndex = line.IndexOf(':', StringComparison.Ordinal);
        if (colonIndex < 0)
        {
            return -1;
        }

        var before = line[..colonIndex].Trim();
        return before.Contains(' ') || before.Contains('\t') ? -1 : colonIndex;
    }

    private static bool TryParseRegister(string text, out int register)
    {
        register = 0;
        if (text.Length != 2 || char.ToUpperInvariant(text[0]) != 'R' || text[1] is < '0' or > '3')
        {
            return false;
        }

        register = text[1] - '0';
        return true;
    }

    private bool TryParseNumberOrChar(string text, int lineNumber, out int value)
    {
        value = 0;
        if (text.StartsWith('\'') && text.EndsWith('\'') && text.Length >= 3)
        {
            var contents = text[1..^1];
            var unescaped = Unescape(contents, lineNumber);
            if (unescaped.Length != 1)
            {
                AddDiagnostic(lineNumber, 1, "Character literal must contain exactly one character");
                return true;
            }

            value = unescaped[0];
            return true;
        }

        var negative = text.StartsWith("-", StringComparison.Ordinal);
        var unsigned = negative ? text[1..] : text;
        var numberStyles = NumberStyles.Integer;
        var radix = 10;
        if (unsigned.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            unsigned = unsigned[2..];
            numberStyles = NumberStyles.HexNumber;
            radix = 16;
        }
        else if (unsigned.StartsWith("0b", StringComparison.OrdinalIgnoreCase))
        {
            unsigned = unsigned[2..];
            radix = 2;
        }

        if (radix == 2)
        {
            if (unsigned.Length == 0 || unsigned.Any(ch => ch is not '0' and not '1'))
            {
                return false;
            }

            value = Convert.ToInt32(unsigned, 2);
            if (negative)
            {
                value = -value;
            }

            return true;
        }

        if (!int.TryParse(unsigned, numberStyles, CultureInfo.InvariantCulture, out value))
        {
            return false;
        }

        if (negative)
        {
            value = -value;
        }

        return true;
    }

    private string Unescape(string text, int lineNumber)
    {
        var chars = new List<char>();
        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] != '\\')
            {
                chars.Add(text[i]);
                continue;
            }

            if (++i >= text.Length)
            {
                AddDiagnostic(lineNumber, 1, "Invalid escape sequence");
                break;
            }

            chars.Add(text[i] switch
            {
                '0' => '\0',
                'n' => '\n',
                'r' => '\r',
                't' => '\t',
                '\\' => '\\',
                '\'' => '\'',
                '"' => '"',
                _ => text[i]
            });
        }

        return new string(chars.ToArray());
    }

    private static bool IsIdentifier(string text)
    {
        if (text.Length == 0 || !(char.IsLetter(text[0]) || text[0] == '_'))
        {
            return false;
        }

        return text.All(ch => char.IsLetterOrDigit(ch) || ch == '_');
    }

    private static bool IsEscaped(string text, int index)
    {
        var backslashes = 0;
        for (var i = index - 1; i >= 0 && text[i] == '\\'; i--)
        {
            backslashes++;
        }

        return backslashes % 2 == 1;
    }

    private void AddDiagnostic(int line, int column, string message)
    {
        _diagnostics.Add(new AssemblyDiagnostic(_path, line, column, message));
    }
}
