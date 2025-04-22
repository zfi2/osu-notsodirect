using osu_notsodirect_overlay.Helpers;
using System.IO;
using System.Net;
using System.Text;

namespace osu_notsodirect_overlay.Services.Download
{
    public class HttpServer : IDisposable
    {
        private readonly HttpListener _listener;
        private readonly IBeatmapDownloader _downloader;
        private readonly int _port;
        private bool _running;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly string _directories;

        public HttpServer(IBeatmapDownloader downloader, int port = 3000)
        {
            _downloader = downloader ?? throw new ArgumentNullException(nameof(downloader));
            _port = port;
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://localhost:{_port}/");

            string assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
            if (string.IsNullOrEmpty(assemblyLocation))
                assemblyLocation = System.AppContext.BaseDirectory;

            string assemblyDirectory = Path.GetDirectoryName(assemblyLocation) ?? string.Empty;
            _directories = Path.Combine(assemblyDirectory, "Services", "Download");
            
            DebugConsoleWindow.Instance.Log($"directories: {_directories}");
        }

        public async Task StartAsync()
        {
            if (_running) return;

            try
            {
                _listener.Start();
                _running = true;
                DebugConsoleWindow.Instance.Log($"HTTP server started on port {_port}");

                _ = Task.Run(() => AcceptConnectionsAsync(_cancellationTokenSource.Token));
            }
            catch (Exception ex)
            {
                DebugConsoleWindow.Instance.Log($"failed to start HTTP server: {ex.Message}");
                throw;
            }
        }

        public void Stop()
        {
            if (!_running) return;

            _cancellationTokenSource.Cancel();
            _listener.Stop();
            _running = false;
            DebugConsoleWindow.Instance.Log("HTTP server stopped");
        }

        private async Task AcceptConnectionsAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && _running)
            {
                try
                {
                    HttpListenerContext context = await _listener.GetContextAsync();

                    _ = Task.Run(() => ProcessRequestAsync(context));
                }
                catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
                {
                    DebugConsoleWindow.Instance.Log($"error accepting connection: {ex.Message}");
                }
            }
        }

        private async Task ProcessRequestAsync(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            try
            {
                string path = request.Url?.AbsolutePath ?? "/";
                DebugConsoleWindow.Instance.Log($"received request: {request.HttpMethod} {path}");

                response.Headers.Add("Access-Control-Allow-Origin", "*");
                response.Headers.Add("Access-Control-Allow-Methods", "GET, POST");
                response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");
                
                if (path.StartsWith("/api/download/"))
                {
                    await HandleDownloadApiAsync(context);
                    return;
                }
                else
                {
                    response.StatusCode = 404;
                    byte[] buffer = Encoding.UTF8.GetBytes("not found");
                    response.ContentLength64 = buffer.Length;
                    await response.OutputStream.WriteAsync(buffer);
                }
            }
            catch (Exception ex)
            {
                DebugConsoleWindow.Instance.Log($"error processing request: {ex.Message}");
                try
                {
                    response.StatusCode = 500;
                    byte[] buffer = Encoding.UTF8.GetBytes($"internal server error: {ex.Message}");
                    response.ContentLength64 = buffer.Length;
                    await response.OutputStream.WriteAsync(buffer);
                }
                catch { }
            }
            finally
            {
                try { response.Close(); } catch { }
            }
        }

        private async Task HandleDownloadApiAsync(HttpListenerContext context)
        {
            string path = context.Request.Url?.AbsolutePath ?? "/";
            string beatmapIdStr = path.Replace("/api/download/", "");

            if (!int.TryParse(beatmapIdStr, out int beatmapId))
            {
                context.Response.StatusCode = 400;
                byte[] buffer = Encoding.UTF8.GetBytes("invalid beatmap id");
                context.Response.ContentLength64 = buffer.Length;
                await context.Response.OutputStream.WriteAsync(buffer);
                return;
            }

            context.Response.StatusCode = 302;
            context.Response.Headers.Add("Location", $"/download/{beatmapId}");
            context.Response.Close();

            _ = Task.Run(() => _downloader.DownloadBeatmapAsync(beatmapId));
        }

        public void Dispose()
        {
            Stop();
            _cancellationTokenSource.Dispose();
        }
    }
} 