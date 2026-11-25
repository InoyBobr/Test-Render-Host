namespace GameServer.Abilities;

public class Add_hoard_counter_on_deploy(AbilityState state) : AbilityLogic(state)
{
    public override void OnGain()
    {
        Bus.Subscribe<CardPlayedEvent>(OnDeploy, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<CardPlayedEvent>(OnDeploy, Owner);
    }

    private void OnDeploy(CardPlayedEvent e)
    {
        if(e.Card != Owner)
            return;
        var amount = State.IntValues.GetValueOrDefault("amount", 1); 
        Bus.Publish(new ChangeRemainingUnitCounterRequestEvent(amount, true, false, false, Owner));
    }
}