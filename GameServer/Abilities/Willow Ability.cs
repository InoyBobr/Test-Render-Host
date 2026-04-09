[AbilityId("willow_ability")]
public class WillowAbility : AbilityLogic
{
    public WillowAbility(AbilityState state) : base(state) {}
    
    public override void OnGain()
    {
        Bus.Subscribe<BattlePhaseEnded>(OnBattleEnded, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<BattlePhaseEnded>(OnBattleEnded, Owner);
    }

    private void OnBattleEnded(BattlePhaseEnded e)
    {
        if (!OnBoardAbilityActive)
            return;
        var ctx = Owner._api.GetContext(Owner.Owner);
        if (ctx == null)
            return;
        var allEnemies = ctx.GetEnemyCards(Owner);
        if (!State.IntValues.TryGetValue("damage", out var damage))
            return;
        foreach (var enemy in allEnemies)
        {
            if (Owner.Position / 3 == enemy.Position / 3)
            {
                Bus.Publish(new CardNonCombatDamageRequestEvent(enemy, damage, Owner));
            }
        }
    }
}