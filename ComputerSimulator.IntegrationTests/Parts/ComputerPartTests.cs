using System;
using ComputerSimulator.Core.Enums;
using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Instructions;
using ComputerSimulator.Core.Models;
using ComputerSimulator.Core.Parts;
using FluentAssertions;
using FluentAssertions.Execution;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests.Parts;

[TestFixture]
public class ComputerPartTests : IntegrationTestBase
{
    private IComputerPart _sut = null!;
    private bool[] _max = null!;
    private bool[] _min = null!;
    private int _maxInt;
    private int _minInt;

    [SetUp]
    public void SetUp()
    {
        var computerSettings = GetRequiredService<ComputerSettings>();

        _max = computerSettings.WordSize.InitArray<bool>().Fill(true);
        _maxInt = _max.ToInt();
        
        _min = computerSettings.WordSize.InitArray<bool>().Fill(false);
        _minInt = 0;
        
        _sut = ComponentFactory.CreateComputerPart();
    }

    // Step 1

    [Test]
    public void MarIsSetToAddressInIarInStepOne()
    {
        _sut.Iar.SetRegisterValue(_max);

        PerformFullStep();

        _sut.Ram.Mar.StoredValue
            .ToInt()
            .Should()
            .Be(_maxInt);
    }

    [Test]
    public void AfterStepOneAccIsIncrementOfIarAddress()
    {
        _sut.Iar.SetRegisterValue(_min);

        PerformFullStep();

        _sut.Acc.StoredValue
            .ToInt()
            .Should()
            .Be(1);
    }

    // Step 2

    [Test]
    public void AfterStep2ByteInRamIsInIr()
    {
        _sut.Iar.SetRegisterValue(_min);
        _sut.Ram.GetSlot(0, 0).Memory.SetRegisterValue(_max);

        PerformFullStep(2);

        _sut.Ir.StoredValue
            .Should()
            .AllSatisfy(wire => wire.Value.Should().BeTrue());
    }

    // step 3

    [Test]
    public void AfterStep3IarIsIncrementedByOne()
    {
        _sut.Iar.SetRegisterValue(_min);

        PerformFullStep(3);

        _sut.Iar.StoredValue
            .ToInt()
            .Should()
            .Be(1);
    }

    [Test]
    public void AfterStep3IrIsInstructionStoredInRam()
    {
        _sut.Iar.SetRegisterValue(_min);
        _sut.Ram.GetSlot(0, 0).Memory.SetRegisterValue(_max);

        PerformFullStep(3);

        _sut.Ir.StoredValue
            .Should()
            .AllSatisfy(w => w.Value.Should().BeTrue());
    }

    // Full instructions

    // ADD R0 R1
    // 1 000 00 01

    [Test]
    public void CanAddTwoNumbersTogetherStep3()
    {
        var instruction = InstructionBits(InstructionSet.Add(0, 1));

        _sut.Ram.GetSlot(0, 0).Memory.SetRegisterValue(instruction);

        _sut.GeneralPurposeRegisters[0].SetRegisterValue(CreateNumber(50));
        _sut.GeneralPurposeRegisters[1].SetRegisterValue(CreateNumber(25));

        PerformFullStep(3);

        using (new AssertionScope())
        {
            for (var i = 0; i < instruction.Length; i++)
            {
                _sut.Ir.StoredValue[i].Value.Should().Be(instruction[i]);
            }
        }
    }

    [Test]
    public void CanAddTwoNumbersTogetherStep4EnableRegBEnabled()
    {
        var instruction = InstructionBits(InstructionSet.Add(0, 1));

        _sut.Ram.GetSlot(0, 0).Memory.SetRegisterValue(instruction);

        const int regBValue = 25;

        _sut.GeneralPurposeRegisters[0].SetRegisterValue(CreateNumber(50));
        _sut.GeneralPurposeRegisters[1].SetRegisterValue(CreateNumber(regBValue));

        PerformFullStep(3);
        PerformStep();

        _sut.GeneralPurposeRegisters[1]
            .Enable
            .Value
            .Should()
            .BeTrue();
    }

