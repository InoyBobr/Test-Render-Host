namespace GameServer.Abilities;

public class Die_at_round_start(AbilityState state) : AbilityLogic(state)
{
    public override void OnGain()
    {
        Bus.Subscribe<RoundStarted>(Die, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<RoundStarted>(Die, Owner);
    }

    private void Die(RoundStarted e)
    {
        if(!OnBoardAbilityActive)
            return;
        Bus.Publish(new CardKillRequestEvent(OwnerUnit, Owner));
    }
}