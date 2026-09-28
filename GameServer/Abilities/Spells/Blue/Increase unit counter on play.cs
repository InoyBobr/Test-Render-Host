namespace GameServer.Abilities.Spells.Blue;
[AbilityId("wealth_ability")]
public class Increase_unit_counter_on_play: AbilityLogic
{
    public Increase_unit_counter_on_play(AbilityState state) : base(state) {}
    
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
        Bus.Publish(new ChangeRemainingUnitCounterRequestEvent(1, false, false, false, Owner));
    }
}