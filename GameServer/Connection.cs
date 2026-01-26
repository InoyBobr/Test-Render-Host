using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

public class Connection
{
    public WebSocket Socket { get; }
    public DateTime LastHeartbeat { get; private set; } = DateTime.UtcNow;
    public Session? Session { get; set; }
    public bool IsDead { get; private set; }

    private readonly Action<Connection> onDead;

    public Connection(WebSocket socket, Action<Connection> onDead)
    {
        Socket = socket;
        this.onDead = onDead;
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
                    LastHeartbeat = DateTime.UtcNow;

                    if (Session == null)
                    {
                        await Send(new
                        {
                            type = "heartbeat_ack",
                            time = DateTime.UtcNow
                        });
                    }
                }
                else if (type == "send" && Session != null)
                {
                    var text = doc.RootElement.GetProperty("text").GetString();
                    if (text != null)
                        Session.Relay(this, text);
                }
            }
        }
        catch
        {
            // соединение умерло
        }
        finally
        {
            IsDead = true;
            onDead(this);
            Session?.End("connection_lost");
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
}
