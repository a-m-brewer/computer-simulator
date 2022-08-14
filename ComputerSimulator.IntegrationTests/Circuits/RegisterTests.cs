using ComputerSimulator.Core.Parts;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests.Circuits;

public class RegisterTests : IntegrationTestBase
{
    [Test]
    public void Set_AllowsValueToBeStored()
    {
        // Arrange
        var sut = ComponentFactory.CreateRegister(
            CreateTestWire(false),
            CreateTestWire(false),
            CreateTestWireGroup(false),
            CreateTestWireGroup(false));
        
        // Act
        sut.Enable.Value = false;
        sut.Set.Value = true;

        for (var i = 0; i < sut.Inputs.Count; i++)
        {
            sut.Inputs.SetValue(i, true);
        }
        
        // Assert
        var andOutputGroup = new Mock<IWireGroup<bool>>().Object;

        using (new AssertionScope())
        {
            for (var i = 0; i < andOutputGroup.Count; i++)
            {
                andOutputGroup.GetValue(i).Should().BeTrue();
            }
        }

        using (new AssertionScope())
        {
            for (var i = 0; i < sut.Outputs.Count; i++)
            {
                sut.Outputs.GetValue(i).Should().BeFalse();
            }
        }
    }
    
    [Test]
    public void Enable_AllowsValueToBePassedOn()
    {
        // Arrange
        var sut = ComponentFactory.CreateRegister(
            CreateTestWire(false),
            CreateTestWire(false),
            CreateTestWireGroup(false),
            CreateTestWireGroup(false));
        
        // Act
        sut.Enable.Value = false;
        sut.Set.Value = true;

        for (var i = 0; i < sut.Inputs.Count; i++)
        {
            sut.Inputs.SetValue(i, true);
        }

        using (new AssertionScope())
        {
            for (var i = 0; i < sut.Outputs.Count; i++)
            {
                sut.Outputs.GetValue(i).Should().BeFalse();
            }
        }

        sut.Enable.Value = true;
        
        // Assert
        using (new AssertionScope())
        {
            for (var i = 0; i < sut.Outputs.Count; i++)
            {
                sut.Outputs.GetValue(i).Should().BeTrue();
            }
        }
    }
}