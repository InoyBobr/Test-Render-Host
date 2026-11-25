namespace GameServer.Abilities.Spells.Blue;
[AbilityId("wealth_ability")]
public class Increase_unit_counter_on_play(AbilityState state) : AbilityLogic(state)
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
        Bus.Publish(new ChangeRemainingUnitCounterRequestEvent(amount, false, false, false, Owner));
    }
}