namespace GameServer.Abilities.Spells.Blue;

public class Discard_random_cards_to_get_unit_counter(AbilityState state) : AbilityLogic(state)
{
    public override void OnGain()
    {
        Bus.Subscribe<CardPlayedEvent>(IncreaseCounter, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<CardPlayedEvent>(IncreaseCounter, Owner);
    }

    private void IncreaseCounter(CardPlayedEvent e)
    {
        if (e.Card != Owner)
            return;
        if(!State.IntValues.TryGetValue("amount", out var amount))
            return;
        if(!State.IntValues.TryGetValue("discard_amount", out var discardValue))
            return;
        Bus.Publish(new DiscardRandomCardRequestEvent(discardValue, Owner.Owner, Owner));
        Bus.Publish(new ChangeRemainingUnitCounterRequestEvent(amount, false, false, false, Owner));
    }

    public override bool CanBePlayed(GameContext ctx)
    {
        if(!State.IntValues.TryGetValue("discard_amount", out var discardValue))
            return false;
        return Owner.Owner.GetHand().Count >= discardValue;
    }
}