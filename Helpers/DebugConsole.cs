using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace osu_notsodirect_overlay.Helpers
{
    public class DebugConsoleWindow : Window
    {
        private readonly TextBox _consoleOutput;
        private static DebugConsoleWindow? _instance;

        public static DebugConsoleWindow Instance
        {
            get
            {
                _instance ??= new DebugConsoleWindow();
                return _instance;
            }
        }

        private DebugConsoleWindow()
        {
            Title = "osu!notsodirect debug console";
            Width = 800;
            Height = 400;
            WindowStyle = WindowStyle.ToolWindow;

            Grid grid = new();
            Content = grid;

            _consoleOutput = new TextBox
            {
                IsReadOnly = true,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                Background = System.Windows.Media.Brushes.Black,
                Foreground = System.Windows.Media.Brushes.LightGreen,
                BorderThickness = new Thickness(0)
            };

            grid.Children.Add(_consoleOutput);

            Closing += (s, e) =>
            {
                e.Cancel = true;
                Hide();
            };
        }

        public void Log(string message)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => Log(message));
                return;
            }

            _consoleOutput.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
            _consoleOutput.ScrollToEnd();
        }

        public void Clear()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(Clear);
                return;
            }

            _consoleOutput.Clear();
        }
    }
} 