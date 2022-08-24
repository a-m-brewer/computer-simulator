using ComputerSimulator.Core;
using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ComputerSimulator;

public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }
    
    public void ConfigureServices(IServiceCollection services)
    {
        services.RegisterCoreServices(Configuration.GetSection("Computer").Get<ComputerSettings>());

        services.AddTransient<IComputer, Computer>();
        services.AddHostedService<ComputerService>();
    }
}