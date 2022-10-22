using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Models;
using ComputerSimulator.Core.Parts;
using FluentAssertions;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests.Parts;

[TestFixture]
public class ComputerPartTests : IntegrationTestBase
{
    private IComputerPart _sut = null!;
    private bool[] _max = null!;
    private bool[] _min = null!;

    [SetUp]
    public void SetUp()
    {
        var computerSettings = GetRequiredService<ComputerSettings>();

        _max = computerSettings.WordSize.InitArray<bool>().Fill(true);
        _min = computerSettings.WordSize.InitArray<bool>().Fill(false);
        _sut = ComponentFactory.CreateComputerPart();
    }

    // Step 1

    [Test]
    public void MarIsSetToAddressInIarInStepOne()
    {
        _sut.Iar.SetRegisterValue(_max);

        PerformStep();

        _sut.Ram.Mar.StoredValue
            .Should()
            .AllBeEquivalentTo(true);
    }

    [Test]
    public void AfterStepOneAccIsIncrementOfIarAddress()
    {
        _sut.Iar.SetRegisterValue(_min);

        PerformStep();

        _sut.Acc.StoredValue
            .ToInt()
            .Should()
            .Be(1);
    }

    // Step 2

    [Test]
    public void AfterStep2ByteInRamIsInIr()
    {
        _sut.Iar.SetRegisterValue(_min);
        _sut.Ram.Slots[0][0].Memory.SetRegisterValue(_max);

        PerformFullStep(2);

        _sut.Ir.StoredValue
            .Should()
            .AllSatisfy(wire => wire.Value.Should().BeTrue());
    }

    // step 3

    [Test]
    public void AfterStep3IarIsIncrementedByOne()
    {
        _sut.Iar.SetRegisterValue(_min);

        PerformFullStep(3);

        _sut.Iar.StoredValue
            .ToInt()
            .Should()
            .Be(1);
    }

    // Full instructions

    // ADD R0 R1
    // 1 000 00 01
    [Test]
    public void CanAddTwoNumbersTogether()
    {
        const int expected = 75;

        var instruction = new[] { true, false, false, false, false, false, false, true };

        _sut.Ram.Slots[0][0].Memory
            .SetRegisterValue(instruction);

        _sut.GeneralPurposeRegisters[0].SetRegisterValue(CreateNumber(50));
        _sut.GeneralPurposeRegisters[1].SetRegisterValue(CreateNumber(25));

        PerformFullStep(6);

        _sut.GeneralPurposeRegisters[1]
            .StoredValue
            .ToInt()
            .Should()
            .Be(expected);
    }

    // SHL R0 R1
    // 1 001 00 01
    [Test]
    public void CanShiftLeft()
    {
        var instruction = new[] { true, false, false, true, false, false, false, true };

        _sut.Ram.Slots[0][0].Memory.SetRegisterValue(instruction);
        _sut.GeneralPurposeRegisters[0].SetRegisterValue(CreateNumber(128));

        PerformFullStep(6);

        var result = _sut.GeneralPurposeRegisters[1].StoredValue;

        result[6]
            .Value
            .Should()
            .BeTrue();
    }

    // SHR R0 R1
    [Test]
    public void CanShiftRight()
    {
        var instruction = new[] { true, false, true, false, false, false, false, true };

        _sut.Ram.Slots[0][0].Memory.SetRegisterValue(instruction);
        _sut.GeneralPurposeRegisters[0].SetRegisterValue(CreateNumber(1));

        PerformFullStep(6);

        _sut.GeneralPurposeRegisters[1].StoredValue[1]
            .Value
            .Should()
            .BeTrue();
    }

    // NOT R0 R1
    [Test]
    public void CanNotAByte()
    {
        var instruction = new[] { true, false, true, true, false, false, false, true };

        _sut.Ram.Slots[0][0].Memory.SetRegisterValue(instruction);
        _sut.GeneralPurposeRegisters[0].SetRegisterValue(_min);

        PerformFullStep(6);

        _sut.GeneralPurposeRegisters[1]
            .StoredValue
            .Should()
            .AllSatisfy(w => w.Value.Should().BeTrue());
    }

