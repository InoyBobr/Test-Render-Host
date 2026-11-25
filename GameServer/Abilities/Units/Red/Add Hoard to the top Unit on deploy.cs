namespace GameServer.Abilities;

public class Add_Horde_to_the_top_Unit_on_deploy(AbilityState state) : AbilityLogic(state)
{
    public override void OnGain()
    {
        Bus.Subscribe<CardPlayedEvent>(OnCardPlayed, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<CardPlayedEvent>(OnCardPlayed, Owner);
    }

    private void OnCardPlayed(CardPlayedEvent e)
    {
        if(e.Card != Owner)
            return;
        var units = Owner.Owner.GetDeck().OfType<UnitInstance>();
        var nonHorde = units.Where(c => !c.Keywords.Contains(Keyword.Horde));
        var amount = State.IntValues.GetValueOrDefault("amount", 1);
        var targets = nonHorde.Take(amount);
        foreach (var target in targets)
        {
            Bus.Publish(new AddKeywordRequestEvent(Keyword.Horde, target, Owner));
        }
        
    }
}