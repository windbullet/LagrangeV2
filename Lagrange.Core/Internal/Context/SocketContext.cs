using System.Buffers.Binary;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Lagrange.Core.Common;
using Lagrange.Core.Internal.Network;

namespace Lagrange.Core.Internal.Context;

internal class SocketContext : IClientListener, IDisposable
{
    private const string Tag = nameof(SocketContext);
    
    public uint HeaderSize => 4;
    
    public bool Connected => _client.Connected;
    
    private readonly ClientListener _client;
    
    private readonly BotConfig _config;
    
    private readonly BotContext _context;
    
    public SocketContext(BotContext context)
    {
        _client = new CallbackClientListener(this);
        _config = context.Config;
        _context = context;
    }

    public uint GetPacketLength(ReadOnlySpan<byte> header) => BinaryPrimitives.ReadUInt32BigEndian(header);

    public void OnRecvPacket(ReadOnlySpan<byte> packet) => _context.PacketContext.DispatchPacket(packet);

    public void OnDisconnect()
    {
        
    }

    public void OnSocketError(Exception e, ReadOnlyMemory<byte> data)
    {
        
    }
    
    public async Task<bool> Connect()
    {
        if (_client.Connected) return true;
        
        var servers = await ResolveDns();
        if (_config.GetOptimumServer) await SortServers(servers);
        bool connected = await _client.Connect(servers[0]);
        
        if (connected) _context.LogInfo(Tag, $"Connected to the server {servers[0]}");
        else _context.LogError(Tag, $"Failed to connect to the server {servers[0]}");
        
        return connected;
    }
    
    public void Disconnect() => _client.Disconnect();
    
    public ValueTask<int> Send(ReadOnlyMemory<byte> packet) => _client.Send(packet);
    
    private async Task SortServers(Uri[] servers)
    {
        using var ping = new Ping();
        var sorted = new List<(long, Uri)>(servers.Length);
        
        foreach (var server in servers)
        {
            var latency = await ping.SendPingAsync(server.Host, 1000);
            if (latency.Status == IPStatus.Success)
            {
                sorted.Add((latency.RoundtripTime, server));
                _context.LogDebug(Tag, $"Server: {server} Latency: {latency.RoundtripTime}ms");
            }
        }
        
        sorted.Sort((a, b) => a.Item1.CompareTo(b.Item1));
        for (int i = 0; i < sorted.Count; i++) servers[i] = sorted[i].Item2;
    }
    
    private static async Task<Uri[]> ResolveDns(bool useIPv6Network = false)
    {
        string host = useIPv6Network ? "msfwifiv6.3g.qq.com" : "msfwifi.3g.qq.com";
        var entry = await Dns.GetHostEntryAsync(host, useIPv6Network ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork);
        var result = new Uri[entry.AddressList.Length];
        
        for (int i = 0; i < entry.AddressList.Length; i++) result[i] = new Uri($"http://{entry.AddressList[i]}:8080");

        return result;
    }
    
    public void Dispose()
    {
        _client.Disconnect();
    }
}