using System.Net.WebSockets;

public class Session
{
    private readonly Connection a;
    private readonly Connection b;
    private readonly Timer heartbeatTimer;

    public Session(Connection a, Connection b)
    {
        this.a = a;
        this.b = b;

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
        var target = from == a ? b : a;
        await target.Send(new { type = "message", text });
    }

    private void CheckHeartbeat(object? _)
    {
        var now = DateTime.UtcNow;

        if ((now - a.LastHeartbeat).TotalSeconds > 100 ||
            (now - b.LastHeartbeat).TotalSeconds > 100)
        {
            End("timeout");
        }
    }

    public async void End(string reason)
    {
        heartbeatTimer.Dispose();

        await a.Send(new { type = "session_end", reason });
        await b.Send(new { type = "session_end", reason });

        await a.Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, reason, CancellationToken.None);
        await b.Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, reason, CancellationToken.None);
    }
}
