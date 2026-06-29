using ComputerSimulator.Core.Enums;
using ComputerSimulator.Core.Instructions;

namespace ComputerSimulator.Assembler;

public sealed class ScottAssembler
{
    private const int MaxImageSize = 1 << 16;
    private const int MaxLayoutIterations = 16;

    public AssemblyResult AssembleFile(string path, AssemblerOptions? options = null)
    {
        var diagnostics = new List<AssemblyDiagnostic>();
        var loader = new AssemblySourceLoader(diagnostics);
        var statements = loader.Load(path);
        if (diagnostics.Count > 0)
        {
            return new AssemblyResult([], diagnostics);
        }

        return Assemble(statements, options ?? new AssemblerOptions(), diagnostics);
    }

    public AssemblyResult AssembleText(string source, string path = "<memory>", AssemblerOptions? options = null)
    {
        var diagnostics = new List<AssemblyDiagnostic>();
        var parser = new AssemblyParser(path, diagnostics);
        var statements = parser.Parse(source.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n'));
        if (diagnostics.Count > 0)
        {
            return new AssemblyResult([], diagnostics);
        }

        return Assemble(statements, options ?? new AssemblerOptions(), diagnostics);
    }

    public byte[] AssembleFileOrThrow(string path, AssemblerOptions? options = null)
    {
        var result = AssembleFile(path, options);
        if (!result.Success)
        {
            throw new AssemblyException(result.Diagnostics);
        }

        return result.Bytes;
    }

    public byte[] AssembleTextOrThrow(string source, string path = "<memory>", AssemblerOptions? options = null)
    {
        var result = AssembleText(source, path, options);
        if (!result.Success)
        {
            throw new AssemblyException(result.Diagnostics);
        }

        return result.Bytes;
    }

    private AssemblyResult Assemble(IReadOnlyList<Statement> statements, AssemblerOptions options, List<AssemblyDiagnostic> diagnostics)
    {
        var symbols = new Dictionary<string, int>(options.Defines, StringComparer.Ordinal);
        var previousLabels = new Dictionary<string, int>(StringComparer.Ordinal);
        var stable = false;

        for (var iteration = 0; iteration < MaxLayoutIterations; iteration++)
        {
            var layoutDiagnostics = new List<AssemblyDiagnostic>();
            var labels = Layout(statements, symbols, previousLabels, layoutDiagnostics);
            if (layoutDiagnostics.Count > 0)
            {
                diagnostics.AddRange(layoutDiagnostics);
                return new AssemblyResult([], diagnostics);
            }

            stable = LabelsEqual(previousLabels, labels);
            previousLabels = labels;
            if (stable)
            {
                break;
            }
        }

        if (!stable)
        {
            diagnostics.Add(new AssemblyDiagnostic("<assembly>", 1, 1, "Assembly layout did not stabilize"));
            return new AssemblyResult([], diagnostics);
        }

        foreach (var (name, value) in previousLabels)
        {
            symbols[name] = value;
        }

        ApplyEquDirectives(statements, symbols, diagnostics);
        if (diagnostics.Count > 0)
        {
            return new AssemblyResult([], diagnostics);
        }

        var bytes = Emit(statements, symbols, diagnostics);
        return new AssemblyResult(bytes, diagnostics);
    }

    private void ApplyEquDirectives(IReadOnlyList<Statement> statements, Dictionary<string, int> symbols, List<AssemblyDiagnostic> diagnostics)
    {
        foreach (var directive in statements.OfType<DirectiveStatement>().Where(directive => directive.Name == ".equ"))
        {
            if (TryGetEqu(directive, symbols, diagnostics, out var name, out var value))
            {
                symbols[name] = value;
            }
        }
    }

    private Dictionary<string, int> Layout(
        IReadOnlyList<Statement> statements,
        Dictionary<string, int> defines,
        Dictionary<string, int> previousLabels,
        List<AssemblyDiagnostic> diagnostics)
    {
        var symbols = new Dictionary<string, int>(defines, StringComparer.Ordinal);
        foreach (var (name, value) in previousLabels)
        {
            symbols[name] = value;
        }

        var labels = new Dictionary<string, int>(StringComparer.Ordinal);
        var address = 0;
        foreach (var statement in statements)
        {
            switch (statement)
            {
                case LabelStatement label:
                    if (!labels.TryAdd(label.Name, address))
                    {
                        AddDiagnostic(diagnostics, label.Location, $"Label '{label.Name}' is already defined");
                    }

                    symbols[label.Name] = address;
                    break;
                case DirectiveStatement directive:
                    address = LayoutDirective(directive, symbols, address, diagnostics);
                    break;
                case InstructionStatement instruction:
                    address += GetInstructionSize(instruction, symbols, diagnostics, allowUndefined: true);
                    break;
            }

            if (address > MaxImageSize)
            {
                AddDiagnostic(diagnostics, statement.Location, $"Program image exceeds {MaxImageSize} bytes");
                break;
            }
        }

        return labels;
    }

    private int LayoutDirective(
        DirectiveStatement directive,
        Dictionary<string, int> symbols,
        int address,
        List<AssemblyDiagnostic> diagnostics)
    {
        switch (directive.Name)
        {
            case ".equ":
                if (!TryGetEqu(directive, symbols, diagnostics, out var name, out var value))
                {
                    return address;
                }

                symbols[name] = value;
                return address;
            case ".org":
                if (!ExpectOperandCount(directive, 1, diagnostics) || !TryResolveValue(directive.Operands[0], symbols, diagnostics, out var org))
                {
                    return address;
                }

                if (org < address)
                {
                    AddDiagnostic(diagnostics, directive.Location, $".org address {org} is before the current address {address}");
                    return address;
                }

                return org;
            case ".byte":
                return address + directive.Operands.Count;
            case ".word":
                return address + (directive.Operands.Count * 2);
            case ".ascii":
            case ".asciz":
                if (!ExpectOperandCount(directive, 1, diagnostics) || directive.Operands[0] is not StringOperand text)
                {
                    AddDiagnostic(diagnostics, directive.Location, $"{directive.Name} expects one string operand");
                    return address;
                }

                return address + text.Value.Length + (directive.Name == ".asciz" ? 1 : 0);
            case ".incbin":
                if (!ExpectOperandCount(directive, 1, diagnostics) || directive.Operands[0] is not StringOperand path)
                {
                    AddDiagnostic(diagnostics, directive.Location, ".incbin expects one string operand");
                    return address;
                }

                var fullPath = ResolveRelativePath(directive.Location.Path, path.Value);
                if (!File.Exists(fullPath))
                {
                    AddDiagnostic(diagnostics, directive.Location, $"Binary include '{path.Value}' does not exist");
                    return address;
                }

                return address + checked((int)new FileInfo(fullPath).Length);
            case ".include":
                return address;
            default:
                AddDiagnostic(diagnostics, directive.Location, $"Unknown directive '{directive.Name}'");
                return address;
        }
    }

    private int GetInstructionSize(
        InstructionStatement instruction,
        Dictionary<string, int> symbols,
        List<AssemblyDiagnostic> diagnostics,
        bool allowUndefined)
    {
        return instruction.Mnemonic switch
        {
            "DATA" or "JMP" => 2,
            "LD" or "ST" or "ADD" or "SHR" or "SHL" or "NOT" or "AND" or "OR" or "XOR" or "CMP" or "CLF" or "JMPR" or "IN" or "OUT" => 1,
            "JC" or "JA" or "JE" or "JZ" or "JCA" or "JCE" or "JCZ" or "JAE" or "JAZ" or "JEZ" or "JCAE" or "JCAZ" or "JCEZ" or "JAEZ" or "JCAEZ" => 2,
            "LDI" => GetLdiSize(instruction, symbols, diagnostics, allowUndefined),
            "MOV" => 3,
            "JMP16" => GetJmp16Size(instruction, symbols, diagnostics, allowUndefined),
            "HALT" => instruction.Operands.Count == 0 ? 2 : GetHaltLongSize(instruction, symbols, diagnostics, allowUndefined),
            _ => UnknownInstruction(instruction, diagnostics)
        };
    }

    private int GetLdiSize(InstructionStatement instruction, Dictionary<string, int> symbols, List<AssemblyDiagnostic> diagnostics, bool allowUndefined)
    {
        if (instruction.Operands.Count is not 2 and not 3)
        {
            AddDiagnostic(diagnostics, instruction.Location, "LDI expects Rdst, value[, Rscratch]");
            return 0;
        }

        if (instruction.Operands[0] is not RegisterOperand || instruction.Operands[1] is not ValueOperand valueOperand)
        {
            AddDiagnostic(diagnostics, instruction.Location, "LDI expects Rdst, value[, Rscratch]");
            return 0;
        }

        if (!TryResolveValue(valueOperand, symbols, diagnostics, out var value, allowUndefined))
        {
            return instruction.Operands.Count == 3 ? 13 : 2;
        }

        return NormalizeWord(value) <= 0xFF ? 2 : 13;
    }

    private int GetJmp16Size(InstructionStatement instruction, Dictionary<string, int> symbols, List<AssemblyDiagnostic> diagnostics, bool allowUndefined)
    {
        if (instruction.Operands.Count != 3)
        {
            AddDiagnostic(diagnostics, instruction.Location, "JMP16 expects label, Raddr, Rtmp");
            return 0;
        }

        if (instruction.Operands[0] is not ValueOperand || instruction.Operands[1] is not RegisterOperand || instruction.Operands[2] is not RegisterOperand)
        {
            AddDiagnostic(diagnostics, instruction.Location, "JMP16 expects label, Raddr, Rtmp");
            return 0;
        }

        return GetLdiSize(new InstructionStatement(instruction.Location, "LDI", [instruction.Operands[1], instruction.Operands[0], instruction.Operands[2]]), symbols, diagnostics, allowUndefined) + 1;
    }

    private int GetHaltLongSize(InstructionStatement instruction, Dictionary<string, int> symbols, List<AssemblyDiagnostic> diagnostics, bool allowUndefined)
    {
        if (instruction.Operands.Count != 2)
        {
            AddDiagnostic(diagnostics, instruction.Location, "HALT expects no operands or Raddr, Rtmp");
            return 0;
        }

        return 14;
    }

    private byte[] Emit(IReadOnlyList<Statement> statements, Dictionary<string, int> symbols, List<AssemblyDiagnostic> diagnostics)
    {
        var bytes = new List<byte>();
        foreach (var statement in statements)
        {
            switch (statement)
            {
                case DirectiveStatement directive:
                    EmitDirective(directive, symbols, bytes, diagnostics);
                    break;
                case InstructionStatement instruction:
                    EmitInstruction(instruction, symbols, bytes, diagnostics);
                    break;
            }
        }

        if (bytes.Count > MaxImageSize)
        {
            diagnostics.Add(new AssemblyDiagnostic("<assembly>", 1, 1, $"Program image exceeds {MaxImageSize} bytes"));
            return [];
        }

        return diagnostics.Count == 0 ? bytes.ToArray() : [];
    }

    private void EmitDirective(DirectiveStatement directive, Dictionary<string, int> symbols, List<byte> bytes, List<AssemblyDiagnostic> diagnostics)
    {
        switch (directive.Name)
        {
            case ".equ":
            case ".include":
                return;
            case ".org":
                if (!ExpectOperandCount(directive, 1, diagnostics) || !TryResolveValue(directive.Operands[0], symbols, diagnostics, out var org))
                {
                    return;
                }

                while (bytes.Count < org)
                {
                    bytes.Add(0);
                }

                return;
            case ".byte":
                foreach (var operand in directive.Operands)
                {
                    if (!TryResolveValue(operand, symbols, diagnostics, out var value) || !ValidateByte(value, directive.Location, diagnostics))
                    {
                        continue;
                    }

                    bytes.Add((byte)value);
                }

                return;
            case ".word":
                foreach (var operand in directive.Operands)
                {
                    if (!TryResolveValue(operand, symbols, diagnostics, out var value) || !ValidateWord(value, directive.Location, diagnostics))
                    {
                        continue;
                    }

                    bytes.Add((byte)(value & 0xFF));
                    bytes.Add((byte)((value >> 8) & 0xFF));
                }

                return;
            case ".ascii":
            case ".asciz":
                if (!ExpectOperandCount(directive, 1, diagnostics) || directive.Operands[0] is not StringOperand text)
                {
                    AddDiagnostic(diagnostics, directive.Location, $"{directive.Name} expects one string operand");
                    return;
                }

                foreach (var ch in text.Value)
                {
                    if (ch > 0xFF)
                    {
                        AddDiagnostic(diagnostics, directive.Location, "String contains a character that does not fit in one byte");
                        return;
                    }

                    bytes.Add((byte)ch);
                }

                if (directive.Name == ".asciz")
                {
                    bytes.Add(0);
                }

                return;
            case ".incbin":
                if (!ExpectOperandCount(directive, 1, diagnostics) || directive.Operands[0] is not StringOperand path)
                {
                    AddDiagnostic(diagnostics, directive.Location, ".incbin expects one string operand");
                    return;
                }

                bytes.AddRange(File.ReadAllBytes(ResolveRelativePath(directive.Location.Path, path.Value)));
                return;
        }
    }

    private void EmitInstruction(InstructionStatement instruction, Dictionary<string, int> symbols, List<byte> bytes, List<AssemblyDiagnostic> diagnostics)
    {
        switch (instruction.Mnemonic)
        {
            case "DATA":
                EmitData(instruction, symbols, bytes, diagnostics);
                return;
            case "LD":
                EmitMemoryInstruction(instruction, isLoad: true, bytes, diagnostics);
                return;
            case "ST":
                EmitMemoryInstruction(instruction, isLoad: false, bytes, diagnostics);
                return;
            case "ADD":
            case "SHR":
            case "SHL":
            case "NOT":
            case "AND":
            case "OR":
            case "XOR":
            case "CMP":
                EmitAlu(instruction, bytes, diagnostics);
                return;
            case "CLF":
                ExpectOperandCount(instruction, 0, diagnostics);
                bytes.Add((byte)InstructionSet.Clf);
                return;
            case "JMP":
                EmitJump(instruction, symbols, bytes, diagnostics);
                return;
            case "JMPR":
                if (!TryGetRegisterInstruction(instruction, diagnostics, out var register))
                {
                    return;
                }

                bytes.Add((byte)InstructionSet.Jmpr(register));
                return;
            case "IN":
            case "OUT":
                EmitIo(instruction, bytes, diagnostics);
                return;
            case "LDI":
                EmitLdi(instruction, symbols, bytes, diagnostics);
                return;
            case "MOV":
                EmitMov(instruction, bytes, diagnostics);
                return;
            case "JMP16":
                EmitJmp16(instruction, symbols, bytes, diagnostics);
                return;
            case "HALT":
                EmitHalt(instruction, bytes, diagnostics);
                return;
        }

        if (TryParseJumpCondition(instruction.Mnemonic, out var condition))
        {
            EmitJumpIf(instruction, condition, symbols, bytes, diagnostics);
        }
    }

    private void EmitData(InstructionStatement instruction, Dictionary<string, int> symbols, List<byte> bytes, List<AssemblyDiagnostic> diagnostics)
    {
        if (!ExpectOperandCount(instruction, 2, diagnostics)
            || instruction.Operands[0] is not RegisterOperand register
            || !TryResolveValue(instruction.Operands[1], symbols, diagnostics, out var value)
            || !ValidateByte(value, instruction.Location, diagnostics))
        {
            return;
        }

        bytes.Add((byte)InstructionSet.Data(register.Register));
        bytes.Add((byte)value);
    }

    private void EmitMemoryInstruction(InstructionStatement instruction, bool isLoad, List<byte> bytes, List<AssemblyDiagnostic> diagnostics)
    {
        if (!ExpectOperandCount(instruction, 2, diagnostics))
        {
            return;
        }

        if (isLoad)
        {
            if (instruction.Operands[0] is RegisterOperand dst && instruction.Operands[1] is MemoryRegisterOperand address)
            {
                bytes.Add((byte)InstructionSet.Ld(address.Register, dst.Register));
                return;
            }

            AddDiagnostic(diagnostics, instruction.Location, "LD expects Rdst, [Raddr]");
            return;
        }

        if (instruction.Operands[0] is MemoryRegisterOperand storeAddress && instruction.Operands[1] is RegisterOperand src)
        {
            bytes.Add((byte)InstructionSet.St(storeAddress.Register, src.Register));
            return;
        }

        AddDiagnostic(diagnostics, instruction.Location, "ST expects [Raddr], Rsrc");
    }

    private void EmitAlu(InstructionStatement instruction, List<byte> bytes, List<AssemblyDiagnostic> diagnostics)
    {
        if (instruction.Operands.Count is not 1 and not 2)
        {
            AddDiagnostic(diagnostics, instruction.Location, $"{instruction.Mnemonic} expects Rdst[, Rsrc]");
            return;
        }

        if (instruction.Mnemonic == "CMP")
        {
            if (instruction.Operands.Count != 2
                || instruction.Operands[0] is not RegisterOperand compareA
                || instruction.Operands[1] is not RegisterOperand compareB)
            {
                AddDiagnostic(diagnostics, instruction.Location, "CMP expects Ra, Rb");
                return;
            }

            bytes.Add((byte)InstructionSet.Cmp(compareA.Register, compareB.Register));
            return;
        }

        if (instruction.Operands[0] is not RegisterOperand dst)
        {
            AddDiagnostic(diagnostics, instruction.Location, $"{instruction.Mnemonic} expects a destination register");
            return;
        }

        var src = dst.Register;
        if (instruction.Operands.Count == 2)
        {
            if (instruction.Operands[1] is not RegisterOperand source)
            {
                AddDiagnostic(diagnostics, instruction.Location, $"{instruction.Mnemonic} expects a source register");
                return;
            }

            src = source.Register;
        }

        var encoded = instruction.Mnemonic switch
        {
            "ADD" => InstructionSet.Add(src, dst.Register),
            "SHR" => InstructionSet.Shr(src, dst.Register),
            "SHL" => InstructionSet.Shl(src, dst.Register),
            "NOT" => InstructionSet.Not(src, dst.Register),
            "AND" => InstructionSet.And(src, dst.Register),
            "OR" => InstructionSet.Or(src, dst.Register),
            "XOR" => InstructionSet.XOr(src, dst.Register),
            _ => 0
        };

        bytes.Add((byte)encoded);
    }

    private void EmitJump(InstructionStatement instruction, Dictionary<string, int> symbols, List<byte> bytes, List<AssemblyDiagnostic> diagnostics)
    {
        if (!ExpectOperandCount(instruction, 1, diagnostics)
            || !TryResolveValue(instruction.Operands[0], symbols, diagnostics, out var address)
            || !ValidateByte(address, instruction.Location, diagnostics))
        {
            return;
        }

        bytes.Add((byte)InstructionSet.Jmp());
        bytes.Add((byte)address);
    }

    private void EmitJumpIf(InstructionStatement instruction, JumpCondition condition, Dictionary<string, int> symbols, List<byte> bytes, List<AssemblyDiagnostic> diagnostics)
    {
        if (!ExpectOperandCount(instruction, 1, diagnostics)
            || !TryResolveValue(instruction.Operands[0], symbols, diagnostics, out var address)
            || !ValidateByte(address, instruction.Location, diagnostics))
        {
            return;
        }

        bytes.Add((byte)InstructionSet.JumpIf(condition));
        bytes.Add((byte)address);
    }

    private void EmitIo(InstructionStatement instruction, List<byte> bytes, List<AssemblyDiagnostic> diagnostics)
    {
        if (!ExpectOperandCount(instruction, 2, diagnostics) || instruction.Operands[1] is not RegisterOperand register)
        {
            AddDiagnostic(diagnostics, instruction.Location, $"{instruction.Mnemonic} expects DATA|ADDR, Rn");
            return;
        }

        if (!TryParseDataAddress(instruction.Operands[0], out var dataAddress))
        {
            AddDiagnostic(diagnostics, instruction.Location, $"{instruction.Mnemonic} expects DATA or ADDR as the first operand");
            return;
        }

        bytes.Add((byte)(instruction.Mnemonic == "IN"
            ? InstructionSet.In(dataAddress, register.Register)
            : InstructionSet.Out(dataAddress, register.Register)));
    }

    private void EmitLdi(InstructionStatement instruction, Dictionary<string, int> symbols, List<byte> bytes, List<AssemblyDiagnostic> diagnostics)
    {
        if (instruction.Operands.Count is not 2 and not 3
            || instruction.Operands[0] is not RegisterOperand dst
            || instruction.Operands[1] is not ValueOperand valueOperand
            || !TryResolveValue(valueOperand, symbols, diagnostics, out var value)
            || !ValidateWord(value, instruction.Location, diagnostics))
        {
            AddDiagnostic(diagnostics, instruction.Location, "LDI expects Rdst, value[, Rscratch]");
            return;
        }

        value = NormalizeWord(value);
        if (value <= 0xFF)
        {
            bytes.Add((byte)InstructionSet.Data(dst.Register));
            bytes.Add((byte)value);
            return;
        }

        if (instruction.Operands.Count != 3 || instruction.Operands[2] is not RegisterOperand scratch)
        {
            AddDiagnostic(diagnostics, instruction.Location, "LDI values above 0xFF require an explicit scratch register");
            return;
        }

        EmitLoadWord(dst.Register, value, scratch.Register, bytes);
    }

    private void EmitMov(InstructionStatement instruction, List<byte> bytes, List<AssemblyDiagnostic> diagnostics)
    {
        if (!ExpectOperandCount(instruction, 2, diagnostics)
            || instruction.Operands[0] is not RegisterOperand dst
            || instruction.Operands[1] is not RegisterOperand src)
        {
            AddDiagnostic(diagnostics, instruction.Location, "MOV expects Rdst, Rsrc");
            return;
        }

        bytes.Add((byte)InstructionSet.Data(dst.Register));
        bytes.Add(0);
        bytes.Add((byte)InstructionSet.Add(src.Register, dst.Register));
    }

    private void EmitJmp16(InstructionStatement instruction, Dictionary<string, int> symbols, List<byte> bytes, List<AssemblyDiagnostic> diagnostics)
    {
        if (instruction.Operands.Count != 3
            || instruction.Operands[0] is not ValueOperand valueOperand
            || instruction.Operands[1] is not RegisterOperand addressRegister
            || instruction.Operands[2] is not RegisterOperand scratch
            || !TryResolveValue(valueOperand, symbols, diagnostics, out var address)
            || !ValidateWord(address, instruction.Location, diagnostics))
        {
            AddDiagnostic(diagnostics, instruction.Location, "JMP16 expects label, Raddr, Rtmp");
            return;
        }

        EmitLdi(new InstructionStatement(instruction.Location, "LDI", [addressRegister, valueOperand, scratch]), symbols, bytes, diagnostics);
        bytes.Add((byte)InstructionSet.Jmpr(addressRegister.Register));
    }

    private void EmitHalt(InstructionStatement instruction, List<byte> bytes, List<AssemblyDiagnostic> diagnostics)
    {
        var address = bytes.Count;
        if (instruction.Operands.Count == 0)
        {
            if (!ValidateByte(address, instruction.Location, diagnostics))
            {
                return;
            }

            bytes.Add((byte)InstructionSet.Jmp());
            bytes.Add((byte)address);
            return;
        }

        if (instruction.Operands.Count != 2 || instruction.Operands[0] is not RegisterOperand addressRegister || instruction.Operands[1] is not RegisterOperand scratch)
        {
            AddDiagnostic(diagnostics, instruction.Location, "HALT expects no operands or Raddr, Rtmp");
            return;
        }

        EmitLoadWord(addressRegister.Register, address, scratch.Register, bytes);
        bytes.Add((byte)InstructionSet.Jmpr(addressRegister.Register));
    }

    private static void EmitLoadWord(int register, int value, int scratchRegister, List<byte> bytes)
    {
        bytes.Add((byte)InstructionSet.Data(register));
        bytes.Add((byte)(value & 0xFF));
        bytes.Add((byte)InstructionSet.Data(scratchRegister));
        bytes.Add((byte)((value >> 8) & 0xFF));
        for (var i = 0; i < 8; i++)
        {
            bytes.Add((byte)InstructionSet.Shl(scratchRegister, scratchRegister));
        }

        bytes.Add((byte)InstructionSet.Add(scratchRegister, register));
    }

    private static bool TryParseJumpCondition(string mnemonic, out JumpCondition condition)
    {
        condition = JumpCondition.None;
        if (mnemonic.Length < 2 || mnemonic[0] != 'J' || mnemonic == "JMP" || mnemonic == "JMPR" || mnemonic == "JMP16")
        {
            return false;
        }

        foreach (var ch in mnemonic[1..])
        {
            condition |= ch switch
            {
                'C' => JumpCondition.Carry,
                'A' => JumpCondition.Above,
                'E' => JumpCondition.Equal,
                'Z' => JumpCondition.Zero,
                _ => JumpCondition.None
            };
        }

        return condition != JumpCondition.None && mnemonic[1..].All(ch => ch is 'C' or 'A' or 'E' or 'Z');
    }

    private static bool TryParseDataAddress(Operand operand, out DataAddress dataAddress)
    {
        dataAddress = DataAddress.Data;
        if (operand is not ValueOperand { Value.Symbol: { } symbol })
        {
            return false;
        }

        if (symbol.Equals("DATA", StringComparison.OrdinalIgnoreCase))
        {
            dataAddress = DataAddress.Data;
            return true;
        }

        if (symbol.Equals("ADDR", StringComparison.OrdinalIgnoreCase) || symbol.Equals("ADDRESS", StringComparison.OrdinalIgnoreCase))
        {
            dataAddress = DataAddress.Address;
            return true;
        }

        return false;
    }

    private static bool TryGetRegisterInstruction(InstructionStatement instruction, List<AssemblyDiagnostic> diagnostics, out int register)
    {
        register = 0;
        if (!ExpectOperandCount(instruction, 1, diagnostics) || instruction.Operands[0] is not RegisterOperand registerOperand)
        {
            AddDiagnostic(diagnostics, instruction.Location, $"{instruction.Mnemonic} expects one register operand");
            return false;
        }

        register = registerOperand.Register;
        return true;
    }

    private static bool TryResolveValue(Operand operand, Dictionary<string, int> symbols, List<AssemblyDiagnostic> diagnostics, out int value, bool allowUndefined = false)
    {
        value = 0;
        if (operand is not ValueOperand valueOperand)
        {
            diagnostics.Add(new AssemblyDiagnostic("<assembly>", 1, 1, "Expected a value operand"));
            return false;
        }

        return TryResolveValue(valueOperand, symbols, diagnostics, out value, allowUndefined);
    }

    private static bool TryResolveValue(ValueOperand operand, Dictionary<string, int> symbols, List<AssemblyDiagnostic> diagnostics, out int value, bool allowUndefined = false)
    {
        if (operand.Value.Literal is { } literal)
        {
            value = literal;
            return true;
        }

        if (operand.Value.Symbol is { } symbol && symbols.TryGetValue(symbol, out value))
        {
            return true;
        }

        value = 0;
        if (!allowUndefined && operand.Value.Symbol is { } missing)
        {
            diagnostics.Add(new AssemblyDiagnostic("<assembly>", 1, 1, $"Symbol '{missing}' is not defined"));
        }

        return false;
    }

    private static bool TryGetEqu(DirectiveStatement directive, Dictionary<string, int> symbols, List<AssemblyDiagnostic> diagnostics, out string name, out int value)
    {
        name = string.Empty;
        value = 0;
        if (!ExpectOperandCount(directive, 2, diagnostics)
            || directive.Operands[0] is not ValueOperand { Value.Symbol: { } symbol }
            || !TryResolveValue(directive.Operands[1], symbols, diagnostics, out value))
        {
            AddDiagnostic(diagnostics, directive.Location, ".equ expects NAME, value");
            return false;
        }

        name = symbol;
        return true;
    }

    private static bool ExpectOperandCount(InstructionStatement instruction, int expected, List<AssemblyDiagnostic> diagnostics)
    {
        if (instruction.Operands.Count == expected)
        {
            return true;
        }

        AddDiagnostic(diagnostics, instruction.Location, $"{instruction.Mnemonic} expects {expected} operand(s)");
        return false;
    }

    private static bool ExpectOperandCount(DirectiveStatement directive, int expected, List<AssemblyDiagnostic> diagnostics)
    {
        if (directive.Operands.Count == expected)
        {
            return true;
        }

        AddDiagnostic(diagnostics, directive.Location, $"{directive.Name} expects {expected} operand(s)");
        return false;
    }

    private static bool ValidateByte(int value, SourceLocation location, List<AssemblyDiagnostic> diagnostics)
    {
        if (value is >= 0 and <= 0xFF)
        {
            return true;
        }

        AddDiagnostic(diagnostics, location, $"Value {value} must fit in one byte");
        return false;
    }

    private static bool ValidateWord(int value, SourceLocation location, List<AssemblyDiagnostic> diagnostics)
    {
        if (value is >= -0x8000 and <= 0xFFFF)
        {
            return true;
        }

        AddDiagnostic(diagnostics, location, $"Value {value} must fit in one word");
        return false;
    }

    private static int NormalizeWord(int value) => value < 0 ? value & 0xFFFF : value;

    private static bool LabelsEqual(Dictionary<string, int> left, Dictionary<string, int> right)
    {
        return left.Count == right.Count && left.All(pair => right.TryGetValue(pair.Key, out var value) && value == pair.Value);
    }

    private static string ResolveRelativePath(string sourcePath, string relativePath)
    {
        return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(sourcePath) ?? string.Empty, relativePath));
    }

    private static int UnknownInstruction(InstructionStatement instruction, List<AssemblyDiagnostic> diagnostics)
    {
        AddDiagnostic(diagnostics, instruction.Location, $"Unknown instruction '{instruction.Mnemonic}'");
        return 0;
    }

    private static void AddDiagnostic(List<AssemblyDiagnostic> diagnostics, SourceLocation location, string message)
    {
        diagnostics.Add(new AssemblyDiagnostic(location.Path, location.Line, location.Column, message));
    }
}
