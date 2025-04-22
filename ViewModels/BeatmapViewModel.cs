using osu_notsodirect_overlay.Helpers;
using osu_notsodirect_overlay.Models;
using osu_notsodirect_overlay.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace osu_notsodirect_overlay.ViewModels
{
    public class BeatmapViewModel : INotifyPropertyChanged
    {
        private readonly IBeatmapService _beatmapService;
        
        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<BeatmapSet> BeatmapSets { get; } = new ObservableCollection<BeatmapSet>();

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private string _searchQuery = string.Empty;
        public string SearchQuery
        {
            get => _searchQuery;
            set => SetProperty(ref _searchQuery, value);
        }

        private int _currentOffset;
        private int _pageSize = 20;
        private string _currentQuery = string.Empty;
        private int[]? _currentStatusFilter;
        private int[]? _currentModeFilter;
        private bool _hasMoreResults = true;

        public BeatmapViewModel(IBeatmapService beatmapService)
        {
            _beatmapService = beatmapService ?? throw new ArgumentNullException(nameof(beatmapService));
        }

        public async Task SearchBeatmapsAsync(string query, int[]? status = null, int[]? mode = null)
        {
            try
            {
                BeatmapSets.Clear();
                _currentOffset = 0;
                _hasMoreResults = true;
                _currentQuery = query;
                _currentStatusFilter = status;
                _currentModeFilter = mode;

                await LoadBeatmapsAsync(true);
            }
            catch (Exception ex)
            {
                DebugConsoleWindow.Instance.Log($"search error: {ex.Message}");
                MessageBox.Show($"error searching beatmaps: {ex.Message}", "error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task LoadMoreBeatmapsAsync()
        {
            if (IsLoading || !_hasMoreResults)
                return;

            await LoadBeatmapsAsync(false);
        }

        private async Task LoadBeatmapsAsync(bool isNewSearch)
        {
            if (IsLoading)
                return;

            IsLoading = true;
            DebugConsoleWindow.Instance.Log($"loading beatmaps from offset: {_currentOffset}");

            try
            {
                if (!isNewSearch)
                    _currentOffset += _pageSize;

                var beatmapSets = await _beatmapService.SearchBeatmapsAsync(
                    _currentQuery,
                    _pageSize,
                    _currentOffset,
                    _currentStatusFilter,
                    _currentModeFilter);

                if (beatmapSets != null)
                {
                    foreach (var beatmapSet in beatmapSets)
                    {
                        BeatmapSets.Add(beatmapSet);
                    }

                    _hasMoreResults = beatmapSets.Count >= _pageSize;
                }
            }
            catch (Exception ex)
            {
                DebugConsoleWindow.Instance.Log($"error loading beatmaps: {ex.Message}");
                if (!isNewSearch)
                    _currentOffset -= _pageSize;
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task DownloadBeatmapAsync(int beatmapId)
        {
            try
            {
                await _beatmapService.DownloadBeatmapAsync(beatmapId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"error downloading beatmap: {ex.Message}", "error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
} 