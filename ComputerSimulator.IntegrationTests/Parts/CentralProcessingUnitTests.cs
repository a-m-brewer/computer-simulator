using ComputerSimulator.Core.Constants;
using ComputerSimulator.Core.Parts;
using FluentAssertions;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests.Parts;

[TestFixture]
public abstract class CentralProcessingUnitTests : IntegrationTestBase
{
    private ICentralProcessingUnit _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = ComponentFactory.CreateCentralProcessingUnit(
            CreateTestWire(false, "bus1"),
            CreateTestWire(false, "ram-enable"),
            CreateTestWire(false, "acc-enable"),
            CreateTestWireGroup(false, WireConstants.ExpectedNumberOfGeneralPurposeRegisters,
                "general-purpose-register-enable"),
            CreateTestOp("op"),
            CreateTestWire(false, "mar-set"),
            CreateTestWire(false, "acc-set"),
            CreateTestWire(false, "ram-set"),
            CreateTestWire(false, "tmp-set"),
            CreateTestWireGroup(false, WireConstants.ExpectedNumberOfGeneralPurposeRegisters,
                "general-purpose-register-set")
        );
    }

    public class CentralProcessingUnitStep1Tests : CentralProcessingUnitTests
    {
        public class CentralProcessingUnitStep1EnableTests : CentralProcessingUnitStep1Tests
        {
            [SetUp]
            public void Step1EnableSetUp()
            {
                PerformStep();
            }
        }
        
        public class CentralProcessingUnitStep1SetTests : CentralProcessingUnitStep1Tests
        {
            [SetUp]
            public void Step1SetSetUp()
            {
                PerformStep(2);
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
        }
        
        public class CentralProcessingUnitStep2SetTests : CentralProcessingUnitStep2Tests
        {
            [SetUp]
            public void Step2SetSetUp()
            {
                PerformStep(2);
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
        }
        
        public class CentralProcessingUnitStep3SetTests : CentralProcessingUnitStep3Tests
        {
            [SetUp]
            public void Step3SetSetUp()
            {
                PerformStep(2);
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
            [SetUp]
            public void Step4EnableSetUp()
            {
                PerformStep();
            }
            
            [Test]
            public void R1EnableIsTrue()
            {
                // Assert
                _sut.GeneralPurposeRegistersEnable[1].Value.Should().BeTrue();
            }
        }
        
        public class CentralProcessingUnitStep4SetTests : CentralProcessingUnitStep4Tests
        {
            [SetUp]
            public void Step4SetSetUp()
            {
                PerformStep(2);
            }
            
            [Test]
            public void TmpSetIsTrue()
            {
                // Assert
                _sut.TmpSet.Value.Should().BeTrue();
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
            [SetUp]
            public void Step5EnableSetUp()
            {
                PerformStep();
            }

            [Test]
            public void R0EnableIsTrue()
            {
                _sut.GeneralPurposeRegistersEnable[0].Value.Should().BeTrue();
            }
        }
        
        public class CentralProcessingUnitStep5SetTests : CentralProcessingUnitStep5Tests
        {
            [SetUp]
            public void Step5SetSetUp()
            {
                PerformStep(2);
            }

            [Test]
            public void AccSetIsTrue()
            {
                _sut.AccSet.Value.Should().BeTrue();
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
            [SetUp]
            public void Step6EnableSetUp()
            {
                PerformStep();
            }

            [Test]
            public void AccEnableIsTrue()
            {
                _sut.AccEnable.Value.Should().BeTrue();
            }
        }
        
        public class CentralProcessingUnitStep6SetTests : CentralProcessingUnitStep6Tests
        {
            [SetUp]
            public void Step6SetSetUp()
            {
                PerformStep(2);
            }

            [Test]
            public void R0SetIsTrue()
            {
                _sut.GeneralPurposeRegistersSet[0].Value.Should().BeTrue();
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
}