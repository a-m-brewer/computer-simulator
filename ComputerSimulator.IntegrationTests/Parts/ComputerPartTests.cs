using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Models;
using ComputerSimulator.Core.Parts;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests.Parts;

[TestFixture]
public class ComputerPartTests : IntegrationTestBase
{
    private IComputerPart _sut = null!;
    private bool[] _max;

    [SetUp]
    public void SetUp()
    {
        var computerSettings = GetRequiredService<ComputerSettings>();
        
        _max = computerSettings.WordSize.InitArray<bool>().Fill(true);
        _sut = ComponentFactory.CreateComputerPart();
    }
    
    [Test]
    public void MarIsSetToAddressInIarInStepOne()
    {
        _sut.Iar.SetRegisterValue();
            
        PerformStep();
            
        var result = _sut..Ram.MemoryAddressRegister.Data;
            
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
}