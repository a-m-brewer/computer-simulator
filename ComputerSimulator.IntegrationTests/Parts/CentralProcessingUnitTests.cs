using System;
using ComputerSimulator.Core.Constants;
using ComputerSimulator.Core.Enums;
using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Instructions;
using ComputerSimulator.Core.Parts;
using FluentAssertions;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests.Parts;

[TestFixture]
public abstract class CentralProcessingUnitTests : IntegrationTestBase
{
    private ICentralProcessingUnit _sut = null!;

    // TODO: write General Purpose Register selector tests
    
    [SetUp]
    public void SetUp()
    {
        _sut = ComponentFactory.CreateCentralProcessingUnit(
            WireFactory.CreateWire<bool>("bus1"),
            WireFactory.CreateSetEnableWire<bool>("iar"),
            WireFactory.CreateSetEnableWire<bool>("ram"),
            WireFactory.CreateSetEnableWire<bool>("acc"),
            WireFactory.CreateSetEnableWire<bool>("ioClk"),
            WireFactory.CreateSetEnableWireGroup<bool>(WireConstants.ExpectedNumberOfGeneralPurposeRegisters,
                "general-purpose-registers"),
            WireFactory.CreateOp("op"),
            WireFactory.CreateWire<bool>("mar-set"),
            WireFactory.CreateWire<bool>("tmp-set"),
            WireFactory.CreateWire<bool>("ir-set"),
            WireFactory.CreateWire<bool>("flags-set"),
            WireFactory.CreateWire<bool>("carry-in-tmp"),
            WireFactory.CreateWire<bool>("io-input-output"),
            WireFactory.CreateWire<bool>("io-data-address"),
            WireFactory.CreateGroup<bool>("instruction-register"),
            WireFactory.CreateCaez<bool>("caez")
        );
    }

    public class CentralProcessingUnitStep1Tests : CentralProcessingUnitTests
    {
        [Test]
        public void IoInputOutputIsIr4()
        {
            _sut.InstructionRegister.InstructionWire(4).Value = true;

            PerformStep();
            
            VerifyWireShouldBeTrue(v => v.IoInputOutput);
        }
        
        [Test]
        public void IoDataAddressIsIr5()
        {
            _sut.InstructionRegister.InstructionWire(5).Value = true;

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

    public class CentralProcessingUnitStep2Tests : CentralProcessingUnitTests
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

    public class CentralProcessingUnitStep3Tests : CentralProcessingUnitTests
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

    public class CentralProcessingUnitStep4Tests : CentralProcessingUnitTests
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
                _sut.InstructionRegister.InstructionWire(0).Value = aluFlag;

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
                _sut.InstructionRegister.InstructionWire(0).Value = aluFlag;
                _sut.InstructionRegister.InstructionWire(3).Value = decoder;

                const int regAPos = 0;
                SetupRegPositions(regAPos, 1);

                PerformStep();

                VerifyRegEnable(!aluFlag, regAPos);
            }

            [Test]
            public void AfterStep4EnableDataInstructionBus1IsSet()
            {
                _sut.InstructionRegister.InstructionWire(2).Value = true;

                PerformStep();

                VerifyWireShouldBeTrue(s => s.Bus1);
            }

            [Test]
            public void AfterStep4EnableDataInstructionIarIsTrue()
            {
                _sut.InstructionRegister.InstructionWire(2).Value = true;

                PerformStep();

                VerifyWireShouldBeTrue(s => s.Iar.Enable);
            }

            [Test]
            public void AfterStep4EnableJumpRegisterInstructionRegBIsTrue()
            {
                _sut.InstructionRegister.InstructionWire(2).Value = true;
                _sut.InstructionRegister.InstructionWire(3).Value = true;

                const int regBPos = 1;
                SetupRegPositions(0, regBPos);

                PerformStep();

                VerifyRegEnable(true, regBPos);
            }

            [Test]
            public void AfterStep4EnableJumpIarIsTrue()
            {
                _sut.InstructionRegister.InstructionWire(1).Value = true;

                PerformStep();

                VerifyWireShouldBeTrue(s => s.Iar.Enable);
            }

            [Test]
            public void AfterStep4EnableJumpIfBus1IsTrue()
            {
                _sut.InstructionRegister.InstructionWire(1).Value = true;
                _sut.InstructionRegister.InstructionWire(3).Value = true;

                PerformStep();

                VerifyWireShouldBeTrue(s => s.Bus1);
            }

