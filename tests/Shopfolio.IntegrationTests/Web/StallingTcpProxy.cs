using System.Net;
using System.Net.Sockets;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// A loopback TCP relay in front of the test database that can be told to stall: once stalled, it forwards nothing more
/// from the client to the server (a connection that is accepted but never answers), and new connections get no upstream at all.
/// It makes the database really slow, which a held pool slot does not (09 19.2 item 4).
/// </summary>
internal sealed class StallingTcpProxy : IAsyncDisposable
{
    private readonly TcpListener _listener = new(IPAddress.Loopback, 0);
    private readonly CancellationTokenSource _stop = new();
    private readonly string _upstreamHost;
    private readonly int _upstreamPort;
    private readonly Task _accepting;
    private volatile bool _stalled;

    public StallingTcpProxy(string upstreamHost, int upstreamPort, bool stalled)
    {
        _upstreamHost = upstreamHost;
        _upstreamPort = upstreamPort;
        _stalled = stalled;
        _listener.Start();
        Port = ((IPEndPoint)_listener.LocalEndpoint).Port;
        _accepting = AcceptLoopAsync();
    }

    public int Port { get; }

    public void Stall() => _stalled = true;

    public async ValueTask DisposeAsync()
    {
        await _stop.CancelAsync();
        _listener.Stop();
        try
        {
            await _accepting;
        }
        catch (Exception ex) when (ex is OperationCanceledException or ObjectDisposedException or SocketException)
        {
            // The listener is closed; the loop ends.
        }

        _stop.Dispose();
    }

    private async Task AcceptLoopAsync()
    {
        while (!_stop.IsCancellationRequested)
        {
            TcpClient client = await _listener.AcceptTcpClientAsync(_stop.Token);
            _ = Task.Run(() => RelayAsync(client), CancellationToken.None);
        }
    }

    private async Task RelayAsync(TcpClient client)
    {
        using (client)
        {
            try
            {
                if (_stalled)
                {
                    await Task.Delay(Timeout.Infinite, _stop.Token);
                    return;
                }

                using TcpClient upstream = new();
                await upstream.ConnectAsync(_upstreamHost, _upstreamPort, _stop.Token);
                NetworkStream clientStream = client.GetStream();
                NetworkStream upstreamStream = upstream.GetStream();
                await Task.WhenAny(
                    PumpAsync(clientStream, upstreamStream, stallable: true),
                    PumpAsync(upstreamStream, clientStream, stallable: false));
            }
            catch (Exception ex) when (ex is OperationCanceledException or IOException or SocketException or ObjectDisposedException)
            {
                // A closed or aborted connection ends the relay.
            }
        }
    }

    private async Task PumpAsync(NetworkStream from, NetworkStream to, bool stallable)
    {
        byte[] buffer = new byte[8192];
        while (true)
        {
            int read = await from.ReadAsync(buffer, _stop.Token);
            if (read == 0)
            {
                return;
            }

            if (stallable && _stalled)
            {
                await Task.Delay(Timeout.Infinite, _stop.Token);
            }

            await to.WriteAsync(buffer.AsMemory(0, read), _stop.Token);
        }
    }
}
