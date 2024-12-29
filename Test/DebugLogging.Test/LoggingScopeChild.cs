namespace DebugLogging.Test;

internal class LoggingScopeChild(object state) : LoggingScope(state)
{
    protected override void Dispose(bool disposing)
    {
        // For coverage only. Validates the dispose pattern.
        base.Dispose(false);
        base.Dispose(disposing);
        base.Dispose(false);
    }
}