    // AND R0 R1
    [Test]
    public void CanAndByte()
    {
        var instruction = new[] { true, true, false, false, false, false, false, true };

        _sut.Ram.Slots[0][0].Memory.SetRegisterValue(instruction);
        _sut.GeneralPurposeRegisters[0].SetRegisterValue(_min);
        _sut.GeneralPurposeRegisters[1].SetRegisterValue(CreateNumber(255));

        PerformFullStep(6);

        _sut.GeneralPurposeRegisters[1]
            .StoredValue
            .Should()
            .AllSatisfy(w => w.Value.Should().BeFalse());
    }

    // Or R0 R1
    [Test]
    public void CanOrByte()
    {
        var instruction = new[] { true, true, false, true, false, false, false, true };

        _sut.Ram.Slots[0][0].Memory.SetRegisterValue(instruction);
        _sut.GeneralPurposeRegisters[0].SetRegisterValue(_min);
        _sut.GeneralPurposeRegisters[1].SetRegisterValue(CreateNumber(255));

        PerformFullStep(6);

        _sut.GeneralPurposeRegisters[1]
            .StoredValue
            .Should()
            .AllSatisfy(w => w.Value.Should().BeTrue());
    }


    // XOR R0 R1
    [Test]
    public void CanXorByte()
    {
        var instruction = new[] { true, true, true, false, false, false, false, true };

        _sut.Ram.Slots[0][0].Memory.SetRegisterValue(instruction);
        _sut.GeneralPurposeRegisters[0].SetRegisterValue(CreateNumber(255));
        _sut.GeneralPurposeRegisters[1].SetRegisterValue(CreateNumber(255));

        PerformFullStep(6);

        _sut.GeneralPurposeRegisters[1]
            .StoredValue
            .Should()
            .AllSatisfy(w => w.Value.Should().BeFalse());
    }

    [Test]
    public void CanCmpEqualBytes()
    {
        var instruction = new[] { true, true, true, true, false, false, false, true };

        _sut.Ram.Slots[0][0].Memory.SetRegisterValue(instruction);
        _sut.GeneralPurposeRegisters[0].SetRegisterValue(CreateNumber(255));
        _sut.GeneralPurposeRegisters[1].SetRegisterValue(CreateNumber(255));

        PerformFullStep(6);

        var result = _sut.Caez.StoredValue;

        result.C.Value.Should().BeFalse();
        result.A.Value.Should().BeTrue();
        result.E.Value.Should().BeFalse();
        // Tried to figure this out but I don't see how following the design this would correctly output false
        // with cmp operation.
        result.Z.Value.Should().BeFalse();
    }

    // LD R0 R1
    // Load RB from RAM Address RA
    [Test]
    public void CanLoadFromRam()
    {
        var instruction = new[] { false, false, false, false, false, false, false, true };

        _sut.Ram.Slots[0][0].Memory.SetRegisterValue(instruction);
        _sut.GeneralPurposeRegisters[0].SetRegisterValue(CreateNumber(1));
        _sut.Ram.Slots[0][1].Memory.SetRegisterValue(CreateNumber(255));

        PerformFullStep(6);

        var result = _sut.GeneralPurposeRegisters[1].StoredValue;

        result
            .Should()
            .AllSatisfy(w => w.Value.Should().BeTrue());
    }

    // ST R0 R1
    // Store RB to Ram address in RA
    [Test]
    public void CanStoreDataInRam()
    {
        var instruction = _byteFactory.Create(false, false, false, true, false, false, false, true);

        _sut.ComputerState.Ram.InternalRegisters[0][0].ApplyOnce(instruction);

        _sut.ComputerState.GeneralPurposeRegisters[0].ApplyOnce(_byteFactory.Create(1));
        _sut.ComputerState.GeneralPurposeRegisters[1].ApplyOnce(_byteFactory.Create(255));

        StepFull(6);

        var result = _sut.ComputerState.Ram.InternalRegisters[0][1].Data;

        Assert.IsTrue(result.All(a => a));
    }

