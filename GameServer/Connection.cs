using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

public class Connection
{
    public WebSocket Socket { get; }
    public DateTime LastHeartbeat { get; private set; } = DateTime.UtcNow;
    public Session? Session { get; set; }

    public Connection(WebSocket socket)
    {
        Socket = socket;
    }

    public async Task Listen()
    {
        var buffer = new byte[4096];

        while (Socket.State == WebSocketState.Open)
        {
            var result = await Socket.ReceiveAsync(buffer, CancellationToken.None);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                Session?.End("disconnect");
                return;
            }

            var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
            var doc = JsonDocument.Parse(json);
            var type = doc.RootElement.GetProperty("type").GetString();

            if (type == "heartbeat")
            {
                LastHeartbeat = DateTime.UtcNow;
            }
            else if (type == "send")
            {
                var text = doc.RootElement.GetProperty("text").GetString();
                Session?.Relay(this, text!);
            }
        }
    }

    public async Task Send(object obj)
    {
        var json = JsonSerializer.Serialize(obj);
        var bytes = Encoding.UTF8.GetBytes(json);
        await Socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
    }
}