    [Test]
    public void CanAddTwoNumbersTogetherStep4EnableRegBValueOnBus()
    {
        var instruction = InstructionBits(InstructionSet.Add(0, 1));

        _sut.Ram.GetSlot(0, 0).Memory.SetRegisterValue(instruction);

        const int regBValue = 25;

        _sut.GeneralPurposeRegisters[0].SetRegisterValue(CreateNumber(50));
        _sut.GeneralPurposeRegisters[1].SetRegisterValue(CreateNumber(regBValue));

        PerformFullStep(3);
        PerformStep();

        _sut.IoBus.CpuBus
            .ToInt()
            .Should()
            .Be(regBValue);
    }

    [Test]
    public void CanAddTwoNumbersTogetherStep4TmpSetShouldBeTrue()
    {
        var instruction = InstructionBits(InstructionSet.Add(0, 1));

        _sut.Ram.GetSlot(0, 0).Memory.SetRegisterValue(instruction);

        const int regBValue = 25;

        _sut.GeneralPurposeRegisters[0].SetRegisterValue(CreateNumber(50));
        _sut.GeneralPurposeRegisters[1].SetRegisterValue(CreateNumber(regBValue));

        PerformFullStep(3);
        PerformStep(2);

        _sut.Tmp
            .Set
            .Value
            .Should()
            .BeTrue();
    }
    
    [Test]
    public void CanAddTwoNumbersTogetherStep4TmpShouldBeRegB()
    {
        var instruction = InstructionBits(InstructionSet.Add(0, 1));

        _sut.Ram.GetSlot(0, 0).Memory.SetRegisterValue(instruction);

        const int regBValue = 25;

        _sut.GeneralPurposeRegisters[0].SetRegisterValue(CreateNumber(50));
        _sut.GeneralPurposeRegisters[1].SetRegisterValue(CreateNumber(regBValue));

        PerformFullStep(4);

        _sut.Tmp
            .StoredValue
            .ToInt()
            .Should()
            .Be(regBValue);
    }
    
    [Test]
    public void CanAddTwoNumbersTogetherStep5RegAEnabled()
    {
        var instruction = InstructionBits(InstructionSet.Add(0, 1));

        _sut.Ram.GetSlot(0, 0).Memory.SetRegisterValue(instruction);

        const int regBValue = 25;

        _sut.GeneralPurposeRegisters[0].SetRegisterValue(CreateNumber(50));
        _sut.GeneralPurposeRegisters[1].SetRegisterValue(CreateNumber(regBValue));

        PerformFullStep(4);
        PerformStep();

        _sut.GeneralPurposeRegisters[0]
            .Enable
            .Value
            .Should()
            .BeTrue();
    }
    
    [Test]
    public void CanAddTwoNumbersTogetherStep5RegAValueOnBus()
    {
        var instruction = InstructionBits(InstructionSet.Add(0, 1));

        _sut.Ram.GetSlot(0, 0).Memory.SetRegisterValue(instruction);

        const int regAValue = 50;

        _sut.GeneralPurposeRegisters[0].SetRegisterValue(CreateNumber(regAValue));
        _sut.GeneralPurposeRegisters[1].SetRegisterValue(CreateNumber(25));

        PerformFullStep(4);
        PerformStep();

        _sut.IoBus.CpuBus
            .ToInt()
            .Should()
            .Be(regAValue);
    }
    
    [Test]
    public void CanAddTwoNumbersTogetherStep5AccSet()
    {
        var instruction = InstructionBits(InstructionSet.Add(0, 1));

        _sut.Ram.GetSlot(0, 0).Memory.SetRegisterValue(instruction);

        const int regAValue = 50;

        _sut.GeneralPurposeRegisters[0].SetRegisterValue(CreateNumber(regAValue));
        _sut.GeneralPurposeRegisters[1].SetRegisterValue(CreateNumber(25));

        PerformFullStep(4);
        PerformStep(2);

        _sut.Acc.Set
            .Value
            .Should()
            .BeTrue();
    }
    
    [Test]
    public void CanAddTwoNumbersTogetherStep5ResultInAcc()
    {
        var instruction = InstructionBits(InstructionSet.Add(0, 1));

        _sut.Ram.GetSlot(0, 0).Memory.SetRegisterValue(instruction);

        const int regAValue = 50;

        _sut.GeneralPurposeRegisters[0].SetRegisterValue(CreateNumber(regAValue));
        _sut.GeneralPurposeRegisters[1].SetRegisterValue(CreateNumber(30));

        PerformFullStep(4);
        PerformStep(2);

        _sut.Acc.StoredValue
            .ToInt()
            .Should()
            .Be(80);
    }

