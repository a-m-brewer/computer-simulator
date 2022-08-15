using System;
using System.Diagnostics;
using ComputerSimulator.Core.Extensions;
using FluentAssertions;
using FluentAssertions.Execution;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests.Parts;

public class RamTests : IntegrationTestBase
{
    [Test]
    // [Ignore("ctor time WIP")]
    public void Ram_CanUpdateBus()
    {
        // Arrange
        
        // Max address available in RAM
        const int ramAddress = 0xFFFF;
        var ramAddressInBools = ramAddress.ToBinaryBools(ComputerSettings.WordSize);

        var sw = Stopwatch.StartNew();
        var sut = ComponentFactory.CreateRam(
            CreateTestWire(false),
            CreateTestBus(),
            CreateTestWire(false),
            CreateTestWire(false),
            CreateTestBus());
        Console.WriteLine($"Ram took {sw.ElapsedMilliseconds}ms to construct");

        var startUpdate = sw.ElapsedMilliseconds;
        // Put address into MAR bus
        for (var i = 0; i < ramAddressInBools.Length; i++)
        {
            sut.MarInputBus[i].Value = ramAddressInBools[i];
        }
        
        // Set that address in the MAR
        sut.MarSet.Value = true;
        // Stop any new values coming in
        sut.MarSet.Value = false;
        
        // Value to store will be all true for test purposes
        const int valueToStoreInRam = 0xFFFF;
        var valueToStoreInRamInBools = valueToStoreInRam.ToBinaryBools(ComputerSettings.WordSize);
        
        // Put the value we want to store onto the bus
        for (var i = 0; i < valueToStoreInRamInBools.Length; i++)
        {
            sut.Io[i].Value = valueToStoreInRamInBools[i];
        }
        
        // Store the value in RAM
        sut.Set.Value = true;
        // Stop any new values being stored.
        sut.Set.Value = false;
        
        Console.WriteLine($"Ram took {sw.ElapsedMilliseconds - startUpdate}ms to run");
        
        // Clear the bus
        for (var i = 0; i < ComputerSettings.WordSize; i++)
        {
            sut.Io[i].Value = false;
        }
        
        // Double check the bus is empty so the test is valid
        using (new AssertionScope())
        {
            for (var i = 0; i < sut.Io.Count; i++)
            {
                sut.Io[i].Value.Should().BeFalse();
            }
        }
        
        // Enable the ram so it puts the value we stored onto the bus
        sut.Enable.Value = true;
        // Turn it off again
        sut.Enable.Value = false;
        
        // Assert the value has been retrieved
        using (new AssertionScope())
        {
            for (var i = 0; i < sut.Io.Count; i++)
            {
                sut.Io[i].Value.Should().BeTrue();
            }
        }
    }
}