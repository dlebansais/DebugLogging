namespace DebugLogDisplay;

using System;
using System.Diagnostics;
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
public partial class App : Application, IDisposable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="App"/> class.
    /// </summary>
    public App()
    {
        using Stream? Stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"{typeof(App).Assembly.GetName().Name}.ChannelGuid.txt");
        Stream ResourceStream = Contract.AssertNotNull(Stream);
        using StreamReader Reader = new(ResourceStream);
        string GuidString = Reader.ReadToEnd();
        Guid ChannelGuid = new(GuidString);

        LogChannel = new Channel(ChannelGuid, Mode.Receive);
        LogChannel.Open();

        if (!LogChannel.IsOpen)
            Process.GetCurrentProcess().Kill();

        PollingTimer = new Timer(new TimerCallback(PollingTimerCallback));
        TimeSpan PollingInterval = TimeSpan.FromMilliseconds(20);
        _ = PollingTimer.Change(PollingInterval, PollingInterval);

        ExitTimer = new Timer(new TimerCallback(ExitTimerCallback));

        Startup += OnStartup;
    }

    private void OnStartup(object sender, StartupEventArgs e)
    {
        string[] Args = e.Args;
        if (Args.Length > 0 && int.TryParse(Args[0], out int MaxDuration))
            _ = ExitTimer.Change(TimeSpan.FromSeconds(MaxDuration), Timeout.InfiniteTimeSpan);
    }

    private void PollingTimerCallback(object? parameter)
    {
        if (PollingOperation is null || PollingOperation.Status is DispatcherOperationStatus.Completed)
            PollingOperation = Dispatcher.BeginInvoke(OnPolling);
    }

    private void OnPolling()
    {
        if (LogChannel.Read() is byte[] Data)
        {
            MainWindow Window = (MainWindow)Current.MainWindow;
            int Offset = 0;

            while (Offset < Data.Length)
            {
                if (!Converter.TryDecodeString(Data, ref Offset, out string Text))
                    break;

                Window.LogMessages.Add(Text);
            }
        }
    }

    private void ExitTimerCallback(object? parameter)
    {
        _ = Dispatcher.BeginInvoke(new Action(() => Current.Shutdown()));
    }

    /// <summary>
    /// Optionally disposes of the instance.
    /// </summary>
    /// <param name="disposing">True if disposing must be done.</param>
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
    private readonly Channel LogChannel;
    private readonly Timer ExitTimer;
    private bool disposedValue;
}
