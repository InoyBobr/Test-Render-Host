namespace GameServer.Abilities;
[AbilityId("duelist_ability")]
public class DuelistAbility : AbilityLogic
{
    public DuelistAbility(AbilityState state) : base(state) {}
    
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
        var enemies = ctx.GetEnemyCardsOnFace(OwnerUnit);
        var allies = ctx.GetFriendlyCardsOnFace(OwnerUnit);
        var unitInstances = enemies as UnitInstance[] ?? enemies.ToArray();
        if(!(unitInstances.Length == 1 && !allies.Any()))
            return;
        foreach (var enemy in unitInstances)
        {
            Bus.Publish(new CardKillRequestEvent(enemy, Owner));
        }
    }
}