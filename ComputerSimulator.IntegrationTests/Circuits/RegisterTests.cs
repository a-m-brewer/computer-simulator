using ComputerSimulator.Core.Circuits;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests.Circuits;

public class RegisterTests : IntegrationTestBase
{
    [Test]
    public void Set_AllowsValueToBeStored()
    {
        // Arrange

        var sut = GetRequiredService<IRegister>();
        sut.Set = CreateTestWire("register-set", false);
        sut.Enable = CreateTestWire("register-enable", false);
        sut.Inputs = CreateTestWireGroup("register-inputs", false);
        sut.Outputs = CreateTestWireGroup("register-outputs", false);
        
        var andOutput = GetWireById<bool>(GetInternalWireLabel(sut, "word-to-enabler"));
    }
}