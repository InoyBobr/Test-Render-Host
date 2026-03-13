public static class CardFactory
{
    public static CardInstance Create(CardData data, Player owner, GameAPI api, CardZone zone = CardZone.Deck)
    {
        return data.Type switch
        {
            CardType.Unit => new UnitInstance(data, owner, api, zone),
            CardType.Spell => new SpellInstance(data, owner, api, zone),
            _ => throw new Exception("Unknown card type")
        };
    }
}