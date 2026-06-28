using System;
using ComputerSimulator.Core.Enums;
using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Parts;
using FluentAssertions;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests.Parts;

public class ComputerPinTests : IntegrationTestBase
{
    private IComputerPart _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = ComponentFactory.CreateComputerPart();
    }

    public class ComputerPinCentralProcessingUnitStep1Tests : ComputerPinTests
    {
        [Test]
        public void IoInputOutputIsIr4()
        {
            _sut.Ir.Inputs.InstructionWire(4).Value = true;
            _sut.Ir.SetRegisterValue();

            PerformStep();

            VerifyWireShouldBeTrue(v => v.IoBus.InputOutput);
        }

        [Test]
        public void IoDataAddressIsIr5()
        {
            _sut.Ir.Inputs.InstructionWire(5).Value = true;
            _sut.Ir.SetRegisterValue();

            PerformStep();

            VerifyWireShouldBeTrue(v => v.IoBus.DataAddress);
        }

        public class ComputerPinCentralProcessingUnitStep1EnableTests : ComputerPinCentralProcessingUnitStep1Tests
        {
            [SetUp]
            public void Step1EnableSetUp()
            {
                PerformStep();
            }

            [Test]
            public void Bus1IsTrue()
            {
                VerifyWireShouldBeTrue(s => s.Bus1.Bit);
            }

            [Test]
            public void IarEnableIsTrue()
            {
                VerifyWireShouldBeTrue(s => s.Iar.Enable);
            }
        }

        public class ComputerPinCentralProcessingUnitStep1SetTests : ComputerPinCentralProcessingUnitStep1Tests
        {
            [SetUp]
            public void Step1SetSetUp()
            {
                PerformStep(2);
            }

            [Test]
            public void MarIsTrue()
            {
                VerifyWireShouldBeTrue(s => s.Ram.Mar.Set);
            }

            [Test]
            public void AccSetIsTrue()
            {
                VerifyWireShouldBeTrue(s => s.Acc.Set);
            }
        }
    }

    public class ComputerPinCentralProcessingUnitStep2Tests : ComputerPinTests
    {
        [SetUp]
        public void Step2SetUp()
        {
            // Step to previous step to the one being tested
            PerformFullStep();
        }

        public class ComputerPinCentralProcessingUnitStep2EnableTests : ComputerPinCentralProcessingUnitStep2Tests
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

        public class ComputerPinCentralProcessingUnitStep2SetTests : ComputerPinCentralProcessingUnitStep2Tests
        {
            [SetUp]
            public void Step2SetSetUp()
            {
                PerformStep(2);
            }

            [Test]
            public void IrSetIsTrue()
            {
                VerifyWireShouldBeTrue(s => s.Ir.Set);
            }
        }
    }

    public class ComputerPinCentralProcessingUnitStep3Tests : ComputerPinTests
    {
        [SetUp]
        public void Step3SetUp()
        {
            // Step to previous step to the one being tested
            PerformFullStep(2);
        }

        public class ComputerPinCentralProcessingUnitStep3EnableTests : ComputerPinCentralProcessingUnitStep3Tests
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

        public class ComputerPinCentralProcessingUnitStep3SetTests : ComputerPinCentralProcessingUnitStep3Tests
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

    public class ComputerPinCentralProcessingUnitStep4Tests : ComputerPinTests
    {
        [SetUp]
        public void Step4SetUp()
        {
            // Step to previous step to the one being tested
            PerformFullStep(3);
        }

        public class ComputerPinCentralProcessingUnitStep4EnableTests : ComputerPinCentralProcessingUnitStep4Tests
        {
            [Test]
            [TestCase(true)]
            [TestCase(false)]
            public void RegBEnableIsTrue(bool aluFlag)
            {
                _sut.Ir.Inputs.InstructionWire(0).Value = aluFlag;
                _sut.Ir.SetRegisterValue();

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
                _sut.Ir.Inputs.InstructionWire(0).Value = aluFlag;
                _sut.Ir.Inputs.InstructionWire(3).Value = decoder;
                _sut.Ir.SetRegisterValue();

                const int regAPos = 0;
                SetupRegPositions(regAPos, 1);

                PerformStep();

                VerifyRegEnable(!aluFlag, regAPos);
            }

            [Test]
            public void AfterStep4EnableDataInstructionBus1IsSet()
            {
                _sut.Ir.Inputs.InstructionWire(2).Value = true;
                _sut.Ir.SetRegisterValue();

                PerformStep();

                VerifyWireShouldBeTrue(s => s.Bus1.Bit);
            }

            [Test]
            public void AfterStep4EnableDataInstructionIarIsTrue()
            {
                _sut.Ir.Inputs.InstructionWire(2).Value = true;
                _sut.Ir.SetRegisterValue();

                PerformStep();

                VerifyWireShouldBeTrue(s => s.Iar.Enable);
            }

            [Test]
            public void AfterStep4EnableJumpRegisterInstructionRegBIsTrue()
            {
                _sut.Ir.Inputs.InstructionWire(2).Value = true;
                _sut.Ir.Inputs.InstructionWire(3).Value = true;
                _sut.Ir.SetRegisterValue();

                const int regBPos = 1;
                SetupRegPositions(0, regBPos);

                PerformStep();

                VerifyRegEnable(true, regBPos);
            }

            [Test]
            public void AfterStep4EnableJumpIarIsTrue()
            {
                _sut.Ir.Inputs.InstructionWire(1).Value = true;
                _sut.Ir.SetRegisterValue();

                PerformStep();

                VerifyWireShouldBeTrue(s => s.Iar.Enable);
            }

            [Test]
            public void AfterStep4EnableJumpIfBus1IsTrue()
            {
                _sut.Ir.Inputs.InstructionWire(1).Value = true;
                _sut.Ir.Inputs.InstructionWire(3).Value = true;
                _sut.Ir.SetRegisterValue();

                PerformStep();

                VerifyWireShouldBeTrue(s => s.Bus1.Bit);
            }

            [Test]
            public void AfterStep4EnableJumpIfIarIsTrue()
            {
                _sut.Ir.Inputs.InstructionWire(1).Value = true;
                _sut.Ir.Inputs.InstructionWire(3).Value = true;
                _sut.Ir.SetRegisterValue();

                PerformStep();

                VerifyWireShouldBeTrue(s => s.Iar.Enable);
            }

            [Test]
            public void AfterStep4EnableClearBus1IsTrue()
            {
                _sut.Ir.Inputs.InstructionWire(1).Value = true;
                _sut.Ir.Inputs.InstructionWire(2).Value = true;
                _sut.Ir.SetRegisterValue();

                PerformStep();

                VerifyWireShouldBeTrue(s => s.Bus1.Bit);
            }

            [Test]
            [TestCase(false)]
            [TestCase(true)]
            public void AfterStep4EnableIoRegBIsTrue(bool output)
            {
                _sut.Ir.Inputs.InstructionWire(1).Value = true;
                _sut.Ir.Inputs.InstructionWire(2).Value = true;
                _sut.Ir.Inputs.InstructionWire(3).Value = true;
                _sut.Ir.Inputs.InstructionWire(4).Value = output;
                _sut.Ir.SetRegisterValue();

                const int regBPos = 1;

                SetupRegBPositions(regBPos);

                PerformStep();

                VerifyRegEnable(output, regBPos);
            }
        }

        public class ComputerPinCentralProcessingUnitStep4SetTests : ComputerPinCentralProcessingUnitStep4Tests
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
                _sut.Ir.Inputs.InstructionWire(0).Value = aluFlag;
                _sut.Ir.SetRegisterValue();

                PerformStep();

                VerifyWireShouldBe(v => v.Tmp.Set, aluFlag);
            }

            [Test]
            [TestCase(true)]
            [TestCase(false)]
            public void AfterFourSetStepCarryInTmpSetIsTrue(bool aluFlag)
            {
                _sut.Ir.Inputs.InstructionWire(0).Value = aluFlag;
                _sut.Ir.SetRegisterValue();

                PerformStep();

                // TODO: CarryInTmp?
                VerifyWireShouldBe(v => v.Alu.CarryIn, aluFlag);
            }

            [Test]
            [TestCase(false, false)]
            [TestCase(false, true)]
            [TestCase(true, false)]
            [TestCase(true, true)]
            public void AfterFourSetMarIsSet(bool aluFlag, bool decoder)
            {
                _sut.Ir.Inputs.InstructionWire(0).Value = aluFlag;
                _sut.Ir.Inputs.InstructionWire(3).Value = decoder;
                _sut.Ir.SetRegisterValue();

                PerformStep();

                VerifyWireShouldBe(v => v.Ram.Mar.Set, !aluFlag);
            }

            [Test]
            public void AfterStep4SetDataInstructionMarIsTrue()
            {
                _sut.Ir.Inputs.InstructionWire(2).Value = true;
                _sut.Ir.SetRegisterValue();

                PerformStep();

                VerifyWireShouldBeTrue(v => v.Ram.Mar.Set);
            }

            [Test]
            public void AfterStep4SetDataInstructionAccIsTrue()
            {
                _sut.Ir.Inputs.InstructionWire(2).Value = true;
                _sut.Ir.SetRegisterValue();

                PerformStep();

                VerifyWireShouldBeTrue(v => v.Acc.Set);
            }

            [Test]
            public void AfterStep4SetJumpRegisterInstructionIarIsTrue()
            {
                _sut.Ir.Inputs.InstructionWire(2).Value = true;
                _sut.Ir.Inputs.InstructionWire(3).Value = true;
                _sut.Ir.SetRegisterValue();

                PerformStep();

                VerifyWireShouldBeTrue(v => v.Iar.Set);
            }

            [Test]
            public void AfterStep4SetJumpMarIsTrue()
            {
                _sut.Ir.Inputs.InstructionWire(1).Value = true;
                _sut.Ir.SetRegisterValue();

                PerformStep();

                VerifyWireShouldBeTrue(v => v.Ram.Mar.Set);
            }

            [Test]
            public void AfterStep4SetJumpIfMarIsTrue()
            {
                _sut.Ir.Inputs.InstructionWire(1).Value = true;
                _sut.Ir.Inputs.InstructionWire(3).Value = true;
                _sut.Ir.SetRegisterValue();

                PerformStep();

                VerifyWireShouldBeTrue(v => v.Ram.Mar.Set);
            }

            [Test]
            public void AfterStep4SetJumpIfAccIsTrue()
            {
                _sut.Ir.Inputs.InstructionWire(1).Value = true;
                _sut.Ir.Inputs.InstructionWire(3).Value = true;
                _sut.Ir.SetRegisterValue();

                PerformStep();

                VerifyWireShouldBeTrue(v => v.Acc.Set);
            }

            [Test]
            public void AfterStep4SetClearCaezFlagsIsTrue()
            {
                _sut.Ir.Inputs.InstructionWire(1).Value = true;
                _sut.Ir.Inputs.InstructionWire(2).Value = true;
                _sut.Ir.SetRegisterValue();

                PerformStep();

                VerifyWireShouldBeTrue(v => v.Caez.Set);
            }

            [Test]
            [TestCase(false)]
            [TestCase(true)]
            public void AfterStep4IoClkSIsTrue(bool output)
            {
                _sut.Ir.Inputs.InstructionWire(1).Value = true;
                _sut.Ir.Inputs.InstructionWire(2).Value = true;
                _sut.Ir.Inputs.InstructionWire(3).Value = true;
                _sut.Ir.Inputs.InstructionWire(4).Value = output;
                _sut.Ir.SetRegisterValue();

                PerformStep();

                VerifyWireShouldBe(s => s.IoBus.Clk.Set, output);
            }
        }
    }

    public class ComputerPinCentralProcessingUnitStep5Tests : ComputerPinTests
    {
        [SetUp]
        public void Step5SetUp()
        {
            // Step to previous step to the one being tested
            PerformFullStep(4);
        }

        public class ComputerPinCentralProcessingUnitStep5EnableTests : ComputerPinCentralProcessingUnitStep5Tests
        {
            [Test]
            [TestCase(true)]
            [TestCase(false)]
            public void AfterStep5RegAEnableIsTrue(bool aluFlag)
            {
                _sut.Ir.Inputs.InstructionWire(0).Value = aluFlag;
                _sut.Ir.SetRegisterValue();

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
                _sut.Ir.Inputs.InstructionWire(0).Value = aluFlag;
                _sut.Ir.Inputs.InstructionWire(3).Value = decoder;
                _sut.Ir.SetRegisterValue();

                const int regBPos = 1;
                SetupRegPositions(0, regBPos);

                PerformStep();

                VerifyWireShouldBe(s => s.Ram.Enable, ramSet);
                VerifyRegEnable(rbSet, regBPos);
            }

            [Test]
            public void AfterStep5EnableDataInstructionRamIsTrue()
            {
                _sut.Ir.Inputs.InstructionWire(2).Value = true;
                _sut.Ir.SetRegisterValue();

                PerformStep();

                VerifyWireShouldBeTrue(v => v.Ram.Enable);
            }

            [Test]
            public void AfterStep5EnableJumpRamIsTrue()
            {
                _sut.Ir.Inputs.InstructionWire(1).Value = true;
                _sut.Ir.SetRegisterValue();

                PerformStep();

                VerifyWireShouldBeTrue(v => v.Ram.Enable);
            }

            [Test]
            public void AfterStep5JumpIfEnableAccIsTrue()
            {
                _sut.Ir.Inputs.InstructionWire(1).Value = true;
                _sut.Ir.Inputs.InstructionWire(3).Value = true;
                _sut.Ir.SetRegisterValue();

                PerformStep();

                VerifyWireShouldBeTrue(v => v.Acc.Enable);
            }

            [Test]
            [TestCase(false)]
            [TestCase(true)]
            public void AfterStep5EnableIoClkEIsTrue(bool output)
            {
                _sut.Ir.Inputs.InstructionWire(1).Value = true;
                _sut.Ir.Inputs.InstructionWire(2).Value = true;
                _sut.Ir.Inputs.InstructionWire(3).Value = true;
                _sut.Ir.Inputs.InstructionWire(4).Value = output;
                _sut.Ir.SetRegisterValue();

                PerformStep();

                VerifyWireShouldBe(v => v.IoBus.Clk.Enable, !output);
            }

            // ALU Op

            [Test]
            // Not ALU operation no op code should be set (Add is 0 therefore default)
            [TestCase(0b01110000, OpCode.Add)]
            // Actual ALU operations
            [TestCase(0b10000000, OpCode.Add)]
            [TestCase(0b10010000, OpCode.Shr)]
            [TestCase(0b10100000, OpCode.Shl)]
            [TestCase(0b10110000, OpCode.Not)]
            [TestCase(0b11000000, OpCode.And)]
            [TestCase(0b11010000, OpCode.Or)]
            [TestCase(0b11100000, OpCode.XOr)]
            [TestCase(0b11110000, OpCode.Cmp)]
            public void CorrectOpCodeIsSetOnStep5(int instruction, OpCode expectedOpCode)
            {
                _sut.Ir.Inputs.SetValue(instruction.ToBinaryBools(8));
                _sut.Ir.SetRegisterValue();

                PerformStep();

                ((OpCode)_sut.Alu.Op.ToInt()).Should().Be(expectedOpCode);
            }
        }

        public class ComputerPinCentralProcessingUnitStep5SetTests : ComputerPinCentralProcessingUnitStep5Tests
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
                _sut.Ir.Inputs.InstructionWire(0).Value = aluFlag;
                _sut.Ir.SetRegisterValue();

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
                _sut.Ir.Inputs.InstructionWire(0).Value = aluFlag;
                _sut.Ir.Inputs.InstructionWire(3).Value = decoder;
                _sut.Ir.SetRegisterValue();

                const int regBPos = 1;
                SetupRegPositions(0, regBPos);

                PerformStep();

                VerifyWireShouldBe(v => v.Ram.Set, ramSet);
                VerifyRegSet(rbSet, regBPos);
            }

            [Test]
            public void AfterStep5SetDataInstructionRegBIsTrue()
            {
                _sut.Ir.Inputs.InstructionWire(2).Value = true;
                _sut.Ir.SetRegisterValue();

                const int regBPos = 1;
                SetupRegPositions(0, regBPos);

                PerformStep();

                VerifyRegSet(true, regBPos);
            }

            [Test]
            public void AfterStep5SetJumpIarIsTrue()
            {
                _sut.Ir.Inputs.InstructionWire(1).Value = true;
                _sut.Ir.SetRegisterValue();

                PerformStep();

                VerifyWireShouldBeTrue(v => v.Iar.Set);
            }

            [Test]
            public void AfterStep5JumpIfSetIarIsTrue()
            {
                _sut.Ir.Inputs.InstructionWire(1).Value = true;
                _sut.Ir.Inputs.InstructionWire(3).Value = true;
                _sut.Ir.SetRegisterValue();

                PerformStep();

                VerifyWireShouldBeTrue(v => v.Iar.Set);
            }

            [Test]
            public void AfterStep5IfAluOpFlagsSetIsTrue()
            {
                _sut.Ir.Inputs.InstructionWire(0).Value = true;
                _sut.Ir.SetRegisterValue();

                PerformStep();

                VerifyWireShouldBeTrue(v => v.Caez.Set);
            }

            [Test]
            [TestCase(false)]
            [TestCase(true)]
            public void AfterStep5SetIoRegBSetIsTrue(bool output)
            {
                _sut.Ir.Inputs.InstructionWire(1).Value = true;
                _sut.Ir.Inputs.InstructionWire(2).Value = true;
                _sut.Ir.Inputs.InstructionWire(3).Value = true;
                _sut.Ir.Inputs.InstructionWire(4).Value = output;
                _sut.Ir.SetRegisterValue();

                const int regBPos = 1;
                SetupRegBPositions(regBPos);

                PerformStep();

                VerifyRegSet(!output, regBPos);
            }
        }
    }

    public class ComputerPinCentralProcessingUnitStep6Tests : ComputerPinTests
    {
        [SetUp]
        public void Step6SetUp()
        {
            // Step to previous step to the one being tested
            PerformFullStep(5);
        }

        public class ComputerPinCentralProcessingUnitStep6EnableTests : ComputerPinCentralProcessingUnitStep6Tests
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
                _sut.Ir.Inputs.InstructionWire(0).Value = aluFlag;
                _sut.Ir.Inputs.InstructionWire(1).Value = ir1;
                _sut.Ir.Inputs.InstructionWire(2).Value = ir2;
                _sut.Ir.Inputs.InstructionWire(3).Value = ir3;
                _sut.Ir.SetRegisterValue();

                PerformStep();

                VerifyWireShouldBe(v => v.Acc.Enable, expected);
            }

            [Test]
            public void AfterStep6EnableDataInstructionAccIsTrue()
            {
                _sut.Ir.Inputs.InstructionWire(2).Value = true;
                _sut.Ir.SetRegisterValue();

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
                _sut.Ir.Inputs.InstructionWire(1).Value = true;
                _sut.Ir.Inputs.InstructionWire(3).Value = true;

                _sut.Ir.Inputs.InstructionWire(4).Value = cCheck;
                _sut.Ir.Inputs.InstructionWire(5).Value = aCheck;
                _sut.Ir.Inputs.InstructionWire(6).Value = eCheck;
                _sut.Ir.Inputs.InstructionWire(7).Value = zCheck;

                _sut.Ir.SetRegisterValue();

                // TODO: this seems sus as Caez inputs into CPU
                _sut.Caez.Inputs.C.Value = c;
                _sut.Caez.Inputs.A.Value = a;
                _sut.Caez.Inputs.E.Value = e;
                _sut.Caez.Inputs.Z.Value = z;
                _sut.Caez.SetRegisterValue();

                PerformStep();

                VerifyWireShouldBe(v => v.Ram.Enable, expected);
            }
        }

        public class ComputerPinCentralProcessingUnitStep6SetTests : ComputerPinCentralProcessingUnitStep6Tests
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
                _sut.Ir.Inputs.InstructionWire(0).Value = aluFlag;
                _sut.Ir.Inputs.InstructionWire(1).Value = ir1;
                _sut.Ir.Inputs.InstructionWire(2).Value = ir2;
                _sut.Ir.Inputs.InstructionWire(3).Value = ir3;
                _sut.Ir.SetRegisterValue();

                const int regBPos = 1;
                SetupRegPositions(0, regBPos);

                PerformStep();

                VerifyRegSet(expected, regBPos);
            }

            [Test]
            public void AfterStep6SetDataInstructionIarIsTrue()
            {
                _sut.Ir.Inputs.InstructionWire(2).Value = true;
                _sut.Ir.SetRegisterValue();

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
                _sut.Ir.Inputs.InstructionWire(1).Value = true;
                _sut.Ir.Inputs.InstructionWire(3).Value = true;

                _sut.Ir.Inputs.InstructionWire(4).Value = cCheck;
                _sut.Ir.Inputs.InstructionWire(5).Value = aCheck;
                _sut.Ir.Inputs.InstructionWire(6).Value = eCheck;
                _sut.Ir.Inputs.InstructionWire(7).Value = zCheck;
                _sut.Ir.SetRegisterValue();

                // TODO: this seems sus as Caez inputs into CPU
                _sut.Caez.Inputs.C.Value = c;
                _sut.Caez.Inputs.A.Value = a;
                _sut.Caez.Inputs.E.Value = e;
                _sut.Caez.Inputs.Z.Value = z;
                _sut.Caez.SetRegisterValue();

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

        _sut.Ir.Inputs.InstructionWire(5).Value = aBool[0];
        _sut.Ir.Inputs.InstructionWire(4).Value = aBool[1];

        _sut.Ir.SetRegisterValue();
    }

    private void SetupRegBPositions(int b)
    {
        var bBool = b.ToBinaryBools(2);

        _sut.Ir.Inputs.InstructionWire(7).Value = bBool[0];
        _sut.Ir.Inputs.InstructionWire(6).Value = bBool[1];

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