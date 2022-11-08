using ComputerSimulator.Core.Parts;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests.Parts;

[TestFixture]
public class DisplayRamTests : IntegrationTestBase
{
    private IDisplayRam _sut;

    [SetUp]
    public void SetUp()
    {
        _sut = ComponentFactory.CreateDisplayRam(
            WireFactory.CreateWire<bool>("set-mar-set"),
            WireFactory.CreateWire<bool>("enable-mar-set"),
            WireFactory.CreateGroup<bool>("set-mar-input-bus"),
            WireFactory.CreateGroup<bool>("enable-mar-input-bus"),
            WireFactory.CreateWire<bool>("set"),
            WireFactory.CreateWire<bool>("enable"),
            WireFactory.CreateGroup<bool>("input-bus"),
            WireFactory.CreateGroup<bool>("output-bus")
        );
    }

    [Test]
    public void CanStoreAValueFromTheInputBus()
    {
        
    }
    
    [Test]
    public void CanOutputAValueFromRam()
    {
        
    }
}