var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var counter = new CounterService();

app.MapGet("/", () => "Game server is running");
app.MapGet("/counter", () => counter.Value);
app.MapPost("/counter/increment", () =>
{
    counter.Increment();
    return Results.Ok(counter.Value);
});

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://0.0.0.0:{port}");

app.Run();
