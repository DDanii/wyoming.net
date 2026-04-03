using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Wyoming.Net.Satellite.App.Tz.Platform;

internal sealed class DebugFileServer : IDisposable
{
    private const int Port = 8089;

    private readonly ILogger _logger;
    private readonly string _dataDir;
    private HttpListener? _listener;
    private Thread? _thread;

    public DebugFileServer(ILogger logger)
    {
        _logger = logger;
        _dataDir = TizenAssetReader.DataDir;
    }

    public void Start()
    {
        if (_listener != null) return;

        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://*:{Port}/");
        _listener.Start();

        _thread = new Thread(ListenLoop) { IsBackground = true, Name = "DebugFileServer" };
        _thread.Start();

        _logger.LogInformation("Debug file server started on port {Port}", Port);
    }

    public void Stop()
    {
        _listener?.Stop();
        _listener = null;
        _thread = null;
    }

    public void Dispose() => Stop();

    private void ListenLoop()
    {
        while (_listener is { IsListening: true })
        {
            try
            {
                var ctx = _listener.GetContext();
                HandleRequest(ctx);
            }
            catch (HttpListenerException)
            {
                break;
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling HTTP request");
            }
        }
    }

    private void HandleRequest(HttpListenerContext ctx)
    {
        var path = ctx.Request.Url?.AbsolutePath?.TrimStart('/');

        if (string.IsNullOrEmpty(path) || path == "index.html")
        {
            ServeIndex(ctx.Response);
        }
        else
        {
            ServeFile(ctx.Response, path);
        }
    }

    private void ServeIndex(HttpListenerResponse response)
    {
        try
        {
            var files = Directory.GetFiles(_dataDir, "ww_debug_*.wav")
                .OrderByDescending(File.GetCreationTime)
                .Select(Path.GetFileName)
                .ToArray();

            var sb = new StringBuilder();
            sb.Append("<!DOCTYPE html><html><head><meta charset='utf-8'>");
            sb.Append("<title>Debug Audio Files</title>");
            sb.Append("<style>body{font-family:monospace;background:#111;color:#eee;padding:2em}");
            sb.Append("a{color:#6cf;display:block;margin:0.3em 0}h1{color:#fff}</style></head><body>");
            sb.Append($"<h1>Debug Audio ({files.Length} files)</h1>");

            foreach (var file in files)
            {
                sb.Append($"<a href='/{file}' download>{file}</a>");
            }

            sb.Append("</body></html>");

            var data = Encoding.UTF8.GetBytes(sb.ToString());
            response.ContentType = "text/html; charset=utf-8";
            response.ContentLength64 = data.Length;
            response.OutputStream.Write(data, 0, data.Length);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error serving index");
            response.StatusCode = 500;
        }
        finally
        {
            response.Close();
        }
    }

    private void ServeFile(HttpListenerResponse response, string fileName)
    {
        try
        {
            // Prevent directory traversal
            if (fileName.Contains("..") || fileName.Contains('/') || fileName.Contains('\\'))
            {
                response.StatusCode = 400;
                response.Close();
                return;
            }

            var filePath = Path.Combine(_dataDir, fileName);
            if (!File.Exists(filePath))
            {
                response.StatusCode = 404;
                response.Close();
                return;
            }

            var data = File.ReadAllBytes(filePath);
            response.ContentType = "audio/wav";
            response.ContentLength64 = data.Length;
            response.AddHeader("Content-Disposition", $"attachment; filename=\"{fileName}\"");
            response.OutputStream.Write(data, 0, data.Length);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error serving file {FileName}", fileName);
            response.StatusCode = 500;
        }
        finally
        {
            response.Close();
        }
    }
}
