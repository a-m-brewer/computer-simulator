using ComputerSimulator.Core.Enums;
using ComputerSimulator.Core.Instructions;
using FluentAssertions;
using NUnit.Framework;

namespace ComputerSimulator.Assembler.Tests;

public class ScottAssemblerTests
{
    [Test]
    public void EmitsDestFirstAluInstructions()
    {
        var bytes = Assemble("ADD R3, R0");

        bytes.Should().Equal(InstructionSet.Add(0, 3));
    }

    [Test]
    public void EmitsCmpInWrittenOperandOrder()
    {
        var bytes = Assemble("CMP R3, R2");

        bytes.Should().Equal(InstructionSet.Cmp(3, 2));
    }

    [Test]
    public void EmitsMemoryInstructionsWithAddressRegisterInBrackets()
    {
        var bytes = Assemble("""
            LD R1, [R0]
            ST [R2], R3
            """);

        bytes.Should().Equal(
            InstructionSet.Ld(0, 1),
            InstructionSet.St(2, 3));
    }

    [Test]
    public void ResolvesCaseSensitiveLabels()
    {
        var result = new ScottAssembler().AssembleText("""
            Loop:
            JMP loop
            loop:
            HALT
            """);

        result.Success.Should().BeTrue();
        result.Bytes.Select(b => (int)b).Should().Equal(
            InstructionSet.Jmp(), 2,
            InstructionSet.Jmp(), 2);
    }

    [Test]
    public void OptimizesLdiWhenValueFitsInOneByte()
    {
        var bytes = Assemble("LDI R0, 0x7F, R3");

        bytes.Should().Equal(InstructionSet.Data(0), 0x7F);
    }

    [Test]
    public void ExpandsLdiForWordValues()
    {
        var bytes = Assemble("LDI R0, 0x1234, R3");

        bytes.Should().Equal([
            InstructionSet.Data(0), 0x34,
            InstructionSet.Data(3), 0x12,
            InstructionSet.Shl(3, 3),
            InstructionSet.Shl(3, 3),
            InstructionSet.Shl(3, 3),
            InstructionSet.Shl(3, 3),
            InstructionSet.Shl(3, 3),
            InstructionSet.Shl(3, 3),
            InstructionSet.Shl(3, 3),
            InstructionSet.Shl(3, 3),
            InstructionSet.Add(3, 0)
        ]);
    }

    [Test]
    public void RequiresScratchRegisterForLargeLdiValues()
    {
        var result = new ScottAssembler().AssembleText("LDI R0, 0x1234");

        result.Success.Should().BeFalse();
        result.Diagnostics.Should().Contain(diagnostic => diagnostic.Message.Contains("scratch", StringComparison.OrdinalIgnoreCase));
    }

    [Test]
    public void EmitsShortJumpsAndJumpConditions()
    {
        var bytes = Assemble("""
            CLF
            JE End
            JMP End
            End:
            HALT
            """);

        bytes.Should().Equal(
            InstructionSet.Clf,
            InstructionSet.JumpIf(JumpCondition.Equal), 5,
            InstructionSet.Jmp(), 5,
            InstructionSet.Jmp(), 5);
    }

    [Test]
    public void ErrorsWhenShortJumpTargetDoesNotFitInOneByte()
    {
        var result = new ScottAssembler().AssembleText("""
            JMP Far
            .org 0x0100
            Far:
            HALT
            """);

        result.Success.Should().BeFalse();
        result.Diagnostics.Should().Contain(diagnostic => diagnostic.Message.Contains("one byte", StringComparison.OrdinalIgnoreCase));
    }

    [Test]
    public void EmitsJmp16ThroughExplicitRegisters()
    {
        var bytes = Assemble("""
            JMP16 Far, R0, R3
            .org 0x0123
            Far:
            .byte 0
            """);

        bytes.Take(14).Should().Equal([
            InstructionSet.Data(0), 0x23,
            InstructionSet.Data(3), 0x01,
            InstructionSet.Shl(3, 3),
            InstructionSet.Shl(3, 3),
            InstructionSet.Shl(3, 3),
            InstructionSet.Shl(3, 3),
            InstructionSet.Shl(3, 3),
            InstructionSet.Shl(3, 3),
            InstructionSet.Shl(3, 3),
            InstructionSet.Shl(3, 3),
            InstructionSet.Add(3, 0),
            InstructionSet.Jmpr(0)
        ]);
    }

    [Test]
    public void SupportsOrgByteWordAndStrings()
    {
        var bytes = Assemble("""
            .org 4
            .byte 1, 'A'
            .word 0x1234
            .ascii "HI"
            .asciz "!"
            """);

        bytes.Should().Equal(0, 0, 0, 0, 1, 65, 0x34, 0x12, (byte)'H', (byte)'I', (byte)'!', 0);
    }

    [Test]
    public void IncludesBinaryFiles()
    {
        var directory = TestContext.CurrentContext.WorkDirectory;
        var path = Path.Combine(directory, $"asset-{Guid.NewGuid():N}.bin");
        try
        {
            File.WriteAllBytes(path, [0xAA, 0xBB]);

            var bytes = Assemble($".incbin \"{Path.GetFileName(path)}\"", Path.Combine(directory, "program.asm"));

            bytes.Should().Equal(0xAA, 0xBB);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Test]
    public void IncludesAssemblyFiles()
    {
        var directory = TestContext.CurrentContext.WorkDirectory;
        var includePath = Path.Combine(directory, $"constants-{Guid.NewGuid():N}.asm");
        var sourcePath = Path.Combine(directory, $"program-{Guid.NewGuid():N}.asm");

        try
        {
            File.WriteAllText(includePath, ".equ VALUE, 42\n");
            File.WriteAllText(sourcePath, $".include \"{Path.GetFileName(includePath)}\"\nDATA R0, VALUE\n");

            var result = new ScottAssembler().AssembleFile(sourcePath);

            result.Success.Should().BeTrue(string.Join(Environment.NewLine, result.Diagnostics));
            result.Bytes.Select(b => (int)b).Should().Equal(InstructionSet.Data(0), 42);
        }
        finally
        {
            File.Delete(includePath);
            File.Delete(sourcePath);
        }
    }

    [Test]
    public void SupportsCliDefinesAsConstants()
    {
        var options = new AssemblerOptions();
        options.Defines["VALUE"] = 42;

        var result = new ScottAssembler().AssembleText("DATA R0, VALUE", options: options);

        result.Success.Should().BeTrue();
        result.Bytes.Select(b => (int)b).Should().Equal(InstructionSet.Data(0), 42);
    }

    private static int[] Assemble(string source, string path = "<memory>")
    {
        var result = new ScottAssembler().AssembleText(source, path);
        result.Success.Should().BeTrue(string.Join(Environment.NewLine, result.Diagnostics));
        return result.Bytes.Select(b => (int)b).ToArray();
    }
}
