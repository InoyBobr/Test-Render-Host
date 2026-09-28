namespace GameServer.Abilities;
[AbilityId("cannot_take_combat_damage")]
public class Cannot_take_combat_damage : AbilityLogic
{
    public Cannot_take_combat_damage(AbilityState state) : base(state) {}
    
    public override void OnGain()
    {
        Bus.Subscribe<CardCombatDamageRequestEvent>(DisableAttack, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<CardCombatDamageRequestEvent>(DisableAttack, Owner);
    }

    private void DisableAttack(CardCombatDamageRequestEvent e)
    {
        if (!OnBoardAbilityActive)
            return;
        if (e.Card != Owner)
            return;
        e.Allowed = false;
    }
}