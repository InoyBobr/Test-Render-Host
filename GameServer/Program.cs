var builder = WebApplication.CreateBuilder(args);
long totalWsRequests = 0;
long acceptedWsRequests = 0;
long statRequests = 0;
var app = builder.Build();

var json = @"
{
  ""CardId"": ""fire_elemental"",
  ""basePower"": 3,
  ""baseHealth"": 5,
  ""color"": ""Red"",
  ""keywords"": [""Charge"", ""Burn""],
  ""abilities"": [
    {
      ""AbilityId"": ""DealDamage"",
      ""Parameters"": [
        { ""key"": ""amount"", ""value"": 3 }
      ]
    }
  ]
}";

var data = CardDataLoader.FromJson(json);
Console.WriteLine(data.basePower);
Console.WriteLine(data.keywords.Count);
Console.WriteLine(data.baseHealth);

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
    Interlocked.Increment(ref statRequests);
    return new
    {
        statRequests,
        totalWsRequests,
        acceptedWsRequests,
        data.basePower,
        data.keywords.Count,
        data.baseHealth,
        time = DateTime.UtcNow
    };
});

app.Run();


    