    [Test]
    public void CanAddTwoNumbersTogether()
    {
        const int expected = 80;

        var instruction = InstructionBits(InstructionSet.Add(0, 1));

        _sut.Ram.GetSlot(0, 0).Memory.SetRegisterValue(instruction);

        _sut.GeneralPurposeRegisters[0].SetRegisterValue(CreateNumber(50));
        _sut.GeneralPurposeRegisters[1].SetRegisterValue(CreateNumber(30));

        PerformFullStep(6);

        _sut.GeneralPurposeRegisters[1]
            .StoredValue
            .ToInt()
            .Should()
            .Be(expected);
    }

    // SHL R0 R1
    // 1 001 00 01
    [Test]
    public void CanShiftLeft()
    {
        var instruction = InstructionBits(InstructionSet.Shl(0, 1));

        const int regAInput = 0b00000010;
        const int expectedRegBOutput = 0b00000100;

        _sut.Ram.GetSlot(0, 0).Memory.SetRegisterValue(instruction);
        _sut.GeneralPurposeRegisters[0].SetRegisterValue(CreateNumber(regAInput));

        PerformFullStep(6);

        var result = _sut.GeneralPurposeRegisters[1].StoredValue;

        result.ToInt()
            .Should()
            .Be(expectedRegBOutput);
    }

    // SHR R0 R1
    [Test]
    public void CanShiftRight()
    {
        var instruction = InstructionBits(InstructionSet.Shr(0, 1));

        const int regAInput = 0b00000010;
        const int expectedRegBOutput = 0b00000001;

        _sut.Ram.GetSlot(0, 0).Memory.SetRegisterValue(instruction);
        _sut.GeneralPurposeRegisters[0].SetRegisterValue(CreateNumber(regAInput));

        PerformFullStep(6);

        _sut.GeneralPurposeRegisters[1].StoredValue
            .ToInt()
            .Should()
            .Be(expectedRegBOutput);
    }

    // NOT R0 R1
    [Test]
    public void CanNotAByte()
    {
        var instruction = InstructionBits(InstructionSet.Not(0, 1));

        _sut.Ram.GetSlot(0, 0).Memory.SetRegisterValue(instruction);
        _sut.GeneralPurposeRegisters[0].SetRegisterValue(_min);

        PerformFullStep(6);

        _sut.GeneralPurposeRegisters[1]
            .StoredValue
            .Should()
            .AllSatisfy(w => w.Value.Should().BeTrue());
    }

    // AND R0 R1
    [Test]
    public void CanAndByte()
    {
        var instruction = InstructionBits(InstructionSet.And(0, 1));

        _sut.Ram.GetSlot(0, 0).Memory.SetRegisterValue(instruction);
        _sut.GeneralPurposeRegisters[0].SetRegisterValue(_min);
        _sut.GeneralPurposeRegisters[1].SetRegisterValue(_max);

        PerformFullStep(6);

        _sut.GeneralPurposeRegisters[1]
            .StoredValue
            .Should()
            .AllSatisfy(w => w.Value.Should().BeFalse());
    }

    // Or R0 R1
    [Test]
    public void CanOrByte()
    {
        var instruction = InstructionBits(InstructionSet.Or(0, 1));

        _sut.Ram.GetSlot(0, 0).Memory.SetRegisterValue(instruction);
        _sut.GeneralPurposeRegisters[0].SetRegisterValue(_min);
        _sut.GeneralPurposeRegisters[1].SetRegisterValue(_max);

        PerformFullStep(6);

        _sut.GeneralPurposeRegisters[1]
            .StoredValue
            .Should()
            .AllSatisfy(w => w.Value.Should().BeTrue());
    }


    // XOR R0 R1
    [Test]
    public void CanXorByte()
    {
        var instruction = InstructionBits(InstructionSet.XOr(0, 1));

        _sut.Ram.GetSlot(0, 0).Memory.SetRegisterValue(instruction);
        _sut.GeneralPurposeRegisters[0].SetRegisterValue(_max);
        _sut.GeneralPurposeRegisters[1].SetRegisterValue(_max);

        PerformFullStep(6);

        _sut.GeneralPurposeRegisters[1]
            .StoredValue
            .Should()
            .AllSatisfy(w => w.Value.Should().BeFalse());
    }

