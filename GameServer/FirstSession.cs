using System.Net.WebSockets;
using System.Threading;
using System.Text.Json;
using System.Text.Json.Serialization;

public class FirstSession
{
    private readonly Connection a;
    private readonly Connection b;
    private readonly Timer heartbeatTimer;
    private readonly Action<FirstSession> onEnded;

    private int ended = 0; // 0 = active, 1 = ended

    public FirstSession(Connection a, Connection b, Action<FirstSession> onEnded)
    {
        this.a = a;
        this.b = b;
        this.onEnded = onEnded;

        a.Session = this;
        b.Session = this;

        heartbeatTimer = new Timer(CheckHeartbeat, null, 5000, 5000);
    }

    public async Task Start()
    {
        await a.Send(new { type = "match_found" });
        await b.Send(new { type = "match_found" });
    }

    public async void Relay(Connection from, string text)
    {
        if (ended == 1) return;

        var target = from == a ? b : a;
        await target.Send(new { type = "message", text });
    }

    private void CheckHeartbeat(object? _)
    {
        if (ended == 1) return;

        var now = DateTime.UtcNow;

        if ((now - a.LastHeartbeat).TotalSeconds > 100 ||
            (now - b.LastHeartbeat).TotalSeconds > 100)
        {
            End("timeout");
        }
    }

    public async void End(string reason)
    {
        // 🔒 Гарантия: End выполняется один раз
        if (Interlocked.Exchange(ref ended, 1) == 1)
            return;

        heartbeatTimer.Dispose();

        await a.Send(new { type = "session_end", reason });
        await b.Send(new { type = "session_end", reason });

        SafeClose(a);
        SafeClose(b);

        onEnded(this);
    }

    private async void SafeClose(Connection c)
    {
        try
        {
            if (c.Socket.State == WebSocketState.Open ||
                c.Socket.State == WebSocketState.CloseReceived)
            {
                await c.Socket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "session_end",
                    CancellationToken.None
                );
            }
        }
        catch
        {
            // игнорируем — сокет уже мёртв
        }
    }
}
