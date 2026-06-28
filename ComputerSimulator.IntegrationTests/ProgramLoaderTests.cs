using System;
using System.IO;
using ComputerSimulator.Core;
using ComputerSimulator.Core.Extensions;
using FluentAssertions;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests;

public class ProgramLoaderTests : IntegrationTestBase
{
    [Test]
    public void ReadsRawBinaryImageFromDisk()
    {
        var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"program-{Guid.NewGuid():N}.bin");
        try
        {
            File.WriteAllBytes(path, [0x20, 0x01, 0x40, 0x00]);

            ProgramLoader.ReadBinaryImage(path)
                .Should()
                .Equal(0x20, 0x01, 0x40, 0x00);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Test]
    public void LoadsRawBinaryImageIntoRamFromAddressZero()
    {
        var ram = ComponentFactory.CreateComputerPart().Ram;

        ProgramLoader.Load(ram, new byte[] { 0x20, 0x7F, 0xA5 });

        ram.GetSlot(0, 0).Memory.StoredValue.ToInt().Should().Be(0x20);
        ram.GetSlot(1, 0).Memory.StoredValue.ToInt().Should().Be(0x7F);
        ram.GetSlot(2, 0).Memory.StoredValue.ToInt().Should().Be(0xA5);
    }

    [Test]
    public void LoadsRawBinaryImageAtStartAddress()
    {
        var ram = ComponentFactory.CreateComputerPart().Ram;

        ProgramLoader.Load(ram, new byte[] { 0x42 }, startAddress: 0x0102);

        ram.GetSlot(0x02, 0x01).Memory.StoredValue.ToInt().Should().Be(0x42);
    }
}
