using ComputerSimulator.Core.Circuits;
using FluentAssertions;
using FluentAssertions.Execution;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests.Circuits;

public class RegisterTests : IntegrationTestBase
{
    [Test]
    public void Set_AllowsValueToBeStored()
    {
        // Arrange
        var sut = GetRequiredService<IRegister>();
        sut.Set = CreateTestWire("register-set", false);
        sut.Enable = CreateTestWire("register-enable", false);
        sut.Inputs = CreateTestWireGroup("register-inputs", false);
        sut.Outputs = CreateTestWireGroup("register-outputs", false);
        
        // Act
        sut.Enable.Value = false;
        sut.Set.Value = true;

        foreach (var input in sut.Inputs)
        {
            input.Value = true;
        }
        
        // Assert
        var andOutputGroup = GetGroupByLabel<bool>(GetInternalWireLabel(sut, "word-to-enabler"));

        using (new AssertionScope())
        {
            foreach (var andOutput in andOutputGroup)
            {
                andOutput.Value.Should().BeTrue();
            }
        }

        using (new AssertionScope())
        {
            foreach (var registerOutput in sut.Outputs)
            {
                registerOutput.Value.Should().BeFalse();
            }
        }
    }
    
    [Test]
    public void Enable_AllowsValueToBePassedOn()
    {
        // Arrange
        var sut = GetRequiredService<IRegister>();
        sut.Set = CreateTestWire("register-set", false);
        sut.Enable = CreateTestWire("register-enable", false);
        sut.Inputs = CreateTestWireGroup("register-inputs", false);
        sut.Outputs = CreateTestWireGroup("register-outputs", false);
        
        // Act
        sut.Enable.Value = false;
        sut.Set.Value = true;

        foreach (var input in sut.Inputs)
        {
            input.Value = true;
        }

        using (new AssertionScope())
        {
            foreach (var registerOutput in sut.Outputs)
            {
                registerOutput.Value.Should().BeFalse();
            }
        }

        sut.Enable.Value = true;
        
        // Assert
        using (new AssertionScope())
        {
            foreach (var registerOutput in sut.Outputs)
            {
                registerOutput.Value.Should().BeTrue();
            }
        }
    }
}