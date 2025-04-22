using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using osu_notsodirect_overlay.Helpers;
using osu_notsodirect_overlay.Services;
using osu_notsodirect_overlay.ViewModels;

namespace osu_notsodirect_overlay.Views
{
    public partial class OverlayWindow : Window
    {
        private readonly BeatmapViewModel _viewModel;
        private readonly KeyboardHook _keyboardHook;
        private bool _isVisible = true;
        private ScrollViewer? _scrollViewer;

        public OverlayWindow()
        {
            InitializeComponent();

            IBeatmapService beatmapService;
            if (Application.Current is App app)
            {
                beatmapService = app.BeatmapService;
                DebugConsoleWindow.Instance.Log("using BeatmapService from App");
            }
            else
            {
                beatmapService = new BeatmapService();
                DebugConsoleWindow.Instance.Log("created new BeatmapService instance");
            }

            _viewModel = new BeatmapViewModel(beatmapService);
            DataContext = _viewModel;

            ConfigureWindow();

            _keyboardHook = new KeyboardHook(OnKeyPressed);

            statusFilter.SelectedIndex = 0;
            modeFilter.SelectedIndex = 0;

            resultsListView.Loaded += (sender, args) =>
            {
                Dispatcher.InvokeAsync(() =>
                {
                    _scrollViewer = FindVisualChild<ScrollViewer>(resultsListView);
                    if (_scrollViewer != null)
                    {
                        _scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
                        DebugConsoleWindow.Instance.Log("ScrollViewer attached");
                    }
                    else
                    {
                        DebugConsoleWindow.Instance.Log("could not find ScrollViewer");
                    }
                }, DispatcherPriority.ContextIdle);
            };

            Closed += (s, e) =>
            {
                _keyboardHook.Dispose();
                DebugConsoleWindow.Instance.Close();
            };

            KeyDown += (s, e) =>
            {
                if (e.Key == Key.F12)
                {
                    if (DebugConsoleWindow.Instance.IsVisible)
                        DebugConsoleWindow.Instance.Hide();
                    else
                        DebugConsoleWindow.Instance.Show();
                }
            };

            DebugConsoleWindow.Instance.Log("application started!");
        }

        private void ConfigureWindow()
        {
            Topmost = true;
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
        }

        private void OnKeyPressed(Key key)
        {
            if (key == Key.Home)
            {
                Dispatcher.Invoke(ToggleInteraction);
            }
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string query = searchBox.Text;

            int[]? status = null;
            if (statusFilter.SelectedItem != null && ((ComboBoxItem)statusFilter.SelectedItem).Tag != null)
            {
                int statusValue = Convert.ToInt32(((ComboBoxItem)statusFilter.SelectedItem).Tag);
                status = statusValue != -1 ? new int[] { statusValue } : null;
            }

            int[]? mode = null;
            if (modeFilter.SelectedItem != null && ((ComboBoxItem)modeFilter.SelectedItem).Tag != null)
            {
                int modeValue = Convert.ToInt32(((ComboBoxItem)modeFilter.SelectedItem).Tag);
                mode = modeValue != -1 ? new int[] { modeValue } : null;
            }

            await _viewModel.SearchBeatmapsAsync(query, status, mode);
            
            if (_scrollViewer != null)
            {
                _scrollViewer.ScrollToTop();
            }
        }

        private async void Download_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int beatmapId)
            {
                double scrollPosition = 0;
                if (_scrollViewer != null)
                {
                    scrollPosition = _scrollViewer.VerticalOffset;
                }
                
                if (button.Parent is Grid buttonParent && buttonParent.Parent is Grid buttonContainer)
                {
                    var progressBorder = buttonParent.Children.OfType<Border>().FirstOrDefault(b => b.Name == "DownloadProgressBorder");
                    var progressGrid = progressBorder?.Child as Grid;
                    var progressBar = progressGrid?.Children.OfType<ProgressBar>().FirstOrDefault();
                    var statusText = progressGrid?.Children.OfType<TextBlock>().FirstOrDefault();
                    
                    try
                    {
                        button.Visibility = Visibility.Hidden;
                        await Dispatcher.InvokeAsync(() => {}, DispatcherPriority.Render);
                        
                        if (progressBorder != null)
                        {
                            progressBorder.Visibility = Visibility.Visible;
                            if (statusText != null)
                                statusText.Text = "downloading...";
                        }
                        
                        await _viewModel.DownloadBeatmapAsync(beatmapId);
                        
                        if (progressBar != null)
                        {
                            progressBar.IsIndeterminate = false;
                            progressBar.Value = 100;
                        }
                        if (statusText != null)
                            statusText.Text = "completed!";
                        
                        await Task.Delay(1000);
                    }
                    catch (Exception ex)
                    {
                        DebugConsoleWindow.Instance.Log($"download error: {ex.Message}");
                        
                        if (progressBar != null)
                        {
                            progressBar.IsIndeterminate = false;
                            progressBar.Value = 0;
                            progressBar.Foreground = new SolidColorBrush(Colors.Red);
                        }
                        if (statusText != null)
                            statusText.Text = "failed";
                        
                        await Task.Delay(1000);
                    }
                    finally
                    {
                        if (progressBar != null)
                        {
                            progressBar.IsIndeterminate = true;
                            progressBar.Value = 0;
                            progressBar.Foreground = (SolidColorBrush)FindResource("AccentBrush");
                        }
                        
                        if (progressBorder != null)
                        {
                            progressBorder.Visibility = Visibility.Collapsed;
                        }
                        await Dispatcher.InvokeAsync(() => {}, DispatcherPriority.Render);
                        
                        button.Visibility = Visibility.Visible;
                        
                        if (_scrollViewer != null)
                        {
                            _scrollViewer.ScrollToVerticalOffset(scrollPosition);
                        }
                    }
                }
            }
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private async void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (sender is ScrollViewer scrollViewer)
            {
                if (scrollViewer.VerticalOffset > (scrollViewer.ScrollableHeight - 50) && 
                    !_viewModel.IsLoading)
                {
                    await _viewModel.LoadMoreBeatmapsAsync();
                }
            }
        }

        private static T? FindVisualChild<T>(DependencyObject? parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child != null && child is T result)
                    return result;

                T? childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }

        private void ToggleInteraction()
        {
            _isVisible = !_isVisible;

            if (_isVisible)
            {
                Visibility = Visibility.Visible;
                WindowHelper.MakeWindowClickable(this);
            }
            else
            {
                Visibility = Visibility.Collapsed;
            }
        }
    }
} 