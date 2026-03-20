using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

public class Connection
{
    public WebSocket Socket { get; }
    public DateTime LastHeartbeat { get; private set; } = DateTime.UtcNow;
    public Session? Session { get; set; }
    public FirstSession? FirstSession { get; set; }
    public bool IsDead { get; private set; }
    public bool IsSleeping { get; private set; }
    public List<string>? DeckIds { get; private set; }
    public bool Validated { get; private set; }

    public event Action<Connection>? OnDead;
    public event Action<Connection>? OnHeartbeat;
    public event Action<Connection>? OnSleep;
    public event Action<Connection>? OnWakeUp;
    public event Action<Connection>? OnDeckReady; 

    private readonly TimeSpan sleepTimeout = TimeSpan.FromSeconds(6);
    private readonly Timer sleepTimer;

    public Connection(WebSocket socket)
    {
        Socket = socket;
        sleepTimer = new Timer(CheckSleep, null, 2000, 2000);
    }

    public async Task Listen()
    {
        var buffer = new byte[4096];

        try
        {
            while (Socket.State == WebSocketState.Open)
            {
                var result = await Socket.ReceiveAsync(buffer, CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                    break;

                var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                using var doc = JsonDocument.Parse(json);

                if (!doc.RootElement.TryGetProperty("type", out var t))
                    continue;

                var type = t.GetString();

                if (type == "heartbeat")
                {
                    var delta = DateTime.UtcNow - LastHeartbeat;
                    LastHeartbeat = DateTime.UtcNow;
                    OnHeartbeat?.Invoke(this);
                    if (IsSleeping)
                    {
                        IsSleeping = false;
                        OnWakeUp?.Invoke(this);
                    }

                    if (Session == null)
                    {
                        await Send(new
                        {
                            type = "heartbeat_ack",
                            time = delta.TotalMilliseconds
                        });
                    }
                }
                else if (type == "deck")
                {
                    if (Validated)
                        continue;

                    if (!TryValidateDeck(doc.RootElement, out var ids))
                    {
                        await Reject("Invalid Deck");
                        return;
                    }

                    DeckIds = ids;
                    Validated = true;

                    OnDeckReady?.Invoke(this);

                    await Send(new { type = "deck_accepted" });
                }
                else if (type == "send" && Session != null)
                {
                    var text = doc.RootElement.GetProperty("text").GetString();
                    if (text != null)
                        Session.HandleMessage(this, text);
                }
            }
        }
        catch
        {
            // соединение умерло
        }
        finally
        {
            Kill();
        }
    }

    public async Task Send(object obj)
    {
        if (Socket.State != WebSocketState.Open)
        {
            Console.WriteLine("Send skipped: socket state = " + Socket.State);
            return;
        }

        var json = JsonSerializer.Serialize(obj);
        var bytes = Encoding.UTF8.GetBytes(json);

        try
        {
            await Socket.SendAsync(
                new ArraySegment<byte>(bytes),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None
            );

            Console.WriteLine("Sent: " + json);
        }
        catch (WebSocketException ex)
        {
            Console.WriteLine("Send failed: " + ex.Message);
        }
    }

    private void CheckSleep(object? _)
    {
        if (IsDead) return;

        if (DateTime.UtcNow - LastHeartbeat > sleepTimeout && !IsSleeping)
        {
            IsSleeping = true;
            OnSleep?.Invoke(this);
        }
    }

    private void Kill()
    {
        if (IsDead) return;

        IsDead = true;
        sleepTimer.Dispose();

        OnDead?.Invoke(this);
        Session?.End("connection_lost");
    }

    private bool TryValidateDeck(JsonElement root, out List<string> deckIds)
    {
        deckIds = new List<string>();

        if (!root.TryGetProperty("cards", out var cardsEl) ||
            cardsEl.ValueKind != JsonValueKind.Array)
            return false;

        foreach (var el in cardsEl.EnumerateArray())
        {
            var id = el.GetString();

            if (string.IsNullOrWhiteSpace(id))
                return false;

            if (!CardDatabase.TryGet(id, out _))
                return false;

            deckIds.Add(id);
        }

        if (deckIds.Count == 0)
            return false;

        return true;
    }
    
    private async Task Reject(string reason)
    {
        if (Socket.State == WebSocketState.Open)
        {
            await Send(new { type = "error", reason });

            await Socket.CloseAsync(
                WebSocketCloseStatus.PolicyViolation,
                reason,
                CancellationToken.None
            );
        }

        Kill();
    }
}
