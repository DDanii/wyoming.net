using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Wyoming.Net.Satellite.App.Tz.Platform;

public sealed class RemoteLogger : IDisposable
{
    private static RemoteLogger? singleton;

    private readonly UdpClient _udpClient;
    private readonly IPEndPoint _endPoint;
    private bool _isDisposed;

    public RemoteLogger(string ipAddress, int port)
    {
        _udpClient = new UdpClient();
        _endPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
    }

    public static void InitSingleton(string ipAddress, int port)
    {
        singleton = new RemoteLogger(ipAddress, port);
    }

    public static RemoteLogger? Singleton => singleton;

    public void Log(string message, string level = "INFO")
    {
        try
        {
            string payload = $"[{DateTime.Now:HH:mm:ss}] [{level}] {message}";
            byte[] bytes = Encoding.UTF8.GetBytes(payload);
            
            _udpClient.Send(bytes, bytes.Length, _endPoint);
        }
        catch (Exception ex)
        {
            //fallback to the internal dlog if UDP fails
            Tizen.Log.Error("REMOTE_LOG", $"Failed to send UDP log: {ex.Message}");
        }
    }

    public void Dispose()
    {
        if (!_isDisposed)
        {
            _udpClient?.Close();
            _isDisposed = true;
        }
    }
}