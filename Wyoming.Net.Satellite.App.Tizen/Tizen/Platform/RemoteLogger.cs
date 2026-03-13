using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace Wyoming.Net.Satellite.App.Tz.Platform;

public sealed class RemoteLogger : IDisposable
{
    private static RemoteLogger? singleton;

    private readonly UdpClient _udpClient;

    private readonly IPEndPoint _endPoint;

    private bool _connected;

    private bool _isDisposed;

    private RemoteLogger(string ipAddress, int port)
    {
        _udpClient = new UdpClient();
        _endPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
        _connected = true;
    }

    public bool Enabled => _connected;

    public static void InitSingleton(string ipAddress, int port)
    {
        singleton = new RemoteLogger(ipAddress, port);
    }

    public static RemoteLogger? Singleton => singleton;

    public void Log(string message, string level = "INFO")
    {
        if(!_connected)
        {
            return;
        }

        try
        {
            string payload = message;//$"[{DateTime.Now:HH:mm:ss}] [{level}] {message}";
            Span<byte> bytes = stackalloc byte[Encoding.UTF8.GetByteCount(payload)];
            Encoding.UTF8.GetBytes(payload, bytes);
            
            _udpClient.Send(bytes, _endPoint);
        }
        catch
        {
            _connected = false;
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