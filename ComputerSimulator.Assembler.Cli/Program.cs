using ComputerSimulator.Assembler;

namespace ComputerSimulator.Assembler.Cli;

public static class Program
{
    public static int Main(string[] args)
    {
        var parsed = ParseArgs(args);
        if (!parsed.Success)
        {
            if (parsed.Error.Length > 0)
            {
                Console.Error.WriteLine(parsed.Error);
            }

            PrintUsage();
            return 2;
        }

        var assembler = new ScottAssembler();
        var result = assembler.AssembleFile(parsed.InputPath!, parsed.Options);
        if (!result.Success)
        {
            foreach (var diagnostic in result.Diagnostics)
            {
                Console.Error.WriteLine(diagnostic);
            }

            return 1;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(parsed.OutputPath!)) ?? ".");
        File.WriteAllBytes(parsed.OutputPath!, result.Bytes);
        return 0;
    }

private static ParsedArgs ParseArgs(string[] args)
{
    if (args.Length == 0)
    {
        return ParsedArgs.Fail("No input file specified");
    }

    var options = new AssemblerOptions();
    string? inputPath = null;
    string? outputPath = null;

    for (var i = 0; i < args.Length; i++)
    {
        var arg = args[i];
        if (arg is "-h" or "--help")
        {
            return ParsedArgs.Fail(string.Empty);
        }

        if (arg is "-o" or "--output")
        {
            if (++i >= args.Length)
            {
                return ParsedArgs.Fail("Missing output path after -o");
            }

            outputPath = args[i];
            continue;
        }

        if (arg == "-D")
        {
            if (++i >= args.Length)
            {
                return ParsedArgs.Fail("Missing define after -D");
            }

            if (!TryParseDefine(args[i], options, out var error))
            {
                return ParsedArgs.Fail(error);
            }

            continue;
        }

        if (arg.StartsWith("-D", StringComparison.Ordinal))
        {
            if (!TryParseDefine(arg[2..], options, out var error))
            {
                return ParsedArgs.Fail(error);
            }

            continue;
        }

        if (inputPath is not null)
        {
            return ParsedArgs.Fail($"Unexpected argument '{arg}'");
        }

        inputPath = arg;
    }

    if (inputPath is null)
    {
        return ParsedArgs.Fail("No input file specified");
    }

    outputPath ??= Path.ChangeExtension(inputPath, ".bin");
    return ParsedArgs.Ok(inputPath, outputPath, options);
}

private static bool TryParseDefine(string text, AssemblerOptions options, out string error)
{
    error = string.Empty;
    var equalsIndex = text.IndexOf('=', StringComparison.Ordinal);
    if (equalsIndex <= 0)
    {
        error = $"Define '{text}' must be NAME=value";
        return false;
    }

    var name = text[..equalsIndex];
    var valueText = text[(equalsIndex + 1)..];
    if (!TryParseInteger(valueText, out var value))
    {
        error = $"Define '{text}' has an invalid value";
        return false;
    }

    options.Defines[name] = value;
    return true;
}

private static bool TryParseInteger(string text, out int value)
{
    var negative = text.StartsWith("-", StringComparison.Ordinal);
    var unsigned = negative ? text[1..] : text;
    try
    {
        if (unsigned.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            value = Convert.ToInt32(unsigned[2..], 16);
        }
        else if (unsigned.StartsWith("0b", StringComparison.OrdinalIgnoreCase))
        {
            value = Convert.ToInt32(unsigned[2..], 2);
        }
        else
        {
            value = int.Parse(unsigned);
        }

        if (negative)
        {
            value = -value;
        }

        return true;
    }
    catch (FormatException)
    {
        value = 0;
        return false;
    }
    catch (OverflowException)
    {
        value = 0;
        return false;
    }
}

private static void PrintUsage()
{
    Console.Error.WriteLine("Usage: asm <program.asm> [-o program.bin] [-D NAME=value]");
}

private sealed record ParsedArgs(bool Success, string? InputPath, string? OutputPath, AssemblerOptions Options, string Error)
{
    public static ParsedArgs Ok(string inputPath, string outputPath, AssemblerOptions options) => new(true, inputPath, outputPath, options, string.Empty);

    public static ParsedArgs Fail(string error) => new(false, null, null, new AssemblerOptions(), error);
}
}