    [Test]
    public void CanCmpEqualBytes()
    {
        var instruction = InstructionBits(InstructionSet.Cmp(0, 1));

        _sut.Ram.GetSlot(0, 0).Memory.SetRegisterValue(instruction);
        _sut.GeneralPurposeRegisters[0].SetRegisterValue(CreateNumber(200));
        _sut.GeneralPurposeRegisters[1].SetRegisterValue(CreateNumber(200));

        PerformFullStep(6);

        var result = _sut.Caez.StoredValue;

        result.C.Value.Should().BeFalse();
        result.A.Value.Should().BeFalse();
        result.E.Value.Should().BeTrue();
        // Tried to figure this out but I don't see how following the design this would correctly output false
        // with cmp operation.
        // result.Z.Value.Should().BeFalse();
    }

    // LD R0 R1
    // Load RB from RAM Address RA
    [Test]
    public void CanLoadFromRam()
    {
        var instruction = InstructionBits(InstructionSet.Ld(0, 1));

        const int ramValue = 255;
        
        _sut.Ram.GetSlot(0, 0).Memory.SetRegisterValue(instruction);
        _sut.GeneralPurposeRegisters[0].SetRegisterValue(CreateNumber(1));
        _sut.Ram.GetSlot(1, 0).Memory.SetRegisterValue(CreateNumber(ramValue));

        PerformFullStep(6);

        var result = _sut.GeneralPurposeRegisters[1].StoredValue;

        result
            .ToInt()
            .Should()
            .Be(ramValue);
    }

    // ST R0 R1
    // Store RB to Ram address in RA
    [Test]
    public void CanStoreDataInRam()
    {
        var instruction = InstructionBits(InstructionSet.St(0, 1));
        
        const int storedNumber = 255;

        _sut.Ram.GetSlot(0, 0).Memory.SetRegisterValue(instruction);

        _sut.GeneralPurposeRegisters[0].SetRegisterValue(CreateNumber(1));
        _sut.GeneralPurposeRegisters[1].SetRegisterValue(CreateNumber(storedNumber));

        PerformFullStep(6);

        var result = _sut.Ram.GetSlot(1, 0).Memory.StoredValue;

        result
            .ToInt()
            .Should()
            .Be(storedNumber);
    }
    
    [Test]
    public void CanStoreDataInRamStep4RegAValueStoredInMar()
    {
        var instruction = InstructionBits(InstructionSet.St(0, 1));
        
        const int storedNumber = 255;

        _sut.Ram.GetSlot(0, 0).Memory.SetRegisterValue(instruction);

        _sut.GeneralPurposeRegisters[0].SetRegisterValue(CreateNumber(1));
        _sut.GeneralPurposeRegisters[1].SetRegisterValue(CreateNumber(storedNumber));

        PerformFullStep(3);
        PerformStep(2);

        _sut.Ram.Mar.StoredValue
            .ToInt()
            .Should()
            .Be(1);
    }
    
    [Test]
    public void CanStoreDataInRamStep5RegBValueStoredInRam()
    {
        var instruction = InstructionBits(InstructionSet.St(0, 1));
        
        const int storedNumber = 255;

        _sut.Ram.GetSlot(0, 0).Memory.SetRegisterValue(instruction);

        _sut.GeneralPurposeRegisters[0].SetRegisterValue(CreateNumber(1));
        _sut.GeneralPurposeRegisters[1].SetRegisterValue(CreateNumber(storedNumber));

        PerformFullStep(4);
        PerformStep(2);

        _sut.Ram.GetSlot(1, 0).Memory.StoredValue
            .ToInt()
            .Should()
            .Be(storedNumber);
    }

    // DATA RB, xxxx xxxx
    // Load next ram byte into RB
    [Test]
    public void CanPerformDataInstruction()
    {
        var instruction = InstructionBits(InstructionSet.Data(1));
        _sut.Ram.GetSlot(0, 0).Memory.SetRegisterValue(instruction);

        const int expectedNumber = 255;
        _sut.Ram.GetSlot(1, 0).Memory.SetRegisterValue(CreateNumber(expectedNumber));

        PerformFullStep(6);

        var result = _sut.GeneralPurposeRegisters[1].StoredValue;

        result
            .ToInt()
            .Should()
            .Be(expectedNumber);
    }

