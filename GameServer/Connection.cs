using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

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

        try
        {
            while (Socket.State == WebSocketState.Open)
            {
                var result = await Socket.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    CancellationToken.None
                );

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    Console.WriteLine("Client sent CLOSE");
                    Session?.End("disconnect");
                    return;
                }

                var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Console.WriteLine("Received: " + json);

                using var doc = JsonDocument.Parse(json);

                if (!doc.RootElement.TryGetProperty("type", out var typeProp))
                    continue;

                var type = typeProp.GetString();

                if (type == "heartbeat")
                {
                    LastHeartbeat = DateTime.UtcNow;

                    // ❗ ВСЕГДА отвечаем, если нет сессии
                    if (Session == null)
                    {
                        await Send(new
                        {
                            type = "heartbeat_ack",
                            time = DateTime.UtcNow
                        });
                    }
                }
                else if (type == "send")
                {
                    if (Session == null)
                        continue;

                    if (!doc.RootElement.TryGetProperty("text", out var textProp))
                        continue;

                    var text = textProp.GetString();
                    if (text != null)
                        Session.Relay(this, text);
                }
            }
        }
        catch (WebSocketException ex)
        {
            Console.WriteLine("WebSocket exception in Listen: " + ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine("General exception in Listen: " + ex);
        }
        finally
        {
            Console.WriteLine("Listen loop ended");
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
