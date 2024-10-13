namespace DebugLogging.Test;

public class LoggingScopeChild : LoggingScope
{
    public LoggingScopeChild(object state)
        : base(state)
    {
    }

    protected override void Dispose(bool disposing)
    {
        // For coverage only. Validates the dispose pattern.
        base.Dispose(false);
        base.Dispose(disposing);
        base.Dispose(false);
    }
}
