namespace DebugLogging;

using System;
using System.IO;
using Microsoft.Extensions.Logging;

/// <summary>
/// Represents a debug-time oriented logger.
/// </summary>
public class DebugLogger : ILogger
{
    /// <inheritdoc/>
    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(state);
#else
        if (state is null) throw new ArgumentNullException(nameof(state));
#endif

        return new LoggingScope(state);
    }

    /// <summary>
    /// Gets or sets the default level.
    /// </summary>
    public LogLevel DefaultLevel { get; set; } = LogLevel.Debug;

    /// <inheritdoc/>
    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    /// <inheritdoc/>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(formatter);
#else
        if (state is null) throw new ArgumentNullException(nameof(state));
        if (formatter is null) throw new ArgumentNullException(nameof(formatter));
#endif

        string Message = formatter(state, exception);
        LogMessage(logLevel, Message);
    }

    /// <summary>
    /// Logs a message.
    /// </summary>
    /// <param name="message">The message.</param>
    public void Log(string message)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(message);
#else
        if (message is null) throw new ArgumentNullException(nameof(message));
#endif

        LogMessage(DefaultLevel, message);
    }

    private static void LogMessage(LogLevel logLevel, string message)
    {
        using FileStream Stream = new("test.txt", FileMode.Append, FileAccess.Write);
        using StreamWriter Writer = new(Stream);

        Writer.Write($"{message}\n");
    }
}
