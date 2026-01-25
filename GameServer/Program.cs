var builder = WebApplication.CreateBuilder(args);
long totalWsRequests = 0;
long acceptedWsRequests = 0;
var app = builder.Build();

app.UseWebSockets();

var matchmaker = new Matchmaker();

app.Map("/ws", async context =>
{
    Interlocked.Increment(ref totalWsRequests);

    if (!context.WebSockets.IsWebSocketRequest)
    {
        context.Response.StatusCode = 400;
        return;
    }

    Interlocked.Increment(ref acceptedWsRequests);

    var ws = await context.WebSockets.AcceptWebSocketAsync();
    await matchmaker.HandleClient(ws);
});

app.MapGet("/stats", () =>
{
    return new
    {
        totalWsRequests,
        acceptedWsRequests,
        time = DateTime.UtcNow
    };
});

app.Run();

