namespace GameServer.Abilities;

public class GetExtraRotation(AbilityState state) : AbilityLogic(state)
{
    public override void OnGain()
    {
        Bus.Subscribe<PlayerRotationPhaseStarted>(GetRotation, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<PlayerRotationPhaseStarted>(GetRotation, Owner);
    }

    private void GetRotation(PlayerRotationPhaseStarted e)
    {
        if(!OnBoardAbilityActive)
            return;
        if(e.Player != Owner.Owner)
            return;
        if(!State.IntValues.TryGetValue("amount", out var amount))
            return;
        Bus.Publish(new ChangeRemainingRotationCounterRequestEvent(amount, false, false, Owner));
    }
}