using ComputerSimulator.Core.Enums;
using ComputerSimulator.Core.Parts;
using FluentAssertions;
using FluentAssertions.Execution;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests.Parts;

public class AluTests : IntegrationTestBase
{
    private IArithmeticLogicUnit _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = ComponentFactory.CreateArithmeticLogicUnit(
            CreateTestWireGroup(false),
            CreateTestWireGroup(false),
            CreateTestWire(false),
            CreateTestOp(),
            CreateTestWireGroup(false),
            CreateTestWire(false),
            CreateTestWire(false),
            CreateTestWire(false),
            CreateTestWire(false));
    }

    [Test]
    public void CanAdd()
    {
        const OpCode opCode = OpCode.Add;
        
        // Act
        _sut.CarryIn.Value = true;
        
        foreach (var aWire in _sut.InputsA)
        {
            aWire.Value = true;
        }
        
        foreach (var bWire in _sut.InputsB)
        {
            bWire.Value = true;
        }

        _sut.Op.SetOpCode(opCode);
        
        _sut.Update();
        
        // Assert
        _sut.CarryOut.Value.Should().BeTrue();
        
        using (new AssertionScope())
        {
            foreach (var sumWire in _sut.Outputs)
            {
                sumWire.Value.Should().BeTrue();
            }
        }
    }
}