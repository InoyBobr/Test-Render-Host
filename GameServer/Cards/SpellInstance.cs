
public class SpellInstance : CardInstance
{
    public SpellInstance(CardData def, Player owner, GameAPI api, CardZone zone = CardZone.Hand)
        : base(def, owner, api, zone)
    {
    }
}