namespace GameServer.Abilities;

public class Add_shield_if_played_near_Orc(AbilityState state) : AbilityLogic(state)
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
        var ctx = Owner._api.GetContext(Owner.Owner);
        if (ctx.GetFriendlyCardsOnFace(OwnerUnit).Any(c => c.Tags.Contains(CardTag.Orc)))
        {
            Bus.Publish(new AddKeywordRequestEvent(Keyword.Shield, OwnerUnit, Owner));
        }

    }
}