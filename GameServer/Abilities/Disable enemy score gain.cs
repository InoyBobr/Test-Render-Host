namespace GameServer.Abilities;
[AbilityId("disable_enemy_score_gain")]
public class Disable_enemy_score_gain : AbilityLogic
{
    public Disable_enemy_score_gain(AbilityState state) : base(state) {}
    
    public override void OnGain()
    {
        Bus.Subscribe<PlayerScoreRequestEvent>(DisableEnemyScoreGain, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<PlayerScoreRequestEvent>(DisableEnemyScoreGain, Owner);
    }

    private void DisableEnemyScoreGain(PlayerScoreRequestEvent e)
    {
        if (!OnBoardAbilityActive)
            return;
        if (e.Player == Owner.Owner)
            return;
        e.Allowed = false;
    }
}