    // DATA RB, xxxx xxxx
    // Load next ram byte into RB
    [Test]
    public void CanPerformDataInstruction()
    {
        var instruction = _byteFactory.Create(false, false, true, false, false, false, false, false);
        _sut.ComputerState.Ram.InternalRegisters[0][0].ApplyOnce(instruction);

        _sut.ComputerState.Ram.InternalRegisters[0][1].ApplyOnce(_byteFactory.Create(255));

        StepFull(6);

        var result = _sut.ComputerState.GeneralPurposeRegisters[0].Data;

        Assert.IsTrue(result.All(a => a));
    }

    // JMPR RB
    // Jump to address in RB
    [Test]
    public void CanPerformJumpRegisterInstruction()
    {
        var instruction = _byteFactory.Create(false, false, true, true, false, false, false, false);
        _sut.ComputerState.Ram.InternalRegisters[0][0].ApplyOnce(instruction);

        _sut.ComputerState.GeneralPurposeRegisters[0].ApplyOnce(_fullByte);

        StepFull(6);

        var result = _sut.ComputerState.Iar.Data;

        Assert.IsTrue(result.All(a => a));
    }

    // JMP Addr
    // Jump to location in next Ram Register
    [Test]
    public void CanPerformJumpInstruction()
    {
        var instruction = _byteFactory.Create(false, true, false, false, false, false, false, false);
        _sut.ComputerState.Ram.InternalRegisters[0][0].ApplyOnce(instruction);

        _sut.ComputerState.Ram.InternalRegisters[0][1].ApplyOnce(_fullByte);

        StepFull(6);

        var result = _sut.ComputerState.Iar.Data;

        Assert.IsTrue(result.All(a => a));
    }

    // JC Addr
    // Jump if Equal is on
    [Test]
    public void CanPerformJumpIfEqualIsOn()
    {
        var addInstruction = _byteFactory.Create(true, false, false, false, false, false, false, true);

        _sut.ComputerState.Ram.InternalRegisters[0][0].ApplyOnce(addInstruction);
        var instruction = _byteFactory.Create(false, true, false, true, false, false, true, false);
        _sut.ComputerState.Ram.InternalRegisters[0][1].ApplyOnce(instruction);
        _sut.ComputerState.Ram.InternalRegisters[0][2].ApplyOnce(_fullByte);

        _sut.ComputerState.GeneralPurposeRegisters[0].ApplyOnce(_byteFactory.Create(200));
        _sut.ComputerState.GeneralPurposeRegisters[1].ApplyOnce(_byteFactory.Create(200));

        StepFull(6);

        Step(1);

        StepFull(6);

        var result = _sut.ComputerState.Iar.Data;

        Assert.IsTrue(result.All(a => a));
    }

    // JC Addr
    // Jump if Carry is on
    [Test]
    public void CanPerformJumpIfCarryIsOn()
    {
        var addInstruction = _byteFactory.Create(true, false, false, false, false, false, false, true);

        _sut.ComputerState.Ram.InternalRegisters[0][0].ApplyOnce(addInstruction);
        var instruction = _byteFactory.Create(false, true, false, true, true, false, false, false);
        _sut.ComputerState.Ram.InternalRegisters[0][1].ApplyOnce(instruction);
        _sut.ComputerState.Ram.InternalRegisters[0][2].ApplyOnce(_fullByte);

        _sut.ComputerState.GeneralPurposeRegisters[0].ApplyOnce(_byteFactory.Create(200));
        _sut.ComputerState.GeneralPurposeRegisters[1].ApplyOnce(_byteFactory.Create(200));

        StepFull(6);

        Assert.IsTrue(_sut.ComputerState.Flags.Data.C);

        Step(1);

        StepFull(6);

        var result = _sut.ComputerState.Iar.Data;

        Assert.IsTrue(result.All(a => a));
    }

