namespace GameServer.Abilities;
[AbilityId("sleep_the_weakest_enemy_before_battle")]
public class Sleep_the_weakest_enemy_before_battle(AbilityState state) : AbilityLogic(state)
{
    public override void OnGain()
    {
        Bus.Subscribe<PreBattlePhaseStarted>(SleepTheWeakest, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<PreBattlePhaseStarted>(SleepTheWeakest, Owner);
    }

    private void SleepTheWeakest(PreBattlePhaseStarted e)
    {
        if (!OnBoardAbilityActive)
            return;
        var ctx = Owner._api.GetContext(Owner.Owner);
        var enemies = ctx.GetEnemyCards(Owner).ToList();
        var weakest = enemies.Where(c => c.CurrentPower == enemies.Min(x => x.CurrentPower));
        foreach (var enemy in weakest)
        {
            Bus.Publish(new AddKeywordRequestEvent(Keyword.Sleeping, enemy, Owner));
        }
    }
};