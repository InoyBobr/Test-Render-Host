namespace GameServer.Abilities;
[AbilityId("cannot_attack")]
public class Cannot_attack : AbilityLogic
{
    public Cannot_attack(AbilityState state) : base(state) {}
    
    public override void OnGain()
    {
        Bus.Subscribe<CardCombatDamageRequestEvent>(DisableSelfAttack, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<CardCombatDamageRequestEvent>(DisableSelfAttack, Owner);
    }

    private void DisableSelfAttack(CardCombatDamageRequestEvent e)
    {
        if (!OnBoardAbilityActive)
            return;
        if (e.Card != Owner)
            return;
        e.Allowed = false;
    }
}