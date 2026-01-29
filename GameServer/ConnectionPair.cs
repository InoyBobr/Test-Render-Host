public class ConnectionPair
{
    private readonly Connection a;
    private readonly Connection b;

    private bool aConfirmed;
    private bool bConfirmed;
    private bool isBroken;

    public event Action<ConnectionPair>? OnConfirmed;
    public event Action<ConnectionPair>? OnBroken;

    public ConnectionPair(Connection a, Connection b)
    {
        this.a = a;
        this.b = b;

        a.Session = null;

        a.OnHeartbeat += OnHeartbeat;
        b.OnHeartbeat += OnHeartbeat;

        a.OnDead += OnDead;
        b.OnDead += OnDead;

        a.OnSleep += OnSleep;
        b.OnSleep += OnSleep;
    }

    private void OnHeartbeat(Connection c)
    {
        if (isBroken) return;

        if (c == a) aConfirmed = true;
        if (c == b) bConfirmed = true;

        if (aConfirmed && bConfirmed)
        {
            Cleanup();
            OnConfirmed?.Invoke(this);
        }
    }

    private void OnDead(Connection _)
    {
        Break();
    }

    private void OnSleep(Connection _)
    {
        Break();
    }

    private void Break()
    {
        if (isBroken) return;
        isBroken = true;

        Cleanup();
        OnBroken?.Invoke(this);
    }

    private void Cleanup()
    {
        a.OnHeartbeat -= OnHeartbeat;
        b.OnHeartbeat -= OnHeartbeat;
        a.OnDead -= OnDead;
        b.OnDead -= OnDead;
        a.OnSleep -= OnSleep;
        b.OnSleep -= OnSleep;
    }

    public (Connection, Connection) GetConnections() => (a, b);
}
