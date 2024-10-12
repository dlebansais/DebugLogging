namespace DebugLogging;

using System;
using System.Collections.Generic;

public class LogScope : IDisposable
{
    public LogScope(object state)
    {
        State = state;
        LogScopeStack.Push(this);
    }

    public object State { get; }

    private bool disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                LogScopeStack.Pop();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private static Stack<LogScope> LogScopeStack = new();
}
