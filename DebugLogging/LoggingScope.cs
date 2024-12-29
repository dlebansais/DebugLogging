namespace DebugLogging;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents a logging scope.
/// </summary>
public class LoggingScope : IDisposable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LoggingScope"/> class.
    /// </summary>
    /// <param name="state">The object state associated to the scope.</param>
    public LoggingScope(object state)
    {
        State = state;
        LogScopeStack.Push(this);
    }

    /// <summary>
    /// Gets the object state associated to the scope.
    /// </summary>
    public object State { get; }

    private bool disposedValue;

    /// <summary>
    /// Disposes of managed and unmanaged resources.
    /// </summary>
    /// <param name="disposing"><see langword="True"/> if the method should dispose of resources; Otherwise, <see langword="false"/>.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _ = LogScopeStack.Pop();
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

    private static readonly Stack<LoggingScope> LogScopeStack = new();
}
