namespace GameServer.Abilities;
[AbilityId("deal_damage_on_being_damaged")]
public class DealDamageOnBeingDamaged: AbilityLogic
{
    public DealDamageOnBeingDamaged(AbilityState state) : base(state) {}
    
    public override void OnGain()
    {
        Bus.Subscribe<CardCombatDamagedEvent>(OnDamaged, SubscriberOwnerType.Card, Owner);
        Bus.Subscribe<CardNonCombatDamagedEvent>(OnDamaged, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<CardCombatDamagedEvent>(OnDamaged, Owner);
        Bus.Unsubscribe<CardNonCombatDamagedEvent>(OnDamaged, Owner);
    }

    private void OnDamaged(CardDamagedEvent e)
    {
        if (!OnBoardAbilityActive)
            return;
        if (e.Card != Owner)
            return;
        if (!State.IntValues.TryGetValue("damage", out var damage))
            return;
        var selector = new TargetSelector(TargetSide.Enemy, CardZone.Board, FaceConstraint.Any, StatConstraint.Any,
            TargetPick.Random);
        Bus.Publish(new RandomCardDamageRequestEvent(selector, damage, Owner));
    }
}