using ComputerSimulator.Core.Parts;
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
        var stepper = ComponentFactory.CreateStepper(
            CreateTestWire<bool>("clk"),
            CreateTestWire<bool>("reset"),
            CreateTestWireGroup<bool>(7, "step"));

        for (var i = 0; i <= onBit; i++)
        {
            stepper.Clk.Value = true;
            stepper.Update();
            stepper.Clk.Value = false;
            stepper.Update();
        }

        VerifyStep(stepper, onBit);
    }

    [Test]
    public void CanResetTo0()
    {
        var stepper = ComponentFactory.CreateStepper(
            CreateTestWire<bool>("clk"),
            CreateTestWire<bool>("reset"),
            CreateTestWireGroup<bool>(7, "step"));

        for (var i = 0; i <= 6; i++)
        {
            stepper.Clk.Value = true;
            stepper.Update();
            stepper.Clk.Value = false;
            stepper.Update();
        }

        VerifyStep(stepper, 6);

        stepper.Clk.Value = true;
        stepper.Reset.Value = true;
        stepper.Update();

        VerifyStep(stepper, 0);
    }

    [Test]
    public void StaysAtIndex7IfNotReset()
    {
        var stepper = ComponentFactory.CreateStepper(
            CreateTestWire<bool>("clk"),
            CreateTestWire<bool>("reset"),
            CreateTestWireGroup<bool>(7, "step"));

        for (var i = 0; i <= 8; i++)
        {
            stepper.Clk.Value = true;
            stepper.Update();
            stepper.Clk.Value = false;
            stepper.Update();
        }

        VerifyStep(stepper, 6);
        
        stepper.Clk.Value = true;
        stepper.Update();
        
        VerifyStep(stepper, 6);
    }

    private static void VerifyStep(IStepper stepper, int onBit)
    {
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