public class Session
{
    private readonly Connection a;
    private readonly Connection b;
    private readonly Timer heartbeatTimer;
    private bool ended = false;
    private readonly object endLock = new object();

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

    public async Task Relay(Connection from, string text)
    {
        var target = from == a ? b : a;
        await target.Send(new { type = "message", text });
    }

    private void CheckHeartbeat(object? _)
    {
        var now = DateTime.UtcNow;

        if ((now - a.LastHeartbeat).TotalSeconds > 10 ||
            (now - b.LastHeartbeat).TotalSeconds > 10)
        {
            _ = End("timeout");  // fire-and-forget
        }
    }

    public async Task End(string reason)
    {
        lock (endLock)
        {
            if (ended) return;
            ended = true;
        }

        heartbeatTimer.Dispose();

        // отправляем, только если сокет открыт
        await a.Send(new { type = "session_end", reason });
        await b.Send(new { type = "session_end", reason });

        try
        {
            if (a.Socket.State == WebSocketState.Open || a.Socket.State == WebSocketState.CloseReceived)
                await a.Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, reason, CancellationToken.None);
        }
        catch { }

        try
        {
            if (b.Socket.State == WebSocketState.Open || b.Socket.State == WebSocketState.CloseReceived)
                await b.Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, reason, CancellationToken.None);
        }
        catch { }
    }
}
