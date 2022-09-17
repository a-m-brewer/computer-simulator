using ComputerSimulator.Core.Parts;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests.Parts;

public class ComputerPartTests : IntegrationTestBase
{
    private IComputerPart _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = ComponentFactory.CreateComputerPart();
    }
}