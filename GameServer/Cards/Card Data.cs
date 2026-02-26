using System.Collections.Generic;

public class CardData
{
    public string CardId;

    public int basePower;
    public int baseHealth;
    public StickerColor color;
    public List<AbilityDefinition> abilities;
    public List<Keyword> keywords;
}
