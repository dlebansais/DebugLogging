#pragma warning disable CA1848 // Use the LoggerMessage delegates

namespace DebugLogging.Test;

using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using ProcessCommunication;

[TestFixture]
public class TestLogging
{
    private const int ExitDelay = 20;

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
    public async Task TestLog()
    {
        Remote.Reset();

        using DebugLogger TestObject = CreateTestLogger();

        TestObject.Log(LogLevel.None, (EventId)0, "Test Scope", null, (object state, Exception? exception) => { return $"{state}"; });

        await Task.Delay(Timeouts.ProcessLaunchTimeout + TimeSpan.FromSeconds(ExitDelay)).ConfigureAwait(true);
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
        Remote.Reset();

        using DebugLogger TestObject = CreateTestLogger();
        Stopwatch LaunchStopwatch = Stopwatch.StartNew();

        for (int i = 0; i < TestObject.MaxInitQueueSize + 1; i++)
            TestObject.Log($"Test Scope {i}");

        await WaitDelay(Timeouts.ProcessLaunchTimeout - TimeSpan.FromSeconds(1) - LaunchStopwatch.Elapsed).ConfigureAwait(true);

        TestObject.Log("Test Scope more 1");
        TestObject.Log("Test Scope more 2");

        await Task.Delay(TimeSpan.FromSeconds(ExitDelay) + TimeSpan.FromSeconds(10)).ConfigureAwait(true);
    }

    [Test]
    public void TestLogSimpleNull()
    {
        using DebugLogger TestObject = CreateTestLogger();
        const string NullString = null!;

        _ = Assert.Throws<ArgumentNullException>(() => TestObject.Log(NullString));
    }

    [Test]
    public void TestDispose()
    {
        using DebugLoggerChild TestObject = new();
    }

    [Test]
    public async Task TestBigLog()
    {
        Remote.Reset();

        using DebugLogger TestObject = CreateTestLogger();
        Stopwatch LaunchStopwatch = Stopwatch.StartNew();

        int MessageLength = (MultiChannel.Capacity * 3) / (TestObject.MaxInitQueueSize * 2);

        StringBuilder MessageBuilder = new();
        for (int i = 0; i < MessageLength; i++)
            MessageBuilder = MessageBuilder.Append('x');

        string Message = MessageBuilder.ToString();

        // Fill the queue.
        for (int i = 0; i < TestObject.MaxInitQueueSize; i++)
            TestObject.Log(Message);

        await WaitDelay(Timeouts.ProcessLaunchTimeout - TimeSpan.FromSeconds(1) - LaunchStopwatch.Elapsed).ConfigureAwait(true);

        TestObject.Log("Empty queue");

        await Task.Delay(TimeSpan.FromSeconds(ExitDelay) + TimeSpan.FromSeconds(10)).ConfigureAwait(true);
    }

    private static DebugLogger CreateTestLogger()
    {
        DebugLogger TestObject = new();

#if NETFRAMEWORK
        TestObject.DisplayAppName = "Foo.exe";
#else
        TestObject.DisplayAppArguments = ExitDelay.ToString(CultureInfo.InvariantCulture);
        TestObject.MaxInitQueueSize = 10;
#endif

        return TestObject;
    }

    private static async Task WaitDelay(TimeSpan delay)
    {
        TimeSpan MinZeroDelay = TimeSpan.FromSeconds(Math.Max(0, delay.TotalSeconds));
        await Task.Delay(MinZeroDelay).ConfigureAwait(true);
    }
}
