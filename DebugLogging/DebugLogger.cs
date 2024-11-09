namespace DebugLogging;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using Contracts;
using Microsoft.Extensions.Logging;
using ProcessCommunication;

/// <summary>
/// Represents a debug-time oriented logger.
/// </summary>
public class DebugLogger : ILogger, IDisposable
{
    /// <summary>
    /// Gets or sets the name of the application used to display logs.
    /// </summary>
    public string DisplayAppName { get; set; } = "DebugLogDisplay.exe";

    /// <summary>
    /// Gets or sets arguments when launching <see cref="DisplayAppName"/>.
    /// </summary>
    public string? DisplayAppArguments { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of messages queued until they can be displayed.
    /// </summary>
    public int MaxInitQueueSize { get; set; } = 1000;

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

    private void LogMessage(LogLevel logLevel, string message)
    {
        if (LogChannel is null)
        {
            Guid ChannelGuid = new(GetResourceContent("ChannelGuid.txt"));
            int MaxChannelCount = int.Parse(GetResourceContent("MaxChannelCount.txt"), CultureInfo.InvariantCulture);

            string PathToProccess = Remote.GetSiblingFullPath(DisplayAppName);
            LogChannel = Remote.LaunchAndOpenChannel(PathToProccess, ChannelGuid, MaxChannelCount, DisplayAppArguments);
        }

        if (LogChannel is not null && LogChannel.IsOpen)
        {
            while (QueuedMessages.Count > 0)
                LogMessage(LogChannel, QueuedMessages.Dequeue());

            LogMessage(LogChannel, message);
        }
        else
        {
            if (QueuedMessages.Count >= MaxInitQueueSize)
                _ = QueuedMessages.Dequeue();

            QueuedMessages.Enqueue(message);
        }
    }

    private static string GetResourceContent(string resourceName)
    {
        using Stream? Stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"{typeof(DebugLogger).Assembly.GetName().Name}.Resources.{resourceName}");
        Stream ResourceStream = Contract.AssertNotNull(Stream);
        using StreamReader Reader = new(ResourceStream);

        return Reader.ReadToEnd();
    }

    private static void LogMessage(IMultiChannel channel, string message)
    {
        byte[] Data = Converter.EncodeString(message);

        if (Data.Length <= channel.GetFreeLength())
            channel.Write(Data);
    }

    /// <summary>
    /// Optionally disposes of the instance.
    /// </summary>
    /// <param name="disposing">True if disposing must be done.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                LogChannel?.Dispose();
            }

            disposedValue = true;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private IMultiChannel? LogChannel;
    private bool disposedValue;
    private readonly Queue<string> QueuedMessages = new();
}
