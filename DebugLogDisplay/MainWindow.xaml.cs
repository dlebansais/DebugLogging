namespace DebugLogDisplay;

using System.Collections.ObjectModel;
using System.Windows;

/// <summary>
/// Interaction logic for MainWindow.xaml.
/// </summary>
public partial class MainWindow : Window
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
    /// Gets the list og log messages.
    /// </summary>
    public ObservableCollection<string> LogMessages { get; } = new() { "Displaying logs. v1.0.0" };
}
