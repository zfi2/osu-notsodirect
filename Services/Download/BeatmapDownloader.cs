using osu_notsodirect_overlay.Helpers;
using System.Diagnostics;
using System.IO;
using System.Net.Http;

namespace osu_notsodirect_overlay.Services.Download
{
    public interface IBeatmapDownloader
    {
        Task DownloadBeatmapAsync(int beatmapId);
    }

    public class BeatmapDownloader : IBeatmapDownloader
    {
        private readonly string _tempDir;
        private readonly HttpClient _httpClient;

        private readonly string[] _mirrors = new[]
        {
            "https://catboy.best/d/{0}",
            "https://api.nerinyan.moe/d/{0}"
        };

        public BeatmapDownloader()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "osu!notsodirect/1.0");
            
            _tempDir = Path.Combine(Path.GetTempPath(), "osu-notsodirect-overlay");
            
            if (!Directory.Exists(_tempDir))
            {
                Directory.CreateDirectory(_tempDir);
            }
        }
        public async Task DownloadBeatmapAsync(int beatmapId)
        {
            try
            {
                string filePath = Path.Combine(_tempDir, $"{beatmapId}.osz");

                await DownloadWithFallbackAsync(beatmapId, filePath);

                if (!File.Exists(filePath) || new FileInfo(filePath).Length <= 0)
                {
                    DebugConsoleWindow.Instance.Log($"downloaded file is empty/corrupted: {filePath}");
                    if (File.Exists(filePath))
                    {
                        try
                        {
                            File.Delete(filePath);
                            DebugConsoleWindow.Instance.Log($"successfully deleted empty/corrupted file: {filePath}");
                        }
                        catch (Exception deleteEx)
                        {
                            DebugConsoleWindow.Instance.Log($"failed to delete empty/corrupted file: {deleteEx.Message}");
                        }
                    }
                    return;
                }

                Process.Start(new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });

                DebugConsoleWindow.Instance.Log($"imported beatmap set {beatmapId} into osu!");
                
                _ = Task.Run(async () => 
                {
                    await Task.Delay(10000);
                    DebugConsoleWindow.Instance.Log($"removed progress tracking for beatmap: {beatmapId}");
                });
            }
            catch (Exception ex)
            {
                DebugConsoleWindow.Instance.Log($"error downloading beatmap: {beatmapId}: {ex.Message}");
            }
        }

        private async Task DownloadWithFallbackAsync(int beatmapId, string filePath)
        {
            foreach (var mirrorPattern in _mirrors)
            {
                string url = string.Format(mirrorPattern, beatmapId);
                try
                {
                    await TryDownloadAsync(url, filePath);
                    return;
                }
                catch (Exception ex)
                {
                    DebugConsoleWindow.Instance.Log($"failed to download from {url}: {ex.Message}");
                }
            }
            throw new Exception("all download mirrors failed");
        }

        private async Task TryDownloadAsync(string url, string filePath, int attempts = 3)
        {
            for (int i = 0; i < attempts; i++)
            {
                try
                {
                    using var request = new HttpRequestMessage(HttpMethod.Get, url);
                    using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                    response.EnsureSuccessStatusCode();

                    long? totalBytes = response.Content.Headers.ContentLength;

                    using var contentStream = await response.Content.ReadAsStreamAsync();
                    using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

                    byte[] buffer = new byte[8192];
                    long totalBytesRead = 0;
                    int bytesRead;

                    while ((bytesRead = await contentStream.ReadAsync(buffer)) != 0)
                    {
                        await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                        totalBytesRead += bytesRead;
                    }

                    return;
                }
                catch (Exception ex)
                {
                    DebugConsoleWindow.Instance.Log($"download attempt: {i + 1}/{attempts} failed: {ex.Message}");
                    if (i == attempts - 1)
                    {
                        throw;
                    }
                }
            }
        }
    }
} 