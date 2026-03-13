using System.Collections.Generic;

public class CardData
{
    public string CardId { get; init; }

    public CardType Type { get; init; }

    public int BasePower { get; init; }
    public int BaseHealth { get; init; }
    public StickerColor Color { get; init; }
    public List<AbilityDefinition> Abilities { get; init; }
    public List<Keyword> Keywords { get; init; }
}

