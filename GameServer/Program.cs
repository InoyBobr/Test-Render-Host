var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Корневой эндпоинт
app.MapGet("/", () => "Game server is running");

// Эндпоинт времени
app.MapGet("/time", () =>
{
    return new TimeResponse
    {
        Utc = DateTime.UtcNow,
        ServerLocal = DateTime.Now
    };
});

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://0.0.0.0:{port}");

app.Run();


// DTO (просто контейнер данных)
public class TimeResponse
{
    public DateTime Utc { get; set; }
    public DateTime ServerLocal { get; set; }
}
