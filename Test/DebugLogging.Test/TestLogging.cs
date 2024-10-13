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
        const string TestString = "Test Scope";

        using LoggingScope? Scope = TestObject.BeginScope(TestString) as LoggingScope;

        Assert.That(Scope, Is.Not.Null);
        Assert.That(Scope.State, Is.EqualTo(TestString));
    }

    [Test]
    public void TestLog()
    {
        DebugLogger TestObject = new();

        TestObject.Log(LogLevel.None, (EventId)0, "Test Scope", null, (object state, Exception? exception) => { return $"{state}"; });
    }

    [Test]
    public void TestLogSimple()
    {
        DebugLogger TestObject = new();

        TestObject.Log("Test Scope");
    }
}
