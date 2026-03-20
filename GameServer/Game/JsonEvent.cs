using System.Text.Json;
using System.Text.Json.Serialization;

public static class JsonEvent
{
    static JsonSerializerOptions options = new()
    
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public static string Build(string type, object data)
    {
        return JsonSerializer.Serialize(new
        {
            @event = type,
            data
        }, options);
    }
    
}