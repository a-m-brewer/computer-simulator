using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ComputerSimulator;

public static class HostExtensions
{
    public static IHost BuildHost(this string[] args)
    {
        var configurationArgs = NormalizeCommonSettingsArgs(args);

        var builder = Host.CreateDefaultBuilder(configurationArgs)
            .ConfigureAppConfiguration((context, configuration) =>
            {
                configuration.SetBasePath(AppContext.BaseDirectory);
                configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                configuration.AddJsonFile(
                    $"appsettings.{context.HostingEnvironment.EnvironmentName}.json",
                    optional: true,
                    reloadOnChange: true);
                configuration.AddEnvironmentVariables();
                configuration.AddCommandLine(configurationArgs);
            })
            .UseDefaultServiceProvider(options => options.ValidateScopes = true);

        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
        });

        builder.ConfigureServices((host, services) => new Startup(host.Configuration).ConfigureServices(services));

        return builder.Build();
    }

    private static string[] NormalizeCommonSettingsArgs(string[] args)
    {
        var normalized = new List<string>(args.Length);

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];

            if (arg.Equals("run", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                normalized.Add($"--Computer:ProgramPath={args[++i]}");
                continue;
            }

            var equalsIndex = arg.IndexOf('=', StringComparison.Ordinal);
            var switchName = equalsIndex >= 0 ? arg[..equalsIndex] : arg;

            if (!TryMapSettingSwitch(switchName, out var configurationKey, out var normalizeValue))
            {
                normalized.Add(arg);
                continue;
            }

            if (equalsIndex >= 0)
            {
                normalized.Add($"--{configurationKey}={normalizeValue(arg[(equalsIndex + 1)..])}");
                continue;
            }

            if (i + 1 >= args.Length)
            {
                normalized.Add(arg);
                continue;
            }

            normalized.Add($"--{configurationKey}={normalizeValue(args[++i])}");
        }

        return normalized.ToArray();
    }

    private static bool TryMapSettingSwitch(
        string switchName,
        out string configurationKey,
        out Func<string, string> normalizeValue)
    {
        normalizeValue = static value => value;

        configurationKey = switchName switch
        {
            "--scan-mode" or "--display-scan-mode" => "Computer:DisplayScanMode",
            "--width" or "--screen-width" => "Computer:ScreenWidth",
            "--height" or "--screen-height" => "Computer:ScreenHeight",
            "--cpu-updates-per-frame" => "Computer:CpuUpdatesPerFrame",
            "--frame-delay-ms" or "--display-frame-delay-ms" => "Computer:DisplayFrameDelayMs",
            "--perf-stats" or "--performance-stats" => "Computer:EnablePerformanceStats",
            "--perf-stats-interval" => "Computer:PerformanceStatsIntervalSeconds",
            "--pixel-mode" => "Terminal:PixelMode",
            "--log-lines" => "Terminal:LogLines",
            "--program" or "--program-path" => "Computer:ProgramPath",
            "--built-in-program" or "--demo" => "Computer:BuiltInProgram",
            _ => string.Empty
        };

        if (configurationKey.Length == 0)
        {
            return false;
        }

        if (configurationKey == "Computer:DisplayScanMode")
        {
            normalizeValue = NormalizeScanMode;
        }

        if (configurationKey == "Computer:BuiltInProgram")
        {
            normalizeValue = NormalizeBuiltInProgram;
        }

        return true;
    }

    private static string NormalizeScanMode(string value)
    {
        return value.Equals("gate", StringComparison.OrdinalIgnoreCase)
            ? "GateLevel"
            : value.Equals("buffer", StringComparison.OrdinalIgnoreCase)
                ? "ScanBuffer"
                : value;
    }

    private static string NormalizeBuiltInProgram(string value)
    {
        return value.Equals("text", StringComparison.OrdinalIgnoreCase)
               || value.Equals("hello", StringComparison.OrdinalIgnoreCase)
               || value.Equals("hello-world", StringComparison.OrdinalIgnoreCase)
            ? "HelloWorld"
            : value.Equals("pattern", StringComparison.OrdinalIgnoreCase)
              || value.Equals("display-pattern", StringComparison.OrdinalIgnoreCase)
                ? "DisplayPattern"
                : value;
    }
}
