namespace GameServer.Abilities;

public class Sleep_card_played_by_opponent(AbilityState state) : AbilityLogic(state)
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
        if (!OnBoardAbilityActive)
            return;

        if (e.Card.Owner == Owner.Owner)
            return;

        if (e.Card is not UnitInstance unit)
            return;
        if(e.Card.Owner == Owner._api.FirstPlayer)
            return;
        Bus.Publish(new AddKeywordRequestEvent(Keyword.Sleeping, unit, Owner));
    }
}