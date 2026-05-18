namespace GameServer.Abilities;
[AbilityId("water_elemental_ability")]
public class Water_Elemental_Ability : AbilityLogic
{
    public Water_Elemental_Ability(AbilityState state) : base(state) {}
    
    
    public override void OnGain()
    {
        Bus.Subscribe<CardKillRequestEvent>(AvoidDeathFromAlly, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<CardKillRequestEvent>(AvoidDeathFromAlly, Owner);
    }

    private void AvoidDeathFromAlly(CardKillRequestEvent e)
    {
        if(!OnBoardAbilityActive)
            return;
        if (e.Card != Owner || e.Source?.Owner != Owner.Owner || !e.Allowed)
            return;
        e.Allowed = false;
        if (!State.IntValues.TryGetValue("damage", out var damage))
            return;
        Bus.Publish(new CardNonCombatDamageRequestEvent(OwnerUnit, damage, Owner));
    }
    
}