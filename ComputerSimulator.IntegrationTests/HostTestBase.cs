using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests;

public class HostTestBase
{
    private IHost _host = null!;
    private IServiceScope _scope = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _host = Array.Empty<string>().BuildHost();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _host.Dispose();
    }

    [SetUp]
    public void HostTestBaseSetup()
    {
        _scope = _host.Services.CreateScope();
    }

    [TearDown]
    public void HostTestBaseTearDown()
    {
        _scope.Dispose();
    }

    protected T GetRequiredService<T>() where T : notnull
    {
        return _scope.ServiceProvider.GetRequiredService<T>();
    }
}