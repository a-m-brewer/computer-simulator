using System;
using System.Diagnostics;
using ComputerSimulator.Core.Peripherals.Display;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests.Peripherals.Display;

public class DisplayRenderPerformanceTests : IntegrationTestBase
{
    [Test]
    [Explicit("Manual display render benchmark; not part of normal correctness runs.")]
    public void CompareDisplayRenderModes()
    {
        const int iterations = 10;

        var gateLevel = MeasureInitialFrames(DisplayScanMode.GateLevel, iterations);
        var scanBuffer = MeasureInitialFrames(DisplayScanMode.ScanBuffer, iterations);
        var staticScanBuffer = MeasureStaticScanBufferFrames(iterations);

        TestContext.Progress.WriteLine($"Gate-level initial frames: {gateLevel.TotalMilliseconds:N2} ms");
        TestContext.Progress.WriteLine($"Scan-buffer initial frames: {scanBuffer.TotalMilliseconds:N2} ms");
        TestContext.Progress.WriteLine($"Scan-buffer static frames: {staticScanBuffer.TotalMilliseconds:N2} ms");
    }

    private TimeSpan MeasureInitialFrames(DisplayScanMode scanMode, int iterations)
    {
        var stopwatch = Stopwatch.StartNew();
        for (var i = 0; i < iterations; i++)
        {
            var display = new DisplayAdapter(
                WireFactory.CreateIoBus($"display-benchmark-{scanMode}-{i}"),
                ComputerSettings.ScreenWidth,
                ComputerSettings.ScreenHeight,
                scanMode,
                ComponentFactory,
                WireFactory);
            var output = new FakeDisplayOutput();
            output.Initialize(display.Width, display.Height);

            display.RenderFrame(output);
        }

        stopwatch.Stop();
        return stopwatch.Elapsed;
    }

    private TimeSpan MeasureStaticScanBufferFrames(int iterations)
    {
        var display = new DisplayAdapter(
            WireFactory.CreateIoBus("display-benchmark-static-scan-buffer"),
            ComputerSettings.ScreenWidth,
            ComputerSettings.ScreenHeight,
            DisplayScanMode.ScanBuffer,
            ComponentFactory,
            WireFactory);
        var output = new FakeDisplayOutput();
        output.Initialize(display.Width, display.Height);

        display.RenderFrame(output);

        var stopwatch = Stopwatch.StartNew();
        for (var i = 0; i < iterations; i++)
        {
            display.RenderFrame(output);
        }

        stopwatch.Stop();
        return stopwatch.Elapsed;
    }
}
