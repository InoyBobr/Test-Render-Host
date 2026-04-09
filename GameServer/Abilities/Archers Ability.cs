[AbilityId("archers_ability")]
public class ArchersAbility : AbilityLogic
{
    public ArchersAbility(AbilityState state) : base(state) {}
    
    public override void OnGain()
    {
        Bus.Subscribe<CardCombatDamageRequestEvent>(OnAllyDamageRequest, SubscriberOwnerType.Card, Owner);
        Bus.Subscribe<CardNonCombatDamageRequestEvent>(OnAllyDamageRequest, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<CardCombatDamageRequestEvent>(OnAllyDamageRequest, Owner);
        Bus.Unsubscribe<CardNonCombatDamageRequestEvent>(OnAllyDamageRequest, Owner);
    }

    private void OnAllyDamageRequest(CardDamageRequestEvent e)
    {
        if (!OnBoardAbilityActive)
            return;
        if (e.Source.Owner != Owner.Owner || e.Source == Owner) return;
        if (State.IntValues.TryGetValue("damageBuff", out var damageBuff))
        {
            e.Damage += damageBuff;
        }
    }
}