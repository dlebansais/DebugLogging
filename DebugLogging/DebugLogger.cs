namespace DebugLogging;

using System;
using System.IO;
using Microsoft.Extensions.Logging;

public class DebugLogger : ILogger
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        ArgumentNullException.ThrowIfNull(state);

        return new LogScope(state);
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        using FileStream Stream = new("C:\\Applications\\test.txt", FileMode.Append, FileAccess.Write);
        using StreamWriter Writer = new(Stream);

        string Message = formatter(state, exception);
        Log(Message);
    }

    public void Log(string message)
    {
        using FileStream Stream = new("C:\\Applications\\test.txt", FileMode.Append, FileAccess.Write);
        using StreamWriter Writer = new(Stream);

        Writer.Write($"{message}\n");
    }
}
