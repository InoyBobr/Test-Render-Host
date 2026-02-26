using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

public static class CardDataLoader
{
    private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false)
        }
    };

    public static CardData FromJson(string json)
    {
        return JsonSerializer.Deserialize<CardData>(json, _options);
    }

    public static CardData FromFile(string path)
    {
        var json = File.ReadAllText(path);
        return FromJson(json);
    }
}