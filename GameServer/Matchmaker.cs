using System.Net.WebSockets;

public class Matchmaker
{
    private Connection? waitingPlayer;
    private readonly List<Session> sessions = new();

    public async Task HandleClient(WebSocket ws)
    {
        var connection = new Connection(ws);

        if (waitingPlayer == null)
        {
            waitingPlayer = connection;
            await connection.Send(new { type = "waiting" });
        }
        else
        {
            var session = new Session(waitingPlayer, connection);
            sessions.Add(session);
            waitingPlayer = null;

            await session.Start();
        }

        await connection.Listen();
    }
}
