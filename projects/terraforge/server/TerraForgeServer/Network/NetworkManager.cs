using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TerraForgeServer.Network
{
    public class NetworkManager
    {
        private HttpListener _listener;
        private ConcurrentDictionary<Guid, PlayerSession> _sessions;
        private CancellationTokenSource _cts;
        private bool _running;
        private const int Port = 8080;

        public event Action<Guid, byte[]> OnPacketReceived;
        public event Action<Guid> OnClientConnected;
        public event Action<Guid> OnClientDisconnected;

        public NetworkManager()
        {
            _sessions = new ConcurrentDictionary<Guid, PlayerSession>();
            _cts = new CancellationTokenSource();
        }

        public async Task StartAsync()
        {
            if (_running) return;

            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://*:{Port}/ws/");
            _listener.Start();
            _running = true;

            Console.WriteLine($"[NetworkManager] WebSocket server started on port {Port}");

            while (!_cts.Token.IsCancellationRequested)
            {
                try
                {
                    var context = await _listener.GetContextAsync();
                    _ = HandleConnectionAsync(context);
                }
                catch (Exception ex)
                {
                    if (!_cts.Token.IsCancellationRequested)
                        Console.WriteLine($"[NetworkManager] Error: {ex.Message}");
                }
            }
        }

        private async Task HandleConnectionAsync(HttpListenerContext context)
        {
            if (!context.Request.IsWebSocketRequest)
            {
                context.Response.StatusCode = 400;
                context.Response.Close();
                return;
            }

            var wsContext = await context.AcceptWebSocketAsync(null);
            var ws = wsContext.WebSocket;
            var sessionId = Guid.NewGuid();

            var session = new PlayerSession(sessionId, ws, this);
            _sessions[sessionId] = session;

            OnClientConnected?.Invoke(sessionId);
            Console.WriteLine($"[NetworkManager] Client connected: {sessionId}");

            try
            {
                await session.ProcessAsync(_cts.Token);
            }
            finally
            {
                _sessions.TryRemove(sessionId, out _);
                OnClientDisconnected?.Invoke(sessionId);
                Console.WriteLine($"[NetworkManager] Client disconnected: {sessionId}");
            }
        }

        public void RoutePacket(Guid sessionId, byte[] data)
        {
            OnPacketReceived?.Invoke(sessionId, data);
        }

        public async Task SendToClientAsync(Guid sessionId, byte[] data)
        {
            if (_sessions.TryGetValue(sessionId, out var session))
            {
                await session.SendAsync(data);
            }
        }

        public async Task BroadcastAsync(byte[] data)
        {
            foreach (var session in _sessions.Values)
            {
                await session.SendAsync(data);
            }
        }

        public void Stop()
        {
            _running = false;
            _cts.Cancel();
            _listener?.Close();
        }
    }

    public class PlayerSession
    {
        public Guid Id { get; }
        public WebSocket Socket { get; }
        public DateTime ConnectedAt { get; }
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }

        private readonly NetworkManager _manager;
        private readonly byte[] _receiveBuffer;

        public PlayerSession(Guid id, WebSocket socket, NetworkManager manager)
        {
            Id = id;
            Socket = socket;
            _manager = manager;
            ConnectedAt = DateTime.UtcNow;
            _receiveBuffer = new byte[8192];
        }

        public async Task ProcessAsync(CancellationToken ct)
        {
            while (Socket.State == WebSocketState.Open && !ct.IsCancellationRequested)
            {
                var result = await Socket.ReceiveAsync(
                    new ArraySegment<byte>(_receiveBuffer), ct);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", ct);
                    break;
                }

                if (result.MessageType == WebSocketMessageType.Binary)
                {
                    var data = new byte[result.Count];
                    Buffer.BlockCopy(_receiveBuffer, 0, data, 0, result.Count);
                    _manager.RoutePacket(Id, data);
                }
            }
        }

        public async Task SendAsync(byte[] data)
        {
            if (Socket.State == WebSocketState.Open)
            {
                await Socket.SendAsync(new ArraySegment<byte>(data),
                    WebSocketMessageType.Binary, true, CancellationToken.None);
            }
        }
    }
}
