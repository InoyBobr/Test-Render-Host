using System.Net.WebSockets;

public class Matchmaker
{
    private readonly object sync = new();

    private readonly List<Connection> waiting = new();
    private readonly List<Connection> sleeping = new();
    private readonly List<ConnectionPair> pairs = new();
    private readonly List<Session> sessions = new();


    public async Task HandleClient(WebSocket ws)
    {
        var c = new Connection(ws);
        
        c.OnDead += OnConnectionDead;
        c.OnSleep += OnConnectionSleep;
        c.OnWakeUp += OnConnectionWakeUp;

        lock (sync)
        {
            waiting.Add(c);
            TryCreatePair();
        }

        if (c.Session == null)
        {
            await c.Send(new { type = "waiting" });
        }

        await c.Listen();
    }

    private void TryCreatePair()
    {
        while (waiting.Count >= 2)
        {
            var a = waiting[0];
            var b = waiting[1];
            waiting.RemoveRange(0, 2);

            var pair = new ConnectionPair(a, b);
            pair.OnConfirmed += OnPairConfirmed;
            pair.OnBroken += OnPairBroken;

            pairs.Add(pair);
        }
    }

    private void OnPairConfirmed(ConnectionPair pair)
    {
        lock (sync)
        {
            pairs.Remove(pair);
            pair.OnConfirmed -= OnPairConfirmed;
            pair.OnBroken -= OnPairBroken;

            var (a, b) = pair.GetConnections();
            var session = new Session(a, b, OnSessionEnded);

            sessions.Add(session);
            _ = session.Start();
        }
    }

    private void OnPairBroken(ConnectionPair pair)
    {
        lock (sync)
        {
            pairs.Remove(pair);
            pair.OnConfirmed -= OnPairConfirmed;
            pair.OnBroken -= OnPairBroken;

            var (a, b) = pair.GetConnections();

            Requeue(a);
            Requeue(b);

            TryCreatePair();
        }
    }

    private void Requeue(Connection c)
    {
        if (c.IsDead) return;

        if (c.IsSleeping)
            sleeping.Add(c);
        else
            waiting.Add(c);
    }

    

    private void OnSessionEnded(Session session)
    {
        lock (sync)
        {
            sessions.Remove(session);
        }
    }

    private void OnConnectionSleep(Connection c)
    {
        lock (sync)
        {
            // если в сессии — игнорируем
            if (c.Session != null)
                return;
    
            // если был в waiting
            if (waiting.Remove(c))
            {
                sleeping.Add(c);
                return;
            }
    
            // если был в паре — пару разорвёт Pair
            // Pair сам вызовет OnPairBroken
        }
    }

    private void OnConnectionWakeUp(Connection c)
    {
        lock (sync)
        {
            if (sleeping.Remove(c))
            {
                waiting.Add(c);
                TryCreatePair();
            }
        }
    }



    private void OnConnectionDead(Connection c)
    {
        c.OnDead -= OnConnectionDead;
        c.OnSleep -= OnConnectionSleep;
        c.OnWakeUp -= OnConnectionWakeUp;

        lock (sync)
        {
            waiting.Remove(c);
            sleeping.Remove(c);
            Console.WriteLine("Connection Died");
        }
    }
}
