#pragma warning disable CA1848 // Use the LoggerMessage delegates

namespace DebugLogging.Test;

using System;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

[TestFixture]
public class TestLogging
{
    [Test]
    public void TestSuccess()
    {
        DebugLogger TestObject = new();

        Assert.That(TestObject, Is.Not.Null);
        Assert.That(TestObject.IsEnabled(TestObject.DefaultLevel), Is.True);
        Assert.That(TestObject.DefaultLevel, Is.EqualTo(LogLevel.Debug));
    }

    [Test]
    public void TestBeginScope()
    {
        DebugLogger TestObject = new();

        using IDisposable? Scope = TestObject.BeginScope("Test Scope");

        Assert.That(Scope, Is.Not.Null);
    }

    [Test]
    public void TestLog()
    {
        DebugLogger TestObject = new();

        TestObject.Log(LogLevel.None, (EventId)0, "Test Scope", null, (object state, Exception? exception) => { return $"{state}"; });
    }
}