    [Test]
    public void CanClearFlags()
    {
        var addInstruction = _byteFactory.Create(true, false, false, false, false, false, false, true);
        _sut.ComputerState.Ram.InternalRegisters[0][0].ApplyOnce(addInstruction);

        var clearInstruction = _byteFactory.Create(false, true, true, false, false, false, false, false);
        _sut.ComputerState.Ram.InternalRegisters[0][1].ApplyOnce(clearInstruction);

        _sut.ComputerState.GeneralPurposeRegisters[0].ApplyOnce(_byteFactory.Create(200));
        _sut.ComputerState.GeneralPurposeRegisters[1].ApplyOnce(_byteFactory.Create(200));

        StepFull(6);

        Assert.IsTrue(_sut.ComputerState.Flags.Data.C);
        Assert.IsFalse(_sut.ComputerState.Flags.Data.A);
        Assert.IsTrue(_sut.ComputerState.Flags.Data.E);
        Assert.IsFalse(_sut.ComputerState.Flags.Data.Z);

        Step(1);

        StepFull(3);

        Assert.IsTrue(_sut.ComputerState.Bus1.Set);
        Step(1);
        Assert.IsTrue(_sut.ComputerState.Flags.Set);
        Step(2);

        StepFull(2);

        Assert.IsFalse(_sut.ComputerState.Flags.Data.C);
        Assert.IsFalse(_sut.ComputerState.Flags.Data.A);
        Assert.IsFalse(_sut.ComputerState.Flags.Data.E);
        Assert.IsFalse(_sut.ComputerState.Flags.Data.Z);
    }

    // TODO: Create instructions for IOBus pg. 149

    [Test]
    public void CanInputIoDataToRb()
    {
        _sut.ComputerState.Io.Bus.Data = new BusMessage<IByte> { Name = "FromIO", Data = _fullByte };
        var instruction = _byteFactory.Create(false, true, true, true, false, false, false, false);
        _sut.ComputerState.Ram.InternalRegisters[0][0].ApplyOnce(instruction);

        StepFull(6);

        var result = _sut.ComputerState.GeneralPurposeRegisters[0].Data;

        Assert.IsTrue(result.All(a => a));
    }

    [Test]
    public void CanInputIoAddressToRb()
    {
        _sut.ComputerState.Io.Bus.Data = new BusMessage<IByte> { Name = "FromIO", Data = _fullByte };
        var instruction = _byteFactory.Create(false, true, true, true, false, true, false, false);
        _sut.ComputerState.Ram.InternalRegisters[0][0].ApplyOnce(instruction);

        StepFull(6);

        var result = _sut.ComputerState.GeneralPurposeRegisters[0].Data;

        Assert.IsTrue(result.All(a => a));
    }

    [Test]
    public void CanOutputToIoAsData()
    {
        _sut.ComputerState.GeneralPurposeRegisters[0].ApplyOnce(_fullByte);
        var instruction = _byteFactory.Create(false, true, true, true, true, false, false, false);
        _sut.ComputerState.Ram.InternalRegisters[0][0].ApplyOnce(instruction);

        StepFull(6);

        var result = _sut.ComputerState.Io.Bus.Data.Data;

        Assert.IsTrue(result.All(a => a));
    }

    [Test]
    public void CanOutputToIoAsAddress()
    {
        _sut.ComputerState.GeneralPurposeRegisters[0].ApplyOnce(_fullByte);
        var instruction = _byteFactory.Create(false, true, true, true, true, true, false, false);
        _sut.ComputerState.Ram.InternalRegisters[0][0].ApplyOnce(instruction);

        StepFull(6);

        var result = _sut.ComputerState.Io.Bus.Data.Data;

        Assert.IsTrue(result.All(a => a));
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
}