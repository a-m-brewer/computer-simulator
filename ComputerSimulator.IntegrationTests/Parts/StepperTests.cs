using FluentAssertions;
using FluentAssertions.Execution;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests.Parts;

public class StepperTests : IntegrationTestBase
{
    [Test]
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
    [TestCase(5)]
    [TestCase(6)]
    public void StepperWorksAsExpected(int onBit)
    {
        var stepper = ComponentFactory.CreateStepper(CreateTestWire(false, "clk"), CreateTestWireGroup(false, 7, "step"));

        for (var i = 0; i <= onBit; i++)
        {
            stepper.Clk.Value = true;
            stepper.Update();
            stepper.Clk.Value = false;
            stepper.Update();
        }

        using (new AssertionScope())
        {
            for (var i = 0; i < stepper.Steps.Count; i++)
            {
                if (i == onBit)
                {
                    continue;
                }

                stepper.Steps[i].Value.Should().BeFalse($"Stepper {i} should be false");
            }
        }
        
        stepper.Steps[onBit].Value.Should().BeTrue();
    }
}