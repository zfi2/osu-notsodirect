using Newtonsoft.Json;
using osu_notsodirect_overlay.Helpers;
using osu_notsodirect_overlay.Models;
using osu_notsodirect_overlay.Services.Download;
using System.Net.Http;

namespace osu_notsodirect_overlay.Services
{
    public interface IBeatmapService : IDisposable
    {
        Task<List<BeatmapSet>> SearchBeatmapsAsync(string query, int limit, int offset, int[]? status, int[]? mode);
        Task DownloadBeatmapAsync(int beatmapId);
    }

    public class BeatmapService : IBeatmapService
    {
        private const string SearchApiUrl = "https://catboy.best/api/v2/search";
        private readonly HttpClient _httpClient;
        private readonly IBeatmapDownloader _downloader;
        private bool _isDisposed;

        public BeatmapService()
        {
            _httpClient = new HttpClient();
            _downloader = new BeatmapDownloader();
        }

        public async Task<List<BeatmapSet>> SearchBeatmapsAsync(string query, int limit = 20, int offset = 0, int[]? status = null, int[]? mode = null)
        {
            var url = $"{SearchApiUrl}?query={Uri.EscapeDataString(query)}&limit={limit}&offset={offset}";

            if (status != null && status.Length > 0 && status[0] != -1)
                url += $"&status={string.Join(",", status)}";

            if (mode != null && mode.Length > 0 && mode[0] != -1)
                url += $"&mode={string.Join(",", mode)}";

            DebugConsoleWindow.Instance.Log($"searching with URL: {url}");

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("User-Agent", "osu!notsodirect/1.0");

                DebugConsoleWindow.Instance.Log("request headers:");
                foreach (var header in request.Headers)
                {
                    DebugConsoleWindow.Instance.Log($"  {header.Key}: {string.Join(", ", header.Value)}");
                }

                var response = await _httpClient.SendAsync(request);

                DebugConsoleWindow.Instance.Log($"response status: {(int)response.StatusCode} {response.StatusCode}");
                DebugConsoleWindow.Instance.Log("response headers:");
                foreach (var header in response.Headers)
                {
                    DebugConsoleWindow.Instance.Log($"  {header.Key}: {string.Join(", ", header.Value)}");
                }

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    DebugConsoleWindow.Instance.Log($"response preview: {content.Substring(0, Math.Min(500, content.Length))}");

                    var beatmapSets = JsonConvert.DeserializeObject<List<BeatmapSet>>(content);
                    
                    if (beatmapSets == null)
                    {
                        DebugConsoleWindow.Instance.Log("failed to deserialize response");
                        return new List<BeatmapSet>();
                    }

                    DebugConsoleWindow.Instance.Log($"found {beatmapSets.Count} beatmap sets");
                    return beatmapSets;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    DebugConsoleWindow.Instance.Log($"error content: {errorContent}");
                    throw new HttpRequestException($"response status code does not indicate success: {(int)response.StatusCode} ({response.StatusCode}).");
                }
            }
            catch (Exception ex)
            {
                DebugConsoleWindow.Instance.Log($"exception: {ex.Message}");
                if (ex.InnerException != null)
                    DebugConsoleWindow.Instance.Log($"inner exception: {ex.InnerException.Message}");

                throw;
            }
        }

        public async Task DownloadBeatmapAsync(int beatmapId)
        {
            try
            {
                DebugConsoleWindow.Instance.Log($"starting direct download for beatmap {beatmapId}...");
                await _downloader.DownloadBeatmapAsync(beatmapId);
            }
            catch (Exception ex)
            {
                DebugConsoleWindow.Instance.Log($"error downloading beatmap: {ex.Message}");
                throw;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    try
                    {
                        _httpClient.Dispose();
                    }
                    catch (Exception ex)
                    {
                        DebugConsoleWindow.Instance.Log($"error disposing BeatmapService: {ex.Message}");
                    }
                }

                _isDisposed = true;
            }
        }

        ~BeatmapService()
        {
            Dispose(false);
        }
    }
} 