            [Test]
            public void AfterStep4EnableJumpIfIarIsTrue()
            {
                _sut.InstructionRegister.InstructionWire(1).Value = true;
                _sut.InstructionRegister.InstructionWire(3).Value = true;

                PerformStep();

                VerifyWireShouldBeTrue(s => s.Iar.Enable);
            }

            [Test]
            public void AfterStep4EnableClearBus1IsTrue()
            {
                _sut.InstructionRegister.InstructionWire(1).Value = true;
                _sut.InstructionRegister.InstructionWire(2).Value = true;

                PerformStep();

                VerifyWireShouldBeTrue(s => s.Bus1);
            }

            [Test]
            [TestCase(false)]
            [TestCase(true)]
            public void AfterStep4EnableIoRegBIsTrue(bool output)
            {
                _sut.InstructionRegister.InstructionWire(1).Value = true;
                _sut.InstructionRegister.InstructionWire(2).Value = true;
                _sut.InstructionRegister.InstructionWire(3).Value = true;
                _sut.InstructionRegister.InstructionWire(4).Value = output;

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
                _sut.InstructionRegister.InstructionWire(0).Value = aluFlag;

                PerformStep();

                VerifyWireShouldBe(v => v.TmpSet, aluFlag);
            }

            [Test]
            [TestCase(true)]
            [TestCase(false)]
            public void AfterFourSetStepCarryInTmpSetIsTrue(bool aluFlag)
            {
                _sut.InstructionRegister.InstructionWire(0).Value = aluFlag;

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
                _sut.InstructionRegister.InstructionWire(0).Value = aluFlag;
                _sut.InstructionRegister.InstructionWire(3).Value = decoder;

                PerformStep();

                VerifyWireShouldBe(v => v.MarSet, !aluFlag);
            }

            [Test]
            public void AfterStep4SetDataInstructionMarIsTrue()
            {
                _sut.InstructionRegister.InstructionWire(2).Value = true;

                PerformStep();

                VerifyWireShouldBeTrue(v => v.MarSet);
            }

            [Test]
            public void AfterStep4SetDataInstructionAccIsTrue()
            {
                _sut.InstructionRegister.InstructionWire(2).Value = true;

                PerformStep();

                VerifyWireShouldBeTrue(v => v.Acc.Set);
            }

            [Test]
            public void AfterStep4SetJumpRegisterInstructionIarIsTrue()
            {
                _sut.InstructionRegister.InstructionWire(2).Value = true;
                _sut.InstructionRegister.InstructionWire(3).Value = true;

                PerformStep();

                VerifyWireShouldBeTrue(v => v.Iar.Set);
            }

            [Test]
            public void AfterStep4SetJumpMarIsTrue()
            {
                _sut.InstructionRegister.InstructionWire(1).Value = true;

                PerformStep();

                VerifyWireShouldBeTrue(v => v.MarSet);
            }

            [Test]
            public void AfterStep4SetJumpIfMarIsTrue()
            {
                _sut.InstructionRegister.InstructionWire(1).Value = true;
                _sut.InstructionRegister.InstructionWire(3).Value = true;

                PerformStep();

                VerifyWireShouldBeTrue(v => v.MarSet);
            }

            [Test]
            public void AfterStep4SetJumpIfAccIsTrue()
            {
                _sut.InstructionRegister.InstructionWire(1).Value = true;
                _sut.InstructionRegister.InstructionWire(3).Value = true;

                PerformStep();

                VerifyWireShouldBeTrue(v => v.Acc.Set);
            }

            [Test]
            public void AfterStep4SetClearCaezFlagsIsTrue()
            {
                _sut.InstructionRegister.InstructionWire(1).Value = true;
                _sut.InstructionRegister.InstructionWire(2).Value = true;

                PerformStep();

                VerifyWireShouldBeTrue(v => v.FlagsSet);
            }

