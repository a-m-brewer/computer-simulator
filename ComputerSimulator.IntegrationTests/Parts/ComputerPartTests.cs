using System;
using System.Linq;
using ComputerSimulator.Core.Extensions;
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
        var instruction = new[] { true, false, false, false, false, false, false, true };

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
        var instruction = 0b10000001.ToBinaryBools(8);

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
        var instruction = 0b10000001.ToBinaryBools(8);

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
        var instruction = 0b10000001.ToBinaryBools(8);

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
        var instruction = new[] { true, false, false, false, false, false, false, true };

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
        var instruction = 0b10000001.ToBinaryBools(8);

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
        var instruction = 0b10000001.ToBinaryBools(8);

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
    public void CanAddTwoNumbersTogether()
    {
        const int expected = 75;

        var instruction = new[] { true, false, false, false, false, false, false, true };

        _sut.Ram.GetSlot(0, 0).Memory.SetRegisterValue(instruction);

        _sut.GeneralPurposeRegisters[0].SetRegisterValue(CreateNumber(50));
        _sut.GeneralPurposeRegisters[1].SetRegisterValue(CreateNumber(25));

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
        var instruction = new[] { true, false, false, true, false, false, false, true };

        _sut.Ram.GetSlot(0, 0).Memory.SetRegisterValue(instruction);
        _sut.GeneralPurposeRegisters[0].SetRegisterValue(CreateNumber(128));

        PerformFullStep(6);

        var result = _sut.GeneralPurposeRegisters[1].StoredValue;

        result[6]
            .Value
            .Should()
            .BeTrue();
    }

    // SHR R0 R1
    [Test]
    public void CanShiftRight()
    {
        var instruction = new[] { true, false, true, false, false, false, false, true };

        _sut.Ram.GetSlot(0, 0).Memory.SetRegisterValue(instruction);
        _sut.GeneralPurposeRegisters[0].SetRegisterValue(CreateNumber(1));

        PerformFullStep(6);

        _sut.GeneralPurposeRegisters[1].StoredValue[1]
            .Value
            .Should()
            .BeTrue();
    }

    // NOT R0 R1
    [Test]
    public void CanNotAByte()
    {
        var instruction = new[] { true, false, true, true, false, false, false, true };

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
        var instruction = new[] { true, true, false, false, false, false, false, true };

        _sut.Ram.GetSlot(0, 0).Memory.SetRegisterValue(instruction);
        _sut.GeneralPurposeRegisters[0].SetRegisterValue(_min);
        _sut.GeneralPurposeRegisters[1].SetRegisterValue(CreateNumber(255));

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
        var instruction = new[] { true, true, false, true, false, false, false, true };

        _sut.Ram.GetSlot(0, 0).Memory.SetRegisterValue(instruction);
        _sut.GeneralPurposeRegisters[0].SetRegisterValue(_min);
        _sut.GeneralPurposeRegisters[1].SetRegisterValue(CreateNumber(255));

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
        var instruction = new[] { true, true, true, false, false, false, false, true };

        _sut.Ram.GetSlot(0, 0).Memory.SetRegisterValue(instruction);
        _sut.GeneralPurposeRegisters[0].SetRegisterValue(CreateNumber(255));
        _sut.GeneralPurposeRegisters[1].SetRegisterValue(CreateNumber(255));

        PerformFullStep(6);

        _sut.GeneralPurposeRegisters[1]
            .StoredValue
            .Should()
            .AllSatisfy(w => w.Value.Should().BeFalse());
    }

    [Test]
    public void CanCmpEqualBytes()
    {
        var instruction = new[] { true, true, true, true, false, false, false, true };

        _sut.Ram.GetSlot(0, 0).Memory.SetRegisterValue(instruction);
        _sut.GeneralPurposeRegisters[0].SetRegisterValue(CreateNumber(255));
        _sut.GeneralPurposeRegisters[1].SetRegisterValue(CreateNumber(255));

        PerformFullStep(6);

        var result = _sut.Caez.StoredValue;

        result.C.Value.Should().BeFalse();
        result.A.Value.Should().BeTrue();
        result.E.Value.Should().BeFalse();
        // Tried to figure this out but I don't see how following the design this would correctly output false
        // with cmp operation.
        result.Z.Value.Should().BeFalse();
    }

    // LD R0 R1
    // Load RB from RAM Address RA
    [Test]
    public void CanLoadFromRam()
    {
        var instruction = new[] { false, false, false, false, false, false, false, true };

        _sut.Ram.GetSlot(0, 0).Memory.SetRegisterValue(instruction);
        _sut.GeneralPurposeRegisters[0].SetRegisterValue(CreateNumber(1));
        _sut.Ram.GetSlot(1, 0).Memory.SetRegisterValue(CreateNumber(255));

        PerformFullStep(6);

        var result = _sut.GeneralPurposeRegisters[1].StoredValue;

        result
            .Should()
            .AllSatisfy(w => w.Value.Should().BeTrue());
    }

    // ST R0 R1
    // Store RB to Ram address in RA
    [Test]
    public void CanStoreDataInRam()
    {
        var instruction = new[] { false, false, false, true, false, false, false, true };

        _sut.Ram.GetSlot(0, 0).Memory.SetRegisterValue(instruction);

        _sut.GeneralPurposeRegisters[0].SetRegisterValue(CreateNumber(1));
        _sut.GeneralPurposeRegisters[1].SetRegisterValue(CreateNumber(255));

        PerformFullStep(6);

        var result = _sut.Ram.GetSlot(1, 0).Memory.StoredValue;

        result
            .Should()
            .AllSatisfy(w => w.Value.Should().BeTrue());
    }

    // DATA RB, xxxx xxxx
    // Load next ram byte into RB
    [Test]
    public void CanPerformDataInstruction()
    {
        var instruction = new[] { false, false, true, false, false, false, false, false };
        _sut.Ram.GetSlot(0, 0).Memory.SetRegisterValue(instruction);

        _sut.Ram.GetSlot(1, 0).Memory.SetRegisterValue(CreateNumber(255));

        PerformFullStep(6);

        var result = _sut.GeneralPurposeRegisters[0].StoredValue;

        result
            .Should()
            .AllSatisfy(w => w.Value.Should().BeTrue());
    }

    // JMPR RB
    // Jump to address in RB
    [Test]
    public void CanPerformJumpRegisterInstruction()
    {
        var instruction = new[] { false, false, true, true, false, false, false, false };
        _sut.Ram.GetSlot(0, 0).Memory.SetRegisterValue(instruction);

        _sut.GeneralPurposeRegisters[0].SetRegisterValue(_max);

        PerformFullStep(6);

        var result = _sut.Iar.StoredValue;

        result
            .Should()
            .AllSatisfy(w => w.Value.Should().BeTrue());
    }

    // JMP Addr
    // Jump to location in next Ram Register
    [Test]
    public void CanPerformJumpInstruction()
    {
        var instruction = new[] { false, true, false, false, false, false, false, false };
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
        var addInstruction = new[] { true, false, false, false, false, false, false, true };

        _sut.Ram.GetSlot(0, 0).Memory.SetRegisterValue(addInstruction);
        var instruction = new[] { false, true, false, true, false, false, true, false };
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
        var addInstruction = new[] { true, false, false, false, false, false, false, true };

        _sut.Ram.GetSlot(0, 0).Memory.SetRegisterValue(addInstruction);
        var instruction = new[] { false, true, false, true, true, false, false, false };
        _sut.Ram.GetSlot(1, 0).Memory.SetRegisterValue(instruction);
        _sut.Ram.GetSlot(2, 0).Memory.SetRegisterValue(_max);

        _sut.GeneralPurposeRegisters[0].SetRegisterValue(CreateNumber(200));
        _sut.GeneralPurposeRegisters[1].SetRegisterValue(CreateNumber(200));

        PerformFullStep(6);

        _sut.Caez.StoredValue.C.Value.Should().BeTrue();

        PerformStep();

        PerformFullStep(6);

        var result = _sut.Iar.StoredValue;

        result
            .Should()
            .AllSatisfy(w => w.Value.Should().BeTrue());
    }

    [Test]
    public void CanClearFlags()
    {
        var addInstruction = new[] { true, false, false, false, false, false, false, true };
        _sut.Ram.GetSlot(0, 0).Memory.SetRegisterValue(addInstruction);

        var clearInstruction = new[] { false, true, true, false, false, false, false, false };
        _sut.Ram.GetSlot(1, 0).Memory.SetRegisterValue(clearInstruction);

        _sut.GeneralPurposeRegisters[0].SetRegisterValue(CreateNumber(200));
        _sut.GeneralPurposeRegisters[1].SetRegisterValue(CreateNumber(200));

        PerformFullStep(6);

        _sut.Caez.StoredValue.C.Value.Should().BeTrue();
        _sut.Caez.StoredValue.A.Value.Should().BeFalse();
        _sut.Caez.StoredValue.E.Value.Should().BeTrue();
        _sut.Caez.StoredValue.Z.Value.Should().BeFalse();

        PerformStep();

        PerformFullStep(3);

        _sut.Bus1.Bit.Value.Should().BeTrue();

        PerformStep();

        _sut.Caez.Set.Value.Should().BeTrue();

        PerformStep(2);

        PerformFullStep(2);

        _sut.Caez.StoredValue.C.Value.Should().BeFalse();
        _sut.Caez.StoredValue.A.Value.Should().BeFalse();
        _sut.Caez.StoredValue.E.Value.Should().BeFalse();
        _sut.Caez.StoredValue.Z.Value.Should().BeFalse();
    }

    // TODO: Create instructions for IOBus pg. 149

    [Test]
    public void CanInputIoDataToRb()
    {
        _sut.IoBus.CpuBus.SetValue(_max);
        var instruction = new[] { false, true, true, true, false, false, false, false };
        _sut.Ram.GetSlot(0, 0).Memory.SetRegisterValue(instruction);

        PerformFullStep(6);

        var result = _sut.GeneralPurposeRegisters[0].StoredValue;

        result
            .Should()
            .AllSatisfy(w => w.Value.Should().BeTrue());
    }

    [Test]
    public void CanInputIoAddressToRb()
    {
        _sut.IoBus.CpuBus.SetValue(_max);
        var instruction = new[] { false, true, true, true, false, true, false, false };
        _sut.Ram.GetSlot(0, 0).Memory.SetRegisterValue(instruction);

        PerformFullStep(6);

        var result = _sut.GeneralPurposeRegisters[0].StoredValue;

        result
            .Should()
            .AllSatisfy(w => w.Value.Should().BeTrue());
    }

    [Test]
    public void CanOutputToIoAsData()
    {
        _sut.GeneralPurposeRegisters[0].SetRegisterValue(_max);
        var instruction = new[] { false, true, true, true, true, false, false, false };
        _sut.Ram.GetSlot(0, 0).Memory.SetRegisterValue(instruction);

        PerformFullStep(6);

        var result = _sut.IoBus.CpuBus;

        result
            .Should()
            .AllSatisfy(w => w.Value.Should().BeTrue());
    }

    [Test]
    public void CanOutputToIoAsAddress()
    {
        _sut.GeneralPurposeRegisters[0].SetRegisterValue(_max);
        var instruction = new[] { false, true, true, true, true, true, false, false };
        _sut.Ram.GetSlot(0, 0).Memory.SetRegisterValue(instruction);

        PerformFullStep(6);

        var result = _sut.IoBus.CpuBus;

        result
            .Should()
            .AllSatisfy(w => w.Value.Should().BeTrue());
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