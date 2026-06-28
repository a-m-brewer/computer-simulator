using System;
using ComputerSimulator.Core.Models;
using FluentAssertions;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests.Peripherals.Display;

public class ComputerSettingsTests
{
    [Test]
    public void ValidDisplaySettingsPassValidation()
    {
        var settings = new ComputerSettings
        {
            ScreenWidth = 16,
            ScreenHeight = 8
        };

        settings.Invoking(s => s.Validate()).Should().NotThrow();
    }

    [Test]
    public void ScreenWidthMustBeMultipleOfEight()
    {
        var settings = new ComputerSettings
        {
            ScreenWidth = 10,
            ScreenHeight = 8
        };

        settings.Invoking(s => s.Validate()).Should().Throw<ArgumentException>();
    }
}
