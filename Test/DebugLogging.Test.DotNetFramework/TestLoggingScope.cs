namespace DebugLogging.Test;

using NUnit.Framework;

[TestFixture]
internal class TestLoggingScope
{
    [Test]
    public void TestSuccess()
    {
        const string TestString = "Test scope";
        using LoggingScopeChild TestObject = new(TestString);
    }
}
