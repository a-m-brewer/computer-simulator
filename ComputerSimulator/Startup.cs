using ComputerSimulator.Core;
using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Models;
using ComputerSimulator.Core.Peripherals.Display;
using ComputerSimulator.Graphics;
using ComputerSimulator.Tui;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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
        services.AddOptions<ComputerSettings>()
            .Bind(Configuration.GetSection("Computer"))
            .Validate(settings =>
            {
                settings.Validate();
                return true;
            }, "Invalid computer settings")
            .ValidateOnStart();

        services.AddOptions<TerminalSettings>()
            .Bind(Configuration.GetSection("Terminal"))
            .ValidateOnStart();

        services.AddSingleton(sp => sp.GetRequiredService<IOptions<ComputerSettings>>().Value);
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<TerminalSettings>>().Value);

        services.RegisterCoreServices();

        services.AddSingleton<ITerminalLogSink, TerminalLogSink>();
        services.AddSingleton<ILoggerProvider, TerminalLoggerProvider>();
        services.AddSingleton<TerminalDisplayBuffer>();
        services.AddSingleton<ITerminalGuiApplication, TerminalGuiApplication>();
        services.AddSingleton<IDisplayOutput, TerminalGuiDisplayOutput>();

        services.AddTransient<IComputer, Computer>();
        services.AddHostedService<TerminalGuiService>();
        services.AddHostedService<ComputerService>();
    }
}
