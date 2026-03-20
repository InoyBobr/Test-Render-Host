using System.Collections.Generic;
using System.IO;

public static class CardDatabase
{
    private static readonly Dictionary<string, CardData> _cards = new();

    public static IReadOnlyDictionary<string, CardData> All => _cards;

    public static void LoadFromFolder(string folder)
    {
        _cards.Clear();

        if (!Directory.Exists(folder))
            throw new DirectoryNotFoundException(folder);

        var files = Directory.GetFiles(folder, "*.json", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            var data = CardDataLoader.FromFile(file);

            if (data == null)
                continue;

            if (_cards.ContainsKey(data.CardId))
                throw new Exception($"Duplicate card id: {data.CardId}");

            _cards[data.CardId.ToLower()] = data;
        }
    }

    public static CardData Get(string id)
    {
        if (!_cards.TryGetValue(id.ToLower(), out var card))
            throw new KeyNotFoundException($"Card not found: {id}");

        return card;
    }

    public static bool TryGet(string id, out CardData card)
    {
        return _cards.TryGetValue(id.ToLower(), out card);
    }
}