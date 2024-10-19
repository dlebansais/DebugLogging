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
        DebugLogger TestObject = CreateTestLogger();

        Assert.That(TestObject, Is.Not.Null);
        Assert.That(TestObject.IsEnabled(TestObject.DefaultLevel), Is.True);
        Assert.That(TestObject.DefaultLevel, Is.EqualTo(LogLevel.Debug));
    }

    [Test]
    public void TestBeginScope()
    {
        DebugLogger TestObject = CreateTestLogger();
        const string TestString = "Test Scope";

        using LoggingScope? Scope = TestObject.BeginScope(TestString) as LoggingScope;

        Assert.That(Scope, Is.Not.Null);
        Assert.That(Scope.State, Is.EqualTo(TestString));
    }

    [Test]
    public void TestBeginScopeNull()
    {
        DebugLogger TestObject = CreateTestLogger();
        const string NullString = null!;

        _ = Assert.Throws<ArgumentNullException>(() => TestObject.BeginScope(NullString));
    }

    [Test]
    public void TestLog()
    {
        DebugLogger TestObject = CreateTestLogger();

        TestObject.Log(LogLevel.None, (EventId)0, "Test Scope", null, (object state, Exception? exception) => { return $"{state}"; });
    }

    [Test]
    public void TestLogNull()
    {
        DebugLogger TestObject = CreateTestLogger();
        const string NullString = null!;
        const string TestString = "Test Scope";
        const Func<string, Exception?, string> NullFormatter = null!;

        _ = Assert.Throws<ArgumentNullException>(() => TestObject.Log(LogLevel.None, (EventId)0, NullString, null, (object state, Exception? exception) => { return $"{state}"; }));
        _ = Assert.Throws<ArgumentNullException>(() => TestObject.Log(LogLevel.None, (EventId)0, TestString, null, NullFormatter));
    }

    [Test]
    public void TestLogSimple()
    {
        DebugLogger TestObject = CreateTestLogger();

        TestObject.Log("Test Scope");
    }

    [Test]
    public void TestLogSimpleNull()
    {
        DebugLogger TestObject = CreateTestLogger();
        const string NullString = null!;

        _ = Assert.Throws<ArgumentNullException>(() => TestObject.Log(NullString));
    }

    private static DebugLogger CreateTestLogger()
    {
        DebugLogger TestObject = new();

        TestObject.DisplayAppName = "Foo.exe";

        return TestObject;
    }
}
