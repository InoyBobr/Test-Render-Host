var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseWebSockets();

var matchmaker = new Matchmaker();

app.Map("/ws", async context =>
{
    if (!context.WebSockets.IsWebSocketRequest)
    {
        context.Response.StatusCode = 400;
        return;
    }

    var ws = await context.WebSockets.AcceptWebSocketAsync();
    await matchmaker.HandleClient(ws);
});

app.Run();
