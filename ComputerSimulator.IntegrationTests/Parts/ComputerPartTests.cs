using System;
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

    [SetUp]
    public void SetUp()
    {
        var computerSettings = GetRequiredService<ComputerSettings>();

        _max = computerSettings.WordSize.InitArray<bool>().Fill(true);
        _min = computerSettings.WordSize.InitArray<bool>().Fill(false);
        _sut = ComponentFactory.CreateComputerPart();
    }

    // Step 1

    [Test]
    public void MarIsSetToAddressInIarInStepOne()
    {
        _sut.Iar.SetRegisterValue(_max);

        PerformFullStep();

        _sut.Ram.Mar.StoredValue
            .Should()
            .AllBeEquivalentTo(true);
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
        _sut.Ram.Slots[0][0].Memory.SetRegisterValue(_max);

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
        _sut.Ram.Slots[0][0].Memory.SetRegisterValue(_max);

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

        _sut.Ram.Slots[0][0].Memory.SetRegisterValue(instruction);

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
        var instruction = new[] { true, false, false, false, false, false, false, true };

        _sut.Ram.Slots[0][0].Memory.SetRegisterValue(instruction);

        const int regBValue = 25;

        _sut.GeneralPurposeRegisters[0].SetRegisterValue(CreateNumber(50));
        _sut.GeneralPurposeRegisters[1].SetRegisterValue(CreateNumber(regBValue));

        PerformFullStep(3);
        PerformStep();

        _sut.GeneralPurposeRegisters[1]
            .Set
            .Value
            .Should()
            .BeTrue();
    }

    [Test]
    public void CanAddTwoNumbersTogetherStep4EnableRegBValueOnBus()
    {
        var instruction = new[] { true, false, false, false, false, false, false, true };

        _sut.Ram.Slots[0][0].Memory.SetRegisterValue(instruction);

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
    public void CanAddTwoNumbersTogetherStep4TmpShouldBeRegB()
    {
        var instruction = new[] { true, false, false, false, false, false, false, true };

        _sut.Ram.Slots[0][0].Memory.SetRegisterValue(instruction);

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
    public void CanAddTwoNumbersTogether()
    {
        const int expected = 75;

        var instruction = new[] { true, false, false, false, false, false, false, true };

        _sut.Ram.Slots[0][0].Memory.SetRegisterValue(instruction);

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

        _sut.Ram.Slots[0][0].Memory.SetRegisterValue(instruction);
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

        _sut.Ram.Slots[0][0].Memory.SetRegisterValue(instruction);
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

        _sut.Ram.Slots[0][0].Memory.SetRegisterValue(instruction);
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

        _sut.Ram.Slots[0][0].Memory.SetRegisterValue(instruction);
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

        _sut.Ram.Slots[0][0].Memory.SetRegisterValue(instruction);
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

        _sut.Ram.Slots[0][0].Memory.SetRegisterValue(instruction);
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

        _sut.Ram.Slots[0][0].Memory.SetRegisterValue(instruction);
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

        _sut.Ram.Slots[0][0].Memory.SetRegisterValue(instruction);
        _sut.GeneralPurposeRegisters[0].SetRegisterValue(CreateNumber(1));
        _sut.Ram.Slots[0][1].Memory.SetRegisterValue(CreateNumber(255));

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

        _sut.Ram.Slots[0][0].Memory.SetRegisterValue(instruction);

        _sut.GeneralPurposeRegisters[0].SetRegisterValue(CreateNumber(1));
        _sut.GeneralPurposeRegisters[1].SetRegisterValue(CreateNumber(255));

        PerformFullStep(6);

        var result = _sut.Ram.Slots[0][1].Memory.StoredValue;

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
        _sut.Ram.Slots[0][0].Memory.SetRegisterValue(instruction);

        _sut.Ram.Slots[0][1].Memory.SetRegisterValue(CreateNumber(255));

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
        _sut.Ram.Slots[0][0].Memory.SetRegisterValue(instruction);

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
        _sut.Ram.Slots[0][0].Memory.SetRegisterValue(instruction);

        _sut.Ram.Slots[0][1].Memory.SetRegisterValue(_max);

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

        _sut.Ram.Slots[0][0].Memory.SetRegisterValue(addInstruction);
        var instruction = new[] { false, true, false, true, false, false, true, false };
        _sut.Ram.Slots[0][1].Memory.SetRegisterValue(instruction);
        _sut.Ram.Slots[0][2].Memory.SetRegisterValue(_max);

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

        _sut.Ram.Slots[0][0].Memory.SetRegisterValue(addInstruction);
        var instruction = new[] { false, true, false, true, true, false, false, false };
        _sut.Ram.Slots[0][1].Memory.SetRegisterValue(instruction);
        _sut.Ram.Slots[0][2].Memory.SetRegisterValue(_max);

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
        _sut.Ram.Slots[0][0].Memory.SetRegisterValue(addInstruction);

        var clearInstruction = new[] { false, true, true, false, false, false, false, false };
        _sut.Ram.Slots[0][1].Memory.SetRegisterValue(clearInstruction);

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
        _sut.Ram.Slots[0][0].Memory.SetRegisterValue(instruction);

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
        _sut.Ram.Slots[0][0].Memory.SetRegisterValue(instruction);

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
        _sut.Ram.Slots[0][0].Memory.SetRegisterValue(instruction);

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
        _sut.Ram.Slots[0][0].Memory.SetRegisterValue(instruction);

        PerformFullStep(6);

        var result = _sut.IoBus.CpuBus;

        result
            .Should()
            .AllSatisfy(w => w.Value.Should().BeTrue());
    }


    public class CentralProcessingUnitStep1Tests : ComputerPartTests
    {
        [Test]
        public void IoInputOutputIsIr4()
        {
            _sut.InstructionRegister[4].Value = true;

            PerformStep();

            VerifyWireShouldBeTrue(v => v.IoInputOutput);
        }

        [Test]
        public void IoDataAddressIsIr5()
        {
            _sut.InstructionRegister[5].Value = true;

            PerformStep();

            VerifyWireShouldBeTrue(v => v.IoDataAddress);
        }

        public class CentralProcessingUnitStep1EnableTests : CentralProcessingUnitStep1Tests
        {
            [SetUp]
            public void Step1EnableSetUp()
            {
                PerformStep();
            }

            [Test]
            public void Bus1IsTrue()
            {
                VerifyWireShouldBeTrue(s => s.Bus1);
            }

            [Test]
            public void IarEnableIsTrue()
            {
                VerifyWireShouldBeTrue(s => s.Iar.Enable);
            }
        }

        public class CentralProcessingUnitStep1SetTests : CentralProcessingUnitStep1Tests
        {
            [SetUp]
            public void Step1SetSetUp()
            {
                PerformStep(2);
            }

            [Test]
            public void MarIsTrue()
            {
                VerifyWireShouldBeTrue(s => s.MarSet);
            }

            [Test]
            public void AccSetIsTrue()
            {
                VerifyWireShouldBeTrue(s => s.Acc.Set);
            }
        }
    }

    public class CentralProcessingUnitStep2Tests : ComputerPartTests
    {
        [SetUp]
        public void Step2SetUp()
        {
            // Step to previous step to the one being tested
            PerformFullStep();
        }

        public class CentralProcessingUnitStep2EnableTests : CentralProcessingUnitStep2Tests
        {
            [SetUp]
            public void Step2EnableSetUp()
            {
                PerformStep();
            }

            [Test]
            public void RamEnableIsTrue()
            {
                VerifyWireShouldBeTrue(s => s.Ram.Enable);
            }
        }

        public class CentralProcessingUnitStep2SetTests : CentralProcessingUnitStep2Tests
        {
            [SetUp]
            public void Step2SetSetUp()
            {
                PerformStep(2);
            }

            [Test]
            public void IrSetIsTrue()
            {
                VerifyWireShouldBeTrue(s => s.IrSet);
            }
        }
    }

    public class CentralProcessingUnitStep3Tests : ComputerPartTests
    {
        [SetUp]
        public void Step3SetUp()
        {
            // Step to previous step to the one being tested
            PerformFullStep(2);
        }

        public class CentralProcessingUnitStep3EnableTests : CentralProcessingUnitStep3Tests
        {
            [SetUp]
            public void Step3EnableSetUp()
            {
                PerformStep();
            }

            [Test]
            public void AccEnableIsTrue()
            {
                VerifyWireShouldBeTrue(s => s.Acc.Enable);
            }
        }

        public class CentralProcessingUnitStep3SetTests : CentralProcessingUnitStep3Tests
        {
            [SetUp]
            public void Step3SetSetUp()
            {
                PerformStep(2);
            }

            [Test]
            public void IarSetIsTrue()
            {
                VerifyWireShouldBeTrue(s => s.Iar.Set);
            }
        }
    }

    public class CentralProcessingUnitStep4Tests : ComputerPartTests
    {
        [SetUp]
        public void Step4SetUp()
        {
            // Step to previous step to the one being tested
            PerformFullStep(3);
        }

        public class CentralProcessingUnitStep4EnableTests : CentralProcessingUnitStep4Tests
        {
            [Test]
            [TestCase(true)]
            [TestCase(false)]
            public void RegBEnableIsTrue(bool aluFlag)
            {
                _sut.InstructionRegister[0].Value = aluFlag;

                const int regBPos = 1;
                SetupRegPositions(0, regBPos);

                PerformStep();

                VerifyRegEnable(aluFlag, regBPos);
            }

            [Test]
            [TestCase(false, false)]
            [TestCase(false, true)]
            [TestCase(true, false)]
            [TestCase(true, true)]
            public void RegAIsEnabled(bool aluFlag, bool decoder)
            {
                _sut.InstructionRegister[0].Value = aluFlag;
                _sut.InstructionRegister[3].Value = decoder;

                const int regAPos = 0;
                SetupRegPositions(regAPos, 1);

                PerformStep();

                VerifyRegEnable(!aluFlag, regAPos);
            }

            [Test]
            public void AfterStep4EnableDataInstructionBus1IsSet()
            {
                _sut.InstructionRegister[2].Value = true;

                PerformStep();

                VerifyWireShouldBeTrue(s => s.Bus1);
            }

            [Test]
            public void AfterStep4EnableDataInstructionIarIsTrue()
            {
                _sut.InstructionRegister[2].Value = true;

                PerformStep();

                VerifyWireShouldBeTrue(s => s.Iar.Enable);
            }

            [Test]
            public void AfterStep4EnableJumpRegisterInstructionRegBIsTrue()
            {
                _sut.InstructionRegister[2].Value = true;
                _sut.InstructionRegister[3].Value = true;

                const int regBPos = 1;
                SetupRegPositions(0, regBPos);

                PerformStep();

                VerifyRegEnable(true, regBPos);
            }

            [Test]
            public void AfterStep4EnableJumpIarIsTrue()
            {
                _sut.InstructionRegister[1].Value = true;

                PerformStep();

                VerifyWireShouldBeTrue(s => s.Iar.Enable);
            }

            [Test]
            public void AfterStep4EnableJumpIfBus1IsTrue()
            {
                _sut.InstructionRegister[1].Value = true;
                _sut.InstructionRegister[3].Value = true;

                PerformStep();

                VerifyWireShouldBeTrue(s => s.Bus1);
            }

            [Test]
            public void AfterStep4EnableJumpIfIarIsTrue()
            {
                _sut.InstructionRegister[1].Value = true;
                _sut.InstructionRegister[3].Value = true;

                PerformStep();

                VerifyWireShouldBeTrue(s => s.Iar.Enable);
            }

            [Test]
            public void AfterStep4EnableClearBus1IsTrue()
            {
                _sut.InstructionRegister[1].Value = true;
                _sut.InstructionRegister[2].Value = true;

                PerformStep();

                VerifyWireShouldBeTrue(s => s.Bus1);
            }

            [Test]
            [TestCase(false)]
            [TestCase(true)]
            public void AfterStep4EnableIoRegBIsTrue(bool output)
            {
                _sut.InstructionRegister[1].Value = true;
                _sut.InstructionRegister[2].Value = true;
                _sut.InstructionRegister[3].Value = true;
                _sut.InstructionRegister[4].Value = output;

                const int regBPos = 1;

                SetupRegBPositions(regBPos);

                PerformStep();

                VerifyRegEnable(output, regBPos);
            }
        }

        public class CentralProcessingUnitStep4SetTests : CentralProcessingUnitStep4Tests
        {
            [SetUp]
            public void Step4SetSetUp()
            {
                PerformStep();
            }

            [Test]
            [TestCase(true)]
            [TestCase(false)]
            public void AfterFourSetStepTmpSetIsTrue(bool aluFlag)
            {
                _sut.InstructionRegister[0].Value = aluFlag;

                PerformStep();

                VerifyWireShouldBe(v => v.TmpSet, aluFlag);
            }

            [Test]
            [TestCase(true)]
            [TestCase(false)]
            public void AfterFourSetStepCarryInTmpSetIsTrue(bool aluFlag)
            {
                _sut.InstructionRegister[0].Value = aluFlag;

                PerformStep();

                VerifyWireShouldBe(v => v.CarryInTmp, aluFlag);
            }

            [Test]
            [TestCase(false, false)]
            [TestCase(false, true)]
            [TestCase(true, false)]
            [TestCase(true, true)]
            public void AfterFourSetMarIsSet(bool aluFlag, bool decoder)
            {
                _sut.InstructionRegister[0].Value = aluFlag;
                _sut.InstructionRegister[3].Value = decoder;

                PerformStep();

                VerifyWireShouldBe(v => v.MarSet, !aluFlag);
            }

            [Test]
            public void AfterStep4SetDataInstructionMarIsTrue()
            {
                _sut.InstructionRegister[2].Value = true;

                PerformStep();

                VerifyWireShouldBeTrue(v => v.MarSet);
            }

            [Test]
            public void AfterStep4SetDataInstructionAccIsTrue()
            {
                _sut.InstructionRegister[2].Value = true;

                PerformStep();

                VerifyWireShouldBeTrue(v => v.Acc.Set);
            }

            [Test]
            public void AfterStep4SetJumpRegisterInstructionIarIsTrue()
            {
                _sut.InstructionRegister[2].Value = true;
                _sut.InstructionRegister[3].Value = true;

                PerformStep();

                VerifyWireShouldBeTrue(v => v.Iar.Set);
            }

            [Test]
            public void AfterStep4SetJumpMarIsTrue()
            {
                _sut.InstructionRegister[1].Value = true;

                PerformStep();

                VerifyWireShouldBeTrue(v => v.MarSet);
            }

            [Test]
            public void AfterStep4SetJumpIfMarIsTrue()
            {
                _sut.InstructionRegister[1].Value = true;
                _sut.InstructionRegister[3].Value = true;

                PerformStep();

                VerifyWireShouldBeTrue(v => v.MarSet);
            }

            [Test]
            public void AfterStep4SetJumpIfAccIsTrue()
            {
                _sut.InstructionRegister[1].Value = true;
                _sut.InstructionRegister[3].Value = true;

                PerformStep();

                VerifyWireShouldBeTrue(v => v.Acc.Set);
            }

            [Test]
            public void AfterStep4SetClearCaezFlagsIsTrue()
            {
                _sut.InstructionRegister[1].Value = true;
                _sut.InstructionRegister[2].Value = true;

                PerformStep();

                VerifyWireShouldBeTrue(v => v.FlagsSet);
            }

            [Test]
            [TestCase(false)]
            [TestCase(true)]
            public void AfterStep4IoClkSIsTrue(bool output)
            {
                _sut.InstructionRegister[1].Value = true;
                _sut.InstructionRegister[2].Value = true;
                _sut.InstructionRegister[3].Value = true;
                _sut.InstructionRegister[4].Value = output;

                PerformStep();

                VerifyWireShouldBe(s => s.IoClk.Set, output);
            }
        }
    }

    public class CentralProcessingUnitStep5Tests : ComputerPartTests
    {
        [SetUp]
        public void Step5SetUp()
        {
            // Step to previous step to the one being tested
            PerformFullStep(4);
        }

        public class CentralProcessingUnitStep5EnableTests : CentralProcessingUnitStep5Tests
        {
            [Test]
            [TestCase(true)]
            [TestCase(false)]
            public void AfterStep5RegAEnableIsTrue(bool aluFlag)
            {
                _sut.InstructionRegister[0].Value = aluFlag;

                const int regAPos = 0;
                SetupRegPositions(regAPos, 1);

                PerformStep();

                VerifyRegEnable(aluFlag, regAPos);
            }

            [Test]
            [TestCase(false, false, true, false)]
            [TestCase(false, true, false, true)]
            [TestCase(true, false, false, false)]
            [TestCase(true, true, false, false)]
            public void AfterStepFiveEnableEitherRamOrRegBIsSet(bool aluFlag, bool decoder, bool ramSet, bool rbSet)
            {
                _sut.InstructionRegister[0].Value = aluFlag;
                _sut.InstructionRegister[3].Value = decoder;

                const int regBPos = 1;
                SetupRegPositions(0, regBPos);

                PerformStep();

                VerifyWireShouldBe(s => s.Ram.Enable, ramSet);
                VerifyRegEnable(rbSet, regBPos);
            }

            [Test]
            public void AfterStep5EnableDataInstructionRamIsTrue()
            {
                _sut.InstructionRegister[2].Value = true;

                PerformStep();

                VerifyWireShouldBeTrue(v => v.Ram.Enable);
            }

            [Test]
            public void AfterStep5EnableJumpRamIsTrue()
            {
                _sut.InstructionRegister[1].Value = true;

                PerformStep();

                VerifyWireShouldBeTrue(v => v.Ram.Enable);
            }

            [Test]
            public void AfterStep5JumpIfEnableAccIsTrue()
            {
                _sut.InstructionRegister[1].Value = true;
                _sut.InstructionRegister[3].Value = true;

                PerformStep();

                VerifyWireShouldBeTrue(v => v.Acc.Enable);
            }

            [Test]
            [TestCase(false)]
            [TestCase(true)]
            public void AfterStep5EnableIoClkEIsTrue(bool output)
            {
                _sut.InstructionRegister[1].Value = true;
                _sut.InstructionRegister[2].Value = true;
                _sut.InstructionRegister[3].Value = true;
                _sut.InstructionRegister[4].Value = output;

                PerformStep();

                VerifyWireShouldBe(v => v.IoClk.Enable, !output);
            }

            // ALU Op

            [Test]
            [TestCase(false, false, false, false, false, false, false)]
            [TestCase(false, false, false, true, false, false, false)]
            [TestCase(false, false, true, false, false, false, false)]
            [TestCase(false, false, true, true, false, false, false)]
            [TestCase(false, true, false, false, false, false, false)]
            [TestCase(false, true, false, true, false, false, false)]
            [TestCase(false, true, true, false, false, false, false)]
            [TestCase(false, true, true, true, false, false, false)]
            [TestCase(true, false, false, false, false, false, false)]
            [TestCase(true, false, false, true, false, false, true)]
            [TestCase(true, false, true, false, false, true, false)]
            [TestCase(true, false, true, true, false, true, true)]
            [TestCase(true, true, false, false, true, false, false)]
            [TestCase(true, true, false, true, true, false, true)]
            [TestCase(true, true, true, false, true, true, false)]
            [TestCase(true, true, true, true, true, true, true)]
            public void CorrectOpCodeIsSetOnStep5(bool aluFlag, bool ir1, bool ir2, bool ir3, bool expected1,
                bool expected2, bool expected3)
            {
                _sut.InstructionRegister[0].Value = aluFlag;
                _sut.InstructionRegister[1].Value = ir1;
                _sut.InstructionRegister[2].Value = ir2;
                _sut.InstructionRegister[3].Value = ir3;

                PerformStep();

                VerifyWireShouldBe(v => v.Op[0], expected1);
                VerifyWireShouldBe(v => v.Op[1], expected2);
                VerifyWireShouldBe(v => v.Op[2], expected3);
            }
        }

        public class CentralProcessingUnitStep5SetTests : CentralProcessingUnitStep5Tests
        {
            [SetUp]
            public void Step5SetSetUp()
            {
                PerformStep();
            }

            [Test]
            [TestCase(true)]
            [TestCase(false)]
            public void AfterStep5AccSetIsTrue(bool aluFlag)
            {
                _sut.InstructionRegister[0].Value = aluFlag;

                PerformStep();

                VerifyWireShouldBe(v => v.Acc.Set, aluFlag);
            }

            [Test]
            [TestCase(false, false, false, true)]
            [TestCase(false, true, true, false)]
            [TestCase(true, false, false, false)]
            [TestCase(true, true, false, false)]
            public void AfterStepFiveSetEitherRamOrRegBIsSet(bool aluFlag, bool decoder, bool ramSet, bool rbSet)
            {
                _sut.InstructionRegister[0].Value = aluFlag;
                _sut.InstructionRegister[3].Value = decoder;

                const int regBPos = 1;
                SetupRegPositions(0, regBPos);

                PerformStep();

                VerifyWireShouldBe(v => v.Ram.Set, ramSet);
                VerifyRegSet(rbSet, regBPos);
            }

            [Test]
            public void AfterStep5SetDataInstructionRegBIsTrue()
            {
                _sut.InstructionRegister[2].Value = true;

                const int regBPos = 1;
                SetupRegPositions(0, regBPos);

                PerformStep();

                VerifyRegSet(true, regBPos);
            }

            [Test]
            public void AfterStep5SetJumpIarIsTrue()
            {
                _sut.InstructionRegister[1].Value = true;

                PerformStep();

                VerifyWireShouldBeTrue(v => v.Iar.Set);
            }

            [Test]
            public void AfterStep5JumpIfSetIarIsTrue()
            {
                _sut.InstructionRegister[1].Value = true;
                _sut.InstructionRegister[3].Value = true;

                PerformStep();

                VerifyWireShouldBeTrue(v => v.Iar.Set);
            }

            [Test]
            public void AfterStep5IfAluOpFlagsSetIsTrue()
            {
                _sut.InstructionRegister[0].Value = true;

                PerformStep();

                VerifyWireShouldBeTrue(v => v.FlagsSet);
            }

            [Test]
            [TestCase(false)]
            [TestCase(true)]
            public void AfterStep5SetIoRegBSetIsTrue(bool output)
            {
                _sut.InstructionRegister[1].Value = true;
                _sut.InstructionRegister[2].Value = true;
                _sut.InstructionRegister[3].Value = true;
                _sut.InstructionRegister[4].Value = output;

                const int regBPos = 1;
                SetupRegBPositions(regBPos);

                PerformStep();

                VerifyRegSet(!output, regBPos);
            }
        }
    }

    public class CentralProcessingUnitStep6Tests : ComputerPartTests
    {
        [SetUp]
        public void Step6SetUp()
        {
            // Step to previous step to the one being tested
            PerformFullStep(5);
        }

        public class CentralProcessingUnitStep6EnableTests : CentralProcessingUnitStep6Tests
        {
            [Test]
            [TestCase(false, false, false, false, false)]
            [TestCase(false, false, false, true, false)]
            // this case is actually the data instruction
            // [TestCase(false, false, true, false, false)]
            [TestCase(false, false, true, true, false)]
            [TestCase(false, true, false, false, false)]
            [TestCase(false, true, false, true, false)]
            [TestCase(false, true, true, false, false)]
            [TestCase(false, true, true, true, false)]
            [TestCase(true, false, false, false, true)]
            [TestCase(true, false, false, true, true)]
            [TestCase(true, false, true, false, true)]
            [TestCase(true, false, true, true, true)]
            [TestCase(true, true, false, false, true)]
            [TestCase(true, true, false, true, true)]
            [TestCase(true, true, true, false, true)]
            [TestCase(true, true, true, true, false)]
            public void AfterStep6EnableAccIsEnabled(bool aluFlag, bool ir1, bool ir2, bool ir3, bool expected)
            {
                _sut.InstructionRegister[0].Value = aluFlag;
                _sut.InstructionRegister[1].Value = ir1;
                _sut.InstructionRegister[2].Value = ir2;
                _sut.InstructionRegister[3].Value = ir3;

                PerformStep();

                VerifyWireShouldBe(v => v.Acc.Enable, expected);
            }

            [Test]
            public void AfterStep6EnableDataInstructionAccIsTrue()
            {
                _sut.InstructionRegister[2].Value = true;

                PerformStep();

                VerifyWireShouldBeTrue(v => v.Acc.Enable);
            }

            [Test]
            [TestCase(false, false, false, false, false, false, false, false, false)]
            [TestCase(false, false, false, false, false, false, false, true, false)]
            [TestCase(false, false, false, true, false, false, false, false, false)]
            [TestCase(false, false, false, true, false, false, false, true, true)]
            [TestCase(false, false, false, false, false, false, false, false, false)]
            [TestCase(false, false, false, false, false, false, true, false, false)]
            [TestCase(false, false, true, false, false, false, false, false, false)]
            [TestCase(false, false, true, false, false, false, true, false, true)]
            [TestCase(false, false, false, false, false, false, false, false, false)]
            [TestCase(false, false, false, false, false, true, false, false, false)]
            [TestCase(false, true, false, false, false, false, false, false, false)]
            [TestCase(false, true, false, false, false, true, false, false, true)]
            [TestCase(false, false, false, false, false, false, false, false, false)]
            [TestCase(false, false, false, false, true, false, false, false, false)]
            [TestCase(true, false, false, false, false, false, false, false, false)]
            [TestCase(true, false, false, false, true, false, false, false, true)]
            public void AfterStep6EnableJumpIfRamIsTrue(bool c, bool a, bool e, bool z, bool cCheck, bool aCheck,
                bool eCheck, bool zCheck, bool expected)
            {
                _sut.InstructionRegister[1].Value = true;
                _sut.InstructionRegister[3].Value = true;

                _sut.InstructionRegister[4].Value = cCheck;
                _sut.InstructionRegister[5].Value = aCheck;
                _sut.InstructionRegister[6].Value = eCheck;
                _sut.InstructionRegister[7].Value = zCheck;

                _sut.Caez.C.Value = c;
                _sut.Caez.A.Value = a;
                _sut.Caez.E.Value = e;
                _sut.Caez.Z.Value = z;

                PerformStep();

                VerifyWireShouldBe(v => v.Ram.Enable, expected);
            }
        }

        public class CentralProcessingUnitStep6SetTests : CentralProcessingUnitStep6Tests
        {
            [SetUp]
            public void Step6SetSetUp()
            {
                PerformStep();
            }

            [Test]
            [TestCase(false, false, false, false, false)]
            [TestCase(false, false, false, true, false)]
            [TestCase(false, false, true, false, false)]
            [TestCase(false, false, true, true, false)]
            [TestCase(false, true, false, false, false)]
            [TestCase(false, true, false, true, false)]
            [TestCase(false, true, true, false, false)]
            [TestCase(false, true, true, true, false)]
            [TestCase(true, false, false, false, true)]
            [TestCase(true, false, false, true, true)]
            [TestCase(true, false, true, false, true)]
            [TestCase(true, false, true, true, true)]
            [TestCase(true, true, false, false, true)]
            [TestCase(true, true, false, true, true)]
            [TestCase(true, true, true, false, true)]
            [TestCase(true, true, true, true, false)]
            public void AfterStep6SetRegBIsSet(bool aluFlag, bool ir1, bool ir2, bool ir3, bool expected)
            {
                _sut.InstructionRegister[0].Value = aluFlag;
                _sut.InstructionRegister[1].Value = ir1;
                _sut.InstructionRegister[2].Value = ir2;
                _sut.InstructionRegister[3].Value = ir3;

                const int regBPos = 1;
                SetupRegPositions(0, regBPos);

                PerformStep();

                VerifyRegSet(expected, regBPos);
            }

            [Test]
            public void AfterStep6SetDataInstructionIarIsTrue()
            {
                _sut.InstructionRegister[2].Value = true;

                PerformStep();

                VerifyWireShouldBeTrue(v => v.Iar.Set);
            }

            [Test]
            [TestCase(false, false, false, false, false, false, false, false, false)]
            [TestCase(false, false, false, false, false, false, false, true, false)]
            [TestCase(false, false, false, true, false, false, false, false, false)]
            [TestCase(false, false, false, true, false, false, false, true, true)]
            [TestCase(false, false, false, false, false, false, false, false, false)]
            [TestCase(false, false, false, false, false, false, true, false, false)]
            [TestCase(false, false, true, false, false, false, false, false, false)]
            [TestCase(false, false, true, false, false, false, true, false, true)]
            [TestCase(false, false, false, false, false, false, false, false, false)]
            [TestCase(false, false, false, false, false, true, false, false, false)]
            [TestCase(false, true, false, false, false, false, false, false, false)]
            [TestCase(false, true, false, false, false, true, false, false, true)]
            [TestCase(false, false, false, false, false, false, false, false, false)]
            [TestCase(false, false, false, false, true, false, false, false, false)]
            [TestCase(true, false, false, false, false, false, false, false, false)]
            [TestCase(true, false, false, false, true, false, false, false, true)]
            public void AfterStep6SetJumpIfIarIsTrue(bool c, bool a, bool e, bool z, bool cCheck, bool aCheck,
                bool eCheck,
                bool zCheck, bool expected)
            {
                _sut.InstructionRegister[1].Value = true;
                _sut.InstructionRegister[3].Value = true;

                _sut.InstructionRegister[4].Value = cCheck;
                _sut.InstructionRegister[5].Value = aCheck;
                _sut.InstructionRegister[6].Value = eCheck;
                _sut.InstructionRegister[7].Value = zCheck;

                _sut.Caez.C.Value = c;
                _sut.Caez.A.Value = a;
                _sut.Caez.E.Value = e;
                _sut.Caez.Z.Value = z;

                PerformStep();

                VerifyWireShouldBe(v => v.Iar.Set, expected);
            }
        }
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