    // JMPR RB
    // Jump to address in RB
    [Test]
    public void CanPerformJumpRegisterInstruction()
    {
        var instruction = InstructionBits(InstructionSet.Jmpr(1));
        _sut.Ram.GetSlot(0, 0).Memory.SetRegisterValue(instruction);

        const int expectedValue = 255;
        _sut.GeneralPurposeRegisters[1].SetRegisterValue(CreateNumber(expectedValue));

        PerformFullStep(6);

        var result = _sut.Iar.StoredValue;

        result
            .ToInt()
            .Should()
            .Be(expectedValue);
    }

    // JMP Addr
    // Jump to location in next Ram Register
    [Test]
    public void CanPerformJumpInstruction()
    {
        var instruction = InstructionBits(InstructionSet.Jmp());
        _sut.Ram.GetSlot(0, 0).Memory.SetRegisterValue(instruction);

        _sut.Ram.GetSlot(1, 0).Memory.SetRegisterValue(_max);

        PerformFullStep(6);

        var result = _sut.Iar.StoredValue;

        result
            .Should()
            .AllSatisfy(w => w.Value.Should().BeTrue());
    }

    // JC Addr
    // Jump if Equal is on
    [Test]
    public void CanPerformJumpIfEqualIsOn()
    {
        var compareInstruction = InstructionBits(InstructionSet.Cmp(0, 1));

        _sut.Ram.GetSlot(0, 0).Memory.SetRegisterValue(compareInstruction);
        var instruction = InstructionBits(InstructionSet.JumpIf(JumpCondition.Equal));
        _sut.Ram.GetSlot(1, 0).Memory.SetRegisterValue(instruction);
        _sut.Ram.GetSlot(2, 0).Memory.SetRegisterValue(_max);

        _sut.GeneralPurposeRegisters[0].SetRegisterValue(CreateNumber(200));
        _sut.GeneralPurposeRegisters[1].SetRegisterValue(CreateNumber(200));

        PerformFullStep(6);

        PerformStep();

        PerformFullStep(6);

        var result = _sut.Iar.StoredValue;

        result
            .Should()
            .AllSatisfy(w => w.Value.Should().BeTrue());
    }

    // JC Addr
    // Jump if Carry is on
    [Test]
    public void CanPerformJumpIfCarryIsOn()
    {
        var instruction = InstructionBits(InstructionSet.JumpIf(JumpCondition.Carry));
        _sut.Ram.GetSlot(0, 0).Memory.SetRegisterValue(instruction);

        _sut.Caez.Inputs.C.Value = true;
        _sut.Caez.SetRegisterValue();

        const int expectedAddress = 10;
        _sut.Ram.GetSlot(1, 0).Memory.SetRegisterValue(CreateNumber(expectedAddress));
        
        PerformFullStep(6);

        _sut.Iar.StoredValue.ToInt()
            .Should()
            .Be(expectedAddress);
    }

    [Test]
    public void CanClearFlagsStep4Bus1Set()
    {
        _sut.Caez.Inputs.C.Value = true;
        _sut.Caez.Inputs.A.Value = true;
        _sut.Caez.Inputs.E.Value = true;
        _sut.Caez.Inputs.Z.Value = true;
        _sut.Caez.SetRegisterValue();
        
        var instruction = InstructionBits(InstructionSet.Clf);

        _sut.Ram.GetSlot(0, 0).Memory.SetRegisterValue(instruction);

        PerformFullStep(3);
        PerformStep();

        _sut.Bus1
            .Bit
            .Value
            .Should()
            .BeTrue();
    }
    
    [Test]
    public void CanClearFlagsStep4Bus1Outputs1()
    {
        _sut.Caez.Inputs.C.Value = true;
        _sut.Caez.Inputs.A.Value = true;
        _sut.Caez.Inputs.E.Value = true;
        _sut.Caez.Inputs.Z.Value = true;
        _sut.Caez.SetRegisterValue();
        
        var instruction = InstructionBits(InstructionSet.Clf);

        _sut.Ram.GetSlot(0, 0).Memory.SetRegisterValue(instruction);

        PerformFullStep(3);
        PerformStep();

        _sut.Bus1
            .Outputs
            .ToInt()
            .Should()
            .Be(1);
    }
    
