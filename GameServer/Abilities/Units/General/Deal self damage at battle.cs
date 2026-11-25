namespace GameServer.Abilities;
[AbilityId("deal_self_damage_at_battle")]
public class Deal_self_damage_at_battle : AbilityLogic
{
    public Deal_self_damage_at_battle(AbilityState state) : base(state) {}
    
    public override void OnGain()
    {
        Bus.Subscribe<BattlePhaseEnded>(DealSelfDamageOnEndTurn, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<BattlePhaseEnded>(DealSelfDamageOnEndTurn, Owner);
    }

    private void DealSelfDamageOnEndTurn(BattlePhaseEnded e)
    {
        if(!OnBoardAbilityActive)
            return;
        if (!State.IntValues.TryGetValue("damage", out var damage))
            return;
        Bus.Publish(new CardNonCombatDamageRequestEvent(OwnerUnit, damage, Owner));
    }
}