namespace DebugLogDisplay;

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Contracts;
using ProcessCommunication;

/// <summary>
/// Interaction logic for App.xaml.
/// </summary>
internal partial class App : Application, IDisposable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="App"/> class.
    /// </summary>
    public App()
    {
        Guid ChannelGuid = new(GetResourceContent("ChannelGuid.txt"));
        int MaxChannelCount = int.Parse(GetResourceContent("MaxChannelCount.txt"), CultureInfo.InvariantCulture);

        LogChannel = new MultiChannel(ChannelGuid, ChannelMode.Receive, MaxChannelCount);
        LogChannel.Open();

        if (!LogChannel.IsOpen)
            Process.GetCurrentProcess().Kill();

        PollingTimer = new Timer(new TimerCallback(PollingTimerCallback));
        TimeSpan PollingInterval = TimeSpan.FromMilliseconds(20);
        _ = PollingTimer.Change(PollingInterval, PollingInterval);

        ExitTimer = new Timer(new TimerCallback(ExitTimerCallback));

        Startup += OnStartup;
    }

    private static string GetResourceContent(string resourceName)
    {
        using Stream? Stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"{typeof(App).Assembly.GetName().Name}.{resourceName}");
        Stream ResourceStream = Contract.AssertNotNull(Stream);
        using StreamReader Reader = new(ResourceStream);

        return Reader.ReadToEnd();
    }

    private void OnStartup(object sender, StartupEventArgs e)
    {
        string[] Args = e.Args;
        if (Args.Length > 0 && int.TryParse(Args[0], out int ParsedMaxDuration))
        {
            MaxDuration = ParsedMaxDuration;
            UpdateExitTimer();

            _ = Dispatcher.BeginInvoke(() =>
            {
                MainWindow Window = (MainWindow)Current.MainWindow;
                Window.AddMessage($"Exit timeout set to {MaxDuration} seconds.");
            });
        }
    }

    private void PollingTimerCallback(object? parameter)
    {
        if (PollingOperation is null || PollingOperation.Status is DispatcherOperationStatus.Completed)
            PollingOperation = Dispatcher.BeginInvoke(OnPolling);
    }

    private void OnPolling()
    {
        if (LogChannel.TryRead(out byte[] Data, out _))
        {
            MainWindow Window = (MainWindow)Current.MainWindow;
            int Offset = 0;

            while (Offset < Data.Length)
            {
                if (!Converter.TryDecodeString(Data, ref Offset, out string Text))
                    break;

                Window.AddMessage(Text);
            }

            UpdateExitTimer();
        }
    }

    private void UpdateExitTimer()
    {
        if (MaxDuration > 0)
            _ = ExitTimer.Change(TimeSpan.FromSeconds(MaxDuration), Timeout.InfiniteTimeSpan);
    }

    private void ExitTimerCallback(object? parameter)
        => _ = Dispatcher.BeginInvoke(new Action(Current.Shutdown));

    /// <summary>
    /// Disposes of managed and unmanaged resources.
    /// </summary>
    /// <param name="disposing"><see langword="True"/> if the method should dispose of resources; Otherwise, <see langword="false"/>.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                LogChannel.Dispose();
                PollingTimer.Dispose();
                ExitTimer.Dispose();
            }

            disposedValue = true;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private readonly Timer PollingTimer;
    private DispatcherOperation? PollingOperation;
    private readonly MultiChannel LogChannel;
    private readonly Timer ExitTimer;
    private bool disposedValue;
    private int MaxDuration;
}
