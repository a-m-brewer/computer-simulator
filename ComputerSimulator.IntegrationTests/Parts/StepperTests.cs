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
            CreateTestWire(false, "clk"),
            CreateTestWire(false, "reset"),
            CreateTestWireGroup(false, 7, "step"));

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

    [Test]
    public void CanResetTo0()
    {
        var stepper = ComponentFactory.CreateStepper(
            CreateTestWire(false, "clk"),
            CreateTestWire(false, "reset"),
            CreateTestWireGroup(false, 7, "step"));

        for (var i = 0; i < 8; i++)
        {
            stepper.Clk.Value = true;
            stepper.Update();
            stepper.Clk.Value = false;
            stepper.Update();
        }

        stepper[]
        
        var output = stepperOutput;
        var rest = stepperOutput.Where(w => output.IndexOf(w) != 6);
        var restOn = rest.Any(w => w);
        Assert.IsFalse(restOn);

        stepper.Clk.Value = true;
        stepper.Reset.Value = true;
        stepper.Update();

        Assert.IsTrue(stepperOutput[0]);
        rest = stepperOutput.Where(w => stepperOutput.IndexOf(w) != 0);
        restOn = rest.Any(w => w);
        Assert.IsFalse(restOn);
    }

    [Test]
    public void InfiniteStepper()
    {
        var stepper = TestUtils.CreateStepper();

        var stepperOutput = new StepperOutput();

        for (var i = 0; i < 8; i++)
        {
            stepper.Step(true, false);
            stepperOutput = stepper.Step(false, false);
        }

        Assert.IsTrue(stepperOutput[6]);
        var output = stepperOutput;
        var rest = stepperOutput.Where(w => output.IndexOf(w) != 6);
        var restOn = rest.Any(w => w);
        Assert.IsFalse(restOn);

        stepperOutput = stepper.Step(true);

        Assert.IsTrue(stepperOutput[0]);
        rest = stepperOutput.Where(w => stepperOutput.IndexOf(w) != 0);
        restOn = rest.Any(w => w);
        Assert.IsFalse(restOn);
    }

    [Test]
    public void StaysAtIndex7IfNotReset()
    {
        var stepper = TestUtils.CreateStepper();

        var stepperOutput = new StepperOutput();

        for (var i = 0; i < 8; i++)
        {
            stepper.Step(true, false);
            stepperOutput = stepper.Step(false, false);
        }

        Assert.IsTrue(stepperOutput[6]);
        var rest = stepperOutput.Where(w => stepperOutput.IndexOf(w) != 6);
        var restOn = rest.Any(w => w);
        Assert.IsFalse(restOn);
    }
}