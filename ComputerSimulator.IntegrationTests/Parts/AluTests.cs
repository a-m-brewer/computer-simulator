using ComputerSimulator.Core.Enums;
using ComputerSimulator.Core.Extensions;
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

    [Test]
    [TestCase(OpCode.Shr, 0x21)]
    [TestCase(OpCode.Shl, 0x84)]
    public void CanShift(OpCode opCode, int expected)
    {
        const int input = 0x42;
        var inputBools = input.ToBinaryBools(ComputerSettings.WordSize);
        
        // Act
        for (var i = 0; i < _sut.InputsA.Count; i++)
        {
            _sut.InputsA[i].Value = inputBools[i];
        }

        _sut.Op.SetOpCode(opCode);
        
        _sut.Update();
        
        // Assert

        _sut.Outputs
            .ToInt()
            .Should()
            .Be(expected);
    }
    
    [Test]
    public void CanNoter()
    {
        const OpCode opCode = OpCode.Not;
        
        // Act
        foreach (var aWire in _sut.InputsA)
        {
            aWire.Value = false;
        }
        
        foreach (var bWire in _sut.InputsB)
        {
            bWire.Value = false;
        }

        _sut.Op.SetOpCode(opCode);
        
        _sut.Update();
        
        // Assert
        using (new AssertionScope())
        {
            foreach (var sumWire in _sut.Outputs)
            {
                sumWire.Value.Should().BeTrue();
            }
        }
    }
    
    [Test]
    public void CanAnder()
    {
        const OpCode opCode = OpCode.And;
        
        // Act
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
        using (new AssertionScope())
        {
            foreach (var sumWire in _sut.Outputs)
            {
                sumWire.Value.Should().BeTrue();
            }
        }
    }
    
    [Test]
    [TestCase(OpCode.Or)]
    [TestCase(OpCode.XOr)]
    public void CanOr(OpCode opCode)
    {
        // Act
        foreach (var aWire in _sut.InputsA)
        {
            aWire.Value = true;
        }
        
        foreach (var bWire in _sut.InputsB)
        {
            bWire.Value = false;
        }

        _sut.Op.SetOpCode(opCode);
        
        _sut.Update();
        
        // Assert
        using (new AssertionScope())
        {
            foreach (var sumWire in _sut.Outputs)
            {
                sumWire.Value.Should().BeTrue();
            }
        }
    }
    
    [Test]
    [TestCase(0xFFFF, 0xFFFF, false, true)]
    [TestCase(0xFFFE, 0xFFFF, false, false)]
    [TestCase(0xFFFF, 0xFFFE, true, false)]
    public void CanCmp(int a, int b, bool aLarger, bool equal)
    {
        const OpCode opCode = OpCode.Cmp;
        var inputABools = a.ToBinaryBools(ComputerSettings.WordSize);
        var inputBBools = b.ToBinaryBools(ComputerSettings.WordSize);
        
        // Act
        for (var i = 0; i < ComputerSettings.WordSize; i++)
        {
            _sut.InputsA[i].Value = inputABools[i];
            _sut.InputsB[i].Value = inputBBools[i];
        }
        
        _sut.Op.SetOpCode(opCode);
        
        _sut.Update();

        // Assert
        _sut.Equal.Value.Should().Be(equal);

        using (new AssertionScope())
        {
            for (var i = 0; i < ComputerSettings.WordSize; i++)
            {
                _sut.Outputs[i].Value.Should().Be(inputABools[i] != inputBBools[i]);
            }
        }

        _sut.ALarger.Value.Should().Be(aLarger);
    }
    
    [Test]
    public void CanCheckIsZero()
    {
        const OpCode opCode = OpCode.And;
        
        // Act
        foreach (var aWire in _sut.InputsA)
        {
            aWire.Value = true;
        }
        
        foreach (var bWire in _sut.InputsB)
        {
            bWire.Value = false;
        }

        _sut.Op.SetOpCode(opCode);
        
        _sut.Update();
        
        // Assert
        _sut.IsZero.Value.Should().BeTrue();
    }
}