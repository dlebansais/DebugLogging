#pragma warning disable CA1812 // Avoid uninstantiated internal classes

namespace DebugLogDisplay;

using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;

/// <summary>
/// Interaction logic for MainWindow.xaml.
/// </summary>
internal partial class MainWindow : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
    }

    /// <summary>
    /// Gets the list of log messages.
    /// </summary>
    public ObservableCollection<string> LogMessages { get; } = [$"Displaying logs. v{GetVersion()}"];

    /// <summary>
    /// Adds a message to the list of log messages.
    /// </summary>
    /// <param name="message">The message.</param>
    public void AddMessage(string message)
    {
        LogMessages.Add(message);
        logScrollViewer.ScrollToBottom();
    }

    private static string? GetVersion()
        => Assembly.GetEntryAssembly()?.GetName().Version?.ToString();
}