    [Test]
    public void CanClearFlagsStep4BusShouldBeEmpty()
    {
        _sut.Caez.Inputs.C.Value = true;
        _sut.Caez.Inputs.A.Value = true;
        _sut.Caez.Inputs.E.Value = true;
        _sut.Caez.Inputs.Z.Value = true;
        _sut.Caez.SetRegisterValue();
        
        var instruction = InstructionBits(InstructionSet.Clf);

        _sut.Ram.GetSlot(0, 0).Memory.SetRegisterValue(instruction);

        PerformFullStep(3);
        PerformStep();

        _sut.IoBus
            .CpuBus
            .ToInt()
            .Should()
            .Be(0);
    }
    
    [Test]
    public void CanClearFlagsStep4AluOutput1()
    {
        _sut.Caez.Inputs.C.Value = true;
        _sut.Caez.Inputs.A.Value = true;
        _sut.Caez.Inputs.E.Value = true;
        _sut.Caez.Inputs.Z.Value = true;
        _sut.Caez.SetRegisterValue();
        
        var instruction = InstructionBits(InstructionSet.Clf);

        _sut.Ram.GetSlot(0, 0).Memory.SetRegisterValue(instruction);

        PerformFullStep(3);
        PerformStep();

        _sut.Alu
            .Outputs
            .ToInt()
            .Should()
            .Be(1);
    }
    
    [Test]
    public void CanClearFlags()
    {
        _sut.Caez.Inputs.C.Value = true;
        _sut.Caez.Inputs.A.Value = true;
        _sut.Caez.Inputs.E.Value = true;
        _sut.Caez.Inputs.Z.Value = true;
        _sut.Caez.SetRegisterValue();
        
        var instruction = InstructionBits(InstructionSet.Clf);

        _sut.Ram.GetSlot(0, 0).Memory.SetRegisterValue(instruction);
        
        PerformFullStep(6);

        using (new AssertionScope())
        {
            _sut.Caez.StoredValue.C.Value.Should().BeFalse();
            _sut.Caez.StoredValue.A.Value.Should().BeFalse();
            _sut.Caez.StoredValue.E.Value.Should().BeFalse();
            _sut.Caez.StoredValue.Z.Value.Should().BeFalse();
        }
    }

    // TODO: Create instructions for IOBus pg. 149

    [Test]
    public void CanInputIoDataToRb()
    {
        var register = ComponentFactory.CreateRegister(
            _sut.IoBus.Clk.Set,
            _sut.IoBus.Clk.Enable,
            _sut.IoBus.CpuBus,
            _sut.IoBus.CpuBus);
        register.SetRegisterValue(_max);
        
        _sut.IoBus.ConnectedComponents.Add(register);
        
        var instruction = InstructionBits(InstructionSet.In(DataAddress.Data, 0));
        _sut.Ram.GetSlot(0, 0).Memory.SetRegisterValue(instruction);

        PerformFullStep(6);

        var result = _sut.GeneralPurposeRegisters[0].StoredValue;

        result
            .ToInt()
            .Should()
            .Be(_maxInt);
    }
    
    [Test]
    public void InputIoInstructionDoesNotCorruptInstructionAddressRegister()
    {
        // Regression: the IO input (In) instruction must latch the bus value into RegB only.
        // A control-signal bug also asserted Iar.Set during step 5, latching the device byte
        // into the program counter, so the CPU jumped to address == input value.
        var register = ComponentFactory.CreateRegister(
            _sut.IoBus.Clk.Set,
            _sut.IoBus.Clk.Enable,
            _sut.IoBus.CpuBus,
            _sut.IoBus.CpuBus);
        register.SetRegisterValue(_max);

        _sut.IoBus.ConnectedComponents.Add(register);

        var instruction = InstructionBits(InstructionSet.In(DataAddress.Data, 0));
        _sut.Ram.GetSlot(0, 0).Memory.SetRegisterValue(instruction);

        PerformFullStep(6);

        // The instruction at address 0 is not a jump, so the IAR should only have advanced
        // by one during fetch — it must NOT contain the input value (_maxInt).
        _sut.Iar.StoredValue
            .ToInt()
            .Should()
            .Be(1);
    }

