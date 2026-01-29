using System.Net.WebSockets;

namespace GameServer;

public class Matchmaker
{
    private Connection? waitingPlayer;
    private readonly List<Session> sessions = new();
    private readonly object sync = new();

    public async Task HandleClient(WebSocket ws)
    {
        var connection = new Connection(ws, OnConnectionDead);

        lock (sync)
        {
            if (waitingPlayer == null || waitingPlayer.IsDead)
            {
                waitingPlayer = connection;
            }
            else
            {
                var session = new Session(
                    waitingPlayer,
                    connection,
                    OnSessionEnded
                );

                sessions.Add(session);
                waitingPlayer = null;

                _ = session.Start();
            }
        }

        if (connection.Session == null)
        {
            await connection.Send(new { type = "waiting" });
        }

        await connection.Listen();
    }

    private void OnSessionEnded(Session session)
    {
        lock (sync)
        {
            sessions.Remove(session);
        }
    }

    private void OnConnectionDead(Connection c)
    {
        lock (sync)
        {
            if (waitingPlayer == c){
                waitingPlayer = null;
                Console.WriteLine("Connection Died");
            }
        }
    }
}
