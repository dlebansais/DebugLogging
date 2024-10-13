#pragma warning disable CA1848 // Use the LoggerMessage delegates

namespace DebugLogging.Test;

using System;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

[TestFixture]
public class TestLoggingScope
{
    [Test]
    public void TestSuccess()
    {
        const string TestString = "Test scope";
        using LoggingScopeChild TestObject = new(TestString);
    }
}
