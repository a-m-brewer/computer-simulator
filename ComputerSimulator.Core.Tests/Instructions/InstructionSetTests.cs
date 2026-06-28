using System.Collections.Generic;
using ComputerSimulator.Core.Enums;
using ComputerSimulator.Core.Exceptions;
using ComputerSimulator.Core.Instructions;
using FluentAssertions;
using NUnit.Framework;

namespace ComputerSimulator.Core.Tests.Instructions;

public class InstructionSetTests
{
    [Test]
    public void EncodesMemoryAndImmediateInstructions()
    {
        InstructionSet.Ld(0, 1).Should().Be(0x01);
        InstructionSet.St(0, 1).Should().Be(0x11);
        InstructionSet.Data(1).Should().Be(0x21);
        InstructionSet.Jmpr(1).Should().Be(0x31);
    }

    [Test]
    public void EncodesJumpsAndFlags()
    {
        InstructionSet.Jmp().Should().Be(0x40);
        InstructionSet.JumpIf(JumpCondition.Equal).Should().Be(0x52);
        InstructionSet.JumpIf(JumpCondition.Carry).Should().Be(0x58);
        InstructionSet.JumpIf(JumpCondition.Carry | JumpCondition.Equal).Should().Be(0x5A);
        InstructionSet.Clf.Should().Be(0x60);
    }

    [Test]
    public void EncodesIoInstructions()
    {
        InstructionSet.In(DataAddress.Data, 0).Should().Be(0x70);
        InstructionSet.In(DataAddress.Address, 0).Should().Be(0x74);
        InstructionSet.Out(DataAddress.Data, 0).Should().Be(0x78);
        InstructionSet.Out(DataAddress.Address, 1).Should().Be(0x7D);
    }

    [Test]
    [TestCase(OpCode.Add, 0x80)]
    [TestCase(OpCode.Shr, 0x90)]
    [TestCase(OpCode.Shl, 0xA0)]
    [TestCase(OpCode.Not, 0xB0)]
    [TestCase(OpCode.And, 0xC0)]
    [TestCase(OpCode.Or, 0xD0)]
    [TestCase(OpCode.XOr, 0xE0)]
    [TestCase(OpCode.Cmp, 0xF0)]
    public void EncodesAluInstructions(OpCode opCode, int expectedPrefix)
    {
        InstructionSet.Alu(opCode, 0, 1).Should().Be(expectedPrefix | 0x01);
    }

    [Test]
    public void RejectsInvalidRegisters()
    {
        FluentActions.Invoking(() => InstructionSet.Add(0, 4))
            .Should()
            .Throw<ComputerSimulatorException>();
    }

    [Test]
    public void RejectsInvalidOpCodes()
    {
        FluentActions.Invoking(() => InstructionSet.Alu((OpCode)8, 0, 0))
            .Should()
            .Throw<ComputerSimulatorException>();
    }

    [Test]
    public void RejectsInvalidOperands()
    {
        var program = new List<int>();

        FluentActions.Invoking(() => InstructionSet.Emit(program, InstructionSet.Data(0), 0x100))
            .Should()
            .Throw<ComputerSimulatorException>();
    }
}
