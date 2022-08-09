using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests;

public class HostTestBase
{
    protected IHost Host = null!;
    
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        Host = Array.Empty<string>().BuildHost();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        Host.Dispose();
    }

    protected T GetRequiredService<T>() where T : notnull
    {
        return Host.Services.GetRequiredService<T>();
    }
}