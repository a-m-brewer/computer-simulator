using ComputerSimulator.Graphics;
using Microsoft.Extensions.Logging;

namespace ComputerSimulator;

public class TerminalLoggerProvider : ILoggerProvider
{
    private readonly ITerminalLogSink _sink;

    public TerminalLoggerProvider(ITerminalLogSink sink)
    {
        _sink = sink;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new TerminalLogger(categoryName, _sink);
    }

    public void Dispose()
    {
    }

    private class TerminalLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly ITerminalLogSink _sink;

        public TerminalLogger(string categoryName, ITerminalLogSink sink)
        {
            _categoryName = categoryName;
            _sink = sink;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var message = formatter(state, exception);
            if (string.IsNullOrWhiteSpace(message) && exception == null)
            {
                return;
            }

            var category = _categoryName.Split('.').Last();
            var line = $"{DateTime.Now:HH:mm:ss} {GetLevel(logLevel)} {category}: {message}";
            if (exception != null)
            {
                line += $" {exception.GetType().Name}: {exception.Message}";
            }

            _sink.Add(line);
        }

        private static string GetLevel(LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.Trace => "TRC",
                LogLevel.Debug => "DBG",
                LogLevel.Information => "INF",
                LogLevel.Warning => "WRN",
                LogLevel.Error => "ERR",
                LogLevel.Critical => "CRT",
                _ => logLevel.ToString()
            };
        }
    }
}
