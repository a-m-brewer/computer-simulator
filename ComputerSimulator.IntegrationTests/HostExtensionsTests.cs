using ComputerSimulator.Core.Models;
using ComputerSimulator.Core.Peripherals.Display;
using ComputerSimulator.Core.Programs;
using ComputerSimulator.Graphics;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests;

public class HostExtensionsTests
{
    [Test]
    public void ScanModeGateAliasOverridesComputerSettings()
    {
        using var host = new[] { "--scan-mode", "gate" }.BuildHost();

        var settings = host.Services.GetRequiredService<ComputerSettings>();

        settings.DisplayScanMode.Should().Be(DisplayScanMode.GateLevel);
    }

    [Test]
    public void ScanModeBufferAliasOverridesComputerSettings()
    {
        using var host = new[] { "--scan-mode=buffer" }.BuildHost();

        var settings = host.Services.GetRequiredService<ComputerSettings>();

        settings.DisplayScanMode.Should().Be(DisplayScanMode.ScanBuffer);
    }

    [Test]
    public void CommonDisplayAliasesOverrideBoundSettings()
    {
        using var host = new[]
        {
            "--width", "320",
            "--height", "200",
            "--perf-stats", "true",
            "--perf-stats-interval", "3",
            "--pixel-mode", "Block"
        }.BuildHost();

        var computerSettings = host.Services.GetRequiredService<ComputerSettings>();
        var terminalSettings = host.Services.GetRequiredService<TerminalSettings>();

        computerSettings.ScreenWidth.Should().Be(320);
        computerSettings.ScreenHeight.Should().Be(200);
        computerSettings.EnablePerformanceStats.Should().BeTrue();
        computerSettings.PerformanceStatsIntervalSeconds.Should().Be(3);
        terminalSettings.PixelMode.Should().Be(TerminalPixelMode.Block);
    }

    [Test]
    public void RunCommandSetsProgramPath()
    {
        using var host = new[] { "run", "demo.bin" }.BuildHost();

        var settings = host.Services.GetRequiredService<ComputerSettings>();

        settings.ProgramPath.Should().Be("demo.bin");
    }

    [Test]
    public void ProgramAliasOverridesProgramPath()
    {
        using var host = new[] { "--program", "demo.bin" }.BuildHost();

        var settings = host.Services.GetRequiredService<ComputerSettings>();

        settings.ProgramPath.Should().Be("demo.bin");
    }

    [Test]
    public void DemoAliasSelectsHelloWorldBuiltInProgram()
    {
        using var host = new[] { "--demo", "hello-world" }.BuildHost();

        var settings = host.Services.GetRequiredService<ComputerSettings>();

        settings.BuiltInProgram.Should().Be(BuiltInProgram.HelloWorld);
    }
}
