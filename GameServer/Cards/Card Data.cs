using System.Collections.Generic;

public class CardData
{
    public string CardId { get; init; }

    public int basePower { get; init; }
    public int baseHealth { get; init; }
    public StickerColor color { get; init; }
    public List<AbilityDefinition> abilities { get; init; }
    public List<Keyword> keywords { get; init; }
}

