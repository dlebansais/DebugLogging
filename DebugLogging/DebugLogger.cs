namespace DebugLogging;

using System;
using System.IO;
using System.Reflection;
using Contracts;
using Microsoft.Extensions.Logging;
using ProcessCommunication;

/// <summary>
/// Represents a debug-time oriented logger.
/// </summary>
public class DebugLogger : ILogger
{
    /// <inheritdoc cref="ILogger.BeginScope{TState}(TState)"/>
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

    /// <inheritdoc cref="ILogger.IsEnabled"/>
    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    /// <inheritdoc cref="ILogger.Log"/>
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
        if (LogChannel is null)
        {
            using Stream? Stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"{typeof(DebugLogger).Assembly.GetName().Name}.Resources.ChannelGuid.txt");
            Stream ResourceStream = Contract.AssertNotNull(Stream);
            using StreamReader Reader = new(ResourceStream);
            string GuidString = Reader.ReadToEnd();
            Guid ChannelGuid = new(GuidString);

            LogChannel = new(ChannelGuid, Mode.Send);
            LogChannel.Open();
        }

        if (LogChannel.IsOpen)
            LogChannel.Write(Converter.EncodeString(message));
    }

    private static Channel? LogChannel;
}