            [Test]
            [TestCase(false)]
            [TestCase(true)]
            public void AfterStep4IoClkSIsTrue(bool output)
            {
                _sut.InstructionRegister.InstructionWire(1).Value = true;
                _sut.InstructionRegister.InstructionWire(2).Value = true;
                _sut.InstructionRegister.InstructionWire(3).Value = true;
                _sut.InstructionRegister.InstructionWire(4).Value = output;

                PerformStep();

                VerifyWireShouldBe(s => s.IoClk.Set, output);
            }
        }
    }

    public class CentralProcessingUnitStep5Tests : CentralProcessingUnitTests
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
                _sut.InstructionRegister.InstructionWire(0).Value = aluFlag;

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
                _sut.InstructionRegister.InstructionWire(0).Value = aluFlag;
                _sut.InstructionRegister.InstructionWire(3).Value = decoder;

                const int regBPos = 1;
                SetupRegPositions(0, regBPos);
                
                PerformStep();

                VerifyWireShouldBe(s => s.Ram.Enable, ramSet);
                VerifyRegEnable(rbSet, regBPos);
            }

            [Test]
            public void AfterStep5EnableDataInstructionRamIsTrue()
            {
                _sut.InstructionRegister.InstructionWire(2).Value = true;

                PerformStep();

                VerifyWireShouldBeTrue(v => v.Ram.Enable);
            }

            [Test]
            public void AfterStep5EnableJumpRamIsTrue()
            {
                _sut.InstructionRegister.InstructionWire(1).Value = true;

                PerformStep();

                VerifyWireShouldBeTrue(v => v.Ram.Enable);
            }

            [Test]
            public void AfterStep5JumpIfEnableAccIsTrue()
            {
                _sut.InstructionRegister.InstructionWire(1).Value = true;
                _sut.InstructionRegister.InstructionWire(3).Value = true;

                PerformStep();

                VerifyWireShouldBeTrue(v => v.Acc.Enable);
            }

            [Test]
            [TestCase(false)]
            [TestCase(true)]
            public void AfterStep5EnableIoClkEIsTrue(bool output)
            {
                _sut.InstructionRegister.InstructionWire(1).Value = true;
                _sut.InstructionRegister.InstructionWire(2).Value = true;
                _sut.InstructionRegister.InstructionWire(3).Value = true;
                _sut.InstructionRegister.InstructionWire(4).Value = output;

                PerformStep();

                VerifyWireShouldBe(v => v.IoClk.Enable, !output);
            }

            // ALU Op

            private static readonly object[] OpCodeCases =
            [
                new object[] { InstructionSet.In(DataAddress.Data, 0), OpCode.Add },
                new object[] { InstructionSet.Add(0, 0), OpCode.Add },
                new object[] { InstructionSet.Shr(0, 0), OpCode.Shr },
                new object[] { InstructionSet.Shl(0, 0), OpCode.Shl },
                new object[] { InstructionSet.Not(0, 0), OpCode.Not },
                new object[] { InstructionSet.And(0, 0), OpCode.And },
                new object[] { InstructionSet.Or(0, 0), OpCode.Or },
                new object[] { InstructionSet.XOr(0, 0), OpCode.XOr },
                new object[] { InstructionSet.Cmp(0, 0), OpCode.Cmp }
            ];

            [Test]
            [TestCaseSource(nameof(OpCodeCases))]
            public void CorrectOpCodeIsSetOnStep5(int instruction, OpCode expectedOpCode)
            {
                _sut.InstructionRegister.SetValue(instruction.ToBinaryBools(8));

                PerformStep();

                ((OpCode)_sut.Op.ToInt()).Should().Be(expectedOpCode);
            }
            
            [Test]
            public void AddOperationOnlyRegAEnabled()
            {
                // TODO: this should be failing
                var instruction = InstructionSet.Add(0, 1).ToBinaryBools(8);

                _sut.InstructionRegister.SetValue(instruction);
                
                PerformStep();

                VerifyRegEnable(true, 0);
                VerifyRegEnable(false, 1);
                VerifyRegEnable(false, 2);
                VerifyRegEnable(false, 3);
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
                _sut.InstructionRegister.InstructionWire(0).Value = aluFlag;

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
                _sut.InstructionRegister.InstructionWire(0).Value = aluFlag;
                _sut.InstructionRegister.InstructionWire(3).Value = decoder;

                const int regBPos = 1;
                SetupRegPositions(0, regBPos);
                
                PerformStep();

                VerifyWireShouldBe(v => v.Ram.Set, ramSet);
                VerifyRegSet(rbSet, regBPos);
            }

            [Test]
            public void AfterStep5SetDataInstructionRegBIsTrue()
            {
                _sut.InstructionRegister.InstructionWire(2).Value = true;

                const int regBPos = 1;
                SetupRegPositions(0, regBPos);
                
                PerformStep();

                VerifyRegSet(true, regBPos);
            }

            [Test]
            public void AfterStep5SetJumpIarIsTrue()
            {
                _sut.InstructionRegister.InstructionWire(1).Value = true;

                PerformStep();

                VerifyWireShouldBeTrue(v => v.Iar.Set);
            }

            [Test]
            public void AfterStep5JumpIfSetIarIsTrue()
            {
                _sut.InstructionRegister.InstructionWire(1).Value = true;
                _sut.InstructionRegister.InstructionWire(3).Value = true;

                PerformStep();

                VerifyWireShouldBeTrue(v => v.Iar.Set);
            }

            [Test]
            public void AfterStep5IfAluOpFlagsSetIsTrue()
            {
                _sut.InstructionRegister.InstructionWire(0).Value = true;

                PerformStep();

                VerifyWireShouldBeTrue(v => v.FlagsSet);
            }

            [Test]
            [TestCase(false)]
            [TestCase(true)]
            public void AfterStep5SetIoRegBSetIsTrue(bool output)
            {
                _sut.InstructionRegister.InstructionWire(1).Value = true;
                _sut.InstructionRegister.InstructionWire(2).Value = true;
                _sut.InstructionRegister.InstructionWire(3).Value = true;
                _sut.InstructionRegister.InstructionWire(4).Value = output;

                const int regBPos = 1;
                SetupRegBPositions(regBPos);
                
                PerformStep();

                VerifyRegSet(!output, regBPos);
            }
        }
    }

    public class CentralProcessingUnitStep6Tests : CentralProcessingUnitTests
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
                _sut.InstructionRegister.InstructionWire(0).Value = aluFlag;
                _sut.InstructionRegister.InstructionWire(1).Value = ir1;
                _sut.InstructionRegister.InstructionWire(2).Value = ir2;
                _sut.InstructionRegister.InstructionWire(3).Value = ir3;

                PerformStep();

                VerifyWireShouldBe(v => v.Acc.Enable, expected);
            }

            [Test]
            public void AfterStep6EnableDataInstructionAccIsTrue()
            {
                _sut.InstructionRegister.InstructionWire(2).Value = true;

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
                _sut.InstructionRegister.InstructionWire(1).Value = true;
                _sut.InstructionRegister.InstructionWire(3).Value = true;

                _sut.InstructionRegister.InstructionWire(4).Value = cCheck;
                _sut.InstructionRegister.InstructionWire(5).Value = aCheck;
                _sut.InstructionRegister.InstructionWire(6).Value = eCheck;
                _sut.InstructionRegister.InstructionWire(7).Value = zCheck;

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
                _sut.InstructionRegister.InstructionWire(0).Value = aluFlag;
                _sut.InstructionRegister.InstructionWire(1).Value = ir1;
                _sut.InstructionRegister.InstructionWire(2).Value = ir2;
                _sut.InstructionRegister.InstructionWire(3).Value = ir3;

                const int regBPos = 1;
                SetupRegPositions(0, regBPos);
                
                PerformStep();

                VerifyRegSet(expected, regBPos);
            }

            [Test]
            public void AfterStep6SetDataInstructionIarIsTrue()
            {
                _sut.InstructionRegister.InstructionWire(2).Value = true;

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
                _sut.InstructionRegister.InstructionWire(1).Value = true;
                _sut.InstructionRegister.InstructionWire(3).Value = true;

                _sut.InstructionRegister.InstructionWire(4).Value = cCheck;
                _sut.InstructionRegister.InstructionWire(5).Value = aCheck;
                _sut.InstructionRegister.InstructionWire(6).Value = eCheck;
                _sut.InstructionRegister.InstructionWire(7).Value = zCheck;

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

    private void VerifyWireShouldBeTrue(Func<ICentralProcessingUnit, IWire<bool>> selector)
    {
        VerifyWireShouldBe(selector, true);
    }

    private void VerifyWireShouldBeFalse(Func<ICentralProcessingUnit, IWire<bool>> selector)
    {
        VerifyWireShouldBe(selector, false);
    }

    private void VerifyWireShouldBe(Func<ICentralProcessingUnit, IWire<bool>> selector, bool expected)
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

        _sut.InstructionRegister.InstructionWire(5).Value = aBool[0];
        _sut.InstructionRegister.InstructionWire(4).Value = aBool[1];
    }
    
    private void SetupRegBPositions(int b)
    {
        var bBool = b.ToBinaryBools(2);

        _sut.InstructionRegister.InstructionWire(7).Value = bBool[0];
        _sut.InstructionRegister.InstructionWire(6).Value = bBool[1];
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
