using ComputerSimulator.Core.Exceptions;

namespace ComputerSimulator.Core.Programs;

public static class BuiltInProgramImages
{
    private const string ProgramDirectory = "programs";
    private const string BinaryDirectory = "bin";

    public static string GetPath(BuiltInProgram program)
    {
        var fileName = program switch
        {
            BuiltInProgram.DisplayPattern => "display-pattern.bin",
            BuiltInProgram.HelloWorld => "hello-world.bin",
            BuiltInProgram.Echo => "echo.bin",
            _ => throw new ComputerSimulatorException($"Unknown built-in program '{program}'")
        };

        return Path.Combine(AppContext.BaseDirectory, ProgramDirectory, BinaryDirectory, fileName);
    }
}
