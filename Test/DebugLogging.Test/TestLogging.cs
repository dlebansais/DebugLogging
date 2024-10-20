#pragma warning disable CA1848 // Use the LoggerMessage delegates

namespace DebugLogging.Test;

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using ProcessCommunication;

[TestFixture]
public class TestLogging
{
    [Test]
    public void TestSuccess()
    {
        using DebugLogger TestObject = CreateTestLogger();

        Assert.That(TestObject, Is.Not.Null);
        Assert.That(TestObject.IsEnabled(TestObject.DefaultLevel), Is.True);
        Assert.That(TestObject.DefaultLevel, Is.EqualTo(LogLevel.Debug));
    }

    [Test]
    public void TestBeginScope()
    {
        using DebugLogger TestObject = CreateTestLogger();
        const string TestString = "Test Scope";

        using LoggingScope? Scope = TestObject.BeginScope(TestString) as LoggingScope;

        Assert.That(Scope, Is.Not.Null);
        Assert.That(Scope.State, Is.EqualTo(TestString));
    }

    [Test]
    public void TestBeginScopeNull()
    {
        using DebugLogger TestObject = CreateTestLogger();
        const string NullString = null!;

        _ = Assert.Throws<ArgumentNullException>(() => TestObject.BeginScope(NullString));
    }

    [Test]
    public void TestLog()
    {
        using DebugLogger TestObject = CreateTestLogger();

        TestObject.Log(LogLevel.None, (EventId)0, "Test Scope", null, (object state, Exception? exception) => { return $"{state}"; });
    }

    [Test]
    public void TestLogNull()
    {
        using DebugLogger TestObject = CreateTestLogger();
        const string NullString = null!;
        const string TestString = "Test Scope";
        const Func<string, Exception?, string> NullFormatter = null!;

        _ = Assert.Throws<ArgumentNullException>(() => TestObject.Log(LogLevel.None, (EventId)0, NullString, null, (object state, Exception? exception) => { return $"{state}"; }));
        _ = Assert.Throws<ArgumentNullException>(() => TestObject.Log(LogLevel.None, (EventId)0, TestString, null, NullFormatter));
    }

    [Test]
    public async Task TestLogSimple()
    {
        using DebugLogger TestObject = CreateTestLogger();
        Stopwatch LaunchStopwatch = Stopwatch.StartNew();

        TestObject.Log("Test Scope 1");

        await Task.Delay(Timeouts.ProcessLaunchTimeout - TimeSpan.FromSeconds(1) - LaunchStopwatch.Elapsed).ConfigureAwait(true);

        TestObject.Log("Test Scope 2");
    }

    [Test]
    public void TestLogSimpleNull()
    {
        using DebugLogger TestObject = CreateTestLogger();
        const string NullString = null!;

        _ = Assert.Throws<ArgumentNullException>(() => TestObject.Log(NullString));
    }

    private static DebugLogger CreateTestLogger()
    {
        DebugLogger TestObject = new();

#if NETFRAMEWORK
        TestObject.DisplayAppName = "Foo.exe";
#else
        TestObject.DisplayAppArguments = "20";
#endif

        return TestObject;
    }
}
