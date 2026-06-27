using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Parts;
using FluentAssertions;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests.Parts;

/// <summary>
/// Regression tests for multi-instruction sequencing. These exercise instruction *sequences*
/// (not single instructions in isolation) which previously broke because the Decoder left stale
/// outputs from the prior instruction, causing spurious register enables.
/// </summary>
public class CpuSequencingTests : IntegrationTestBase
{
    private IComputerPart _sut = null!;

    private void Load(params int[] program)
    {
        _sut = ComponentFactory.CreateComputerPart();
        for (var a = 0; a < program.Length; a++)
            _sut.Ram.GetSlot(a & 0xFF, a >> 8).Memory.SetRegisterValue(program[a].ToBinaryBools(8));
    }

    private void RunInstructions(int count)
    {
        for (var i = 0; i < count * 24; i++) _sut.Update();
    }

    private int Reg(int r) => _sut.GeneralPurposeRegisters[r].StoredValue.ToInt();

    [Test]
    public void AddAfterOutStillIncrements()
    {
        // DATA R0,1 ; DATA R3,5 ; OUT Data R3 ; ADD R0 R3   -> R3 should be 6
        Load(0x20, 0x01, 0x23, 0x05, 0x7B, 0x83);
        RunInstructions(4);
        Reg(3).Should().Be(6);
    }

    [Test]
    public void AddAfterStoreStillIncrements()
    {
        // DATA R0,1 ; DATA R3,5 ; ST R0 R1 ; ADD R0 R3   -> R3 should be 6
        Load(0x20, 0x01, 0x23, 0x05, 0x11, 0x83);
        RunInstructions(4);
        Reg(3).Should().Be(6);
    }

    [Test]
    public void JumpDoesNotCorruptRegisters()
    {
        // DATA R0,7 ; DATA R3,9 ; JMP 8 ; (halt at 8: JMP 8)
        // After the jump, R0/R3 must be unchanged (7 and 9).
        Load(0x20, 0x07, 0x23, 0x09, 0x40, 0x08, 0x00, 0x00, 0x40, 0x08);
        RunInstructions(6);
        Reg(0).Should().Be(7);
        Reg(3).Should().Be(9);
    }

    [Test]
    public void CountedLoopWithCompareTerminates()
    {
        // R3 counts 0..4 via ADD, CMP against R2=4, JE to halt.
        //  0: DATA R0,1
        //  2: DATA R2,4
        //  4: DATA R3,0
        //  6: ADD R0 R3      (R3++)
        //  7: CMP R3 R2      (sets Equal when R3==4)
        //  8: JE 12          (jump-if Equal, prefix 0101 + Equal select = 0x52)
        // 10: JMP 6
        // 12: JMP 12         (halt)
        Load(0x20, 0x01, 0x22, 0x04, 0x23, 0x00, 0x83, 0xFE, 0x52, 0x0C, 0x40, 0x06, 0x40, 0x0C);
        RunInstructions(40); // generous; loop is short
        Reg(3).Should().Be(4);
        _sut.Iar.StoredValue.ToInt().Should().Be(12); // parked on the halt
    }
}
