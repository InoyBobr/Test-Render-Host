namespace GameServer.Abilities;

public class DieAfterBattle(AbilityState state) : AbilityLogic(state)
{
    public override void OnGain()
    {
        Bus.Subscribe<PostBattlePhaseStarted>(Die, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<PostBattlePhaseStarted>(Die, Owner);
    }

    private void Die(PostBattlePhaseStarted e)
    {
        if(!OnBoardAbilityActive)
            return;
        Bus.Publish(new CardKillRequestEvent(OwnerUnit, Owner));
    }
}