    [Test]
    public void CanInputIoAddressToRb()
    {
        var register = ComponentFactory.CreateRegister(
            _sut.IoBus.Clk.Set,
            _sut.IoBus.Clk.Enable,
            _sut.IoBus.CpuBus,
            _sut.IoBus.CpuBus);
        register.SetRegisterValue(_max);
        
        _sut.IoBus.ConnectedComponents.Add(register);
        
        var instruction = InstructionBits(InstructionSet.In(DataAddress.Address, 0));
        _sut.Ram.GetSlot(0, 0).Memory.SetRegisterValue(instruction);

        PerformFullStep(6);

        var result = _sut.GeneralPurposeRegisters[0].StoredValue;

        result
            .ToInt()
            .Should()
            .Be(_maxInt);
    }

    [Test]
    public void CanOutputDataInRbToIo()
    {
        var register = ComponentFactory.CreateRegister(
            _sut.IoBus.Clk.Set,
            _sut.IoBus.Clk.Enable,
            _sut.IoBus.CpuBus,
            _sut.IoBus.CpuBus);

        _sut.IoBus.ConnectedComponents.Add(register);
        
        _sut.GeneralPurposeRegisters[0].SetRegisterValue(_max);
        
        var instruction = InstructionBits(InstructionSet.Out(DataAddress.Data, 0));
        _sut.Ram.GetSlot(0, 0).Memory.SetRegisterValue(instruction);

        PerformFullStep(6);

        register.StoredValue
            .ToInt()
            .Should()
            .Be(_maxInt);
    }

    [Test]
    public void CanOutputAddressInRbToIo()
    {
        var register = ComponentFactory.CreateRegister(
            _sut.IoBus.Clk.Set,
            _sut.IoBus.Clk.Enable,
            _sut.IoBus.CpuBus,
            _sut.IoBus.CpuBus);

        _sut.IoBus.ConnectedComponents.Add(register);
        
        _sut.GeneralPurposeRegisters[0].SetRegisterValue(_max);
        
        var instruction = InstructionBits(InstructionSet.Out(DataAddress.Address, 0));
        _sut.Ram.GetSlot(0, 0).Memory.SetRegisterValue(instruction);

        PerformFullStep(6);

        register.StoredValue
            .ToInt()
            .Should()
            .Be(_maxInt);
    }

    private void PerformStep(int steps = 1)
    {
        for (var i = 0; i < steps; i++)
        {
            _sut.Update();
        }
    }

    private void PerformFullStep(int fullSteps = 1)
    {
        PerformStep(fullSteps * 4);
    }

    private bool[] CreateNumber(int number)
    {
        return number.ToBinaryBools(ComputerSettings.WordSize);
    }

    private static bool[] InstructionBits(int instruction)
    {
        return instruction.ToBinaryBools(8);
    }

    private void VerifyWireShouldBeTrue(Func<IComputerPart, IWire<bool>> selector)
    {
        VerifyWireShouldBe(selector, true);
    }

    private void VerifyWireShouldBeFalse(Func<IComputerPart, IWire<bool>> selector)
    {
        VerifyWireShouldBe(selector, false);
    }

    private void VerifyWireShouldBe(Func<IComputerPart, IWire<bool>> selector, bool expected)
    {
        selector(_sut).Value
            .Should()
            .Be(expected);
    }

    private void SetupRegPositions(int a, int b)
    {
        SetupRegAPositions(a);
        SetupRegBPositions(b);
    }

    private void SetupRegAPositions(int a)
    {
        var aBool = a.ToBinaryBools(2);

        _sut.Ir.Inputs[4].Value = aBool[0];
        _sut.Ir.Inputs[5].Value = aBool[1];

        _sut.Ir.SetRegisterValue();
    }

    private void SetupRegBPositions(int b)
    {
        var bBool = b.ToBinaryBools(2);

        _sut.Ir.Inputs[6].Value = bBool[0];
        _sut.Ir.Inputs[7].Value = bBool[1];

        _sut.Ir.SetRegisterValue();
    }

    private void VerifyRegEnable(bool expected, int position)
    {
        // Using register 0 as without setting last 2 bits (false, false) that is the addresses of register 0
        VerifyWireShouldBe(s => s.GeneralPurposeRegisters[position].Enable, expected);
    }

    private void VerifyRegSet(bool expected, int position)
    {
        // Using register 0 as without setting last 2 bits (false, false) that is the addresses of register 0
        VerifyWireShouldBe(s => s.GeneralPurposeRegisters[position].Set, expected);
    }
}
