using Moq;
using Moq.AutoMock;
using NUnit.Framework;

namespace ComputerSimulator.TestUtilities;

public class MockBase<T> where T : class
{
    private AutoMocker Mocker { get; set; }

    [SetUp]
    public void BaseSetUp()
    {
        Mocker = new AutoMocker();
    }

    protected T CreateSut()
    {
        return Mocker.CreateInstance<T>();
    }
    
    protected Mock<TMock> GetMock<TMock>() where TMock : class
    {
        return Mocker.GetMock<TMock>();
    }
}